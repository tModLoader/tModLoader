using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Terraria.ModLoader.Engine;

/// <summary>
/// Provides a SynchronizationContext for running continuations on the Main thread in the Update loop, for platforms which don't initialized with one
/// </summary>
internal static class FallbackSyncContext
{
	private class SyncContext : SynchronizationContext
	{
		private static ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

		public override void Send(SendOrPostCallback d, object state)
		{
			var handle = new ManualResetEvent(false);
			Exception e = null;
			actions.Enqueue(() => {
				try {
					d.Invoke(state);
				}
				catch (Exception e2) {
					e = e2;
				}
				finally {
					handle.Set();
				}
			});

			handle.WaitOne();
			if (e != null)
				throw e;
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			actions.Enqueue(() => {
				try {
					d.Invoke(state);
				}
				catch (Exception e) {
					Logging.tML.Error("Posted event", e);
				}
			});
		}

		public override SynchronizationContext CreateCopy()
		{
			return this;
		}

		internal void Update()
		{
			while (actions.TryDequeue(out var action))
				action.Invoke();
		}
	}

	private static SyncContext ctx;
	public static void Update()
	{
		if (SynchronizationContext.Current == null) {
			SynchronizationContext.SetSynchronizationContext(ctx = new SyncContext());
			Logging.tML.Debug("Fallback synchronization context assigned");
		}

		ctx?.Update();
	}
}
