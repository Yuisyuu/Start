using System.Text.RegularExpressions;

namespace Start.Utils;

internal partial class Regex
{
    [GeneratedRegex("https://minecraft\\.azureedge\\.net/bin-win-preview/bedrock-server-(.+?)\\.zip")]
    internal static partial System.Text.RegularExpressions.Regex UrlRegex();
    [GeneratedRegex("[0-9]{0,}\\.[0-9]{0,}\\.[0-9]{0,}\\.[0-9]{0,}")]
    internal static partial System.Text.RegularExpressions.Regex VersionRegex();
}
