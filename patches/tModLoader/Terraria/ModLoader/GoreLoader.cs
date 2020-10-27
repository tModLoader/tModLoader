using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader
{
	public static class GoreLoader
	{
		internal static readonly IDictionary<string, int> gores = new Dictionary<string, int>();
		internal static readonly IDictionary<int, ModGore> modGores = new Dictionary<int, ModGore>();

		private static int nextGore = GoreID.Count;

		public static int GoreCount => nextGore;

		internal static int ReserveGoreID() => nextGore++;

		internal static void AddGore(string texture, ModGore modGore = null) {
			int id = ReserveGoreID();

			gores[texture] = id;

			if (modGore != null) {
				modGores[id] = modGore;
			}
		}

		//in Terraria.GameContent.ChildSafety make SafeGore internal and not readonly
		internal static void ResizeAndFillArrays() {
			//Textures
			Array.Resize(ref TextureAssets.Gore, nextGore);

			//Sets
			LoaderUtils.ResetStaticMembers(typeof(GoreID), true);
			Array.Resize(ref ChildSafety.SafeGore, nextGore);

			for (int k = GoreID.Count; k < nextGore; k++) {
				GoreID.Sets.DisappearSpeed[k] = 1;
				GoreID.Sets.DisappearSpeedAlpha[k] = 1;
			}

			foreach (string texture in gores.Keys) {
				TextureAssets.Gore[gores[texture]] = ModContent.GetTexture(texture);
			}
		}

		internal static void AutoloadGores(Mod mod, IList<Type> modGores) {
			var modGoreNames = modGores.ToDictionary(t => t.FullName);

			foreach (string texturePath in mod.Assets.EnumeratePaths<Texture2D>().Where(t => t.StartsWith("Gores/"))) {
				ModGore modGore = null;

				if (modGoreNames.TryGetValue($"{mod.Name}.{texturePath.Replace('/', '.')}", out Type t))
					modGore = (ModGore)Activator.CreateInstance(t);
				else
					modGore = new ModGore();

				AddGore($"{mod.Name}/{texturePath}", modGore);
			}
		}

		internal static void Unload() {
			gores.Clear();
			modGores.Clear();
			nextGore = GoreID.Count;
		}

		//in Terraria.Gore add modGore property (internal set)
		//in Terraria.Gore.NewGore after resetting properties call ModGore.SetupGore(Main.gore[num]);
		internal static void SetupGore(Gore gore) {
			if (modGores.ContainsKey(gore.type)) {
				gore.modGore = modGores[gore.type];
				gore.modGore.OnSpawn(gore);
			}
			else {
				gore.modGore = null;
			}
		}

		internal static void SetupUpdateType(Gore gore) {
			if (gore.modGore != null && gore.modGore.updateType > 0) {
				gore.realType = gore.type;
				gore.type = gore.modGore.updateType;
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
			if (gore.modGore != null) {
				return gore.modGore.DrawBehind(gore);
			}

			return (((gore.type >= 706 && gore.type <= 717) || gore.type == 943 || gore.type == 1147 || (gore.type >= 1160 && gore.type <= 1162)) && (gore.frame < 7 || gore.frame > 9));
		}
	}
}
