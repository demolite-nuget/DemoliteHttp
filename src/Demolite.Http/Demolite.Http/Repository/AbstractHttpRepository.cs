using Demolite.Http.Interfaces;
using Flurl.Http;
using Polly;

namespace Demolite.Http.Repository;

/// <summary>
///     Core Http Repository class.
/// </summary>
public abstract partial class AbstractHttpRepository<TPb>
	where TPb : class, IParameterBuilder<TPb>
{
	protected AbstractHttpRepository()
	{
		SetupClient();
	}

	/// <summary>
	///     Default request timeout in milliseconds.
	/// </summary>
	protected virtual int Timeout => 5000;
	
	/// <summary>
	/// Default resilience pipeline builder.
	/// Can be set null if not intended to be used.
	/// </summary>
	protected virtual IResiliencePipelineBuilder? ResiliencePipelineBuilder { get; }

	/// <summary>
	///     Setup method used to prepare the Flurl Client
	/// </summary>
	protected abstract void SetupClient();

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