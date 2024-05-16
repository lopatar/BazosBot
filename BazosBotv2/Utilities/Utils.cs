using System.Diagnostics;
using BazosBotv2.Bazos;
using BazosBotv2.Configuration;
using BazosBotv2.Interfaces;

namespace BazosBotv2.Utilities;

internal static class Utils
{
    public static void Print(string message, bool error = false, string location = "", bool newLine = true)
    {
        if (error) Console.ForegroundColor = ConsoleColor.Red;

        message = location == "" ? $"[BazosBot] {message}" : $"[BazosBot] [{location.ToUpper()}] {message}";

        if (newLine)
            Console.WriteLine(message);
        else
            Console.Write(message);

        if (error) Console.ForegroundColor = ConsoleColor.White;
    }

    public static string UploadImage(byte[] imgData, string imgName, ILocationProvider locationProvider, Config config,
        Uri sectionLink)
    {
        using var httpClient = new BazosHttp(locationProvider, config);
        using var requestContent = new MultipartFormDataContent("----WebKitFormBoundary" + RandomString(16u));
        requestContent.Add(new StreamContent(new MemoryStream(imgData)), "file[0]", imgName);
        var httpResponse = httpClient.Post(new Uri(sectionLink + "upload.php"), requestContent);
        return JsonConvert.DeserializeObject<List<string>>(httpResponse)?[0] ?? "";
    }

    public static void Exit(string message = "", bool error = false, string location = "")
    {
        if (!error) Console.ForegroundColor = ConsoleColor.Yellow;

        Print($"{message}, Press any key to exit...", error, location);
        Console.ReadKey();
        Process.GetCurrentProcess().Kill();
    }

    public static void DownloadImage(Uri url, string filePath)
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = url
        };

        var imageBytes = httpClient.GetByteArrayAsync("").GetAwaiter().GetResult();
        File.WriteAllBytes(filePath, imageBytes);
    }

    public static string ExtractZipCodeFromLocation(string listingLocationInfo, ILocationProvider locationProvider)
    {
        return listingLocationInfo.Substring(listingLocationInfo.Length - locationProvider.GetZipCodeLength())
            .Replace(" ", "");
    }

    public static uint ExtractUintFromString(string text)
    {
        return uint.Parse(text.Where(char.IsDigit).ToArray());
    }

    public static string RandomString(uint length)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456789";
        var outputString = "";
        var random = new Random();

        for (var i = 0; i < length; i++) outputString += alphabet[random.Next(0, alphabet.Length)];

        return outputString;
    }
}