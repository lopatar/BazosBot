namespace BazosBotv2.Interfaces;

internal interface ILocationProvider
{
    string GetMyListingPath();
    string GetAddListingPath();
    string GetDeleteListingField();
    KeyValuePair<string, string> GetInputValidationField();
    void SetInputValidationField(KeyValuePair<string, string> field);
    Uri GetUri();
}