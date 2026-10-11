using System.Net;
using Demolite.Core.Models;
using Demolite.Http.Auth;
using Demolite.Http.Extensions;
using Demolite.Http.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Demolite.Http.Tests.Extensions;

[TestClass]
public class RegistrationTests
{
	private readonly FakeHandler _handler = FakeHandler.Responding(HttpStatusCode.OK, """{"id":1,"name":"Elf"}""");

	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	public async Task AddApiClient_SetsBaseAddress()
	{
		var services = new ServiceCollection();
		services.AddApiClient<TestApiClient>("https://api.test/v1/").ConfigurePrimaryHttpMessageHandler(() => _handler);

		await using var provider = services.BuildServiceProvider();
		var result = await provider.GetRequiredService<TestApiClient>().GetItemAsync("items/1", TestContext.CancellationToken);

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("https://api.test/v1/items/1", _handler.Requests[0].Uri!.ToString());
	}

	[TestMethod]
	public async Task AddApiClient_ResolvesBaseAddressFromServices()
	{
		var services = new ServiceCollection();
		services.AddSingleton(new ApiSettings("https://settings.test/"));
		services.AddApiClient<TestApiClient>(sp => sp.GetRequiredService<ApiSettings>().BaseUrl)
			.ConfigurePrimaryHttpMessageHandler(() => _handler);

		await using var provider = services.BuildServiceProvider();
		await provider.GetRequiredService<TestApiClient>().GetItemAsync("items/1", TestContext.CancellationToken);

		Assert.AreEqual("https://settings.test/items/1", _handler.Requests[0].Uri!.ToString());
	}

	[TestMethod]
	public async Task AddStaticAuth_SetsAuthorizationHeader()
	{
		var services = new ServiceCollection();
		services.AddSingleton(new ApiSettings("https://api.test/", "abc"));
		services.AddApiClient<TestApiClient>("https://api.test/")
			.AddStaticAuth("Token", sp => sp.GetRequiredService<ApiSettings>().Key)
			.ConfigurePrimaryHttpMessageHandler(() => _handler);

		await using var provider = services.BuildServiceProvider();
		await provider.GetRequiredService<TestApiClient>().GetItemAsync("items/1", TestContext.CancellationToken);

		Assert.AreEqual("Token abc", _handler.Requests[0].Authorization!.ToString());
	}

	[TestMethod]
	public async Task AddBasicAuth_SetsAuthorizationHeader()
	{
		var services = new ServiceCollection();
		services.AddApiClient<TestApiClient>("https://api.test/")
			.AddBasicAuth(_ => "elf", _ => "secret")
			.ConfigurePrimaryHttpMessageHandler(() => _handler);

		await using var provider = services.BuildServiceProvider();
		await provider.GetRequiredService<TestApiClient>().GetItemAsync("items/1", TestContext.CancellationToken);

		Assert.AreEqual("Basic", _handler.Requests[0].Authorization!.Scheme);
	}

	[TestMethod]
	public async Task AddAuth_UsesSingletonProvider()
	{
		var services = new ServiceCollection();
		services.AddApiClient<TestApiClient>("https://api.test/")
			.AddAuth<CountingProvider>()
			.ConfigurePrimaryHttpMessageHandler(() => _handler);

		await using var provider = services.BuildServiceProvider();
		var client = provider.GetRequiredService<TestApiClient>();
		await client.GetItemAsync("items/1", TestContext.CancellationToken);
		await client.GetItemAsync("items/2", TestContext.CancellationToken);

		Assert.AreEqual(2, provider.GetRequiredService<CountingProvider>().Calls);
		Assert.AreEqual("Bearer counted", _handler.Requests[1].Authorization!.ToString());
	}

	private sealed record ApiSettings(string BaseUrl, string Key = "");

	private sealed class CountingProvider : IAuthProvider
	{
		public int Calls { get; private set; }

		public ValueTask<Result<System.Net.Http.Headers.AuthenticationHeaderValue>> GetHeaderAsync(
			CancellationToken ct = default)
		{
			Calls++;

			return ValueTask.FromResult(
				Result<System.Net.Http.Headers.AuthenticationHeaderValue>.Success(new("Bearer", "counted")));
		}
	}
}
