using System.Net;
using System.Security.Authentication;
using BazosBotv2.Configuration;
using BazosBotv2.Interfaces;

namespace BazosBotv2.Bazos;

internal sealed class BazosHttp : IDisposable
{
    private readonly Config _config;
    private readonly HttpClient _httpClient;
    private readonly ILocationProvider _locationProvider;

    public BazosHttp(ILocationProvider locationProvider, Config config)
    {
        _locationProvider = locationProvider;
        _config = config;
        _httpClient = new HttpClient(new HttpClientHandler
        {
            CookieContainer = InitCookieContainer(),
            SslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12
        });
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    public string Get(string fileName)
    {
        return Get(BuildUri(fileName));
    }

    public string Get(Uri uri)
    {
        _httpClient.BaseAddress = uri;
        return _httpClient.GetStringAsync("").GetAwaiter().GetResult();
    }

    public string Post(string fileName, HttpContent httpContent)
    {
        return Post(BuildUri(fileName), httpContent);
    }

    public string Post(Uri uri, HttpContent httpContent)
    {
        _httpClient.BaseAddress = uri;
        return _httpClient.PostAsync("", httpContent).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter()
            .GetResult();
    }

    private CookieContainer InitCookieContainer()
    {
        var cookieContainer = new CookieContainer();

        cookieContainer.Add(new CookieCollection
        {
            BuildCookie("bjmeno", _config.UserName),
            BuildCookie("bmail", _config.UserEmail),
            BuildCookie("bheslo", _config.UserPassword),
            BuildCookie("btelefon", _config.UserPhoneNum.ToString()),
            BuildCookie("bid", _config.UserCookieBId.ToString()),
            BuildCookie("bkod", _config.UserCookieBKod)
        });

        return cookieContainer;
    }

    private Cookie BuildCookie(string name, string value)
    {
        return new Cookie(name, value, "/", $".{_locationProvider.GetUri().Host}");
    }

    private Uri BuildUri(string fileName)
    {
        return new Uri(_locationProvider.GetUri() + fileName);
    }
}