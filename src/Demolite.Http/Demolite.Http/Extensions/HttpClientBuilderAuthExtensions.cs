using Demolite.Http.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Demolite.Http.Extensions;

/// <summary>
/// Authentication setup for typed api clients.
/// </summary>
public static class HttpClientBuilderAuthExtensions
{
	/// <summary>
	/// Authenticates the client with a provider that is registered once as a singleton.
	/// Use it for providers that cache tokens.
	/// </summary>
	/// <param name="builder">The client builder.</param>
	/// <typeparam name="TProvider">The provider type.</typeparam>
	public static IHttpClientBuilder AddAuth<TProvider>(this IHttpClientBuilder builder)
		where TProvider : class, IAuthProvider
	{
		builder.Services.TryAddSingleton<TProvider>();

		return builder.AddHttpMessageHandler(provider => new AuthHandler(provider.GetRequiredService<TProvider>()));
	}

	/// <summary>
	/// Authenticates the client with a provider created by the factory.
	/// </summary>
	/// <param name="builder">The client builder.</param>
	/// <param name="factory">Creates the provider.</param>
	public static IHttpClientBuilder AddAuth(
		this IHttpClientBuilder builder,
		Func<IServiceProvider, IAuthProvider> factory)
		=> builder.AddHttpMessageHandler(provider => new AuthHandler(factory(provider)));

	/// <summary>
	/// Authenticates the client with a scheme and a token that is read on every request.
	/// </summary>
	/// <param name="builder">The client builder.</param>
	/// <param name="scheme">The authorization scheme.</param>
	/// <param name="token">Returns the current token.</param>
	public static IHttpClientBuilder AddStaticAuth(
		this IHttpClientBuilder builder,
		string scheme,
		Func<IServiceProvider, string> token)
		=> builder.AddAuth(provider => new StaticAuthProvider(scheme, () => token(provider)));

	/// <summary>
	/// Authenticates the client with basic authentication. The credentials are read on every request.
	/// </summary>
	/// <param name="builder">The client builder.</param>
	/// <param name="userName">Returns the current user name.</param>
	/// <param name="password">Returns the current password.</param>
	public static IHttpClientBuilder AddBasicAuth(
		this IHttpClientBuilder builder,
		Func<IServiceProvider, string> userName,
		Func<IServiceProvider, string> password)
		=> builder.AddAuth(provider => new BasicAuthProvider(() => userName(provider), () => password(provider)));

	/// <summary>
	/// Sets a header on every request, for example an api key header. The value is read on every request.
	/// </summary>
	/// <param name="builder">The client builder.</param>
	/// <param name="name">The header name.</param>
	/// <param name="value">Returns the current header value.</param>
	public static IHttpClientBuilder AddHeader(
		this IHttpClientBuilder builder,
		string name,
		Func<IServiceProvider, string> value)
		=> builder.AddHttpMessageHandler(provider => new HeaderHandler(name, () => value(provider)));
}
