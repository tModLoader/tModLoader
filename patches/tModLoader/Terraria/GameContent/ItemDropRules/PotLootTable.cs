using System;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Terraria.WorldGen;

namespace Terraria.GameContent.ItemDropRules;

// Builds the vanilla rules for a single pot break before mod hooks run.
internal static class PotLootTable
{
	// Fixed stacks must not roll luck or consume a stack RNG call. Variable stacks
	// retain their original RNG, and each item is spawned before the next rule rolls.
	// Clearing the table also removes selection rules and special-seed effects.
	internal static void Register(PotLoot loot, int i, int j)
	{
		var context = new PotDropContext(loot, i, j);
		bool InStyle(int first, int last) => loot.Style >= first && loot.Style <= last;
		IItemDropRule AddItem(int item, Func<bool> condition, int min = 1, int max = 1) =>
			AddStackItem(item, condition, _ => min == max ? min : Main.rand.Next(min, max + 1), min, max);
		IItemDropRule AddStackItem(int item, Func<bool> condition, Func<DropAttemptInfo, int> stack, int min, int max) {
			var rule = new LeadingConditionRule(new Conditions.PotDrop(_ => condition()));
			rule.OnSuccess(new PotStackRule(item, stack, min, max));
			return loot.Add(rule);
		}
		IItemDropRule AddChanceItem(int item, Func<bool> condition, int chance, int min = 1, int max = 1, bool generationRandom = false) {
			var rule = new LeadingConditionRule(new Conditions.PotDrop(_ => condition() && (generationRandom ? genRand : Main.rand).Next(chance) == 0));
			rule.OnSuccess(new PotStackRule(item, _ => min == max ? min : Main.rand.Next(min, max + 1), min, max));
			return loot.Add(rule);
		}

		if (Main.tenthAnniversaryWorld && Main.notTheBeesWorld && !Main.drunkWorld)
			AddChanceItem(1130, () => true, 50, 12, 20, generationRandom: true);

		// The first successful outcome suppresses all subsequent vanilla outcomes.
		LeadingConditionRule previous = null;
		void Outcome(Func<bool> condition, Action action) {
			var rule = new LeadingConditionRule(new Conditions.PotDrop(_ => condition()));
			rule.OnSuccess(new PotEffectRule(_ => action()));
			if (previous == null)
				loot.Add(rule);
			else
				previous.OnFailedConditions(rule);
			previous = rule;
		}

		Outcome(() => Player.GetClosestRollLuck(i, j, (int)(500f / ((context.ValueMultiplier + 1f) / 2f))) == 0f,
			() => Projectile.NewProjectile(new EntitySource_TileBreak(i, j), i * 16 + 16, j * 16 + 16, 0f, -12f, 518, 0, 0f, Main.myPlayer));
		Outcome(() => genRand.Next(35) == 0 && Main.wallDungeon[Main.tile[i, j].wall] && j > Main.worldSurface,
			() => context.Reward = PotReward.Key);
		Outcome(() => j > Main.worldSurface && Main.dontStarveWorld && !Main.remixWorld && genRand.Next(20) == 0,
			() => { context.Reward = PotReward.Fruit; context.Option = InWorld(i, j, 2) ? genRand.Next(context.Fruits.Length) : -1; });
		Outcome(() => Main.getGoodWorld && genRand.Next(Main.tenthAnniversaryWorld && !Main.remixWorld ? 12 : 6) == 0,
			() => {
				int projectile = Projectile.NewProjectile(new EntitySource_TileBreak(i, j), i * 16 + 16, j * 16 + 8,
					Main.rand.Next(-100, 101) * 0.002f, 0f, Main.tenthAnniversaryWorld && !Main.remixWorld ? 75 : 28, 0, 0f, Main.myPlayer, 16f, 16f);
				Main.projectile[projectile].npcProj = true;
			});
		Outcome(() => Main.remixWorld && Main.netMode != NetmodeID.MultiplayerClient && genRand.Next(5) == 0, () => SelectRemixPotReward(context));
		Outcome(() => Main.remixWorld && i > Main.maxTilesX * 0.37 && i < Main.maxTilesX * 0.63 && j > Main.maxTilesY - 220,
			() => context.Reward = PotReward.Rope);
		Outcome(() => genRand.Next(45) == 0 || (Main.rand.Next(45) == 0 && Main.expertMode), () => {
			context.Reward = PotReward.Potion;
			context.Option = genRand.Next(context.Potions.Length);
		});
		Outcome(() => Main.netMode == NetmodeID.Server && Main.rand.Next(30) == 0,
			() => context.Reward = PotReward.WormholePotion);
		previous.OnFailedConditions(new PotEffectRule(_ => {
			context.Reward = PotReward.Common;
			context.CommonRoll = Main.rand.Next(7) - (Main.expertMode ? 1 : 0);
			int torchCount = 0;
			int torchTarget = Main.vampireSeed ? 30 : 20;
			for (int slot = 0; slot < 50 && torchCount < torchTarget; slot++) {
				Item item = loot.Player.inventory[slot];
				if (!item.IsAir && item.createTile >= 0 && TileID.Sets.Torches[item.createTile])
					torchCount += item.stack;
			}
			context.NeedsTorches = torchCount < torchTarget;
			if (context.NeedsTorches && Main.vampireSeed)
				context.CommonRoll = 1;
		}));

		AddItem(ItemID.GoldenKey, () => context.Reward == PotReward.Key);
		// Even an out-of-world fruit attempt consumes its stack roll in vanilla.
		AddStackItem(-1, () => context.Reward == PotReward.Fruit && context.Option == -1, _ => genRand.Next(1, 3), 1, 2);
		for (int option = 0; option < context.Fruits.Length; option++) {
			int index = option;
			AddStackItem(context.Fruits[index], () => context.Reward == PotReward.Fruit && context.Option == index, _ => genRand.Next(1, 3), 1, 2);
		}
		AddItem(75, () => context.Reward == PotReward.RemixBomb);
		for (int option = 0; option < context.Potions.Length; option++) {
			int index = option;
			if (context.Potions[index] != 0)
				AddItem(context.Potions[index], () => context.Reward == PotReward.Potion && context.Option == index);
		}
		if (j < Main.worldSurface || context.AboveRock || context.AboveUnderworld)
			AddStackItem(ItemID.RecallPotion, () => context.Reward == PotReward.Potion && context.Option >= 7, _ => genRand.Next(1, 3), 1, 2);
		if (j >= Main.worldSurface && !context.AboveRock)
			AddChanceItem(4870, () => context.Reward == PotReward.Potion, context.AboveUnderworld ? 15 : 5, generationRandom: true);
		AddItem(ItemID.WormholePotion, () => context.Reward == PotReward.WormholePotion);

		// Common rewards are selected once. Failed health and bomb conditions fall back
		// to torches, rope or coins exactly as in the original pot branch ordering.
		var common = new LeadingConditionRule(new Conditions.PotDrop(_ => context.Reward == PotReward.Common));
		loot.Add(common);
		var health = new LeadingConditionRule(new Conditions.PotDrop(_ => context.CommonRoll == 0 && loot.Player.statLife < loot.Player.statLifeMax2));
		common.OnSuccess(health);
		health.OnSuccess(new PotEffectRule(_ => context.Reward = PotReward.Heart));
		var wood = new LeadingConditionRule(new Conditions.PotDrop(_ => Main.vampireSeed && (context.AboveUnderworld || context.AboveRock) && genRand.Next(6) == 0));
		health.OnFailedConditions(wood);
		wood.OnSuccess(new PotEffectRule(_ => context.Reward = PotReward.Wood));
		var torches = new LeadingConditionRule(new Conditions.PotDrop(_ => context.CommonRoll == 1 || (context.CommonRoll == 0 && context.NeedsTorches)));
		wood.OnFailedConditions(torches);
		torches.OnSuccess(new PotEffectRule(_ => context.Reward = PotReward.Torch));
		var ammo = new LeadingConditionRule(new Conditions.PotDrop(_ => context.CommonRoll == 2));
		torches.OnFailedConditions(ammo);
		ammo.OnSuccess(new PotEffectRule(_ => {
			context.Reward = PotReward.Ammo;
			context.AmmoStack = Main.rand.Next(10, 21);
			context.Ammo = ItemID.WoodenArrow;
			if (context.AboveRock && genRand.Next(2) == 0)
				context.Ammo = Main.hardMode ? 168 : ItemID.Shuriken;
			if (j > Main.UnderworldLayer)
				context.Ammo = ItemID.HellfireArrow;
			else if (Main.hardMode)
				context.Ammo = Main.rand.Next(2) != 0 ? ItemID.UnholyArrow : SavedOreTiers.Silver != 168 ? ItemID.SilverBullet : 4915;
		}));
		var healing = new LeadingConditionRule(new Conditions.PotDrop(_ => context.CommonRoll == 3));
		ammo.OnFailedConditions(healing);
		healing.OnSuccess(new PotEffectRule(_ => context.Reward = PotReward.HealingPotion));
		var bombs = new LeadingConditionRule(new Conditions.PotDrop(_ => context.CommonRoll == 4 && (InStyle(34, 36) || context.AboveUnderworld)));
		healing.OnFailedConditions(bombs);
		bombs.OnSuccess(new PotEffectRule(_ => context.Reward = PotReward.Bomb));
		var rope = new LeadingConditionRule(new Conditions.PotDrop(_ => (context.CommonRoll == 4 || context.CommonRoll == 5) && j < Main.UnderworldLayer && !Main.hardMode));
		bombs.OnFailedConditions(rope);
		rope.OnSuccess(new PotEffectRule(_ => context.Reward = PotReward.Rope));
		rope.OnFailedConditions(new PotEffectRule(_ => context.Reward = PotReward.Coins));

		AddItem(ItemID.Rope, () => context.Reward == PotReward.Rope, 20, 40);
		AddItem(ItemID.Heart, () => context.Reward == PotReward.Heart);
		AddChanceItem(ItemID.Heart, () => context.Reward == PotReward.Heart, 2);
		if (Main.expertMode) {
			AddChanceItem(ItemID.Heart, () => context.Reward == PotReward.Heart, 2);
			AddChanceItem(ItemID.Heart, () => context.Reward == PotReward.Heart, 2);
		}
		AddItem(InStyle(4, 6) ? ItemID.BorealWood : InStyle(7, 9) ? ItemID.RichMahogany : ItemID.Wood,
			() => context.Reward == PotReward.Wood, 10, 30);

		int torch = ItemID.Torch;
		int glowstick = ItemID.Glowstick;
		bool extraTorches = false;
		bool jungleTorches = false;
		if (loot.Player.ZoneHallow) { torch = 4387; extraTorches = true; }
		else if (InStyle(22, 24) || loot.Player.ZoneCrimson) { torch = 4386; extraTorches = true; }
		else if (InStyle(16, 18) || loot.Player.ZoneCorrupt) { torch = 4385; extraTorches = true; }
		else if (InStyle(7, 9)) { torch = 4388; extraTorches = true; jungleTorches = true; }
		else if (InStyle(4, 6)) { torch = ItemID.IceTorch; glowstick = ItemID.StickyGlowstick; }
		else if (InStyle(34, 36)) { torch = 4383; extraTorches = true; }
		else if (loot.Player.ZoneGlowshroom) { torch = 5293; extraTorches = true; }
		var torchRule = new PotStackRule(Main.tile[i, j].liquid > 0 ? glowstick : torch, info => {
			int stack = info.rng.Next(2, 7);
			if (Main.expertMode) stack += info.rng.Next(1, 7);
			if (Main.vampireSeed) stack += info.rng.Next(2, 7);
			if (extraTorches) stack += info.rng.Next(2, 7);
			return jungleTorches ? (int)(stack * 1.5f) : stack;
		}, 2, 40);
		var torchCondition = new LeadingConditionRule(new Conditions.PotDrop(_ => context.Reward == PotReward.Torch));
		torchCondition.OnSuccess(torchRule);
		loot.Add(torchCondition);

		foreach (int item in new[] { ItemID.WoodenArrow, ItemID.Shuriken, 168, ItemID.SilverBullet, ItemID.HellfireArrow, ItemID.UnholyArrow, 4915 })
			AddStackItem(item, () => context.Reward == PotReward.Ammo && context.Ammo == item, _ => context.AmmoStack, 10, 20);
		int healingItem = j > Main.UnderworldLayer || Main.hardMode ? ItemID.HealingPotion : ItemID.LesserHealingPotion;
		AddStackItem(healingItem, () => context.Reward == PotReward.HealingPotion,
			_ => Main.expertMode && Main.rand.Next(3) != 0 ? 2 : 1, 1, Main.expertMode ? 2 : 1);
		int bombItem = InStyle(34, 36) ? 4423 : ItemID.Bomb;
		var bombCondition = new LeadingConditionRule(new Conditions.PotDrop(_ => context.Reward == PotReward.Bomb));
		bombCondition.OnSuccess(new PotStackRule(bombItem, info => info.rng.Next(4) + 1 + (Main.expertMode ? info.rng.Next(4) : 0), 1, Main.expertMode ? 7 : 4));
		loot.Add(bombCondition);
		var coins = new LeadingConditionRule(new Conditions.PotDrop(_ => context.Reward == PotReward.Coins));
		coins.OnSuccess(new PotCoinsRule(j, context.AboveRock, context.ValueMultiplier));
		loot.Add(coins);
	}

