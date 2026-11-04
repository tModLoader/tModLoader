using ExampleMod.Content.Biomes;
using ExampleMod.Content.Items.Placeable.LiquidBuckets;
using ExampleMod.Content.Liquids;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalItems
{
	public class VanillaBucketUse : GlobalItem
	{
		/// <summary>
		/// Example of overriding vanilla bucket behavior, only work with absorbable liquid
		/// </summary>
		public override bool? OnBucketUse(Item sItem, int liquidType, out int newItemId) {
			// Simple example to show how to make vanilla bucket work with your custom liquid
			if (liquidType == ModContent.LiquidType<ExampleLiquid>()) {
				newItemId = ModContent.ItemType<ExampleLiquidBucket>();
				return true;
			}

			// Example where you can override vanilla bucket behavior for vanilla liquid
			if (liquidType == LiquidID.Water && Main.LocalPlayer.InModBiome<ExampleSurfaceBiome>()) {
				newItemId = ModContent.ItemType<ExampleComplexLiquidBucket>();
				return true;
			}

			return base.OnBucketUse(sItem, liquidType, out newItemId);
		}
	}
}
