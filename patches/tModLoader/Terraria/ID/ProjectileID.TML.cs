namespace Terraria.ID;

partial class ProjectileID
{
	partial class Sets
	{
		/// <summary>
		/// Set of Grapple Hook Projectile IDs that determines whether or not said projectile can only have one copy of it within the world per player.
		/// </summary>
		public static bool[] SingleGrappleHook = Factory.CreateBoolSet(false, 13, 315, 230, 231, 232, 233, 234, 235, 331, 753, 865, 935);

		// Values taken from Player.ItemCheck_Shoot type == 3029 check
		/// <summary>
		/// Set of Projectile IDs that have a chance to fire fewer projectiles from <see cref="ItemID.DaedalusStormbow"/>
		/// </summary>
		public static bool[] FiresFewerFromDaedalusStormbow = Factory.CreateBoolSet(false, 91, 4, 5, 41);

		/// <summary>
		/// If <see langword="true"/> for a given projectile type (<see cref="Projectile.type"/>), then this minion will not be replaced when resummoning.
		/// <br/><br/> Defaults to <see langword="false"/>. Vanilla examples: <see cref="StardustGuardian"/>, <see cref="StardustDragon1"/>, <see cref="StardustDragon4"/>
		/// </summary>
		public static bool[] MinionCannotBeFreed = Factory.CreateBoolSet(false, StardustGuardian, StardustDragon1, StardustDragon4);

		/// <summary>
		/// Used to scale down summon tag damage for fast hitting minions and sentries.
		/// </summary>
		public static float[] SummonTagDamageMultiplier = Factory.CreateFloatSet(1f,
			ProjectileID.Smolstar, 0.75f,
			ProjectileID.DD2LightningAuraT1, 0.5f,
			ProjectileID.DD2LightningAuraT2, 0.5f,
			ProjectileID.DD2LightningAuraT3, 0.5f
		);

		/// <summary>Used in <see cref="FallingBlockTileItem"/>.</summary>
		public class FallingBlockTileItemInfo
		{
			public FallingBlockTileItemInfo(int TileType, int ItemType = 0)
			{
				this.TileType = TileType;
				this.ItemType = ItemType;
			}
			public int TileType { get; set; }
			public int ItemType { get; set; }
		}

		/// <summary>
		/// Maps falling tile projectiles (projectiles using <see cref="ProjAIStyleID.FallingTile"/>) to the tile they place and the item they drop if tile placement fails. Falling tile projectiles come in 2 variants, the falling version and the weapon version. The weapon version typically leaves item drop as 0 so that the item is not recovered.
		/// </summary>
		public static FallingBlockTileItemInfo[] FallingBlockTileItem = Factory.CreateCustomSet<FallingBlockTileItemInfo>(null,
			SandBallGun, new FallingBlockTileItemInfo(TileID.Sand),
			EbonsandBallGun, new FallingBlockTileItemInfo(TileID.Ebonsand),
			PearlSandBallGun, new FallingBlockTileItemInfo(TileID.Pearlsand),
			CrimsandBallGun, new FallingBlockTileItemInfo(TileID.Crimsand),
			MudBall, new FallingBlockTileItemInfo(TileID.Mud),
			AshBallFalling, new FallingBlockTileItemInfo(TileID.Ash),
			SnowBallHostile, new FallingBlockTileItemInfo(TileID.SnowBlock),
			SandBallFalling, new FallingBlockTileItemInfo(TileID.Sand, ItemID.SandBlock),
			EbonsandBallFalling, new FallingBlockTileItemInfo(TileID.Ebonsand, ItemID.EbonsandBlock),
			PearlSandBallFalling, new FallingBlockTileItemInfo(TileID.Pearlsand, ItemID.PearlsandBlock),
			CrimsandBallFalling, new FallingBlockTileItemInfo(TileID.Crimsand, ItemID.CrimsandBlock),
			SiltBall, new FallingBlockTileItemInfo(TileID.Silt, ItemID.SiltBlock),
			SlushBall, new FallingBlockTileItemInfo(TileID.Slush, ItemID.SlushBlock),
			CopperCoinsFalling, new FallingBlockTileItemInfo(TileID.CopperCoinPile, ItemID.CopperCoin),
			SilverCoinsFalling, new FallingBlockTileItemInfo(TileID.SilverCoinPile, ItemID.SilverCoin),
			GoldCoinsFalling, new FallingBlockTileItemInfo(TileID.GoldCoinPile, ItemID.GoldCoin),
			PlatinumCoinsFalling, new FallingBlockTileItemInfo(TileID.PlatinumCoinPile, ItemID.PlatinumCoin),
			ShellPileFalling, new FallingBlockTileItemInfo(TileID.ShellPile, ItemID.ShellPileBlock)
		);

