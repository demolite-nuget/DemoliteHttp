# Demolite.Http

A base class for typed api clients on top of `HttpClient`.

- Every call returns a `Result` or `Result<T>` from `Demolite.Core`. Http failures are returned, not thrown.
- Authentication is a separate, reusable handler.
- Works with `IHttpClientFactory` and dependency injection.

## Migrating from 0.1.x

Version 0.2.0 is a rewrite and breaks the old API.

| Before | Now |
|---|---|
| `AbstractHttpRepository` | `ApiClient` |
| `IHttpResponse` | `Result` and `Result<T>` |
| `AbstractSerializer` | Built in. Pass `JsonSerializerOptions` to the client. |
| `Set*Headers`, `PrepareRequestAsync` | `AuthHandler` with an `IAuthProvider`, or `CreateRequest` for single calls |
| `Log` event | `ILogger` |
| `Demolite.Http.Uri` namespace | `Demolite.Http.Requests` |
| .NET 9 | .NET 10 |

`Demolite.Http.Legacy` is removed.

## Writing a client

Inherit from `ApiClient` and expose your calls. The verb methods are protected, so callers only see your methods.

```csharp
public sealed class ArmorClient(HttpClient httpClient, JsonSerializerOptions? options = null, ILogger<ArmorClient>? logger = null)
    : ApiClient(httpClient, options, logger)
{
    public Task<Result<Armor>> GetAsync(int id, CancellationToken ct = default)
        => GetAsync<Armor>($"armors/{id}", ct);

    public Task<Result<Armor>> CreateAsync(Armor armor, CancellationToken ct = default)
        => PostAsync<Armor>("armors", armor, ct);

    public Task<Result> DeleteAsync(int id, CancellationToken ct = default)
        => DeleteAsync($"armors/{id}", ct);
}
```

- Methods with a type parameter deserialize the response body. Methods without one ignore it.
- Request bodies are sent as json.
- An empty response body, such as a 204, gives a successful result with a default value.
- Without options, the web defaults are used (camel case, case insensitive).
- Without a logger, nothing is logged.

### Errors

| Code | Meaning |
|---|---|
| `Http.<status>` | The api returned a failure status. The message is the response body. |
| `Http.RequestFailed` | The request could not be sent, for example the host is unreachable. |
| `Http.Timeout` | The request timed out. |
| `Http.DeserializationFailed` | The body is not valid for the requested type. The raw content is in the metadata. |
| `Http.EmptyBody` | The body is valid json that is `null`. |

A cancelled token throws `OperationCanceledException`, as usual.

### Headers for a single call

```csharp
var request = CreateRequest(HttpMethod.Get, "armors");
request.Headers.Add("X-Type", type);

return SendAsync<List<Armor>>(request, ct);
```

Set headers that never change once in the `AddApiClient` setup. Do not change `DefaultRequestHeaders` per call, because the client is shared between threads.

## Registration

```csharp
services.AddApiClient<ArmorClient>("https://armors.example/api/")
    .AddStaticAuth("Token", sp => sp.GetRequiredService<ArmorSettings>().ApiKey);
```

`AddApiClient` returns the normal `IHttpClientBuilder`, so everything from `Microsoft.Extensions.Http` works. The base address can also be resolved from the services.

```csharp
services.AddApiClient<ArmorClient>(sp => sp.GetRequiredService<ArmorSettings>().BaseUrl);
```

For retries and timeouts, use the Microsoft resilience handler.

```csharp
services.AddApiClient<ArmorClient>("https://armors.example/api/")
    .AddStandardResilienceHandler();
```

### Logging with Serilog

The package only uses `Microsoft.Extensions.Logging`. If your host uses Serilog (`UseSerilog()`), the messages of the clients go through Serilog.

## Authentication

`AuthHandler` sets the `Authorization` header on every request. It asks an `IAuthProvider` for the header. If the provider fails, the request is not sent and the client returns `Http.401` with the failure message.

| Registration | Use for |
|---|---|
| `AddStaticAuth(scheme, token)` | Api keys and tokens. The token is read on every request, so rotating credentials work. |
| `AddBasicAuth(userName, password)` | Basic authentication. |
| `AddAuth<TProvider>()` | Your own provider, registered as a singleton. |
| `AddAuth(factory)` | Your own provider, created by a factory. |

### Tokens that expire

Inherit from `CachedTokenProvider` and implement one method. The base class caches the token, renews it 60 seconds before it expires, and makes concurrent requests share one token request.

```csharp
public sealed class ArmorTokenProvider(ArmorAuthClient authClient, ArmorSettings settings) : CachedTokenProvider
{
    protected override async Task<Result<AccessToken>> RequestTokenAsync(CancellationToken ct)
    {
        var result = await authClient.AuthenticateAsync(settings.Key, settings.Secret, ct);

        return result.IsSuccess
            ? Result<AccessToken>.Success(new AccessToken(result.Value!.Token, TimeSpan.FromSeconds(result.Value.ExpiresIn)))
            : Result<AccessToken>.Failure(result.Error!);
    }
}

services.AddApiClient<ArmorClient>("https://armors.example/api/").AddAuth<ArmorTokenProvider>();
```

Call `Invalidate()` on the provider to force a new token on the next request.

## Building uris

`RequestUri` builds uris with encoded segments and parameters. `Build()` returns the string.

```csharp
var uri = new RequestUri("armors")
    .WithSegments("type", "leather")
    .WithParameter("page", 2)
    .Build();

await client.GetAsync<List<Armor>>(uri, ct);
```

- Segments and parameter names and values are url-encoded.
- Numbers are formatted with the invariant culture.
- A parameter with a null value is skipped.
- A parameter name can be added more than once.