	private enum PotReward { None, Key, Fruit, RemixBomb, Rope, Potion, WormholePotion, Common, Heart, Wood, Torch, Ammo, HealingPotion, Bomb, Coins }

	private sealed class PotDropContext
	{
		public readonly PotLoot Loot;
		public readonly int I;
		public readonly int J;
		public readonly bool AboveRock;
		public readonly bool AboveUnderworld;
		public readonly float ValueMultiplier;
		public readonly int[] Fruits;
		public readonly int[] Potions;
		public PotReward Reward;
		public int Option;
		public int CommonRoll;
		public int Ammo;
		public int AmmoStack;
		public bool NeedsTorches;

		public PotDropContext(PotLoot loot, int i, int j)
		{
			Loot = loot;
			I = i;
			J = j;
			AboveRock = Main.remixWorld ? j > Main.rockLayer && j < Main.UnderworldLayer : j < Main.rockLayer;
			AboveUnderworld = Main.remixWorld ? j > Main.worldSurface && j < Main.rockLayer : j < Main.UnderworldLayer;
			float multiplier = loot.Style switch {
				>= 4 and <= 6 => 1.25f,
				>= 7 and <= 9 => 1.75f,
				>= 10 and <= 12 => 1.9f,
				>= 13 and <= 15 => 2.1f,
				>= 16 and <= 18 => 1.6f,
				>= 19 and <= 21 => 3.5f,
				>= 22 and <= 24 => 1.6f,
				>= 25 and <= 27 => 10f,
				>= 28 and <= 30 when Main.hardMode => 4f,
				>= 31 and <= 33 => 2f,
				>= 34 and <= 36 => 1.25f,
				_ => 1f
			};
			ValueMultiplier = (multiplier * 2f + 1f) / 3f;
			Fruits = loot.Style switch {
				>= 4 and <= 6 => new[] { 4286, 4295 },
				>= 7 and <= 9 or >= 28 and <= 30 => new[] { 4294, 4292 },
				>= 25 and <= 27 or >= 34 and <= 36 => new[] { 4283, 4287 },
				>= 16 and <= 18 => new[] { 4284, 4289 },
				>= 22 and <= 24 => new[] { 4296, 4285 },
				>= 13 and <= 15 => new[] { 5277, 5278 },
				_ => new[] { 4009, 4293, 4282, 4290, 4291 }
			};
			// Zero entries retain the original empty option and recall-only outcomes.
			Potions = j < Main.worldSurface ? new[] { 292, 298, 299, 290, 2322, 2324, 2325, 0, 0, 0 }
				: AboveRock ? new[] { 289, 298, 299, 290, 303, 291, 304, 2322, 2329, 0, 0 }
				: AboveUnderworld ? new[] { 296, 295, 299, 302, 303, 305, 301, 302, 297, 304, 2322, 2323, 2327, 2329, 0 }
				: new[] { 296, 295, 293, 288, 294, 297, 304, 305, 301, 302, 288, 300, 2323, 2326 };
		}
	}

	private static void SelectRemixPotReward(PotDropContext context)
	{
		if (Main.rand.Next(2) == 0) {
			context.Reward = PotReward.RemixBomb;
			return;
		}
		int npcType;
		if (context.Loot.Player.ZoneJungle)
			npcType = -10;
		else if (context.J > Main.rockLayer && context.J < Main.maxTilesY - 350)
			npcType = Main.rand.Next(9) == 0 ? -7 : Main.rand.Next(7) == 0 ? -8 : Main.rand.Next(6) == 0 ? -9 : Main.rand.Next(3) != 0 ? 1 : -3;
		else if (context.J > Main.worldSurface && context.J <= Main.rockLayer)
			npcType = -6;
		else {
			context.Reward = PotReward.RemixBomb;
			return;
		}
		int npc = NPC.NewNPC(GetNPCSource_FromTileBreak(context.I, context.J), context.Loot.X * 16 + 16, context.Loot.Y * 16 + 32, npcType);
		if (npc > -1) {
			Main.npc[npc].ai[1] = 75f;
			Main.npc[npc].netUpdate = true;
		}
	}

}
