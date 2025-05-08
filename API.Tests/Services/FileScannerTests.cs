using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs.Internal.Scanner;
using API.Entities.Enums;
using API.Services;
using API.Services.Tasks.Scanner;
using API.Services.Tasks.Scanner.Parser;
using API.Tests.Helpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace API.Tests.Services;

public class FileScannerTests : AbstractDbTest
{
    private readonly FileScanner _fileScanner;
    private readonly IDirectoryService _directoryService;
    private readonly ScannerHelper _scannerHelper;
    private readonly string _outputDirectory = Path.Join(Directory.GetCurrentDirectory(), "../../../Services/Test Data/ScannerService/ScanTests");
    private readonly string _testDirectory = Path.Join(Directory.GetCurrentDirectory(), "../../../Services/Test Data/ScannerService/TestCases");

    public FileScannerTests(ITestOutputHelper testOutputHelper)
    {
        _directoryService = new DirectoryService(Substitute.For<ILogger<DirectoryService>>(), new FileSystem());
        _fileScanner = new FileScanner(_directoryService, UnitOfWork);
        _scannerHelper = new ScannerHelper(UnitOfWork, testOutputHelper);
    }

    #region ScanFiles - Basic Tests

    /// <summary>
    /// Validates that FileTypePattern works
    /// </summary>
    [Fact]
    public async Task ScanFiles_ShouldIncludeOnlyArchiveTypes()
    {
        const string testcase = "Flat Series - Manga.json";
        var library = await _scannerHelper.GenerateScannerData(testcase);
        var folder = library.Folders.First().Path;

        var options = new ScannerOption
        {
            FolderPaths = [folder],
            FileTypePattern = [FileTypeGroup.Archive],
            ExcludePatterns = []
        };

        var result = _fileScanner.ScanFiles(options);

        Assert.Single(result); // One folder
        var scanned = result[0];
        Assert.Equal(Parser.NormalizePath(Path.Join(folder, "My Dress-Up Darling")), scanned.DirectoryPath);
        Assert.All(scanned.Files, file =>
        {
            Assert.EndsWith(".cbz", file.FilePath);
        });
    }

    [Fact]
    public async Task ScanFiles_ShouldIncludeMultipleTypes()
    {
        const string testcase = "Mixed Formats - Manga.json";
        var library = await _scannerHelper.GenerateScannerData(testcase);
        var folder = library.Folders.First().Path;

        var options = new ScannerOption
        {
            FolderPaths = [folder],
            FileTypePattern = [FileTypeGroup.Archive, FileTypeGroup.Epub],
            ExcludePatterns = []
        };

        var result = _fileScanner.ScanFiles(options);

        Assert.Single(result); // One folder
        var scanned = result[0];
        Assert.Equal(Parser.NormalizePath(Path.Join(folder, "My Dress-Up Darling")), scanned.DirectoryPath);
        var validExtensions = new[] { ".cbz", ".epub" };
        Assert.All(scanned.Files, file =>
        {
            Assert.Contains(Path.GetExtension(file.FilePath)?.ToLowerInvariant(), validExtensions);
        });
    }




    #endregion

    #region ScannFiles - Exclude Patterns


    [Fact]
    public async Task ScanFiles_ShouldExcludeMatchingPattern()
    {
        const string testcase = "Flat Series - Manga.json";
        var library = await _scannerHelper.GenerateScannerData(testcase);
        var folder = library.Folders.First().Path;

        var options = new ScannerOption
        {
            FolderPaths = [folder],
            FileTypePattern = [FileTypeGroup.Archive],
            ExcludePatterns = ["*ch 10.cbz"] // Exclude chapter 10
        };

        var result = _fileScanner.ScanFiles(options);

        var scannedFiles = result.SelectMany(d => d.Files).ToList();
        Assert.DoesNotContain(scannedFiles, f => f.FilePath.Contains("ch 10.cbz"));
        Assert.Contains(scannedFiles, f => f.FilePath.Contains("v01.cbz"));
        Assert.Contains(scannedFiles, f => f.FilePath.Contains("v02.cbz"));
    }

    #endregion

    #region ScannFiles - Change Detection

    [Fact]
    public async Task ScanFiles_ShouldHaveAccurateLastModifiedUtc()
    {
        const string testcase = "Flat Series - Manga.json";
        var library = await _scannerHelper.GenerateScannerData(testcase);
        var folder = library.Folders.First().Path;

        var options = new ScannerOption
        {
            FolderPaths = [folder],
            FileTypePattern = [FileTypeGroup.Archive],
            ExcludePatterns = []
        };

        var result = _fileScanner.ScanFiles(options);

        Assert.Single(result);
        var scannedDir = result[0];
        var file = scannedDir.Files[0];

        var expected = _directoryService.GetLastWriteTime(file.FilePath).ToUniversalTime();
        Assert.Equal(expected, file.LastModifiedUtc);
    }

    #endregion


    protected override async Task ResetDb()
    {
        Context.Series.RemoveRange(Context.Series);
        Context.Library.RemoveRange(Context.Library);
        await Context.SaveChangesAsync();
    }
}
