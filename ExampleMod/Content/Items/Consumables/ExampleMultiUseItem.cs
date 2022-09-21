using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Content.Items.Consumables
{
	public class ExampleMultiUseItem : ModItem
	{
		public static int MaxUses = 4;
		public int useCount;

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 26;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item3;
			Item.maxStack = 30;
			Item.consumable = true;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(gold: 1);
			Item.buffType = BuffID.Regeneration;
			Item.buffTime = 120;
		}

		public override void SaveData(TagCompound tag) {
			tag["useCount"] = useCount;
		}

		public override void LoadData(TagCompound tag) {
			useCount = tag.Get<int>("useCount");
		}

		public override void NetSend(BinaryWriter writer) {
			writer.Write(useCount);
		}

		public override void NetReceive(BinaryReader reader) {
			useCount = reader.ReadInt32();
		}

		public override bool ConsumeItem(Player player) {
			useCount++;
			if (useCount == MaxUses) {
				useCount = 0;
				return true;
			}

			return false;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (useCount == 0)
				return;

			Vector2 spriteSize = frame.Size() * scale;

			spriteBatch.Draw(TextureAssets.MagicPixel.Value,
				position: new Vector2(position.X, position.Y + spriteSize.Y * 0.9f),
				sourceRectangle: new Rectangle(0, 0, 1, 1),
				Color.Red,
				rotation: 0f,
				origin: Vector2.Zero,
				scale: new Vector2(spriteSize.X * (MaxUses - useCount) / MaxUses , 2f),
				SpriteEffects.None,
				layerDepth: 0f);
		}

		public override void OnStack(Item decrease, int numberToBeTransfered) {
			if (Item.stack == 0) {
				// work-around, the stack has just been split via cloning, so we need to clear the useCount
				useCount = 0;
			}

			var incoming = (ExampleMultiUseItem)decrease.ModItem;
			useCount += incoming.useCount;
			if (useCount >= MaxUses) {
				Item.stack--;
				useCount -= MaxUses;
			}

			incoming.useCount = 0;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
