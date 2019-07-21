using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class allows you to customize the behavior of a custom gore. Create a new instance of this and pass it as a parameter to Mod.AddGore to customize the gore you are adding. If you are autoloading gore, then give it the same name as the gore texture.
	/// </summary>
	public class ModGore
	{
		private static int nextGore = GoreID.Count;
		public static int GoreCount => nextGore;
		internal static readonly IDictionary<string, int> gores = new Dictionary<string, int>();
		internal static readonly IDictionary<int, ModGore> modGores = new Dictionary<int, ModGore>();

		/// <summary> Allows you to copy the Update behavior of a different type of gore. This defaults to 0, which means no behavior is copied. </summary>
		public int updateType = -1;

		internal static int ReserveGoreID() {
			int reserveID = nextGore;
			nextGore++;
			return reserveID;
		}

		/// <summary>
		/// Gets the type of the custom gore corresponding to the given texture. Returns 0 if the texture does not represent a gore.
		/// </summary>
		public static int GetGoreSlot(string texture) {
			if (gores.ContainsKey(texture)) {
				return gores[texture];
			}
			else {
				return 0;
			}
		}

		//in Terraria.GameContent.ChildSafety make SafeGore internal and not readonly
		internal static void ResizeAndFillArrays() {
			Array.Resize(ref Main.goreLoaded, nextGore);
			Array.Resize(ref Main.goreTexture, nextGore);
			Array.Resize(ref ChildSafety.SafeGore, nextGore);
			Array.Resize(ref GoreID.Sets.SpecialAI, nextGore);
			Array.Resize(ref GoreID.Sets.DisappearSpeed, nextGore);
			Array.Resize(ref GoreID.Sets.DisappearSpeedAlpha, nextGore);
			for (int k = GoreID.Count; k < nextGore; k++) {
				Main.goreLoaded[k] = true;
				GoreID.Sets.DisappearSpeed[k] = 1;
				GoreID.Sets.DisappearSpeedAlpha[k] = 1;
			}
			foreach (string texture in gores.Keys) {
				Main.goreTexture[gores[texture]] = ModContent.GetTexture(texture);
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
			return ((gore.type >= 706 && gore.type <= 717) || gore.type == 943) && (gore.frame < 7 || gore.frame > 9);
		}

		/// <summary>
		/// Allows you to modify a gore's fields when it is created.
		/// </summary>
		public virtual void OnSpawn(Gore gore) {
		}

		//in Terraria.Gore.Update at beginning of if block checking for active add
		//  if(this.modGore != null && !this.modGore.Update(this)) { return; }
		/// <summary>
		/// Allows you to customize how you want this type of gore to behave. Return true to allow for vanilla gore updating to also take place; returns true by default.
		/// </summary>
		public virtual bool Update(Gore gore) {
			return true;
		}

		//at beginning of Terraria.Gore.Update add
		//  if(this.modGore != null) { Color? modColor = this.modGore.GetAlpha(this, newColor);
		//    if(modColor.HasValue) { return modColor.Value; } }
		/// <summary>
		/// Allows you to override the color this gore will draw in. Return null to draw it in the normal light color; returns null by default.
		/// </summary>
		/// <returns></returns>
		public virtual Color? GetAlpha(Gore gore, Color lightColor) {
			return null;
		}

		/// <summary>
		/// Allows you to determine whether or not this gore will draw behind tiles, etc. Returns false by default.
		/// </summary>
		public virtual bool DrawBehind(Gore gore) {
			return false;
		}
	}
}
