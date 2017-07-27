using System;

namespace NetServer
{
    public static class NetUtility
    {
        /// <summary>
        /// 解包
        /// </summary>
        public static DataPackage UnPack(this byte[] bytes)
        {
            DynamicBuffer dynamicBuffer = new DynamicBuffer(bytes);
            return dynamicBuffer.UnPack();
        }

        public static DataPackage UnPack(this DynamicBuffer dynamicBuffer)
        {
            dynamicBuffer.MarkReaderIndex();
            int messageLength = 0;
            if (dynamicBuffer.TryReadInt(ref messageLength))
            {
                int readByteLength = dynamicBuffer.ReadableBytes();
                if (messageLength > readByteLength)
                {
                    dynamicBuffer.ResetReaderIndex();
                    return null;
                }
                else
                {
                    byte[] messageBytes = new byte[messageLength];
                    dynamicBuffer.ReadBytes(messageBytes, 0, messageLength);
                    dynamicBuffer.DiscardReadBytes();
                    return DataPackage.Parse(messageBytes);
                }
            }
            return null;
        }

        /// <summary>
        /// 封包
        /// </summary>
        public static byte[] Pack(this byte[] bytes, int packetType)
        {
            DataPackage packet = new DataPackage(bytes, packetType);
            return packet.Pack();
        }

        public static byte[] Pack(this DataPackage dataPackage)
        {
            byte[] packetTypeBytes = BitConverter.GetBytes(dataPackage.PacketType);
            byte[] packetBytes = dataPackage.Data.ToArray();
            byte[] packetLengthBytes = BitConverter.GetBytes(packetBytes.Length + packetTypeBytes.Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(packetTypeBytes);
                Array.Reverse(packetLengthBytes);
            }

            byte[] bytes = new byte[packetLengthBytes.Length + packetTypeBytes.Length + packetBytes.Length];
            Array.Copy(packetLengthBytes, 0, bytes, 0, packetLengthBytes.Length);
            Array.Copy(packetTypeBytes, 0, bytes, packetLengthBytes.Length, packetTypeBytes.Length);
            Array.Copy(packetBytes, 0, bytes, packetLengthBytes.Length + packetTypeBytes.Length, packetBytes.Length);

            return bytes;
        }
    }
}
