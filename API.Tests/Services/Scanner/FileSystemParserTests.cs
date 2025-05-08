using System.IO;
using System.Threading.Tasks;
using API.Data.Repositories;
using API.Tests.Helpers;
using Hangfire;
using Xunit;
using Xunit.Abstractions;

namespace API.Tests.Services.Scanner;

/// <summary>
/// Responsible for testing Change Detection, Exclude Patterns,
/// </summary>
public class FileSystemParserTests : AbstractDbTest
{

    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ScannerHelper _scannerHelper;
    private readonly string _testDirectory = Path.Join(Directory.GetCurrentDirectory(), "../../../Services/Test Data/ScannerService/ScanTests");

    public FileSystemParserTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        // Set up Hangfire to use in-memory storage for testing
        GlobalConfiguration.Configuration.UseInMemoryStorage();
        _scannerHelper = new ScannerHelper(UnitOfWork, testOutputHelper);
    }

    protected override async Task ResetDb()
    {
        Context.Library.RemoveRange(Context.Library);
        await Context.SaveChangesAsync();
    }


    #region Validate Change Detection


    [Fact]
    public async Task ScanLibrary_ComicVine_PublisherFolder()
    {
        var testcase = "Publisher - ComicVine.json";
        var library = await _scannerHelper.GenerateScannerData(testcase);
        var scanner = _scannerHelper.CreateServices();
        await scanner.ScanLibrary(library.Id);
        var postLib = await UnitOfWork.LibraryRepository.GetLibraryForIdAsync(library.Id, LibraryIncludes.Series);

        Assert.NotNull(postLib);
        Assert.Equal(4, postLib.Series.Count);
    }

    #endregion
}
