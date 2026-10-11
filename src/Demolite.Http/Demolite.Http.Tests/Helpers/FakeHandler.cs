using System.Net;
using System.Text;

namespace Demolite.Http.Tests.Helpers;

public sealed class FakeHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) : HttpMessageHandler
{
	public List<RecordedRequest> Requests { get; } = [];

	public static FakeHandler Responding(HttpStatusCode status, string content = "")
		=> new(_ => Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(content, Encoding.UTF8) }));

	public static FakeHandler Throwing(Exception exception) => new(_ => throw exception);

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
		Requests.Add(new RecordedRequest(request.Method, request.RequestUri, request.Headers.Authorization, request.Headers, request.Content?.Headers.ContentType?.MediaType, body));

		return await responder(request);
	}
}

public sealed record RecordedRequest(
	HttpMethod Method,
	Uri? Uri,
	System.Net.Http.Headers.AuthenticationHeaderValue? Authorization,
	System.Net.Http.Headers.HttpRequestHeaders Headers,
	string? ContentType,
	string? Body);
