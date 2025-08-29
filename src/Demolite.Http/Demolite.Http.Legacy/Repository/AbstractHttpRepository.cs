using Demolite.Http.Legacy.Enum;
using Demolite.Http.Legacy.Interfaces;
using Flurl.Http;
using Polly;

namespace Demolite.Http.Legacy.Repository;

/// <summary>
///     Core Http Repository class.
/// </summary>
public abstract partial class AbstractHttpRepository<TPb>
	where TPb : class, IParameterBuilder<TPb>
{
	protected AbstractHttpRepository()
	{
		SetupClient();
		SetupPipelines();
	}

	/// <summary>
	///     Default request timeout in milliseconds.
	/// </summary>
	protected virtual int Timeout => 5000;


	/// <summary>
	///     Creates the base request.
	/// </summary>
	/// <param name="urlBuilder">Fully formed UrlBuilder</param>
	/// <returns></returns>
	protected virtual IFlurlRequest CreateRequest(IUrlBuilder<TPb> urlBuilder)
	{
		var request = new FlurlRequest(urlBuilder.BaseUrl).AppendPathSegments(urlBuilder.PathSegments);
		request.AppendPathSegment(urlBuilder.LastPathSegment);
		request.AppendQueryParam(urlBuilder.Parameters.Values);
		request.WithTimeout(Timeout);

		return request;
	}
}