using BazosBotv2.Configuration;
using Newtonsoft.Json;

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
        return $"{ConfigLoader.ListingDirectory}{BazosLocation}/{Name.Replace(" ", "-").Replace("|", "")}/";
    }

    public void Save()
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        var path = $"{GetListingPath()}Data.json";
        
        Utilities.Utils.Print($"Saving listing: {Name} data to {path}", location: BazosLocation);
        File.WriteAllText(path, json);
    }
}