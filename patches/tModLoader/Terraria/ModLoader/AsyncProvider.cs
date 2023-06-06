using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Terraria.ModLoader;

public static class AsyncProviderStateExtensions
{
	public static bool IsFinished(this AsyncProvider.State s) => (s == AsyncProvider.State.Completed || s == AsyncProvider.State.Aborted);
};

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

		public IEnumerable<T> GetData(bool clearHasNewData) => Enumerable.Empty<T>();
		public int Count => 0;
		public AsyncProvider.State State => AsyncProvider.State.Completed;
		public bool HasNewData => false;
	}
}

public static class AsyncProviderExtensions
{
	private class AsyncProviderSelect<T, U> : IAsyncProvider<U>
	{
		private IAsyncProvider<T> _provider;
		private Func<T, U> _converter;
		public AsyncProviderSelect(IAsyncProvider<T> provider, Func<T, U> converter)
		{
			_provider = provider;
			_converter = converter;
		}

		public void Start(CancellationToken token) => _provider.Start(token);

		public IEnumerable<U> GetData(bool clearHasNewData) => _provider.GetData(clearHasNewData).Select(_converter);
		public int Count => _provider.Count;
		public AsyncProvider.State State => _provider.State;
		public bool HasNewData => _provider.HasNewData;
	}

	public static IAsyncProvider<U> Select<T, U>(this IAsyncProvider<T> provider, Func<T, U> converter)
	{
		return new AsyncProviderSelect<T, U>(provider, converter);
	}
}

// Only interfaces can be variant/covariant, so let's make an interface
// IList is NOT vairant... :( <out T> no like
public interface IAsyncProvider<out T>
{
	public void Start(CancellationToken token);
	public IEnumerable<T> GetData(bool clearHasNewData);
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
			// @TODO: from Chicken Bones: should catch cancellation and both make error true and
			// avoid hitting the thread pool, correct, you can have a full list of exceptions
			// and all should bubble (hopefully being intercepted and logged) except cancellations
			catch (OperationCanceledException) {
			}
			finally {
				if (State == AsyncProvider.State.Loading)
					State = AsyncProvider.State.Aborted;
			}
		});
	}
	protected List<T> _Data = new();
	public IEnumerable<T> GetData(bool clearHasNewData)
	{
		List<T> copy;
		lock (this) {
			if (clearHasNewData)
				HasNewData = false;
			copy = _Data.ToList(); // Make a copy to free the lock
		}
		return copy;
	}

	public int Count {
		get {
			// https://stackoverflow.com/questions/24206623/is-listt-count-thread-safe
			// Should not be a problem in current implementation, but lock for safety
			lock (this) {
				return _Data.Count;
			}
		}
	}
	public AsyncProvider.State State { get; protected set; } = AsyncProvider.State.NotStarted;
	public bool HasNewData { get; protected set; } = false;
}
