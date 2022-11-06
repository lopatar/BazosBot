using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Html.Parser;

namespace BazosBot;

internal sealed class Bazos : IDisposable
{
    private readonly Http _http;
    private readonly List<ExtractedListing> _listings;
    private readonly HtmlParser _parser = new();

    public Bazos(Config config)
    {
        _http = new(config);

        Utils.Print("Everything initialized, press any key to continue...");
        Console.ReadKey();

        CategoryScraper.ScrapeCategories(_parser, config);
        _listings = GetListings(config);
    }

    public void Dispose()
    {
        _http.Dispose();
    }

    private List<ExtractedListing> GetListings(Config config)
    {
        Utils.Print("Getting listings...");
        var htmlText = _http.GetResponse("moje-inzeraty.php");
        var htmlDocument = _parser.ParseDocument(htmlText);
        var listings = new List<ExtractedListing>();

        foreach (var htmlElement in htmlDocument.QuerySelectorAll("div"))
        {
            if (htmlElement.GetAttribute("class") != "inzeraty inzeratyflex") continue;

            var listingLink = "";
            var listingName = "";
            var listingDateString = "";
            uint listingPrice = 0;
            uint listingPostalCode = 0;

            foreach (var child in htmlElement.Children)
                switch (child.GetAttribute("class"))
                {
                    case "inzeratycena":
                        listingPrice = uint.Parse(new(child.TextContent.Where(char.IsDigit).ToArray()));
                        continue;
                    case "inzeratylok":
                        listingPostalCode = uint.Parse(new(child.TextContent.Where(char.IsDigit).ToArray()));
                        continue;
                    case "inzeratynadpis":
                        var linkElement = child.Children[0];
                        var imgElement = linkElement.Children[0];

                        listingDateString = child.Children[2].TextContent.Replace(" ", "").Replace("-", "")
                            .Replace("[", "")
                            .Replace("]", "");

                        listingLink = linkElement.GetAttribute("href") ?? "";
                        listingName = imgElement.GetAttribute("alt") ?? "";
                        continue;
                }

            var listingId = uint.Parse(listingLink.Split('/')[4]);
            var listingDateParts = listingDateString.Split('.');
            var listingDate = new DateOnly(int.Parse(listingDateParts[2]), int.Parse(listingDateParts[1]),
                int.Parse(listingDateParts[0]));

            listings.Add(
                new(listingName, listingId, listingLink, listingPrice,
                    listingPostalCode, listingDate, _parser, config));
        }

        Utils.Print($"Got {listings.Count} listings!");
        return listings;
    }

    public List<ExtractedListing> GetListingsDueForRenewal()
    {
        return _listings.Where(listing => listing.ShouldRenew()).ToList();
    }
}