using System.Drawing;
using System.Drawing.Text;

namespace runeforge.Views;

internal static class FontLibrary
{
    private const string PreferredFontFamilyName = "Timaday";

    private static readonly Lazy<PrivateFontCollection> FontCollection = new(LoadFonts);
    private static readonly Lazy<FontFamily?> PrivateFontFamily = new(ResolvePrivateFontFamily);

    public static Font Create(float size, FontStyle style)
    {
        var family = PrivateFontFamily.Value;
        if (family == null)
        {
            return new Font("Segoe UI", size, style, GraphicsUnit.Pixel);
        }

        var resolvedStyle = ResolveStyle(family, style);
        return new Font(family, size, resolvedStyle, GraphicsUnit.Pixel);
    }

    private static PrivateFontCollection LoadFonts()
    {
        var collection = new PrivateFontCollection();

        foreach (var path in ResolveFontPaths())
        {
            collection.AddFontFile(path);
        }

        return collection;
    }

    private static FontFamily? ResolvePrivateFontFamily()
    {
        var family = FontCollection.Value.Families.FirstOrDefault(static family =>
            string.Equals(family.Name, PreferredFontFamilyName, StringComparison.OrdinalIgnoreCase));

        if (family != null)
        {
            return family;
        }

        if (FontCollection.Value.Families.Length > 0)
        {
            return FontCollection.Value.Families[0];
        }

        return null;
    }

    private static FontStyle ResolveStyle(FontFamily family, FontStyle requestedStyle)
    {
        if (family.IsStyleAvailable(requestedStyle))
        {
            return requestedStyle;
        }

        if ((requestedStyle & FontStyle.Bold) != 0 && family.IsStyleAvailable(FontStyle.Bold))
        {
            return FontStyle.Bold;
        }

        if (family.IsStyleAvailable(FontStyle.Regular))
        {
            return FontStyle.Regular;
        }

        return family.IsStyleAvailable(FontStyle.Italic)
            ? FontStyle.Italic
            : FontStyle.Regular;
    }

    private static IReadOnlyList<string> ResolveFontPaths()
    {
        string[] candidateDirectories =
        [
            Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets", "Fonts"))
        ];

        foreach (var candidateDirectory in candidateDirectories)
        {
            if (!Directory.Exists(candidateDirectory))
            {
                continue;
            }

            var paths = Directory
                .EnumerateFiles(candidateDirectory, "*.*", SearchOption.AllDirectories)
                .Where(static path =>
                    path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".otf", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (paths.Length > 0)
            {
                return paths;
            }
        }

        return Array.Empty<string>();
    }
}
