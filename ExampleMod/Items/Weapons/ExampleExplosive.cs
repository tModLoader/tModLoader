using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;
using static Terraria.ModLoader.ModContent;
using System;

namespace ExampleMod.Items.Weapons
{
	internal class ExampleExplosive : ModItem
	{
		public override bool Autoload(ref string name)
		{
			IL.Terraria.Main.UpdateTime_SpawnTownNPCs += HookUpdateTime_SpawnTownNPCs;
			return base.Autoload(ref name);
		}
		//Trying to find the if statement that checks the players inventory for explosives and add Example explosive as one of them
		private void HookUpdateTime_SpawnTownNPCs(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);
			//Try to find where the item id for the Bomb is pushed onto stack
			if (!cursor.TryGotoNext(MoveType.Before,
				i => i.MatchLdcI4(166)))
				return;
			//Move to the start of the if statement 
			if (!cursor.TryGotoPrev(MoveType.Before,
			i => i.MatchLdsfld(typeof(Main).GetField(nameof(Main.player)))))
				return;
			//We need to create another cursor to jump to when our check is true
			ILCursor retCursor = cursor.Clone();
			//Push the player field onto the stack
			cursor.Emit(Ldsfld, typeof(Main).GetField(nameof(Main.player)));
			cursor.Emit(Ldloc, 37);
			cursor.Emit(Ldelem_Ref);
			//Here we check if the player at the current index has ExampleExplosive 
			cursor.EmitDelegate<Func<Player, bool>>(player => player.GetModPlayer<ExamplePlayer>().exampleExplosive);
			//We have to find the flag that allows the demolitionist to spawn so we can jump to it if our delegate returns true
			retCursor.TryGotoNext(i => i.Match(Ldc_I4_1));
			cursor.Emit(Brtrue, retCursor.MarkLabel());
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault("Example Explosive");
		}

		public override void SetDefaults()
		{
			item.useStyle = 1;
			item.shootSpeed = 12f;
			item.shoot = ProjectileType<Projectiles.ExampleExplosive>();
			item.width = 8;
			item.height = 28;
			item.maxStack = 30;
			item.consumable = true;
			item.UseSound = SoundID.Item1;
			item.useAnimation = 40;
			item.useTime = 40;
			item.noUseGraphic = true;
			item.noMelee = true;
			item.value = Item.buyPrice(0, 0, 20, 0);
			item.rare = 1;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ExampleItem>());
			recipe.AddIngredient(ItemID.Dynamite);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void UpdateInventory(Player player)
		{
			if (player.HasItem(ModContent.ItemType<ExampleExplosive>()))
			{
				player.GetModPlayer<ExamplePlayer>().exampleExplosive = true;
			}
			base.UpdateInventory(player);
		}
	}
}
