using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIExtractMod : UIState
	{
		private UILoadProgress loadProgress;
		private int gotoMenu;
		private LocalMod mod;

		private static IList<string> codeExtensions = new List<string>(ModCompile.sourceExtensions) { ".dll", ".pdb" };

		public override void OnInitialize()
		{
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
				.StartNew(() => {
					Interface.extractMod.mod.modFile.Read(TmodFile.LoadedState.Streaming, updateFileCountOnly: true);
					Interface.extractMod._Extract();
				})
				.ContinueWith(t =>
				{
					var exception = t?.Exception;
					if (exception != null)
						Logging.tML.Error(Language.GetTextValue("tModLoader.ExtractErrorWhileExtractingMod", mod.Name), exception);
					else
						Main.menuMode = gotoMenu;
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		internal void SetMod(LocalMod mod)
		{
			this.mod = mod;
		}

		internal void SetGotoMenu(int gotoMenu)
		{
			this.gotoMenu = gotoMenu;
		}

		private Exception _Extract()
		{
			StreamWriter log = null;
			try
			{
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

				void WriteFile(string name, byte[] content)
				{
					//this access is not threadsafe, but it should be atomic enough to not cause issues
					loadProgress.SetText(name);
					loadProgress.SetProgress(i++ / (float)mod.modFile.FileCount);

					bool hidden = codeExtensions.Contains(Path.GetExtension(name))
						? mod.properties.hideCode
						: mod.properties.hideResources;

					if (hidden)
						log.Write("[hidden] ");
					log.WriteLine(name);

					if (!hidden)
					{
						if (name == "Info")
							name = "build.txt";

						var path = Path.Combine(dir, name);
						Directory.CreateDirectory(Path.GetDirectoryName(path));
						File.WriteAllBytes(path, content);
					}
				}

				mod.modFile.Read(TmodFile.LoadedState.Streaming, (name, len, reader) =>
				{
					byte[] data = reader.ReadBytes(len);

					// check if subject is rawimg, then read it as rawimg and convert back to png
					if (name.EndsWith(".rawimg"))
					{
						using (var ms = new MemoryStream(data))
						{
							var img = ImageIO.RawToTexture2D(Main.instance.GraphicsDevice, ms);
							using (var pngstream = new MemoryStream())
							{
								img.SaveAsPng(pngstream, img.Width, img.Height);
								data = pngstream.ToArray();
							}
						}

						name = Path.ChangeExtension(name, "png");
					}

					WriteFile(name, data);
				});

				foreach (var entry in mod.modFile)
					WriteFile(entry.Key, entry.Value);
			}
			catch (Exception e)
			{
				log?.WriteLine(e);
				return e;
			}
			finally
			{
				log?.Close();
				mod?.modFile.UnloadAssets();
			}
			return null;
		}
	}
}
