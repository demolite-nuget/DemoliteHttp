using System.Net.Http.Json;
using System.Text.Json;
using Demolite.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Demolite.Http.Client;

/// <summary>
/// Base class for typed api clients. Every call returns a <see cref="Result" /> and never throws on http failures.
/// </summary>
/// <param name="httpClient">The client used to send requests.</param>
/// <param name="serializerOptions">The options for request and response bodies. Defaults to the web defaults.</param>
/// <param name="logger">The logger for requests. Nothing is logged if it is not set.</param>
public abstract partial class ApiClient(
	HttpClient httpClient,
	JsonSerializerOptions? serializerOptions = null,
	ILogger? logger = null)
{
	/// <summary>
	/// The client used to send requests.
	/// </summary>
	protected HttpClient HttpClient { get; } = httpClient;

	/// <summary>
	/// The options for request and response bodies.
	/// </summary>
	protected JsonSerializerOptions SerializerOptions { get; } = serializerOptions ?? JsonSerializerOptions.Web;

	/// <summary>
	/// The logger for requests.
	/// </summary>
	protected ILogger Logger { get; } = logger ?? NullLogger.Instance;

	/// <summary>
	/// Creates a request. A body is serialized as json.
	/// Use it to add headers for a single call before passing the request to SendAsync.
	/// </summary>
	/// <param name="method">The http method.</param>
	/// <param name="requestUri">The uri relative to the base address of the client.</param>
	/// <param name="body">The object to send as json body.</param>
	protected HttpRequestMessage CreateRequest(HttpMethod method, string requestUri, object? body = null)
	{
		var request = new HttpRequestMessage(method, requestUri);

		if (body is not null)
			request.Content = JsonContent.Create(body, body.GetType(), options: SerializerOptions);

		return request;
	}

	/// <summary>
	/// Sends the request and deserializes the response body into <typeparamref name="T" />.
	/// An empty response body results in a default value.
	/// </summary>
	/// <param name="request">The request to send. It is disposed after sending.</param>
	/// <param name="ct">The cancellation token.</param>
	protected async Task<Result<T>> SendAsync<T>(HttpRequestMessage request, CancellationToken ct = default)
	{
		using (request)
		{
			var content = await ExecuteAsync(request, ct);

			return content.IsSuccess
				? Deserialize<T>(content.Value!, request)
				: Result<T>.Failure(content.Error!);
		}
	}

	/// <summary>
	/// Sends the request and ignores the response body.
	/// </summary>
	/// <param name="request">The request to send. It is disposed after sending.</param>
	/// <param name="ct">The cancellation token.</param>
	protected async Task<Result> SendAsync(HttpRequestMessage request, CancellationToken ct = default)
	{
		using (request)
		{
			var content = await ExecuteAsync(request, ct);

			return content.IsSuccess
				? Result.Success()
				: Result.Failure(content.Error!);
		}
	}

	private async Task<Result<string>> ExecuteAsync(HttpRequestMessage request, CancellationToken ct)
	{
		var sendResult = await TrySendAsync(request, ct);

		if (!sendResult.IsSuccess)
		{
			Logger.LogError(
				"Request {Method} {Uri} failed to send: {Error}",
				request.Method, request.RequestUri, sendResult.Error!.Message);

			return Result<string>.Failure(sendResult.Error);
		}

		using var response = sendResult.Value!;
		var content = await response.Content.ReadAsStringAsync(ct);

		if (response.IsSuccessStatusCode)
		{
			Logger.LogDebug(
				"Request {Method} {Uri} returned {StatusCode}",
				request.Method, request.RequestUri, (int)response.StatusCode);

			return Result<string>.Success(content);
		}

		Logger.LogError(
			"Request {Method} {Uri} returned {StatusCode}: {Body}",
			request.Method, request.RequestUri, (int)response.StatusCode, content);

		return Result<string>.Failure(HttpErrors.FromResponse(request, response, content));
	}

	private async Task<Result<HttpResponseMessage>> TrySendAsync(HttpRequestMessage request, CancellationToken ct)
	{
		try
		{
			var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

			return Result<HttpResponseMessage>.Success(response);
		}
		catch (HttpRequestException ex)
		{
			return Result<HttpResponseMessage>.Failure(HttpErrors.RequestFailed(request, ex));
		}
		catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
		{
			return Result<HttpResponseMessage>.Failure(HttpErrors.Timeout(request, ex));
		}
	}

	private Result<T> Deserialize<T>(string content, HttpRequestMessage request)
	{
		if (content.Length == 0)
			return Result<T>.Success(default!);

		try
		{
			var value = JsonSerializer.Deserialize<T>(content, SerializerOptions);

			return value is null
				? Result<T>.Failure(HttpErrors.EmptyBody(request))
				: Result<T>.Success(value);
		}
		catch (JsonException ex)
		{
			return Result<T>.Failure(HttpErrors.DeserializationFailed(request, ex, content));
		}
	}
}
