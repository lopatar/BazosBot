using AngleSharp.Html.Parser;
using BazosBotv2.Bazos.LocationProviders;
using BazosBotv2.Configuration;
using BazosBotv2.Interfaces;
using BazosBotv2.Utilities;

namespace BazosBotv2.Bazos;

internal sealed class Bazos
{
    private readonly CategoryScraper _categoryScraper;
    private readonly Config _config;
    private readonly HtmlParser _htmlParser = new();
    private readonly List<BazosListing> _listings = new();
    private readonly ILocationProvider _locationProvider;

    public Bazos(Config config)
    {
        _config = config;
        Utils.Print("Initializing BazosBot", location: _config.BazosLocation);

        _locationProvider = InitLocationProvider();
        _categoryScraper = new(_locationProvider, _htmlParser, config);

        var validationField = InitValidationField();
        _locationProvider.SetInputValidationField(validationField);
        Utils.Print($"Got add listing validation field {validationField.Key} => {validationField.Value}",
            location: _config.BazosLocation);

        Utils.Print("Initialized BazosBot, press any key to fetch listings!", location: _config.BazosLocation);
        Console.ReadKey();
        InitListings();
    }

    public List<BazosListing> GetDueListings()
    {
        return _listings.Where(listing => listing.IsDueForRenewal()).ToList();
    }

    private void InitListings()
    {
        using var httpClient = new BazosHttp(_locationProvider, _config);
        using var htmlDocument = _htmlParser.ParseDocument(httpClient.Get(_locationProvider.GetMyListingPath()));
        var listingElements = htmlDocument.GetElementsByClassName("inzeraty inzeratyflex");
        var dateOnlyNow = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

        foreach (var listing in listingElements)
        {
            string listingName = "", listingLink = "", listingDateString = "";
            uint listingPrice = 0, listingPostalCode = 0;

            var skipCycle = false;
            
            foreach (var listingDiv in listing.Children)
                switch (listingDiv.GetAttribute("class"))
                {
                    case "hodnocenitext":
                        skipCycle = true;
                        continue;
                    case "inzeratynadpis":
                        var linkElement = listingDiv.Children[0];
                        var imgElement = linkElement.Children[0];

                        listingDateString = listingDiv.Children[2].TextContent.Replace(" ", "").Replace("-", "")
                            .Replace("[", "")
                            .Replace("]", "");
                        
                        listingName = imgElement.GetAttribute("alt") ?? "";
                        
                        if (listingDateString.Contains("TOP") && _config.SkipTopListings)
                        {
                            Utils.Print($"Skipping TOP listing: {listingName}", location: _config.BazosLocation);
                            skipCycle = true;
                            break;
                        }
                        
                        listingDateString = listingDateString.Replace("TOP", "");
                        listingLink = linkElement.GetAttribute("href") ?? "";
                        break;
                    case "inzeratycena":
                        listingPrice = Utils.ExtractUintFromText(listingDiv.TextContent);
                        break;
                    case "inzeratylok":
                        listingPostalCode = Utils.ExtractUintFromText(listingDiv.TextContent);
                        break;
                }

            if (skipCycle)
            {
                continue;
            }
            
            var listingId = uint.Parse(listingLink.Split('/')[4]);
            var listingDateParts = listingDateString.Split('.');
            var listingDate = new DateOnly(int.Parse(listingDateParts[2]), int.Parse(listingDateParts[1]),
                int.Parse(listingDateParts[0]));

            var listingAge = (uint)(dateOnlyNow.DayNumber - listingDate.DayNumber);

            _listings.Add(new(listingId, listingName, new(listingLink), listingPrice, listingPostalCode,
                listingAge, _htmlParser, _locationProvider, _config, _categoryScraper));
        }

        Utils.Print($"Got {_listings.Count} listings!", location: _config.BazosLocation);
    }

    private ILocationProvider InitLocationProvider()
    {
        return _config.BazosLocation switch
        {
            "cz" => new CzLocationProvider(),
            "sk" => new SkLocationProvider(),
            "pl" => new PlLocationProvider(),
            "at" => new AtLocationProvider(),
            _ => new CzLocationProvider() //Won't happen, since we check for validity of the location in ConfigLoader
        };
    }

    private KeyValuePair<string, string> InitValidationField()
    {
        var sampleSection = _categoryScraper.GetSampleSection();
        var uri = new Uri(
            $"https://{sampleSection}.{_locationProvider.GetUri().Host}/{_locationProvider.GetAddListingPath()}");
        using var httpClient = new BazosHttp(_locationProvider, _config);
        using var htmlDocument = _htmlParser.ParseDocument(httpClient.Get(uri));
        var addForm = htmlDocument.GetElementById("formpridani");

        foreach (var inputElement in addForm.GetElementsByTagName("INPUT"))
            if (inputElement.GetAttribute("type") == "hidden")
                return new(inputElement.GetAttribute("name") ?? "", inputElement.GetAttribute("value") ?? "");

        return new("", "");
    }
}