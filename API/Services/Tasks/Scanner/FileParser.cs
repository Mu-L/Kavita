using System;
using API.Data.Metadata;
using API.DTOs.Internal.Scanner;
using API.Entities.Enums;
using API.Services.Tasks.Scanner.Parser;
using Microsoft.Extensions.Logging;

namespace API.Services.Tasks.Scanner;
#nullable enable

public interface IFileParser
{
    ParsedFile? Parse(ScannedFile file);
}

public class FileParser : IFileParser
{
    private readonly IArchiveService _archiveService;
    private readonly IBookService _bookService;
    private readonly IImageService _imageService;
    private readonly ILogger<FileParser> _logger;
    private readonly BasicParser _basicParser;
    private readonly ComicVineParser _comicVineParser;
    private readonly ImageParser _imageParser;
    private readonly BookParser _bookParser;
    private readonly PdfParser _pdfParser;

    public FileParser(IArchiveService archiveService, IDirectoryService directoryService,
        IBookService bookService, IImageService imageService, ILogger<FileParser> logger)
    {
        _archiveService = archiveService;
        _bookService = bookService;
        _imageService = imageService;
        _logger = logger;

        _imageParser = new ImageParser(directoryService);
        _basicParser = new BasicParser(directoryService, _imageParser);
        _bookParser = new BookParser(directoryService, bookService, _basicParser);
        _comicVineParser = new ComicVineParser(directoryService);
        _pdfParser = new PdfParser(directoryService);
    }




    /// <summary>
    /// Processes files found during a library scan.
    /// </summary>
    /// <param name="path">Path of a file</param>
    /// <param name="rootPath"></param>
    /// <param name="type">Library type to determine parsing to perform</param>
    // public ParserInfo? ParseFile(string path, string rootPath, string libraryRoot, LibraryType type)
    // {
    //     try
    //     {
    //         var info = Parse(path, rootPath, libraryRoot, type);
    //         if (info == null)
    //         {
    //             _logger.LogError("Unable to parse any meaningful information out of file {FilePath}", path);
    //             return null;
    //         }
    //
    //         return info;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "There was an exception when parsing file {FilePath}", path);
    //         return null;
    //     }
    // }


    public ParsedFile? Parse(ScannedFile file, string folderRoot, LibraryType type)
    {
        var path = file.FilePath;
        var rootPath = file.FolderRoot;

        ParserInfo? parserInfo = null;
        if (_comicVineParser.IsApplicable(path, type))
        {
            parserInfo = _comicVineParser.Parse(path, rootPath, folderRoot, type, GetComicInfo(path));
        }
        if (_imageParser.IsApplicable(path, type))
        {
            parserInfo = _imageParser.Parse(path, rootPath, folderRoot, type, GetComicInfo(path));
        }
        if (_bookParser.IsApplicable(path, type))
        {
            parserInfo = _bookParser.Parse(path, rootPath, folderRoot, type, GetComicInfo(path));
        }
        if (_pdfParser.IsApplicable(path, type))
        {
            parserInfo = _pdfParser.Parse(path, rootPath, folderRoot, type, GetComicInfo(path));
        }
        if (_basicParser.IsApplicable(path, type))
        {
            parserInfo = _basicParser.Parse(path, rootPath, folderRoot, type, GetComicInfo(path));
        }

        if (parserInfo == null) return null;

        return null;
    }


    /// <summary>
    /// Gets the ComicInfo for the file if it exists. Null otherwise.
    /// </summary>
    /// <param name="filePath">Fully qualified path of file</param>
    /// <returns></returns>
    private ComicInfo? GetComicInfo(string filePath)
    {
        if (Parser.Parser.IsEpub(filePath) || Parser.Parser.IsPdf(filePath))
        {
            return _bookService.GetComicInfo(filePath);
        }

        if (Parser.Parser.IsComicInfoExtension(filePath))
        {
            return _archiveService.GetComicInfo(filePath);
        }

        return null;
    }
}
