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
			if (!Main.dedServ)
			{
				loadProgress.SetText("Finding Mods...");
				loadProgress.SetProgress(0f);
			}
		}

		internal void SetProgressCompatibility(string mod, int num, int max)
		{
			if (!Main.dedServ)
			{
				loadProgress.SetText("Compatibilizing: " + mod);
				loadProgress.SetProgress((float)num / (float)max);
			}
		}

		internal void SetProgressReading(string mod, int num, int max)
		{
			if (!Main.dedServ)
			{
				loadProgress.SetText("Reading: " + mod);
				loadProgress.SetProgress((float)num / (float)max);
			}
			else if (num == 0)
			{
				Console.WriteLine("Reading: " + mod);
			}
		}

		internal void SetProgressInit(string mod, int num, int max)
		{
			if (!Main.dedServ)
			{
				loadProgress.SetText("Initializing: " + mod);
				loadProgress.SetProgress((float)num / (float)max);
			}
		}

		internal void SetProgressSetup(float progress)
		{
			if (!Main.dedServ)
			{
				loadProgress.SetText("Setting up...");
				loadProgress.SetProgress(progress);
			}
		}

		internal void SetProgressLoad(string mod, int num, int max)
		{
			if (!Main.dedServ)
			{
				loadProgress.SetText("Loading Mod: " + mod);
				loadProgress.SetProgress((float)num / (float)max);
			}
		}

		internal void SetProgressRecipes()
		{
			if (!Main.dedServ)
			{
				loadProgress.SetText("Adding Recipes...");
				loadProgress.SetProgress(0f);
			}
		}
	}
}
