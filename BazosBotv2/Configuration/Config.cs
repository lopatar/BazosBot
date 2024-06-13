namespace BazosBotv2.Configuration;

internal readonly struct Config
{
    public readonly string BazosLocation;
    public readonly bool Enabled;
    public readonly string UserName;
    public readonly string UserPassword;
    public readonly string UserEmail;
    public readonly ulong UserPhoneNum;
    public readonly uint UserCookieBId;
    public readonly string UserCookieBKod;
    public readonly uint ListingDaysUntilRenewal;
    public readonly bool SkipTopListings;
    public readonly bool EnableRestorer;

    public Config(string bazosLocation, bool enabled, string userName, string userPassword, string userEmail,
        ulong userPhoneNum,
        uint userCookieBId, string userCookieBKod, uint listingDaysUntilRenewal, bool skipTopListings,
        bool enableRestorer)
    {
        BazosLocation = bazosLocation;
        Enabled = enabled;
        UserName = userName;
        UserPassword = userPassword;
        UserEmail = userEmail;
        UserPhoneNum = userPhoneNum;
        UserCookieBId = userCookieBId;
        UserCookieBKod = userCookieBKod;
        ListingDaysUntilRenewal = listingDaysUntilRenewal;
        SkipTopListings = skipTopListings;
        EnableRestorer = enableRestorer;
    }
}