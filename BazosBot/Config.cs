using System;

namespace BazosBot;

[Serializable]
internal sealed class Config
{
    public string Jmeno { get; set; }
    public uint TelCislo { get; set; }
    public string Heslo { get; set; }
    public uint BIdCookie { get; set; }
    public string BKodCookie { get; set; }
    public uint StariDoSmazani { get; set; }

    public void PrintValues()
    {
        Utils.Print("Configuration values:");
        Utils.Print($"Jméno: {Jmeno}");
        Utils.Print($"Heslo: {Heslo}");
        Utils.Print($"Tel. cislo: {TelCislo}");
        Utils.Print($"Bid cookie: {BIdCookie}");
        Utils.Print($"BKod Cookie: {BKodCookie}");
        Utils.Print($"Stáří inzerátu do znovupřidání: {StariDoSmazani} dnů");
    }
}