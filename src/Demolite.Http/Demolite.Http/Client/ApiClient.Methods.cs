using Demolite.Core.Models;

namespace Demolite.Http.Client;

public abstract partial class ApiClient
{
	/// <summary>
	/// Sends a GET request and deserializes the response body into <typeparamref name="T" />.
	/// </summary>
	/// <param name="requestUri">The uri relative to the base address of the client.</param>
	/// <param name="ct">The cancellation token.</param>
	protected Task<Result<T>> GetAsync<T>(string requestUri, CancellationToken ct = default)
		=> SendAsync<T>(CreateRequest(HttpMethod.Get, requestUri), ct);

	/// <summary>
	/// Sends a GET request and ignores the response body.
	/// </summary>
	/// <param name="requestUri">The uri relative to the base address of the client.</param>
	/// <param name="ct">The cancellation token.</param>
	protected Task<Result> GetAsync(string requestUri, CancellationToken ct = default)
		=> SendAsync(CreateRequest(HttpMethod.Get, requestUri), ct);

	/// <summary>
	/// Sends a POST request and deserializes the response body into <typeparamref name="T" />.
	/// </summary>
	/// <param name="requestUri">The uri relative to the base address of the client.</param>
	/// <param name="body">The object to send as json body.</param>
	/// <param name="ct">The cancellation token.</param>
	protected Task<Result<T>> PostAsync<T>(string requestUri, object? body = null, CancellationToken ct = default)
		=> SendAsync<T>(CreateRequest(HttpMethod.Post, requestUri, body), ct);

	/// <summary>
	/// Sends a POST request and ignores the response body.
	/// </summary>
	/// <param name="requestUri">The uri relative to the base address of the client.</param>
	/// <param name="body">The object to send as json body.</param>
	/// <param name="ct">The cancellation token.</param>
	protected Task<Result> PostAsync(string requestUri, object? body = null, CancellationToken ct = default)
		=> SendAsync(CreateRequest(HttpMethod.Post, requestUri, body), ct);

	/// <summary>
	/// Sends a PUT request and deserializes the response body into <typeparamref name="T" />.
	/// </summary>
	/// <param name="requestUri">The uri relative to the base address of the client.</param>
	/// <param name="body">The object to send as json body.</param>
	/// <param name="ct">The cancellation token.</param>
	protected Task<Result<T>> PutAsync<T>(string requestUri, object? body = null, CancellationToken ct = default)
		=> SendAsync<T>(CreateRequest(HttpMethod.Put, requestUri, body), ct);

	/// <summary>
	/// Sends a PUT request and ignores the response body.
	/// </summary>
	/// <param name="requestUri">The uri relative to the base address of the client.</param>
	/// <param name="body">The object to send as json body.</param>
	/// <param name="ct">The cancellation token.</param>
	protected Task<Result> PutAsync(string requestUri, object? body = null, CancellationToken ct = default)
		=> SendAsync(CreateRequest(HttpMethod.Put, requestUri, body), ct);

	/// <summary>
	/// Sends a PATCH request and deserializes the response body into <typeparamref name="T" />.
	/// </summary>
	/// <param name="requestUri">The uri relative to the base address of the client.</param>
	/// <param name="body">The object to send as json body.</param>
	/// <param name="ct">The cancellation token.</param>
	protected Task<Result<T>> PatchAsync<T>(string requestUri, object? body = null, CancellationToken ct = default)
		=> SendAsync<T>(CreateRequest(HttpMethod.Patch, requestUri, body), ct);

	/// <summary>
	/// Sends a PATCH request and ignores the response body.
	/// </summary>
	/// <param name="requestUri">The uri relative to the base address of the client.</param>
	/// <param name="body">The object to send as json body.</param>
	/// <param name="ct">The cancellation token.</param>
	protected Task<Result> PatchAsync(string requestUri, object? body = null, CancellationToken ct = default)
		=> SendAsync(CreateRequest(HttpMethod.Patch, requestUri, body), ct);

	/// <summary>
	/// Sends a DELETE request and deserializes the response body into <typeparamref name="T" />.
	/// </summary>
	/// <param name="requestUri">The uri relative to the base address of the client.</param>
	/// <param name="ct">The cancellation token.</param>
	protected Task<Result<T>> DeleteAsync<T>(string requestUri, CancellationToken ct = default)
		=> SendAsync<T>(CreateRequest(HttpMethod.Delete, requestUri), ct);

	/// <summary>
	/// Sends a DELETE request and ignores the response body.
	/// </summary>
	/// <param name="requestUri">The uri relative to the base address of the client.</param>
	/// <param name="ct">The cancellation token.</param>
	protected Task<Result> DeleteAsync(string requestUri, CancellationToken ct = default)
		=> SendAsync(CreateRequest(HttpMethod.Delete, requestUri), ct);
}
