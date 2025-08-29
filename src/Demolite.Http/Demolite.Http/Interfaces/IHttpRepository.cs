namespace Demolite.Http.Interfaces;

public interface IHttpRepository
{
	public Task<IHttpResponse> GetAsync(IRequestUri requestUri);

	public Task<IHttpResponse> PostJsonAsync(IRequestUri requestUri, string body);

	public Task<IHttpResponse> PutJsonAsync(IRequestUri requestUri, string body);

	public Task<IHttpResponse> PatchJsonAsync(IRequestUri requestUri, string body);

	public Task<IHttpResponse> DeleteAsync(IRequestUri requestUri);
}