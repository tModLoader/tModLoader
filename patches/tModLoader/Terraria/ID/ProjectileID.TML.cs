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
		/// Set of Projectile IDs that will fire less from <see cref="ItemID.DaedalusStormbow"/>
		/// </summary>
		public static bool[] FiresLessFromDaedalusStormbow = Factory.CreateBoolSet(false, 91, 4, 5, 41);

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
	}
}
