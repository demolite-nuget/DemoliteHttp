using System.Net.Http.Headers;
using Demolite.Core.Models;

namespace Demolite.Http.Auth;

/// <summary>
/// Provides the authorization header for requests of an api client.
/// </summary>
public interface IAuthProvider
{
	/// <summary>
	/// Provides the authorization header for a request.
	/// </summary>
	/// <param name="ct">The cancellation token.</param>
	ValueTask<Result<AuthenticationHeaderValue>> GetHeaderAsync(CancellationToken ct = default);
}
