using System.Drawing;
using System.Drawing.Text;

namespace runeforge.Views;

internal static class FontLibrary
{
    private const string PreferredFontFamilyName = "KZ Supercell-Magic";
    private const float PreferredFontSizeScale = 0.84f;
    private const string NumericFontFamilyName = "Timaday";
    private const float NumericFontSizeScale = 1f;

    private static readonly Lazy<PrivateFontCollection> FontCollection = new(LoadFonts);
    private static readonly Lazy<FontFamily?> PrivateFontFamily = new(() => ResolvePrivateFontFamily(PreferredFontFamilyName));
    private static readonly Lazy<FontFamily?> NumericFontFamily = new(() => ResolvePrivateFontFamily(NumericFontFamilyName));

    public static Font Create(float size, FontStyle style)
    {
        return CreateInternal(PrivateFontFamily.Value, size, style, PreferredFontSizeScale);
    }

    public static Font CreateNumeric(float size, FontStyle style)
    {
        return CreateInternal(NumericFontFamily.Value ?? PrivateFontFamily.Value, size, style, NumericFontSizeScale);
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

    private static FontFamily? ResolvePrivateFontFamily(string familyName)
    {
        var family = FontCollection.Value.Families.FirstOrDefault(family =>
            string.Equals(family.Name, familyName, StringComparison.OrdinalIgnoreCase));

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

    private static Font CreateInternal(FontFamily? family, float size, FontStyle style, float sizeScale)
    {
        if (family == null)
        {
            return new Font("Segoe UI", size, style, GraphicsUnit.Pixel);
        }

        var resolvedStyle = ResolveStyle(family, style);
        return new Font(family, size * sizeScale, resolvedStyle, GraphicsUnit.Pixel);
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
