using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Terraria.ModLoader.Setup;

namespace Terraria.ModLoader.Setup
{
	class AutoSetup : ITaskInterface
	{
		private CancellationTokenSource cancelSource;

		public CancellationToken CancellationToken => cancelSource.Token;

		public IntPtr Handle => throw new NotImplementedException();

		public IAsyncResult BeginInvoke(Delegate action) => throw new NotImplementedException();
		public object Invoke(Delegate action) => action.DynamicInvoke();

		int max = 1;
		public void SetMaxProgress(int max) => this.max = max;
		public void SetProgress(int progress) {
			Console.WriteLine(string.Format("Value: {0:P2}.", (float)progress / max));
		}

		public void SetStatus(string status) => Console.WriteLine(status);

		public void DoAuto() {
			Func<SetupOperation> buttonDecompile = () => new DecompileTask(this, "src/decompiled");
			Func<SetupOperation> buttonPatchTerraria = () => new PatchTask(this, "src/decompiled", "src/Terraria", "patches/Terraria", new ProgramSetting<DateTime>("TerrariaDiffCutoff"));
			Func<SetupOperation> buttonPatchModLoader = () => new PatchTask(this, "src/Terraria", "src/tModLoader", "patches/tModLoader", new ProgramSetting<DateTime>("tModLoaderDiffCutoff"));
			Func<SetupOperation> buttonSetupDebugging = () => new SetupDebugTask(this);

			Func<SetupOperation> buttonRegenSource = () =>
				new RegenSourceTask(this, new[] { buttonPatchTerraria, buttonPatchModLoader, buttonSetupDebugging }
					.Select(b => b()).ToArray());
			Func<SetupOperation> task = () =>
				new SetupTask(this, new[] { buttonDecompile, buttonRegenSource }
					.Select(b => b()).ToArray());

			cancelSource = new CancellationTokenSource();
			DoAuto2(task());
		}
		public void DoAuto2(SetupOperation task) {
			var errorLogFile = Path.Combine(Program.logsDir, "error.log");
			try {
				SetupOperation.DeleteFile(errorLogFile);

				if (!task.ConfigurationDialog())
					return;

				if (!task.StartupWarning())
					return;

				try {
					task.Run();

					if (cancelSource.IsCancellationRequested)
						throw new OperationCanceledException();
				}
				catch (OperationCanceledException e) {
					Invoke(new Action(() => {
						//labelStatus.Text = "Cancelled";
						//if (e.Message != new OperationCanceledException().Message)
						//	labelStatus.Text += ": " + e.Message;
					}));
					return;
				}

				if (task.Failed() || task.Warnings())
					task.FinishedDialog();

				Invoke(new Action(() => {
					//labelStatus.Text = task.Failed() ? "Failed" : "Done";
				}));
			}
			catch (Exception e) {
				var status = "";
				Invoke(new Action(() => {
					//status = labelStatus.Text;
					//labelStatus.Text = "Error: " + e.Message.Trim();
				}));

				SetupOperation.CreateDirectory(Program.logsDir);
				File.WriteAllText(errorLogFile, status + "\r\n" + e);
			}
			finally {
				Invoke(new Action(() => {
					//foreach (var b in taskButtons.Keys)
					//	b.Enabled = true;
					//buttonCancel.Enabled = false;
					//progressBar.Value = 0;
					//if (closeOnCancel)
					//	Close();
				}));
			}
		}
	}
}