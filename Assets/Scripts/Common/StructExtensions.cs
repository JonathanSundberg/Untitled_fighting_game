using System.Runtime.InteropServices;

namespace Common
{
    public static class StructExtensions
    {
        public static byte[] ToBytes<T>(this T @struct)
        where T : struct
        {
            var handle = default(GCHandle);
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
            var handle = default(GCHandle);
            var @struct = default(T);
            
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