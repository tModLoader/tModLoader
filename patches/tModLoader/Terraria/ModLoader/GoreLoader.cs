using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader
{
	public static class GoreLoader
	{
		internal static readonly IDictionary<int, ModGore> gores = new Dictionary<int, ModGore>();

		public static int GoreCount { get; private set; } = GoreID.Count;

		/// <summary> Registers a new gore with the provided texture. </summary>
		public static void AddGoreFromTexture<T>(Mod mod, string texture) where T : ModGore, new() {
			if (mod == null)
				throw new ArgumentNullException(nameof(mod));

			if (texture == null)
				throw new ArgumentNullException(nameof(texture));
			
			if (!mod.loading)
				throw new Exception(Language.GetTextValue("tModLoader.LoadErrorNotLoading"));

			var modGore = Activator.CreateInstance<T>();

			modGore.nameOverride = Path.GetFileNameWithoutExtension(texture);
			modGore.textureOverride = texture;

			mod.AddContent(modGore);
		}

		//Called by ModGore.Register
		internal static void RegisterModGore(ModGore modGore) {
			int id = GoreCount++;

			modGore.Type = id;

			gores[id] = modGore;
		}

		internal static void AutoloadGores(Mod mod) {
			foreach (string texturePath in mod.Assets.EnumeratePaths<Texture2D>().Where(t => t.StartsWith("Gores/"))) {
				string textureKey = $"{mod.Name}/{texturePath}";

				if (!mod.TryFind<ModGore>($"{mod.Name}/{Path.GetFileName(texturePath)}", out _)) //ModGore gores will already be loaded at this point.
					AddGoreFromTexture<ModGore>(mod, textureKey);
			}
		}

		//in Terraria.GameContent.ChildSafety make SafeGore internal and not readonly
		internal static void ResizeAndFillArrays() {
			//Textures
			Array.Resize(ref TextureAssets.Gore, GoreCount);

			//Sets
			LoaderUtils.ResetStaticMembers(typeof(GoreID), true);
			Array.Resize(ref ChildSafety.SafeGore, GoreCount);

			for (int k = GoreID.Count; k < GoreCount; k++) {
				GoreID.Sets.DisappearSpeed[k] = 1;
				GoreID.Sets.DisappearSpeedAlpha[k] = 1;
			}

			foreach (var pair in gores) {
				TextureAssets.Gore[pair.Key] = ModContent.GetTexture(pair.Value.Texture);
			}
		}

		internal static void Unload() {
			gores.Clear();
			
			GoreCount = GoreID.Count;
		}

		internal static ModGore SetupGore(Gore gore) {
			gores.TryGetValue(gore.type, out var modGore);
			
			return modGore;
		}

		internal static void SetupUpdateType(Gore gore) {
			if (gore.ModGore != null && gore.ModGore.updateType > 0) {
				gore.realType = gore.type;
				gore.type = gore.ModGore.updateType;
			}
		}

		internal static void TakeDownUpdateType(Gore gore) {
			if (gore.realType > 0) {
				gore.type = gore.realType;
				gore.realType = 0;
			}
		}

		//in Terraria.Main.DrawGore and DrawGoreBehind replace type checks with this
		internal static bool DrawBackGore(Gore gore) {
			if (gore.ModGore != null) {
				return gore.ModGore.DrawBehind(gore);
			}

			//TODO: Whatever calls this is a very bad patch. Don't move vanilla code, reuse it where it is instead.
			return (((gore.type >= 706 && gore.type <= 717) || gore.type == 943 || gore.type == 1147 || (gore.type >= 1160 && gore.type <= 1162)) && (gore.frame < 7 || gore.frame > 9));
		}
	}
}
