using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	// TODO: yet another progress bar
	internal class UIExtractMod : UIState
	{
		private UILoadProgress loadProgress;
		private int gotoMenu;
		private LocalMod mod;

		private static IList<string> codeExtensions = new List<string>(ModCompile.sourceExtensions) { ".dll", ".pdb" };

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
		}

		public override void OnActivate() {
			Main.menuMode = Interface.extractModID;
			// I expect this will move out of Activate during progress UI merger
			Task.Factory.StartNew(() => {
				Interface.extractMod._Extract(); // Interface.extractMod is just `this`
			}).ContinueWith(t => {
				var exception = t?.Exception;//TODO can you even continue on an exceptional task?
				if (exception != null)
					Logging.tML.Error(Language.GetTextValue("tModLoader.ExtractErrorWhileExtractingMod", mod.Name), exception);
				else
					Main.menuMode = gotoMenu;
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		internal void Show(LocalMod mod, int gotoMenu) {
			this.mod = mod;
			this.gotoMenu = gotoMenu;
			Main.menuMode = Interface.extractModID;
		}

		private Exception _Extract() {
			StreamWriter log = null;
			IDisposable modHandle = null;
			try {
				var dir = Path.Combine(Main.SavePath, "Mod Reader", mod.Name);
				if (Directory.Exists(dir))
					Directory.Delete(dir, true);
				Directory.CreateDirectory(dir);

				log = new StreamWriter(Path.Combine(dir, "tModReader.txt")) { AutoFlush = true };

				if (mod.properties.hideCode)
					log.WriteLine(Language.GetTextValue("tModLoader.ExtractHideCodeMessage"));
				else if (!mod.properties.includeSource)
					log.WriteLine(Language.GetTextValue("tModLoader.ExtractNoSourceCodeMessage"));
				if (mod.properties.hideResources)
					log.WriteLine(Language.GetTextValue("tModLoader.ExtractHideResourcesMessage"));

				log.WriteLine(Language.GetTextValue("tModLoader.ExtractFileListing"));

				int i = 0;
				modHandle = mod.modFile.EnsureOpen();
				mod.modFile.ForEach((name, len, getStream) => {
					ContentConverters.Reverse(ref name, out var converter);

					//this access is not threadsafe, but it should be atomic enough to not cause issues
					loadProgress.SetText(name);
					loadProgress.SetProgress(i++ / (float)mod.modFile.Count);

					if (name == "tModReader.txt")
						return;

					bool hidden = codeExtensions.Contains(Path.GetExtension(name))
						? mod.properties.hideCode
						: mod.properties.hideResources;

					if (hidden)
						log.Write("[hidden] ");
					log.WriteLine(name);
					if (hidden)
						return;

					var path = Path.Combine(dir, name);
					Directory.CreateDirectory(Path.GetDirectoryName(path));

					using (var dst = File.OpenWrite(path))
					using (var src = getStream()) {
						if (converter != null)
							converter(src, dst);
						else
							src.CopyTo(dst);
					}
				});
			}
			catch (Exception e) {
				log?.WriteLine(e);
				return e;
			}
			finally {
				log?.Close();
				modHandle?.Dispose();
			}
			return null;
		}
	}
}
