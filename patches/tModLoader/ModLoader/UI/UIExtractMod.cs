using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.Utilities;

namespace Terraria.ModLoader.UI
{
	internal class UIExtractMod : UIProgress
	{
		private const string LOG_NAME = "extract.log";
		private LocalMod mod;
		private static readonly IList<string> codeExtensions = new List<string>(ModCompile.sourceExtensions) {
			".dll", ".pdb"
		};

		private CancellationTokenSource _cts;

		public override void OnActivate() {
			base.OnActivate();

			_cts = new CancellationTokenSource();
			OnCancel += () => {
				// TODO should cancel also clean up the extract folder?
				_cts.Cancel();
			};
			Task.Run(Extract, _cts.Token);
		}

		internal void Show(LocalMod mod, int gotoMenu) {
			this.mod = mod;
			this.gotoMenu = gotoMenu;
			Main.menuMode = Interface.extractModID;
		}

		private Task Extract() {
			StreamWriter log = null;
			IDisposable modHandle = null;
			try {
				var dir = Path.Combine(Main.SavePath, "Mod Reader", mod.Name);
				if (Directory.Exists(dir))
					Directory.Delete(dir, true);
				Directory.CreateDirectory(dir);

				log = new StreamWriter(Path.Combine(dir, LOG_NAME)) { AutoFlush = true };

				if (mod.properties.hideCode)
					log.WriteLine(Language.GetTextValue("tModLoader.ExtractHideCodeMessage"));
				else if (!mod.properties.includeSource)
					log.WriteLine(Language.GetTextValue("tModLoader.ExtractNoSourceCodeMessage"));
				if (mod.properties.hideResources)
					log.WriteLine(Language.GetTextValue("tModLoader.ExtractHideResourcesMessage"));

				log.WriteLine(Language.GetTextValue("tModLoader.ExtractFileListing"));

				int i = 0;
				modHandle = mod.modFile.Open();
				foreach (var entry in mod.modFile) {
					_cts.Token.ThrowIfCancellationRequested();
					var name = entry.Name;
					ContentConverters.Reverse(ref name, out var converter);

					//this access is not threadsafe, but it should be atomic enough to not cause issues
					DisplayText = name;
					Progress = i++ / (float)mod.modFile.Count;

					if (name == LOG_NAME)
						continue;

					bool hidden = codeExtensions.Contains(Path.GetExtension(name))
						? mod.properties.hideCode
						: mod.properties.hideResources;

					if (hidden) {
						log.Write($"[hidden] {name}");
						continue;
					} else {
						log.WriteLine(name);
					}

					var path = Path.Combine(dir, name);
					Directory.CreateDirectory(Path.GetDirectoryName(path));

					using (var dst = File.OpenWrite(path))
					using (var src = mod.modFile.GetStream(entry)) if (converter != null)
							converter(src, dst);
						else
							src.CopyTo(dst);

					// Copy the dll to ModLoader\references\mods for easy collaboration.
					if (name == "All.dll" || PlatformUtilities.IsXNA ? name == "Windows.dll" || name == $"{mod.Name}.XNA.dll" : name == "Mono.dll" || name == $"{mod.Name}.FNA.dll") {
						string modReferencesPath = Path.Combine(Program.SavePath, "references", "mods");
						Directory.CreateDirectory(modReferencesPath);
						File.Copy(path, Path.Combine(modReferencesPath, $"{mod.Name}_v{mod.modFile.version}.dll"), true);
						log?.WriteLine("You can find this mod's .dll files under ModLoader\\references\\mods for easy mod collaboration!");
					}
					if (name == $"{mod.Name}.xml" && !mod.properties.hideCode) {
						string modReferencesPath = Path.Combine(Program.SavePath, "references", "mods");
						Directory.CreateDirectory(modReferencesPath);
						File.Copy(path, Path.Combine(modReferencesPath, $"{mod.Name}_v{mod.modFile.version}.xml"), true);
						log?.WriteLine("You can find this mod's documentation .xml file under ModLoader\\references\\mods for easy mod collaboration!");
					}
				};
				Process.Start(dir);
			}
			catch (OperationCanceledException e) {
				log?.WriteLine("Extraction was cancelled.");
				return Task.FromResult(false);
			}
			catch (Exception e) {
				log?.WriteLine(e);
				Logging.tML.Error(Language.GetTextValue("tModLoader.ExtractErrorWhileExtractingMod", mod.Name), e);
				Main.menuMode = gotoMenu;
				return Task.FromResult(false);
			}
			finally {
				log?.Close();
				modHandle?.Dispose();
			}
			Main.menuMode = gotoMenu;
			return Task.FromResult(true);
		}
	}
}
