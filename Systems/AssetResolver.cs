namespace runeforge.Systems;

internal static class AssetResolver
{
    private const string AssetsDirectoryName = "Assets";

    public static string ResolveFile(params string[] relativePathSegments)
    {
        foreach (var assetRoot in EnumerateAssetRoots())
        {
            var path = Combine(assetRoot, relativePathSegments);
            if (File.Exists(path))
            {
                return path;
            }
        }

        throw new FileNotFoundException($"Asset file was not found: {Path.Combine(relativePathSegments)}.");
    }

    public static string ResolveDirectory(params string[] relativePathSegments)
    {
        foreach (var assetRoot in EnumerateAssetRoots())
        {
            var path = Combine(assetRoot, relativePathSegments);
            if (Directory.Exists(path))
            {
                return path;
            }
        }

        throw new DirectoryNotFoundException($"Asset directory was not found: {Path.Combine(relativePathSegments)}.");
    }

    public static string ResolveFileByName(string relativeDirectory, string fileName, SearchOption searchOption)
    {
        foreach (var assetRoot in EnumerateAssetRoots())
        {
            var directory = Path.Combine(assetRoot, relativeDirectory);
            if (!Directory.Exists(directory))
            {
                continue;
            }

            var directPath = Path.Combine(directory, fileName);
            if (File.Exists(directPath))
            {
                return directPath;
            }

            var recursivePath = Directory
                .EnumerateFiles(directory, fileName, searchOption)
                .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (recursivePath != null)
            {
                return recursivePath;
            }
        }

        throw new FileNotFoundException($"Asset file '{fileName}' was not found under Assets/{relativeDirectory}.");
    }

    private static IEnumerable<string> EnumerateAssetRoots()
    {
        yield return Path.Combine(AppContext.BaseDirectory, AssetsDirectoryName);
        yield return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", AssetsDirectoryName));
    }

    private static string Combine(string root, IReadOnlyList<string> relativePathSegments)
    {
        var path = root;
        foreach (var segment in relativePathSegments)
        {
            path = Path.Combine(path, segment);
        }

        return path;
    }
}
