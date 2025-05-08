using System;
using System.Collections.Generic;
using API.Entities.Enums;
using API.Services.Tasks.Scanner.Parser;

namespace API.DTOs.Internal.Scanner;

/// <summary>
/// Represents a Directory on disk and metadata information for the Scan
/// </summary>
public sealed record ScannedDirectory
{
    /// <summary>
    /// Normalized Directory Path
    /// </summary>
    public required string DirectoryPath { get => _directoryPath; set => _directoryPath = Parser.NormalizePath(value); }
    private string _directoryPath;

    public required DateTime LastModifiedUtc { get; set; }

    public List<ScannedFile> Files { get; set; } = [];
}
