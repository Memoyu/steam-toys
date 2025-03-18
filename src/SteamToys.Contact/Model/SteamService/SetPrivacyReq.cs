namespace SteamToys.Contact.Model.SteamService;

public class SetPrivacyReq
{
    public string Sessionid { get; set; }

    public PrivacyItem Privacy { get; set; }

    public int ECommentPermission { get; set; } = 0;
}

public class PrivacyItem
{
    /// <summary>
    /// 基本信息
    /// </summary>
    [JsonProperty("PrivacyProfile")]
    public PrivacyType PrivacyProfile { get; set; }

    /// <summary>
    /// 隐私库存
    /// </summary>
    [JsonProperty("PrivacyInventory")]
    public PrivacyType PrivacyInventory { get; set; }

    /// <summary>
    /// 个人资料
    /// </summary>
    [JsonProperty("PrivacyInventoryGifts")]
    public PrivacyType PrivacyInventoryGifts { get; set; }

    /// <summary>
    /// 个人资料页面留言
    /// </summary>
    [JsonProperty("PrivacyOwnedGames")]
    public PrivacyType PrivacyOwnedGames { get; set; }

    /// <summary>
    /// 游戏详情
    /// </summary>
    [JsonProperty("PrivacyPlaytime")]
    public PrivacyType PrivacyPlaytime { get; set; }

    /// <summary>
    /// 好友列表
    /// </summary>
    [JsonProperty("PrivacyFriendsList")]
    public PrivacyType PrivacyFriendsList { get; set; }
}
