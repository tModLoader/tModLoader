using System;
using System.Threading;

namespace ReLogic.Content;

public static class RunOnceAction
{
	public static Action OnlyRunnableOnce(this Action action) => () => Interlocked.Exchange(ref action, null)?.Invoke();
}
