using Microsoft.Extensions.Logging;
using ModularPipelines.FileSystem;
using System.IO.Compression;
using MPFile = ModularPipelines.FileSystem.File;

namespace NRG.Matrix.Build;

public record MyZipFile(string Name, MPFile File);

public static class Utils
{
    public static async Task<Stream> ZipFilesAndFolders(MPFile[] files, Folder[] folders, ILogger logger)
    {
        var fi = files
            .Where(e => e.Exists)
            .Select(e => new MyZipFile(e.Name, e));
        var fo = folders
            .Where(e => e.Exists)
            .SelectMany(x => x.GetFiles(e => true).Select(e => new MyZipFile(Path.GetRelativePath(x.Parent?.Path ?? "", e.Path), e)));
        MyZipFile[] toZip = [.. fi, .. fo];

        var zipStream = new MemoryStream();
        using var zip = new ZipArchive(zipStream, ZipArchiveMode.Update, true);
        foreach (var file in toZip)
        {
            try
            {
                var entry = await zip.CreateEntryFromFileAsync(file.File.Path, file.Name, CompressionLevel.Optimal);
                using var es = entry.Open();
                using var fs = file.File.GetStream();
                await fs.CopyToAsync(es);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to zip '{Path}'", file.File.Path);
            }
        }

        zipStream.Position = 0;
        return zipStream;
    }
}
