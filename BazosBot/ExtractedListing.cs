using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace BazosBot;

internal sealed class ExtractedListing
{
    private readonly uint _bazosId;
    private readonly uint _categoryId;
    private readonly Config _config;
    private readonly int _daysAge;
    private readonly string _description;
    private readonly List<string?> _imagesLinks;
    private readonly uint _postalCode;
    private readonly uint _price;
    private readonly Uri _sectionLink;
    public readonly string Name;

    public ExtractedListing(string name, uint bazosId, string link, uint price,
        uint postalCode, DateOnly creationDate, IHtmlParser htmlParser, Config config)
    {
        Name = name;
        _bazosId = bazosId;
        Uri link1 = new(link);
        _price = price;
        _postalCode = postalCode;
        _config = config;
        _sectionLink = new($"https://{link1.Authority}/");
        var sectionName = _sectionLink.Host.Split('.')[0];

        var dateOnlyNow = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        _daysAge = dateOnlyNow.DayNumber - creationDate.DayNumber;

        using var httpClient = new HttpClient
        {
            BaseAddress = new(link)
        };

        var listingHtml = httpClient.GetStringAsync("").GetAwaiter().GetResult();
        var htmlDocument = htmlParser.ParseDocument(listingHtml);

        _description = GetDescription(htmlDocument) ?? "";
        _imagesLinks = GetImagesLinks(htmlDocument);
        var category = GetCategory(htmlDocument) ?? "";
        _categoryId = CategoryScraper.GetCategoryId(sectionName, category);

        Utils.Print($"Gathered {_imagesLinks.Count} image links for: {Name}");
    }

    private string? GetCategory(IHtmlDocument htmlDocument)
    {
        var navBar = htmlDocument.GetElementsByClassName("drobky").FirstOrDefault();
        return navBar?.Children[2].TextContent;
    }

    private static string? GetDescription(IDocument htmlDocument)
    {
        var descriptionDiv = htmlDocument.GetElementsByClassName("popisdetail").FirstOrDefault();
        return descriptionDiv?.TextContent.Replace("O tomto produktu", "").Replace("O této položce", "")
            .Replace("?", "");
    }

    private List<string?> GetImagesLinks(IDocument htmlDocument)
    {
        var list = htmlDocument.QuerySelectorAll("img").Select(imgElement => imgElement.GetAttribute("src"))
            .Where(link => link != null && link.Contains(_bazosId.ToString())).ToList();

        list.RemoveAt(0); //to prevent duplicating the main image
        return list;
    }

    public bool ShouldRenew()
    {
        return _daysAge >= _config.StariDoSmazani;
    }

    private void DownloadImages()
    {
        var directory = GetListingImagesPath();

        if (Directory.Exists(directory))
        {
            Utils.Print($"Deleting old saved images for: {Name}");
            Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);
        }
        else
        {
            Directory.CreateDirectory(directory);
        }

        Utils.Print($"Downloading Bazos images for {Name}");
        for (var i = 0; i < _imagesLinks.Count; i++)
        {
            var imagesLink = _imagesLinks[i] ?? "";
            Utils.DownloadImage(imagesLink, $"{directory}{i}.jpg");
        }
    }

    private void Remove()
    {
        var uri = new Uri(_sectionLink + "deletei2.php");
        using var httpClient = new Http(_config);

        using var values = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("heslobazar", _config.Heslo),
            new KeyValuePair<string, string>("administrace", "Vymazat"),
            new KeyValuePair<string, string>("idad", _bazosId.ToString())
        });

        Utils.Print($"Removing Bazos listing for {Name}");
        httpClient.Post(uri, values);
    }

    private IEnumerable<string> UploadImages()
    {
        var imagesPath = GetListingImagesPath();
        var backendImgNames = new List<string>();

        for (var i = 0; i < _imagesLinks.Count; i++)
        {
            var imgPath = $"{imagesPath}{i}.jpg";
            var imgBytes = File.ReadAllBytes(imgPath);
            backendImgNames.Add(UploadImage(imgBytes, $"{i}.jpg"));
        }

        return backendImgNames;
    }

    private string UploadImage(byte[] data, string name)
    {
        using var httpClient = new Http(_config);
        using var requestContent = new MultipartFormDataContent("----WebKitFormBoundaryXXXXXXXXXXXXXXXX");
        requestContent.Add(new StreamContent(new MemoryStream(data)), "file[0]", name);

        Utils.Print($"Uploading image: {name} for: {Name}");

        var httpResponse = httpClient.Post(new(_sectionLink + "upload.php"), requestContent);
        return JsonSerializer.Deserialize<List<string>>(httpResponse)?[0] ?? "";
    }

    private void CreateListing()
    {
        var uploadedImages = UploadImages();
        var keyValuePairs = new List<KeyValuePair<string, string>>
        {
            new("category", _categoryId.ToString()),
            new("nadpis", Name),
            new("popis", _description),
            new("cena", _price.ToString()),
            new("cenavyber", "1"),
            new("lokalita", _postalCode.ToString()),
            new("jmeno", _config.Jmeno),
            new("telefoni", _config.TelCislo.ToString()),
            new("maili", ""),
            new("heslobazar", _config.Heslo),
            new("sfsdf", "sdfwerewr"),
            new("Submit", "Odeslat")
        };

        keyValuePairs.AddRange(uploadedImages.Select(uploadedImage =>
            new KeyValuePair<string, string>("files[]", uploadedImage)));
        using var httpContent = new FormUrlEncodedContent(keyValuePairs);
        using var httpClient = new Http(_config);
        Utils.Print($"Re-created listing: {Name}");
        httpClient.Post(new(_sectionLink + "insert.php"), httpContent);
    }

    private string GetListingImagesPath()
    {
        return $"{ConfigLoader.ListingDirectory}{Name.Replace(" ", "-")}/";
    }

    public void Renew()
    {
        DownloadImages();
        Remove();
        CreateListing();
    }
}