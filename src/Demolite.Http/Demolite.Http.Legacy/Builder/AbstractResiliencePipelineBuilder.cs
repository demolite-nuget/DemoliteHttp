using System.Net;
using Demolite.Http.Legacy.Common;
using Demolite.Http.Legacy.Interfaces;
using Flurl.Http;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Serilog;

namespace Demolite.Http.Legacy.Builder;

/// <summary>
/// Abstract implementation of the IResiliencePipelineBuilder interface
/// </summary>
public abstract class AbstractResiliencePipelineBuilder : IResiliencePipelineBuilder
{
    private readonly ResiliencePipelineBuilder<IFlurlResponse> _pipelineBuilder = new();

    /// <summary>
    /// StatusCode Policy builder.
    /// Can be set null if not intended to be used.
    /// </summary>
    protected abstract IHttpStatusCodePolicyBuilder HttpStatusCodePolicyBuilder { get; }

    public virtual IResiliencePipelineBuilder ForGetRequests()
        => DefaultPipeline();

    public virtual IResiliencePipelineBuilder ForPostRequests()
        => DefaultPipeline();

    public virtual IResiliencePipelineBuilder ForPutRequests()
        => DefaultPipeline();

    public virtual IResiliencePipelineBuilder ForDeleteRequests()
        => DefaultPipeline();

    public virtual IResiliencePipelineBuilder ForPatchRequests() 
        => DefaultPipeline();


    public ResiliencePipeline<IFlurlResponse> Build()
    {
        return _pipelineBuilder.Build();
    }

    protected abstract IResiliencePipelineBuilder DefaultPipeline();


    /// <summary>
    /// Adds a retry strategy to pipeline.
    /// </summary>
    /// <param name="retryAttempts">Amount of times to retry</param>
    /// <param name="delayBackoffType">Backoff type for the delay</param>
    /// <param name="delay">Delay (Time) until next retry</param>
    /// <param name="statusCodePolicy">Which StatusCodes are valid/not valid for retry</param>
    /// <typeparam name="T">ResponseModel Type</typeparam>
    protected IResiliencePipelineBuilder WithRetry<T>(
        int retryAttempts,
        DelayBackoffType? delayBackoffType = null,
        TimeSpan? delay = null,
        HttpStatusCodePolicy? statusCodePolicy = null)
    {
        var retryOptions = new RetryStrategyOptions<IFlurlResponse>
        {
            ShouldHandle = new PredicateBuilder<IFlurlResponse>().HandleResult(res =>
                statusCodePolicy?.ShouldRetry(res.StatusCode) ?? !res.ResponseMessage.IsSuccessStatusCode),
            MaxRetryAttempts = retryAttempts,
            Delay = delay ?? TimeSpan.FromMilliseconds(100),
            BackoffType = delayBackoffType ?? DelayBackoffType.Constant,

            OnRetry = args =>
            {
                var typeName = "Unknown";

                if (args.Outcome.Result is IHttpResponse<T> response)
                {
                    typeName = response.Object?.GetType().Name ?? typeof(T).Name;
                }

                Log.Debug("Retry attempt: {AttemptNumber} | Type: {TypeName} | Message: {ExceptionMessage}",
                    args.AttemptNumber,
                    typeName,
                    args.Outcome.Exception?.Message);
                return default;
            }
        };

        _pipelineBuilder.AddRetry(retryOptions);

        return this;
    }

    /// <summary>
    /// Adds timeout of specified time to pipeline.
    /// </summary>
    /// <param name="timeout">Time until request should time out</param>
    /// <returns></returns>
    protected IResiliencePipelineBuilder WithTimeout(TimeSpan timeout)
    {
        var timeoutOptions = new TimeoutStrategyOptions
        {
            Timeout = timeout,
            OnTimeout = args =>
            {
                Log.Warning("Request timed out after {Timeout}ms", args.Timeout.TotalMilliseconds);
                return default;
            }
        };
        _pipelineBuilder.AddTimeout(timeoutOptions);
        return this;
    }

    /// <summary>
    /// Adds circuit breaker strategy to pipeline.
    /// </summary>
    /// <param name="failureRatio">The failure to success ratio at which the circuit will break</param>
    /// <param name="minimumThroughput">Amount of actions that must pass through before circuit breakter takes action</param>
    /// <param name="samplingDuration">Timeframe within which failure ratios are assessed</param>
    /// <param name="breakDuration">Duration of open circuit</param>
    /// <param name="statusCodePolicy">Which StatusCodes are valid/not valid for circuit to break</param>
    protected IResiliencePipelineBuilder WithCircuitBreaker(
        double failureRatio = 0.5,
        int minimumThroughput = 10,
        TimeSpan? samplingDuration = null,
        TimeSpan? breakDuration = null,
        HttpStatusCodePolicy? statusCodePolicy = null)
    {
        var circuitBreaker = new CircuitBreakerStrategyOptions<IFlurlResponse>()
        {
            ShouldHandle = new PredicateBuilder<IFlurlResponse>().HandleResult(res =>
                statusCodePolicy?.ShouldRetry(res.StatusCode) ?? !res.ResponseMessage.IsSuccessStatusCode),
            FailureRatio = failureRatio,
            MinimumThroughput = minimumThroughput,
            SamplingDuration = samplingDuration ?? TimeSpan.FromSeconds(30),
            BreakDuration = breakDuration ?? TimeSpan.FromSeconds(5),
            OnOpened = args =>
            {
                var statusCode = (HttpStatusCode)args.Outcome.Result.StatusCode;

                Log.Warning(
                    "Breaker logging: Breaking the circuit for {args.BreakDuration.TotalMilliseconds}ms ..due to: {StatusCode} ",
                    args.BreakDuration.TotalMilliseconds, statusCode);
                return default;
            },
            OnClosed = args =>
            {
                Log.Information("Breaker logging: Call OK! Closed the circuit again!");
                return default;
            },
            OnHalfOpened = args =>
            {
                Log.Information("Breaker logging: Half-open: Next call is a trial!");
                return default;
            }
        };

        _pipelineBuilder.AddCircuitBreaker(circuitBreaker);
        return this;
    }
}