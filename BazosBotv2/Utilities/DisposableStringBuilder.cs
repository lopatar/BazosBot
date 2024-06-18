using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace BazosBotv2.Utilities;

internal class DisposableStringBuilder : IDisposable
{
    private readonly StringBuilder _stringBuilder;

    public DisposableStringBuilder(int capacity) => _stringBuilder = new StringBuilder(capacity);

    public DisposableStringBuilder() => _stringBuilder = new StringBuilder();

    public void Append(string text) => _stringBuilder.Append(text);
    public void Append(char character) => _stringBuilder.Append(character);
    public new string ToString() => _stringBuilder.ToString();

    public void Dispose() => _stringBuilder.Clear();

    public static string GetStringQuick(string text)
    {
        using var stringBuilder = Get();
        stringBuilder.Append(text);
        return stringBuilder.ToString();
    }

    public static DisposableStringBuilder Get()
    {
        return new DisposableStringBuilder();
    }
}