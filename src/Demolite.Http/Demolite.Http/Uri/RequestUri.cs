using System.Text;
using System.Text.RegularExpressions;
using Demolite.Http.Interfaces;

namespace Demolite.Http.Uri;

public class RequestUri(string baseUri, bool trailingSlash = false) : IRequestUri
{
	private List<string> UriSegments { get; } = [];

	private Dictionary<string, string> UriParameters { get; } = [];

	public IRequestUri WithSegments(params string[] segments)
	{
		UriSegments.AddRange(segments);
		return this;
	}

	public IRequestUri WithParameter(string name, string value)
	{
		UriParameters.Add(name, value);
		return this;
	}

	public IRequestUri WithParameters(Dictionary<string, string> parameters)
	{
		foreach (var parameter in parameters)
			UriParameters.Add(parameter.Key, parameter.Value);
		
		return this;
	}

	public string Build()
	{
		var sb = new StringBuilder(baseUri);
		
		// create a separating slash between the base uri and the parameters if there are any
		// and there is not yet a slash at the end
		if (UriSegments.Count > 0 && !baseUri.EndsWith('/'))
			sb.Append('/');
			
		// append the segments
		if (UriSegments.Count > 0)
			sb.Append(string.Join("/", UriSegments));

		// If a trailing uri / parameter slash is requested
		// and the last character in the StringBuilder is not a slash, append it
		if (trailingSlash && sb[^1] != '/')
			sb.Append('/');

		// Append parameters if there are any.
		if (UriParameters.Count > 0) 
			sb.Append($"?{string.Join("&", UriParameters.Select(x => $"{x.Key}={x.Value}"))}");
			
		// create a string
		var uri = sb.ToString();

		// In case the user has entered any slashes that are doubled, remove them here.
		// The pattern will match any double slashes that are not preceded by http: or https:
		var re = new Regex("(?<!https{0,1}?:)\\/{2,}");
		
		return re.Replace(uri, "/");
	}
}