using Microsoft.Build.Utilities;
using System;

namespace tModLoader.BuildTools.Tasks;

public abstract class TaskBase : Task
{
	public sealed override bool Execute()
	{
		try {
			Run();
		}
		catch (Exception e) {
			Log.LogErrorFromException(e, true);
		}

		return !Log.HasLoggedErrors;
	}

	protected abstract void Run();
}