namespace Demolite.Http.Auth;

/// <summary>
/// Sets a header on every request, for example an api key header. An existing header with the same name is replaced.
/// </summary>
/// <param name="name">The header name.</param>
/// <param name="value">Returns the current header value.</param>
public sealed class HeaderHandler(string name, Func<string> value) : DelegatingHandler
{
	/// <inheritdoc />
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
	{
		request.Headers.Remove(name);
		request.Headers.TryAddWithoutValidation(name, value());

		return base.SendAsync(request, ct);
	}
}
