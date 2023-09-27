using System;
using System.Runtime.InteropServices;

namespace IoT_Server.Modules.Socket
{
    class AddonMemory
    {
        public static long alloc_len { private set; get; }


        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void Copy(IntPtr dest, IntPtr src, uint count);
        public static void Copy(IntPtr dest, IntPtr src, int count) { Copy(dest, src, (uint)count); }
        public static void Copy(Array src, Array dst, int byteLen)
        {
            var addrSrc = Marshal.UnsafeAddrOfPinnedArrayElement(src, 0);
            var addrDest = Marshal.UnsafeAddrOfPinnedArrayElement(dst, 0);
            Copy(addrDest, addrSrc, byteLen);
        }
        public static void Copy(Array src, int offsetSrc, Array dst, int offsetDst, int byteLen)
        {
            var addrSrc = Marshal.UnsafeAddrOfPinnedArrayElement(src, offsetSrc);
            var addrDest = Marshal.UnsafeAddrOfPinnedArrayElement(dst, offsetDst);
            Copy(addrDest, addrSrc, byteLen);
        }
        /// <summary>
        /// Copy Array to PTR
        /// </summary>
        /// <param name="src"></param>
        /// <param name="offsetSrc"></param>
        /// <param name="dst"></param>
        /// <param name="byteLen"></param>
        public static void Copy(Array src, int offsetSrc, IntPtr dst, int byteLen)
        {
            var addrSrc = Marshal.UnsafeAddrOfPinnedArrayElement(src, offsetSrc);

            Copy(dst, addrSrc, byteLen);
        }
        /// <summary>
        /// Copy Array from ptr
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="offsetDst"></param>
        /// <param name="byteLen"></param>
        public static void Copy(IntPtr src, Array dst, int offsetDst, int byteLen)
        {
            var addrDest = Marshal.UnsafeAddrOfPinnedArrayElement(dst, offsetDst);
            Copy(addrDest, src, byteLen);
        }
        public static byte[] ToBytes(Array src, int byteLen, int offsetSrc = 0)
        {
            byte[] dst = new byte[byteLen];
            var addrSrc = Marshal.UnsafeAddrOfPinnedArrayElement(src, offsetSrc);
            var addrDest = Marshal.UnsafeAddrOfPinnedArrayElement(dst, 0);
            Copy(addrDest, addrSrc, byteLen);
            return dst;
        }
        public static T[] FromBytes<T>(Array src, int byteLen, int resultingLen, int offsetSrc = 0)
        {
            T[] dst = new T[resultingLen];
            var addrSrc = Marshal.UnsafeAddrOfPinnedArrayElement(src, offsetSrc);
            var addrDest = Marshal.UnsafeAddrOfPinnedArrayElement(dst, 0);
            Copy(addrDest, addrSrc, byteLen);
            return dst;
        }
        public static bool IsSameSize(Type a, Type b)
        {
            return Marshal.SizeOf(a) == Marshal.SizeOf(b);
        }

        public static byte[] ToBytes(object structure)
        {
            int len = Marshal.SizeOf(structure);
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(structure, ptr, false);
            byte[] ray = new byte[len];
            var addrDest = Marshal.UnsafeAddrOfPinnedArrayElement(ray, 0);
            Copy(addrDest, ptr, len);
            Marshal.FreeHGlobal(ptr);
            return ray;
        }
        public static object FromBytes(Type t, byte[] ray)
        {
            int len = ray.Length;
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Copy(ray, 0, ptr, len);
            object result = Marshal.PtrToStructure(ptr, t);
            Marshal.FreeHGlobal(ptr);
            return result;
        }

        public static IntPtr Alloc(int len)
        {
            IntPtr ptr = Marshal.AllocHGlobal(len);
            alloc_len += len;
            return ptr;
        }
        public static void Free(IntPtr ptr, long len)
        {
            Marshal.FreeHGlobal(ptr);
            alloc_len -= len;
        }
    }
}
