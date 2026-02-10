using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace ShinyMediatorMcp.Tools;

[McpServerToolType]
public static class MediatorDocsTool
{
    private static readonly string SubmodulePath = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "submodules", "mediator");

    private static readonly Lazy<string> SkillContent = new(() =>
        LoadFileContent(Path.Combine(SubmodulePath, "skills", "shiny-mediator", "SKILL.md")));

    private static readonly Lazy<string> ReadmeContent = new(() =>
        LoadFileContent(Path.Combine(SubmodulePath, "readme.md")));

    private static string LoadFileContent(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (File.Exists(fullPath))
        {
            return File.ReadAllText(fullPath);
        }
        return $"File not found: {fullPath}";
    }

    private static string GetAllDocumentation()
    {
        var sb = new StringBuilder();
        sb.AppendLine(SkillContent.Value);
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine(ReadmeContent.Value);
        return sb.ToString();
    }

    [McpServerTool]
    [Description("Get the complete Shiny.Mediator documentation from the local repository. Returns the full SKILL.md and readme.md content.")]
    public static string GetMediatorDocs(
        [Description("Optional: 'full' for all docs, 'skill' for SKILL.md only, 'readme' for readme.md only. Defaults to 'full'.")]
        string section = "full")
    {
        return section.ToLowerInvariant() switch
        {
            "skill" => SkillContent.Value,
            "readme" => ReadmeContent.Value,
            _ => GetAllDocumentation()
        };
    }

    [McpServerTool]
    [Description("List all available Shiny.Mediator documentation files from the local repository")]
    public static string ListMediatorTopics()
    {
        var files = new List<string>();
        var fullPath = Path.GetFullPath(SubmodulePath);

        if (Directory.Exists(fullPath))
        {
            var mdFiles = Directory.GetFiles(fullPath, "*.md", SearchOption.TopDirectoryOnly);
            foreach (var file in mdFiles)
            {
                var fileName = Path.GetFileName(file);
                var fileInfo = new FileInfo(file);
                files.Add($"- **{fileName}** ({fileInfo.Length / 1024.0:F1} KB)");
            }
        }

        return $@"# Available Shiny.Mediator Documentation

## Local Repository Files
Path: {fullPath}

{string.Join("\n", files)}

## Usage

Use `GetMediatorDocs(section)` with these options:
- `full` - Complete documentation (SKILL.md + readme.md)
- `skill` - Skill documentation only
- `readme` - README documentation only

Use `SearchMediatorDocs(searchTerm)` to search across all documentation.

## Links
- **GitHub**: https://github.com/shinyorg/mediator
- **Documentation**: https://shinylib.net/client/mediator/";
    }

    [McpServerTool]
    [Description("Search across all Shiny.Mediator documentation files for a specific term or concept")]
    public static string SearchMediatorDocs(
        [Description("The search term to find in the documentation")]
        string searchTerm)
    {
        var results = new List<(string Source, List<string> Matches)>();

        // Search in all documentation
        var sources = new Dictionary<string, string>
        {
            ["SKILL.md"] = SkillContent.Value,
            ["readme.md"] = ReadmeContent.Value
        };

        foreach (var (source, content) in sources)
        {
            if (content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            {
                var lines = content.Split('\n');
                var matchingLines = new List<string>();

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        // Get context: 1 line before and after
                        var contextLines = new List<string>();
                        if (i > 0) contextLines.Add($"  {lines[i - 1].Trim()}");
                        contextLines.Add($"→ {lines[i].Trim()}");
                        if (i < lines.Length - 1) contextLines.Add($"  {lines[i + 1].Trim()}");

                        matchingLines.Add($"Line {i + 1}:\n{string.Join("\n", contextLines)}");

                        // Limit matches per file
                        if (matchingLines.Count >= 5) break;
                    }
                }

                if (matchingLines.Count > 0)
                {
                    results.Add((source, matchingLines));
                }
            }
        }

        if (results.Count == 0)
        {
            return $"No results found for '{searchTerm}'. Try different search terms or use GetMediatorDocs() to browse the full documentation.";
        }

        var output = new StringBuilder();
        output.AppendLine($"# Search Results for '{searchTerm}'");
        output.AppendLine();
        output.AppendLine($"Found in {results.Count} file(s):");
        output.AppendLine();

        foreach (var (source, matches) in results)
        {
            output.AppendLine($"## {source}");
            output.AppendLine();
            foreach (var match in matches)
            {
                output.AppendLine(match);
                output.AppendLine();
            }
            output.AppendLine("---");
            output.AppendLine();
        }

        output.AppendLine("Use `GetMediatorDocs()` for complete documentation.");

        return output.ToString();
    }

    [McpServerTool]
    [Description("Get a quick code example for a specific Shiny.Mediator feature by searching the documentation")]
    public static string GetMediatorExample(
        [Description("The feature to get an example for: request, command, event, stream, caching, validation, http, middleware, offline, resilience")]
        string feature)
    {
        var searchTerms = feature.ToLowerInvariant() switch
        {
            "request" or "requests" => "IRequestHandler",
            "command" or "commands" => "ICommandHandler",
            "event" or "events" => "IEventHandler",
            "stream" or "streams" => "IStreamRequestHandler",
            "caching" or "cache" => "[Cache(",
            "validation" or "validate" => "[Validate]",
            "http" => "[Get(",
            "middleware" => "IRequestMiddleware",
            "offline" => "[OfflineAvailable]",
            "resilience" or "resilient" => "[Resilient(",
            _ => feature
        };

        var content = SkillContent.Value;
        var lines = content.Split('\n');

        var codeBlocks = new List<string>();
        var inCodeBlock = false;
        var currentBlock = new StringBuilder();
        var blockLanguage = "";

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (line.TrimStart().StartsWith("```"))
            {
                if (!inCodeBlock)
                {
                    inCodeBlock = true;
                    blockLanguage = line.Trim().Substring(3);
                    currentBlock.Clear();
                }
                else
                {
                    inCodeBlock = false;
                    var block = currentBlock.ToString();
                    if (block.Contains(searchTerms, StringComparison.OrdinalIgnoreCase) ||
                        block.Contains(feature, StringComparison.OrdinalIgnoreCase))
                    {
                        codeBlocks.Add($"```{blockLanguage}\n{block}```");
                        if (codeBlocks.Count >= 3) break;
                    }
                }
            }
            else if (inCodeBlock)
            {
                currentBlock.AppendLine(line);
            }
        }

        if (codeBlocks.Count == 0)
        {
            return $"No specific code examples found for '{feature}'. Try: request, command, event, stream, caching, validation, http, middleware, offline, resilience\n\nOr use SearchMediatorDocs(\"{feature}\") for a broader search.";
        }

        var output = new StringBuilder();
        output.AppendLine($"# {char.ToUpper(feature[0]) + feature[1..]} Examples");
        output.AppendLine();

        foreach (var block in codeBlocks)
        {
            output.AppendLine(block);
            output.AppendLine();
        }

        return output.ToString();
    }

    [McpServerTool]
    [Description("Read any source code file from the Shiny.Mediator repository")]
    public static string ReadMediatorSource(
        [Description("The relative path to the file within the mediator repository, e.g., 'src/Shiny.Mediator/IMediator.cs'")]
        string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(SubmodulePath, relativePath));

        // Security check: ensure the path is within the submodule
        var submoduleFullPath = Path.GetFullPath(SubmodulePath);
        if (!fullPath.StartsWith(submoduleFullPath))
        {
            return "Error: Path must be within the mediator submodule.";
        }

        if (!File.Exists(fullPath))
        {
            // Try to find similar files
            var directory = Path.GetDirectoryName(fullPath);
            var fileName = Path.GetFileName(fullPath);

            if (Directory.Exists(directory))
            {
                var similarFiles = Directory.GetFiles(directory, $"*{Path.GetFileNameWithoutExtension(fileName)}*")
                    .Take(5)
                    .Select(f => f.Replace(submoduleFullPath, "").TrimStart(Path.DirectorySeparatorChar));

                if (similarFiles.Any())
                {
                    return $"File not found: {relativePath}\n\nDid you mean one of these?\n{string.Join("\n", similarFiles.Select(f => $"- {f}"))}";
                }
            }

            return $"File not found: {relativePath}\n\nUse ListMediatorSourceFiles() to browse available files.";
        }

        var content = File.ReadAllText(fullPath);
        var extension = Path.GetExtension(fullPath).TrimStart('.');

        return $"# {relativePath}\n\n```{extension}\n{content}\n```";
    }

    [McpServerTool]
    [Description("List source code files in the Shiny.Mediator repository")]
    public static string ListMediatorSourceFiles(
        [Description("Optional: subdirectory to list, e.g., 'src/Shiny.Mediator'. Defaults to 'src'.")]
        string directory = "src",
        [Description("Optional: file extension filter, e.g., 'cs'. Defaults to all files.")]
        string? extension = null)
    {
        var fullPath = Path.GetFullPath(Path.Combine(SubmodulePath, directory));
        var submoduleFullPath = Path.GetFullPath(SubmodulePath);

        if (!fullPath.StartsWith(submoduleFullPath))
        {
            return "Error: Path must be within the mediator submodule.";
        }

        if (!Directory.Exists(fullPath))
        {
            return $"Directory not found: {directory}\n\nAvailable directories:\n" +
                string.Join("\n", Directory.GetDirectories(submoduleFullPath)
                    .Select(d => $"- {Path.GetFileName(d)}"));
        }

        var pattern = string.IsNullOrEmpty(extension) ? "*.*" : $"*.{extension}";
        var files = Directory.GetFiles(fullPath, pattern, SearchOption.AllDirectories)
            .Select(f => f.Replace(submoduleFullPath, "").TrimStart(Path.DirectorySeparatorChar))
            .OrderBy(f => f)
            .Take(100);

        var dirs = Directory.GetDirectories(fullPath)
            .Select(d => d.Replace(submoduleFullPath, "").TrimStart(Path.DirectorySeparatorChar))
            .OrderBy(d => d);

        var output = new StringBuilder();
        output.AppendLine($"# Files in {directory}");
        output.AppendLine();

        if (dirs.Any())
        {
            output.AppendLine("## Subdirectories");
            foreach (var dir in dirs)
            {
                output.AppendLine($"- 📁 {dir}");
            }
            output.AppendLine();
        }

        output.AppendLine("## Files");
        foreach (var file in files)
        {
            output.AppendLine($"- {file}");
        }

        if (files.Count() >= 100)
        {
            output.AppendLine();
            output.AppendLine("(Showing first 100 files. Use a more specific directory path to see more.)");
        }

        return output.ToString();
    }
}
