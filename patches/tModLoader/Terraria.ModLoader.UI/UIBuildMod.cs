using System;
using System.IO;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIBuildMod : UIState
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

		internal void SetProgress(int num, int max)
		{
			loadProgress.SetProgress((float)num / (float)max);
		}

		internal void SetReading()
		{
			loadProgress.SetText("Reading Properties: " + Path.GetFileName(ModLoader.modToBuild));
		}

		internal void SetCompiling()
		{
			loadProgress.SetText("Compiling " + Path.GetFileName(ModLoader.modToBuild) + "...");
		}

		internal void SetBuildText()
		{
			loadProgress.SetText("Building " + Path.GetFileName(ModLoader.modToBuild) + "...");
		}
	}
}
