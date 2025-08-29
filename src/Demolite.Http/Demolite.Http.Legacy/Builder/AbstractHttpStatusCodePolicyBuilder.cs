using System.Net;
using Demolite.Http.Legacy.Common;
using Demolite.Http.Legacy.Interfaces;

namespace Demolite.Http.Legacy.Builder;

public abstract class AbstractHttpStatusCodePolicyBuilder : IHttpStatusCodePolicyBuilder
{
    private readonly List<int> _retryCodes = [];

    private readonly List<int> _nonRetryCodes = [];


    public virtual IHttpStatusCodePolicyBuilder WithCustomRetryCodes(params HttpStatusCode[] retryCodes)
    {
        foreach (var retryCode in retryCodes)
        {
            _retryCodes.Add((int)retryCode);
        }

        return this;
    }

    public virtual IHttpStatusCodePolicyBuilder WithCustomNonRetryCodes(params HttpStatusCode[] nonRetryCodes)
    {
        foreach (var nonRetryCode in nonRetryCodes)
        {
            _retryCodes.Add((int)nonRetryCode);
        }

        return this;
    }

    public virtual IHttpStatusCodePolicyBuilder WithTooManyRequests()
    {
        _retryCodes.Add((int)HttpStatusCode.TooManyRequests);
        return this;
    }

    public virtual IHttpStatusCodePolicyBuilder WithServerErrors()
    {
        _retryCodes.Add((int)HttpStatusCode.InternalServerError);
        _retryCodes.Add((int)HttpStatusCode.BadGateway);
        _retryCodes.Add((int)HttpStatusCode.ServiceUnavailable);
        _retryCodes.Add((int)HttpStatusCode.GatewayTimeout);
        return this;
    }

    public virtual IHttpStatusCodePolicyBuilder WithTimeouts()
    {
        _retryCodes.Add((int)HttpStatusCode.RequestTimeout);
        return this;
    }

    public virtual IHttpStatusCodePolicyBuilder ExcludeUnauthorized()
    {
        _nonRetryCodes.Add((int)HttpStatusCode.Unauthorized);
        return this;
    }

    public virtual IHttpStatusCodePolicyBuilder ExcludeNotFound()
    {
        _nonRetryCodes.Add((int)HttpStatusCode.NotFound);
        return this;
    }

    public virtual IHttpStatusCodePolicyBuilder ExcludeBadRequest()
    {
        _nonRetryCodes.Add((int)HttpStatusCode.BadRequest);
        return this;
    }

    public HttpStatusCodePolicy Build() => new(_retryCodes, _nonRetryCodes);
}