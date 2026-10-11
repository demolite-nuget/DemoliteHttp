namespace Demolite.Http.Requests;

/// <summary>
/// Builds a request uri from a base uri, segments and parameters.
/// </summary>
public interface IRequestUri
{
	/// <summary>
	/// Builds the uri into a fully formed request uri.
	/// </summary>
	/// <returns>The base uri with all uri segments and parameters.</returns>
	string Build();

	/// <summary>
	/// Adds one or more segments to the uri. Segments are url-encoded and empty segments are skipped.
	/// </summary>
	/// <param name="segments">The uri segments to add.</param>
	/// <returns>The object itself for chaining.</returns>
	IRequestUri WithSegments(params string[] segments);

	/// <summary>
	/// Adds one parameter to the uri. The value is url-encoded and formatted with the invariant culture.
	/// A null value is skipped.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <param name="value">Parameter value.</param>
	/// <returns>The object itself for chaining.</returns>
	IRequestUri WithParameter(string name, object? value);

	/// <summary>
	/// Adds many parameters to the uri.
	/// </summary>
	/// <param name="parameters">The parameter names and values.</param>
	/// <typeparam name="TValue">The type of the parameter values.</typeparam>
	/// <returns>The object itself for chaining.</returns>
	IRequestUri WithParameters<TValue>(IEnumerable<KeyValuePair<string, TValue>> parameters);
}
