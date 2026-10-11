using System.Text.Json;
using Demolite.Core.Models;
using Demolite.Http.Client;
using Microsoft.Extensions.Logging;

namespace Demolite.Http.Tests.Helpers;

public sealed record Item(int Id, string Name);

public sealed class TestApiClient(
	HttpClient httpClient,
	JsonSerializerOptions? serializerOptions = null,
	ILogger? logger = null)
	: ApiClient(httpClient, serializerOptions, logger)
{
	public Task<Result<Item>> GetItemAsync(string uri, CancellationToken ct = default) => GetAsync<Item>(uri, ct);

	public Task<Result> GetNothingAsync(string uri, CancellationToken ct = default) => GetAsync(uri, ct);

	public Task<Result<Item>> PostItemAsync(string uri, object? body, CancellationToken ct = default)
		=> PostAsync<Item>(uri, body, ct);

	public Task<Result> PostNothingAsync(string uri, object? body, CancellationToken ct = default)
		=> PostAsync(uri, body, ct);

	public Task<Result<Item>> PutItemAsync(string uri, object? body) => PutAsync<Item>(uri, body);

	public Task<Result<Item>> PatchItemAsync(string uri, object? body) => PatchAsync<Item>(uri, body);

	public Task<Result<Item>> DeleteItemAsync(string uri) => DeleteAsync<Item>(uri);

	public Task<Result<Item>> GetItemWithHeaderAsync(string uri, string name, string value)
	{
		var request = CreateRequest(HttpMethod.Get, uri);
		request.Headers.Add(name, value);

		return SendAsync<Item>(request);
	}
}
