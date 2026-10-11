using System.Net;
using Demolite.Http.Auth;
using Demolite.Http.Tests.Helpers;

namespace Demolite.Http.Tests.Auth;

[TestClass]
public class HeaderHandlerTests
{
	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	public async Task SendAsync_SetsHeader()
	{
		var inner = FakeHandler.Responding(HttpStatusCode.OK);
		using var client = CreateClient(new HeaderHandler("X-Api-Key", () => "abc"), inner);

		await client.GetAsync("items", TestContext.CancellationToken);

		Assert.AreEqual("abc", inner.Requests[0].Headers.GetValues("X-Api-Key").Single());
	}

	[TestMethod]
	public async Task SendAsync_ReplacesExistingHeader()
	{
		var inner = FakeHandler.Responding(HttpStatusCode.OK);
		using var client = CreateClient(new HeaderHandler("X-Api-Key", () => "new"), inner);
		using var request = new HttpRequestMessage(HttpMethod.Get, "items");
		request.Headers.Add("X-Api-Key", "old");

		await client.SendAsync(request, TestContext.CancellationToken);

		Assert.AreEqual("new", inner.Requests[0].Headers.GetValues("X-Api-Key").Single());
	}

	[TestMethod]
	public async Task SendAsync_ReadsValueOnEveryRequest()
	{
		var inner = FakeHandler.Responding(HttpStatusCode.OK);
		var key = "first";
		using var client = CreateClient(new HeaderHandler("X-Api-Key", () => key), inner);

		await client.GetAsync("items", TestContext.CancellationToken);
		key = "second";
		await client.GetAsync("items", TestContext.CancellationToken);

		Assert.AreEqual("second", inner.Requests[1].Headers.GetValues("X-Api-Key").Single());
	}

	private static HttpClient CreateClient(HeaderHandler handler, FakeHandler inner)
	{
		handler.InnerHandler = inner;

		return new HttpClient(handler) { BaseAddress = new Uri("https://api.test/") };
	}
}
