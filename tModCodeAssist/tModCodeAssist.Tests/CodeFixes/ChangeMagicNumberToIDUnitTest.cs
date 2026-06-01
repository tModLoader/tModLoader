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
			item.rare = [|-1|];
			item.rare = [|4|];
			item.useTime = 69;

			var player = new Player();
			player.cursorItemIconID = [|327|];
			player.cursorItemIconID = -1;

			var mount = new Mount();
			if (mount.Type != [|12|])
			{
				if (mount._data.buff != [|168|] || mount.BuffType != [|168|])
				{
					item.buffType = [|168|];
					item.mountType = [|12|];
				}
			}

			Terraria.ModLoader.ModTile modTile = null;
			modTile.DustType = [|1|];
			Terraria.ModLoader.ModWall modWall = null;
			modWall.DustType = [|2|];

			var tile = Main.tile[10, 20];
			tile.TileType = [|490|];
			tile.WallType = [|276|];
			tile.TileColor = [|1|];
			tile.WallColor = [|1|];
			tile.LiquidType = [|1|];

			var projectile = new Projectile();
			projectile.aiStyle = [|1|];

			Terraria.ModLoader.ModProjectile modProjectile = null;
			modProjectile.AIType = [|93|];

			var npc = new NPC();
			npc.aiStyle = [|18|];

			Terraria.ModLoader.ModNPC modNPC = null;
			modNPC.AIType = [|103|];
			modNPC.AnimationType = [|64|];
			""",
			"""
			using Terraria;
			using Terraria.ID;

			var item = new Item();
			item.createTile = TileID.HangingLanterns;
			item.type = ItemID.Shuriken;
			item.useStyle = ItemUseStyleID.HoldUp;
			item.shoot = ProjectileID.SandBallGun;
			item.rare = ItemRarityID.Gray;
			item.rare = ItemRarityID.LightRed;
			item.useTime = 69;

			var player = new Player();
			player.cursorItemIconID = ItemID.GoldenKey;
			player.cursorItemIconID = -1;

			var mount = new Mount();
			if (mount.Type != MountID.CuteFishron)
			{
				if (mount._data.buff != BuffID.CuteFishronMount || mount.BuffType != BuffID.CuteFishronMount)
				{
					item.buffType = BuffID.CuteFishronMount;
					item.mountType = MountID.CuteFishron;
				}
			}

			Terraria.ModLoader.ModTile modTile = null;
			modTile.DustType = DustID.Stone;
			Terraria.ModLoader.ModWall modWall = null;
			modWall.DustType = DustID.Grass;

			var tile = Main.tile[10, 20];
			tile.TileType = TileID.WeatherVane;
			tile.WallType = WallID.Corruption1Echo;
			tile.TileColor = PaintID.RedPaint;
			tile.WallColor = PaintID.RedPaint;
			tile.LiquidType = LiquidID.Lava;

			var projectile = new Projectile();
			projectile.aiStyle = ProjAIStyleID.Arrow;

			Terraria.ModLoader.ModProjectile modProjectile = null;
			modProjectile.AIType = ProjectileID.MagicDagger;

			var npc = new NPC();
			npc.aiStyle = NPCAIStyleID.Jellyfish;

			Terraria.ModLoader.ModNPC modNPC = null;
			modNPC.AIType = NPCID.GreenJellyfish;
			modNPC.AnimationType = NPCID.PinkJellyfish;
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
			_ = Main.tile[10, 20].TileType == [|8|]; // ref property

			// https://github.com/tModLoader/tModLoader/issues/4849
			_ = new Player().CountItem([|2|]) < 10;
			""",
			"""
			using Terraria;
			using Terraria.ID;

			_ = new Item().type == ItemID.IronPickaxe;
			_ = new Projectile().type == ProjectileID.Xenopopper;
			_ = Main.tile[10, 20].TileType == TileID.Gold; // ref property

			// https://github.com/tModLoader/tModLoader/issues/4849
			_ = new Player().CountItem(ItemID.DirtBlock) < 10;
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
			using Terraria.DataStructures;

			var recipe = Recipe.Create([|420|]);
			recipe.AddTile([|412|]);
			recipe.AddIngredient([|430|]);
			NetMessage.SendData(number: 42, number2: 42, number5: 42, msgType: [|42|]);
			Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), Main.LocalPlayer.Top, new Vector2(0, -Main.rand.NextFloat(2f, 4f)).RotatedByRandom(0.3f), [|60|], 0, 0, Main.myPlayer);

			var item = new Item();
			item.CloneDefaults([|5450|]);
			item.netDefaults([|42|]);
			item.SetDefaults([|42|]);
			item.SetDefaults([|42|], true, null);
			Dust.NewDust(Vector2.Zero, 1, 2, [|3|], 4, 5, 6, Color.Red, 7);
			Dust.NewDustDirect(Vector2.Zero, 1, 2, [|75|], 4, 5);
			Dust.NewDustPerfect(Vector2.Zero, [|76|]);

			var player = new Player();
			player.AddBuff([|20|], 120);
			player.ClearBuff([|20|]);
			player.FindBuffIndex([|20|]);
			player.HasBuff([|20|]);
			player.CountItem([|42|]);
			player.ConsumeItem([|42|]);
			player.FindItem([|42|], []);
			player.FindItemInInventoryOrOpenVoidBag([|42|], out _);
			player.HasItem([|42|]);
			player.HasItem([|42|], []);
			player.HasItemInInventoryOrOpenVoidBag([|42|]);
			player.HasItemInAnyInventory([|42|]);
			player.OpenBossBag([|42|]);
			player.PutItemInInventoryFromItemUsage([|42|]);
			player.StatusToNPC([|42|], 0);
			player.StatusToPlayerPvP([|42|], 0);
			player.PutItemInInventoryFromItemUsage([|42|]);
			var entitySource = new EntitySource_ItemOpen(player, item.type);
			player.QuickSpawnItem(entitySource, [|42|], 1);
			player.QuickSpawnItemDirect(entitySource, [|42|], 1);
			player.isNearNPC([|1|]);

			var npc = new NPC();
			npc.AddBuff([|24|], 120, true);
			npc.FindBuffIndex([|24|]);
			npc.HasBuff([|24|]);
			NPC.NewNPC(entitySource, 0, 0, [|1|], 0, 0, 0, 0, 0, 0);
			NPC.NewNPCDirect(entitySource, 0, 0, [|1|], 0, 0, 0, 0, 0, 0);
			NPC.NewNPCDirect(entitySource, new Vector2(0, 0), [|1|], 0, 0, 0, 0, 0, 0);
			""",
			"""
			using Microsoft.Xna.Framework;
			using Terraria;
			using Terraria.ID;
			using Terraria.DataStructures;

			var recipe = Recipe.Create(ItemID.CobaltBrickWall);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.AddIngredient(ItemID.PurpleTorch);
			NetMessage.SendData(number: 42, number2: 42, number5: 42, msgType: MessageID.PlayerMana);
			Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), Main.LocalPlayer.Top, new Vector2(0, -Main.rand.NextFloat(2f, 4f)).RotatedByRandom(0.3f), ProjectileID.MythrilDrill, 0, 0, Main.myPlayer);

			var item = new Item();
			item.CloneDefaults(ItemID.RainbowMossBlockWall);
			item.netDefaults(ItemID.Shuriken);
			item.SetDefaults(ItemID.Shuriken);
			item.SetDefaults(ItemID.Shuriken, true, null);
			Dust.NewDust(Vector2.Zero, 1, 2, DustID.GrassBlades, 4, 5, 6, Color.Red, 7);
			Dust.NewDustDirect(Vector2.Zero, 1, 2, DustID.CursedTorch, 4, 5);
			Dust.NewDustPerfect(Vector2.Zero, DustID.Snow);

			var player = new Player();
			player.AddBuff(BuffID.Poisoned, 120);
			player.ClearBuff(BuffID.Poisoned);
			player.FindBuffIndex(BuffID.Poisoned);
			player.HasBuff(BuffID.Poisoned);
			player.CountItem(ItemID.Shuriken);
			player.ConsumeItem(ItemID.Shuriken);
			player.FindItem(ItemID.Shuriken, []);
			player.FindItemInInventoryOrOpenVoidBag(ItemID.Shuriken, out _);
			player.HasItem(ItemID.Shuriken);
			player.HasItem(ItemID.Shuriken, []);
			player.HasItemInInventoryOrOpenVoidBag(ItemID.Shuriken);
			player.HasItemInAnyInventory(ItemID.Shuriken);
			player.OpenBossBag(ItemID.Shuriken);
			player.PutItemInInventoryFromItemUsage(ItemID.Shuriken);
			player.StatusToNPC(ItemID.Shuriken, 0);
			player.StatusToPlayerPvP(ItemID.Shuriken, 0);
			player.PutItemInInventoryFromItemUsage(ItemID.Shuriken);
			var entitySource = new EntitySource_ItemOpen(player, item.type);
			player.QuickSpawnItem(entitySource, ItemID.Shuriken, 1);
			player.QuickSpawnItemDirect(entitySource, ItemID.Shuriken, 1);
			player.isNearNPC(NPCID.BlueSlime);

			var npc = new NPC();
			npc.AddBuff(BuffID.OnFire, 120, true);
			npc.FindBuffIndex(BuffID.OnFire);
			npc.HasBuff(BuffID.OnFire);
			NPC.NewNPC(entitySource, 0, 0, NPCID.BlueSlime, 0, 0, 0, 0, 0, 0);
			NPC.NewNPCDirect(entitySource, 0, 0, NPCID.BlueSlime, 0, 0, 0, 0, 0, 0);
			NPC.NewNPCDirect(entitySource, new Vector2(0, 0), NPCID.BlueSlime, 0, 0, 0, 0, 0, 0);
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
			using Terraria.GameContent;
			using Terraria.ID;

			ItemID.Sets.StaffMinionSlotsRequired[[|1309|]] = 2f;
			NPCID.Sets.MustAlwaysDraw[[|114|]] = true;
			ProjectileID.Sets.TrailingMode[[|94|]] = 1;
			TileID.Sets.TouchDamageHot[[|2|]] = true;
			TileID.Sets.Conversion.Sand[[|461|]] = true;
			WallID.Sets.Transparent[[|12|]] = true;
			WallID.Sets.Conversion.Grass[[|65|]] = true;
			MountID.Sets.Cart[[|12|]] = true;
			_ = TextureAssets.Extra[[|98|]].Value;
			""",
			"""
			using Terraria;
			using Terraria.GameContent;
			using Terraria.ID;

			ItemID.Sets.StaffMinionSlotsRequired[ItemID.SlimeStaff] = 2f;
			NPCID.Sets.MustAlwaysDraw[NPCID.WallofFleshEye] = true;
			ProjectileID.Sets.TrailingMode[ProjectileID.CrystalStorm] = 1;
			TileID.Sets.TouchDamageHot[TileID.Grass] = true;
			TileID.Sets.Conversion.Sand[TileID.SandDrip] = true;
			WallID.Sets.Transparent[WallID.CopperBrick] = true;
			WallID.Sets.Conversion.Grass[WallID.FlowerUnsafe] = true;
			MountID.Sets.Cart[MountID.CuteFishron] = true;
			_ = TextureAssets.Extra[ExtrasID.SharpTears].Value;
			""");
	}

	// Note: It seems that even with WithTriviaFrom, some whitespace formatting is lost due to auto-formatting. Anything with comments is preserved, but errant spaces are removed, and some tabs are being turned into to spaces in these tests.
	// This is fine, however, since the important trivia such as comments are still preserved and the tabs issue is corrected when used in a real mod.
	[TestMethod]
	public async Task Test_Whitespace()
	{
		await VerifyCS.Run(
			"""
			using Terraria;
			using Microsoft.Xna.Framework;

			// Invocation tests
			Dust d = Dust.NewDustPerfect(
				Vector2.Zero,
				[|68|]
			);

			var player = new Player();
			player.AddBuff( /* Before */ [|20|]  /* After */ , 120);
			player.ClearBuff(
				[|20|]);
			player.HasBuff(
				[|20|] // Test
			);

			// ArrayIndexing
			Terraria.ID.ProjectileID.Sets.TrailingMode[[|94|] /* Note */] = 1;

			// Assignment
			var item = new Item();
			item.createTile =
							[|42|];

			//Binary
			_ = new Item().type == /* Note */ [|1|] /* Note2 */;
			_ = new Item().type ==[|1|];

			// CaseSwitchLabel
			switch (new NPC().type) {
				case [|420|] /* Note */:
					break;
			}
			""",
			""""
			using Terraria;
			using Microsoft.Xna.Framework;
			using Terraria.ID;

			// Invocation tests
			Dust d = Dust.NewDustPerfect(
				Vector2.Zero,
			    DustID.BlueCrystalShard
			);

			var player = new Player();
			player.AddBuff( /* Before */ BuffID.Poisoned  /* After */ , 120);
			player.ClearBuff(
			    BuffID.Poisoned);
			player.HasBuff(
			    BuffID.Poisoned // Test
			);

			// ArrayIndexing
			Terraria.ID.ProjectileID.Sets.TrailingMode[ProjectileID.CrystalStorm /* Note */] = 1;

			// Assignment
			var item = new Item();
			item.createTile =
			                TileID.HangingLanterns;

			//Binary
			_ = new Item().type == /* Note */ ItemID.IronPickaxe /* Note2 */;
			_ = new Item().type == ItemID.IronPickaxe;

			// CaseSwitchLabel
			switch (new NPC().type) {
				case NPCID.NebulaBrain /* Note */:
					break;
			}
			"""");
	}

	[TestMethod]
	public async Task Test_4889()
	{
		// https://github.com/tModLoader/tModLoader/issues/4889

		// TODO: In the future, we may want to teach the analyzer how to process
		//       these complex expressions.

		await VerifyCS.Run(
			"""
			using Terraria;
			using Terraria.ID;

			Projectile.NewProjectile(null, 0f, 0f, 0f, 0f, [|0|], 0, 0f, 0, 0f, 0f);
			Projectile.NewProjectile(null, 0f, 0f, 0f, 0f, (true ? 0 : 1), 0, 0f, 0, 0f, 0f);
			Projectile.NewProjectile(null, 0f, 0f, 0f, 0f, (true ? 0 : ProjectileID.WoodenArrowFriendly), 0, 0f, 0, 0f, 0f);
			Projectile.NewProjectile(null, 0f, 0f, 0f, 0f, (true ? ProjectileID.None : 1), 0, 0f, 0, 0f, 0f);
			Projectile.NewProjectile(null, 0f, 0f, 0f, 0f, (true ? ProjectileID.None : ProjectileID.WoodenArrowFriendly), 0, 0f, 0, 0f, 0f);
			""",
			"""
			using Terraria;
			using Terraria.ID;

			Projectile.NewProjectile(null, 0f, 0f, 0f, 0f, ProjectileID.None, 0, 0f, 0, 0f, 0f);
			Projectile.NewProjectile(null, 0f, 0f, 0f, 0f, (true ? 0 : 1), 0, 0f, 0, 0f, 0f);
			Projectile.NewProjectile(null, 0f, 0f, 0f, 0f, (true ? 0 : ProjectileID.WoodenArrowFriendly), 0, 0f, 0, 0f, 0f);
			Projectile.NewProjectile(null, 0f, 0f, 0f, 0f, (true ? ProjectileID.None : 1), 0, 0f, 0, 0f, 0f);
			Projectile.NewProjectile(null, 0f, 0f, 0f, 0f, (true ? ProjectileID.None : ProjectileID.WoodenArrowFriendly), 0, 0f, 0, 0f, 0f);
			""");
	}
}
