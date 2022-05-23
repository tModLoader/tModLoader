using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader
{
	partial class Mod
	{
		internal bool loading;

		private readonly Queue<Task> AsyncLoadQueue = new Queue<Task>();

		//Entities
		internal readonly IDictionary<Tuple<string, EquipType>, EquipTexture> equipTextures = new Dictionary<Tuple<string, EquipType>, EquipTexture>();
		internal readonly IList<ILoadable> content = new List<ILoadable>();

		internal void SetupContent() {
			LoaderUtils.ForEachAndAggregateExceptions(GetContent<ModType>(), e => e.SetupContent());
		}

		internal void UnloadContent() {
			Unload();

			foreach (var loadable in content.Reverse()) {
				loadable.Unload();
			}
			content.Clear();

			equipTextures.Clear();

			Assets?.Dispose();
		}

		internal void Autoload() {
			if (Code == null)
				return;

			Interface.loadMods.SubProgressText = Language.GetTextValue("tModLoader.MSFinishingResourceLoading");
			while (AsyncLoadQueue.Count > 0)
				AsyncLoadQueue.Dequeue().Wait();

			LocalizationLoader.Autoload(this);
			ModSourceBestiaryInfoElement = new GameContent.Bestiary.ModSourceBestiaryInfoElement(this, DisplayName);

			if (ContentAutoloadingEnabled) {
				var loadableTypes = AssemblyManager.GetLoadableTypes(Code)
					.Where(t => !t.IsAbstract && !t.ContainsGenericParameters)
					.Where(t => t.IsAssignableTo(typeof(ILoadable)))
					.Where(t => t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null) // has default constructor
					.Where(t => AutoloadAttribute.GetValue(t).NeedsAutoloading)
					.OrderBy(type => type.FullName, StringComparer.InvariantCulture);

				LoaderUtils.ForEachAndAggregateExceptions(loadableTypes, t => AddContent((ILoadable)Activator.CreateInstance(t, true)));
			}

			// Skip loading client assets if this is a dedicated server;
			if (Main.dedServ)
				return;

			if (GoreAutoloadingEnabled)
				GoreLoader.AutoloadGores(this);

			if (SoundAutoloadingEnabled)
				SoundLoader.AutoloadSounds(this);

			if (MusicAutoloadingEnabled)
				MusicLoader.AutoloadMusic(this);

			if (BackgroundAutoloadingEnabled)
				BackgroundTextureLoader.AutoloadBackgrounds(this);
		}

		internal void PrepareAssets()
		{
			fileHandle = File?.Open();
			RootContentSource = CreateDefaultContentSource();
			Assets = new AssetRepository(Main.instance.Services.Get<AssetReaderCollection>(), new[] { RootContentSource }) {
				AssetLoadFailHandler = Main.OnceFailedLoadingAnAsset
			};
		}

		internal void TransferAllAssets() {
			Assets.TransferAllAssets();
		}
	}
}
