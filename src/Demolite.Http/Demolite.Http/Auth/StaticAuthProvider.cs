using System.Net.Http.Headers;
using Demolite.Core.Models;

namespace Demolite.Http.Auth;

/// <summary>
/// Provides a header with a fixed scheme and a token that is read on every request.
/// </summary>
/// <param name="scheme">The authorization scheme.</param>
/// <param name="token">Returns the current token.</param>
public sealed class StaticAuthProvider(string scheme, Func<string> token) : IAuthProvider
{
	/// <inheritdoc />
	public ValueTask<Result<AuthenticationHeaderValue>> GetHeaderAsync(CancellationToken ct = default)
		=> ValueTask.FromResult(Result<AuthenticationHeaderValue>.Success(new AuthenticationHeaderValue(scheme, token())));
}
