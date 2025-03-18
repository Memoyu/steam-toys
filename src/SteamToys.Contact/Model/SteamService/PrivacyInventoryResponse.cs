namespace SteamToys.Contact.Model.SteamService
{
    public class PrivacyInventoryResponse
    {
        public int Success { get; set; }

        public PrivacyReponse Privacy { get; set; }
    }

    public class PrivacyReponse
    { 
        public PrivacySettings PrivacySettings { get; set; }

        public int ECommentPermission { get; set; }
    }

    public class PrivacySettings
    { 
        public int PrivacyProfile { get; set; }
        public int PrivacyInventory { get; set; }
        public int PrivacyInventoryGifts { get; set; }
        public int PrivacyOwnedGames { get; set; }
        public int PrivacyPlaytime { get; set; }
        public int PrivacyFriendsList { get; set; }
    }
}
