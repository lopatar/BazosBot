using BazosBotv2.Interfaces;

namespace BazosBotv2.Bazos.LocationProviders;

internal sealed class SkLocationProvider : ILocationProvider
{
    private KeyValuePair<string, string> _inputValidationField;

    public string GetMyListingPath()
    {
        return "moje-inzeraty.php";
    }

    public string GetAddListingPath()
    {
        return "pridat-inzerat.php";
    }

    public string GetDeleteListingField()
    {
        return "Zmazať";
    }

    public KeyValuePair<string, string> GetInputValidationField()
    {
        return _inputValidationField;
    }

    public void SetInputValidationField(KeyValuePair<string, string> field)
    {
        _inputValidationField = field;
    }

    public Uri GetUri()
    {
        return new("https://bazos.sk/");
    }
    
    public int GetZipCodeLength()
    {
        return 6;
    }
}