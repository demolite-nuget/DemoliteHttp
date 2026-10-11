using Demolite.Http.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Demolite.Http.Extensions;

/// <summary>
/// Registration of typed api clients.
/// </summary>
public static class ApiClientServiceCollectionExtensions
{
	/// <summary>
	/// Registers a typed api client with a fixed base address.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="baseAddress">The base address of the api.</param>
	/// <typeparam name="TClient">The client type.</typeparam>
	public static IHttpClientBuilder AddApiClient<TClient>(this IServiceCollection services, string baseAddress)
		where TClient : ApiClient
		=> services.AddHttpClient<TClient>(client => client.BaseAddress = new Uri(baseAddress));

	/// <summary>
	/// Registers a typed api client whose base address is resolved when the client is created.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="baseAddress">Returns the base address of the api.</param>
	/// <typeparam name="TClient">The client type.</typeparam>
	public static IHttpClientBuilder AddApiClient<TClient>(
		this IServiceCollection services,
		Func<IServiceProvider, string> baseAddress)
		where TClient : ApiClient
		=> services.AddHttpClient<TClient>((provider, client) => client.BaseAddress = new Uri(baseAddress(provider)));
}
