using System.ComponentModel;
using ModelContextProtocol.Server;

namespace ShinyMediatorMcp.Tools;

[McpServerToolType]
public static class MediatorDocsTool
{
    private static readonly Dictionary<string, string> Documentation = new()
    {
        ["overview"] = @"# Shiny.Mediator Overview

Shiny.Mediator is a .NET implementation of the mediator design pattern that reduces dependencies between objects.

## Four Contract Types

1. **Commands** (`ICommand`) - Fire-and-forget messages to single handlers
2. **Requests** (`IRequest<Result>`) - Messages expecting a response from one handler
3. **Streams** (`IStreamRequest<TResult>`) - Messages returning async enumerables
4. **Events** (`IEvent`) - Messages broadcast to multiple handlers

## Key Problems Addressed

- **Service Injection Bloat**: Consolidates dependencies into a single mediator
- **Unsafe Messaging**: Includes pipeline support, automatic error handling
- **Type-Safe Navigation**: Strongly-typed navigation requests

## NuGet Packages

- `Shiny.Mediator` - Core package
- `Shiny.Mediator.Maui` - MAUI extensions
- `Shiny.Mediator.Blazor` - Blazor extensions
- `Shiny.Mediator.AspNet` - ASP.NET extensions

GitHub: https://github.com/shinyorg/mediator
Documentation: https://shinylib.net/client/mediator/",

        ["getting-started"] = @"# Getting Started with Shiny.Mediator

## MAUI Setup

### 1. Install NuGet Package
```bash
dotnet add package Shiny.Mediator.Maui
```

### 2. Define a Request
```csharp
public record MyRequest(string Argument) : IRequest<MyResponse>;
public record MyResponse(string Result);
```

### 3. Create a Handler
```csharp
[MediatorSingleton]
public partial class MyRequestHandler : IRequestHandler<MyRequest, MyResponse>
{
    public async Task<MyResponse> Handle(MyRequest request, IMediatorContext context, CancellationToken ct)
    {
        return new MyResponse($""Processed: {request.Argument}"");
    }
}
```

### 4. Register in MauiProgram.cs
```csharp
builder.Services.AddShinyMediator(cfg =>
{
    cfg.UseMaui();
    cfg.AddMediatorRegistry();
});
```

### 5. Use in ViewModel
```csharp
public class MyViewModel(IMediator mediator)
{
    public async Task DoSomething()
    {
        var response = await mediator.Request(new MyRequest(""Hello""));
    }
}
```

## Blazor Setup

```csharp
builder.Services.AddShinyMediator(cfg =>
{
    cfg.UseBlazor();
    cfg.AddMediatorRegistry();
});
```

Add to your HTML:
```html
<script src=""_content/Shiny.Mediator.Blazor/Mediator.js""></script>
```",

        ["requests"] = @"# Requests in Shiny.Mediator

Requests are the center of all action in the Mediator. A request can only be fulfilled by a single handler.

## Creating a Request

```csharp
public record MyRequest(string Argument) : IRequest<MyResponse>;
public record MyResponse(string Data);
```

## Creating a Handler

```csharp
[MediatorSingleton]
public partial class MyRequestHandler : IRequestHandler<MyRequest, MyResponse>
{
    public async Task<MyResponse> Handle(
        MyRequest request,
        IMediatorContext context,
        CancellationToken cancellationToken)
    {
        // Your logic here
        return new MyResponse(""Result"");
    }
}
```

## Registration Options

1. **Source Generation (Recommended)**
```csharp
[MediatorSingleton]
public partial class MyHandler : IRequestHandler<MyRequest, MyResponse> { }
```

2. **Manual Registration**
```csharp
services.AddSingleton<IRequestHandler<MyRequest, MyResponse>, MyHandler>();
```

3. **Extension Method**
```csharp
services.AddSingletonAsImplementedInterfaces<MyHandler>();
```

## Sending Requests

```csharp
var response = await mediator.Request(new MyRequest(""arg""));
```

## With Context

```csharp
await mediator.RequestWithContext(new MyRequest(""arg""), (result, context) =>
{
    // Access context values set by middleware
    var cacheInfo = context.Cache();
});
```

## Important Notes

- Scoped dependencies are disposed after request completion
- If the result implements `IEvent`, it's automatically published",

        ["commands"] = @"# Commands in Shiny.Mediator

Commands are fire-and-forget messages without return values. They support deferred or scheduled execution.

## Creating a Command

```csharp
public record MyCommand(string Argument) : ICommand;
```

## Creating a Handler

```csharp
[MediatorSingleton]
public partial class MyCommandHandler : ICommandHandler<MyCommand>
{
    public async Task Handle(
        MyCommand command,
        IMediatorContext context,
        CancellationToken cancellationToken)
    {
        // Execute command logic
    }
}
```

## Sending Commands

```csharp
var context = await mediator.Send(new MyCommand(""arg""));
```

## Command Scheduling

### Via Context
```csharp
await mediator.Send(new MyCommand(""arg""), ctx =>
{
    ctx.SetCommandSchedule(DateTimeOffset.Now.AddMinutes(10));
});
```

### Via Contract
```csharp
public record ScheduledCommand(string Arg) : IScheduledCommand
{
    public DateTimeOffset? DueAt => DateTimeOffset.Now.AddHours(1);
}
```

### Registration
```csharp
services.AddShinyMediator(cfg =>
{
    cfg.AddInMemoryCommandScheduling();
});
```

## Middleware

```csharp
public class LoggingMiddleware<TCommand> : ICommandMiddleware<TCommand>
    where TCommand : ICommand
{
    public async Task Process(
        IMediatorContext context,
        CommandHandlerDelegate next,
        CancellationToken ct)
    {
        Console.WriteLine($""Before: {typeof(TCommand).Name}"");
        await next();
        Console.WriteLine($""After: {typeof(TCommand).Name}"");
    }
}
```",

        ["events"] = @"# Events in Shiny.Mediator

Events notify multiple parts of the application that something has happened. Unlike requests, events don't return values.

## Creating an Event

```csharp
public record UserLoggedInEvent(string UserId) : IEvent;
```

## Creating Handlers

```csharp
[MediatorSingleton]
public partial class LoggingEventHandler : IEventHandler<UserLoggedInEvent>
{
    public async Task Handle(
        UserLoggedInEvent @event,
        IMediatorContext context,
        CancellationToken ct)
    {
        Console.WriteLine($""User logged in: {@event.UserId}"");
    }
}
```

## Publishing Events

```csharp
// Await all handlers
await mediator.Publish(new UserLoggedInEvent(""user123""));

// Fire-and-forget
mediator.Publish(new UserLoggedInEvent(""user123"")).RunInBackground(ex =>
{
    // Handle errors
});
```

## MAUI ViewModel Events

ViewModels can implement `IEventHandler<T>` directly:

```csharp
public class MyViewModel : IEventHandler<UserLoggedInEvent>
{
    public Task Handle(UserLoggedInEvent @event, IMediatorContext context, CancellationToken ct)
    {
        // React to event
        return Task.CompletedTask;
    }
}
```

Configuration:
```csharp
builder.Services.AddShinyMediator(cfg => cfg.UseMaui());
```

## Blazor Components

```csharp
@implements IEventHandler<UserLoggedInEvent>

@code {
    public Task Handle(UserLoggedInEvent @event, IMediatorContext context, CancellationToken ct)
    {
        // Update component state
        StateHasChanged();
        return Task.CompletedTask;
    }
}
```

## Important Notes

- Events are published in parallel using `Task.WhenAll`
- Order of handler execution is not guaranteed
- Failed handlers don't block other handlers",

        ["streams"] = @"# Stream Requests in Shiny.Mediator

Stream Request Handlers return `IAsyncEnumerable` for continuous data transmission.

## Creating a Stream Request

```csharp
public record DataStreamRequest(string Query) : IStreamRequest<DataItem>;
public record DataItem(int Id, string Value);
```

## Creating a Handler

```csharp
[MediatorSingleton]
public partial class DataStreamHandler : IStreamRequestHandler<DataStreamRequest, DataItem>
{
    public async IAsyncEnumerable<DataItem> Handle(
        DataStreamRequest request,
        IMediatorContext context,
        [EnumeratorCancellation] CancellationToken ct)
    {
        for (int i = 0; i < 100; i++)
        {
            if (ct.IsCancellationRequested) yield break;

            yield return new DataItem(i, $""{request.Query}-{i}"");
            await Task.Delay(100, ct);
        }
    }
}
```

## Consuming Streams

```csharp
var stream = await mediator.Request(new DataStreamRequest(""search""));

await foreach (var item in stream.Result)
{
    Console.WriteLine($""Received: {item.Value}"");
}
```

## Stream Middleware

Middleware processes each yielded item:

```csharp
public class UppercaseMiddleware<TRequest, TResult>
    : IStreamRequestMiddleware<TRequest, TResult>
    where TRequest : IStreamRequest<TResult>
    where TResult : class
{
    public async IAsyncEnumerable<TResult> Process(
        TRequest request,
        IMediatorContext context,
        StreamRequestHandlerDelegate<TResult> next,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var item in next())
        {
            // Transform each item
            yield return item;
        }
    }
}
```",

        ["middleware"] = @"# Middleware in Shiny.Mediator

Middleware provides before/after hooks for cross-cutting concerns like logging, caching, and error handling.

## Configuration

Configuration uses `appsettings.json`:

```json
{
  ""Mediator"": {
    ""Cache"": {
      ""MyNamespace.MyRequest"": {
        ""AbsoluteExpirationSeconds"": 60
      }
    },
    ""Resilience"": {
      ""MyNamespace.*"": {
        ""TimeoutSeconds"": 30,
        ""RetryCount"": 3
      }
    }
  }
}
```

## Resolution Priority

1. Full contract type name
2. Full handler type name
3. Contract namespace (`namespace.*`)
4. Handler namespace (`namespace.*`)
5. Global settings (`*`)

## Creating Custom Middleware

### Request Middleware

```csharp
[MediatorSingleton]
public partial class TimingMiddleware<TRequest, TResult>
    : IRequestMiddleware<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    public async Task<TResult> Process(
        TRequest request,
        IMediatorContext context,
        RequestHandlerDelegate<TResult> next,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return await next();
        }
        finally
        {
            Console.WriteLine($""{typeof(TRequest).Name} took {sw.ElapsedMilliseconds}ms"");
        }
    }
}
```

### Registration

```csharp
// Open generic
services.AddSingleton(typeof(IRequestMiddleware<,>), typeof(TimingMiddleware<,>));

// Or via extension
services.AddOpenRequestMiddleware(typeof(TimingMiddleware<,>));
```

## Bypassing Middleware

```csharp
await mediator.Request(new MyRequest(), ctx =>
{
    ctx.BypassMiddlewareEnabled = true;
});
```",

        ["caching"] = @"# Caching Middleware

Caching is built on `Microsoft.Extensions.Caching.Memory`.

## Setup

```bash
dotnet add package Shiny.Mediator.Caching.MicrosoftMemoryCache
```

```csharp
services.AddShinyMediator(cfg =>
{
    cfg.AddMemoryCaching();
});
```

## Usage via Attribute

```csharp
[MediatorSingleton]
[Cache(AbsoluteExpirationSeconds = 60, SlidingExpirationSeconds = 30)]
public partial class CachedHandler : IRequestHandler<MyRequest, MyResponse>
{
    public async Task<MyResponse> Handle(MyRequest request, IMediatorContext context, CancellationToken ct)
    {
        // Expensive operation
        return new MyResponse();
    }
}
```

## Runtime Control

```csharp
// Force cache refresh
await mediator.Request(new MyRequest(), ctx =>
{
    ctx.ForceCacheRefresh();
});

// Custom cache config
await mediator.Request(new MyRequest(), ctx =>
{
    ctx.SetCacheConfig(new CacheConfig
    {
        AbsoluteExpiration = TimeSpan.FromMinutes(5)
    });
});
```

## Contract Keys

For requests with parameters:

```csharp
[ContractKey(""{Id}-{Name}"")]
public partial record MyRequest(int Id, string Name) : IRequest<MyResponse>;
```

## Check Cache Status

```csharp
await mediator.RequestWithContext(new MyRequest(), (result, context) =>
{
    var cacheInfo = context.Cache();
    if (cacheInfo?.IsFromCache == true)
    {
        Console.WriteLine($""From cache, stored at: {cacheInfo.Timestamp}"");
    }
});
```

## Persistent Caching (MAUI)

```csharp
cfg.AddFileCaching(); // Survives app restarts
```",

        ["offline"] = @"# Offline Availability Middleware

Automatically caches responses for offline access in mobile/field applications.

## Setup

Included by default with `UseMaui()` and `UseBlazor()`.

## Usage

```csharp
[MediatorSingleton]
[OfflineAvailable]
public partial class OfflineHandler : IRequestHandler<DataRequest, DataResponse>
{
    public async Task<DataResponse> Handle(DataRequest request, IMediatorContext context, CancellationToken ct)
    {
        // Network call - cached when successful
        return await httpClient.GetFromJsonAsync<DataResponse>(""api/data"");
    }
}
```

## Custom Keys

```csharp
public record DataRequest(int Id, string Filter) : IRequest<DataResponse>, IRequestKey
{
    public string GetKey() => $""data-{Id}-{Filter}"";
}
```

## Check Offline Status

```csharp
await mediator.RequestWithContext(new DataRequest(1, ""filter""), (result, context) =>
{
    var offlineInfo = context.Offline();
    if (offlineInfo?.IsFromOfflineStore == true)
    {
        Console.WriteLine($""Using cached data from: {offlineInfo.Timestamp}"");
    }
});
```

## Important Notes

- Unlike caching, offline middleware always calls the handler when online
- Only returns cached data when connectivity is unavailable
- Data persists across sessions",

        ["resilience"] = @"# Resilience Middleware

Built on `Microsoft.Extensions.Resilience` for retry policies and circuit breakers.

## Setup

```bash
dotnet add package Shiny.Mediator.Resilience
```

```csharp
services.AddShinyMediator(cfg =>
{
    cfg.AddResilience();
});
```

## Usage via Attribute

```csharp
[MediatorSingleton]
[Resilient(""api-policy"")]
public partial class ApiHandler : IRequestHandler<ApiRequest, ApiResponse>
{
    public async Task<ApiResponse> Handle(ApiRequest request, IMediatorContext context, CancellationToken ct)
    {
        // Unreliable API call
        return await httpClient.GetFromJsonAsync<ApiResponse>(""api/data"");
    }
}
```

## Configuration

```json
{
  ""Mediator"": {
    ""Resilience"": {
      ""MyNamespace.ApiRequest"": {
        ""TimeoutSeconds"": 30,
        ""RetryCount"": 3,
        ""UseJitter"": true,
        ""DelayMilliseconds"": 500,
        ""BackoffType"": ""Exponential""
      }
    }
  }
}
```

## Code-Based Configuration

```csharp
cfg.AddResilience(resilienceBuilder =>
{
    resilienceBuilder.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential
    });

    resilienceBuilder.AddTimeout(TimeSpan.FromSeconds(30));
});
```

## Warning

Avoid broad wildcards in configuration. Be explicit about which handlers need resilience.",

        ["validation"] = @"# Validation Middleware

Enforces request contracts before handler execution.

## Data Annotations

```csharp
services.AddShinyMediator(cfg =>
{
    cfg.AddDataAnnotations();
});

[Validate]
public record CreateUserRequest(
    [Required] string Name,
    [EmailAddress] string Email,
    [Range(18, 120)] int Age
) : IRequest<User>;
```

## Fluent Validation

```bash
dotnet add package Shiny.Mediator.FluentValidation
```

```csharp
[Validate]
public record CreateUserRequest(string Name, string Email) : IRequest<User>;

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```

## Handling Validation Errors

### Exception-Based

```csharp
try
{
    var result = await mediator.Request(new CreateUserRequest("""", ""invalid""));
}
catch (ValidateException ex)
{
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($""{error.Key}: {string.Join("", "", error.Value)}"");
    }
}
```

### Result-Based

```csharp
var result = await mediator.Request(new CreateUserRequest("""", ""invalid""));
if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine(error.ErrorMessage);
    }
}
```",

        ["http"] = @"# HTTP Extension

Streamlines HTTP requests without boilerplate code.

## Setup

```csharp
services.AddShinyMediator(cfg =>
{
    cfg.AddHttpClient();
});
```

## Creating HTTP Requests

```csharp
[Http(HttpVerb.Get, ""users/{id}"")]
public record GetUserRequest(
    [HttpParameter(HttpParameterType.Path)] int Id
) : IHttpRequest<User>;

[Http(HttpVerb.Post, ""users"")]
public record CreateUserRequest(
    [HttpBody] UserDto Body
) : IHttpRequest<User>;

[Http(HttpVerb.Get, ""users"")]
public record ListUsersRequest(
    [HttpParameter(HttpParameterType.Query)] int Page,
    [HttpParameter(HttpParameterType.Query)] int PageSize
) : IHttpRequest<PagedResult<User>>;
```

## Configuration

```json
{
  ""Mediator"": {
    ""Http"": {
      ""MyNamespace.*"": ""https://api.example.com"",
      ""Timeout"": 20,
      ""Debug"": true
    }
  }
}
```

## HTTP Request Decorator

For authentication, headers, etc.:

```csharp
public class AuthDecorator : IHttpRequestDecorator
{
    public Task Decorate(HttpRequestMessage request, CancellationToken ct)
    {
        request.Headers.Authorization =
            new AuthenticationHeaderValue(""Bearer"", ""token"");
        return Task.CompletedTask;
    }
}
```

## OpenAPI Code Generation

In your `.csproj`:

```xml
<ItemGroup>
  <MediatorHttp Include=""openapi.json""
                Namespace=""MyApp.Api""
                GenerateJsonConverters=""true"" />
</ItemGroup>
```

## Direct HTTP Requests

```csharp
var result = await mediator.Request(new HttpDirectRequest<User>
{
    Method = HttpMethod.Get,
    Uri = ""https://api.example.com/users/1""
});
```",

        ["context"] = @"# Execution Context

The context enables middleware to inject supplementary data without contaminating data structures.

## Accessing Context

### Requests

```csharp
await mediator.RequestWithContext(new MyRequest(), (result, context) =>
{
    var cacheInfo = context.Cache();
    var offlineInfo = context.Offline();
});
```

### Commands

```csharp
var context = await mediator.Send(new MyCommand());
// Access context.Values dictionary
```

### Events

```csharp
await mediator.PublishWithContext(new MyEvent(), (aggregatedContext) =>
{
    foreach (var childContext in aggregatedContext.ChildContexts)
    {
        // Per-handler context
    }
});
```

## Setting Context Values

In middleware:

```csharp
public async Task<TResult> Process(
    TRequest request,
    IMediatorContext context,
    RequestHandlerDelegate<TResult> next,
    CancellationToken ct)
{
    context.Values[""CustomKey""] = ""CustomValue"";
    return await next();
}
```

## Context Options

```csharp
await mediator.Request(new MyRequest(), ctx =>
{
    ctx.BypassMiddlewareEnabled = true;      // Skip all middleware
    ctx.BypassExceptionHandlingEnabled = true; // Skip exception handlers
});
```

## IMediatorContext Interface

The context mirrors `IMediator` methods, enabling nested calls within the same service scope:

```csharp
public async Task<MyResponse> Handle(MyRequest request, IMediatorContext context, CancellationToken ct)
{
    // Call another request within same scope
    var related = await context.Request(new RelatedRequest());
    return new MyResponse();
}
```",

        ["source-generation"] = @"# Source Generation Features

Shiny.Mediator offers source generation for AOT compatibility and reduced boilerplate.

## Handler Registration

```csharp
[MediatorSingleton]
public partial class MyHandler : IRequestHandler<MyRequest, MyResponse>
{
    // Handler implementation
}

[MediatorScoped]
public partial class ScopedHandler : ICommandHandler<MyCommand>
{
    // Handler implementation
}
```

Register all:
```csharp
services.AddShinyMediator(cfg =>
{
    cfg.AddMediatorRegistry();
});
```

## Contract Keys

```csharp
[ContractKey(""{Id}-{Category}"")]
public partial record ProductRequest(int Id, string Category) : IRequest<Product>;
```

## JSON Converters

For AOT scenarios:

```csharp
[SourceGenerateJsonConverter]
public partial record MyResponse(string Data);
```

## Custom Attributes

```csharp
[AttributeUsage(AttributeTargets.Method)]
public class AuditAttribute : MediatorMiddlewareAttribute
{
    public string AuditLevel { get; set; }
}

// In handler
[Audit(AuditLevel = ""High"")]
public async Task<MyResponse> Handle(MyRequest request, IMediatorContext context, CancellationToken ct)
{
    // ...
}

// Access in middleware
var audit = context.GetHandlerAttribute<AuditAttribute>();
```

## HTTP Contracts (OpenAPI)

```xml
<ItemGroup>
  <MediatorHttp Include=""api.json""
                Namespace=""MyApp.Contracts""
                GenerateJsonConverters=""true"" />
</ItemGroup>
```",

        ["exception-handlers"] = @"# Exception Handling

Global exception handling across all handler types.

## Creating Exception Handlers

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public Task<bool> Handle(Exception exception, IMediatorContext context)
    {
        _logger.LogError(exception, ""Mediator error in {Handler}"",
            context.HandlerType?.Name);

        // Return true if handled, false to propagate
        return Task.FromResult(true);
    }
}
```

Register:
```csharp
services.AddSingleton<IExceptionHandler, GlobalExceptionHandler>();
```

## User Notification Middleware

Automatic try-catch-notify for MAUI/Blazor:

```json
{
  ""Mediator"": {
    ""UserNotification"": {
      ""MyNamespace.*"": {
        ""Title"": ""Error"",
        ""Message"": ""An error occurred""
      }
    }
  }
}
```

## Bypassing Exception Handling

```csharp
await mediator.Request(new MyRequest(), ctx =>
{
    ctx.BypassExceptionHandlingEnabled = true;
});
```

## Event Exception Handling

Event handler exceptions are logged but don't block other handlers:

```csharp
// All handlers execute even if one fails
await mediator.Publish(new MyEvent());
```

## Important Notes

- Stream handlers don't receive global exception handling
- Use explicit configuration over wildcards
- Multiple exception handlers can be registered",

        ["advanced"] = @"# Advanced Topics

## DI Container Support

- **Microsoft.Extensions.DependencyInjection**: Basic support
- **DryIoc**: Full support
- **Autofac**: Full support

## Event Collectors

For components outside DI lifecycle (MAUI ViewModels, Blazor components):

```csharp
services.AddShinyMediator(cfg =>
{
    cfg.UseMaui();  // Adds MAUI event collector
    // or
    cfg.UseBlazor(); // Adds Blazor event collector
});
```

## Covariance

Event handlers can handle base types:

```csharp
public class BaseEventHandler : IEventHandler<BaseEvent>
{
    public Task Handle(BaseEvent @event, IMediatorContext context, CancellationToken ct)
    {
        // Handles BaseEvent and all derived events
        return Task.CompletedTask;
    }
}
```

## Project Structure (Vertical Slices)

```
MyApp/
├── Features/
│   ├── Users/
│   │   ├── GetUser/
│   │   │   ├── GetUserRequest.cs
│   │   │   ├── GetUserHandler.cs
│   │   │   └── GetUserValidator.cs
│   │   └── CreateUser/
│   │       ├── CreateUserRequest.cs
│   │       └── CreateUserHandler.cs
│   └── Products/
│       └── ...
└── Shared/
    ├── Middleware/
    └── Contracts/
```

## Multiple Handlers per Class

```csharp
[MediatorSingleton]
public partial class MultiHandler :
    IRequestHandler<Request1, Response1>,
    ICommandHandler<Command1>,
    IEventHandler<Event1>
{
    // All handlers in one class
}
```

## Extension Method Helpers

```csharp
// Register all interfaces
services.AddSingletonAsImplementedInterfaces<MyMultiHandler>();
services.AddScopedAsImplementedInterfaces<MyScopedHandler>();
```"
    };

    [McpServerTool]
    [Description("Search and retrieve Shiny.Mediator documentation. Returns detailed documentation for the specified topic.")]
    public static string GetMediatorDocs(
        [Description("The documentation topic. Available: overview, getting-started, requests, commands, events, streams, middleware, caching, offline, resilience, validation, http, context, source-generation, exception-handlers, advanced")]
        string topic)
    {
        var normalizedTopic = topic.ToLowerInvariant().Trim();

        if (Documentation.TryGetValue(normalizedTopic, out var doc))
        {
            return doc;
        }

        var availableTopics = string.Join(", ", Documentation.Keys.OrderBy(k => k));
        return $"Topic '{topic}' not found. Available topics: {availableTopics}";
    }

    [McpServerTool]
    [Description("List all available Shiny.Mediator documentation topics")]
    public static string ListMediatorTopics()
    {
        var topics = Documentation.Keys.OrderBy(k => k).ToList();

        return $@"# Available Shiny.Mediator Documentation Topics

## Core Concepts
- **overview** - Introduction and key features
- **getting-started** - Setup guide for MAUI and Blazor

## Contract Types
- **requests** - Request/Response pattern
- **commands** - Fire-and-forget commands
- **events** - Pub/Sub events
- **streams** - Async enumerable streams

## Middleware & Extensions
- **middleware** - Middleware pipeline overview
- **caching** - In-memory and persistent caching
- **offline** - Offline availability for mobile
- **resilience** - Retry policies and circuit breakers
- **validation** - Data annotations and FluentValidation
- **http** - HTTP request extension

## Advanced
- **context** - Execution context and metadata
- **source-generation** - AOT-compatible source generators
- **exception-handlers** - Global exception handling
- **advanced** - DI containers, event collectors, covariance

Use `GetMediatorDocs(topic)` to retrieve detailed documentation for any topic.

**GitHub**: https://github.com/shinyorg/mediator
**Documentation**: https://shinylib.net/client/mediator/";
    }

    [McpServerTool]
    [Description("Search across all Shiny.Mediator documentation for a specific term or concept")]
    public static string SearchMediatorDocs(
        [Description("The search term to find in the documentation")]
        string searchTerm)
    {
        var results = new List<(string Topic, string Excerpt)>();
        var normalizedSearch = searchTerm.ToLowerInvariant();

        foreach (var (topic, content) in Documentation)
        {
            if (content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            {
                // Find relevant excerpts
                var lines = content.Split('\n');
                var matchingLines = lines
                    .Where(l => l.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .Take(3)
                    .ToList();

                if (matchingLines.Count > 0)
                {
                    results.Add((topic, string.Join("\n", matchingLines)));
                }
            }
        }

        if (results.Count == 0)
        {
            return $"No results found for '{searchTerm}'. Try different terms or use ListMediatorTopics() to see available topics.";
        }

        var output = $"# Search Results for '{searchTerm}'\n\nFound in {results.Count} topic(s):\n\n";

        foreach (var (topic, excerpt) in results)
        {
            output += $"## {topic}\n{excerpt}\n\n---\n\n";
        }

        output += $"\nUse `GetMediatorDocs(topic)` for full documentation on any topic.";

        return output;
    }

    [McpServerTool]
    [Description("Get a quick code example for a specific Shiny.Mediator feature")]
    public static string GetMediatorExample(
        [Description("The feature to get an example for: request, command, event, stream, caching, validation, http, middleware")]
        string feature)
    {
        var examples = new Dictionary<string, string>
        {
            ["request"] = @"// Request Contract
public record GetUserRequest(int UserId) : IRequest<User>;
public record User(int Id, string Name, string Email);

// Handler
[MediatorSingleton]
public partial class GetUserHandler : IRequestHandler<GetUserRequest, User>
{
    private readonly IUserRepository _repo;

    public GetUserHandler(IUserRepository repo) => _repo = repo;

    public async Task<User> Handle(GetUserRequest request, IMediatorContext context, CancellationToken ct)
    {
        return await _repo.GetByIdAsync(request.UserId, ct);
    }
}

// Usage
var user = await mediator.Request(new GetUserRequest(123));",

            ["command"] = @"// Command Contract
public record SendEmailCommand(string To, string Subject, string Body) : ICommand;

// Handler
[MediatorSingleton]
public partial class SendEmailHandler : ICommandHandler<SendEmailCommand>
{
    private readonly IEmailService _email;

    public SendEmailHandler(IEmailService email) => _email = email;

    public async Task Handle(SendEmailCommand command, IMediatorContext context, CancellationToken ct)
    {
        await _email.SendAsync(command.To, command.Subject, command.Body, ct);
    }
}

// Usage
await mediator.Send(new SendEmailCommand(""user@example.com"", ""Hello"", ""Welcome!""));",

            ["event"] = @"// Event Contract
public record OrderPlacedEvent(int OrderId, decimal Total) : IEvent;

// Handlers (multiple can exist)
[MediatorSingleton]
public partial class InventoryHandler : IEventHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent @event, IMediatorContext context, CancellationToken ct)
    {
        // Update inventory
    }
}

[MediatorSingleton]
public partial class NotificationHandler : IEventHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent @event, IMediatorContext context, CancellationToken ct)
    {
        // Send notification
    }
}

// Usage
await mediator.Publish(new OrderPlacedEvent(123, 99.99m));",

            ["stream"] = @"// Stream Request
public record SearchResultsRequest(string Query) : IStreamRequest<SearchResult>;
public record SearchResult(int Id, string Title);

// Handler
[MediatorSingleton]
public partial class SearchHandler : IStreamRequestHandler<SearchResultsRequest, SearchResult>
{
    public async IAsyncEnumerable<SearchResult> Handle(
        SearchResultsRequest request,
        IMediatorContext context,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var results = await SearchDatabaseAsync(request.Query);
        foreach (var result in results)
        {
            if (ct.IsCancellationRequested) yield break;
            yield return result;
        }
    }
}

// Usage
var stream = await mediator.Request(new SearchResultsRequest(""query""));
await foreach (var result in stream.Result)
{
    Console.WriteLine(result.Title);
}",

            ["caching"] = @"// Cached Request Handler
[MediatorSingleton]
[Cache(AbsoluteExpirationSeconds = 300)]
public partial class GetProductHandler : IRequestHandler<GetProductRequest, Product>
{
    public async Task<Product> Handle(GetProductRequest request, IMediatorContext context, CancellationToken ct)
    {
        // This result will be cached for 5 minutes
        return await _db.Products.FindAsync(request.Id);
    }
}

// With Contract Key for parameterized caching
[ContractKey(""{Id}"")]
public partial record GetProductRequest(int Id) : IRequest<Product>;

// Force refresh
await mediator.Request(new GetProductRequest(1), ctx => ctx.ForceCacheRefresh());",

            ["validation"] = @"// With Data Annotations
[Validate]
public record CreateUserRequest(
    [Required] [MinLength(2)] string Name,
    [Required] [EmailAddress] string Email,
    [Range(18, 150)] int Age
) : IRequest<User>;

// With FluentValidation
[Validate]
public record CreateOrderRequest(int ProductId, int Quantity) : IRequest<Order>;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).InclusiveBetween(1, 100);
    }
}

// Handle validation errors
try
{
    var user = await mediator.Request(new CreateUserRequest("""", ""invalid"", 5));
}
catch (ValidateException ex)
{
    foreach (var error in ex.Errors)
        Console.WriteLine($""{error.Key}: {error.Value}"");
}",

            ["http"] = @"// HTTP GET Request
[Http(HttpVerb.Get, ""users/{id}"")]
public record GetUserRequest(
    [HttpParameter(HttpParameterType.Path)] int Id
) : IHttpRequest<User>;

// HTTP POST with Body
[Http(HttpVerb.Post, ""users"")]
public record CreateUserRequest(
    [HttpBody] CreateUserDto Body
) : IHttpRequest<User>;

// HTTP GET with Query Parameters
[Http(HttpVerb.Get, ""users"")]
public record SearchUsersRequest(
    [HttpParameter(HttpParameterType.Query)] string Search,
    [HttpParameter(HttpParameterType.Query)] int Page = 1
) : IHttpRequest<PagedResult<User>>;

// Configuration (appsettings.json)
{
  ""Mediator"": {
    ""Http"": {
      ""MyApp.Contracts.*"": ""https://api.myapp.com""
    }
  }
}",

            ["middleware"] = @"// Custom Request Middleware
[MediatorSingleton]
public partial class LoggingMiddleware<TRequest, TResult> : IRequestMiddleware<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly ILogger _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware<TRequest, TResult>> logger) => _logger = logger;

    public async Task<TResult> Process(
        TRequest request,
        IMediatorContext context,
        RequestHandlerDelegate<TResult> next,
        CancellationToken ct)
    {
        _logger.LogInformation(""Handling {Request}"", typeof(TRequest).Name);
        var sw = Stopwatch.StartNew();

        try
        {
            return await next();
        }
        finally
        {
            _logger.LogInformation(""Handled {Request} in {Ms}ms"", typeof(TRequest).Name, sw.ElapsedMilliseconds);
        }
    }
}

// Registration
services.AddSingleton(typeof(IRequestMiddleware<,>), typeof(LoggingMiddleware<,>));"
        };

        var normalizedFeature = feature.ToLowerInvariant().Trim();

        if (examples.TryGetValue(normalizedFeature, out var example))
        {
            return $"# {char.ToUpper(feature[0]) + feature[1..]} Example\n\n```csharp\n{example}\n```";
        }

        var available = string.Join(", ", examples.Keys.OrderBy(k => k));
        return $"Example '{feature}' not found. Available examples: {available}";
    }
}
