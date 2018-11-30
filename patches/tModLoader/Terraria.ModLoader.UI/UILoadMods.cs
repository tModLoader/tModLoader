using System;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UILoadMods : UIState
	{
		private UILoadProgress loadProgress;
		private UIText subProgress;

		public override void OnInitialize()
		{
			loadProgress = new UILoadProgress();
			loadProgress.Width.Set(0f, 0.8f);
			loadProgress.MaxWidth.Set(600f, 0f);
			loadProgress.Height.Set(150f, 0f);
			loadProgress.HAlign = 0.5f;
			loadProgress.VAlign = 0.5f;
			loadProgress.Top.Set(10f, 0f);
			base.Append(loadProgress);

			subProgress = new UIText("", 0.5f, true);
			subProgress.Top.Set(65f, 0f);
			subProgress.HAlign = 0.5f;
			subProgress.VAlign = 0.5f;
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
		public void SetLoadStage(string stageText, int modCount = -1)
		{
			this.stageText = stageText;
			this.modCount = modCount;
			if (modCount < 0)
				SetProgressText(Language.GetTextValue(stageText));
				
			loadProgress?.SetProgress(0);
			SubProgressText = "";
		}

		private void SetProgressText(string text)
		{
			Logging.tML.Info(text);
			if (Main.dedServ)
				Console.WriteLine(text);
			else
				loadProgress.SetText(text);
		}

		public void SetCurrentMod(int i, string mod)
		{
			SetProgressText(Language.GetTextValue(stageText, mod));
			loadProgress?.SetProgress(i / (float) modCount);
		}
	}
}
