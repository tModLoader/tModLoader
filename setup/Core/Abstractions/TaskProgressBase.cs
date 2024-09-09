namespace Terraria.ModLoader.Setup.Core.Abstractions;

public abstract class TaskProgressBase : ITaskProgress
{
	private readonly List<WorkItemProgress> currentWorkItems = [];

	protected TaskProgressBase(string description)
	{
		Description = description;
		State = State with { Status = string.Empty };
	}

	protected string Description { get; }

	protected ProgressState State { get; private set; } = new(0, 0, string.Empty);

	public abstract void Dispose();

	public void SetMaxProgress(int max) => State = State with { Max = max };

	public void SetCurrentProgress(int current) => State = State with { Current = current };

	public void ReportStatus(string status)
	{
		string[] parts = [State.Status, TransformStatus(status)];
		status = string.Join(Environment.NewLine, parts.Where(x => !string.IsNullOrWhiteSpace(x)));

		State = State with { Status = status };
	}

	public IWorkItemProgress StartWorkItem(string status)
	{
		var progress = new WorkItemProgress(
			status,
			UpdateStatusFromWorkItems,
			RemoveFromWorkItems);

		lock (currentWorkItems) {
			currentWorkItems.Add(progress);
		}

		UpdateStatusFromWorkItems();

		return progress;
	}

	private void UpdateStatusFromWorkItems()
	{
		lock (currentWorkItems) {
			State = State with {
				Status = string.Join(Environment.NewLine, currentWorkItems.Select(x => TransformStatus(x.Status))),
			};
		}
	}

	protected virtual string TransformStatus(string status) => status;

	private void RemoveFromWorkItems(WorkItemProgress workItemProgress)
	{
		lock (currentWorkItems) {
			currentWorkItems.Remove(workItemProgress);
		}
	}

	protected sealed record ProgressState(int Current, int Max, string Status);

	protected sealed class WorkItemProgress : IWorkItemProgress
	{
		private readonly Action updateStatus;
		private readonly Action<WorkItemProgress> complete;

		public WorkItemProgress(string status, Action updateStatus, Action<WorkItemProgress> complete)
		{
			Status = status;
			this.updateStatus = updateStatus;
			this.complete = complete;
		}

		public string Status { get; private set; }

		public void ReportStatus(string status)
		{
			Status = status;
			updateStatus();
		}

		public void Dispose() => complete(this);
	}
}