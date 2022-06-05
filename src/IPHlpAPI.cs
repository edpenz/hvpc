using System.Net;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

public class IPHlpAPI
{
    [StructLayout(LayoutKind.Sequential)]
    struct _PHYSADDR
    {
        public Byte b0;
        public Byte b1;
        public Byte b2;
        public Byte b3;
        public Byte b4;
        public Byte b5;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct _ADDR
    {
        public Byte b0;
        public Byte b1;
        public Byte b2;
        public Byte b3;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MIB_IPNETROW
    {
        public Int32 dwIndex;
        public Int32 dwPhysAddrLen;
        public _PHYSADDR bPhysAddr;
        Int16 _pad0;
        public _ADDR dwAddr;
        public Int32 dwType;
    }

    // TODO: Support mashelling dynamic table sizes.
    const int MAX_ENTRIES = 1024;

    [StructLayout(LayoutKind.Sequential)]
    struct MIB_IPNETTABLE
    {
        public Int32 dwNumEntries;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ENTRIES)]
        public MIB_IPNETROW[] table;
    }

    [DllImport("Iphlpapi.dll")]
    static extern ulong GetIpNetTable(ref MIB_IPNETTABLE IpNetTable, ref UInt32 SizePointer, Boolean Order);

    static MIB_IPNETROW[] _GetIpNetTable(Boolean Order)
    {
        unsafe
        {
            MIB_IPNETTABLE nativeTable = new MIB_IPNETTABLE();
            UInt32 nativeTableSize = (UInt32)(sizeof(Int32) + sizeof(MIB_IPNETROW) * MAX_ENTRIES);

            // TODO: Check result
            var result = GetIpNetTable(ref nativeTable, ref nativeTableSize, false);

            return nativeTable.table[..nativeTable.dwNumEntries];
        }
    }

    public readonly struct ArpEntry
    {
        public readonly int AdapterIndex;
        public readonly IPAddress Ip;
        public readonly PhysicalAddress Mac;

        public ArpEntry(int adapterIndex, IPAddress ip, PhysicalAddress mac)
        {
            AdapterIndex = adapterIndex;
            Ip = ip;
            Mac = mac;
        }
    }

    public static IEnumerable<ArpEntry> GetArpTable()
    {
        return _GetIpNetTable(false).Select(row =>
        {
            var mac = new PhysicalAddress(new byte[] {
                    row.bPhysAddr.b0,
                    row.bPhysAddr.b1,
                    row.bPhysAddr.b2,
                    row.bPhysAddr.b3,
                    row.bPhysAddr.b4,
                    row.bPhysAddr.b5,
                });

            var ip = new IPAddress(new byte[] {
                    row.dwAddr.b0,
                    row.dwAddr.b1,
                    row.dwAddr.b2,
                    row.dwAddr.b3,
                });

            return new ArpEntry(row.dwIndex, ip, mac);
        });
    }
}
