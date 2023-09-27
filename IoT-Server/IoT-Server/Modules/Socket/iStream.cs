using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace IoT_Server.Modules.Socket
{
    public static class BitArrayExt
    {
        public static byte[] ToByteArray(this BitArray r)
        {
            byte[] bytes = new byte[(int)Math.Ceiling((float)r.Length / 8)];
            r.CopyTo(bytes, 0);
            return bytes;
        }
    }

    public class iStream: IDisposable
    {
        public int bytesSent = 0;
        public int bytesReceived = 0;

        public Stream baseStream;
        public iStream(Stream s)
        {
            baseStream = s;
        }
        #region read
        byte[] buffer = new byte[16];
        public bool ReadBool()
        {
            baseStream.Read(buffer, 0, 1);
            return BitConverter.ToBoolean(buffer, 0);
        }
        public bool[] ReadBoolArray(int count)
        {
            int bytecount = (int)Math.Ceiling((float)count / 8);
            byte[] bytes = new byte[bytecount];
            bool[] bray = new bool[count];
            baseStream.Read(bytes, 0, bytecount);
            var br = new BitArray(bytes);
            for (int i = 0; i < count; i++)
            {
                bray[i] = br.Get(i);
            }
            return bray;
        }
        public byte ReadByte()
        {
            baseStream.Read(buffer, 0, 1); bytesReceived += 1;
            return buffer[0];
        }
        public byte[] ReadBytes()
        {
            int len = ReadInt();
            byte[] buf = new byte[len];
            baseStream.Read(buf, 0, len);
            return buf;
        }
        public byte[] ReadRawBytes(int len)
        {
            byte[] buf = new byte[len];
            baseStream.Read(buf, 0, len);
            return buf;
        }
        public ushort ReadUshort()
        {
            baseStream.Read(buffer, 0, 2); bytesReceived += 2;
            return BitConverter.ToUInt16(buffer, 0);
        }
        public short ReadShort()
        {
            baseStream.Read(buffer, 0, 2); bytesReceived += 2;
            return BitConverter.ToInt16(buffer, 0);
        }
        public uint ReadUint()
        {
            baseStream.Read(buffer, 0, 4); bytesReceived += 4;
            return BitConverter.ToUInt32(buffer, 0);
        }
        public int ReadInt()
        {
            baseStream.Read(buffer, 0, 4); bytesReceived += 4;
            return BitConverter.ToInt32(buffer, 0);
        }
        public ulong ReadUlong()
        {
            baseStream.Read(buffer, 0, 8); bytesReceived += 8;
            return BitConverter.ToUInt64(buffer, 0);
        }
        public long ReadLong()
        {
            baseStream.Read(buffer, 0, 8); bytesReceived += 8;
            return BitConverter.ToInt64(buffer, 0);
        }
        public float ReadFloat()
        {
            baseStream.Read(buffer, 0, 4); bytesReceived += 4;
            return BitConverter.ToSingle(buffer, 0);
        }
        public double ReadDouble()
        {
            baseStream.Read(buffer, 0, 8); bytesReceived += 8;
            return BitConverter.ToDouble(buffer, 0);
        }
        public string ReadString()
        {
            return ReadString(Encoding.UTF8);
        }
        public string ReadString(Encoding e)
        {
            int length = (int)ReadShort();
            byte[] bytes = new byte[length];
            baseStream.Read(bytes, 0, length); bytesReceived += 2 + length;
            return e.GetString(bytes);
        }
        public string ReadNullTermString(Encoding e)
        {
            int b = 255;
            List<byte> data = new List<byte>();
            while (b > 0)
            {
                b = baseStream.ReadByte();
                if (b > 0) data.Add((byte)b);
            }
            bytesReceived += 1 + data.Count;
            return e.GetString(data.ToArray());
        }
        public string[] ReadNullTermString(Encoding e, int count)
        {
            string[] r = new string[count];
            for (int i = 0; i < count; i++)
            {
                r[i] = ReadNullTermString(e);
            }
            return r;
        }
        public Guid ReadGuid()
        {
            baseStream.Read(buffer, 0, 16);
            return new Guid(buffer);
        }

        public T[] ReadArray<T>(int count) where T : struct
        {
            int blen = count * Marshal.SizeOf(typeof(T));
            T[] r = new T[count];
            var b = ReadRawBytes(blen);
            AddonMemory.Copy(b, r, blen);
            return r;
        }
        public T ReadStruct<T>() where T : struct
        {
            var t = typeof(T);
            int blen = Marshal.SizeOf(t);
            var br = ReadRawBytes(blen);
            return (T)AddonMemory.FromBytes(t, br);
        }
        #endregion
        #region write
        public void Write(bool b)
        {
            baseStream.Write(BitConverter.GetBytes(b), 0, 1);
        }
        public void WriteRaw(bool[] b)
        {
            var r = new BitArray(b).ToByteArray();
            baseStream.Write(r, 0, r.Length);
        }
        public void Write(byte b)
        {
            baseStream.WriteByte(b); bytesSent += 1;
        }
        public void Write(byte[] b)
        {
            Write(b.Length);
            baseStream.Write(b, 0, b.Length);
        }
        public void WriteRaw(byte[] b)
        {
            baseStream.Write(b, 0, b.Length);
        }
        public void Write(ushort s)
        {
            baseStream.Write(BitConverter.GetBytes(s), 0, 2); bytesSent += 2;
        }
        public void Write(short s)
        {
            baseStream.Write(BitConverter.GetBytes(s), 0, 2); bytesSent += 2;
        }
        public void Write(uint s)
        {
            baseStream.Write(BitConverter.GetBytes(s), 0, 4); bytesSent += 4;
        }
        public void Write(int s)
        {
            baseStream.Write(BitConverter.GetBytes(s), 0, 4); bytesSent += 4;
        }
        public void Write(ulong s)
        {
            baseStream.Write(BitConverter.GetBytes(s), 0, 8); bytesSent += 8;
        }
        public void Write(long s)
        {
            baseStream.Write(BitConverter.GetBytes(s), 0, 8); bytesSent += 8;
        }
        public void Write(float s)
        {
            baseStream.Write(BitConverter.GetBytes(s), 0, 4); bytesSent += 4;
        }
        public void Write(double s)
        {
            baseStream.Write(BitConverter.GetBytes(s), 0, 8); bytesSent += 8;
        }
        public void Write(string s)
        {
            Write(s, Encoding.UTF8);
        }
        public void Write(string s, Encoding e)
        {
            byte[] bytes = e.GetBytes(s);
            Write((short)bytes.Length);
            baseStream.Write(bytes, 0, bytes.Length); bytesSent += 2 + bytes.Length;
        }
        public void WriteNullTermString(string s, Encoding e)
        {
            byte[] bytes = e.GetBytes(s);
            short len = (short)s.Length;
            baseStream.Write(bytes, 0, bytes.Length); bytesSent += 1 + bytes.Length;
            baseStream.WriteByte(0);
        }
        public void Write(Guid s)
        {
            baseStream.Write(s.ToByteArray(), 0, 16);
        }

        public void WriteArray<T>(T[] r) where T : struct
        {
            int blen = r.Length * Marshal.SizeOf(typeof(T));
            byte[] bl = new byte[blen];
            Buffer.BlockCopy(r, 0, bl, 0, blen);
            WriteRaw(bl);
        }
        public void WriteArrayMemcpy<T>(T[] r) where T : struct
        {
            int blen = r.Length * Marshal.SizeOf(typeof(T));
            byte[] bl = AddonMemory.ToBytes(r, blen);
            WriteRaw(bl);
        }
        public void WriteStruct<T>(T s) where T : struct
        {
            WriteRaw(AddonMemory.ToBytes(s));
        }
        #endregion



        public void Dispose()
        {
            baseStream.Dispose();
        }
    }
}
