using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Security.Cryptography;
using Terraria.ModLoader.Properties;
using static Terraria.ModLoader.Setup.Program;

namespace Terraria.ModLoader.Setup
{
	public partial class MainForm : Form, ITaskInterface
	{
		private CancellationTokenSource cancelSource;

		private bool closeOnCancel;
		private IDictionary<Button, Func<SetupOperation>> taskButtons = new Dictionary<Button, Func<SetupOperation>>();

		public MainForm()
		{
			InitializeComponent();

			taskButtons[buttonDecompile] = () => new DecompileTask(this, "src/decompiled");
			taskButtons[buttonDiffTerraria] = () => new DiffTask(this, "src/decompiled", "src/Terraria", "patches/Terraria", new ProgramSetting<DateTime>("TerrariaDiffCutoff"));
			taskButtons[buttonPatchTerraria] = () => new PatchTask(this, "src/decompiled", "src/Terraria", "patches/Terraria", new ProgramSetting<DateTime>("TerrariaDiffCutoff"));
			taskButtons[buttonDiffModLoader] = () => new DiffTask(this, "src/Terraria", "src/tModLoader", "patches/tModLoader", new ProgramSetting<DateTime>("tModLoaderDiffCutoff"));
			taskButtons[buttonPatchModLoader] = () => new PatchTask(this, "src/Terraria", "src/tModLoader", "patches/tModLoader", new ProgramSetting<DateTime>("tModLoaderDiffCutoff"));

			taskButtons[buttonRegenSource] = () =>
				new RegenSourceTask(this, new[] { buttonPatchTerraria, buttonPatchModLoader }
					.Select(b => taskButtons[b]()).ToArray());

			taskButtons[buttonSetup] = () =>
				new SetupTask(this, new[] { buttonDecompile, buttonRegenSource }
					.Select(b => taskButtons[b]()).ToArray());

			SetPatchMode(Settings.Default.PatchMode);
			formatDecompiledOutputToolStripMenuItem.Checked = Settings.Default.FormatAfterDecompiling;

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

		public void SetMaxProgress(int max)
		{
			Invoke(new Action(() =>
			{
				progressBar.Maximum = max;
			}));
		}

		public void SetStatus(string status)
		{
			Invoke(new Action(() =>
			{
				labelStatus.Text = status;
			}));
		}

		public void SetProgress(int progress)
		{
			Invoke(new Action(() =>
			{
				progressBar.Value = progress;
			}));
		}

		public CancellationToken CancellationToken => cancelSource.Token;

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			cancelSource.Cancel();
		}

		private void menuItemTerraria_Click(object sender, EventArgs e)
		{
			SelectTerrariaDialog();
		}

		private void menuItemResetTimeStampOptmizations_Click(object sender, EventArgs e)
		{
			Settings.Default.TerrariaDiffCutoff = new DateTime(2015, 1, 1);
			Settings.Default.tModLoaderDiffCutoff = new DateTime(2015, 1, 1);
			Settings.Default.Save();
		}

		private void menuItemDecompileServer_Click(object sender, EventArgs e) {
			RunTask(new DecompileTask(this, "src/decompiled_server", true));
		}

		private void menuItemFormatCode_Click(object sender, EventArgs e) {
			RunTask(new FormatTask(this));
		}

		private void menuItemHookGen_Click(object sender, EventArgs e) {
			RunTask(new HookGenTask(this));
		}

		private void simplifierToolStripMenuItem_Click(object sender, EventArgs e) {
			RunTask(new SimplifierTask(this));
		}

		private void buttonTask_Click(object sender, EventArgs e)
		{
			RunTask(taskButtons[(Button)sender]());
		}

		private void RunTask(SetupOperation task)
		{
			cancelSource = new CancellationTokenSource();
			foreach (var b in taskButtons.Keys) b.Enabled = false;
			buttonCancel.Enabled = true;

			new Thread(() => RunTaskThread(task)).Start();
		}

		private void RunTaskThread(SetupOperation task)
		{
			var errorLogFile = Path.Combine(Program.logsDir, "error.log");
			try
			{
				SetupOperation.DeleteFile(errorLogFile);

				if (!task.ConfigurationDialog())
					return;

				if (!task.StartupWarning())
					return;

				try
				{
					task.Run();

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

				if (task.Failed() || task.Warnings())
					task.FinishedDialog();

				Invoke(new Action(() =>
				{
					labelStatus.Text = task.Failed() ? "Failed" : "Done";
				}));
			}
			catch (Exception e)
			{
				var status = "";
				Invoke(new Action(() =>
				{
					status = labelStatus.Text;
					labelStatus.Text = "Error: " + e.Message.Trim();
				}));

				SetupOperation.CreateDirectory(Program.logsDir);
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

		private void SetPatchMode(int mode) {
			exactToolStripMenuItem.Checked = mode == 0;
			offsetToolStripMenuItem.Checked = mode == 1;
			fuzzyToolStripMenuItem.Checked = mode == 2;
			Settings.Default.PatchMode = mode;
			Settings.Default.Save();
		}

		private void exactToolStripMenuItem_Click(object sender, EventArgs e) {
			SetPatchMode(0);
		}

		private void offsetToolStripMenuItem_Click(object sender, EventArgs e) {
			SetPatchMode(1);
		}

		private void fuzzyToolStripMenuItem_Click(object sender, EventArgs e) {
			SetPatchMode(2);
		}

		private void formatDecompiledOutputToolStripMenuItem_Click(object sender, EventArgs e) {
			Settings.Default.FormatAfterDecompiling ^= true;
			Settings.Default.Save();
			formatDecompiledOutputToolStripMenuItem.Checked = Settings.Default.FormatAfterDecompiling;
		}

		private void mainMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {

		}

		private void toolTipButtons_Popup(object sender, PopupEventArgs e) {

		}

		private void menuItemTmlPath_Click(object sender, EventArgs e) {
			Program.SelectTmlDirectoryDialog();
		}
	}
}
