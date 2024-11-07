using Demolite.Http.Enum;
using Demolite.Http.Interfaces;
using Flurl.Http;
using Polly;

namespace Demolite.Http.Repository;

public abstract partial class AbstractHttpRepository<TPb>
{
    /// <summary>
    /// Default resilience pipeline builder.
    /// Can be set null if not intended to be used.
    /// </summary>
    protected virtual IResiliencePipelineBuilder? ResiliencePipelineBuilder => null;

    /// <summary>
    /// Holds pipelines if any are created.
    /// </summary>
    protected Dictionary<RequestType, ResiliencePipeline<IFlurlResponse>> ResiliencePipelines = [];

    /// <summary>
    ///     Setup method used to prepare the Flurl Client
    /// </summary>
    protected abstract void SetupClient();

    /// <summary>
    /// Setup method to create ResiliencePipelines dictionary.
    /// If a ResiliencePipelineBuilder, it will create the pipelines for the respective request type.
    /// Returns an empty dictionary if ResiliencePipelineBuilder is null. 
    /// </summary>
    protected virtual void SetupPipelines()
    {
        if (ResiliencePipelineBuilder == null) return;

        var getPipeline = ResiliencePipelineBuilder.ForGetRequests().Build();
        ResiliencePipelines.Add(RequestType.Get, getPipeline);

        var putPipeline = ResiliencePipelineBuilder.ForPutRequests().Build();
        ResiliencePipelines.Add(RequestType.Put, putPipeline);

        var patchPipeline = ResiliencePipelineBuilder.ForPatchRequests().Build();
        ResiliencePipelines.Add(RequestType.Patch, patchPipeline);

        var deletePipeline = ResiliencePipelineBuilder.ForDeleteRequests().Build();
        ResiliencePipelines.Add(RequestType.Delete, deletePipeline);

        var postPipeline = ResiliencePipelineBuilder.ForPostRequests().Build();
        ResiliencePipelines.Add(RequestType.Post, postPipeline);
    }

    /// <summary>
    /// Returns the request type specific pipeline or an empty pipeline. 
    /// </summary>
    /// <param name="requestType"></param>
    /// <returns></returns>
    protected ResiliencePipeline<IFlurlResponse> GetPipeline(RequestType requestType)
    {
        return ResiliencePipelines.TryGetValue(requestType, out var pipeline)
            ? pipeline
            : ResiliencePipeline<IFlurlResponse>.Empty;
    }
}