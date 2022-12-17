using AngleSharp.Html.Parser;
using BazosBotv2.Configuration;
using BazosBotv2.Interfaces;
using BazosBotv2.Utilities;

namespace BazosBotv2.Bazos;

internal sealed class CategoryScraper
{
    private readonly Config _config;
    private readonly IHtmlParser _htmlParser;
    private readonly ILocationProvider _locationProvider;
    private readonly Dictionary<string, Dictionary<string, uint>> _sectionCategoryIds = new();

    public CategoryScraper(ILocationProvider locationProvider, IHtmlParser htmlParser, Config config)
    {
        _locationProvider = locationProvider;
        _htmlParser = htmlParser;
        _config = config;

        InitSections();
        InitCategories();
    }

    public string GetSampleSection()
    {
        return _sectionCategoryIds.Keys.ElementAt(0);
    }

    public uint GetCategoryId(string sectionName, string categoryName)
    {
        return _sectionCategoryIds[sectionName][categoryName];
    }

    private void InitSections()
    {
        using var httpClient = new BazosHttp(_locationProvider, _config);
        using var htmlDocument = _htmlParser.ParseDocument(httpClient.Get(""));
        var sectionElement = htmlDocument.GetElementsByName("rubriky")[0];

        for (var i = 1;
             i < sectionElement.Children.Length;
             i++) //starting from 1, because the first section is "All sections"
        {
            var sectionName = sectionElement.Children[i].GetAttribute("value") ?? "";
            _sectionCategoryIds.Add(sectionName, new());
        }
    }

    private void InitCategories()
    {
        var categoryCount = 0;

        foreach (var sectionName in _sectionCategoryIds.Keys)
        {
            try
            {
                var uri = new Uri(
                    $"https://{sectionName}.{_locationProvider.GetUri().Host}/{_locationProvider.GetAddListingPath()}");
                using var httpClient = new BazosHttp(_locationProvider, _config);
                using var htmlDocument = _htmlParser.ParseDocument(httpClient.Get(uri));
                var categoryElement = htmlDocument.GetElementById("category");

                for (var i = 1;
                     i < categoryElement.Children.Length;
                     i++) //starting from 1, because the first category is "Select category"
                {
                    var categoryOption = categoryElement.Children[i];
                    var categoryName = categoryOption.TextContent;
                    var categoryId = uint.Parse(categoryOption.GetAttribute("value") ?? "0");

                    _sectionCategoryIds[sectionName].Add(categoryName, categoryId);
                }

                categoryCount += _sectionCategoryIds[sectionName].Count;
            }
            catch
            {
                Utils.Exit($"Could not access section: {sectionName} categories! Please check that your Config.json contains the valid values!", true, _config.BazosLocation);
            }
        }

        Utils.Print($"Scraped {categoryCount} categories in {_sectionCategoryIds.Count} sections!",
            location: _config.BazosLocation);
    }
}