using log4net.Core;
using System;
using System.CodeDom.Compiler;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIBuildMod : UIState, ModCompile.IBuildStatus
	{
		private UILoadProgress loadProgress;

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
		}

		public void SetProgress(int num, int max)
		{
			loadProgress.SetProgress((float)num / (float)max);
		}

		public void SetStatus(string msg)
		{
			Logging.tML.Info(msg);
			loadProgress.SetText(msg);
		}

		public void LogError(string mod, string msg, Exception e = null)
		{
			Logging.tML.Error(msg, e);

			msg = Language.GetTextValue("tModLoader.BuildError", mod) + "\n" + msg;
			if (e != null)
				msg += "\n" + e;
			Interface.errorMessage.SetMessage(msg);
			Interface.errorMessage.SetGotoMenu(Interface.modSourcesID);
			Main.menuMode = Interface.errorMessageID;
		}

		public void LogCompileErrors(string mod, CompilerErrorCollection errors, string hint)
		{
			int warnings = 0;
			CompilerError displayError = null;
			foreach (CompilerError error in errors)
			{
				Logging.tML.Logger.Log(null, error.IsWarning ? Level.Warn : Level.Error, error, null);
				if (error.IsWarning)
					warnings++;
				else if (displayError == null)
					displayError = error;
			}
			var msg = Language.GetTextValue("tModLoader.CompileError", mod, errors.Count - warnings, warnings);
			if (hint != null)
				msg += "\n" + hint;
			Interface.errorMessage.SetMessage(msg + "\n\n" + displayError);
			Interface.errorMessage.SetGotoMenu(Interface.modSourcesID);
		}
	}
}
