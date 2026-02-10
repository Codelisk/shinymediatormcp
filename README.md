# Shiny Mediator MCP Server

Ein Model Context Protocol (MCP) Server, der Dokumentation und Code-Beispiele für [Shiny.Mediator](https://github.com/shinyorg/mediator) bereitstellt.

## Features

- **GetMediatorDocs** - Dokumentation aus dem lokalen Repository abrufen (SKILL.md, readme.md)
- **ListMediatorTopics** - Alle verfügbaren Dokumentationsdateien auflisten
- **SearchMediatorDocs** - Suche über die gesamte Dokumentation
- **GetMediatorExample** - Code-Beispiele für Features aus der Dokumentation extrahieren
- **ReadMediatorSource** - Quellcode-Dateien aus dem Mediator-Repository lesen
- **ListMediatorSourceFiles** - Quellcode-Dateien im Repository durchsuchen

## Architektur

Das Projekt verwendet das originale [shinyorg/mediator](https://github.com/shinyorg/mediator) Repository als Git-Submodule und liest die Dokumentation und den Quellcode direkt aus den lokalen Dateien.

```
shinymediatormcp/
├── ShinyMediatorMcp.csproj    # Projektdatei
├── Program.cs                  # MCP Server Einstiegspunkt
├── server.json                 # MCP Server Metadaten
├── README.md                   # Diese Datei
├── Tools/
│   └── MediatorDocsTool.cs    # MCP Tools Implementation
└── submodules/
    └── mediator/              # Git Submodule (shinyorg/mediator)
        ├── readme.md          # Repository README
        ├── skills/
        │   └── shiny-mediator/
        │       └── SKILL.md   # Skill Dokumentation
        └── src/               # Quellcode
```

## Installation & Konfiguration

### Voraussetzungen

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) oder höher
- Git (für Submodule)

### Clone & Build

```bash
# Repository klonen mit Submodules
git clone --recurse-submodules https://github.com/your-repo/shinymediatormcp.git
cd shinymediatormcp

# Oder bei bestehendem Clone die Submodules initialisieren
git submodule update --init --recursive

# Build
dotnet build
```

### Submodule aktualisieren

```bash
# Auf neueste Version des Mediator-Repos aktualisieren
cd submodules/mediator
git pull origin main
cd ../..
git add submodules/mediator
git commit -m "Update mediator submodule"
```

---

> **Hinweis:** Ersetze `<PFAD>` in allen Beispielen mit dem absoluten Pfad zu deinem geklonten Repository.

## Claude Code

### Per CLI-Befehl (empfohlen)

```bash
claude mcp add shiny-mediator --scope user -- dotnet run --project <PFAD>/ShinyMediatorMcp.csproj
```

### Per JSON-Befehl

```bash
claude mcp add-json shiny-mediator '{"command":"dotnet","args":["run","--project","<PFAD>/ShinyMediatorMcp.csproj"]}'
```

### Manuell in Konfigurationsdatei

Füge zu `~/.claude/mcp.json` hinzu:

```json
{
  "mcpServers": {
    "shiny-mediator": {
      "command": "dotnet",
      "args": ["run", "--project", "<PFAD>/ShinyMediatorMcp.csproj"]
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
codex mcp add shiny-mediator -- dotnet run --project <PFAD>/ShinyMediatorMcp.csproj
```

### Manuell in Konfigurationsdatei

Füge zu `~/.codex/config.toml` hinzu:

```toml
[mcp_servers.shiny-mediator]
command = "dotnet"
args = ["run", "--project", "<PFAD>/ShinyMediatorMcp.csproj"]
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
      "args": ["run", "--project", "<PFAD>/ShinyMediatorMcp.csproj"]
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
GetMediatorDocs("full")      # Komplette Dokumentation
GetMediatorDocs("skill")     # Nur SKILL.md
GetMediatorDocs("readme")    # Nur readme.md
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

### Quellcode lesen
```
ReadMediatorSource("src/Shiny.Mediator/IMediator.cs")
ListMediatorSourceFiles("src/Shiny.Mediator", "cs")
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
