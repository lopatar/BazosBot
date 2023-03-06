using BazosBotv2.Interfaces;

namespace BazosBotv2.Bazos.LocationProviders;

internal sealed class AtLocationProvider : ILocationProvider
{
    private KeyValuePair<string, string> _inputValidationField;

    public string GetMyListingPath()
    {
        return "meine-anzeigen.php";
    }

    public string GetAddListingPath()
    {
        return "anzeige-aufgeben.php";
    }

    public string GetDeleteListingField()
    {
        return "Löschen";
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
        return new Uri("https://bazos.at/");
    }

    public int GetZipCodeLength()
    {
        return 4;
    }
}