using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Content.Items
{
	public class ExampleStackableDurabilityItem : ModItem
	{
		// 0 to 1
		// All items in the stack have the same durability
		// Durability is combined and averaged when stacking
		public float durability;
		public static LocalizedText DurabilityText { get; private set; }

		public override void SetStaticDefaults() {
			DurabilityText = this.GetLocalization("Durability");
		}

		public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack; // This item is stackable, otherwise the example wouldn't work
			Item.width = 8;
			Item.height = 8;
		}

		public override void SaveData(TagCompound tag) {
			tag["durability"] = durability;
		}

		public override void LoadData(TagCompound tag) {
			durability = tag.Get<float>("durability");
		}

		public override void NetSend(BinaryWriter writer) {
			writer.Write(durability);
		}

		public override void NetReceive(BinaryReader reader) {
			durability = reader.ReadSingle();
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (durability == 1) {
				return;
			}

			Vector2 spriteSize = frame.Size() * scale;

			spriteBatch.Draw(TextureAssets.MagicPixel.Value,
				position: new Vector2(position.X, position.Y + spriteSize.Y * 0.9f),
				sourceRectangle: new Rectangle(0, 0, 1, 1),
				Color.LightGreen,
				rotation: 0f,
				origin: Vector2.Zero,
				scale: new Vector2(spriteSize.X * durability, 2f),
				SpriteEffects.None,
				layerDepth: 0f);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.Add(new TooltipLine(Mod, "ExampleStackableDurabilityItem", DurabilityText.Format((int)(durability * 100))) { OverrideColor = Color.LightGreen });
		}

		private static float WeightedAverage(float durability1, int stack1, float durability2, int stack2) {
			return (durability1 * stack1 + durability2 * stack2) / (stack1 + stack2);
		}

		public override void OnStack(Item source, int numToTransfer) {
			var incomingDurability = ((ExampleStackableDurabilityItem)source.ModItem).durability;
			durability = WeightedAverage(durability, Item.stack, incomingDurability, numToTransfer);
		}

		// SplitStack:  This example does not need to use SplitStack because durability will be the intended value from being cloned.

		public override void OnCreated(ItemCreationContext context) {
			if (context is RecipeItemCreationContext) {
				durability = Main.rand.NextFloat();
			}
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
