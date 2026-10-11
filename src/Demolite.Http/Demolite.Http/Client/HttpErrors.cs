using Demolite.Core.Models;

namespace Demolite.Http.Client;

internal static class HttpErrors
{
	public static Error RequestFailed(HttpRequestMessage request, HttpRequestException exception) => new(
		"Http.RequestFailed",
		$"Request to {request.RequestUri} failed: {exception.Message}",
		exception,
		new Dictionary<string, object?> { ["statusCode"] = (int?)exception.StatusCode });

	public static Error Timeout(HttpRequestMessage request, Exception exception) => new(
		"Http.Timeout",
		$"Request to {request.RequestUri} timed out: {exception.Message}",
		exception);

	public static Error FromResponse(HttpRequestMessage request, HttpResponseMessage response, string content) => new(
		$"Http.{(int)response.StatusCode}",
		string.IsNullOrWhiteSpace(content) ? response.ReasonPhrase ?? "Request failed" : content,
		Metadata: new Dictionary<string, object?>
		{
			["statusCode"] = (int)response.StatusCode,
			["requestUri"] = request.RequestUri?.ToString(),
		});

	public static Error EmptyBody(HttpRequestMessage request) => new(
		"Http.EmptyBody",
		$"Response body for {request.RequestUri} deserialized to null.");

	public static Error DeserializationFailed(HttpRequestMessage request, Exception exception, string content) => new(
		"Http.DeserializationFailed",
		$"Failed to deserialize response from {request.RequestUri}: {exception.Message}",
		exception,
		new Dictionary<string, object?> { ["rawContent"] = content });
}
