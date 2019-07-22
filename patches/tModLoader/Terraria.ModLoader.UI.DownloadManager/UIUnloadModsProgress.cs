using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ModLoader.Engine;

namespace Terraria.ModLoader.UI.DownloadManager
{
	// This UI is triggered on ModLoader.Unload() for clients
	internal class UIUnloadModsProgress : UIProgress
	{
		private int _modCount;
		private string _stageText;

		public override void OnInitialize() {
			base.OnInitialize();
			_cancelButton.Remove(); // We can't cancel unloading.
		}

		public override void OnActivate() {
			base.OnActivate();
			gotoMenu = 888; // // ModLoader will redirect to the mods menu
			GLCallLocker.ActionsAreSpeedrun = true;
		}

		public override void OnDeactivate() {
			base.OnDeactivate();
			GLCallLocker.ActionsAreSpeedrun = false;
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			GLCallLocker.SpeedrunActions();
		}

		public void SetLoadStage(string stageText, int modCount = -1) {
			_stageText = stageText;
			_modCount = modCount;
			if (modCount < 0) SetProgressText(Language.GetTextValue(stageText));
			Progress = 0;
		}

		private void SetProgressText(string text) {
			Logging.tML.Info(text);
			if (Main.dedServ) Console.WriteLine(text);
			else DisplayText = text;
		}

		public void SetCurrentMod(int i, string mod) {
			SetProgressText(Language.GetTextValue(_stageText, mod));
			Progress = i / (float)_modCount;
		}
	}
}
