using System.Globalization;

namespace Demolite.Http.Requests;

/// <summary>
/// Default implementation of <see cref="IRequestUri" />.
/// </summary>
/// <param name="baseUri">The base uri.</param>
/// <param name="trailingSlash">Whether the built uri ends with a slash before the parameters.</param>
public class RequestUri(string baseUri, bool trailingSlash = false) : IRequestUri
{
	private readonly List<string> _segments = [];
	private readonly List<string> _parameters = [];

	/// <summary>
	/// Builds the uri when a string is expected.
	/// </summary>
	public static implicit operator string(RequestUri requestUri) => requestUri.Build();

	/// <inheritdoc />
	public IRequestUri WithSegments(params string[] segments)
	{
		foreach (var segment in segments)
			_segments.AddRange(EncodeSegment(segment));

		return this;
	}

	/// <inheritdoc />
	public IRequestUri WithParameter(string name, object? value)
	{
		if (value is null)
			return this;

		_parameters.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(Format(value))}");

		return this;
	}

	/// <inheritdoc />
	public IRequestUri WithParameters<TValue>(IEnumerable<KeyValuePair<string, TValue>> parameters)
	{
		foreach (var parameter in parameters)
			WithParameter(parameter.Key, parameter.Value);

		return this;
	}

	/// <inheritdoc />
	public string Build()
	{
		var uri = BuildPath();

		if (trailingSlash && !uri.EndsWith('/'))
			uri += '/';

		return _parameters.Count > 0
			? $"{uri}?{string.Join('&', _parameters)}"
			: uri;
	}

	/// <inheritdoc />
	public override string ToString() => Build();

	private string BuildPath()
	{
		if (_segments.Count == 0)
			return baseUri;

		var path = string.Join('/', _segments);

		return baseUri.Length == 0
			? path
			: $"{baseUri.TrimEnd('/')}/{path}";
	}

	private static IEnumerable<string> EncodeSegment(string segment)
		=> segment
			.Split('/', StringSplitOptions.RemoveEmptyEntries)
			.Select(Uri.EscapeDataString);

	private static string Format(object value)
		=> value is IFormattable formattable
			? formattable.ToString(null, CultureInfo.InvariantCulture)
			: value.ToString() ?? string.Empty;
}
