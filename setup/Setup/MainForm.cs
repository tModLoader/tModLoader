using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Terraria.ModLoader.Setup
{
	public partial class MainForm : Form, ITaskInterface
	{
		private CancellationTokenSource cancelSource;

		private bool closeOnCancel;
		private IDictionary<Button, Func<Task>> taskButtons = new Dictionary<Button, Func<Task>>();

		public MainForm()
		{
			InitializeComponent();

			taskButtons[buttonDecompile] = () => new DecompileTask(this, "src/decompiled");
			taskButtons[buttonDiffMerged] = () => new DiffTask(this, "src/decompiled", "src/merged", "patches/merged", DiffTask.MergedDiffCutoff);
			taskButtons[buttonPatchMerged] = () => new PatchTask(this, "src/decompiled", "src/merged", "patches/merged", DiffTask.MergedDiffCutoff);
			taskButtons[buttonDiffTerraria] = () => new DiffTask(this, "src/merged", "src/Terraria", "patches/Terraria", DiffTask.TerrariaDiffCutoff);
			taskButtons[buttonPatchTerraria] = () => new PatchTask(this, "src/merged", "src/Terraria", "patches/Terraria", DiffTask.TerrariaDiffCutoff);
			taskButtons[buttonDiffModLoader] = () => new DiffTask(this, "src/Terraria", "src/tModLoader", "patches/tModLoader", DiffTask.tModLoaderDiffCutoff, FormatTask.tModLoaderFormat);
			taskButtons[buttonPatchModLoader] = () => new PatchTask(this, "src/Terraria", "src/tModLoader", "patches/tModLoader", DiffTask.tModLoaderDiffCutoff, FormatTask.tModLoaderFormat);
			taskButtons[buttonFormat] = () => new FormatTask(this, FormatTask.tModLoaderFormat);
			taskButtons[buttonSetup] = () =>
				new SetupTask(this, new[] { buttonDecompile, buttonPatchMerged, buttonPatchTerraria, buttonPatchModLoader }
					.Select(b => taskButtons[b]()).ToArray());

			menuItemWarnings.Checked = Program.SuppressWarnings.Get();
			menuItemSingleDecompileThread.Checked = DecompileTask.SingleDecompileThread.Get();

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

		public CancellationToken CancellationToken()
		{
			return cancelSource.Token;
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			cancelSource.Cancel();
		}

		private void menuItemTerraria_Click(object sender, EventArgs e)
		{
			DecompileTask.SelectTerrariaDialog();
		}

		private void menuItemWarnings_Click(object sender, EventArgs e)
		{
			Program.SuppressWarnings.Set(menuItemWarnings.Checked);
		}

		private void menuItemSingleDecompileThread_Click(object sender, EventArgs e)
		{
			DecompileTask.SingleDecompileThread.Set(menuItemSingleDecompileThread.Checked);
		}

		private void menuItemResetTimeStampOptmizations_Click(object sender, EventArgs e)
		{
			DiffTask.MergedDiffCutoff.Set(new DateTime(2015, 1, 1));
			DiffTask.TerrariaDiffCutoff.Set(new DateTime(2015, 1, 1));
			DiffTask.tModLoaderDiffCutoff.Set(new DateTime(2015, 1, 1));
		}

		private void buttonTask_Click(object sender, EventArgs e)
		{
			RunTask(taskButtons[(Button)sender]());
		}

		private void RunTask(Task task)
		{
			cancelSource = new CancellationTokenSource();
			foreach (var b in taskButtons.Keys) b.Enabled = false;
			buttonCancel.Enabled = true;

			new Thread(() => RunTaskThread(task)).Start();
		}

		private void RunTaskThread(Task task)
		{
			var errorLogFile = Path.Combine(Program.LogDir, "error.log");
			try
			{
				if (File.Exists(errorLogFile))
					File.Delete(errorLogFile);

				if (!task.ConfigurationDialog())
					return;

				if (!Program.SuppressWarnings.Get() && !task.StartupWarning())
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

				if ((task.Failed() || task.Warnings() && !Program.SuppressWarnings.Get()))
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

				Task.CreateDirectory(Program.LogDir);
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
	}
}
