using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIExtractMod : UIState
	{
		private UILoadProgress loadProgress;
		private int gotoMenu = 0;
		private TmodFile mod;

		private static IList<string> codeExtensions = new List<string>(ModCompile.sourceExtensions) {".dll", ".pdb"};

		public override void OnInitialize() {
			loadProgress = new UILoadProgress();
			loadProgress.Width.Set(0f, 0.8f);
			loadProgress.MaxWidth.Set(600f, 0f);
			loadProgress.Height.Set(150f, 0f);
			loadProgress.HAlign = 0.5f;
			loadProgress.VAlign = 0.5f;
			loadProgress.Top.Set(10f, 0f);
			Append(loadProgress);
		}

		public override void OnActivate()
		{
			Main.menuMode = Interface.extractModID;
			Task.Factory
				.StartNew(() => Interface.extractMod._Extract(mod))
				.ContinueWith(t => {
					var e = t.Result;
					if (e != null)
						ErrorLogger.LogException(e, "An error occured while extracting " + mod.name);
					else
						Main.menuMode = gotoMenu;
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		internal void SetMod(TmodFile mod)
		{
			this.mod = mod;
		}

		internal void SetGotoMenu(int gotoMenu)
		{
			this.gotoMenu = gotoMenu;
		}

		private Exception _Extract(TmodFile mod) {
			StreamWriter log = null;
			try {
				var dir = Path.Combine(Main.SavePath, "Mod Reader", mod.name);
				if (Directory.Exists(dir))
					Directory.Delete(dir, true);
				Directory.CreateDirectory(dir);

				log = new StreamWriter(Path.Combine(dir, "tModReader.txt")) {AutoFlush = true};

				var buildProperties = BuildProperties.ReadModFile(mod);
				if (buildProperties.hideCode)
					log.WriteLine("The modder has chosen to hide the code from tModReader.");
				else if (!buildProperties.includeSource)
					log.WriteLine("The modder has not chosen to include their source code.");
				if (buildProperties.hideResources)
					log.WriteLine("The modder has chosen to hide resources (ie. images) from tModReader.");

				log.WriteLine("Files:");
				int i = 0;
				foreach (var entry in mod) {
					var name = entry.Key;
					//this access is not threadsafe, but it should be atomic enough to not cause issues
					loadProgress.SetText(name);
					loadProgress.SetProgress(i++/(float)mod.FileCount);

					bool hidden = codeExtensions.Contains(Path.GetExtension(name))
						? buildProperties.hideCode
						: buildProperties.hideResources;

					if (hidden)
						log.Write("[not really hidden] ");
					log.WriteLine(name);

					//if (!hidden) {
						var path = Path.Combine(dir, name);
						Directory.CreateDirectory(Path.GetDirectoryName(path));
						File.WriteAllBytes(path, entry.Value);
					//}
				}
			}
			catch (Exception e) {
				log?.WriteLine(e);
				return e;
			}
			finally {
				log?.Close();
			}
			return null;
		}
	}
}
