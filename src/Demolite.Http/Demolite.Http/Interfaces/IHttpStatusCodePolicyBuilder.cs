﻿using System.Net;
using Demolite.Http.Common;

namespace Demolite.Http.Interfaces;

/// <summary>
/// Interface for HttpStatusCodePolicy Builder
/// </summary>
public interface IHttpStatusCodePolicyBuilder
{
    public IHttpStatusCodePolicyBuilder WithCustomRetryCodes(params HttpStatusCode[] retryCodes);

    public IHttpStatusCodePolicyBuilder WithCustomNonRetryCodes(params HttpStatusCode[] nonRetryCodes);

    public IHttpStatusCodePolicyBuilder WithTooManyRequests();

    public IHttpStatusCodePolicyBuilder WithServerErrors();

    public IHttpStatusCodePolicyBuilder WithTimeouts();

    public IHttpStatusCodePolicyBuilder ExcludeUnauthorized();

    public IHttpStatusCodePolicyBuilder ExcludeNotFound();

    public IHttpStatusCodePolicyBuilder ExcludeBadRequest();

    public HttpStatusCodePolicy Build();
}