		// Data taken from all projectiles who use aiStyle = 16
		// That means this excludes some things like Fireworks and Celebration Mk2 rockets
		/// <summary>
		/// Contains all of the projectiles that are classified as explosives. This includes all vanilla projectiles using <see cref="ProjAIStyleID.Explosive"/> and any modded projectiles that should be regarded as explosives that do not necessarily use that aiStyle. This set facilitates implementing explosive projectiles without limiting them to using <see cref="ProjAIStyleID.Explosive"/> directly, allowing for custom AI code to be used without sacrificing other expected behaviors. Projectiles using this set need to also use <see cref="ModLoader.ModProjectile.PrepareBombToBlow"/>.
		/// <para/> Several shared behaviors of explosive projectiles will be automatically applied to projectiles using this set:
		/// <para/> Sets the timeLeft to 3 and the projectile direction when colliding with an NPC or player in PVP (so the explosive can detonate).
		/// <para/> Explosives also bounce off the top of Shimmer, detonate with no blast damage when touching the bottom or sides of Shimmer, and damage other players in For the Worthy worlds.
		/// <para/> Note that code should check both <c>(projectile.aiStyle == ProjAIStyleID.Explosive || ProjectileID.Sets.Explosive[projectile.type])</c> for any code targeting explosive projectiles since this set might not be complete.
		/// </summary>
		public static bool[] Explosive = Factory.CreateBoolSet(false, Bomb, Dynamite, Grenade, StickyBomb, HappyBomb, BombSkeletronPrime, Explosives,
			GrenadeI, RocketI, ProximityMineI, GrenadeII, RocketII, ProximityMineII, GrenadeIII, RocketIII, ProximityMineIII, GrenadeIV, RocketIV, ProximityMineIV,
			Landmine, RocketSkeleton, RocketSnowmanI, RocketSnowmanII, RocketSnowmanIII, RocketSnowmanIV, StickyGrenade, StickyDynamite, BouncyBomb, BouncyGrenade,
			BombFish, PartyGirlGrenade, BouncyDynamite, DD2GoblinBomb, ScarabBomb, ClusterRocketI, ClusterGrenadeI, ClusterMineI, ClusterFragmentsI, ClusterRocketII,
			ClusterRocketII, ClusterMineII, ClusterFragmentsII, WetRocket, WetGrenade, WetMine, LavaRocket, LavaGrenade, LavaMine, HoneyRocket, HoneyGrenade, HoneyMine,
			MiniNukeRocketI, MiniNukeGrenadeI, MiniNukeMineI, MiniNukeRocketII, MiniNukeGrenadeII, MiniNukeMineII, DryRocket, DryGrenade, DryMine, ClusterSnowmanRocketI,
			ClusterSnowmanRocketII, WetSnowmanRocket, LavaSnowmanRocket, HoneySnowmanRocket, MiniNukeSnowmanRocketI, MiniNukeSnowmanRocketII, DrySnowmanRocket,
			ClusterSnowmanFragmentsI, ClusterSnowmanFragmentsII, WetBomb, LavaBomb, HoneyBomb, DryBomb, DirtBomb, DirtStickyBomb, SantankMountRocket, TNTBarrel);

		/// <summary>
		/// This projectile is a candidate for player interaction. The projectile will be able to be targeted with smart cursor. Projectile that can be right clicked should set this to true.
		/// <br/><br/> The <see href="https://github.com/tModLoader/tModLoader/tree/1.4.4/ExampleMod/Content/Projectiles/ExampleInteractableProjectile.cs">ExampleInteractableProjectile.cs</see> example demonstrates properly implementing an interactable projectile.
		/// <br/><br/> Defaults to false. Vanilla entries include <see cref="FlyingPiggyBank"/>, <see cref="VoidLens"/>, and <see cref="ChesterPet"/>.
		/// </summary>
		public static bool[] IsInteractable = Factory.CreateBoolSet(false, 525, 734, 960);
	}
}
