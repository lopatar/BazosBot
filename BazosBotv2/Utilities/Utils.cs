using System.Diagnostics;
using System.Security.Cryptography;
using BazosBotv2.Bazos;
using BazosBotv2.Configuration;
using BazosBotv2.Interfaces;
using Newtonsoft.Json;

namespace BazosBotv2.Utilities;

internal static class Utils
{
    public static void Print(string message, bool error = false, string location = "", bool newLine = true, ConsoleColor consoleColor = ConsoleColor.White)
    {
        if (error) Console.ForegroundColor = ConsoleColor.Red;

        using var stringBuilder = DisposableStringBuilder.Get();

        stringBuilder.Append(location == string.Empty
            ? $"[BazosBot] {message}"
            : $"[BazosBot] [{location.ToUpper()}] {message}");

        if (newLine)
        { 
            Console.WriteLine(stringBuilder.ToString());
        }
        else
        {
           Console.Write(stringBuilder.ToString());
        }

        if (error) Console.ForegroundColor = ConsoleColor.White;
    }

    public static string UploadImage(byte[] imgData, string imgName, ILocationProvider locationProvider, Config config,
        Uri sectionLink)
    {
        using var httpClient = new BazosHttp(locationProvider, config);
        using var requestContent = new MultipartFormDataContent("----WebKitFormBoundary" + RandomString(16u));
        requestContent.Add(new StreamContent(new MemoryStream(imgData)), "file[0]", imgName);
        var httpResponse = httpClient.Post(sectionLink + "upload.php", requestContent);
        return JsonConvert.DeserializeObject<List<string>>(httpResponse)?[0] ?? "";
    }

    public static void Exit(string message = "", bool error = false, string location = "")
    {
        if (!error) Console.ForegroundColor = ConsoleColor.Yellow;

        Print(DisposableStringBuilder.StringQuick($"{message}, Press any key to exit..."), error, location);
        Console.ReadKey();

        Process.GetCurrentProcess().Kill();
    }

    public static void DownloadImage(Uri url, string filePath)
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = url;

        var imageBytes = httpClient.GetByteArrayAsync("").GetAwaiter().GetResult();
        File.WriteAllBytes(filePath, imageBytes);
    }

    public static string ExtractZipCodeFromLocation(string listingLocationInfo, ILocationProvider locationProvider)
    {
        return listingLocationInfo[^locationProvider.GetZipCodeLength()..]
            .Replace(" ", "");
    }

    public static uint ExtractUintFromString(string text)
    {
        return uint.Parse(text.Where(char.IsDigit).ToArray());
    }

    public static bool AskYesNoQuestion(string question, string bazosLocation)
    {
        Print(DisposableStringBuilder.StringQuick( $"{question} (Y/y = yes, other = no) [Default: yes]"), location: bazosLocation, newLine: false);
        var output = Console.ReadLine()?.ToUpper();
        return output != null;
    }

    public static string RandomString(uint length)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456789";
        using var stringBuilder = DisposableStringBuilder.Get();
        for (var i = 0; i < length; i++)
            stringBuilder.Append(alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)]);

        return stringBuilder.ToString();
    }
}