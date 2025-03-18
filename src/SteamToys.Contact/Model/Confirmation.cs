namespace SteamToys.Contact.Model;

/// <summary>
/// 账号操作 返回实体
/// </summary>
public class Confirmation
{
    /// <summary>
    /// The ID of this confirmation
    /// </summary>
    public ulong ID;

    /// <summary>
    /// 用于执行此确认的唯一密钥。
    /// </summary>
    public ulong Key;

    /// <summary>
    /// 为此贡献返回的数据类型HTML属性的值。
    /// </summary>
    public int IntType;

    /// <summary>
    /// 表示导致创建此确认的交易报价ID或市场交易ID。
    /// </summary>
    public ulong Creator;

    /// <summary>
    /// The type of this confirmation.
    /// </summary>
    public ConfirmationType ConfType;

    public Confirmation(ulong id, ulong key, int type, ulong creator)
    {
        ID = id;
        Key = key;
        IntType = type;
        Creator = creator;

        // 做一个简单的转换，因为我们不是100%确定所有可能的类型。
        switch (type)
        {
            case 1:
                ConfType = ConfirmationType.GenericConfirmation;
                break;
            case 2:
                ConfType = ConfirmationType.Trade;
                break;
            case 3:
                ConfType = ConfirmationType.MarketSellTransaction;
                break;
            default:
                ConfType = ConfirmationType.Unknown;
                break;
        }
    }
}
