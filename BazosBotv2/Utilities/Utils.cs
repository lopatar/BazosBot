using System.Diagnostics;
using AngleSharp.Text;
using BazosBotv2.Interfaces;

namespace BazosBotv2.Utilities;

internal static class Utils
{
    public static void Print(string message, bool error = false, string location = "")
    {
        if (error) Console.ForegroundColor = ConsoleColor.Red;

        message = location == "" ? $"[BazosBot] {message}" : $"[BazosBot] [{location.ToUpper()}] {message}";

        Console.WriteLine(message);

        if (error) Console.ForegroundColor = ConsoleColor.White;
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
}