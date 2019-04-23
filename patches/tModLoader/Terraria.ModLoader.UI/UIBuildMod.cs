using log4net.Core;
using System;
using System.Threading;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	// TODO: Extend a progress UI? No cancel button here.
	internal class UIBuildMod : UIState, ModCompile.IBuildStatus
	{
		private UILoadProgress loadProgress;

		public override void OnInitialize() {
			loadProgress = new UILoadProgress {
				Top = { Pixels = 10 },
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Height = { Pixels = 150 },
				HAlign = 0.5f,
				VAlign = 0.5f
			};
			Append(loadProgress);
		}

		private int numProgressItems;
		public void SetProgress(int i, int n = -1) {
			if (n >= 0)
				numProgressItems = n;

			loadProgress.SetProgress(i / (float)numProgressItems);
		}

		public void SetStatus(string msg) {
			Logging.tML.Info(msg);
			loadProgress.SetText(msg);
		}

		public void LogCompilerLine(string msg, Level level) {
			Logging.tML.Logger.Log(null, level, msg, null);
		}

		internal void Build(string mod, bool reload) => Build(mc => mc.Build(mod), reload);

		internal void BuildAll(bool reload) => Build(mc => mc.BuildAll(), reload);

		private void Build(Action<ModCompile> buildAction, bool reload) {
			Main.menuMode = Interface.buildModID;
			ThreadPool.QueueUserWorkItem(_ => {
				while (loadProgress == null)
					Thread.Sleep(1);// wait for the UI to init

				try {
					buildAction(new ModCompile(this));
					if (reload)
						Main.menuMode = Interface.reloadModsID;
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
