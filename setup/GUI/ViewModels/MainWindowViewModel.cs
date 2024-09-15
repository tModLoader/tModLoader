using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiffPatch;
using Setup.GUI.Avalonia.Services;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Abstractions;
using Timer = System.Timers.Timer;

namespace Setup.GUI.Avalonia.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IProgress
{
	private readonly IFilesService filesService;
	private readonly TerrariaExecutableSetter terrariaExecutableSetter;
	private readonly ProgramSettings programSettings;
	private readonly IServiceProvider serviceProvider;
	private readonly WorkspaceInfo workspaceInfo;

	private CancellationTokenSource cts = new();
	private string? projectSelectionProjectPath;

	[ObservableProperty] private int? currentProgress;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(SetupCommand))]
	[NotifyCanExecuteChangedFor(nameof(DecompileCommand))]
	[NotifyCanExecuteChangedFor(nameof(DiffTerrariaCommand))]
	[NotifyCanExecuteChangedFor(nameof(DiffTerrariaNetCoreCommand))]
	[NotifyCanExecuteChangedFor(nameof(DiffTModLoaderCommand))]
	[NotifyCanExecuteChangedFor(nameof(PatchTerrariaCommand))]
	[NotifyCanExecuteChangedFor(nameof(PatchTerrariaNetCoreCommand))]
	[NotifyCanExecuteChangedFor(nameof(PatchTModLoaderCommand))]
	[NotifyCanExecuteChangedFor(nameof(RegenSourceCommand))]
	[NotifyCanExecuteChangedFor(nameof(FormatCommand))]
	[NotifyCanExecuteChangedFor(nameof(SimplifyCommand))]
	[NotifyCanExecuteChangedFor(nameof(UpdateLocalizationFilesCommand))]
	private bool noCommandRunning = true;

	[ObservableProperty] private int? maxProgress;
	[ObservableProperty] private string statusText = string.Empty;
	[ObservableProperty] private string taskText = string.Empty;

	public MainWindowViewModel(
		WorkspaceInfo workspaceInfo,
		ProgramSettings programSettings,
		IFilesService filesService,
		TerrariaExecutableSetter terrariaExecutableSetter,
		IServiceProvider serviceProvider)
	{
		this.workspaceInfo = workspaceInfo;
		this.programSettings = programSettings;
		this.filesService = filesService;
		this.terrariaExecutableSetter = terrariaExecutableSetter;
		this.serviceProvider = serviceProvider;

		this.programSettings.PropertyChanged += ProgramSettingsOnPropertyChanged;
	}

	public int SelectedPatchMode => (int)programSettings.PatchMode;

	public bool FormatAfterDecompiling => programSettings.FormatAfterDecompiling;

	public ITaskProgress StartTask(string description) => new TaskProgress(description, this);

	[RelayCommand(CanExecute = nameof(NoCommandRunning))]
	private async Task Setup() =>
		await RunTaskThread(new SetupTask(DecompileTaskParameters.CreateDefault(null, null), serviceProvider));

	[RelayCommand(CanExecute = nameof(NoCommandRunning))]
	private async Task Decompile() =>
		await RunTaskThread(new DecompileTask(DecompileTaskParameters.CreateDefault(null, null), serviceProvider));

	[RelayCommand(CanExecute = nameof(NoCommandRunning))]
	private async Task DecompileServer() =>
		await RunTaskThread(new DecompileTask(DecompileTaskParameters.CreateDefault(null, null, serverOnly: true),
			serviceProvider));

	[RelayCommand(CanExecute = nameof(NoCommandRunning))]
	private async Task DiffTerraria() =>
		await RunTaskThread(new DiffTask(DiffTaskParameters.ForTerraria(programSettings)));

	[RelayCommand(CanExecute = nameof(NoCommandRunning))]
	private async Task DiffTerrariaNetCore() =>
		await RunTaskThread(new DiffTask(DiffTaskParameters.ForTerrariaNetCore(programSettings)));

	[RelayCommand(CanExecute = nameof(NoCommandRunning))]
	private async Task DiffTModLoader() =>
		await RunTaskThread(new DiffTask(DiffTaskParameters.ForTModLoader(programSettings)));

	[RelayCommand(CanExecute = nameof(NoCommandRunning))]
	private async Task PatchTerraria() =>
		await RunTaskThread(new PatchTask(PatchTaskParameters.ForTerraria(programSettings), serviceProvider));

	[RelayCommand(CanExecute = nameof(NoCommandRunning))]
	private async Task PatchTerrariaNetCore() =>
		await RunTaskThread(new PatchTask(PatchTaskParameters.ForTerrariaNetCore(programSettings), serviceProvider));

	[RelayCommand(CanExecute = nameof(NoCommandRunning))]
	private async Task PatchTModLoader() =>
		await RunTaskThread(new PatchTask(PatchTaskParameters.ForTModLoader(programSettings), serviceProvider));

	[RelayCommand(CanExecute = nameof(NoCommandRunning))]
	private async Task RegenSource() => await RunTaskThread(new RegenSourceTask(serviceProvider));

	[RelayCommand]
	private void Cancel()
	{
		cts.Cancel();
		cts.Dispose();
		cts = new CancellationTokenSource();
	}

	[RelayCommand(CanExecute = nameof(NoCommandRunning))]
	private async Task Format()
	{
		string? selectedPath = await PromptForProjectPath(projectSelectionProjectPath);

		if (selectedPath != null) {
			projectSelectionProjectPath = selectedPath;
			await RunTaskThread(
				new FormatTask(new FormatTaskParameters { ProjectPath = projectSelectionProjectPath! }));
		}
	}

	[RelayCommand(CanExecute = nameof(NoCommandRunning))]
	private async Task HookGen() => await RunTaskThread(new HookGenTask(serviceProvider));

	[RelayCommand(CanExecute = nameof(NoCommandRunning))]
	private async Task Simplify()
	{
		string? selectedPath = await PromptForProjectPath(projectSelectionProjectPath);

		if (selectedPath != null) {
			projectSelectionProjectPath = selectedPath;
			await RunTaskThread(
				new SimplifierTask(new RoslynTaskParameters { ProjectPath = projectSelectionProjectPath! }));
		}
	}

	[RelayCommand(CanExecute = nameof(NoCommandRunning))]
	private async Task UpdateLocalizationFiles() =>
		await RunTaskThread(new UpdateLocalizationFilesTask(serviceProvider));

	[RelayCommand]
	private void SetExactPatchMode() => SetPatchMode(Patcher.Mode.EXACT);

	[RelayCommand]
	private void SetOffsetPatchMode() => SetPatchMode(Patcher.Mode.OFFSET);

	[RelayCommand]
	private void SetFuzzyPatchMode() => SetPatchMode(Patcher.Mode.FUZZY);

	private void SetPatchMode(Patcher.Mode patchMode)
	{
		if (programSettings.PatchMode != patchMode) {
			programSettings.PatchMode = patchMode;
			programSettings.Save();
		}
	}

	[RelayCommand]
	private void ToggleFormatAfterDecompiling()
	{
		programSettings.FormatAfterDecompiling = !programSettings.FormatAfterDecompiling;
		programSettings.Save();
	}

	[RelayCommand]
	private async Task SelectTerrariaDirectory()
	{
		try {
			await terrariaExecutableSetter.SelectAndSetTerrariaDirectory();
		}
		catch (OperationCanceledException) { }
	}

	[RelayCommand]
	private async Task SelectTModLoaderDirectory()
	{
		IStorageFolder? folder = await filesService.OpenFolder(
			"Select custom TML output directory",
			Path.GetFullPath(Directory.Exists(workspaceInfo.TMLDevSteamDirectory)
				? workspaceInfo.TMLDevSteamDirectory
				: "."));

		string? path = folder?.TryGetLocalPath();

		if (path is not null) {
			workspaceInfo.UpdatePaths(workspaceInfo.TerrariaSteamDirectory, path);
		}
	}

	[RelayCommand]
	private void ResetDiffTimestamps()
	{
		programSettings.TerrariaDiffCutoff = null;
		programSettings.TerrariaNetCoreDiffCutoff = null;
		programSettings.TModLoaderDiffCutoff = null;
		programSettings.Save();
	}

	private async Task<string?> PromptForProjectPath(string? currentProjectPath)
	{
		IStorageFile? file = await filesService.OpenFile(
			"Select C# Project",
			[new FilePickerFileType("C# Project|*.csproj") { Patterns = ["*.csproj"] }],
			Path.GetDirectoryName(currentProjectPath) ?? Path.GetFullPath("."),
			Path.GetFileName(currentProjectPath));

		return file?.TryGetLocalPath();
	}

	private void ProgramSettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName) {
			case nameof(ProgramSettings.PatchMode):
				OnPropertyChanged(nameof(SelectedPatchMode));
				break;

			case nameof(ProgramSettings.FormatAfterDecompiling):
				OnPropertyChanged(nameof(FormatAfterDecompiling));
				break;
		}
	}

	private async Task RunTaskThread(SetupOperation task)
	{
		string errorLogFile = Path.Combine(ProgramSettings.LogsDir, "error.log");
		try {
			NoCommandRunning = false;
			SetupOperation.DeleteFile(errorLogFile);

			workspaceInfo.UpdateGitInfo();

			await task.ConfigurationPrompt(cts.Token);

			if (!await task.StartupWarning()) {
				return;
			}

			await task.Run(this, cts.Token);

			await task.FinishedPrompt();

			StatusText = task.Failed() ? "Failed" : "Done";
		}
		catch (OperationCanceledException e) {
			StatusText = "Cancelled";
			if (e.Message != new OperationCanceledException().Message) {
				StatusText += ": " + e.Message;
			}
		}
		catch (Exception e) {
			if (e is AggregateException aggregateException) {
				e = aggregateException.Flatten();
			}

			string status = StatusText;
			StatusText = $"Error: {e.Message.Trim()}{Environment.NewLine}Log: {Path.GetFullPath(errorLogFile)}";
			SetupOperation.CreateDirectory(ProgramSettings.LogsDir);
			File.WriteAllText(errorLogFile, status + "\r\n" + e);
		}
		finally {
			NoCommandRunning = true;
			CurrentProgress = null;
			MaxProgress = null;
		}
	}

	private class TaskProgress : TaskProgressBase
	{
		private readonly Timer timer;
		private readonly MainWindowViewModel viewModel;

		public TaskProgress(string description, MainWindowViewModel viewModel) : base(description)
		{
			this.viewModel = viewModel;
			viewModel.TaskText = description;
			timer = new Timer(TimeSpan.FromMilliseconds(50));
			timer.Elapsed += RefreshProgress;
			timer.Start();
		}

		public override void Dispose()
		{
			timer.Dispose();
			viewModel.TaskText = string.Empty;
		}

		private void RefreshProgress(object? sender, ElapsedEventArgs e)
		{
			viewModel.StatusText = State.Status;
			viewModel.CurrentProgress = State.Current;
			viewModel.MaxProgress = State.Max;
		}
	}
}