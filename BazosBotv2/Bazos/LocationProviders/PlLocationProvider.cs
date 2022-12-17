using BazosBotv2.Interfaces;

namespace BazosBotv2.Bazos.LocationProviders;

internal sealed class PlLocationProvider : ILocationProvider
{
    private KeyValuePair<string, string> _inputValidationField;

    public string GetMyListingPath()
    {
        return "moje-ogloszenia.php";
    }

    public string GetAddListingPath()
    {
        return "dodaj-ogloszenie.php";
    }

    public KeyValuePair<string, string> GetInputValidationField()
    {
        return _inputValidationField;
    }

    public void SetInputValidationField(KeyValuePair<string, string> field)
    {
        _inputValidationField = field;
    }

    public string GetDeleteListingField()
    {
        return "Usuń";
    }

    public Uri GetUri()
    {
        return new("https://bazos.pl/");
    }

    public int GetZipCodeLength()
    {
        return 6;
    }
}