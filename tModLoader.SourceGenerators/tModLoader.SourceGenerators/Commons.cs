using System.Diagnostics;

namespace tModLoader.SourceGenerators;

internal static class Commons
{
	public static void AssignDebugger()
	{
#if DEBUG
		if (!Debugger.IsAttached)
			Debugger.Launch();
#endif
	}
}
