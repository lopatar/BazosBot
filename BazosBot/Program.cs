using System;
using BazosBot;

Utils.Print("Welcome to BazosBot v0.1");
Utils.Print("Reading Config.json");

var config = ConfigLoader.LoadConfig();
var configLoaded = config != null;

Utils.Print(configLoaded ? "Successfully loaded Config.json" : "Failed reading Config.json, wrong format!!",
    !configLoaded);

if (!configLoaded)
{
    Console.ReadKey();
    return;
}

config?.PrintValues();

try
{
    using var bazos = new Bazos(config);

    var renewalListings = bazos.GetListingsDueForRenewal();
    Utils.Print($"{renewalListings.Count} are due for renewal! Press any key to continue.....");

    Console.ReadKey();

    foreach (var listing in renewalListings)
    {
        Utils.Print($"Trying to renew: {listing.Name}");
        listing.Renew();
    }
}
catch
{
    Utils.Print("Error occured, please check that your Config.json is filled with correct values!", true);
}