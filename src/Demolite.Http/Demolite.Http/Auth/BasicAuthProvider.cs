using System.Net.Http.Headers;
using System.Text;
using Demolite.Core.Models;

namespace Demolite.Http.Auth;

/// <summary>
/// Provides a basic authentication header. The credentials are read on every request.
/// </summary>
/// <param name="userName">Returns the current user name.</param>
/// <param name="password">Returns the current password.</param>
public sealed class BasicAuthProvider(Func<string> userName, Func<string> password) : IAuthProvider
{
	/// <inheritdoc />
	public ValueTask<Result<AuthenticationHeaderValue>> GetHeaderAsync(CancellationToken ct = default)
	{
		var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName()}:{password()}"));

		return ValueTask.FromResult(Result<AuthenticationHeaderValue>.Success(new AuthenticationHeaderValue("Basic", credentials)));
	}
}
