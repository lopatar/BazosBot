using System.IO;
using System.Text.Json;

namespace BazosBot;

internal static class ConfigLoader
{
    private const string FileDirectory = "Files/";
    private const string ConfigFile = "Files/Config.json";
    public const string ListingDirectory = FileDirectory + "ListingImages/";

    public static Config? LoadConfig()
    {
        if (!File.Exists(ConfigFile)) return CreateDummyConfig();

        var configJson = File.ReadAllText(ConfigFile);
        try
        {
            return JsonSerializer.Deserialize<Config>(configJson);
        }
        catch
        {
            return null;
        }
    }

    private static Config CreateDummyConfig()
    {
        CreateDirectories();

        var config = new Config
        {
            Jmeno = "Jmeno Prijmeni",
            Heslo = "Heslo",
            TelCislo = 606606606,
            BIdCookie = 88888888,
            BKodCookie = "XYXYXYXYXY",
            StariDoSmazani = 2
        };

        var configJson = JsonSerializer.Serialize(config);
        File.WriteAllText(ConfigFile, configJson);
        return config;
    }

    private static void CreateDirectories()
    {
        if (!Directory.Exists(FileDirectory)) Directory.CreateDirectory(FileDirectory);

        if (!Directory.Exists(ListingDirectory)) Directory.CreateDirectory(ListingDirectory);
    }
}