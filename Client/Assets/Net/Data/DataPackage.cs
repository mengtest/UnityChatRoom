/// <summary>
/// 数据包
/// </summary>
public class DataPackage
{
    /// <summary>
    /// 包数据
    /// </summary>
    public DynamicBuffer Data { get; set; }

    /// <summary>
    /// 包类型
    /// </summary>
    public int PacketType { get; set; }

    public DataPackage(byte[] bytes, int type) : this(new DynamicBuffer(bytes), type)
    {

    }

    public DataPackage(DynamicBuffer buffer, int type)
    {
        PacketType = type;
        Data = buffer;
    }

    /// <summary>
    /// 字节数组转化为数据包
    /// </summary>
    public static DataPackage Parse(byte[] bytes)
    {
        DynamicBuffer buffer = new DynamicBuffer(bytes);

        int packetType = 0;
        if (buffer.TryReadInt(ref packetType))
        {
            buffer.DiscardReadBytes();
            return new DataPackage(buffer, packetType);
        }
        else
        {
            return null;
        }
    }
}

