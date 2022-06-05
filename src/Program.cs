using System.Net;
using System.Net.Sockets;


public class Program
{
    static void PrintStatus(String stage, String? details, Throbber throbber)
    {
        Console.Error.Write(
            Vt.CURSOR_LEFT +
            stage +
            throbber.Text +
            (details != null ? $" ({details})" : "") +
            Vt.CLEAR_RIGHT
        );
    }

    static void ClearStatus()
    {
        Console.Error.Write(Vt.CURSOR_LEFT + Vt.CLEAR_RIGHT);
    }

    public static void Main(string[] args)
    {
        // Check arguments
        String vmName;
        UInt16 sshPort;
        try
        {
            vmName = args[0];
            sshPort = UInt16.Parse(args[1]);
        }
        catch (Exception)
        {
            Console.Error.WriteLine("Usage: hvpc.exe <VM name> <SSH port>");
            Environment.Exit(1);
            return;
        }

        var throbber = new Throbber();

        // Start the VM
        var vm = new Msvm.ComputerSystem(vmName);
        vm.Boot();

        while (vm.EnabledState != Msvm.EnabledState.Enabled)
        {
            PrintStatus("HYPER-V", vm.EnabledState.ToString(), throbber);
            throbber.Sleep();
        }

        // Wait until ARP table contains an entry for the VM's ethernet adapter
        var vmPort = vm.Port;
        IPAddress? vmGuestIP = null;
        while (vmGuestIP == null)
        {
            foreach (var entry in IPHlpAPI.GetArpTable())
            {
                if (entry.Mac.Equals(vmPort.Mac))
                {
                    vmGuestIP = entry.Ip;
                    break;
                }
            }

            if (vmGuestIP == null)
            {
                PrintStatus("ARP", null, throbber);
                throbber.Sleep();
            }
        }

        // Open the TCP connection
        TcpClient sshClient = new TcpClient();
        String? sshStatus = null;
        while (!sshClient.Connected)
        {
            var connectResult = sshClient.BeginConnect(vmGuestIP, sshPort, null, null);
            while (!connectResult.IsCompleted)
            {
                PrintStatus("TCP", sshStatus, throbber);
                throbber.Sleep();
            }

            try
            {
                sshClient.EndConnect(connectResult);
            }
            catch (Exception e)
            {
                sshStatus = e.Message;
            }
        }

        // Forward data bidirectionally.
        ClearStatus();

        var sshStream = sshClient.GetStream();
        var stdin = Console.OpenStandardInput();
        var stdout = Console.OpenStandardOutput();

        Cat.DualCat(stdin, sshStream, sshStream, stdout);
    }
}
