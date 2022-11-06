using System;
using System.Collections.Generic;
using AngleSharp.Html.Parser;

namespace BazosBot;

internal static class CategoryScraper
{
    //Jmeno rubriky, nazev kategorie - id kategorie
    private static readonly Dictionary<string, Dictionary<string, uint>> SectionCategories = new()
    {
        { "auto", new() },
        { "deti", new() },
        { "dum", new() },
        { "elektro", new() },
        { "foto", new() },
        { "hudba", new() },
        { "knihy", new() },
        { "mobil", new() },
        { "motorky", new() },
        { "nabytek", new() },
        { "obleceni", new() },
        { "pc", new() },
        { "prace", new() },
        { "reality", new() },
        { "sluzby", new() },
        { "sport", new() },
        { "stroje", new() },
        { "vstupenky", new() },
        { "zvirata", new() },
        { "ostatni", new() }
    };

    public static uint GetCategoryId(string sectionName, string categoryName)
    {
        return SectionCategories[sectionName][categoryName];
    }

    public static void ScrapeCategories(IHtmlParser htmlParser, Config config)
    {
        Utils.Print("Scraping categories...");
        foreach (var sectionName in SectionCategories.Keys)
        {
            using var httpClient = new Http(config);
            var httpResponse = httpClient.GetResponse(new Uri($"https://{sectionName}.bazos.cz/pridat-inzerat.php"));
            var htmlDocument = htmlParser.ParseDocument(httpResponse);
            var categorySelect = htmlDocument.GetElementsByName("category")[0];

            foreach (var option in categorySelect.Children)
            {
                var categoryId = uint.Parse(option.GetAttribute("value") ?? "0");

                if (categoryId == 0) continue;

                var categoryName = option.TextContent;

                SectionCategories[sectionName].Add(categoryName, categoryId);
            }

            Utils.Print($"Scraped {SectionCategories[sectionName].Count} categories for: {sectionName}");
        }
    }
}