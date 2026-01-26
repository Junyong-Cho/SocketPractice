using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StudyProject;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct PacketHeader
{
    public ushort Size;
    public ushort Id;

    static public int PacketSize => Unsafe.SizeOf<PacketHeader>();
}
