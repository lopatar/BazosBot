using BazosBotv2.Configuration;
using BazosBotv2.Interfaces;
using BazosBotv2.Utilities;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace BazosBotv2.Bazos;

internal readonly struct StoredListing
{
    public readonly uint CategoryId;
    public readonly string Description;
    public readonly Uri Link;
    public readonly Uri SectionLink;
    public readonly string PostalCode;
    public readonly uint Price;
    public readonly string Name;
    public readonly string BazosLocation;
    public readonly uint ImagesCount;

    public StoredListing(uint categoryId, string description, Uri link, Uri sectionLink, string postalCode,
        uint price, string name, string bazosLocation, uint imagesCount)
    {
        CategoryId = categoryId;
        Description = description;
        Link = link;
        SectionLink = sectionLink;
        PostalCode = postalCode;
        Price = price;
        Name = name;
        BazosLocation = bazosLocation;
        ImagesCount = imagesCount;
    }

    private string GetListingPath()
    {
        var dirName = Path.GetInvalidPathChars()
            .Aggregate(Name, (current, invalidPathChar) => current.Replace(invalidPathChar, '-'));
        return $"{ConfigLoader.ListingDirectory}{BazosLocation}/{dirName}/";
    }

    public void AntiImageBan()
    {
        var listingPath = GetListingPath();

        var randomGen = new Random();

        for (var i = 0; i < ImagesCount; i++)
        {
            var imgPath = $"{listingPath}{i}.jpg";
            using var image = Image.Load<Bgr24>(imgPath);

            var randomX = randomGen.Next(0, image.Size.Width);
            var randomY = randomGen.Next(0, image.Size.Height);

            image[randomX, randomY] = new Bgr24(255, 255, 255);
            image.SaveAsJpeg(imgPath);
        }
        
        Utils.Print($"Executed anti image ban feature for listing: {Name}, affected: {ImagesCount} images!", location: BazosLocation);
    }

    public void RestoreListing(ILocationProvider locationProvider, Config config)
    {
        var bazosUploadedImages = UploadImagesToBazos(locationProvider, config);
        var validationField = locationProvider.GetInputValidationField();

        var postContentPairs = new List<KeyValuePair<string, string>>
        {
            new("category", CategoryId.ToString()),
            new("nadpis", Name),
            new("popis", Description),
            new("cena", Price.ToString()),
            new("cenavyber", "1"),
            new("lokalita", PostalCode),
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
        httpClient.Post(new Uri(SectionLink + "insert.php"), httpContent);
        Utils.Print($"Re-created listing: {Name}!!", location: config.BazosLocation);
    }

    private List<string> UploadImagesToBazos(ILocationProvider locationProvider, Config config)
    {
        var imagesDirectory = GetListingPath();
        var bazosImgNames = new List<string>();

        for (var i = 0; i < ImagesCount; i++)
        {
            var imgPath = $"{imagesDirectory}{i}.jpg";
            var imgBytes = File.ReadAllBytes(imgPath);
            var bazosImgName = Utils.UploadImage(imgBytes, $"{Utils.RandomString(16)}.jpg", locationProvider, config, SectionLink);

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

        Utils.Print($"Saving listing: {Name} data to {path}", location: BazosLocation);
        File.WriteAllText(path, json);
    }
}