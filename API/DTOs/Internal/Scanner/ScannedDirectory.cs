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

    /// <summary>
    /// Root where the directory resides
    /// </summary>
    /// <remarks>Library Root</remarks>
    public required string FolderRoot { get => _folderRoot; set => _folderRoot = Parser.NormalizePath(value); }
    private string _folderRoot;

    public required DateTime LastModifiedUtc { get; init; }

    public List<ScannedFile> Files { get; init; } = [];
}
