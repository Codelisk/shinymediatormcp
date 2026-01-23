# Shiny Mediator MCP Server

Ein Model Context Protocol (MCP) Server, der Dokumentation und Code-Beispiele für [Shiny.Mediator](https://github.com/shinyorg/mediator) bereitstellt.

## Features

- **GetMediatorDocs** - Detaillierte Dokumentation zu spezifischen Themen abrufen
- **ListMediatorTopics** - Alle verfügbaren Dokumentationsthemen auflisten
- **SearchMediatorDocs** - Suche über die gesamte Dokumentation
- **GetMediatorExample** - Schnelle Code-Beispiele für Features

## Verfügbare Dokumentationsthemen

| Kategorie | Themen |
|-----------|--------|
| Core | `overview`, `getting-started` |
| Contracts | `requests`, `commands`, `events`, `streams` |
| Middleware | `middleware`, `caching`, `offline`, `resilience`, `validation`, `http` |
| Advanced | `context`, `source-generation`, `exception-handlers`, `advanced` |

## Installation & Konfiguration

### Voraussetzungen

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) oder höher

### Build

```bash
cd /home/orderlyze/dev/shinymediatormcp
dotnet build
```

---

## Claude Code

### Per CLI-Befehl (empfohlen)

```bash
claude mcp add shiny-mediator --scope user -- dotnet run --project /home/orderlyze/dev/shinymediatormcp/ShinyMediatorMcp.csproj
```

### Per JSON-Befehl

```bash
claude mcp add-json shiny-mediator '{"command":"dotnet","args":["run","--project","/home/orderlyze/dev/shinymediatormcp/ShinyMediatorMcp.csproj"]}'
```

### Manuell in Konfigurationsdatei

Füge zu `~/.claude/mcp.json` hinzu:

```json
{
  "mcpServers": {
    "shiny-mediator": {
      "command": "dotnet",
      "args": ["run", "--project", "/home/orderlyze/dev/shinymediatormcp/ShinyMediatorMcp.csproj"]
    }
  }
}
```

### Verifizieren

```bash
claude mcp list
```

Oder innerhalb von Claude Code:
```
/mcp
```

---

## OpenAI Codex CLI

### Per CLI-Befehl

```bash
codex mcp add shiny-mediator -- dotnet run --project /home/orderlyze/dev/shinymediatormcp/ShinyMediatorMcp.csproj
```

### Manuell in Konfigurationsdatei

Füge zu `~/.codex/config.toml` hinzu:

```toml
[mcp_servers.shiny-mediator]
command = "dotnet"
args = ["run", "--project", "/home/orderlyze/dev/shinymediatormcp/ShinyMediatorMcp.csproj"]
```

---

## Gemini CLI

### Manuell in Konfigurationsdatei

Füge zu `~/.gemini/settings.json` hinzu:

```json
{
  "mcpServers": {
    "shiny-mediator": {
      "command": "dotnet",
      "args": ["run", "--project", "/home/orderlyze/dev/shinymediatormcp/ShinyMediatorMcp.csproj"]
    }
  }
}
```

Für projektspezifische Konfiguration verwende `.gemini/settings.json` im Projektverzeichnis.

---

## Verwendung

Nach der Installation stehen folgende Tools zur Verfügung:

### Dokumentation abrufen
```
GetMediatorDocs("requests")
GetMediatorDocs("caching")
GetMediatorDocs("getting-started")
```

### Themen auflisten
```
ListMediatorTopics()
```

### Suchen
```
SearchMediatorDocs("IRequest")
SearchMediatorDocs("middleware")
```

### Code-Beispiele
```
GetMediatorExample("request")
GetMediatorExample("command")
GetMediatorExample("validation")
```

## Projektstruktur

```
shinymediatormcp/
├── ShinyMediatorMcp.csproj    # Projektdatei
├── Program.cs                  # MCP Server Einstiegspunkt
├── server.json                 # MCP Server Metadaten
├── README.md                   # Diese Datei
└── Tools/
    └── MediatorDocsTool.cs    # MCP Tools Implementation
```

## Quellen

- [Shiny.Mediator GitHub](https://github.com/shinyorg/mediator)
- [Shiny.Mediator Dokumentation](https://shinylib.net/client/mediator/)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Claude Code MCP Docs](https://code.claude.com/docs/en/mcp)
- [Codex CLI MCP Configuration](https://developers.openai.com/codex/mcp/)
- [Gemini CLI MCP Servers](https://geminicli.com/docs/tools/mcp-server/)

## Lizenz

MIT
