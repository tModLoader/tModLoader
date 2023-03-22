using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
}

public abstract class AsyncProvider<T>
{
	public class Empty : AsyncProvider<T>
	{
		protected override Task<bool> Run(CancellationToken token)
		{
			return Task.FromResult(true);
		}
	}

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
	public List<T> GetData()
	{
		lock (this) {
			HasNewData = false;
			return _Data.ToList(); // Make a copy
		}
	}
	public int Count => _Data.Count; // @TODO: Is this atomic or need lock?
	public AsyncProvider.State State { get; protected set; } = AsyncProvider.State.NotStarted;
	public bool HasNewData { get; protected set; } = false;
}
