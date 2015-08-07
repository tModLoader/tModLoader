using System;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UILoadMods : UIState
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

		internal void SetProgressFinding()
		{
			loadProgress.SetText("Finding Mods...");
			loadProgress.SetProgress(0f);
		}

		internal void SetProgressReading(string mod, int num, int max)
		{
			loadProgress.SetText("Reading: " + mod);
			loadProgress.SetProgress((float)num / (float)max);
		}

		internal void SetProgressInit(string mod, int num, int max)
		{
			loadProgress.SetText("Initializing: " + mod);
			loadProgress.SetProgress((float)num / (float)max);
		}

		internal void SetProgressSetup(float progress)
		{
			loadProgress.SetText("Setting up...");
			loadProgress.SetProgress(progress);
		}

		internal void SetProgressLoad(string mod, int num, int max)
		{
			loadProgress.SetText("Loading Mod: " + mod);
			loadProgress.SetProgress((float)num / (float)max);
		}

		internal void SetProgressRecipes()
		{
			loadProgress.SetText("Adding Recipes...");
			loadProgress.SetProgress(0f);
		}
	}
}
