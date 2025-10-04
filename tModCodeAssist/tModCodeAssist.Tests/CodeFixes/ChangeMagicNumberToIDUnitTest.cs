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
			using Terraria.DataStructures;

			var item = new Item();
			item.createTile = [|42|];
			item.type = [|42|];
			item.useStyle = [|4|];
			item.shoot = [|42|];
			item.rare = [|-1|];
			item.rare = [|4|];
			item.useTime = 69;
			item.netDefaults([|42|]);
			item.SetDefaults([|42|]);
			item.SetDefaults([|42|], true, null);

			var player = new Player();
			player.cursorItemIconID = [|327|];
			player.cursorItemIconID = -1;
			player.CountItem([|42|]);
			player.ConsumeItem([|42|]);
			player.FindItem([|42|], []);
			bool inVoidBag = false;
			int num = player.FindItemInInventoryOrOpenVoidBag([|42|], out inVoidBag);
			player.HasItem([|42|]);
			player.HasItem([|42|], []);
			player.HasItemInInventoryOrOpenVoidBag([|42|]);
			player.HasItemInAnyInventory([|42|]);
			player.OpenBossBag([|42|]);
			player.PutItemInInventoryFromItemUsage([|42|]);
			player.StatusToNPC([|42|], 0);
			player.StatusToPlayerPvP([|42|], 0);
			player.PutItemInInventoryFromItemUsage([|42|]);
			var entitySource = new EntitySource_ItemOpen(this, itemType, context);
			player.QuickSpawnItem(entitySource, [|42|], 1);
			player.QuickSpawnItemDirect(entitySource, [|42|], 1);
			player.isNearNPC([|1|]);

			var mount = Mount();
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
			NPC.NewNPC(entitySource, 0, 0, [|1|], 0, 0, 0, 0, 0, 0);
			NPC.NewNPCDirect(entitySource, 0, 0, [|1|], 0, 0, 0, 0, 0, 0);
			NPC.NewNPCDirect(entitySource, new Vector2(0, 0), [|1|], 0, 0, 0, 0, 0, 0);

			Terraria.ModLoader.ModNPC modNPC = null;
			modNPC.AIType = [|103|];
			modNPC.AnimationType = [|64|];
			""",
			"""
			using Terraria;
			using Terraria.DataStructures;
			using Terraria.ID;

			var item = new Item();
			item.createTile = TileID.HangingLanterns;
			item.type = ItemID.Shuriken;
			item.useStyle = ItemUseStyleID.HoldUp;
			item.shoot = ProjectileID.SandBallGun;
			item.rare = ItemRarityID.Gray;
			item.rare = ItemRarityID.LightRed;
			item.useTime = 69;
			item.netDefaults(ItemID.Shuriken);
			item.SetDefaults(ItemID.Shuriken);
			item.SetDefaults(ItemID.Shuriken, true, null);

			var player = new Player();
			player.cursorItemIconID = ItemID.GoldenKey;
			player.cursorItemIconID = -1;
			player.CountItem(ItemID.Shuriken);
			player.ConsumeItem(ItemID.Shuriken);
			player.FindItem(ItemID.Shuriken, []);
			bool inVoidBag = false;
			int num = player.FindItemInInventoryOrOpenVoidBag(ItemID.Shuriken, out inVoidBag);
			player.HasItem(ItemID.Shuriken);
			player.HasItem(ItemID.Shuriken, []);
			player.HasItemInInventoryOrOpenVoidBag(ItemID.Shuriken);
			player.HasItemInAnyInventory(ItemID.Shuriken);
			player.OpenBossBag(ItemID.Shuriken);
			player.PutItemInInventoryFromItemUsage(ItemID.Shuriken);
			player.StatusToNPC(ItemID.Shuriken, 0);
			player.StatusToPlayerPvP(ItemID.Shuriken, 0);
			player.PutItemInInventoryFromItemUsage(ItemID.Shuriken);
			var entitySource = new EntitySource_ItemOpen(this, itemType, context);
			player.QuickSpawnItem(entitySource, ItemID.Shuriken, 1);
			player.QuickSpawnItemDirect(entitySource, ItemID.Shuriken, 1);
			player.isNearNPC(NPCID.BlueSlime);

			var mount = Mount();
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
			NPC.NewNPC(entitySource, 0, 0, NPCID.BlueSlime, 0, 0, 0, 0, 0, 0);
			NPC.NewNPCDirect(entitySource, 0, 0, NPCID.BlueSlime, 0, 0, 0, 0, 0, 0);
			NPC.NewNPCDirect(entitySource, new Vector2(0, 0), NPCID.BlueSlime, 0, 0, 0, 0, 0, 0);

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
			""",
			"""
			using Terraria;
			using Terraria.ID;

			_ = new Item().type == ItemID.IronPickaxe;
			_ = new Projectile().type == ProjectileID.Xenopopper;
			_ = Main.tile[10, 20].TileType == TileID.Gold; // ref property
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
			new Player().AddBuff([|20|], 120);
			new Player().ClearBuff([|20|]);
			new Player().FindBuffIndex([|20|]);
			new Player().HasBuff([|20|]);
			new NPC().AddBuff([|24|], 120, true);
			new Player().ClearBuff([|24|]);
			new Player().FindBuffIndex([|24|]);
			new Player().HasBuff([|24|]);
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
			new Player().AddBuff(BuffID.Poisoned, 120);
			new Player().ClearBuff(BuffID.Poisoned);
			new Player().FindBuffIndex(BuffID.Poisoned);
			new Player().HasBuff(BuffID.Poisoned);
			new NPC().AddBuff(BuffID.OnFire, 120, true);
			new NPC().ClearBuff(BuffID.OnFire);
			new NPC().FindBuffIndex(BuffID.OnFire);
			new NPC().HasBuff(BuffID.OnFire);
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
			_ = TextureAssets.Extra[ExtrasID.SharpTears].Value;
			""");
	}
}