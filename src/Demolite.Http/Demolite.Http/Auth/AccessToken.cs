namespace Demolite.Http.Auth;

/// <summary>
/// A token and the time it stays valid.
/// </summary>
/// <param name="Value">The token.</param>
/// <param name="ExpiresIn">How long the token is valid, counted from now.</param>
public sealed record AccessToken(string Value, TimeSpan ExpiresIn);
