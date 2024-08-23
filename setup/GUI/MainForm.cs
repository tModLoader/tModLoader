using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiffPatch;
using PatchReviewer;
using Terraria.ModLoader.Setup.Core;
using Terraria.ModLoader.Setup.Core.Abstractions;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.GUI
{
	public partial class MainForm : Form, IProgress, IPatchReviewer
	{
		private readonly ProgramSettings programSettings;
		private readonly TerrariaExecutableSetter terrariaExecutableSetter;
		private readonly TargetsFilesUpdater targetsFilesUpdater;
		private readonly IServiceProvider serviceProvider;
		private CancellationTokenSource cancelSource;

		private bool closeOnCancel;
		private IDictionary<Button, Func<SetupOperation>> taskButtons = new Dictionary<Button, Func<SetupOperation>>();

		public MainForm(
			ProgramSettings programSettings,
			TerrariaExecutableSetter terrariaExecutableSetter,
			TargetsFilesUpdater targetsFilesUpdater,
			IServiceProvider serviceProvider)
		{
			this.programSettings = programSettings;
			this.terrariaExecutableSetter = terrariaExecutableSetter;
			this.targetsFilesUpdater = targetsFilesUpdater;
			this.serviceProvider = serviceProvider;
			InitializeComponent();

			labelWorkingDirectory.Text = $"{Directory.GetCurrentDirectory()}";

			taskButtons[buttonDecompile] = () => new DecompileTask(DecompileTaskParameters.CreateDefault(), serviceProvider);
			// Terraria
			taskButtons[buttonDiffTerraria] = () => new DiffTask(DiffTaskParameters.ForTerraria(this.programSettings));
			taskButtons[buttonPatchTerraria] = () => new PatchTask(PatchTaskParameters.ForTerraria(programSettings), serviceProvider);
			// Terraria .NET Core
			taskButtons[buttonDiffTerrariaNetCore] = () => new DiffTask(DiffTaskParameters.ForTerrariaNetCore(programSettings));
			taskButtons[buttonPatchTerrariaNetCore] = () => new PatchTask(PatchTaskParameters.ForTerrariaNetCore(programSettings), serviceProvider);
			// tModLoader
			taskButtons[buttonDiffModLoader] = () => new DiffTask(DiffTaskParameters.ForTModLoader(programSettings));
			taskButtons[buttonPatchModLoader] = () => new PatchTask(PatchTaskParameters.ForTModLoader(programSettings), serviceProvider);

			taskButtons[buttonRegenSource] = () => new RegenSourceTask(serviceProvider);

			taskButtons[buttonSetup] = () => new SetupTask(DecompileTaskParameters.CreateDefault(), serviceProvider);

			SetPatchMode(this.programSettings.PatchMode);
			formatDecompiledOutputToolStripMenuItem.Checked = programSettings.FormatAfterDecompiling;

			Closing += (sender, args) =>
			{
				if (buttonCancel.Enabled)
				{
					cancelSource.Cancel();
					args.Cancel = true;
					closeOnCancel = true;
				}
			};
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			cancelSource.Cancel();
		}

		private void menuItemTerraria_Click(object sender, EventArgs e)
		{
			terrariaExecutableSetter.SelectAndSetTerrariaDirectory();
		}

		private void menuItemResetTimeStampOptmizations_Click(object sender, EventArgs e)
		{
			DateTime cutoffDate = new DateTime(2015, 1, 1);
			programSettings.TerrariaDiffCutoff = cutoffDate;
			programSettings.TerrariaNetCoreDiffCutoff = cutoffDate;
			programSettings.TModLoaderDiffCutoff = cutoffDate;
			programSettings.Save();
		}

		private void menuItemDecompileServer_Click(object sender, EventArgs e) {
			RunTask(new DecompileTask(DecompileTaskParameters.CreateDefault(serverOnly: true), serviceProvider));
		}

		private void menuItemFormatCode_Click(object sender, EventArgs e) {
			RunTask(new FormatTask(serviceProvider));
		}

		private void menuItemHookGen_Click(object sender, EventArgs e) {
			RunTask(new HookGenTask(serviceProvider));
		}

		private void simplifierToolStripMenuItem_Click(object sender, EventArgs e) {
			RunTask(new SimplifierTask(serviceProvider));
		}

		private void buttonTask_Click(object sender, EventArgs e)
		{
			RunTask(taskButtons[(Button)sender]());
		}

		private void RunTask(SetupOperation task)
		{
			cancelSource?.Dispose();
			cancelSource = new CancellationTokenSource();
			foreach (var b in taskButtons.Keys) b.Enabled = false;
			buttonCancel.Enabled = true;

			_ = RunTaskThread(task);
		}

		private async Task RunTaskThread(SetupOperation task)
		{
			var errorLogFile = Path.Combine(programSettings.LogsDir, "error.log");
			try
			{
				SetupOperation.DeleteFile(errorLogFile);

				if (!await task.ConfigurationPrompt(cancelSource.Token).ConfigureAwait(true))
					return;

				if (!task.StartupWarning())
					return;

				try
				{
					await task.Run(this, cancelSource.Token).ConfigureAwait(true);

					if (cancelSource.IsCancellationRequested)
						throw new OperationCanceledException();
				}
				catch (OperationCanceledException e)
				{
					Invoke(new Action(() =>
					{
						labelStatus.Text = "Cancelled";
						if (e.Message != new OperationCanceledException().Message)
							labelStatus.Text += ": " + e.Message;
					}));
					return;
				}

				task.FinishedPrompt();

				Invoke(new Action(() =>
				{
					labelStatus.Text = task.Failed() ? "Failed" : "Done";
					labelTask.Text = string.Empty;
				}));
			}
			catch (Exception e)
			{
				var status = "";
				Invoke(new Action(() =>
				{
					status = labelStatus.Text;
					labelStatus.Text = $"Error: {e.Message.Trim()}{Environment.NewLine}Log: {Path.GetFullPath(errorLogFile)}";
				}));

				SetupOperation.CreateDirectory(programSettings.LogsDir);
				File.WriteAllText(errorLogFile, status + "\r\n" + e);
			}
			finally
			{
				Invoke(new Action(() =>
				{
					foreach (var b in taskButtons.Keys) b.Enabled = true;
					buttonCancel.Enabled = false;
					progressBar.Value = 0;
					if (closeOnCancel) Close();
				}));
			}
		}

		private void SetPatchMode(Patcher.Mode mode) {
			exactToolStripMenuItem.Checked = mode == Patcher.Mode.EXACT;
			offsetToolStripMenuItem.Checked = mode == Patcher.Mode.OFFSET;
			fuzzyToolStripMenuItem.Checked = mode == Patcher.Mode.FUZZY;

			programSettings.PatchMode = mode;
			programSettings.Save();
		}

		private void exactToolStripMenuItem_Click(object sender, EventArgs e) {
			SetPatchMode(Patcher.Mode.EXACT);
		}

		private void offsetToolStripMenuItem_Click(object sender, EventArgs e) {
			SetPatchMode(Patcher.Mode.OFFSET);
		}

		private void fuzzyToolStripMenuItem_Click(object sender, EventArgs e) {
			SetPatchMode(Patcher.Mode.FUZZY);
		}

		private void formatDecompiledOutputToolStripMenuItem_Click(object sender, EventArgs e)
		{
			programSettings.FormatAfterDecompiling ^= true;
			programSettings.Save();

			formatDecompiledOutputToolStripMenuItem.Checked = programSettings.FormatAfterDecompiling;
		}

		private void mainMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {

		}

		private void toolTipButtons_Popup(object sender, PopupEventArgs e) {

		}

		private void menuItemTmlPath_Click(object sender, EventArgs e) {
			while (true) {
				var dialog = new OpenFileDialog {
					InitialDirectory = Path.GetFullPath(Directory.Exists(programSettings.TerrariaSteamDir) ? programSettings.TerrariaSteamDir : "."),
					ValidateNames = false,
					CheckFileExists = false,
					CheckPathExists = true,
					FileName = "Folder Selection.",
				};

				if (dialog.ShowDialog() != DialogResult.OK)
					return;

				programSettings.TMLDevSteamDir = Path.GetDirectoryName(dialog.FileName);
				programSettings.Save();
				targetsFilesUpdater.Update();

				return;
			}
		}

		private void updateLocalizationFilesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RunTask(new UpdateLocalizationFilesTask());
		}

		public void Show(IReadOnlyCollection<FilePatcher> results, string commonBasePath = null)
		{
			Invoke(() => new ReviewWindow(results, commonBasePath) { AutoHeaders = true }.Show());
		}

		public ITaskProgress StartTask(string description)
		{
			Debug.Assert(description.Length <= 60);

			Invoke(() => labelTask.Text = description);

			return new TaskProgress(this);
		}

		private sealed class TaskProgress : ITaskProgress
		{
			private readonly MainForm mainForm;

			public TaskProgress(MainForm mainForm)
			{
				this.mainForm = mainForm;
			}

			public void Dispose() { }

			public void SetMaxProgress(int max) =>
				mainForm.Invoke(() => {
					mainForm.progressBar.Maximum = max;
				});

			public void SetCurrentProgress(int current) =>
				mainForm.Invoke(() => {
					mainForm.progressBar.Value = current;
				});

			public void ReportStatus(string status, bool overwrite = false)
			{
				mainForm.Invoke(() => {
					if (overwrite) {
						mainForm.labelStatus.Text = status;
					}
					else {
						string[] parts = [mainForm.labelStatus.Text, status];
						mainForm.labelStatus.Text =
							string.Join(Environment.NewLine, parts.Where(x => !string.IsNullOrEmpty(x)));
					}
				});
			}
		}
	}
}
