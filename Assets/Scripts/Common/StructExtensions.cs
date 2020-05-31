using System.Runtime.InteropServices;

namespace Common
{
    public static class StructExtensions
    {
        public static byte[] ToBytes<T>(this T @struct)
        where T : struct
        {
            GCHandle handle;
            var bytes = new byte[Marshal.SizeOf<T>()];

            try
            {
                handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                Marshal.StructureToPtr(@struct, handle.AddrOfPinnedObject(), false);
            }
            finally { if (handle.IsAllocated)
            {
                handle.Free();
            }}
            
            return bytes;
        }
        
        public static T ToStruct<T>(this byte[] bytes)
        where T : struct
        {
            GCHandle handle;
            T @struct;
            
            try
            {
                handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                @struct = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally { if (handle.IsAllocated)
            {
                handle.Free();
            }}
            
            return @struct;
        }
    }
}