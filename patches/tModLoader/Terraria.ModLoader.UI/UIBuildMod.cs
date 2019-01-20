using log4net.Core;
using System;
using System.CodeDom.Compiler;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	// TODO: yet another progress UI?, otherwise it's fine, no cancel button in this one
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

		public void SetProgress(int num, int max) {
			loadProgress.SetProgress((float)num / (float)max);
		}

		public void SetStatus(string msg) {
			Logging.tML.Info(msg);
			loadProgress.SetText(msg);
		}

		private string mod;
		public void SetMod(string modName) {
			mod = modName;
		}

		public void LogError(string msg, Exception e = null) {
			Logging.tML.Error(msg, e);

			msg = Language.GetTextValue("tModLoader.BuildError", mod ?? "") + "\n" + msg;
			if (e != null)
				msg += "\n" + e;

			Interface.errorMessage.Show(msg, Interface.modSourcesID, e.HelpLink);
		}

		public void LogCompileErrors(string dllName, CompilerErrorCollection errors, string hint) {
			int warnings = 0;
			string displayError = null;
			foreach (CompilerError error in errors) {
				string errorFileName = error.FileName;
				string errorString = $"{(errorFileName==null ? null : $"{errorFileName}({error.Line},{error.Column}) : ")}error {error.ErrorNumber}: {error.ErrorText}";

				Logging.tML.Logger.Log(null, error.IsWarning ? Level.Warn : Level.Error, errorString, null);
				if (error.IsWarning)
					warnings++;
				else if (displayError == null)
					displayError = errorString;
			}
			string msg = Language.GetTextValue("tModLoader.CompileError", dllName, errors.Count - warnings, warnings);
			if (hint != null)
				msg += "\n" + hint;

			Interface.errorMessage.Show(msg + "\n\n" + displayError, Interface.modSourcesID);
		}
	}
}
