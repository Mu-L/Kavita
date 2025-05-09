using System;
using API.Entities.Enums;
using API.Services.Tasks.Scanner.Parser;

namespace API.DTOs.Internal.Scanner;

public sealed record ScannedFile
{
    public required string FilePath { get => _filePath; set => _filePath = Parser.NormalizePath(value); }
    private string _filePath;

    public required DateTime LastModifiedUtc { get; set; }
    public required string FolderRoot { get; set; }
    public required MangaFormat Format { get; set; }
}
