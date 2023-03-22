using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis;

namespace Terraria.ModLoader;

public static class AsyncProvider
{
	public enum State
	{
		NotStarted,
		Loading,
		Completed,
		Aborted
	}

	public class Empty<T> : IAsyncProvider<T>
	{
		public void Start(CancellationToken token) { }

		public IEnumerable<T> GetData() => Enumerable.Empty<T>();
		public int Count => 0;
		public AsyncProvider.State State => AsyncProvider.State.Completed;
		public bool HasNewData => false;
	}
}

// Only interfaces can be variant/covariant, so let's make an interface
// IList is NOT vairant... :( <out T> no like
public interface IAsyncProvider<out T>
{
	public void Start(CancellationToken token);
	public IEnumerable<T> GetData();
	public int Count { get; }
	public AsyncProvider.State State { get; }
	public bool HasNewData { get; }
}

/*
// This made sense when GetData returned a List
public class AsyncProviderCasted<T, ST> where ST : T, IAsyncProvider<T>
{
	private IAsyncProvider<ST> _provider;
	public AsyncProviderCasted(IAsyncProvider<ST> provider)
	{
		_provider = provider;
	}

	public void Start(CancellationToken token) => _provider.Start(token);

	public IEnumerable<T> GetData() => _provider.GetData().Cast<T>();
	public int Count => _provider.Count;
	public AsyncProvider.State State => _provider.State;
	public bool HasNewData => _provider.HasNewData;
}
*/

public abstract class AsyncProvider<T> : IAsyncProvider<T>
{
	protected abstract Task<bool> Run(CancellationToken token);
	public virtual void Start(CancellationToken token)
	{
		State = AsyncProvider.State.Loading;
		Task.Run(async () => {
			try {
				if (await this.Run(token))
					State = AsyncProvider.State.Completed;
			}
			/*
			catch (OperationCanceledException) {
			}
			*/
			finally {
				if (State == AsyncProvider.State.Loading)
					State = AsyncProvider.State.Aborted;
			}
		});
	}
	protected List<T> _Data = new();
	public IEnumerable<T> GetData()
	{
		List<T> copy;
		lock (this) {
			HasNewData = false;
			copy = _Data.ToList(); // Make a copy to free the lock
		}
		return copy;
	}

	public int Count => _Data.Count; // @TODO: Is this atomic or need lock?
	public AsyncProvider.State State { get; protected set; } = AsyncProvider.State.NotStarted;
	public bool HasNewData { get; protected set; } = false;
}
