using log4net.Core;
using System;
using System.Threading;
using Terraria.Localization;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader.UI.DownloadManager
{
	internal class UIBuildModProgress : UIProgress, ModCompile.IBuildStatus
	{
		private int numProgressItems;
		public void SetProgress(int i, int n = -1) {
			if (n >= 0) numProgressItems = n;
			Progress = i / (float)numProgressItems;
		}

		public void SetStatus(string msg) {
			Logging.tML.Info(msg);
			DisplayText = msg;
		}

		public void LogCompilerLine(string msg, Level level) {
			Logging.tML.Logger.Log(null, level, msg, null);
		}

		internal void Build(string mod, bool reload)
			=> Build(mc => mc.Build(mod), reload);

		internal void BuildAll(bool reload)
			=> Build(mc => mc.BuildAll(), reload);

		private void Build(Action<ModCompile> buildAction, bool reload) {
			Main.menuMode = Interface.buildModProgressID;
			ThreadPool.QueueUserWorkItem(_ => {
				while (_progressBar == null)
					Thread.Sleep(1);// wait for the UI to init

				try {
					buildAction(new ModCompile(this));
					Main.menuMode = reload ? Interface.reloadModsID : Interface.modSourcesID;
				}
				catch (Exception e) {
					Logging.tML.Error(e.Message, e);

					var mod = e.Data.Contains("mod") ? e.Data["mod"] : null;
					var msg = Language.GetTextValue("tModLoader.BuildError", mod ?? "");
					if (e is BuildException)
						msg += $"\n{e.Message}\n\n{e.InnerException?.ToString() ?? ""}";
					else
						msg += $"\n\n{e}";

					Interface.errorMessage.Show(msg, Interface.modSourcesID, e.HelpLink);
				}
			});
		}
	}
}
