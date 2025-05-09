using API.Data.Metadata;
using API.Services.Tasks.Scanner.Parser;

namespace API.DTOs.Internal.Scanner;
#nullable enable

public sealed record ParsedFile
{
    public int Pages { get; set; }
    public ComicInfo? Metadata { get; set; }
    public ParserInfo? ParsedInformation { get; set; }
}
