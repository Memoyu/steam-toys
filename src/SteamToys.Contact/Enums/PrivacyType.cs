namespace SteamToys.Contact.Enums;

public enum PrivacyType
{
    [Description("默认")]
    None = 0,

    [Description("私密")]
    Private = 1,

    [Description("仅限好友")]
    FriendsOnly = 2,

    [Description("公开")]
    Public = 3
}
