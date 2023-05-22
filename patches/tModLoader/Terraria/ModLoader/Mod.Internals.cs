using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader;

partial class Mod
{
	internal bool loading;

	private readonly Queue<Task> AsyncLoadQueue = new Queue<Task>();

	//Entities
	internal readonly IDictionary<Tuple<string, EquipType>, EquipTexture> equipTextures = new Dictionary<Tuple<string, EquipType>, EquipTexture>();
	internal readonly IList<ILoadable> content = new List<ILoadable>();

	internal void SetupContent()
	{
		LoaderUtils.ForEachAndAggregateExceptions(GetContent<ModType>(), e => e.SetupContent());
	}

	internal void UnloadContent()
	{
		SystemLoader.OnModUnload(this);

		Unload();

		foreach (var loadable in content.Reverse()) {
			loadable.Unload();
		}
		content.Clear();

		equipTextures.Clear();

		Assets?.Dispose();
	}

	internal void Autoload()
	{
		if (Code == null)
			return;

		LocalizationLoader.Autoload(this);

		Interface.loadMods.SubProgressText = Language.GetTextValue("tModLoader.MSFinishingResourceLoading");
		while (AsyncLoadQueue.Count > 0)
			AsyncLoadQueue.Dequeue().Wait();

		ModSourceBestiaryInfoElement = new GameContent.Bestiary.ModSourceBestiaryInfoElement(this, DisplayName);

		if (ContentAutoloadingEnabled) {
			var loadableTypes = AssemblyManager.GetLoadableTypes(Code)
				.Where(t => !t.IsAbstract && !t.ContainsGenericParameters)
				.Where(t => t.IsAssignableTo(typeof(ILoadable)))
				.Where(t => t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) != null) // has default constructor
				.Where(t => AutoloadAttribute.GetValue(t).NeedsAutoloading)
				.OrderBy(type => type.FullName, StringComparer.InvariantCulture);

			LoaderUtils.ForEachAndAggregateExceptions(loadableTypes, t => AddContent((ILoadable)Activator.CreateInstance(t, true)));
		}

		// Skip loading client assets if this is a dedicated server;
		if (Main.dedServ)
			return;

		if (GoreAutoloadingEnabled)
			GoreLoader.AutoloadGores(this);

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
			AssetLoadFailHandler = OnceFailedLoadingAnAsset
		};
	}

	internal void TransferAllAssets()
	{
		initialTransferComplete = false;
		Assets.TransferAllAssets();
		initialTransferComplete = true;
		if (AssetExceptions.Count > 0) {
			if (AssetExceptions.Count == 1)
				throw AssetExceptions[0];

			if (AssetExceptions.Count > 0)
				throw new MultipleException(AssetExceptions);
		}
	}

	internal bool initialTransferComplete;
	internal List<Exception> AssetExceptions = new List<Exception>();
	internal void OnceFailedLoadingAnAsset(string assetPath, Exception e)
	{
		if (initialTransferComplete) {
			// TODO: Add a user friendly indicator/inbox for viewing these errors that happen in-game
			Logging.Terraria.Error($"Failed to load asset: \"{assetPath}\"", e);
			Terraria.UI.FancyErrorPrinter.ShowFailedToLoadAssetError(e, assetPath);
		}
		else {
			if (e is AssetLoadException AssetLoadException) {
				// Fix this once ContenSources are sane with extensions
				ICollection<string> keys = RootContentSource.EnumerateAssets().ToList();
				var cleanKeys = new List<string>();
				foreach (var key in keys) {
					string keyWithoutExtension = key.Substring(0, key.LastIndexOf("."));
					string extension = RootContentSource.GetExtension(keyWithoutExtension);
					if (extension != null) {
						cleanKeys.Add(key.Substring(0, key.LastIndexOf(extension)));
						// Should probably check RootContentSource.Rejections.IsRejected before adding to cleanKeys, but it doesn't matter because of MissingResourceException logic.
					}
				}
				var reasons = new List<string>();
				RootContentSource.Rejections.TryGetRejections(reasons); // Not technically the rejection reasons for the specific asset, but there is no current way of getting that.
				var MissingResourceException = new Exceptions.MissingResourceException(reasons, assetPath.Replace("\\", "/"), cleanKeys);
				AssetExceptions.Add(MissingResourceException);
			}
			else {
				AssetExceptions.Add(e);
			}
		}
	}
}
