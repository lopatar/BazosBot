using BazosBotv2.Configuration;
using Newtonsoft.Json;

namespace BazosBotv2.Bazos.ListingRestorer;

internal struct StoredListing
{
    public readonly uint CategoryId;
    public readonly string Description;
    public readonly uint Id;
    public readonly Uri Link;
    public readonly Uri SectionLink;
    public readonly string PostalCode;
    public readonly uint Price;
    public readonly string Name;
    public readonly string BazosLocation;
    public readonly uint ImagesCount;
    
    public StoredListing(uint categoryId, string description, uint id, Uri link, Uri sectionLink, string postalCode,
        uint price, string name, string bazosLocation, uint imagesCount)
    {
        CategoryId = categoryId;
        Description = description;
        Id = id;
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
        var json = JsonConvert.SerializeObject(this);
        File.WriteAllText($"{GetListingPath()}/Data.json", json);
    }
}