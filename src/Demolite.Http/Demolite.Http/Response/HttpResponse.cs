using System.Net;
using Demolite.Http.Interfaces;

namespace Demolite.Http.Response;

public class HttpResponse : IHttpResponse
{
	public HttpStatusCode StatusCode { get; set; }

	public string? ResponseBody { get; set; }

	public bool IsSuccess { get; set; }

	public string? ErrorContent { get; set; }

	public Exception? Exception { get; set; }

	public static async Task<IHttpResponse> Parse(HttpResponseMessage response)
	{
		if (response.IsSuccessStatusCode)
			return await Ok(response);

		return await Error(response);
	}

	public static IHttpResponse Parse(Exception exception)
		=> Error(exception);

	private static async Task<IHttpResponse> Error(HttpResponseMessage response)
	{
		return new HttpResponse()
		{
			StatusCode = response.StatusCode,
			IsSuccess = response.IsSuccessStatusCode,
			ErrorContent = await response.Content.ReadAsStringAsync(),
		};
	}

	private static HttpResponse Error(Exception exception)
	{
		return new HttpResponse()
		{
			StatusCode = HttpStatusCode.InternalServerError,
			IsSuccess = false,
			ErrorContent = exception.Message,
			Exception = exception,
		};
	}

	private static async Task<IHttpResponse> Ok(HttpResponseMessage response)
	{
		return new HttpResponse()
		{
			StatusCode = response.StatusCode,
			IsSuccess = response.IsSuccessStatusCode,
			ResponseBody = await response.Content.ReadAsStringAsync(),
		};
	}
}