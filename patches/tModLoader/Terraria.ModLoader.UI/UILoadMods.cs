using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UILoadMods : UIState
	{
		private UILoadProgress loadProgress;
		private UIText subProgress;

		public override void OnInitialize() {
			loadProgress = new UILoadProgress {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Height = { Pixels = 150 },
				HAlign = 0.5f,
				VAlign = 0.5f,
				Top = { Pixels = 10 }
			};
			Append(loadProgress);

			subProgress = new UIText("", 0.5f, true) {
				Top = { Pixels = 65 },
				HAlign = 0.5f,
				VAlign = 0.5f
			};
			Append(subProgress);
		}

		public override void OnActivate() {
			ModLoader.BeginLoad();
			GLCallLocker.ActionsAreSpeedrun = true;
		}

		public override void OnDeactivate() {
			GLCallLocker.ActionsAreSpeedrun = false;
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			GLCallLocker.SpeedrunActions();
		}

		public string SubProgressText {
			set => subProgress?.SetText(value);
		}

		public int modCount;
		private string stageText;
		public void SetLoadStage(string stageText, int modCount = -1) {
			this.stageText = stageText;
			this.modCount = modCount;
			if (modCount < 0)
				SetProgressText(Language.GetTextValue(stageText));

			loadProgress?.SetProgress(0);
			SubProgressText = "";
		}

		private void SetProgressText(string text) {
			Logging.tML.Info(text);
			if (Main.dedServ)
				Console.WriteLine(text);
			else
				loadProgress.SetText(text);
		}

		public void SetCurrentMod(int i, string mod) {
			SetProgressText(Language.GetTextValue(stageText, mod));
			loadProgress?.SetProgress(i / (float)modCount);
		}
	}
}
