using System.Net;

namespace Demolite.Http.Auth;

/// <summary>
/// Sets the authorization header of every request using an <see cref="IAuthProvider" />.
/// If the provider fails, the request is not sent and a 401 response with the failure message is returned.
/// </summary>
/// <param name="provider">Provides the authorization header.</param>
public sealed class AuthHandler(IAuthProvider provider) : DelegatingHandler
{
	/// <inheritdoc />
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
	{
		var header = await provider.GetHeaderAsync(ct);

		if (!header.IsSuccess)
			return new HttpResponseMessage(HttpStatusCode.Unauthorized)
			{
				Content = new StringContent(header.Error!.Message),
				RequestMessage = request,
			};

		request.Headers.Authorization = header.Value;

		return await base.SendAsync(request, ct);
	}
}
