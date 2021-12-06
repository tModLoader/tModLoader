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

			Func<SetupOperation> buttonRegenSource = () =>
				new RegenSourceTask(this, new[] { buttonPatchTerraria, buttonPatchModLoader }
					.Select(b => b()).ToArray());
			Func<SetupOperation> task = () =>
				new SetupTask(this, new[] { buttonDecompile, buttonRegenSource }
					.Select(b => b()).ToArray());
			if (Directory.Exists("src/decompiled")) {
				Console.WriteLine("decompiled folder found, skipping decompile step");
				task = () => new SetupTask(this, buttonRegenSource());
			}

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
					return;
				}

				if (task.Failed() || task.Warnings())
					task.FinishedDialog();
			}
			catch (Exception e) {
				SetStatus(e.Message);
				Environment.Exit(1);
				var status = "";

				SetupOperation.CreateDirectory(Program.logsDir);
				File.WriteAllText(errorLogFile, status + "\r\n" + e);
			}
		}
	}
}