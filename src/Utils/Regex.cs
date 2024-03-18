using System.Text.RegularExpressions;

namespace Start.Utils;

internal static partial class Regex
{
    [GeneratedRegex(@"https://minecraft\.azureedge\.net/bin-win-preview/bedrock-server-(.+?)\.zip")]
    public static partial System.Text.RegularExpressions.Regex UrlRegex();

    [GeneratedRegex(@"[0-9]{0,}\.[0-9]{0,}\.[0-9]{0,}\.[0-9]{0,}")]
    public static partial System.Text.RegularExpressions.Regex VersionRegex();
}