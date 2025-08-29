using System.Runtime.CompilerServices;
using Demolite.Http.Args;
using Demolite.Http.Interfaces;
using Demolite.Http.Response;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global

namespace Demolite.Http.Repository;

public abstract partial class AbstractHttpRepository : IHttpRepository
{
	protected readonly HttpClient Client = new();

	public event EventHandler<LogEventArgs>? Log;

	public virtual async Task PrepareRequestAsync()
	{
		// this is an overrideable method
		await Task.CompletedTask;
	}

	public async Task<IHttpResponse> GetAsync(IRequestUri requestUri)
	{
		await PrepareRequestAsync();
		SetGetHeaders();

		Log?.Invoke(this, new LogEventArgs($"Executing GET to: {requestUri.Build()}"));
		return await ExecuteRequestAsync(Client.GetAsync(requestUri.Build()));
	}

	public async Task<IHttpResponse> GetWithFormContentAsync(
		IRequestUri requestUri,
		Dictionary<string, string> formContent
	)
	{
		await PrepareRequestAsync();
		SetGetHeaders();

		var request = new HttpRequestMessage()
		{
			Method = HttpMethod.Get,
			RequestUri = new System.Uri(requestUri.Build()),
			Content = new FormUrlEncodedContent(formContent),
		};

		return await ExecuteRequestAsync(Client.SendAsync(request));
	}

	public async Task<IHttpResponse> PostJsonAsync(IRequestUri requestUri, string body)
	{
		await PrepareRequestAsync();
		SetPostHeaders();

		var content = new StringContent(body);

		return await ExecuteRequestAsync(Client.PostAsync(requestUri.Build(), content));
	}

	public async Task<IHttpResponse> PutJsonAsync(IRequestUri requestUri, string body)
	{
		await PrepareRequestAsync();
		SetPostHeaders();

		var content = new StringContent(body);

		return await ExecuteRequestAsync(Client.PutAsync(requestUri.Build(), content));
	}

	public async Task<IHttpResponse> PatchJsonAsync(IRequestUri requestUri, string body)
	{
		await PrepareRequestAsync();
		SetPatchHeaders();

		var content = new StringContent(body);

		return await ExecuteRequestAsync(Client.PatchAsync(requestUri.Build(), content));
	}

	public async Task<IHttpResponse> DeleteAsync(IRequestUri requestUri)
	{
		await PrepareRequestAsync();
		SetDeleteHeaders();

		return await ExecuteRequestAsync(Client.DeleteAsync(requestUri.Build()));
	}

	private async Task<IHttpResponse> ExecuteRequestAsync(
		Task<HttpResponseMessage> call,
		[CallerMemberName] string callerMemberName = ""
	)
	{
		try
		{
			var response = await call;
			return await HttpResponse.Parse(response);
		}
		catch (Exception ex)
		{
			Log?.Invoke(this, new LogEventArgs($"Exception in call from {callerMemberName}: {ex.Message}"));
			return HttpResponse.Parse(ex);
		}
	}
}