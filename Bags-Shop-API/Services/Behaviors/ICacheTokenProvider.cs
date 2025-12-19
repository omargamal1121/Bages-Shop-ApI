namespace Bags_Shop_API.Services.Behaviors
{
	public interface ICacheTokenProvider
    {
        public void Reset();
       public CancellationToken Token { get; }

    }

}
