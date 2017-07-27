/// <summary>
/// 处理基类
/// </summary>
public abstract class ActionBase
{
    /// <summary>
    /// 处理类型
    /// </summary>
    public abstract int ActionType { get; }

    /// <summary>
    /// 数据包
    /// </summary>
    public DataPackage Packet { get; set; }

    /// <summary>
    /// 发送过程
    /// </summary>
    public abstract bool SendProcess(ActionParameter parameter);

    /// <summary>
    /// 接收过程
    /// </summary>
    public abstract bool ReceiveProcess(ActionParameter parameter);

    /// <summary>
    /// 清理
    /// </summary>
    public virtual void Clean()
    {
        Packet = null;
    }
}

