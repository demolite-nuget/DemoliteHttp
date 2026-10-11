using System.Globalization;
using Demolite.Http.Requests;

namespace Demolite.Http.Tests.Requests;

[TestClass]
public class RequestUriTests
{
	private const string BaseUri = "https://www.example.com/";

	private RequestUri _testUri = null!;

	[TestInitialize]
	public void TestSetup()
	{
		_testUri = new RequestUri(BaseUri);
	}

	[TestMethod]
	public void TestUri_UriBuiltCorrectly()
	{
		Assert.AreEqual(BaseUri, _testUri.Build());
	}

	[TestMethod]
	public void TestUri_TrailingSlash_UriBuiltCorrectly()
	{
		_testUri = new RequestUri("https://www.example.com", trailingSlash: true);

		Assert.AreEqual("https://www.example.com/", _testUri.Build());
	}
	
	[TestMethod]
	public void TestUri_WithSegments_UriBuiltCorrectly()
	{
		_testUri.WithSegments("api", "v1", "test");

		Assert.AreEqual($"{BaseUri}api/v1/test", _testUri.Build());
	}

	[TestMethod]
	public void TestUri_TrailingSlash_WithSegments_UriBuiltCorrectly()
	{
		_testUri = new RequestUri("https://www.example.com", trailingSlash: true);
		_testUri.WithSegments("api", "v1", "test");

		Assert.AreEqual("https://www.example.com/api/v1/test/", _testUri.Build());
	}

	[TestMethod]
	public void TestUri_WithParameter_UriBuiltCorrectly()
	{
		_testUri.WithParameter("version", "1");

		Assert.AreEqual($"{BaseUri}?version=1", _testUri.Build());
	}

	[TestMethod]
	public void TestUri_WithParameters_UriBuiltCorrectly()
	{
		_testUri.WithParameter("version", "1");
		_testUri.WithParameter("action", "2");

		Assert.AreEqual($"{BaseUri}?version=1&action=2", _testUri.Build());
	}

	[TestMethod]
	public void TestUri_WithParametersDict_UriBuiltCorrectly()
	{
		var parameters = new Dictionary<string, string>()
		{
			{ "version", "1" },
			{ "action", "2" },
		};

		_testUri.WithParameters(parameters);

		Assert.AreEqual($"{BaseUri}?version=1&action=2", _testUri.Build());
	}

	[TestMethod]
	public void TestUri_WithSegmentsAndParameters_UriBuiltCorrectly()
	{
		_testUri.WithSegments("api", "v1", "test");
		_testUri.WithParameter("version", "1");
		_testUri.WithParameter("action", "2");

		Assert.AreEqual($"{BaseUri}api/v1/test?version=1&action=2", _testUri.Build());
	}

	[TestMethod]
	public void TestUri_WithSegmentsAndParameterDict_UriBuiltCorrectly()
	{
		_testUri.WithSegments("api", "v1", "test");

		var parameters = new Dictionary<string, string>()
		{
			{ "version", "1" },
			{ "action", "2" },
		};

		_testUri.WithParameters(parameters);

		Assert.AreEqual($"{BaseUri}api/v1/test?version=1&action=2", _testUri.Build());
	}

	[TestMethod]
	public void TestUri_SegmentsAreEncodedAndSlashesTrimmed()
	{
		_testUri.WithSegments("/api/", "elf lord", "");

		Assert.AreEqual($"{BaseUri}api/elf%20lord", _testUri.Build());
	}

	[TestMethod]
	public void TestUri_SegmentWithSlashes_IsSplit()
	{
		_testUri.WithSegments("api/v1", "test");

		Assert.AreEqual($"{BaseUri}api/v1/test", _testUri.Build());
	}

	[TestMethod]
	public void TestUri_ParametersAreEncoded()
	{
		_testUri.WithParameter("na me", "a&b=c");

		Assert.AreEqual($"{BaseUri}?na%20me=a%26b%3Dc", _testUri.Build());
	}

	[TestMethod]
	public void TestUri_ParameterValues_UseInvariantCulture()
	{
		var current = CultureInfo.CurrentCulture;
		CultureInfo.CurrentCulture = new CultureInfo("de-DE");

		try
		{
			_testUri.WithParameter("lat", 52.5);
			_testUri.WithParameter("lng", 13.25m);

			Assert.AreEqual($"{BaseUri}?lat=52.5&lng=13.25", _testUri.Build());
		}
		finally
		{
			CultureInfo.CurrentCulture = current;
		}
	}

	[TestMethod]
	public void TestUri_NullParameterValue_IsSkipped()
	{
		_testUri.WithParameter("version", null);

		Assert.AreEqual(BaseUri, _testUri.Build());
	}

	[TestMethod]
	public void TestUri_DuplicateParameterNames_AreKept()
	{
		_testUri.WithParameter("tag", "elf");
		_testUri.WithParameter("tag", "orc");

		Assert.AreEqual($"{BaseUri}?tag=elf&tag=orc", _testUri.Build());
	}

	[TestMethod]
	public void TestUri_EmptyBase_BuildsRelativeUri()
	{
		var relative = new RequestUri("");
		relative.WithSegments("organizers", "elves");
		relative.WithParameter("page", 2);

		Assert.AreEqual("organizers/elves?page=2", relative.Build());
	}

	[TestMethod]
	public void TestUri_ImplicitStringConversion_BuildsUri()
	{
		var requestUri = new RequestUri(BaseUri);
		requestUri.WithSegments("api");
		string uri = requestUri;

		Assert.AreEqual($"{BaseUri}api", uri);
	}
}
