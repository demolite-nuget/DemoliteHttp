using System.Net.Http.Headers;
using Demolite.Core.Models;

namespace Demolite.Http.Auth;

/// <summary>
/// Base class for providers that request a token and reuse it until shortly before it expires.
/// Concurrent callers share a single token request. Register implementations as singletons.
/// </summary>
/// <param name="scheme">The authorization scheme.</param>
/// <param name="timeProvider">The clock used for expiry. Defaults to the system clock.</param>
public abstract class CachedTokenProvider(string scheme = "Bearer", TimeProvider? timeProvider = null) : IAuthProvider
{
	private readonly SemaphoreSlim _lock = new(1, 1);
	private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
	private volatile CachedToken? _cached;

	/// <summary>
	/// The time before the real expiry at which a token is renewed.
	/// </summary>
	protected virtual TimeSpan RenewalMargin => TimeSpan.FromSeconds(60);

	/// <summary>
	/// Requests a new token from the identity service.
	/// </summary>
	/// <param name="ct">The cancellation token.</param>
	protected abstract Task<Result<AccessToken>> RequestTokenAsync(CancellationToken ct);

	/// <summary>
	/// Discards the cached token so the next request fetches a new one.
	/// </summary>
	public void Invalidate() => _cached = null;

	/// <inheritdoc />
	public async ValueTask<Result<AuthenticationHeaderValue>> GetHeaderAsync(CancellationToken ct = default)
	{
		var token = await GetTokenAsync(ct);

		return token.IsSuccess
			? Result<AuthenticationHeaderValue>.Success(new AuthenticationHeaderValue(scheme, token.Value))
			: Result<AuthenticationHeaderValue>.Failure(token.Error!);
	}

	private async Task<Result<string>> GetTokenAsync(CancellationToken ct)
	{
		if (TryGetValid() is { } cached)
			return Result<string>.Success(cached);

		await _lock.WaitAsync(ct);

		try
		{
			return TryGetValid() is { } renewed
				? Result<string>.Success(renewed)
				: await RenewAsync(ct);
		}
		finally
		{
			_lock.Release();
		}
	}

	private async Task<Result<string>> RenewAsync(CancellationToken ct)
	{
		var result = await RequestTokenAsync(ct);

		if (!result.IsSuccess)
			return Result<string>.Failure(result.Error!);

		var token = result.Value!;
		_cached = new CachedToken(token.Value, _timeProvider.GetUtcNow() + token.ExpiresIn - RenewalMargin);

		return Result<string>.Success(token.Value);
	}

	private string? TryGetValid()
	{
		var cached = _cached;

		return cached is not null && _timeProvider.GetUtcNow() < cached.ExpiresAt
			? cached.Value
			: null;
	}

	private sealed record CachedToken(string Value, DateTimeOffset ExpiresAt);
}
