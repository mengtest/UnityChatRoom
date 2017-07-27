using System;
using System.Text;
using Newtonsoft.Json;

namespace NetServer.Data
{
    /// <summary>
    /// 动态缓存类
    /// </summary>
    public class DynamicBuffer
    {
        //字节缓存区
        private byte[] buffer;
        //读取索引
        private int readIndex = 0;
        //写入索引
        private int writeIndex = 0;
        //读取索引标记
        private int markReadIndex = 0;
        //写入索引标记
        private int markWriteIndex = 0;
        //缓存区字节数组的长度
        private int capacity;

        public DynamicBuffer(int capacity)
        {
            buffer = new byte[capacity];
            this.capacity = capacity;
        }

        public DynamicBuffer(byte[] bytes)
        {
            buffer = bytes;
            this.capacity = bytes.Length;
            writeIndex = bytes.Length;
        }

        /// <summary>
        /// 翻转字节数组，如果本地字节序列为低字节序列，则进行翻转以转换为高字节序列
        /// </summary>
        private byte[] flip(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 确定内部字节缓存数组的大小
        /// </summary>
        private int FixSizeAndReset(int currentLength, int futureLength)
        {
            if (futureLength > currentLength)
            {
                int size = FixLength(currentLength) * 2;
                if (futureLength > size)
                {
                    size = FixLength(futureLength) * 2;
                }
                byte[] newBuffer = new byte[size];
                Array.Copy(buffer, 0, newBuffer, 0, currentLength);
                buffer = newBuffer;
                capacity = newBuffer.Length;
            }

            return futureLength;
        }

        /// <summary>
        /// 根据length长度，确定大于此leng的最近的2次方数，如length=7，则返回值为8
        /// </summary>
        private int FixLength(int length)
        {
            int n = 2;
            int num = 2;
            while (num < length)
            {
                num = 2 << n;
                n++;
            }
            return num;
        }

        /// <summary>
        /// 从读取索引位置开始读取length长度的字节数组
        /// </summary>
        private byte[] Read(int length)
        {
            byte[] bytes = new byte[length];
            Array.Copy(buffer, readIndex, bytes, 0, length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            readIndex += length;
            return bytes;
        }

        /// <summary>
        /// 读取一个字符串数据
        /// </summary>
        private string ReadString(int length)
        {
            byte[] strBytes = new byte[length];
            ReadBytes(strBytes, 0, length);
            return Encoding.UTF8.GetString(strBytes);
        }

        /// <summary>
        /// 读取一个对象数据
        /// </summary>
        private T ReadObject<T>(int length) where T : class
        {
            byte[] objectBytes = new byte[length];
            ReadBytes(objectBytes, 0, length);
            string objectJson = Encoding.UTF8.GetString(objectBytes);
            try
            {
                T _object = JsonConvert.DeserializeObject<T>(objectJson);
                return _object;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 将bytes字节数组从startIndex开始的length字节写入到此缓存区
        /// </summary>
        public DynamicBuffer WriteBytes(byte[] bytes, int startIndex, int length)
        {
            lock (this)
            {
                int offset = length - startIndex;
                if (offset <= 0) return this;
                int totalLength = offset + writeIndex;
                int bufferLength = buffer.Length;
                FixSizeAndReset(bufferLength, totalLength);
                for (int i = writeIndex, j = startIndex; i < totalLength; i++, j++)
                {
                    this.buffer[i] = bytes[j];
                }
                writeIndex = totalLength;
            }
            return this;
        }

        /// <summary>
        /// 将字节数组中从0到length的元素写入缓存区
        /// </summary>
        public DynamicBuffer WriteBytes(byte[] bytes, int length)
        {
            return WriteBytes(bytes, 0, length);
        }

        /// <summary>
        /// 将字节数组全部写入缓存区
        /// </summary>
        public DynamicBuffer WriteBytes(byte[] bytes)
        {
            return WriteBytes(bytes, bytes.Length);
        }

        /// <summary>
        /// 尝试读取byte类型数据
        /// </summary>
        public bool TryReadByte(ref byte value)
        {
            if (ReadableBytes() < sizeof(byte))
                return false;
            else
                value = ReadByte();
            return true;
        }

        /// <summary>
        /// 读一个字节
        /// </summary>
        public byte ReadByte()
        {
            byte _byte = buffer[readIndex];
            readIndex++;
            return _byte;
        }

        /// <summary>
        /// 尝试读取int16类型数据
        /// </summary>
        public bool TryReadShort(ref short value)
        {
            if (ReadableBytes() < sizeof(short))
                return false;
            else
                value = ReadShort();
            return true;
        }

        /// <summary>
        /// 读取一个int16数据
        /// </summary>
        public short ReadShort()
        {
            return BitConverter.ToInt16(Read(2), 0);
        }

        /// <summary>
        /// 尝试读取uint16类型数据
        /// </summary>
        public bool TryReadUShort(ref ushort value)
        {
            if (ReadableBytes() < sizeof(ushort))
                return false;
            else
                value = ReadUShort();
            return true;
        }

        /// <summary>
        /// 读取一个uint16数据
        /// </summary>
        public ushort ReadUShort()
        {
            return BitConverter.ToUInt16(Read(2), 0);
        }

        /// <summary>
        /// 尝试读取int32类型数据
        /// </summary>
        public bool TryReadInt(ref int value)
        {
            if (ReadableBytes() < sizeof(int))
                return false;
            else
                value = ReadInt();
            return true;
        }

        /// <summary>
        /// 读取一个int32数据
        /// </summary>
        public int ReadInt()
        {
            return BitConverter.ToInt32(Read(4), 0);
        }

        /// <summary>
        /// 尝试读取uint32类型数据
        /// </summary>
        public bool TryReadUInt(ref uint value)
        {
            if (ReadableBytes() < sizeof(uint))
                return false;
            else
                value = ReadUInt();
            return true;
        }

        /// <summary>
        /// 读取一个uint32数据
        /// </summary>
        public uint ReadUInt()
        {
            return BitConverter.ToUInt32(Read(4), 0);
        }

        /// <summary>
        /// 尝试读取int64类型数据
        /// </summary>
        public bool TryReadLong(ref long value)
        {
            if (ReadableBytes() < sizeof(long))
                return false;
            else
                value = ReadLong();
            return true;
        }

        /// <summary>
        /// 读取一个int64数据
        /// </summary>
        public long ReadLong()
        {
            return BitConverter.ToInt64(Read(8), 0);
        }

        /// <summary>
        /// 尝试读取uint64类型数据
        /// </summary>
        public bool TryReadULong(ref ulong value)
        {
            if (ReadableBytes() < sizeof(ulong))
                return false;
            else
                value = ReadULong();
            return true;
        }

        /// <summary>
        /// 读取一个uint64数据
        /// </summary>
        public ulong ReadULong()
        {
            return BitConverter.ToUInt64(Read(8), 0);
        }

        /// <summary>
        /// 尝试读取float类型数据
        /// </summary>
        public bool TryReadFloat(ref float value)
        {
            if (ReadableBytes() < sizeof(float))
                return false;
            else
                value = ReadFloat();
            return true;
        }

        /// <summary>
        /// 读取一个float数据
        /// </summary>
        public float ReadFloat()
        {
            return BitConverter.ToSingle(Read(4), 0);
        }

        /// <summary>
        /// 尝试读取double类型数据
        /// </summary>
        public bool TryReadDouble(ref double value)
        {
            if (ReadableBytes() < sizeof(double))
                return false;
            else
                value = ReadDouble();
            return true;
        }

        /// <summary>
        /// 读取一个double数据
        /// </summary>
        public double ReadDouble()
        {
            return BitConverter.ToDouble(Read(8), 0);
        }

        /// <summary>
        /// 尝试读取一个字符串数据
        /// </summary>
        public bool TryReadString(ref string value)
        {
            int strBytesLength = 0;
            if (TryReadInt(ref strBytesLength) && ReadableBytes() >= strBytesLength)
            {
                value = ReadString(strBytesLength);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 尝试读取一个对象数据
        /// </summary>
        public bool TryObject<T>(ref T value) where T : class
        {
            int objcetBytesLength = 0;
            if (TryReadInt(ref objcetBytesLength) && ReadableBytes() >= objcetBytesLength)
            {
                try
                {
                    value = ReadObject<T>(objcetBytesLength);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 从读取索引位置开始读取length长度的字节到disBytes目标字节数组中
        /// </summary>
        public void ReadBytes(byte[] disBytes, int disStart, int length)
        {
            int size = disStart + length;
            for (int i = disStart; i < length; i++)
            {
                disBytes[i] = this.ReadByte();
            }
        }

        public DynamicBuffer WriteObject(object value)
        {
            string json = JsonConvert.SerializeObject(value);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            WriteValue(jsonBytes.Length);
            return WriteBytes(jsonBytes);
        }

        /// <summary>
        /// 写入一个byte数据
        /// </summary>
        public DynamicBuffer WriteValue(byte value)
        {
            return WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个int16数据
        /// </summary>
        public DynamicBuffer WriteValue(short value)
        {
            return WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个uint16数据
        /// </summary>
        public DynamicBuffer WriteValue(ushort value)
        {
            return WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个int32数据
        /// </summary>
        public DynamicBuffer WriteValue(int value)
        {
            return WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个uint32数据
        /// </summary>
        public DynamicBuffer WriteValue(uint value)
        {
            return WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个int64数据
        /// </summary>
        public DynamicBuffer WriteValue(long value)
        {
            return WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个uint64数据
        /// </summary>
        public DynamicBuffer WriteValue(ulong value)
        {
            return WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个float数据
        /// </summary>
        public DynamicBuffer WriteValue(float value)
        {
            return WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个double数据
        /// </summary>
        public DynamicBuffer WriteValue(double value)
        {
            return WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入字符串
        /// </summary>
        public DynamicBuffer WriteValue(string value)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(value);
            WriteValue(strBytes.Length);
            return WriteBytes(strBytes);
        }

        /// <summary>
        /// 清除已读字节并重建缓存区
        /// </summary>
        public void DiscardReadBytes()
        {
            if (readIndex <= 0) return;
            int length = buffer.Length - readIndex;
            byte[] newBuffer = new byte[length];
            Array.Copy(buffer, readIndex, newBuffer, 0, length);
            buffer = newBuffer;
            writeIndex -= readIndex;
            markReadIndex -= readIndex;
            if (markReadIndex < 0)
            {
                markReadIndex = readIndex;
            }
            markWriteIndex -= writeIndex;
            if (markWriteIndex < 0)
            {
                markWriteIndex = writeIndex;
            }
            readIndex = 0;
        }

        /// <summary>
        /// 清空此对象
        /// </summary>
        public void Clear()
        {
            buffer = new byte[buffer.Length];
            readIndex = 0;
            writeIndex = 0;
            markReadIndex = 0;
            markWriteIndex = 0;
        }

        /// <summary>
        /// 设置开始读取的索引
        /// </summary>
        public void SetReaderIndex(int index)
        {
            if (index < 0) return;
            readIndex = index;
        }

        /// <summary>
        /// 标记读取的索引位置
        /// </summary>
        public int MarkReaderIndex()
        {
            markReadIndex = readIndex;
            return markReadIndex;
        }

        /// <summary>
        /// 标记写入的索引位置
        /// </summary>
        public void MarkWriterIndex()
        {
            markWriteIndex = writeIndex;
        }

        /// <summary>
        /// 将读取的索引位置重置为标记的读取索引位置
        /// </summary>
        public void ResetReaderIndex()
        {
            readIndex = markReadIndex;
        }

        /// <summary>
        /// 将写入的索引位置重置为标记的写入索引位置
        /// </summary>
        public void ResetWriterIndex()
        {
            writeIndex = markWriteIndex;
        }

        /// <summary>
        /// 可读的有效字节数
        /// </summary>
        public int ReadableBytes()
        {
            return writeIndex - readIndex;
        }

        /// <summary>
        /// 获取可读的字节数组
        /// </summary>
        public byte[] ToArray()
        {
            byte[] bytes = new byte[writeIndex];
            Array.Copy(buffer, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// 获取缓存区大小
        /// </summary>
        public int GetCapacity()
        {
            return this.capacity;
        }
    }
}
