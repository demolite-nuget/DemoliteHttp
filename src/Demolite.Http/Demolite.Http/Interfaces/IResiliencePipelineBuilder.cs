using Flurl.Http;
using Polly;

namespace Demolite.Http.Interfaces;
/// <summary>
/// Interface for the base Resiliencepipelinebuilder
/// </summary>
public interface IResiliencePipelineBuilder
{
    /// <summary>
    /// Preassembles a pipeline for GET requests. 
    /// If no specific pipeline is implemented, the default pipeline is used
    /// </summary>
    /// <typeparam name="TD">Type of response model</typeparam>
    IResiliencePipelineBuilder ForGetRequests<TD>()
        where TD : class;
    
    /// <summary>
    /// Preassembles a pipeline for POST requests. 
    /// If no specific pipeline is implemented, the default pipeline is used
    /// </summary>
    /// <typeparam name="TD">Type of response model</typeparam>
    IResiliencePipelineBuilder ForPostRequests<TD>()
        where TD : class;
    
    /// <summary>
    /// Preassembles a pipeline for PUT requests. 
    /// If no specific pipeline is implemented, the default pipeline is used
    /// </summary>
    /// <typeparam name="TD">Type of response model</typeparam>
    IResiliencePipelineBuilder ForPutRequests<TD>()
        where TD : class;
    
    /// <summary>
    /// Preassembles a pipeline for DELETE requests. 
    /// If no specific pipeline is implemented, the default pipeline is used
    /// </summary>
    /// <typeparam name="TD">Type of response model</typeparam>
    IResiliencePipelineBuilder ForDeleteRequests<TD>()
        where TD : class;

    /// <summary>
    /// Preassembles a pipeline for PATCH requests. 
    /// If no specific pipeline is implemented, the default pipeline is used
    /// </summary>
    /// <typeparam name="TD">Type of response model</typeparam>
    IResiliencePipelineBuilder ForPatchRequests<TD>()
        where TD : class;

    /// <summary>
    /// Assembles and returns a pipeline of Type IFlurlResponse
    /// </summary>
    ResiliencePipeline<IFlurlResponse> Build();
}