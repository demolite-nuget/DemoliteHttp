using System.Net;
using System.Text.Json;
using Demolite.Http.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace Demolite.Http.Tests.Client;

[TestClass]
public class ApiClientTests
{
	private const string ElfJson = """{"id":1,"name":"Elf"}""";

	public TestContext TestContext { get; set; } = null!;

	private CancellationToken Ct => TestContext.CancellationToken;

	[TestMethod]
	public async Task Get_ReturnsDeserializedValue()
	{
		var handler = FakeHandler.Responding(HttpStatusCode.OK, ElfJson);
		var sut = CreateClient(handler);

		var result = await sut.GetItemAsync("items/1", Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(new Item(1, "Elf"), result.Value);
		Assert.AreEqual("https://api.test/items/1", handler.Requests[0].Uri!.ToString());
	}

	[TestMethod]
	public async Task Get_ReturnsStatusCodeError_OnFailureResponse()
	{
		var sut = CreateClient(FakeHandler.Responding(HttpStatusCode.NotFound, "no such item"));

		var result = await sut.GetItemAsync("items/1", Ct);

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("Http.404", result.Error!.Code);
		Assert.AreEqual("no such item", result.Error.Message);
		Assert.AreEqual(404, result.Error.Metadata!["statusCode"]);
	}

	[TestMethod]
	public async Task Get_UsesReasonPhrase_WhenFailureResponseHasNoBody()
	{
		var sut = CreateClient(FakeHandler.Responding(HttpStatusCode.BadGateway));

		var result = await sut.GetItemAsync("items/1", Ct);

		Assert.AreEqual("Http.502", result.Error!.Code);
		Assert.AreEqual("Bad Gateway", result.Error.Message);
	}

	[TestMethod]
	public async Task Get_ReturnsDefaultValue_WhenResponseIsEmpty()
	{
		var sut = CreateClient(FakeHandler.Responding(HttpStatusCode.NoContent));

		var result = await sut.GetItemAsync("items/1", Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.IsNull(result.Value);
	}

	[TestMethod]
	public async Task Get_ReturnsDeserializationFailed_OnInvalidJson()
	{
		var sut = CreateClient(FakeHandler.Responding(HttpStatusCode.OK, "not json"));

		var result = await sut.GetItemAsync("items/1", Ct);

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("Http.DeserializationFailed", result.Error!.Code);
		Assert.AreEqual("not json", result.Error.Metadata!["rawContent"]);
	}

	[TestMethod]
	public async Task Get_ReturnsEmptyBody_WhenJsonIsNull()
	{
		var sut = CreateClient(FakeHandler.Responding(HttpStatusCode.OK, "null"));

		var result = await sut.GetItemAsync("items/1", Ct);

		Assert.AreEqual("Http.EmptyBody", result.Error!.Code);
	}

	[TestMethod]
	public async Task Send_ReturnsRequestFailed_WhenHttpRequestThrows()
	{
		var sut = CreateClient(FakeHandler.Throwing(new HttpRequestException("connection refused")));

		var result = await sut.GetItemAsync("items/1", Ct);

		Assert.AreEqual("Http.RequestFailed", result.Error!.Code);
		Assert.IsInstanceOfType<HttpRequestException>(result.Error.Exception);
	}

	[TestMethod]
	public async Task Send_ReturnsTimeout_WhenRequestTimesOut()
	{
		var sut = CreateClient(FakeHandler.Throwing(new TaskCanceledException("timed out")));

		var result = await sut.GetItemAsync("items/1", Ct);

		Assert.AreEqual("Http.Timeout", result.Error!.Code);
	}

	[TestMethod]
	public async Task Send_Throws_WhenCallerCancels()
	{
		var sut = CreateClient(FakeHandler.Responding(HttpStatusCode.OK, ElfJson));
		using var cts = new CancellationTokenSource();
		await cts.CancelAsync();

		await Assert.ThrowsAsync<OperationCanceledException>(() => sut.GetItemAsync("items/1", cts.Token));
	}

	[TestMethod]
	public async Task Post_SendsBodyAsJson()
	{
		var handler = FakeHandler.Responding(HttpStatusCode.Created, ElfJson);
		var sut = CreateClient(handler);

		var result = await sut.PostItemAsync("items", new Item(1, "Elf"), Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(HttpMethod.Post, handler.Requests[0].Method);
		Assert.AreEqual("application/json", handler.Requests[0].ContentType);
		Assert.AreEqual(ElfJson, handler.Requests[0].Body);
	}

	[TestMethod]
	public async Task Post_SendsNoBody_WhenBodyIsNull()
	{
		var handler = FakeHandler.Responding(HttpStatusCode.OK, ElfJson);
		var sut = CreateClient(handler);

		await sut.PostItemAsync("items/1/done", null, Ct);

		Assert.IsNull(handler.Requests[0].Body);
	}

	[TestMethod]
	public async Task Post_WithoutResponseType_IgnoresResponseBody()
	{
		var sut = CreateClient(FakeHandler.Responding(HttpStatusCode.OK, "anything"));

		var result = await sut.PostNothingAsync("items/1/done", null, Ct);

		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task Post_WithoutResponseType_ReturnsError_OnFailureResponse()
	{
		var sut = CreateClient(FakeHandler.Responding(HttpStatusCode.Forbidden, "denied"));

		var result = await sut.PostNothingAsync("items/1/done", null, Ct);

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("Http.403", result.Error!.Code);
	}

	[TestMethod]
	public async Task Get_WithoutResponseType_ReturnsSuccess()
	{
		var sut = CreateClient(FakeHandler.Responding(HttpStatusCode.NoContent));

		var result = await sut.GetNothingAsync("ping", Ct);

		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task PutPatchDelete_UseTheirHttpMethod()
	{
		var handler = FakeHandler.Responding(HttpStatusCode.OK, ElfJson);
		var sut = CreateClient(handler);

		await sut.PutItemAsync("items/1", new Item(1, "Elf"));
		await sut.PatchItemAsync("items/1", new Item(1, "Elf"));
		await sut.DeleteItemAsync("items/1");

		CollectionAssert.AreEqual(
			new[] { HttpMethod.Put, HttpMethod.Patch, HttpMethod.Delete },
			handler.Requests.Select(r => r.Method).ToArray());
	}

	[TestMethod]
	public async Task Send_UsesGivenSerializerOptions()
	{
		var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
		var handler = FakeHandler.Responding(HttpStatusCode.OK, """{"id":1,"name":"Elf"}""");
		var sut = CreateClient(handler, options);

		await sut.PostItemAsync("items", new { ItemName = "Elf" }, Ct);

		Assert.AreEqual("""{"item_name":"Elf"}""", handler.Requests[0].Body);
	}

	[TestMethod]
	public async Task CreateRequest_AllowsHeadersForSingleCall()
	{
		var handler = FakeHandler.Responding(HttpStatusCode.OK, ElfJson);
		var sut = CreateClient(handler);

		await sut.GetItemWithHeaderAsync("items/1", "X-Realm", "Elves");

		Assert.AreEqual("Elves", handler.Requests[0].Headers.GetValues("X-Realm").Single());
	}

	[TestMethod]
	public async Task Send_LogsFailedRequests()
	{
		var logger = new ListLogger();
		var sut = CreateClient(FakeHandler.Responding(HttpStatusCode.NotFound, "missing"), logger: logger);

		await sut.GetItemAsync("items/1", Ct);

		Assert.IsTrue(logger.Entries.Any(e => e.Level == LogLevel.Error && e.Message.Contains("404")));
	}

	[TestMethod]
	public async Task Send_DoesNotThrow_WithoutLogger()
	{
		var sut = CreateClient(FakeHandler.Responding(HttpStatusCode.NotFound, "missing"));

		var result = await sut.GetItemAsync("items/1", Ct);

		Assert.IsFalse(result.IsSuccess);
	}

	private static TestApiClient CreateClient(
		FakeHandler handler,
		JsonSerializerOptions? options = null,
		ILogger? logger = null)
		=> new(new HttpClient(handler) { BaseAddress = new Uri("https://api.test/") }, options, logger);
}
