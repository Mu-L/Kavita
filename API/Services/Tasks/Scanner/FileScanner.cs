using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Data.Repositories;
using API.DTOs.Internal.Scanner;
using API.Entities.Enums;
using API.Extensions;
using Kavita.Common.Helpers;

namespace API.Services.Tasks.Scanner;

public interface IFileScanner
{
    // TODO: Move this to the scanner service
    //Task ScanLibrary(int libraryId, bool forceScan = false);
    List<ScannedDirectory> ScanFiles(ScannerOption options);
}


public class FileScanner : IFileScanner
{
    private readonly IDirectoryService _directoryService;
    private readonly IUnitOfWork _unitOfWork;

    public FileScanner(IDirectoryService directoryService, IUnitOfWork unitOfWork)
    {
        _directoryService = directoryService;
        _unitOfWork = unitOfWork;
    }


    // public async Task ScanLibrary(int libraryId, bool forceScan = false)
    // {
    //     var library = await _unitOfWork.LibraryRepository.GetLibraryForIdAsync(libraryId,
    //         LibraryIncludes.Folders | LibraryIncludes.ExcludePatterns | LibraryIncludes.FileTypes);
    //
    //     if (library == null)
    //     {
    //         return;
    //     }
    //
    //     // Create a ScannerOption
    //     var options = new ScannerOption()
    //     {
    //         FileTypePattern = library.LibraryFileTypes.Select(s => s.FileTypeGroup).ToList(),
    //         ForceScan = forceScan,
    //         ExcludePatterns = [.. library.LibraryExcludePatterns.Select(s => s.Pattern)],
    //         FolderPaths = [.. library.Folders.Select(f => Parser.Parser.NormalizePath(f.Path))]
    //     };
    //
    //
    //     // Find all the information about the directories and their files
    //     var files = ScanFiles(options);
    //
    //     // Parse said information
    //
    //
    //     return;
    // }

    public List<ScannedDirectory> ScanFiles(ScannerOption options)
    {
        // Validate input options
        if (options == null || options.FolderPaths.Count == 0 || options.FileTypePattern.Count == 0)
        {
            return [];
        }

        // Build the file extensions regex from the file type patterns
        var fileExtensions = string.Join("|", options.FileTypePattern.Select(l => l.GetRegex()));
        if (string.IsNullOrWhiteSpace(fileExtensions))
        {
            return [];
        }


        var matcher = BuildMatcher(options.ExcludePatterns);
        var scannedDirectories = new List<ScannedDirectory>();

        foreach (var folderPath in options.FolderPaths)
        {
            var normalizedFolderPath = Parser.Parser.NormalizePath(folderPath);

            var allDirectories = _directoryService.GetAllDirectories(normalizedFolderPath, matcher)
                .Select(Parser.Parser.NormalizePath)
                .OrderByDescending(d => d.Length)
                .ToList();

            // TODO: Optimization: If allDirectories is large, split into Parallel tasks

            foreach (var directory in allDirectories)
            {
                var files = _directoryService.ScanFiles(directory, fileExtensions, matcher)
                    .Select(filePath =>
                    {
                        // Gather metadata for each file
                        var lastModifiedUtc = _directoryService.GetLastWriteTime(filePath).ToUniversalTime();
                        var format = Parser.Parser.ParseFormat(filePath);
                        return new ScannedFile
                        {
                            FilePath = filePath,
                            LastModifiedUtc = lastModifiedUtc,
                            Format = format
                        };
                    })
                    .ToList();

                // Skip directories with no valid files
                if (files.Count == 0)
                {
                    continue;
                }

                // Get directory's metadata (TODO: Replace with _directoryService.GetLastWriteTime(folder).Truncate(TimeSpan.TicksPerSecond);)
                //var directoryLastModifiedUtc = files.Max(f => f.LastModifiedUtc);
                var directoryLastModifiedUtc = _directoryService.GetLastWriteTime(normalizedFolderPath).Truncate(TimeSpan.TicksPerSecond);

                // Add the directory and its files to the result
                scannedDirectories.Add(new ScannedDirectory
                {
                    FolderRoot = folderPath,
                    DirectoryPath = directory,
                    LastModifiedUtc = directoryLastModifiedUtc,
                    Files = files
                });
            }
        }

        return scannedDirectories;
    }



    private static GlobMatcher BuildMatcher(List<string> excludePatterns)
    {
        var matcher = new GlobMatcher();
        foreach (var pattern in excludePatterns.Where(p => !string.IsNullOrEmpty(p)))
        {
            matcher.AddExclude(pattern);
        }

        return matcher;
    }
}
