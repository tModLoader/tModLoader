using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = tModCodeAssist.Tests.Verifier.Analyzer<tModCodeAssist.Analyzers.ChangeMagicNumberToIDAnalyzer>.CodeFixer<tModCodeAssist.CodeFixes.ChangeMagicNumberToIDCodeFixProvider>;

namespace tModCodeAssist.Tests.CodeFixes;

[TestClass]
public sealed class ChangeMagicNumberToIDUnitTest
{
	[TestMethod]
	public async Task Test_Assignment()
	{
		await VerifyCS.Run(
			"""
			using Terraria;

			var item = new Item();
			item.createTile = [|42|];
			item.type = [|42|];
			item.useStyle = [|4|];
			item.shoot = [|42|];
			item.rare = [|4|];
			item.useTime = 69;
			var player = new Player();
			player.cursorItemIconID = [|327|];

			Terraria.ModLoader.ModTile modTile = null;
			modTile.DustType = [|1|];
			Terraria.ModLoader.ModWall modWall = null;
			modWall.DustType = [|2|];

			Main.tile[10, 20].TileType = [|490|];
			Main.tile[20, 30].WallType = [|276|];
			""",
			"""
			using Terraria;
			using Terraria.ID;
			
			var item = new Item();
			item.createTile = TileID.HangingLanterns;
			item.type = ItemID.Shuriken;
			item.useStyle = ItemUseStyleID.HoldUp;
			item.shoot = ProjectileID.SandBallGun;
			item.rare = ItemRarityID.LightRed;
			item.useTime = 69;
			var player = new Player();
			player.cursorItemIconID = ItemID.GoldenKey;

			Terraria.ModLoader.ModTile modTile = null;
			modTile.DustType = DustID.Stone;
			Terraria.ModLoader.ModWall modWall = null;
			modWall.DustType = DustID.Grass;

			Main.tile[10, 20].TileType = TileID.WeatherVane;
			Main.tile[20, 30].WallType = WallID.Corruption1Echo;
			""");
	}

	[TestMethod]
	public async Task Test_Binary()
	{
		await VerifyCS.Run(
			"""
			using Terraria;

			_ = new Item().type == [|1|];
			_ = new Projectile().type == [|444|];
			""",
			"""
			using Terraria;
			using Terraria.ID;
			
			_ = new Item().type == ItemID.IronPickaxe;
			_ = new Projectile().type == ProjectileID.Xenopopper;
			""");
	}

	[TestMethod]
	public async Task Test_Invocation()
	{
		await VerifyCS.Run(
			"""
			using Microsoft.Xna.Framework;
			using Terraria;
			using Terraria.ID;

			var recipe = Recipe.Create([|420|]);
			recipe.AddTile([|412|]);
			recipe.AddIngredient([|430|]);
			NetMessage.SendData(number: 42, number2: 42, number5: 42, msgType: [|42|]);
			Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), Main.LocalPlayer.Top, new Vector2(0, -Main.rand.NextFloat(2f, 4f)).RotatedByRandom(0.3f), [|60|], 0, 0, Main.myPlayer);
			new Item().CloneDefaults([|5450|]);
			Dust.NewDust(Vector2.Zero, 1, 2, [|3|], 4, 5, 6, Color.Red, 7);
			Dust.NewDustDirect(Vector2.Zero, 1, 2, [|75|], 4, 5);
			Dust.NewDustPerfect(Vector2.Zero, [|76|]);
			""",
			"""
			using Microsoft.Xna.Framework;
			using Terraria;
			using Terraria.ID;
			
			var recipe = Recipe.Create(ItemID.CobaltBrickWall);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.AddIngredient(ItemID.PurpleTorch);
			NetMessage.SendData(number: 42, number2: 42, number5: 42, msgType: MessageID.PlayerMana);
			Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), Main.LocalPlayer.Top, new Vector2(0, -Main.rand.NextFloat(2f, 4f)).RotatedByRandom(0.3f), ProjectileID.MythrilDrill, 0, 0, Main.myPlayer);
			new Item().CloneDefaults(ItemID.RainbowMossBlockWall);
			Dust.NewDust(Vector2.Zero, 1, 2, DustID.GrassBlades, 4, 5, 6, Color.Red, 7);
			Dust.NewDustDirect(Vector2.Zero, 1, 2, DustID.CursedTorch, 4, 5);
			Dust.NewDustPerfect(Vector2.Zero, DustID.Snow);
			""");
	}

	[TestMethod]
	public async Task Test_CaseSwitchLabel()
	{
		await VerifyCS.Run(
			"""
			using Terraria;

			switch (new NPC().type) {
				case [|420|]:
					break;
			}
			""",
			"""
			using Terraria;
			using Terraria.ID;
			
			switch (new NPC().type) {
				case NPCID.NebulaBrain:
					break;
			}
			""");
	}

	[TestMethod]
	public async Task Test_ArrayIndexing()
	{
		await VerifyCS.Run(
			"""
			using Terraria;
			using Terraria.ID;

			ItemID.Sets.StaffMinionSlotsRequired[[|1309|]] = 2f;
			TileID.Sets.TouchDamageHot[[|2|]] = true;
			TileID.Sets.Conversion.Sand[[|461|]] = true;
			""",
			"""
			using Terraria;
			using Terraria.ID;
			
			ItemID.Sets.StaffMinionSlotsRequired[ItemID.SlimeStaff] = 2f;
			TileID.Sets.TouchDamageHot[TileID.Grass] = true;
			TileID.Sets.Conversion.Sand[TileID.SandDrip] = true;
			""");
	}
}
