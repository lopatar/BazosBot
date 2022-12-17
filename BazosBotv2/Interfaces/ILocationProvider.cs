namespace BazosBotv2.Interfaces;

internal interface ILocationProvider
{
    string GetMyListingPath();
    string GetAddListingPath();
    string GetDeleteListingField();
    /// <summary>
    /// This field is some sort of "spam" prevention, it's a constant value hidden POST field for each Location.
    /// </summary>
    KeyValuePair<string, string> GetInputValidationField();
    /// <summary>
    /// This field is some sort of "spam" prevention, it's a constant value hidden POST field for each Location.
    /// </summary>
    void SetInputValidationField(KeyValuePair<string, string> field);
    Uri GetUri();
    /// <summary>
    /// This method rather returns the amount of characters to grab from the back of the listing location info, including spaces and other characters, that are later removed to form the ZIP code
    /// </summary>
    int GetZipCodeLength();
}