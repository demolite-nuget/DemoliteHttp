using System.Net;

namespace Demolite.Http.Interfaces;

public interface IHttpResponse
{
	public HttpStatusCode StatusCode { get; set; }

	public string? ResponseBody { get; set; }

	public bool IsSuccess { get; set; }

	public string? ErrorContent { get; set; }

	public Exception? Exception { get; set; }
}