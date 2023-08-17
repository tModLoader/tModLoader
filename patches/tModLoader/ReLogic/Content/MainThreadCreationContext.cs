using System;
using System.Runtime.CompilerServices;

namespace ReLogic.Content;

public readonly struct MainThreadCreationContext : INotifyCompletion
{
	private readonly AssetRepository.ContinuationScheduler _continuationScheduler;

	internal MainThreadCreationContext(AssetRepository.ContinuationScheduler continuationScheduler)
	{
		_continuationScheduler = continuationScheduler;
	}

	public MainThreadCreationContext GetAwaiter() => this;
	public void OnCompleted(Action action) => _continuationScheduler.OnCompleted(action);
	public bool IsCompleted => AssetRepository.IsMainThread;
	public void GetResult() { }
}
