using Flurl.Http;
using Polly;

namespace Demolite.Http.Legacy.Interfaces;
/// <summary>
/// Interface for the base Resiliencepipelinebuilder
/// </summary>
public interface IResiliencePipelineBuilder
{
    /// <summary>
    /// Preassembles a pipeline for GET requests. 
    /// If no specific pipeline is implemented, the default pipeline is used
    /// </summary>
    IResiliencePipelineBuilder ForGetRequests();
    
    /// <summary>
    /// Preassembles a pipeline for POST requests. 
    /// If no specific pipeline is implemented, the default pipeline is used
    /// </summary>
    IResiliencePipelineBuilder ForPostRequests();
    
    /// <summary>
    /// Preassembles a pipeline for PUT requests. 
    /// If no specific pipeline is implemented, the default pipeline is used
    /// </summary>
    IResiliencePipelineBuilder ForPutRequests();
    
    /// <summary>
    /// Preassembles a pipeline for DELETE requests. 
    /// If no specific pipeline is implemented, the default pipeline is used
    /// </summary>
    IResiliencePipelineBuilder ForDeleteRequests();


    /// <summary>
    /// Preassembles a pipeline for PATCH requests. 
    /// If no specific pipeline is implemented, the default pipeline is used
    /// </summary>
    IResiliencePipelineBuilder ForPatchRequests();

    /// <summary>
    /// Assembles and returns a pipeline of Type IFlurlResponse
    /// </summary>
    ResiliencePipeline<IFlurlResponse> Build();
}