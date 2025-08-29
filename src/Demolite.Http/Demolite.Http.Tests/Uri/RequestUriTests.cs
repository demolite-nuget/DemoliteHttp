using Demolite.Http.Uri;

namespace Demolite.Http.Tests.Uri;

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
}