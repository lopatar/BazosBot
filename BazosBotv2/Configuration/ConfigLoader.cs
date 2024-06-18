using BazosBotv2.Utilities;
using Newtonsoft.Json;

namespace BazosBotv2.Configuration;

internal static class ConfigLoader
{
    private const string FilesDirectory = "Files/";
    private const string ConfigFile = $"{FilesDirectory}Config.json";
    public const string ListingDirectory = $"{FilesDirectory}Listings/";

    private static readonly List<string> BazosTypes = new()
    {
        "cz",
        "sk",
        "pl",
        "at"
    };

    private static List<Config> _loadedConfigs = new();

    public static void LoadConfigs()
    {
        if (!File.Exists(ConfigFile))
        {
            CreateDummyConfigs();
            Utils.Print($"Created initial config file {ConfigFile}, please edit it. Press any key to continue!");
            Console.ReadKey();

            LoadConfigs(); //re-load the configs
        }

        var configJson = File.ReadAllText(ConfigFile);

        try
        {
            _loadedConfigs = JsonConvert.DeserializeObject<List<Config>>(configJson) ?? new List<Config>();
        }
        catch
        {
            Utils.Exit($"Failed deserializing {ConfigFile}, incorrect format?", true);
        }

        //reject invalid BazosLocation
        foreach (var config in _loadedConfigs.Where(config => !BazosTypes.Contains(config.BazosLocation)))
            Utils.Exit(
                $"{ConfigFile} does not have a correct {nameof(config.BazosLocation)}, value: {config.BazosLocation}",
                true);
    }

    public static List<Config> GetEnabledConfigs()
    {
        return _loadedConfigs.Where(config => config.Enabled).ToList();
    }

    private static void CreateDummyConfigs()
    {
        InitDirectories();
        var dummyConfigs = new List<Config>();

        foreach (var bazosType in BazosTypes)
        {
            var config = new Config(bazosType, false, "First Last", "Password", "test@example.com", 606606606, 88888888,
                "XYXYXYXYXY", 2, true, true);

            Utils.Print("Creating initial Config.json file", location: bazosType);
            dummyConfigs.Add(config);
        }

        var configsJson = JsonConvert.SerializeObject(dummyConfigs, Formatting.Indented);
        File.WriteAllText(ConfigFile, configsJson);
    }

    private static void InitDirectories()
    {
        Utils.Print("Creating initial directory structure!");

        if (!Directory.Exists(FilesDirectory))
        {
            Utils.Print("Creating files directory");
            Directory.CreateDirectory(FilesDirectory);
        }

        if (!Directory.Exists(ListingDirectory))
        {
            Utils.Print("Creating listings directory");
            Directory.CreateDirectory(ListingDirectory);
        }

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var bazosType in BazosTypes)
        {
            var countryListingPath = Path.Combine(ListingDirectory, bazosType);

            if (Directory.Exists(countryListingPath))
                continue;

            Utils.Print("Creating data directory", location: bazosType);
            Directory.CreateDirectory(countryListingPath);
        }
    }
}