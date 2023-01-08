using BazosBotv2.Configuration;
using BazosBotv2.Interfaces;
using BazosBotv2.Utilities;
using Newtonsoft.Json;

namespace BazosBotv2.Bazos;

internal readonly struct StoredListing
{
    private readonly uint _categoryId;
    private readonly string _description;
    public readonly Uri Link;
    private readonly Uri _sectionLink;
    private readonly string _postalCode;
    private readonly uint _price;
    public readonly string Name;
    private readonly string _bazosLocation;
    private readonly uint _imagesCount;
    
    public StoredListing(uint categoryId, string description, Uri link, Uri sectionLink, string postalCode,
        uint price, string name, string bazosLocation, uint imagesCount)
    {
        _categoryId = categoryId;
        _description = description;
        Link = link;
        _sectionLink = sectionLink;
        _postalCode = postalCode;
        _price = price;
        Name = name;
        _bazosLocation = bazosLocation;
        _imagesCount = imagesCount;
    }
    
    private string GetListingPath()
    {
        return $"{ConfigLoader.ListingDirectory}{_bazosLocation}/{Name.Replace(" ", "-").Replace("|", "")}/";
    }

    public void RestoreListing(ILocationProvider locationProvider, Config config)
    {
        var bazosUploadedImages = UploadImagesToBazos(locationProvider, config);
        var validationField = locationProvider.GetInputValidationField();

        var postContentPairs = new List<KeyValuePair<string, string>>
        {
            new("category", _categoryId.ToString()),
            new("nadpis", Name),
            new("popis", _description),
            new("cena", _price.ToString()),
            new("cenavyber", "1"),
            new("lokalita", _postalCode),
            new("jmeno", config.UserName),
            new("telefoni", config.UserPhoneNum.ToString()),
            new("maili", config.UserEmail),
            new("heslobazar", config.UserPassword),
            new(validationField.Key, validationField.Value),
            new("Submit", "submitButton")
        };

        postContentPairs.AddRange(bazosUploadedImages.Select(uploadedImage =>
            new KeyValuePair<string, string>("files[]", uploadedImage)));
        using var httpContent = new FormUrlEncodedContent(postContentPairs);
        using var httpClient = new BazosHttp(locationProvider, config);
        httpClient.Post(new Uri(_sectionLink + "insert.php"), httpContent);
        Utils.Print($"Re-created listing: {Name}!!", location: config.BazosLocation);
    }

    private List<string> UploadImagesToBazos(ILocationProvider locationProvider, Config config)
    {
        var imagesDirectory = GetListingPath();
        var bazosImgNames = new List<string>();

        for (var i = 0; i < _imagesCount; i++)
        {
            var imgPath = $"{imagesDirectory}{i}.jpg";
            var imgBytes = File.ReadAllBytes(imgPath);
            var bazosImgName = Utils.UploadImage(imgBytes, $"{i}.jpg", locationProvider, config, _sectionLink);

            Utils.Print($"Uploaded image: {imgPath} for listing: {Name} as: {bazosImgName}",
                location: config.BazosLocation);
            bazosImgNames.Add(bazosImgName);
        }

        return bazosImgNames;
    }
    
    public void Save()
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        var path = $"{GetListingPath()}Data.json";
        
        Utilities.Utils.Print($"Saving listing: {Name} data to {path}", location: _bazosLocation);
        File.WriteAllText(path, json);
    }
}