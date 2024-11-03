namespace Demolite.Http.Common;

/// <summary>
/// HttpStatusCodePolicy
/// </summary>
/// <param name="retryCodes"></param>
/// <param name="nonRetryCodes"></param>
public class HttpStatusCodePolicy(List<int> retryCodes, List<int> nonRetryCodes)
{
    /// <summary>
    /// Returns true based on policy specified in buider.
    /// </summary>
    /// <param name="httpStatusCode"></param>
    /// <returns></returns>
    public bool ShouldRetry(int httpStatusCode)
    {
        return !nonRetryCodes.Contains(httpStatusCode) && retryCodes.Contains(httpStatusCode);
    }
}