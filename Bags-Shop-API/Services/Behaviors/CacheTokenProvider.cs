namespace Bags_Shop_API.Services.Behaviors
{
	public class CacheTokenProvider: ICacheTokenProvider
    {
        private CancellationTokenSource _cts = new();

        public CancellationToken Token {  get { return _cts.Token; } }
        public void Reset()
        {
            _cts.Cancel();     
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

	  
	}

}
