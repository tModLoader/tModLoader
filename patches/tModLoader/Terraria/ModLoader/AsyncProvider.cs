using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Iced.Intel;

namespace Terraria.ModLoader;

public static class AsyncProviderStateExtensions
{
	public static bool IsFinished(this AsyncProviderState s) => (
		s == AsyncProviderState.Completed ||
		s == AsyncProviderState.Canceled ||
		s == AsyncProviderState.Aborted
	);
};

public enum AsyncProviderState {
	Loading,
	Completed,
	Canceled,
	Aborted
}

public class AsyncProvider<T>
{
	private Channel<T> _Channel;
	private CancellationTokenSource TokenSource;

	public AsyncProvider(Func<ChannelWriter<T>, CancellationToken, Task> task) {
		this._Channel = Channel.CreateUnbounded<T>();
		TokenSource = new CancellationTokenSource();
		// No need to store the task, the completion event is present in the channel itself
		Task.Run(async () => {
			var writer = this._Channel.Writer;
			Exception ex = null;
			try {
				await task(writer, this.TokenSource.Token);
			}
			catch (Exception _ex) {
				ex = _ex;
			}
			finally {
				writer.TryComplete(ex); // Accept the runner to close the channel beforehand
			}
		}, this.TokenSource.Token);
	}

	public void Cancel()
	{
		this.TokenSource.Cancel();
	}

	public bool IsCancellationRequested => this.TokenSource.IsCancellationRequested;
	public AsyncProviderState State {
		get {
			var completion = _Channel.Reader.Completion;
			if (!completion.IsCompleted)
				return AsyncProviderState.Loading;
			if (completion.IsCanceled)
				return AsyncProviderState.Canceled;
			if (completion.IsFaulted)
				return AsyncProviderState.Aborted;
			return AsyncProviderState.Completed;
		}
	}
	public Exception Exception => this._Channel.Reader.Completion.Exception;

	public IEnumerable<T> GetData()
	{
		T item;
		while (this._Channel.Reader.TryRead(out item)) {
			yield return item;
		}
	}

	//protected abstract Task Run(ChannelWriter<T> writer, CancellationToken token);
}
