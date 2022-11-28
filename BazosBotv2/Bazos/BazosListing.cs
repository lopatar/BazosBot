using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BazosBotv2.Configuration;
using BazosBotv2.Interfaces;
using BazosBotv2.Utilities;
using Newtonsoft.Json;

namespace BazosBotv2.Bazos;

internal sealed class BazosListing
{
    private readonly uint _ageDays;
    private readonly uint _categoryId;
    private readonly Config _config;
    private readonly string _description;
    private readonly uint _id;
    private readonly List<string> _imagesList;
    private readonly ILocationProvider _locationProvider;
    private readonly uint _postalCode;
    private readonly uint _price;
    private readonly Uri _sectionLink;
    public readonly string Name;

    public BazosListing(uint id, string name, Uri link, uint price, uint postalCode, uint ageDays,
        IHtmlParser htmlParser,
        ILocationProvider locationProvider, Config config, CategoryScraper categoryScraper)
    {
        _id = id;
        Name = name;
        _sectionLink = new($"https://{link.Host}/");
        _price = price;
        _ageDays = ageDays;
        _postalCode = postalCode;
        _locationProvider = locationProvider;
        _config = config;

        using var httpClient = new BazosHttp(_locationProvider, _config);
        using var htmlDocument = htmlParser.ParseDocument(httpClient.Get(link));

        _description = InitDescription(htmlDocument);

        var categoryName = InitCategory(htmlDocument);
        var sectionName = _sectionLink.Host.Split('.')[0];
        _categoryId = categoryScraper.GetCategoryId(sectionName, categoryName);

        _imagesList = InitImages(htmlDocument);
    }

    public void Renew()
    {
        DownloadBazosImages();
        //DeleteFromBazos();
        //CreateListing();
    }

    public bool IsDueForRenewal()
    {
        return _ageDays >= _config.ListingDaysUntilRenewal;
    }

    private void DeleteFromBazos()
    {
        var deleteUri = new Uri(_sectionLink + "deletei2.php");
        using var httpClient = new BazosHttp(_locationProvider, _config);
        using var httpContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("heslobazar", _config.UserPassword),
            new KeyValuePair<string, string>("administrace", _locationProvider.GetDeleteListingField()),
            new KeyValuePair<string, string>("idad", _id.ToString())
        });

        Utils.Print($"Deleting listing: {Name} from {_locationProvider.GetUri()}", location: _config.BazosLocation);
        httpClient.Post(deleteUri, httpContent);
    }

    private void CreateListing()
    {
        var bazosUploadedImages = UploadImagesToBazos();
        var validationField = _locationProvider.GetInputValidationField();

        var postContentPairs = new List<KeyValuePair<string, string>>
        {
            new("category", _categoryId.ToString()),
            new("nadpis", Name),
            new("popis", _description),
            new("cena", _price.ToString()),
            new("cenavyber", "1"),
            new("lokalita", _postalCode.ToString()),
            new("jmeno", _config.UserName),
            new("telefoni", _config.UserPhoneNum.ToString()),
            new("maili", _config.UserEmail),
            new("heslobazar", _config.UserPassword),
            new(validationField.Key, validationField.Value),
            new("Submit", "submitButton")
        };

        postContentPairs.AddRange(bazosUploadedImages.Select(uploadedImage =>
            new KeyValuePair<string, string>("files[]", uploadedImage)));
        using var httpContent = new FormUrlEncodedContent(postContentPairs);
        using var httpClient = new BazosHttp(_locationProvider, _config);
        httpClient.Post(new Uri(_sectionLink + "insert.php"), httpContent);
        Utils.Print($"Re-created listing: {Name}!!", location: _config.BazosLocation);
    }

    private List<string> UploadImagesToBazos()
    {
        var imagesDirectory = GetImagesPath();
        var bazosImgNames = new List<string>();

        for (var i = 0; i < _imagesList.Count; i++)
        {
            var imgPath = $"{imagesDirectory}{i}.jpg";
            var imgBytes = File.ReadAllBytes(imgPath);
            var bazosImgName = UploadImage(imgBytes, $"{i}.jpg");

            Utils.Print($"Uploaded image: {imgPath} for listing: {Name} as: {bazosImgName}",
                location: _config.BazosLocation);
            bazosImgNames.Add(bazosImgName);
        }

        return bazosImgNames;
    }

    private string UploadImage(byte[] imgData, string imgName)
    {
        using var httpClient = new BazosHttp(_locationProvider, _config);
        using var requestContent = new MultipartFormDataContent("----WebKitFormBoundaryXXXXXXXXXXXXXXXX");
        requestContent.Add(new StreamContent(new MemoryStream(imgData)), "file[0]", imgName);
        var httpResponse = httpClient.Post(new Uri(_sectionLink + "upload.php"), requestContent);
        return JsonConvert.DeserializeObject<List<string>>(httpResponse)?[0] ?? "";
    }

    private void DownloadBazosImages()
    {
        InitImageDirectory();

        for (var i = 0; i < _imagesList.Count; i++)
        {
            var imageLink = _imagesList[i];
            var imagePath = $"{GetImagesPath()}{i}.jpg";

            Utils.Print($"Downloading image: {imagePath} for listing: {Name}", location: _config.BazosLocation);
            Utils.DownloadImage(new(imageLink), imagePath);
        }
    }

    private void InitImageDirectory()
    {
        var directoryPath = GetImagesPath();

        if (Directory.Exists(directoryPath))
        {
            Utils.Print($"Deleting old image files for listing: {Name}", location: _config.BazosLocation);
            Directory.Delete(directoryPath, true);
        }

        Utils.Print($"Creating directory: {directoryPath} for listing: {Name}", location: _config.BazosLocation);
        Directory.CreateDirectory(directoryPath);
    }

    private static string InitDescription(IHtmlDocument htmlDocument)
    {
        return htmlDocument.GetElementsByClassName("popisdetail")[0].TextContent;
    }

    private static string InitCategory(IHtmlDocument htmlDocument)
    {
        var navBarElement = htmlDocument.GetElementsByClassName("drobky")[0];
        return navBarElement.Children[2].TextContent;
    }

    private List<string> InitImages(IHtmlDocument htmlDocument)
    {
        var list = new List<string>();

        foreach (var imgElement in htmlDocument.QuerySelectorAll("img"))
        {
            var imgLink = imgElement.GetAttribute("src");

            if (imgLink == null || !imgLink.Contains(_id.ToString())) continue;

            var imgLinkParts = imgLink.Split('/');
            imgLinkParts[4] = imgLinkParts[4].Replace("t", ""); //t = thumbnail, we want full resolution

            imgLink = string.Join('/', imgLinkParts);
            list.Add(imgLink);
        }

        list.RemoveAt(0); //Removing first image, as the second one is a duplicate

        Utils.Print($"Got {list.Count} image links for listing: {Name}", location: _config.BazosLocation);
        return list;
    }

    private string GetImagesPath()
    {
        return $"{ConfigLoader.ListingDirectory}{Name.Replace(" ", "-")}/";
    }
}