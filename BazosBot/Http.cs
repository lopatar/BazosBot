using System;
using System.Net;
using System.Net.Http;

namespace BazosBot;

internal sealed class Http : IDisposable
{
    private const string BazosUrl = "https://bazos.cz/";
    private readonly CookieContainer _cookieContainer = new();
    private readonly HttpClient _httpClient;

    public Http(Config config)
    {
        SetUpCookieContainer(config);

        _httpClient = new(new HttpClientHandler
        {
            CookieContainer = _cookieContainer
        });
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    public string GetResponse(string file)
    {
        _httpClient.BaseAddress = new(BazosUrl + file);
        return _httpClient.GetStringAsync("").GetAwaiter().GetResult();
    }

    public string GetResponse(Uri uri)
    {
        _httpClient.BaseAddress = uri;
        return _httpClient.GetStringAsync("").GetAwaiter().GetResult();
    }

    public string Post(Uri uri, HttpContent content)
    {
        _httpClient.BaseAddress = uri;
        return _httpClient.PostAsync("", content).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter()
            .GetResult();
    }

    private void SetUpCookieContainer(Config config)
    {
        _cookieContainer.Add(new CookieCollection
        {
            BuildCookie("bheslo", config.Heslo),
            BuildCookie("bjmeno", config.Jmeno),
            BuildCookie("btelefon", config.TelCislo.ToString()),
            BuildCookie("bid", config.BIdCookie.ToString()),
            BuildCookie("bkod", "58TDJXW48P")
        });
    }

    private static Cookie BuildCookie(string name, string value)
    {
        return new(name, value, "/", "bazos.cz");
    }
}