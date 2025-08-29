namespace Demolite.Http.Interfaces;

public interface IRequestUri
{
	/// <summary>
	/// Builds the uri into a fully formed request uri.
	/// </summary>
	/// <returns>The base uri with all uri segments and parameters.</returns>
	public string Build();

	/// <summary>
	/// Adds one or more segments to the uri.
	/// </summary>
	/// <param name="segments">The uri segments to add.</param>
	/// <returns>The object itself for chaining.</returns>
	public IRequestUri WithSegments(params string[] segments);

	/// <summary>
	/// Adds one parameter to the uri.
	/// </summary>
	/// <param name="name">Parameter name.</param>
	/// <param name="value">Parameter value.</param>
	/// <returns>The object itself for chaining.</returns>
	public IRequestUri WithParameter(string name, string value);

	/// <summary>
	/// Adds many parameters to the uri.
	/// </summary>
	/// <param name="parameters">A dictionary of parameter names and values.</param>
	/// <returns>The object itself for chaining.</returns>
	public IRequestUri WithParameters(Dictionary<string, string> parameters);
}