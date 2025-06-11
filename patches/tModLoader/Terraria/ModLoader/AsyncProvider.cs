using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

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

	/**
	 * <remarks>
	 *   Remember to provide your enumerator with
	 *   `[EnumeratorCancellation] CancellationToken token = default`
	 *   as argument to allow cancellation notification.
	 * </remarks>
	 */
	public AsyncProvider(IAsyncEnumerable<T> provider) {
		_Channel = Channel.CreateUnbounded<T>();
		TokenSource = new CancellationTokenSource();
		var taskRunner = async () => {
			var writer = _Channel.Writer;
			try {
				await foreach (var item in provider.WithCancellation(TokenSource.Token)) {
					await writer.WriteAsync(item);
				}
				writer.Complete();
			}
			catch (Exception ex) {
				writer.Complete(ex);
			}
		};
		// No need to store the task, the completion event is present in the channel itself
		taskRunner();
	}

	public void Cancel()
	{
		TokenSource.Cancel();
	}

	public bool IsCancellationRequested => TokenSource.IsCancellationRequested;
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
	public Exception Exception => _Channel.Reader.Completion.Exception;

	public IEnumerable<T> GetData()
	{
		while (_Channel.Reader.TryRead(out T item)) {
			yield return item;
		}
	}

	//protected abstract Task Run(ChannelWriter<T> writer, CancellationToken token);
}
