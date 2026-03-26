using ReLogic.Reflection;

namespace Terraria.ID;

public class ProjAIStyleID
{
	public static readonly IdDictionary Search = IdDictionary.Create<ProjAIStyleID, short>();
	/// <summary>
	/// Behavior: Includes Bullets and Lasers<br/><br/>
	/// Used by: <see cref="ProjectileID.WoodenArrowFriendly"/>, <see cref="ProjectileID.FireArrow"/>, <see cref="ProjectileID.UnholyArrow"/>, <see cref="ProjectileID.JestersArrow"/>, <see cref="ProjectileID.Bullet"/>, <see cref="ProjectileID.GreenLaser"/>, <see cref="ProjectileID.MeteorShot"/>, <see cref="ProjectileID.HarpyFeather"/>, <see cref="ProjectileID.HellfireArrow"/>, <see cref="ProjectileID.Seed"/>, <see cref="ProjectileID.Stinger"/>, <see cref="ProjectileID.WoodenArrowHostile"/>, <see cref="ProjectileID.FlamingArrow"/>, <see cref="ProjectileID.EyeLaser"/>, <see cref="ProjectileID.PinkLaser"/>, <see cref="ProjectileID.PurpleLaser"/>, <see cref="ProjectileID.CrystalBullet"/>, <see cref="ProjectileID.HolyArrow"/>, <see cref="ProjectileID.PoisonDart"/>, <see cref="ProjectileID.DeathLaser"/>, <see cref="ProjectileID.CursedArrow"/>, <see cref="ProjectileID.CursedBullet"/>, <see cref="ProjectileID.BulletSnowman"/>, <see cref="ProjectileID.BoneArrow"/>, <see cref="ProjectileID.FrostArrow"/>, <see cref="ProjectileID.CopperCoin"/>, <see cref="ProjectileID.SilverCoin"/>, <see cref="ProjectileID.GoldCoin"/>, <see cref="ProjectileID.PlatinumCoin"/>, <see cref="ProjectileID.FrostburnArrow"/>, <see cref="ProjectileID.IceSpike"/>, <see cref="ProjectileID.JungleSpike"/>, <see cref="ProjectileID.ConfettiGun"/>, <see cref="ProjectileID.BulletDeadeye"/>, <see cref="ProjectileID.PoisonDartTrap"/>, <see cref="ProjectileID.PygmySpear"/>, <see cref="ProjectileID.ChlorophyteBullet"/>, <see cref="ProjectileID.ChlorophyteArrow"/>, <see cref="ProjectileID.BulletHighVelocity"/>, <see cref="ProjectileID.Stynger"/>, <see cref="ProjectileID.FlowerPowPetal"/>, <see cref="ProjectileID.FrostBeam"/>, <see cref="ProjectileID.EyeBeam"/>, <see cref="ProjectileID.PoisonFang"/>, <see cref="ProjectileID.PoisonDartBlowgun"/>, <see cref="ProjectileID.Skull"/>, <see cref="ProjectileID.SeedPlantera"/>, <see cref="ProjectileID.PoisonSeedPlantera"/>, <see cref="ProjectileID.IchorArrow"/>, <see cref="ProjectileID.IchorBullet"/>, <see cref="ProjectileID.VenomArrow"/>, <see cref="ProjectileID.VenomBullet"/>, <see cref="ProjectileID.PartyBullet"/>, <see cref="ProjectileID.NanoBullet"/>, <see cref="ProjectileID.ExplosiveBullet"/>, <see cref="ProjectileID.GoldenBullet"/>, <see cref="ProjectileID.ConfettiMelee"/>, <see cref="ProjectileID.Shadowflames"/>, <see cref="ProjectileID.SniperBullet"/>, <see cref="ProjectileID.CandyCorn"/>, <see cref="ProjectileID.JackOLantern"/>, <see cref="ProjectileID.Stake"/>, <see cref="ProjectileID.FlamingWood"/>, <see cref="ProjectileID.PineNeedleFriendly"/>, <see cref="ProjectileID.Blizzard"/>, <see cref="ProjectileID.NorthPoleSnowflake"/>, <see cref="ProjectileID.PineNeedleHostile"/>, <see cref="ProjectileID.FrostWave"/>, <see cref="ProjectileID.FrostShard"/>, <see cref="ProjectileID.Missile"/>, <see cref="ProjectileID.VenomFang"/>, <see cref="ProjectileID.PulseBolt"/>, <see cref="ProjectileID.HornetStinger"/>, <see cref="ProjectileID.ImpFireball"/>, <see cref="ProjectileID.MiniRetinaLaser"/>, <see cref="ProjectileID.MiniSharkron"/>, <see cref="ProjectileID.Meteor1"/>, <see cref="ProjectileID.Meteor2"/>, <see cref="ProjectileID.Meteor3"/>, <see cref="ProjectileID.MartianTurretBolt"/>, <see cref="ProjectileID.BrainScramblerBolt"/>, <see cref="ProjectileID.GigaZapperSpear"/>, <see cref="ProjectileID.RayGunnerLaser"/>, <see cref="ProjectileID.LaserMachinegunLaser"/>, <see cref="ProjectileID.ElectrosphereMissile"/>, <see cref="ProjectileID.SaucerLaser"/>, <see cref="ProjectileID.ChargedBlasterOrb"/>, <see cref="ProjectileID.PhantasmalBolt"/>, <see cref="ProjectileID.CultistBossFireBall"/>, <see cref="ProjectileID.CultistBossFireBallClone"/>, <see cref="ProjectileID.BeeArrow"/>, <see cref="ProjectileID.WebSpit"/>, <see cref="ProjectileID.BoneArrowFromMerchant"/>, <see cref="ProjectileID.CrystalDart"/>, <see cref="ProjectileID.CursedDart"/>, <see cref="ProjectileID.IchorDart"/>, <see cref="ProjectileID.SeedlerThorn"/>, <see cref="ProjectileID.Hellwing"/>, <see cref="ProjectileID.ShadowFlameArrow"/>, <see cref="ProjectileID.Nail"/>, <see cref="ProjectileID.JavelinFriendly"/>, <see cref="ProjectileID.JavelinHostile"/>, <see cref="ProjectileID.BoneGloveProj"/>, <see cref="ProjectileID.SalamanderSpit"/>, <see cref="ProjectileID.NebulaLaser"/>, <see cref="ProjectileID.VortexLaser"/>, <see cref="ProjectileID.VortexAcid"/>, <see cref="ProjectileID.ClothiersCurse"/>, <see cref="ProjectileID.PainterPaintball"/>, <see cref="ProjectileID.MartianWalkerLaser"/>, <see cref="ProjectileID.AncientDoomProjectile"/>, <see cref="ProjectileID.BlowupSmoke"/>, <see cref="ProjectileID.PortalGunBolt"/>, <see cref="ProjectileID.SpikedSlimeSpike"/>, <see cref="ProjectileID.ScutlixLaser"/>, <see cref="ProjectileID.VortexBeaterRocket"/>, <see cref="ProjectileID.BlowupSmokeMoonlord"/>, <see cref="ProjectileID.NebulaBlaze1"/>, <see cref="ProjectileID.NebulaBlaze2"/>, <see cref="ProjectileID.MoonlordBullet"/>, <see cref="ProjectileID.MoonlordArrow"/>, <see cref="ProjectileID.MoonlordArrowTrail"/>, <see cref="ProjectileID.LunarFlare"/>, <see cref="ProjectileID.SkyFracture"/>, <see cref="ProjectileID.BlackBolt"/>, <see cref="ProjectileID.DD2JavelinHostile"/>, <see cref="ProjectileID.DD2DrakinShot"/>, <see cref="ProjectileID.DD2DarkMageBolt"/>, <see cref="ProjectileID.DD2OgreSpit"/>, <see cref="ProjectileID.DD2BallistraProj"/>, <see cref="ProjectileID.DD2LightningBugZap"/>, <see cref="ProjectileID.DD2SquireSonicBoom"/>, <see cref="ProjectileID.DD2JavelinHostileT3"/>, <see cref="ProjectileID.DD2BetsyFireball"/>, <see cref="ProjectileID.DD2PhoenixBowShot"/>, <see cref="ProjectileID.MonkStaffT3_AltShot"/>, <see cref="ProjectileID.DD2BetsyArrow"/>, <see cref="ProjectileID.ApprenticeStaffT3Shot"/>, <see cref="ProjectileID.BookStaffShot"/>, <see cref="ProjectileID.QueenBeeStinger"/>, <see cref="ProjectileID.RollingCactusSpike"/>, <see cref="ProjectileID.Geode"/>, <see cref="ProjectileID.BloodShot"/>, <see cref="ProjectileID.BloodNautilusShot"/>, <see cref="ProjectileID.BloodArrow"/>, <see cref="ProjectileID.BookOfSkullsSkull"/>, <see cref="ProjectileID.ZapinatorLaser"/>, <see cref="ProjectileID.QueenSlimeMinionBlueSpike"/>, <see cref="ProjectileID.QueenSlimeMinionPinkBall"/>, <see cref="ProjectileID.QueenSlimeGelAttack"/>, <see cref="ProjectileID.VolatileGelatinBall"/>, <see cref="ProjectileID.DeerclopsRangedProjectile"/>, <see cref="ProjectileID.VenomDartTrap"/>, <see cref="ProjectileID.SilverBullet"/>, <see cref="ProjectileID.ShimmerArrow"/>
	/// </summary>
	public const short Arrow = 1;
	/// <summary>
	/// Behavior: Includes Shurikens, Bones, and Knives<br/><br/>
	/// Used by: <see cref="ProjectileID.Shuriken"/>, <see cref="ProjectileID.Bone"/>, <see cref="ProjectileID.ThrowingKnife"/>, <see cref="ProjectileID.PoisonedKnife"/>, <see cref="ProjectileID.HolyWater"/>, <see cref="ProjectileID.UnholyWater"/>, <see cref="ProjectileID.MagicDagger"/>, <see cref="ProjectileID.CannonballFriendly"/>, <see cref="ProjectileID.SnowBallFriendly"/>, <see cref="ProjectileID.CannonballHostile"/>, <see cref="ProjectileID.StyngerShrapnel"/>, <see cref="ProjectileID.PaladinsHammerHostile"/>, <see cref="ProjectileID.VampireKnife"/>, <see cref="ProjectileID.EatersBite"/>, <see cref="ProjectileID.RottenEgg"/>, <see cref="ProjectileID.StarAnise"/>, <see cref="ProjectileID.OrnamentHostileShrapnel"/>, <see cref="ProjectileID.LovePotion"/>, <see cref="ProjectileID.FoulPotion"/>, <see cref="ProjectileID.SkeletonBone"/>, <see cref="ProjectileID.ShadowFlameKnife"/>, <see cref="ProjectileID.DrManFlyFlask"/>, <see cref="ProjectileID.Spark"/>, <see cref="ProjectileID.ToxicFlask"/>, <see cref="ProjectileID.FrostDaggerfish"/>, <see cref="ProjectileID.NurseSyringeHurt"/>, <see cref="ProjectileID.SantaBombs"/>, <see cref="ProjectileID.BoneDagger"/>, <see cref="ProjectileID.BloodWater"/>, <see cref="ProjectileID.Football"/>, <see cref="ProjectileID.TreeGlobe"/>, <see cref="ProjectileID.WorldGlobe"/>, <see cref="ProjectileID.RockGolemRock"/>, <see cref="ProjectileID.GelBalloon"/>, <see cref="ProjectileID.WandOfSparkingSpark"/>, <see cref="ProjectileID.PewMaticHornShot"/>, <see cref="ProjectileID.WandOfFrostingFrost"/>, <see cref="ProjectileID.MoonGlobe"/>, <see cref="ProjectileID.Waffle"/>
	/// </summary>
	public const short ThrownProjectile = 2;
	/// <summary>
	/// Used by: <see cref="ProjectileID.EnchantedBoomerang"/>, <see cref="ProjectileID.Flamarang"/>, <see cref="ProjectileID.ThornChakram"/>, <see cref="ProjectileID.WoodenBoomerang"/>, <see cref="ProjectileID.LightDisc"/>, <see cref="ProjectileID.IceBoomerang"/>, <see cref="ProjectileID.PossessedHatchet"/>, <see cref="ProjectileID.Bananarang"/>, <see cref="ProjectileID.PaladinsHammerFriendly"/>, <see cref="ProjectileID.BloodyMachete"/>, <see cref="ProjectileID.FruitcakeChakram"/>, <see cref="ProjectileID.Anchor"/>, <see cref="ProjectileID.BouncingShield"/>, <see cref="ProjectileID.Shroomerang"/>, <see cref="ProjectileID.CombatWrench"/>, <see cref="ProjectileID.Trimarang"/>
	/// </summary>
	public const short Boomerang = 3;
	/// <summary>
	/// Used by: <see cref="ProjectileID.VilethornBase"/>, <see cref="ProjectileID.VilethornTip"/>, <see cref="ProjectileID.NettleBurstRight"/>, <see cref="ProjectileID.NettleBurstLeft"/>, <see cref="ProjectileID.NettleBurstEnd"/>, <see cref="ProjectileID.CrystalVileShardHead"/>, <see cref="ProjectileID.CrystalVileShardShaft"/>
	/// </summary>
	public const short Vilethorn = 4;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Starfury"/>, <see cref="ProjectileID.FallingStar"/>, <see cref="ProjectileID.HallowStar"/>, <see cref="ProjectileID.StarWrath"/>, <see cref="ProjectileID.ManaCloakStar"/>, <see cref="ProjectileID.BeeCloakStar"/>, <see cref="ProjectileID.StarVeilStar"/>, <see cref="ProjectileID.StarCloakStar"/>, <see cref="ProjectileID.StarCannonStar"/>
	/// </summary>
	public const short FallingStar = 5;
	/// <summary>
	/// Used by: <see cref="ProjectileID.PurificationPowder"/>, <see cref="ProjectileID.VilePowder"/>, <see cref="ProjectileID.ViciousPowder"/>, <see cref="ProjectileID.Fertilizer"/>
	/// </summary>
	public const short Powder = 6;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Hook"/>, <see cref="ProjectileID.IvyWhip"/>, <see cref="ProjectileID.DualHookBlue"/>, <see cref="ProjectileID.DualHookRed"/>, <see cref="ProjectileID.Web"/>, <see cref="ProjectileID.GemHookAmethyst"/>, <see cref="ProjectileID.GemHookTopaz"/>, <see cref="ProjectileID.GemHookSapphire"/>, <see cref="ProjectileID.GemHookEmerald"/>, <see cref="ProjectileID.GemHookRuby"/>, <see cref="ProjectileID.GemHookDiamond"/>, <see cref="ProjectileID.SkeletronHand"/>, <see cref="ProjectileID.BatHook"/>, <see cref="ProjectileID.WoodHook"/>, <see cref="ProjectileID.CandyCaneHook"/>, <see cref="ProjectileID.ChristmasHook"/>, <see cref="ProjectileID.FishHook"/>, <see cref="ProjectileID.SlimeHook"/>, <see cref="ProjectileID.TrackHook"/>, <see cref="ProjectileID.AntiGravityHook"/>, <see cref="ProjectileID.TendonHook"/>, <see cref="ProjectileID.ThornHook"/>, <see cref="ProjectileID.IlluminantHook"/>, <see cref="ProjectileID.WormHook"/>, <see cref="ProjectileID.LunarHookSolar"/>, <see cref="ProjectileID.LunarHookVortex"/>, <see cref="ProjectileID.LunarHookNebula"/>, <see cref="ProjectileID.LunarHookStardust"/>, <see cref="ProjectileID.StaticHook"/>, <see cref="ProjectileID.AmberHook"/>, <see cref="ProjectileID.SquirrelHook"/>, <see cref="ProjectileID.QueenSlimeHook"/>
	/// </summary>
	public const short Hook = 7;
	/// <summary>
	/// Behavior: Includes the Flower of Fire, Waterbolt, Cursed Flame, and Meowmere projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.BallofFire"/>, <see cref="ProjectileID.WaterBolt"/>, <see cref="ProjectileID.CursedFlameFriendly"/>, <see cref="ProjectileID.CursedFlameHostile"/>, <see cref="ProjectileID.BallofFrost"/>, <see cref="ProjectileID.Fireball"/>, <see cref="ProjectileID.Meowmere"/>
	/// </summary>
	public const short Bounce = 8;
	/// <summary>
	/// Behavior: Includes Flame Lash and Magic Missile<br/><br/>
	/// Used by: <see cref="ProjectileID.MagicMissile"/>, <see cref="ProjectileID.Flamelash"/>, <see cref="ProjectileID.RainbowRodBullet"/>, <see cref="ProjectileID.FlyingKnife"/>
	/// </summary>
	public const short MagicMissile = 9;
	/// <summary>
	/// Falling tiles like Sand spawn falling tile projectiles with this aiStyle. Item drop and placed tile can customized using <see cref="ProjectileID.Sets.FallingBlockTileItem"/>.<br/><br/>
	/// Used by: <see cref="ProjectileID.DirtBall"/>, <see cref="ProjectileID.SandBallFalling"/>, <see cref="ProjectileID.MudBall"/>, <see cref="ProjectileID.AshBallFalling"/>, <see cref="ProjectileID.SandBallGun"/>, <see cref="ProjectileID.EbonsandBallFalling"/>, <see cref="ProjectileID.EbonsandBallGun"/>, <see cref="ProjectileID.PearlSandBallFalling"/>, <see cref="ProjectileID.PearlSandBallGun"/>, <see cref="ProjectileID.SiltBall"/>, <see cref="ProjectileID.SnowBallHostile"/>, <see cref="ProjectileID.SlushBall"/>, <see cref="ProjectileID.CrimsandBallFalling"/>, <see cref="ProjectileID.CrimsandBallGun"/>, <see cref="ProjectileID.CopperCoinsFalling"/>, <see cref="ProjectileID.SilverCoinsFalling"/>, <see cref="ProjectileID.GoldCoinsFalling"/>, <see cref="ProjectileID.PlatinumCoinsFalling"/>, <see cref="ProjectileID.BlueDungeonDebris"/>, <see cref="ProjectileID.GreenDungeonDebris"/>, <see cref="ProjectileID.PinkDungeonDebris"/>, <see cref="ProjectileID.ShellPileFalling"/>
	/// </summary>
	public const short FallingTile = 10;
	/// <summary>
	/// Behavior: Includes Shadow Orb and Fairy pets<br/><br/>
	/// Used by: <see cref="ProjectileID.ShadowOrb"/>, <see cref="ProjectileID.BlueFairy"/>, <see cref="ProjectileID.PinkFairy"/>, <see cref="ProjectileID.GreenFairy"/>
	/// </summary>
	public const short FloatingFollow = 11;
	/// <summary>
	/// Behavior: Includes Aqua Scepter and Golden Shower projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.WaterStream"/>, <see cref="ProjectileID.GoldenShowerFriendly"/>, <see cref="ProjectileID.GoldenShowerHostile"/>
	/// </summary>
	public const short Stream = 12;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Harpoon"/>, <see cref="ProjectileID.GolemFist"/>, <see cref="ProjectileID.BoxingGlove"/>, <see cref="ProjectileID.ChainKnife"/>, <see cref="ProjectileID.ChainGuillotine"/>
	/// </summary>
	public const short Harpoon = 13;
	/// <summary>
	/// Behavior: Includes most non-destructive Explosive, Glowstick, and Spike Ball projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.SpikyBall"/>, <see cref="ProjectileID.Glowstick"/>, <see cref="ProjectileID.StickyGlowstick"/>, <see cref="ProjectileID.Beenade"/>, <see cref="ProjectileID.SpikyBallTrap"/>, <see cref="ProjectileID.SmokeBomb"/>, <see cref="ProjectileID.BoulderStaffOfEarth"/>, <see cref="ProjectileID.ThornBall"/>, <see cref="ProjectileID.GreekFire1"/>, <see cref="ProjectileID.GreekFire2"/>, <see cref="ProjectileID.GreekFire3"/>, <see cref="ProjectileID.OrnamentHostile"/>, <see cref="ProjectileID.Spike"/>, <see cref="ProjectileID.SpiderEgg"/>, <see cref="ProjectileID.MolotovFire"/>, <see cref="ProjectileID.MolotovFire2"/>, <see cref="ProjectileID.MolotovFire3"/>, <see cref="ProjectileID.SaucerScrap"/>, <see cref="ProjectileID.SpelunkerGlowstick"/>, <see cref="ProjectileID.CursedDartFlame"/>, <see cref="ProjectileID.SeedlerNut"/>, <see cref="ProjectileID.BouncyGlowstick"/>, <see cref="ProjectileID.Twinkle"/>, <see cref="ProjectileID.FairyGlowstick"/>, <see cref="ProjectileID.DripplerFlailExtraBall"/>
	/// </summary>
	public const short GroundProjectile = 14;
	/// <summary>
	/// Used by: <see cref="ProjectileID.BallOHurt"/>, <see cref="ProjectileID.BlueMoon"/>, <see cref="ProjectileID.Sunfury"/>, <see cref="ProjectileID.TheDaoofPow"/>, <see cref="ProjectileID.TheMeatball"/>, <see cref="ProjectileID.FlowerPow"/>, <see cref="ProjectileID.DripplerFlail"/>, <see cref="ProjectileID.Mace"/>, <see cref="ProjectileID.FlamingMace"/>
	/// </summary>
	public const short Flail = 15;
	/// <summary>
	/// Note that projectiles using <see cref="ProjectileID.Sets.Explosive"/> will utilize much of the custom logic corresponding to this aiStyle, allowing projectiles to behave like explosives without using this aiStyle directly.<br/><br/>
	/// Used by: <see cref="ProjectileID.Bomb"/>, <see cref="ProjectileID.Dynamite"/>, <see cref="ProjectileID.Grenade"/>, <see cref="ProjectileID.StickyBomb"/>, <see cref="ProjectileID.HappyBomb"/>, <see cref="ProjectileID.BombSkeletronPrime"/>, <see cref="ProjectileID.Explosives"/>, <see cref="ProjectileID.GrenadeI"/>, <see cref="ProjectileID.RocketI"/>, <see cref="ProjectileID.ProximityMineI"/>, <see cref="ProjectileID.GrenadeII"/>, <see cref="ProjectileID.RocketII"/>, <see cref="ProjectileID.ProximityMineII"/>, <see cref="ProjectileID.GrenadeIII"/>, <see cref="ProjectileID.RocketIII"/>, <see cref="ProjectileID.ProximityMineIII"/>, <see cref="ProjectileID.GrenadeIV"/>, <see cref="ProjectileID.RocketIV"/>, <see cref="ProjectileID.ProximityMineIV"/>, <see cref="ProjectileID.Landmine"/>, <see cref="ProjectileID.RocketSkeleton"/>, <see cref="ProjectileID.RocketSnowmanI"/>, <see cref="ProjectileID.RocketSnowmanII"/>, <see cref="ProjectileID.RocketSnowmanIII"/>, <see cref="ProjectileID.RocketSnowmanIV"/>, <see cref="ProjectileID.StickyGrenade"/>, <see cref="ProjectileID.StickyDynamite"/>, <see cref="ProjectileID.BouncyBomb"/>, <see cref="ProjectileID.BouncyGrenade"/>, <see cref="ProjectileID.BombFish"/>, <see cref="ProjectileID.PartyGirlGrenade"/>, <see cref="ProjectileID.BouncyDynamite"/>, <see cref="ProjectileID.DD2GoblinBomb"/>, <see cref="ProjectileID.ScarabBomb"/>, <see cref="ProjectileID.ClusterRocketI"/>, <see cref="ProjectileID.ClusterGrenadeI"/>, <see cref="ProjectileID.ClusterMineI"/>, <see cref="ProjectileID.ClusterFragmentsI"/>, <see cref="ProjectileID.ClusterRocketII"/>, <see cref="ProjectileID.ClusterGrenadeII"/>, <see cref="ProjectileID.ClusterMineII"/>, <see cref="ProjectileID.ClusterFragmentsII"/>, <see cref="ProjectileID.WetRocket"/>, <see cref="ProjectileID.WetGrenade"/>, <see cref="ProjectileID.WetMine"/>, <see cref="ProjectileID.LavaRocket"/>, <see cref="ProjectileID.LavaGrenade"/>, <see cref="ProjectileID.LavaMine"/>, <see cref="ProjectileID.HoneyRocket"/>, <see cref="ProjectileID.HoneyGrenade"/>, <see cref="ProjectileID.HoneyMine"/>, <see cref="ProjectileID.MiniNukeRocketI"/>, <see cref="ProjectileID.MiniNukeGrenadeI"/>, <see cref="ProjectileID.MiniNukeMineI"/>, <see cref="ProjectileID.MiniNukeRocketII"/>, <see cref="ProjectileID.MiniNukeGrenadeII"/>, <see cref="ProjectileID.MiniNukeMineII"/>, <see cref="ProjectileID.DryRocket"/>, <see cref="ProjectileID.DryGrenade"/>, <see cref="ProjectileID.DryMine"/>, <see cref="ProjectileID.ClusterSnowmanRocketI"/>, <see cref="ProjectileID.ClusterSnowmanRocketII"/>, <see cref="ProjectileID.WetSnowmanRocket"/>, <see cref="ProjectileID.LavaSnowmanRocket"/>, <see cref="ProjectileID.HoneySnowmanRocket"/>, <see cref="ProjectileID.MiniNukeSnowmanRocketI"/>, <see cref="ProjectileID.MiniNukeSnowmanRocketII"/>, <see cref="ProjectileID.DrySnowmanRocket"/>, <see cref="ProjectileID.ClusterSnowmanFragmentsI"/>, <see cref="ProjectileID.ClusterSnowmanFragmentsII"/>, <see cref="ProjectileID.WetBomb"/>, <see cref="ProjectileID.LavaBomb"/>, <see cref="ProjectileID.HoneyBomb"/>, <see cref="ProjectileID.DryBomb"/>, <see cref="ProjectileID.DirtBomb"/>, <see cref="ProjectileID.DirtStickyBomb"/>, <see cref="ProjectileID.SantankMountRocket"/>, <see cref="ProjectileID.TNTBarrel"/>
	/// </summary>
	public const short Explosive = 16;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Tombstone"/>, <see cref="ProjectileID.GraveMarker"/>, <see cref="ProjectileID.CrossGraveMarker"/>, <see cref="ProjectileID.Headstone"/>, <see cref="ProjectileID.Gravestone"/>, <see cref="ProjectileID.Obelisk"/>, <see cref="ProjectileID.RichGravestone1"/>, <see cref="ProjectileID.RichGravestone2"/>, <see cref="ProjectileID.RichGravestone3"/>, <see cref="ProjectileID.RichGravestone4"/>, <see cref="ProjectileID.RichGravestone5"/>
	/// </summary>
	public const short GraveMarker = 17;
	/// <summary>
	/// Used by: <see cref="ProjectileID.DemonSickle"/>, <see cref="ProjectileID.DemonScythe"/>, <see cref="ProjectileID.IceSickle"/>, <see cref="ProjectileID.DeathSickle"/>
	/// </summary>
	public const short Sickle = 18;
	/// <summary>
	/// Used by: <see cref="ProjectileID.DarkLance"/>, <see cref="ProjectileID.Trident"/>, <see cref="ProjectileID.Spear"/>, <see cref="ProjectileID.MythrilHalberd"/>, <see cref="ProjectileID.AdamantiteGlaive"/>, <see cref="ProjectileID.CobaltNaginata"/>, <see cref="ProjectileID.Gungnir"/>, <see cref="ProjectileID.MushroomSpear"/>, <see cref="ProjectileID.TheRottedFork"/>, <see cref="ProjectileID.PalladiumPike"/>, <see cref="ProjectileID.OrichalcumHalberd"/>, <see cref="ProjectileID.TitaniumTrident"/>, <see cref="ProjectileID.ChlorophytePartisan"/>, <see cref="ProjectileID.NorthPoleWeapon"/>, <see cref="ProjectileID.ObsidianSwordfish"/>, <see cref="ProjectileID.Swordfish"/>, <see cref="ProjectileID.ThunderSpear"/>, <see cref="ProjectileID.JoustingLance"/>, <see cref="ProjectileID.ShadowJoustingLance"/>, <see cref="ProjectileID.HallowJoustingLance"/>
	/// </summary>
	public const short Spear = 19;
	/// <summary>
	/// Behavior: Includes Saws<br/><br/>
	/// Used by: <see cref="ProjectileID.CobaltChainsaw"/>, <see cref="ProjectileID.MythrilChainsaw"/>, <see cref="ProjectileID.CobaltDrill"/>, <see cref="ProjectileID.MythrilDrill"/>, <see cref="ProjectileID.AdamantiteChainsaw"/>, <see cref="ProjectileID.AdamantiteDrill"/>, <see cref="ProjectileID.Hamdrax"/>, <see cref="ProjectileID.PalladiumDrill"/>, <see cref="ProjectileID.PalladiumChainsaw"/>, <see cref="ProjectileID.OrichalcumDrill"/>, <see cref="ProjectileID.OrichalcumChainsaw"/>, <see cref="ProjectileID.TitaniumDrill"/>, <see cref="ProjectileID.TitaniumChainsaw"/>, <see cref="ProjectileID.ChlorophyteDrill"/>, <see cref="ProjectileID.ChlorophyteChainsaw"/>, <see cref="ProjectileID.ChlorophyteJackhammer"/>, <see cref="ProjectileID.SawtoothShark"/>, <see cref="ProjectileID.VortexChainsaw"/>, <see cref="ProjectileID.VortexDrill"/>, <see cref="ProjectileID.NebulaChainsaw"/>, <see cref="ProjectileID.NebulaDrill"/>, <see cref="ProjectileID.SolarFlareChainsaw"/>, <see cref="ProjectileID.SolarFlareDrill"/>, <see cref="ProjectileID.ButchersChainsaw"/>, <see cref="ProjectileID.StardustDrill"/>, <see cref="ProjectileID.StardustChainsaw"/>
	/// </summary>
	public const short Drill = 20;
	/// <summary>
	/// Used by: <see cref="ProjectileID.QuarterNote"/>, <see cref="ProjectileID.EighthNote"/>, <see cref="ProjectileID.TiedEighthNote"/>
	/// </summary>
	public const short MusicNote = 21;
	/// <summary>
	/// Used by: <see cref="ProjectileID.IceBlock"/>
	/// </summary>
	public const short IceRod = 22;
	/// <summary>
	/// Behavior: Includes Cursed Flames and Eye Fire<br/><br/>
	/// Used by: <see cref="ProjectileID.EyeFire"/>, <see cref="ProjectileID.FlamesTrap"/>
	/// </summary>
	public const short Flames = 23;
	/// <summary>
	/// Used by: <see cref="ProjectileID.CrystalShard"/>, <see cref="ProjectileID.CrystalStorm"/>
	/// </summary>
	public const short CrystalShard = 24;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Boulder"/>, <see cref="ProjectileID.BeeHive"/>, <see cref="ProjectileID.RollingCactus"/>, <see cref="ProjectileID.MiniBoulder"/>, <see cref="ProjectileID.BouncyBoulder"/>, <see cref="ProjectileID.LifeCrystalBoulder"/>, <see cref="ProjectileID.MoonBoulder"/>
	/// </summary>
	public const short Boulder = 25;
	/// <summary>
	/// Behavior: Includes some minions with simple AI, such as the Baby Slime<br/><br/>
	/// Used by: <see cref="ProjectileID.Bunny"/>, <see cref="ProjectileID.Penguin"/>, <see cref="ProjectileID.Turtle"/>, <see cref="ProjectileID.BabyEater"/>, <see cref="ProjectileID.Pygmy"/>, <see cref="ProjectileID.Pygmy2"/>, <see cref="ProjectileID.Pygmy3"/>, <see cref="ProjectileID.Pygmy4"/>, <see cref="ProjectileID.BabySkeletronHead"/>, <see cref="ProjectileID.BabyHornet"/>, <see cref="ProjectileID.TikiSpirit"/>, <see cref="ProjectileID.PetLizard"/>, <see cref="ProjectileID.Parrot"/>, <see cref="ProjectileID.Truffle"/>, <see cref="ProjectileID.Sapling"/>, <see cref="ProjectileID.Wisp"/>, <see cref="ProjectileID.BabyDino"/>, <see cref="ProjectileID.BabySlime"/>, <see cref="ProjectileID.EyeSpring"/>, <see cref="ProjectileID.BabySnowman"/>, <see cref="ProjectileID.Spider"/>, <see cref="ProjectileID.Squashling"/>, <see cref="ProjectileID.BlackCat"/>, <see cref="ProjectileID.CursedSapling"/>, <see cref="ProjectileID.Puppy"/>, <see cref="ProjectileID.BabyGrinch"/>, <see cref="ProjectileID.ZephyrFish"/>, <see cref="ProjectileID.VenomSpider"/>, <see cref="ProjectileID.JumperSpider"/>, <see cref="ProjectileID.DangerousSpider"/>, <see cref="ProjectileID.MiniMinotaur"/>, <see cref="ProjectileID.BabyFaceMonster"/>, <see cref="ProjectileID.SugarGlider"/>, <see cref="ProjectileID.SharkPup"/>, <see cref="ProjectileID.LilHarpy"/>, <see cref="ProjectileID.FennecFox"/>, <see cref="ProjectileID.GlitteryButterfly"/>, <see cref="ProjectileID.BabyImp"/>, <see cref="ProjectileID.BabyRedPanda"/>, <see cref="ProjectileID.Plantero"/>, <see cref="ProjectileID.DynamiteKitten"/>, <see cref="ProjectileID.BabyWerewolf"/>, <see cref="ProjectileID.ShadowMimic"/>, <see cref="ProjectileID.VoltBunny"/>, <see cref="ProjectileID.KingSlimePet"/>, <see cref="ProjectileID.BrainOfCthulhuPet"/>, <see cref="ProjectileID.SkeletronPet"/>, <see cref="ProjectileID.QueenBeePet"/>, <see cref="ProjectileID.SkeletronPrimePet"/>, <see cref="ProjectileID.PlanteraPet"/>, <see cref="ProjectileID.GolemPet"/>, <see cref="ProjectileID.DukeFishronPet"/>, <see cref="ProjectileID.MoonLordPet"/>, <see cref="ProjectileID.EverscreamPet"/>, <see cref="ProjectileID.MartianPet"/>, <see cref="ProjectileID.DD2OgrePet"/>, <see cref="ProjectileID.DD2BetsyPet"/>, <see cref="ProjectileID.QueenSlimePet"/>, <see cref="ProjectileID.BerniePet"/>, <see cref="ProjectileID.DeerclopsPet"/>, <see cref="ProjectileID.PigPet"/>, <see cref="ProjectileID.ChesterPet"/>, <see cref="ProjectileID.JunimoPet"/>, <see cref="ProjectileID.BlueChickenPet"/>, <see cref="ProjectileID.Spiffo"/>, <see cref="ProjectileID.CavelingGardener"/>
	/// </summary>
	public const short Pet = 26;
	/// <summary>
	/// Used by: <see cref="ProjectileID.UnholyTridentFriendly"/>, <see cref="ProjectileID.UnholyTridentHostile"/>, <see cref="ProjectileID.SwordBeam"/>, <see cref="ProjectileID.TerraBeam"/>, <see cref="ProjectileID.LightBeam"/>, <see cref="ProjectileID.NightBeam"/>, <see cref="ProjectileID.EnchantedBeam"/>
	/// </summary>
	public const short Beam = 27;
	/// <summary>
	/// Behavior: Includes Ice Sword, Frost Hydra, Frost Bolt, and Icy Spit projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.IceBolt"/>, <see cref="ProjectileID.FrostBoltSword"/>, <see cref="ProjectileID.FrostBlastHostile"/>, <see cref="ProjectileID.RuneBlast"/>, <see cref="ProjectileID.IcewaterSpit"/>, <see cref="ProjectileID.FrostBlastFriendly"/>, <see cref="ProjectileID.FrostBoltStaff"/>, <see cref="ProjectileID.HoundiusShootiusFireball"/>
	/// </summary>
	public const short ColdBolt = 28;
	/// <summary>
	/// Used by: <see cref="ProjectileID.AmethystBolt"/>, <see cref="ProjectileID.TopazBolt"/>, <see cref="ProjectileID.SapphireBolt"/>, <see cref="ProjectileID.EmeraldBolt"/>, <see cref="ProjectileID.RubyBolt"/>, <see cref="ProjectileID.DiamondBolt"/>, <see cref="ProjectileID.CrystalPulse"/>, <see cref="ProjectileID.CrystalPulse2"/>, <see cref="ProjectileID.AmberBolt"/>, <see cref="ProjectileID.NebulaArcanumExplosionShot"/>, <see cref="ProjectileID.NebulaArcanumExplosionShotShard"/>, <see cref="ProjectileID.ThunderStaffShot"/>
	/// </summary>
	public const short GemStaffBolt = 29;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Mushroom"/>, <see cref="ProjectileID.OrnamentFriendly"/>, <see cref="ProjectileID.OrnamentStar"/>
	/// </summary>
	public const short Mushroom = 30;
	/// <summary>
	/// Used by: <see cref="ProjectileID.PureSpray"/>, <see cref="ProjectileID.HallowSpray"/>, <see cref="ProjectileID.CorruptSpray"/>, <see cref="ProjectileID.MushroomSpray"/>, <see cref="ProjectileID.CrimsonSpray"/>, <see cref="ProjectileID.SandSpray"/>, <see cref="ProjectileID.SnowSpray"/>, <see cref="ProjectileID.DirtSpray"/>
	/// </summary>
	public const short Spray = 31;
	/// <summary>
	/// Used by: <see cref="ProjectileID.BeachBall"/>
	/// </summary>
	public const short BeachBall = 32;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Flare"/>, <see cref="ProjectileID.BlueFlare"/>, <see cref="ProjectileID.SpelunkerFlare"/>, <see cref="ProjectileID.CursedFlare"/>, <see cref="ProjectileID.RainbowFlare"/>, <see cref="ProjectileID.ShimmerFlare"/>
	/// </summary>
	public const short Flare = 33;
	/// <summary>
	/// Used by: <see cref="ProjectileID.RocketFireworkRed"/>, <see cref="ProjectileID.RocketFireworkGreen"/>, <see cref="ProjectileID.RocketFireworkBlue"/>, <see cref="ProjectileID.RocketFireworkYellow"/>, <see cref="ProjectileID.RocketFireworksBoxRed"/>, <see cref="ProjectileID.RocketFireworksBoxGreen"/>, <see cref="ProjectileID.RocketFireworksBoxBlue"/>, <see cref="ProjectileID.RocketFireworksBoxYellow"/>
	/// </summary>
	public const short FireWork = 34;
	/// <summary>
	/// Used by: <see cref="ProjectileID.RopeCoil"/>, <see cref="ProjectileID.VineRopeCoil"/>, <see cref="ProjectileID.SilkRopeCoil"/>, <see cref="ProjectileID.WebRopeCoil"/>
	/// </summary>
	public const short RopeCoil = 35;
	/// <summary>
	/// Behavior: Includes Bee, Wasp, Tiny Eater, and Bat projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.Bee"/>, <see cref="ProjectileID.Wasp"/>, <see cref="ProjectileID.TinyEater"/>, <see cref="ProjectileID.Bat"/>, <see cref="ProjectileID.GiantBee"/>
	/// </summary>
	public const short SmallFlying = 36;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SpearTrap"/>
	/// </summary>
	public const short SpearTrap = 37;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FlamethrowerTrap"/>
	/// </summary>
	public const short FlameThrower = 38;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MechanicalPiranha"/>
	/// </summary>
	public const short MechanicalPiranha = 39;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Leaf"/>
	/// </summary>
	public const short Leaf = 40;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FlowerPetal"/>
	/// </summary>
	public const short FlowerPetal = 41;
	/// <summary>
	/// Used by: <see cref="ProjectileID.CrystalLeaf"/>
	/// </summary>
	public const short CrystalLeaf = 42;
	/// <summary>
	/// Used by: <see cref="ProjectileID.CrystalLeafShot"/>
	/// </summary>
	public const short CrystalLeafShot = 43;
	/// <summary>
	/// Behavior: Moves a short distance and then stops, includes Spore Cloud, Chlorophyte Orb, and Storm Spear Shot projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.SporeCloud"/>, <see cref="ProjectileID.ChlorophyteOrb"/>, <see cref="ProjectileID.ThunderSpearShot"/>
	/// </summary>
	public const short MoveShort = 44;
	/// <summary>
	/// Used by: <see cref="ProjectileID.RainCloudMoving"/>, <see cref="ProjectileID.RainCloudRaining"/>, <see cref="ProjectileID.RainFriendly"/>, <see cref="ProjectileID.BloodCloudMoving"/>, <see cref="ProjectileID.BloodCloudRaining"/>, <see cref="ProjectileID.BloodRain"/>, <see cref="ProjectileID.RainNimbus"/>
	/// </summary>
	public const short RainCloud = 45;
	/// <summary>
	/// Used by: <see cref="ProjectileID.RainbowFront"/>, <see cref="ProjectileID.RainbowBack"/>
	/// </summary>
	public const short Rainbow = 46;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MagnetSphereBall"/>
	/// </summary>
	public const short MagnetSphere = 47;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MagnetSphereBolt"/>, <see cref="ProjectileID.HeatRay"/>, <see cref="ProjectileID.ShadowBeamHostile"/>, <see cref="ProjectileID.ShadowBeamFriendly"/>, <see cref="ProjectileID.UFOLaser"/>
	/// </summary>
	public const short Ray = 48;
	/// <summary>
	/// Used by: <see cref="ProjectileID.ExplosiveBunny"/>
	/// </summary>
	public const short ExplosiveBunny = 49;
	/// <summary>
	/// Used by: <see cref="ProjectileID.InfernoHostileBolt"/>, <see cref="ProjectileID.InfernoHostileBlast"/>, <see cref="ProjectileID.InfernoFriendlyBolt"/>, <see cref="ProjectileID.InfernoFriendlyBlast"/>
	/// </summary>
	public const short Inferno = 50;
	/// <summary>
	/// Used by: <see cref="ProjectileID.LostSoulHostile"/>, <see cref="ProjectileID.LostSoulFriendly"/>
	/// </summary>
	public const short LostSoul = 51;
	/// <summary>
	/// Behavior: Includes Spirit Heal from the Spectre Hood and Vampire Heal from the Vampire Knives<br/><br/>
	/// Used by: <see cref="ProjectileID.SpiritHeal"/>, <see cref="ProjectileID.VampireHeal"/>
	/// </summary>
	public const short Heal = 52;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FrostHydra"/>, <see cref="ProjectileID.SpiderHiver"/>, <see cref="ProjectileID.HoundiusShootius"/>
	/// </summary>
	public const short FrostHydra = 53;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Raven"/>
	/// </summary>
	public const short Raven = 54;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FlamingJack"/>
	/// </summary>
	public const short FlamingJack = 55;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FlamingScythe"/>
	/// </summary>
	public const short FlamingScythe = 56;
	/// <summary>
	/// Used by: <see cref="ProjectileID.NorthPoleSpear"/>
	/// </summary>
	public const short NorthPoleSpear = 57;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Present"/>
	/// </summary>
	public const short Present = 58;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SpectreWrath"/>
	/// </summary>
	public const short SpectreWrath = 59;
	/// <summary>
	/// Used by: <see cref="ProjectileID.WaterGun"/>, <see cref="ProjectileID.SlimeGun"/>
	/// </summary>
	public const short WaterJet = 60;
	/// <summary>
	/// Used by: <see cref="ProjectileID.BobberWooden"/>, <see cref="ProjectileID.BobberReinforced"/>, <see cref="ProjectileID.BobberFiberglass"/>, <see cref="ProjectileID.BobberFisherOfSouls"/>, <see cref="ProjectileID.BobberGolden"/>, <see cref="ProjectileID.BobberMechanics"/>, <see cref="ProjectileID.BobbersittingDuck"/>, <see cref="ProjectileID.BobberFleshcatcher"/>, <see cref="ProjectileID.BobberHotline"/>, <see cref="ProjectileID.BobberBloody"/>, <see cref="ProjectileID.BobberScarab"/>, <see cref="ProjectileID.FishingBobber"/>, <see cref="ProjectileID.FishingBobberGlowingStar"/>, <see cref="ProjectileID.FishingBobberGlowingLava"/>, <see cref="ProjectileID.FishingBobberGlowingKrypton"/>, <see cref="ProjectileID.FishingBobberGlowingXenon"/>, <see cref="ProjectileID.FishingBobberGlowingArgon"/>, <see cref="ProjectileID.FishingBobberGlowingViolet"/>, <see cref="ProjectileID.FishingBobberGlowingRainbow"/>
	/// </summary>
	public const short Bobber = 61;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Hornet"/>, <see cref="ProjectileID.FlyingImp"/>, <see cref="ProjectileID.Tempest"/>, <see cref="ProjectileID.UFOMinion"/>, <see cref="ProjectileID.StardustCellMinion"/>, <see cref="ProjectileID.AbigailMinion"/>
	/// </summary>
	public const short Hornet = 62;
	/// <summary>
	/// Used by: <see cref="ProjectileID.BabySpider"/>
	/// </summary>
	public const short BabySpider = 63;
	/// <summary>
	/// Behavior: Includes Sharknado and Cthulunado projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.Sharknado"/>, <see cref="ProjectileID.Cthulunado"/>
	/// </summary>
	public const short Nado = 64;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SharknadoBolt"/>
	/// </summary>
	public const short SharknadoBolt = 65;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Retanimini"/>, <see cref="ProjectileID.Spazmamini"/>, <see cref="ProjectileID.DeadlySphere"/>
	/// </summary>
	public const short MiniTwins = 66;
	/// <summary>
	/// Behavior: Includes Mini Pirate, Crimson Heart, Companion Cube, Vampire Frog, and Desert Tiger projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.OneEyedPirate"/>, <see cref="ProjectileID.SoulscourgePirate"/>, <see cref="ProjectileID.PirateCaptain"/>, <see cref="ProjectileID.CrimsonHeart"/>, <see cref="ProjectileID.CompanionCube"/>, <see cref="ProjectileID.VampireFrog"/>, <see cref="ProjectileID.StormTigerTier1"/>, <see cref="ProjectileID.StormTigerTier2"/>, <see cref="ProjectileID.StormTigerTier3"/>, <see cref="ProjectileID.FlinxMinion"/>, <see cref="ProjectileID.DirtiestBlock"/>
	/// </summary>
	public const short CommonFollow = 67;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MolotovCocktail"/>, <see cref="ProjectileID.Ale"/>
	/// </summary>
	public const short MolotovCocktail = 68;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Flairon"/>
	/// </summary>
	public const short Flairon = 69;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FlaironBubble"/>
	/// </summary>
	public const short FlaironBubble = 70;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Typhoon"/>
	/// </summary>
	public const short Typhoon = 71;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Bubble"/>
	/// </summary>
	public const short Bubble = 72;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FireworkFountainYellow"/>, <see cref="ProjectileID.FireworkFountainRed"/>, <see cref="ProjectileID.FireworkFountainBlue"/>, <see cref="ProjectileID.FireworkFountainRainbow"/>
	/// </summary>
	public const short FireWorkFountain = 73;
	/// <summary>
	/// Used by: <see cref="ProjectileID.ScutlixLaserFriendly"/>
	/// </summary>
	public const short ScutlixLaser = 74;
	/// <summary>
	/// Used by: <see cref="ProjectileID.LaserMachinegun"/>, <see cref="ProjectileID.LaserDrill"/>, <see cref="ProjectileID.ChargedBlasterCannon"/>, <see cref="ProjectileID.Arkhalis"/>, <see cref="ProjectileID.PortalGun"/>, <see cref="ProjectileID.SolarWhipSword"/>, <see cref="ProjectileID.VortexBeater"/>, <see cref="ProjectileID.Phantasm"/>, <see cref="ProjectileID.LastPrism"/>, <see cref="ProjectileID.DD2PhoenixBow"/>, <see cref="ProjectileID.Celeb2Weapon"/>, <see cref="ProjectileID.Terragrim"/>, <see cref="ProjectileID.PiercingStarlight"/>
	/// </summary>
	public const short HeldProjectile = 75;
	/// <summary>
	/// Used by: <see cref="ProjectileID.ScutlixLaserCrosshair"/>, <see cref="ProjectileID.DrillMountCrosshair"/>
	/// </summary>
	public const short Crosshair = 76;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Electrosphere"/>
	/// </summary>
	public const short Electrosphere = 77;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Xenopopper"/>
	/// </summary>
	public const short Xenopopper = 78;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SaucerDeathray"/>
	/// </summary>
	public const short MartianDeathRay = 79;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SaucerMissile"/>
	/// </summary>
	public const short MartianRocket = 80;
	/// <summary>
	/// Used by: <see cref="ProjectileID.InfluxWaver"/>
	/// </summary>
	public const short InfluxWaver = 81;
	/// <summary>
	/// Used by: <see cref="ProjectileID.PhantasmalEye"/>
	/// </summary>
	public const short PhantasmalEye = 82;
	/// <summary>
	/// Used by: <see cref="ProjectileID.PhantasmalSphere"/>
	/// </summary>
	public const short PhantasmalSphere = 83;
	/// <summary>
	/// Behavior: Includes Charged Laser Blaster, Stardust Laser, Last Prism, and Lunar Portal Laser projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.PhantasmalDeathray"/>, <see cref="ProjectileID.ChargedBlasterLaser"/>, <see cref="ProjectileID.StardustSoldierLaser"/>, <see cref="ProjectileID.LastPrismLaser"/>, <see cref="ProjectileID.MoonlordTurretLaser"/>
	/// </summary>
	public const short ThickLaser = 84;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MoonLeech"/>
	/// </summary>
	public const short MoonLeech = 85;
	/// <summary>
	/// Used by: <see cref="ProjectileID.CultistBossIceMist"/>
	/// </summary>
	public const short IceMist = 86;
	/// <summary>
	/// Used by: <see cref="ProjectileID.ClingerStaff"/>
	/// </summary>
	public const short CursedFlameWall = 87;
	/// <summary>
	/// Used by: <see cref="ProjectileID.CultistBossLightningOrb"/>, <see cref="ProjectileID.CultistBossLightningOrbArc"/>, <see cref="ProjectileID.VortexLightning"/>
	/// </summary>
	public const short LightningOrb = 88;
	/// <summary>
	/// Used by: <see cref="ProjectileID.CultistRitual"/>
	/// </summary>
	public const short LightningRitual = 89;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MagicLantern"/>
	/// </summary>
	public const short MagicLantern = 90;
	/// <summary>
	/// Used by: <see cref="ProjectileID.ShadowFlame"/>
	/// </summary>
	public const short ShadowFlame = 91;
	/// <summary>
	/// Used by: <see cref="ProjectileID.ToxicCloud"/>, <see cref="ProjectileID.ToxicCloud2"/>, <see cref="ProjectileID.ToxicCloud3"/>, <see cref="ProjectileID.GasTrap"/>
	/// </summary>
	public const short ToxicCloud = 92;
	/// <summary>
	/// Used by: <see cref="ProjectileID.NailFriendly"/>
	/// </summary>
	public const short Nail = 93;
	/// <summary>
	/// Used by: <see cref="ProjectileID.CoinPortal"/>
	/// </summary>
	public const short CoinPortal = 94;
	/// <summary>
	/// Used by: <see cref="ProjectileID.ToxicBubble"/>
	/// </summary>
	public const short ToxicBubble = 95;
	/// <summary>
	/// Used by: <see cref="ProjectileID.IchorSplash"/>
	/// </summary>
	public const short IchorSplash = 96;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FlyingPiggyBank"/>
	/// </summary>
	public const short FlyingPiggyBank = 97;
	/// <summary>
	/// Used by: <see cref="ProjectileID.CultistBossParticle"/>
	/// </summary>
	public const short MysteriousTablet = 98;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Code1"/>, <see cref="ProjectileID.WoodYoyo"/>, <see cref="ProjectileID.CorruptYoyo"/>, <see cref="ProjectileID.CrimsonYoyo"/>, <see cref="ProjectileID.JungleYoyo"/>, <see cref="ProjectileID.Cascade"/>, <see cref="ProjectileID.Chik"/>, <see cref="ProjectileID.Code2"/>, <see cref="ProjectileID.Rally"/>, <see cref="ProjectileID.Yelets"/>, <see cref="ProjectileID.RedsYoyo"/>, <see cref="ProjectileID.ValkyrieYoyo"/>, <see cref="ProjectileID.Amarok"/>, <see cref="ProjectileID.HelFire"/>, <see cref="ProjectileID.Kraken"/>, <see cref="ProjectileID.TheEyeOfCthulhu"/>, <see cref="ProjectileID.BlackCounterweight"/>, <see cref="ProjectileID.BlueCounterweight"/>, <see cref="ProjectileID.GreenCounterweight"/>, <see cref="ProjectileID.PurpleCounterweight"/>, <see cref="ProjectileID.RedCounterweight"/>, <see cref="ProjectileID.YellowCounterweight"/>, <see cref="ProjectileID.FormatC"/>, <see cref="ProjectileID.Gradient"/>, <see cref="ProjectileID.Valor"/>, <see cref="ProjectileID.Terrarian"/>, <see cref="ProjectileID.HiveFive"/>
	/// </summary>
	public const short Yoyo = 99;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MedusaHead"/>
	/// </summary>
	public const short MedusaRay = 100;
	/// <summary>
	/// Behavior: Includes Medusa Head Ray and Mechanical Cart Laser projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.MedusaHeadRay"/>, <see cref="ProjectileID.MinecartMechLaser"/>
	/// </summary>
	public const short HorizontalRay = 101;
	/// <summary>
	/// Behavior: Includes Flow Invader, Nebular Piercer, and Nebula Eye projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.StardustJellyfishSmall"/>, <see cref="ProjectileID.NebulaBolt"/>, <see cref="ProjectileID.NebulaEye"/>
	/// </summary>
	public const short LunarProjectile = 102;
	/// <summary>
	/// Used by: <see cref="ProjectileID.StardustTowerMark"/>
	/// </summary>
	public const short Starmark = 103;
	/// <summary>
	/// Used by: <see cref="ProjectileID.BrainOfConfusion"/>
	/// </summary>
	public const short BrainofConfusion = 104;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SporeTrap"/>, <see cref="ProjectileID.SporeTrap2"/>
	/// </summary>
	public const short SporeTrap = 105;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SporeGas"/>, <see cref="ProjectileID.SporeGas2"/>, <see cref="ProjectileID.SporeGas3"/>
	/// </summary>
	public const short SporeGas = 106;
	/// <summary>
	/// Behavior: Includes Desert Sprit's Curse<br/><br/>
	/// Used by: <see cref="ProjectileID.NebulaSphere"/>, <see cref="ProjectileID.DesertDjinnCurse"/>
	/// </summary>
	public const short NebulaSphere = 107;
	/// <summary>
	/// Behavior: Includes Blood Tears<br/><br/>
	/// Used by: <see cref="ProjectileID.VortexVortexLightning"/>, <see cref="ProjectileID.VortexVortexPortal"/>, <see cref="ProjectileID.BloodNautilusTears"/>
	/// </summary>
	public const short Vortex = 108;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MechanicWrench"/>
	/// </summary>
	public const short MechanicWrench = 109;
	/// <summary>
	/// Used by: <see cref="ProjectileID.NurseSyringeHeal"/>
	/// </summary>
	public const short NurseSyringe = 110;
	/// <summary>
	/// Used by: <see cref="ProjectileID.DryadsWardCircle"/>
	/// </summary>
	public const short DryadWard = 111;
	/// <summary>
	/// Behavior: Includes Truffle Spore, Rainbow Crystal Explosion, and Dandelion Seed projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.TruffleSpore"/>, <see cref="ProjectileID.RainbowCrystalExplosion"/>, <see cref="ProjectileID.DandelionSeed"/>
	/// </summary>
	public const short SmallProximityExplosion = 112;
	/// <summary>
	/// Behavior: Includes Bone Javelin, Stardust Cell Shot, and Daybreak projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.BoneJavelin"/>, <see cref="ProjectileID.StardustCellMinionShot"/>, <see cref="ProjectileID.Daybreak"/>, <see cref="ProjectileID.TentacleSpike"/>, <see cref="ProjectileID.BloodButcherer"/>
	/// </summary>
	public const short StickProjectile = 113;
	/// <summary>
	/// Used by: <see cref="ProjectileID.PortalGunGate"/>
	/// </summary>
	public const short PortalGate = 114;
	/// <summary>
	/// Used by: <see cref="ProjectileID.TerrarianBeam"/>
	/// </summary>
	public const short TerrarianBeam = 115;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SolarFlareRay"/>
	/// </summary>
	public const short DrakomiteFlare = 116;
	/// <summary>
	/// Behavior: Includes Solar Radience and Solar Eruption Explosion projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.SolarCounter"/>, <see cref="ProjectileID.SolarWhipSwordExplosion"/>, <see cref="ProjectileID.StardustGuardianExplosion"/>, <see cref="ProjectileID.DaybreakExplosion"/>
	/// </summary>
	public const short SolarEffect = 117;
	/// <summary>
	/// Used by: <see cref="ProjectileID.NebulaArcanum"/>
	/// </summary>
	public const short NebulaArcanum = 118;
	/// <summary>
	/// Used by: <see cref="ProjectileID.NebulaArcanumSubshot"/>
	/// </summary>
	public const short ArcanumSubShot = 119;
	/// <summary>
	/// Used by: <see cref="ProjectileID.StardustGuardian"/>
	/// </summary>
	public const short StardustGuardian = 120;
	/// <summary>
	/// Used by: <see cref="ProjectileID.StardustDragon1"/>, <see cref="ProjectileID.StardustDragon2"/>, <see cref="ProjectileID.StardustDragon3"/>, <see cref="ProjectileID.StardustDragon4"/>
	/// </summary>
	public const short StardustDragon = 121;
	/// <summary>
	/// Behavior: The effect displayed when killing a Lunar Event enemy while it's respective Celestial Pillar is alive, also used by Phantasm Arrow<br/><br/>
	/// Used by: <see cref="ProjectileID.TowerDamageBolt"/>, <see cref="ProjectileID.PhantasmArrow"/>
	/// </summary>
	public const short ReleasedEnergy = 122;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MoonlordTurret"/>, <see cref="ProjectileID.RainbowCrystal"/>
	/// </summary>
	public const short LunarSentry = 123;
	/// <summary>
	/// Behavior: Includes Suspicious Looking Tentacle, Suspicious Eye, Rez and Spaz, Fairy Princess, Jack 'O Lantern, and Ice Queen pets<br/><br/>
	/// Used by: <see cref="ProjectileID.SuspiciousTentacle"/>, <see cref="ProjectileID.EyeOfCthulhuPet"/>, <see cref="ProjectileID.TwinsPet"/>, <see cref="ProjectileID.FairyQueenPet"/>, <see cref="ProjectileID.PumpkingPet"/>, <see cref="ProjectileID.IceQueenPet"/>, <see cref="ProjectileID.GlommerPet"/>
	/// </summary>
	public const short FloatInFrontPet = 124;
	/// <summary>
	/// Used by: <see cref="ProjectileID.WireKite"/>
	/// </summary>
	public const short WireKite = 125;
	/// <summary>
	/// Used by: <see cref="ProjectileID.GeyserTrap"/>, <see cref="ProjectileID.DD2OgreStomp"/>
	/// </summary>
	public const short Geyser = 126;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SandnadoFriendly"/>, <see cref="ProjectileID.SandnadoHostile"/>
	/// </summary>
	public const short AncientStorm = 127;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SandnadoHostileMark"/>
	/// </summary>
	public const short AncientStormMark = 128;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SpiritFlame"/>
	/// </summary>
	public const short SpiritFlame = 129;
	/// <summary>
	/// Used by: <see cref="ProjectileID.DD2FlameBurstTowerT1"/>, <see cref="ProjectileID.DD2FlameBurstTowerT2"/>, <see cref="ProjectileID.DD2FlameBurstTowerT3"/>
	/// </summary>
	public const short DD2FlameBurst = 130;
	/// <summary>
	/// Used by: <see cref="ProjectileID.DD2FlameBurstTowerT1Shot"/>, <see cref="ProjectileID.DD2FlameBurstTowerT2Shot"/>, <see cref="ProjectileID.DD2FlameBurstTowerT3Shot"/>
	/// </summary>
	public const short DD2FlameBurstShot = 131;
	/// <summary>
	/// Behavior: Eternia Crystal destroyed<br/><br/>
	/// Used by: <see cref="ProjectileID.DD2ElderWins"/>
	/// </summary>
	public const short DD2GrimEnd = 132;
	/// <summary>
	/// Used by: <see cref="ProjectileID.DD2DarkMageRaise"/>, <see cref="ProjectileID.DD2DarkMageHeal"/>
	/// </summary>
	public const short DD2DarkSigil = 133;
	/// <summary>
	/// Used by: <see cref="ProjectileID.DD2BallistraTowerT1"/>, <see cref="ProjectileID.DD2BallistraTowerT2"/>, <see cref="ProjectileID.DD2BallistraTowerT3"/>
	/// </summary>
	public const short DD2Ballista = 134;
	/// <summary>
	/// Behavior: Includes Ogre's Stomp and Geyser projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.DD2OgreSmash"/>, <see cref="ProjectileID.QueenSlimeSmash"/>
	/// </summary>
	public const short UpwardExpand = 135;
	/// <summary>
	/// Used by: <see cref="ProjectileID.DD2BetsyFlameBreath"/>
	/// </summary>
	public const short DD2BetsysBreath = 136;
	/// <summary>
	/// Used by: <see cref="ProjectileID.DD2LightningAuraT1"/>, <see cref="ProjectileID.DD2LightningAuraT2"/>, <see cref="ProjectileID.DD2LightningAuraT3"/>
	/// </summary>
	public const short DD2LightningAura = 137;
	/// <summary>
	/// Used by: <see cref="ProjectileID.DD2ExplosiveTrapT1"/>, <see cref="ProjectileID.DD2ExplosiveTrapT2"/>, <see cref="ProjectileID.DD2ExplosiveTrapT3"/>
	/// </summary>
	public const short DD2ExplosiveTrap = 138;
	/// <summary>
	/// Used by: <see cref="ProjectileID.DD2ExplosiveTrapT1Explosion"/>, <see cref="ProjectileID.DD2ExplosiveTrapT2Explosion"/>, <see cref="ProjectileID.DD2ExplosiveTrapT3Explosion"/>
	/// </summary>
	public const short DD2ExplosiveTrapExplosion = 139;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MonkStaffT1"/>, <see cref="ProjectileID.MonkStaffT3"/>
	/// </summary>
	public const short SleepyOctopod = 140;
	/// <summary>
	/// Behavior: The effect created on use of the Sleepy Octopod<br/><br/>
	/// Used by: <see cref="ProjectileID.MonkStaffT1Explosion"/>
	/// </summary>
	public const short PoleSmash = 141;
	/// <summary>
	/// Behavior: Use style of the Ghastly Glaive and Sky Dragon's Fury alt1<br/><br/>
	/// Used by: <see cref="ProjectileID.MonkStaffT2"/>, <see cref="ProjectileID.MonkStaffT3_Alt"/>
	/// </summary>
	public const short ForwardStab = 142;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MonkStaffT2Ghast"/>
	/// </summary>
	public const short Ghast = 143;
	/// <summary>
	/// Behavior: Includes the Hoardragon, Flickerwick, Estee, and Propeller Gato<br/><br/>
	/// Used by: <see cref="ProjectileID.DD2PetDragon"/>, <see cref="ProjectileID.DD2PetGhost"/>, <see cref="ProjectileID.DD2PetGato"/>, <see cref="ProjectileID.UpbeatStar"/>
	/// </summary>
	public const short FloatBehindPet = 144;
	/// <summary>
	/// Used by: <see cref="ProjectileID.DD2ApprenticeStorm"/>
	/// </summary>
	public const short WisdomWhirlwind = 145;
	/// <summary>
	/// Behavior: Old One's Army defeated<br/><br/>
	/// Used by: <see cref="ProjectileID.DD2Win"/>
	/// </summary>
	public const short DD2Victory = 146;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Celeb2Rocket"/>, <see cref="ProjectileID.Celeb2RocketExplosive"/>, <see cref="ProjectileID.Celeb2RocketLarge"/>, <see cref="ProjectileID.Celeb2RocketExplosiveLarge"/>
	/// </summary>
	public const short CelebrationMk2Shots = 147;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FallingStarSpawner"/>
	/// </summary>
	public const short FallingStarAnimation = 148;
	/// <summary>
	/// Used by: <see cref="ProjectileID.DirtGolfBall"/>, <see cref="ProjectileID.GolfBallDyedBlack"/>, <see cref="ProjectileID.GolfBallDyedBlue"/>, <see cref="ProjectileID.GolfBallDyedBrown"/>, <see cref="ProjectileID.GolfBallDyedCyan"/>, <see cref="ProjectileID.GolfBallDyedGreen"/>, <see cref="ProjectileID.GolfBallDyedLimeGreen"/>, <see cref="ProjectileID.GolfBallDyedOrange"/>, <see cref="ProjectileID.GolfBallDyedPink"/>, <see cref="ProjectileID.GolfBallDyedPurple"/>, <see cref="ProjectileID.GolfBallDyedRed"/>, <see cref="ProjectileID.GolfBallDyedSkyBlue"/>, <see cref="ProjectileID.GolfBallDyedTeal"/>, <see cref="ProjectileID.GolfBallDyedViolet"/>, <see cref="ProjectileID.GolfBallDyedYellow"/>
	/// </summary>
	public const short GolfBall = 149;
	/// <summary>
	/// Used by: <see cref="ProjectileID.GolfClubHelper"/>
	/// </summary>
	public const short GolfClub = 150;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SuperStar"/>
	/// </summary>
	public const short SuperStar = 151;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SuperStarSlash"/>, <see cref="ProjectileID.BladeOfGrass"/>, <see cref="ProjectileID.Muramasa"/>
	/// </summary>
	public const short SuperStarBeam = 152;
	/// <summary>
	/// Used by: <see cref="ProjectileID.ToiletEffect"/>
	/// </summary>
	public const short ToiletEffect = 153;
	/// <summary>
	/// Used by: <see cref="ProjectileID.VoidLens"/>
	/// </summary>
	public const short VoidBag = 154;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MysticSnakeCoil"/>
	/// </summary>
	public const short SnakeCoil = 155;
	/// <summary>
	/// Behavior: Includes the Sanguine Bat<br/><br/>
	/// Used by: <see cref="ProjectileID.BatOfLight"/>, <see cref="ProjectileID.EmpressBlade"/>
	/// </summary>
	public const short Terraprisma = 156;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SharpTears"/>, <see cref="ProjectileID.DeerclopsIceSpike"/>
	/// </summary>
	public const short BloodThorn = 157;
	/// <summary>
	/// Used by: <see cref="ProjectileID.BabyBird"/>
	/// </summary>
	public const short Finch = 158;
	/// <summary>
	/// Used by: <see cref="ProjectileID.PaperAirplaneA"/>, <see cref="ProjectileID.PaperAirplaneB"/>
	/// </summary>
	public const short PaperPlane = 159;
	/// <summary>
	/// Used by: <see cref="ProjectileID.KiteBlue"/>, <see cref="ProjectileID.KiteBlueAndYellow"/>, <see cref="ProjectileID.KiteRed"/>, <see cref="ProjectileID.KiteRedAndYellow"/>, <see cref="ProjectileID.KiteYellow"/>, <see cref="ProjectileID.KiteWyvern"/>, <see cref="ProjectileID.KiteBoneSerpent"/>, <see cref="ProjectileID.KiteWorldFeeder"/>, <see cref="ProjectileID.KiteBunny"/>, <see cref="ProjectileID.KitePigron"/>, <see cref="ProjectileID.KiteManEater"/>, <see cref="ProjectileID.KiteJellyfishBlue"/>, <see cref="ProjectileID.KiteJellyfishPink"/>, <see cref="ProjectileID.KiteShark"/>, <see cref="ProjectileID.KiteSandShark"/>, <see cref="ProjectileID.KiteBunnyCorrupt"/>, <see cref="ProjectileID.KiteBunnyCrimson"/>, <see cref="ProjectileID.KiteGoldfish"/>, <see cref="ProjectileID.KiteAngryTrapper"/>, <see cref="ProjectileID.KiteKoi"/>, <see cref="ProjectileID.KiteCrawltipede"/>, <see cref="ProjectileID.KiteSpectrum"/>, <see cref="ProjectileID.KiteWanderingEye"/>, <see cref="ProjectileID.KiteUnicorn"/>
	/// </summary>
	public const short Kite = 160;
	/// <summary>
	/// Used by: <see cref="ProjectileID.GladiusStab"/>, <see cref="ProjectileID.RulerStab"/>, <see cref="ProjectileID.CopperShortswordStab"/>, <see cref="ProjectileID.TinShortswordStab"/>, <see cref="ProjectileID.IronShortswordStab"/>, <see cref="ProjectileID.LeadShortswordStab"/>, <see cref="ProjectileID.SilverShortswordStab"/>, <see cref="ProjectileID.TungstenShortswordStab"/>, <see cref="ProjectileID.GoldShortswordStab"/>, <see cref="ProjectileID.PlatinumShortswordStab"/>
	/// </summary>
	public const short ShortSword = 161;
	/// <summary>
	/// Used by: <see cref="ProjectileID.WhiteTigerPounce"/>
	/// </summary>
	public const short DesertTiger = 162;
	/// <summary>
	/// Used by: <see cref="ProjectileID.ChumBucket"/>
	/// </summary>
	public const short Chum = 163;
	/// <summary>
	/// Used by: <see cref="ProjectileID.StormTigerGem"/>, <see cref="ProjectileID.AbigailCounter"/>
	/// </summary>
	public const short DesertTigerBall = 164;
	/// <summary>
	/// Used by: <see cref="ProjectileID.BlandWhip"/>, <see cref="ProjectileID.SwordWhip"/>, <see cref="ProjectileID.MaceWhip"/>, <see cref="ProjectileID.ScytheWhip"/>, <see cref="ProjectileID.CoolWhip"/>, <see cref="ProjectileID.FireWhip"/>, <see cref="ProjectileID.ThornWhip"/>, <see cref="ProjectileID.RainbowWhip"/>, <see cref="ProjectileID.BoneWhip"/>
	/// </summary>
	public const short Whip = 165;
	/// <summary>
	/// Behavior: Includes Dove and Lantern projectiles<br/><br/>
	/// Used by: <see cref="ProjectileID.ReleaseDoves"/>, <see cref="ProjectileID.ReleaseLantern"/>
	/// </summary>
	public const short ReleasedProjectile = 166;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SparkleGuitar"/>
	/// </summary>
	public const short StellarTune = 167;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FirstFractal"/>
	/// </summary>
	public const short FirstFractal = 168;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Smolstar"/>
	/// </summary>
	public const short EnchantedDagger = 169;
	/// <summary>
	/// Behavior: Only used when the Fairy GlowSticks's ai[1] is greater than 0<br/><br/>
	/// Used by: None
	/// </summary>
	public const short FairyGlowStick = 170;
	/// <summary>
	/// Behavior: Includes the Prismatic Bolt and Nightglow projectiles, these float for a second and then fly toward their target<br/><br/>
	/// Used by: <see cref="ProjectileID.HallowBossRainbowStreak"/>, <see cref="ProjectileID.FairyQueenMagicItemShot"/>
	/// </summary>
	public const short FloatAndFly = 171;
	/// <summary>
	/// Used by: <see cref="ProjectileID.HallowBossSplitShotCore"/>
	/// </summary>
	public const short SplitShotCore = 172;
	/// <summary>
	/// Used by: <see cref="ProjectileID.HallowBossLastingRainbow"/>
	/// </summary>
	public const short EverlastingRainbow = 173;
	/// <summary>
	/// Used by: <see cref="ProjectileID.EaterOfWorldsPet"/>, <see cref="ProjectileID.DestroyerPet"/>, <see cref="ProjectileID.LunaticCultistPet"/>
	/// </summary>
	public const short WormPet = 174;
	/// <summary>
	/// Used by: <see cref="ProjectileID.TitaniumStormShard"/>
	/// </summary>
	public const short TitaniumShard = 175;
	/// <summary>
	/// Behavior: The effect displayed when an enemy is hit with the Dark Harvest whip<br/><br/>
	/// Used by: <see cref="ProjectileID.ScytheWhipProj"/>
	/// </summary>
	public const short Reaping = 176;
	/// <summary>
	/// Used by: <see cref="ProjectileID.CoolWhipProj"/>, <see cref="ProjectileID.WeatherPainShot"/>
	/// </summary>
	public const short CoolFlake = 177;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FireWhipProj"/>
	/// </summary>
	public const short FireCracker = 178;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FairyQueenLance"/>
	/// </summary>
	public const short EtherealLance = 179;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FairyQueenSunDance"/>
	/// </summary>
	public const short SunDance = 180;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FairyQueenRangedItemShot"/>
	/// </summary>
	public const short TwilightLance = 181;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FinalFractal"/>
	/// </summary>
	public const short Zenith = 182;
	/// <summary>
	/// Used by: <see cref="ProjectileID.ZoologistStrikeGreen"/>, <see cref="ProjectileID.ZoologistStrikeRed"/>
	/// </summary>
	public const short ZoologistStike = 183;
	/// <summary>
	/// Behavior: The Torch God event, not the projectiles fired out of the torches<br/><br/>
	/// Used by: <see cref="ProjectileID.TorchGod"/>
	/// </summary>
	public const short TorchGod = 184;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SoulDrain"/>
	/// </summary>
	public const short LifeDrain = 185;
	/// <summary>
	/// Used by: <see cref="ProjectileID.PrincessWeapon"/>
	/// </summary>
	public const short PrincessWeapon = 186;
	/// <summary>
	/// Used by: <see cref="ProjectileID.InsanityShadowFriendly"/>, <see cref="ProjectileID.InsanityShadowHostile"/>
	/// </summary>
	public const short ShadowHand = 187;
	/// <summary>
	/// Used by: <see cref="ProjectileID.LightsBane"/>
	/// </summary>
	public const short LightsBane = 188;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Volcano"/>
	/// </summary>
	public const short Volcano = 189;
	/// <summary>
	/// Used by: <see cref="ProjectileID.NightsEdge"/>, <see cref="ProjectileID.Excalibur"/>, <see cref="ProjectileID.TrueExcalibur"/>, <see cref="ProjectileID.TerraBlade2"/>, <see cref="ProjectileID.TheHorsemansBlade"/>
	/// </summary>
	public const short NightsEdge = 190;
	/// <summary>
	/// Used by: <see cref="ProjectileID.TrueNightsEdge"/>, <see cref="ProjectileID.TerraBlade2Shot"/>
	/// </summary>
	public const short TrueNightsEdge = 191;
	/// <summary>
	/// Used by: <see cref="ProjectileID.JuminoStardropAnimation"/>
	/// </summary>
	public const short JuminoAnimation = 192;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Flames"/>
	/// </summary>
	public const short Flamethrower = 193;
	/// <summary>
	/// Used by: <see cref="ProjectileID.HorsemanPumpkin"/>
	/// </summary>
	public const short HorsemanPumpkin = 194;
	/// <summary>
	/// Used by: <see cref="ProjectileID.JimsDrone"/>
	/// </summary>
	public const short JimsDrone = 195;
}