namespace Terraria.ModLoader.Setup.Core.Abstractions;

/// <summary>
///		Represents a top level progress type.
/// </summary>
public interface IProgress
{
	/// <summary>
	///		Start a new task with the given <paramref name="description"/>. The description should not be longer
	///		than 60 characters. Returns a <see cref="ITaskProgress"/> which can be used to report further details
	///		for the started task. The <see cref="ITaskProgress"/> should be disposed after the task completed.
	/// </summary>
	/// <param name="description">The task description. Not longer than 60 characters.</param>
	/// <returns>
	///		A <see cref="ITaskProgress"/> which can be used to report further details for the started task.
	///		The <see cref="ITaskProgress"/> should be disposed after the task completed.
	/// </returns>
	ITaskProgress StartTask(string description);
}

/// <summary>
///		Represents a type for reporting progress details of a task.
/// </summary>
public interface ITaskProgress : IDisposable
{
	/// <summary>
	///		Sets the max progress.
	/// </summary>
	/// <param name="max">The max progress.</param>
	void SetMaxProgress(int max);

	/// <summary>
	///		Sets the current progress.
	/// </summary>
	/// <param name="current">The current progress.</param>
	void SetCurrentProgress(int current);

	/// <summary>
	///		Reports a status.
	/// </summary>
	/// <param name="status">The status.</param>
	/// <param name="overwrite">If <see langword="true"/> the previous status will be overriden, otherwise <paramref name="status"/> is appended.</param>
	void ReportStatus(string status, bool overwrite = false);
}