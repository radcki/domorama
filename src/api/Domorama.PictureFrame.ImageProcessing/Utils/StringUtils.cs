namespace Domorama.PictureFrame.ImageProcessing.Utils;

public static class StringUtils
{
    public static string? JoinNotNull(string separator, params IEnumerable<string?> strings) => string.Join(separator, strings.OfType<string>());
}