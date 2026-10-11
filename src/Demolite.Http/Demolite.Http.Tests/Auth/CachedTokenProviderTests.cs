using Demolite.Core.Models;
using Demolite.Http.Auth;
using Demolite.Http.Tests.Helpers;

namespace Demolite.Http.Tests.Auth;

[TestClass]
public class CachedTokenProviderTests
{
	private ManualTimeProvider _time = null!;
	private TestTokenProvider _sut = null!;

	public TestContext TestContext { get; set; } = null!;

	private CancellationToken Ct => TestContext.CancellationToken;

	[TestInitialize]
	public void TestInitialize()
	{
		_time = new ManualTimeProvider();
		_sut = new TestTokenProvider(_time);
	}

	[TestMethod]
	public async Task GetHeaderAsync_ReturnsBearerToken()
	{
		var result = await _sut.GetHeaderAsync(Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("Bearer", result.Value!.Scheme);
		Assert.AreEqual("token-1", result.Value.Parameter);
	}

	[TestMethod]
	public async Task GetHeaderAsync_ReusesToken_UntilItNearsExpiry()
	{
		await _sut.GetHeaderAsync(Ct);
		_time.Advance(TimeSpan.FromMinutes(3));
		var result = await _sut.GetHeaderAsync(Ct);

		Assert.AreEqual(1, _sut.Requests);
		Assert.AreEqual("token-1", result.Value!.Parameter);
	}

	[TestMethod]
	public async Task GetHeaderAsync_RenewsToken_WithinRenewalMargin()
	{
		await _sut.GetHeaderAsync(Ct);
		_time.Advance(TimeSpan.FromMinutes(4));
		var result = await _sut.GetHeaderAsync(Ct);

		Assert.AreEqual(2, _sut.Requests);
		Assert.AreEqual("token-2", result.Value!.Parameter);
	}

	[TestMethod]
	public async Task GetHeaderAsync_SharesOneRequest_BetweenConcurrentCallers()
	{
		_sut.Gate = new TaskCompletionSource();

		var calls = Enumerable.Range(0, 10).Select(_ => _sut.GetHeaderAsync(Ct).AsTask()).ToArray();
		_sut.Gate.SetResult();
		var results = await Task.WhenAll(calls);

		Assert.AreEqual(1, _sut.Requests);
		Assert.IsTrue(results.All(r => r.Value!.Parameter == "token-1"));
	}

	[TestMethod]
	public async Task GetHeaderAsync_ReturnsError_AndDoesNotCacheIt()
	{
		_sut.FailNext = true;

		var failed = await _sut.GetHeaderAsync(Ct);
		var succeeded = await _sut.GetHeaderAsync(Ct);

		Assert.IsFalse(failed.IsSuccess);
		Assert.AreEqual("Auth.Failed", failed.Error!.Code);
		Assert.IsTrue(succeeded.IsSuccess);
	}

	[TestMethod]
	public async Task Invalidate_ForcesNewToken()
	{
		await _sut.GetHeaderAsync(Ct);
		_sut.Invalidate();
		var result = await _sut.GetHeaderAsync(Ct);

		Assert.AreEqual("token-2", result.Value!.Parameter);
	}

	private sealed class TestTokenProvider(TimeProvider timeProvider) : CachedTokenProvider(timeProvider: timeProvider)
	{
		private int _requests;

		public int Requests => _requests;

		public TaskCompletionSource? Gate { get; set; }

		public bool FailNext { get; set; }

		protected override async Task<Result<AccessToken>> RequestTokenAsync(CancellationToken ct)
		{
			var number = Interlocked.Increment(ref _requests);

			if (Gate is not null)
				await Gate.Task;

			if (FailNext)
			{
				FailNext = false;
				return Result<AccessToken>.Failure(new Error("Auth.Failed", "identity service unavailable"));
			}

			return Result<AccessToken>.Success(new AccessToken($"token-{number}", TimeSpan.FromMinutes(5)));
		}
	}
}
