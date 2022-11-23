using ReLogic.Content.Readers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReLogic.Content.Sources;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ReLogic.Content;

/// <summary>
/// <br/> Async loading has been fully integrated into AssetRepository
/// <br/> Assets which are asynchronously loaded will:
///	<br/> 	- be deserialized on the thread pool
///	<br/> 	- return to the main thread if the asset can only be created there (for assets requiring GraphicsDevice)
///	<br/> 	- become loaded at a defined time:
///	<br/> 		- at the end of a frame or
///	<br/> 		- when content sources are changing or
///	<br/> 		- when requested by ImmediateLoad on the main thread
///	<br/> 
/// <br/> Assets which require main thread creation, but are requested via ImmediateLoad on a worker thread will:
///	<br/> 	- be deserialized immediately on the worker thread
///	<br/> 	- transition to asynchronous loading for creation
/// </summary>
partial class AssetRepository
{
	internal struct ContinuationScheduler
	{
		public readonly IAsset asset;
		public readonly AssetRepository repository;

		internal ContinuationScheduler(IAsset asset, AssetRepository repository)
		{
			this.asset = asset;
			this.repository = repository;
		}

		public void OnCompleted(Action continuation)
		{
			if (asset == null)
				throw new Exception("Main thread transition requested without an asset");

			continuation = continuation.OnlyRunnableOnce();
			repository._assetTransferQueue.Enqueue(continuation);
			asset.Continuation = continuation;
		}
	}

	private struct SafeToTransferAwaitable : INotifyCompletion
	{
		public ContinuationScheduler ContinuationScheduler { private get; init; }

		public bool IsCompleted => Monitor.IsEntered(ContinuationScheduler.repository._requestLock);

		public SafeToTransferAwaitable GetAwaiter()
			=> this;

		public void OnCompleted(Action action)
			=> ContinuationScheduler.OnCompleted(action);

		public void GetResult() { }
	}

	private static Thread _mainThread;

	public static bool IsMainThread => Thread.CurrentThread == _mainThread;

	protected readonly AssetReaderCollection _readers;
	internal readonly ConcurrentQueue<Action> _assetTransferQueue = new();

	private int _Remaining;

	public bool IsDisposed => _isDisposed;

	public AssetRepository(AssetReaderCollection readers, IEnumerable<IContentSource> sources = null)
	{
		_readers = readers;
		_sources = sources?.ToArray() ?? Array.Empty<IContentSource>();
	}

	public virtual void SetSources(IEnumerable<IContentSource> sources, AssetRequestMode mode = AssetRequestMode.ImmediateLoad)
	{
		ThrowIfDisposed();
		ThrowIfNotMainThread();

		lock (_requestLock) { // prevent new assets being requested or loaded
			TransferAllAssets();

			_sources = sources.ToArray();
			ReloadAssetsIfSourceChanged(mode);

			if (mode == AssetRequestMode.ImmediateLoad && _Remaining > 0)
				throw new Exception("Some assets loaded asynchronously, despite AssetRequestMode.ImmediateLoad on main thread");
 		}
 	}

	public IAsset[] GetLoadedAssets()
	{
 		lock (_requestLock) {
			return _assets.Values.ToArray();
		}
	}

	public void TransferAllAssets()
	{
		if (!IsMainThread) {
			Invoke(TransferAllAssets);
			return;
		}

		while (_Remaining > 0) {
			TransferCompletedAssets();
		}
	}

	public Asset<T> CreateUntracked<T>(Stream stream, string name, AssetRequestMode mode = AssetRequestMode.ImmediateLoad) where T : class
	{
		string ext = Path.GetExtension(name);

		if (!_readers.TryGetReader(ext, out var reader))
			throw AssetLoadException.FromMissingReader(ext);

		var asset = new Asset<T>(name[..^ext.Length]);
		var loadTask = LoadUntracked(stream, reader, asset, mode);

		asset.Wait = () => SafelyWaitForLoad(asset, loadTask, tracked: false);

		if (mode == AssetRequestMode.ImmediateLoad)
			asset.Wait();

		return asset;
	}

	private async Task LoadUntracked<T>(Stream stream, IAssetReader reader, Asset<T> asset, AssetRequestMode mode) where T : class
	{
		if (mode == AssetRequestMode.AsyncLoad) {
			// To the worker thread!
			await Task.Yield();
		}

		var mainThreadCtx = new MainThreadCreationContext {
			ContinuationScheduler = new ContinuationScheduler(asset, this)
		};

		asset.SubmitLoadedContent(await reader.FromStream<T>(stream, mainThreadCtx), null);
 	}

