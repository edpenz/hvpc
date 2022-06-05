using System.Management;
using System.Net.NetworkInformation;

// Host switch MAC:
// * VirtualSwitchNames from Msvm_SummaryInformation by Name
// * PermanentAddress from Msvm_InternalEthernetPort by ElementName

// Guest port MAC:
// * PermanentAddress from Msvm_SyntheticEthernetPort by SystemName

public class Wmi
{
    static ManagementScope VIRT_SCOPE = new ManagementScope(@"root\virtualization\v2");

    public static ManagementObject GetObject(string query)
    {
        var searcher = new ManagementObjectSearcher(VIRT_SCOPE, new ObjectQuery(query));
        foreach (ManagementObject item in searcher.Get())
        {
            return item;
        }
        throw new ArgumentException("Empty query result set");
    }
}

namespace Msvm
{
    public enum EnabledState : UInt16
    {

        Unknown = 0,
        Other = 1,
        Enabled = 2,
        Disabled = 3,
        ShuttingDown = 4,
        NotApplicable = 5,
        EnabledButOffline = 6,
        InTest = 7,
        Deferred = 8,
        Quiesce = 9,
        Starting = 10,
    }

    public class ComputerSystem
    {

        ManagementObject system;

        public ComputerSystem(String systemName)
        {
            system = Wmi.GetObject($"select * from Msvm_ComputerSystem where ElementName = '{systemName}'");
        }

        public void Boot()
        {
            if (EnabledState != EnabledState.Enabled)
            {
                system.InvokeMethod("RequestStateChange", new Object?[] { (UInt16)2, null, null });
            }
        }

        public EnabledState EnabledState
        {
            get
            {
                system.Get();
                return (EnabledState)system["EnabledState"];
            }
        }

        public SyntheticEthernetPort Port
        {
            get
            {
                var port = Wmi.GetObject($"select * from Msvm_SyntheticEthernetPort where SystemName = '{system["Name"]}'");
                return new SyntheticEthernetPort(port);
            }
        }
    }

    public class SyntheticEthernetPort
    {
        ManagementObject port;

        public SyntheticEthernetPort(ManagementObject port)
        {
            this.port = port;
        }

        public PhysicalAddress Mac
        {
            get
            {
                return PhysicalAddress.Parse(port["PermanentAddress"].ToString());
            }
        }
    }
}
