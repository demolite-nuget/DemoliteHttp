using System.Net;
using System.Net.Http.Headers;
using Demolite.Core.Models;
using Demolite.Http.Auth;
using Demolite.Http.Tests.Helpers;

namespace Demolite.Http.Tests.Auth;

[TestClass]
public class AuthHandlerTests
{
	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	public async Task SendAsync_SetsAuthorizationHeader()
	{
		var inner = FakeHandler.Responding(HttpStatusCode.OK);
		using var client = CreateClient(new StaticAuthProvider("Token", () => "abc"), inner);

		await client.GetAsync("items", TestContext.CancellationToken);

		Assert.AreEqual("Token abc", inner.Requests[0].Authorization!.ToString());
	}

	[TestMethod]
	public async Task SendAsync_ReturnsUnauthorizedWithMessage_WhenProviderFails()
	{
		var inner = FakeHandler.Responding(HttpStatusCode.OK);
		using var client = CreateClient(new FailingAuthProvider(), inner);

		var response = await client.GetAsync("items", TestContext.CancellationToken);

		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.AreEqual("no credentials", await response.Content.ReadAsStringAsync(TestContext.CancellationToken));
		Assert.IsEmpty(inner.Requests);
	}

	private static HttpClient CreateClient(IAuthProvider provider, FakeHandler inner)
		=> new(new AuthHandler(provider) { InnerHandler = inner }) { BaseAddress = new Uri("https://api.test/") };

	private sealed class FailingAuthProvider : IAuthProvider
	{
		public ValueTask<Result<AuthenticationHeaderValue>> GetHeaderAsync(CancellationToken ct = default)
			=> ValueTask.FromResult(
				Result<AuthenticationHeaderValue>.Failure(new Error("Auth.Missing", "no credentials")));
	}
}
