using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Content.Items
{
	// An instanced item (a ModItem that contains instanced fields) needs to make sure that the data inside is persistent across
	// a wide range of situations, for example: Inventory movement, reforging, dropping, and loading/saving
	// The main hooks to override for this are: Clone, SaveData, LoadData, NetSend, NetReceive
	public class ExampleInstancedItem : ModItem
	{
		public Color[] colors;

		public static LocalizedText EMText { get; private set; }

		public override string Texture => "ExampleMod/Content/Items/ExampleItem";

		public override void SetStaticDefaults() {
			EMText = this.GetLocalization("EM");
		}

		public override void SetDefaults() {
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Swing;
		}

		// Clone is only needed if you store reference types. In this example, Color[] is a reference type (Array),
		// but a value type (int, Vector2, etc.) wouldn't need this
		// Reference types only:
		// In some rare cases, [CloneByReference] attribute on a field (or [field: CloneByReference] on a property) can be used
		// if sharing the same data across all instances of the item is intended, then the hook does not need to be overridden
		public override ModItem Clone(Item item) {
			ExampleInstancedItem clone = (ExampleInstancedItem)base.Clone(item); // If we had any value types, they would be automatically cloned here
			clone.colors = (Color[])colors?.Clone(); // note the ? here is important, colors may be null if spawned from other mods which don't call OnCreate
			return clone;
		}

		// The LoadData + SaveData hooks are responsible for making the data persistent between world/player saving
		// NOTE: The tag instance provided here is always empty by default.
		// Read https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound to better understand Saving and Loading data.
		public override void SaveData(TagCompound tag) {
			tag["Colors"] = colors;
		}

		public override void LoadData(TagCompound tag) {
			colors = tag.Get<Color[]>("Colors");
		}

		// The NetSend + NetReceive hooks are responsible for making the data persistent between client and server, and during reforging
		public override void NetSend(BinaryWriter writer) {
			// For most data like int, float, etc., a simple writer.Write(value) + corresponding reader method is sufficient.
			// For arrays, lists, and other sequences, the general approach is to send the length, and then send the elements one by one.
			// And on receive, read length, initialize the data structure, and read into it in the same order.
			// For more advanced data, the send/receive code varies greatly. Here, in order to send a Color, we send its bit-packed RGBA values as a uint
			// Because colors can also be null, we utilize flags and send them efficiently using BitsByte. This allows for simple control flow on both sides

			bool isNull = colors == null;
			writer.Write(new BitsByte(isNull));

			if (isNull) {
				return;
			}

			int length = colors.Length;
			writer.Write(length);
			for (int i = 0; i < length; i++) {
				writer.Write(colors[i].PackedValue);
			}
		}

		public override void NetReceive(BinaryReader reader) {
			BitsByte bits = reader.ReadByte();
			bool isNull = bits[0];

			if (isNull) {
				colors = null;
				return;
			}

			int length = reader.ReadInt32();
			colors = new Color[length];
			for (int i = 0; i < length; i++) {
				colors[i] = new Color {
					PackedValue = reader.ReadUInt32()
				};
			}
		}

		public override void OnCreated(ItemCreationContext context) {
			GenerateNewColors();
		}

		private void GenerateNewColors() {
			colors = new Color[5];
			for (int i = 0; i < 5; i++) {
				colors[i] = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.7f);
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (colors == null) // colors may be null if spawned from other mods which don't call OnCreate
				return;

			for (int i = 0; i < colors.Length; i++) {
				TooltipLine tooltipLine = new TooltipLine(Mod, "EM" + i, EMText.Format(i)) { OverrideColor = colors[i] };
				tooltips.Add(tooltipLine);
			}
		}

		public override void UseAnimation(Player player) {
			if (colors == null) {
				GenerateNewColors();
			}
			else {
				// cycle through the colors
				colors = colors.Skip(1).Concat(colors.Take(1)).ToArray();
			}
		}

		public override void AddRecipes() => CreateRecipe().AddIngredient<ExampleItem>(10).Register();
	}
}
