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
		public static void AddGoreFromTexture<TGore>(Mod mod, string texture) where TGore : ModGore, new() {
			if (mod == null)
				throw new ArgumentNullException(nameof(mod));

			if (texture == null)
				throw new ArgumentNullException(nameof(texture));
			
			if (!mod.loading)
				throw new Exception(Language.GetTextValue("tModLoader.LoadErrorNotLoading"));

			mod.AddContent(new TGore {
				nameOverride = Path.GetFileNameWithoutExtension(texture),
				textureOverride = texture
			});
		}

		//Called by ModGore.Register
		internal static void RegisterModGore(ModGore modGore) {
			int id = GoreCount++;

			modGore.Type = id;

			gores[id] = modGore;
		}

		internal static void AutoloadGores(Mod mod) {
			foreach (string fullTexturePath in mod.RootContentSource.EnumerateAssets().Where(t => t.Contains("Gores/"))) {
				string texturePath = Path.ChangeExtension(fullTexturePath, null);

				// ModGore gores will already be loaded at this point.
				if (!mod.TryFind<ModGore>(Path.GetFileName(texturePath), out _)) {
					string textureKey = $"{mod.Name}/{texturePath}";

					AddGoreFromTexture<SimpleModGore>(mod, textureKey);
				}
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
				TextureAssets.Gore[pair.Key] = ModContent.Request<Texture2D>(pair.Value.Texture);
			}
		}

		internal static void Unload() {
			gores.Clear();
			
			GoreCount = GoreID.Count;
		}

		internal static ModGore GetModGore(int type) {
			gores.TryGetValue(type, out var modGore);
			
			return modGore;
		}

		internal static void SetupUpdateType(Gore gore) {
			if (gore.ModGore != null && gore.ModGore.UpdateType > 0) {
				gore.realType = gore.type;
				gore.type = gore.ModGore.UpdateType;
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
