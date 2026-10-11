using Demolite.Http.Auth;

namespace Demolite.Http.Tests.Auth;

[TestClass]
public class SimpleAuthProviderTests
{
	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	public async Task StaticAuthProvider_ReturnsSchemeAndToken()
	{
		var sut = new StaticAuthProvider("Token", () => "abc");

		var result = await sut.GetHeaderAsync(TestContext.CancellationToken);

		Assert.AreEqual("Token", result.Value!.Scheme);
		Assert.AreEqual("abc", result.Value.Parameter);
	}

	[TestMethod]
	public async Task StaticAuthProvider_ReadsTokenOnEveryCall()
	{
		var token = "first";
		var sut = new StaticAuthProvider("Token", () => token);

		await sut.GetHeaderAsync(TestContext.CancellationToken);
		token = "second";
		var result = await sut.GetHeaderAsync(TestContext.CancellationToken);

		Assert.AreEqual("second", result.Value!.Parameter);
	}

	[TestMethod]
	public async Task BasicAuthProvider_ReturnsBase64Credentials()
	{
		var sut = new BasicAuthProvider(() => "elf", () => "secret");

		var result = await sut.GetHeaderAsync(TestContext.CancellationToken);

		Assert.AreEqual("Basic", result.Value!.Scheme);
		Assert.AreEqual(Convert.ToBase64String("elf:secret"u8.ToArray()), result.Value.Parameter);
	}
}