	private async Task LoadAssetWithPotentialAsync<T>(Asset<T> asset, AssetRequestMode mode) where T : class
	{
 		try {
			// The request lock is held until we move to another thread/delayed callback.
			if (!Monitor.IsEntered(_requestLock))
				throw new Exception($"Asset load started without holding {nameof(_requestLock)}");

			TotalAssets++;

			asset.SetToLoadingState();
			Interlocked.Increment(ref _Remaining);

			var rejectionReasons = new List<string>();
			var asyncCtx = new ContinuationScheduler(asset, this);

			foreach (var source in _sources) {
				if (source.Rejections.IsRejected(asset.Name) || source.GetExtension(asset.Name) is not string extension)
					continue;

				if (!_readers.TryGetReader(extension, out var reader)) {
					source.Rejections.Reject(asset.Name, new ContentRejectionNoCompatibleReader(extension, _readers.GetSupportedExtensions()));
					continue;
				}

				if (mode == AssetRequestMode.AsyncLoad)
					await Task.Yield(); // to the worker thread!

				T resultAsset;

				using (var stream = source.OpenStream(asset.Name + extension)) {
					try {
						resultAsset = await reader.FromStream<T>(stream, new MainThreadCreationContext() { ContinuationScheduler = asyncCtx });
					} catch (Exception e) {
						source.Rejections.Reject(asset.Name, new ContentRejectionAssetReaderException(e));
						continue;
					}
				}

				// continuation may be running on main thread
				if (source.ContentValidator != null && !source.ContentValidator.AssetIsValid(resultAsset, asset.Name, out var rejectionReason)) {
					source.Rejections.Reject(asset.Name, rejectionReason);
					continue;
				}

				await new SafeToTransferAwaitable {
					ContinuationScheduler = asyncCtx
				};

				if (!Monitor.IsEntered(_requestLock)) // check async code is functioning as expected
					throw new Exception($"Asset transfer started without holding {nameof(_requestLock)}");

				asset.SubmitLoadedContent(resultAsset, source);
				LoadedAssets++;
				return;
			}

			throw AssetLoadException.FromMissingAsset(asset.Name);
		}
		catch (Exception e) {
			AssetLoadFailHandler?.Invoke(asset.Name, e);

			if (mode == AssetRequestMode.ImmediateLoad)
				throw;
		}
		finally {
			Interlocked.Decrement(ref _Remaining);
 		}
 	}

	private void SafelyWaitForLoad<T>(Asset<T> asset, Task loadTask, bool tracked) where T : class
	{
		if (asset.State == AssetState.Loaded)
			return;

		// Asset has been loaded asynchronously, and we need the result now.
		// This specific mix of async and synchronous loading, combined with the requirement to load some assets on the main thread is rife with deadlock potential
		// A careful understanding of the exact threading model and requirements is required, and there's no clean or best practice way to solve it
		if (!loadTask.IsCompleted && IsMainThread) {
			// the asset loading may be blocked on a continuation which is currently in our _assetTransferQueue
			// given that this is the main thread, if we block, no-one else will process it
			// rather than running the whole transfer queue, and potentially inducing stuttering, we run the continuation directly from the asset

			// wait for a continuation to be scheduled (all async loads will schedule one)
			while (asset.Continuation == null) {
				Thread.Yield();
			}

			// running continuations requires the main thread (MainThreadAwaitable)
			if (tracked) {
				// and the request lock (SafeToTransferAwaitable) for tracked assets
				lock (_requestLock) {
					asset.Continuation();
				}
			}
			else {
				asset.Continuation();
			}

			if (!loadTask.IsCompleted)
				throw new Exception($"Load task not completed after running continuations on main thread?");
		}
	}

	private void Invoke(Action action)
	{
		// Skip loading assets if this is a dedicated server; this avoids deadlocks on waiting for queue to empty
		if (_readers == null) {
			_assetTransferQueue.Clear();
			return;
		}

		var evt = new ManualResetEvent(false);
		_assetTransferQueue.Enqueue(() => { action(); evt.Set(); });
		evt.WaitOne();
	}

	public static void SetMainThread()
	{
		if (_mainThread != null)
			throw new InvalidOperationException("Main thread already set");

		_mainThread = Thread.CurrentThread;
	}

	public static void ThrowIfNotMainThread()
	{
		if (!IsMainThread)
			throw new Exception("Must be on main thread");
	}
}
