namespace Terraria.ModLoader.Setup.Core.Abstractions;

public sealed class GenericWorkItemProgress : IWorkItemProgress
{
	private readonly Action updateStatus;
	private readonly Action<GenericWorkItemProgress> complete;

	public GenericWorkItemProgress(string status, Action updateStatus, Action<GenericWorkItemProgress> complete)
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