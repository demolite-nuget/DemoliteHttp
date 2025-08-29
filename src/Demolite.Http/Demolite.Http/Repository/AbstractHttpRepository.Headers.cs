namespace Demolite.Http.Repository;

// ReSharper disable MemberCanBeProtected.Global
public abstract partial class AbstractHttpRepository
{
	public virtual void SetGetHeaders()
	{
		Client.DefaultRequestHeaders.Clear();
	}

	public virtual void SetPostHeaders()
	{
		Client.DefaultRequestHeaders.Clear();
	}

	public virtual void SetPutHeaders()
	{
		Client.DefaultRequestHeaders.Clear();
	}
	
	public virtual void SetPatchHeaders()
	{
		Client.DefaultRequestHeaders.Clear();
	}
	
	public virtual void SetUpdateHeaders()
	{
		Client.DefaultRequestHeaders.Clear();
	}

	public virtual void SetDeleteHeaders()
	{
		Client.DefaultRequestHeaders.Clear();
	}
}