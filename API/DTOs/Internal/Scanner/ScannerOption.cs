using System.Collections.Generic;
using API.Entities.Enums;

namespace API.DTOs.Internal.Scanner;

public sealed record ScannerOption
{
    /// <summary>
    /// A list of File Type Patterns to search files for. If empty, scan will abort
    /// </summary>
    public List<FileTypeGroup> FileTypePattern { get; set; } = [FileTypeGroup.Archive, FileTypeGroup.Epub, FileTypeGroup.Images, FileTypeGroup.Pdf];
    /// <summary>
    /// Folders to scan
    /// </summary>
    public List<string> FolderPaths { get; set; }

    /// <summary>
    /// Glob syntax to exclude from scan results
    /// </summary>
    public List<string> ExcludePatterns { get; set; } = [];
    /// <summary>
    /// Skip LastModified checks
    /// </summary>
    public bool ForceScan { get; set; }
    /// <summary>
    /// Allow use of Filename Parsing
    /// </summary>
    public bool UseFilenameParsing { get; set; }
    /// <summary>
    /// Allow use of Internal Metadata
    /// </summary>
    public bool UseInternalMetadataParsing { get; set; }
}
