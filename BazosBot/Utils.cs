using System;
using System.IO;
using System.Net.Http;

namespace BazosBot;

internal static class Utils
{
    public static void Print(string text, bool error = false)
    {
        if (error) Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine($"[BazosBot] {text}");

        if (error) Console.ForegroundColor = ConsoleColor.White;
    }

    public static void DownloadImage(string url, string filePath)
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = new(url)
        };

        var imageData = httpClient.GetByteArrayAsync("").GetAwaiter().GetResult();
        File.WriteAllBytes(filePath, imageData);
    }
}