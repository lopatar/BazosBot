using AngleSharp.Html.Parser;
using BazosBotv2.Bazos.LocationProviders;
using BazosBotv2.Configuration;
using BazosBotv2.Interfaces;
using BazosBotv2.Utilities;
using Newtonsoft.Json;

namespace BazosBotv2.Bazos;

internal sealed class Bazos
{
    private readonly CategoryScraper _categoryScraper;
    private readonly Config _config;
    private readonly HtmlParser _htmlParser = new();
    private readonly List<BazosListing> _listings = new();
    private readonly ILocationProvider _locationProvider;
    private readonly List<StoredListing> _storedListings = new();

    public Bazos(Config config)
    {
        _config = config;
        Utils.Print("Initializing BazosBot", location: _config.BazosLocation);

        _locationProvider = InitLocationProvider();
        _categoryScraper = new CategoryScraper(_locationProvider, _htmlParser, config);

        var validationField = InitValidationField();
        _locationProvider.SetInputValidationField(validationField);
        Utils.Print(
            DisposableStringBuilder.StringQuick(
                $"Got add listing validation field {validationField.Key} => {validationField.Value}"),
            location: _config.BazosLocation);

        Utils.Print("Initialized BazosBot, press any key to fetch listings!", location: _config.BazosLocation);
        Console.ReadKey();
        InitListings();
    }

    public uint GetListingCount()
    {
        return (uint)_listings.Count;
    }

    private bool ListingUrlExists(Uri url)
    {
        return _listings.Any(listing => listing.Link == url);
    }

    public void RestoreListings()
    {
        Utils.Print("Restorer enabled! Loading stored listings!", location: _config.BazosLocation);

        var path = DisposableStringBuilder.StringQuick($"{ConfigLoader.ListingDirectory}{_config.BazosLocation}/");
        var directories = Directory.EnumerateDirectories(path);

        foreach (var directoryPath in directories)
        {
            var dataJsonPath = DisposableStringBuilder.StringQuick($"{directoryPath}/Data.json");
            var json = File.ReadAllText(dataJsonPath);
            var storedListing = JsonConvert.DeserializeObject<StoredListing>(json);

            if (!ListingUrlExists(storedListing.Link)) _storedListings.Add(storedListing);
        }

        Utils.Print(
            DisposableStringBuilder.StringQuick(
                $"Got {_storedListings.Count} stored listings that have been deleted! Press any key to continue!"),
            location: _config.BazosLocation);
        Console.ReadKey();

        var executeAntiBan = false;

        if (_storedListings.Count > 0)
            executeAntiBan =
                Utils.AskYesNoQuestion("Do you want to execute anti image banning?", _config.BazosLocation);

        //If restored at least 1 listing
        var restoredListing = false;

        foreach (var deletedListing in _storedListings)
        {
            var restoreDeletedListing = Utils.AskYesNoQuestion(DisposableStringBuilder.StringQuick(
                $"Do you want to restore deleted listing: {deletedListing.Name}?"), _config.BazosLocation);

            // ReSharper disable once InvertIf
            if (!restoreDeletedListing)
            {
                Utils.Print(
                    DisposableStringBuilder.StringQuick($"Skipping restoring deleted listing: {deletedListing.Name}"),
                    location: _config.BazosLocation);
                continue;
            }

            if (executeAntiBan)
            {
                Utils.Print(
                    DisposableStringBuilder.StringQuick(
                        $"Executing anti ban feature for listing: {deletedListing.Name}!"),
                    location: _config.BazosLocation);
                deletedListing.AntiImageBan();
            }

            deletedListing.RestoreListing(_locationProvider, _config);

            restoredListing = true;
        }

        if (!restoredListing) return;

        Utils.Print("At least 1 listing has been restored, going to re-fetch the listings!",
            location: _config.BazosLocation);
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
            string listingName = "", listingLink = "", listingDateString = "", listingPostalCode = "";
            uint listingPrice = 0;

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
                            Utils.Print(DisposableStringBuilder.StringQuick($"Skipping TOP listing: {listingName}"),
                                location: _config.BazosLocation);
                            skipCycle = true;
                            break;
                        }

                        listingDateString = listingDateString.Replace("TOP", "");
                        listingLink = linkElement.GetAttribute("href") ?? "";
                        break;
                    case "inzeratycena":
                        listingPrice = Utils.ExtractUintFromString(listingDiv.TextContent);
                        break;
                    case "inzeratylok":
                        listingPostalCode = Utils.ExtractZipCodeFromLocation(listingDiv.TextContent, _locationProvider);
                        break;
                }

            if (skipCycle) continue;

            var listingId = uint.Parse(listingLink.Split('/')[4]);
            var listingDateParts = listingDateString.Split('.');
            var listingDate = new DateOnly(int.Parse(listingDateParts[2]), int.Parse(listingDateParts[1]),
                int.Parse(listingDateParts[0]));

            var listingAge = (uint)(dateOnlyNow.DayNumber - listingDate.DayNumber);

            _listings.Add(new BazosListing(listingId, listingName, new Uri(listingLink), listingPrice,
                listingPostalCode,
                listingAge, _htmlParser, _locationProvider, _config, _categoryScraper));
        }

        Utils.Print(DisposableStringBuilder.StringQuick($"Got {_listings.Count} listings!"),
            location: _config.BazosLocation);
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
        var uri = new Uri(DisposableStringBuilder.StringQuick(
            $"https://{sampleSection}.{_locationProvider.GetUri().Host}/{_locationProvider.GetAddListingPath()}"));
        using var httpClient = new BazosHttp(_locationProvider, _config);
        using var htmlDocument = _htmlParser.ParseDocument(httpClient.Get(uri));
        var addForm = htmlDocument.GetElementById("formpridani");

        if (addForm == null) Utils.Exit("Invalid \"bid\" and \"bid\" Config.json values", true, _config.BazosLocation);

        foreach (var inputElement in addForm.GetElementsByTagName("INPUT"))
            if (inputElement.GetAttribute("type") == "hidden")
                return new KeyValuePair<string, string>(inputElement.GetAttribute("name") ?? "",
                    inputElement.GetAttribute("value") ?? "");

        return new KeyValuePair<string, string>("", "");
    }
}