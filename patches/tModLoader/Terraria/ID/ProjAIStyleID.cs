using ReLogic.Reflection;
using static Terraria.ID.ProjectileID;

namespace Terraria.ID;

public class ProjAIStyleID
{
	public static readonly IdDictionary Search = IdDictionary.Create<ProjAIStyleID, short>();
	/// <summary>
	/// Behavior: Includes Bullets and Lasers<br/><br/>
	/// Used by: <see cref="WoodenArrowFriendly"/>, <see cref="FireArrow"/>, <see cref="UnholyArrow"/>, <see cref="JestersArrow"/>, <see cref="Bullet"/>, <see cref="GreenLaser"/>, <see cref="MeteorShot"/>, <see cref="HarpyFeather"/>, <see cref="HellfireArrow"/>, <see cref="Seed"/>, <see cref="Stinger"/>, <see cref="WoodenArrowHostile"/>, <see cref="FlamingArrow"/>, <see cref="EyeLaser"/>, <see cref="PinkLaser"/>, <see cref="PurpleLaser"/>, <see cref="CrystalBullet"/>, <see cref="HolyArrow"/>, <see cref="PoisonDart"/>, <see cref="DeathLaser"/>, <see cref="CursedArrow"/>, <see cref="CursedBullet"/>, <see cref="BulletSnowman"/>, <see cref="BoneArrow"/>, <see cref="FrostArrow"/>, <see cref="CopperCoin"/>, <see cref="SilverCoin"/>, <see cref="GoldCoin"/>, <see cref="PlatinumCoin"/>, <see cref="FrostburnArrow"/>, <see cref="IceSpike"/>, <see cref="JungleSpike"/>, <see cref="ConfettiGun"/>, <see cref="BulletDeadeye"/>, <see cref="PoisonDartTrap"/>, <see cref="PygmySpear"/>, <see cref="ChlorophyteBullet"/>, <see cref="ChlorophyteArrow"/>, <see cref="BulletHighVelocity"/>, <see cref="Stynger"/>, <see cref="FlowerPowPetal"/>, <see cref="FrostBeam"/>, <see cref="EyeBeam"/>, <see cref="PoisonFang"/>, <see cref="PoisonDartBlowgun"/>, <see cref="Skull"/>, <see cref="SeedPlantera"/>, <see cref="PoisonSeedPlantera"/>, <see cref="IchorArrow"/>, <see cref="IchorBullet"/>, <see cref="VenomArrow"/>, <see cref="VenomBullet"/>, <see cref="PartyBullet"/>, <see cref="NanoBullet"/>, <see cref="ExplosiveBullet"/>, <see cref="GoldenBullet"/>, <see cref="ConfettiMelee"/>, <see cref="Shadowflames"/>, <see cref="SniperBullet"/>, <see cref="CandyCorn"/>, <see cref="JackOLantern"/>, <see cref="Stake"/>, <see cref="FlamingWood"/>, <see cref="PineNeedleFriendly"/>, <see cref="Blizzard"/>, <see cref="NorthPoleSnowflake"/>, <see cref="PineNeedleHostile"/>, <see cref="FrostWave"/>, <see cref="FrostShard"/>, <see cref="Missile"/>, <see cref="VenomFang"/>, <see cref="PulseBolt"/>, <see cref="HornetStinger"/>, <see cref="ImpFireball"/>, <see cref="MiniRetinaLaser"/>, <see cref="MiniSharkron"/>, <see cref="Meteor1"/>, <see cref="Meteor2"/>, <see cref="Meteor3"/>, <see cref="MartianTurretBolt"/>, <see cref="BrainScramblerBolt"/>, <see cref="GigaZapperSpear"/>, <see cref="RayGunnerLaser"/>, <see cref="LaserMachinegunLaser"/>, <see cref="ElectrosphereMissile"/>, <see cref="SaucerLaser"/>, <see cref="ChargedBlasterOrb"/>, <see cref="PhantasmalBolt"/>, <see cref="CultistBossFireBall"/>, <see cref="CultistBossFireBallClone"/>, <see cref="BeeArrow"/>, <see cref="WebSpit"/>, <see cref="BoneArrowFromMerchant"/>, <see cref="CrystalDart"/>, <see cref="CursedDart"/>, <see cref="IchorDart"/>, <see cref="SeedlerThorn"/>, <see cref="Hellwing"/>, <see cref="ShadowFlameArrow"/>, <see cref="ProjectileID.Nail"/>, <see cref="JavelinFriendly"/>, <see cref="JavelinHostile"/>, <see cref="BoneGloveProj"/>, <see cref="SalamanderSpit"/>, <see cref="NebulaLaser"/>, <see cref="VortexLaser"/>, <see cref="VortexAcid"/>, <see cref="ClothiersCurse"/>, <see cref="PainterPaintball"/>, <see cref="MartianWalkerLaser"/>, <see cref="AncientDoomProjectile"/>, <see cref="BlowupSmoke"/>, <see cref="PortalGunBolt"/>, <see cref="SpikedSlimeSpike"/>, <see cref="ProjectileID.ScutlixLaser"/>, <see cref="VortexBeaterRocket"/>, <see cref="BlowupSmokeMoonlord"/>, <see cref="NebulaBlaze1"/>, <see cref="NebulaBlaze2"/>, <see cref="MoonlordBullet"/>, <see cref="MoonlordArrow"/>, <see cref="MoonlordArrowTrail"/>, <see cref="LunarFlare"/>, <see cref="SkyFracture"/>, <see cref="BlackBolt"/>, <see cref="DD2JavelinHostile"/>, <see cref="DD2DrakinShot"/>, <see cref="DD2DarkMageBolt"/>, <see cref="DD2OgreSpit"/>, <see cref="DD2BallistraProj"/>, <see cref="DD2LightningBugZap"/>, <see cref="DD2SquireSonicBoom"/>, <see cref="DD2JavelinHostileT3"/>, <see cref="DD2BetsyFireball"/>, <see cref="DD2PhoenixBowShot"/>, <see cref="MonkStaffT3_AltShot"/>, <see cref="DD2BetsyArrow"/>, <see cref="ApprenticeStaffT3Shot"/>, <see cref="BookStaffShot"/>, <see cref="QueenBeeStinger"/>, <see cref="RollingCactusSpike"/>, <see cref="Geode"/>, <see cref="BloodShot"/>, <see cref="BloodNautilusShot"/>, <see cref="BloodArrow"/>, <see cref="BookOfSkullsSkull"/>, <see cref="ZapinatorLaser"/>, <see cref="QueenSlimeMinionBlueSpike"/>, <see cref="QueenSlimeMinionPinkBall"/>, <see cref="QueenSlimeGelAttack"/>, <see cref="VolatileGelatinBall"/>, <see cref="DeerclopsRangedProjectile"/>, <see cref="VenomDartTrap"/>, <see cref="SilverBullet"/>, <see cref="ShimmerArrow"/>, <see cref="DeadCellsBarrel"/>, <see cref="PoisonDartShotFromSlimes"/>, <see cref="PalworldMinionFoxsparksFireball"/>, <see cref="SoundGun"/>
	/// </summary>
	public const short Arrow = 1;
	/// <summary>
	/// Behavior: Includes Shurikens, Bones, and Knives<br/><br/>
	/// Used by: <see cref="Shuriken"/>, <see cref="Bone"/>, <see cref="ThrowingKnife"/>, <see cref="PoisonedKnife"/>, <see cref="HolyWater"/>, <see cref="UnholyWater"/>, <see cref="MagicDagger"/>, <see cref="CannonballFriendly"/>, <see cref="SnowBallFriendly"/>, <see cref="CannonballHostile"/>, <see cref="StyngerShrapnel"/>, <see cref="PaladinsHammerHostile"/>, <see cref="VampireKnife"/>, <see cref="EatersBite"/>, <see cref="RottenEgg"/>, <see cref="StarAnise"/>, <see cref="OrnamentHostileShrapnel"/>, <see cref="LovePotion"/>, <see cref="FoulPotion"/>, <see cref="SkeletonBone"/>, <see cref="ShadowFlameKnife"/>, <see cref="DrManFlyFlask"/>, <see cref="Spark"/>, <see cref="ToxicFlask"/>, <see cref="FrostDaggerfish"/>, <see cref="NurseSyringeHurt"/>, <see cref="SantaBombs"/>, <see cref="BoneDagger"/>, <see cref="BloodWater"/>, <see cref="Football"/>, <see cref="TreeGlobe"/>, <see cref="WorldGlobe"/>, <see cref="RockGolemRock"/>, <see cref="GelBalloon"/>, <see cref="WandOfSparkingSpark"/>, <see cref="PewMaticHornShot"/>, <see cref="WandOfFrostingFrost"/>, <see cref="MoonGlobe"/>, <see cref="Waffle"/>, <see cref="PrettyMirror"/>
	/// </summary>
	public const short ThrownProjectile = 2;
	/// <summary>
	/// Used by: <see cref="EnchantedBoomerang"/>, <see cref="Flamarang"/>, <see cref="ThornChakram"/>, <see cref="WoodenBoomerang"/>, <see cref="LightDisc"/>, <see cref="IceBoomerang"/>, <see cref="PossessedHatchet"/>, <see cref="Bananarang"/>, <see cref="PaladinsHammerFriendly"/>, <see cref="BloodyMachete"/>, <see cref="FruitcakeChakram"/>, <see cref="Anchor"/>, <see cref="BouncingShield"/>, <see cref="Shroomerang"/>, <see cref="CombatWrench"/>, <see cref="Trimarang"/>, <see cref="Axearang"/>, <see cref="BluePhaseblade"/>, <see cref="RedPhaseblade"/>, <see cref="GreenPhaseblade"/>, <see cref="PurplePhaseblade"/>, <see cref="WhitePhaseblade"/>, <see cref="YellowPhaseblade"/>, <see cref="BluePhasesaber"/>, <see cref="RedPhasesaber"/>, <see cref="GreenPhasesaber"/>, <see cref="PurplePhasesaber"/>, <see cref="WhitePhasesaber"/>, <see cref="YellowPhasesaber"/>, <see cref="OrangePhaseblade"/>, <see cref="OrangePhasesaber"/>, <see cref="Keybrand"/>, <see cref="PinkPhaseblade"/>, <see cref="PinkPhasesaber"/>, <see cref="RainbowPhaseblade"/>, <see cref="RainbowPhasesaber"/>
	/// </summary>
	public const short Boomerang = 3;
	/// <summary>
	/// Used by: <see cref="VilethornBase"/>, <see cref="VilethornTip"/>, <see cref="NettleBurstRight"/>, <see cref="NettleBurstLeft"/>, <see cref="NettleBurstEnd"/>, <see cref="CrystalVileShardHead"/>, <see cref="CrystalVileShardShaft"/>
	/// </summary>
	public const short Vilethorn = 4;
	/// <summary>
	/// Used by: <see cref="Starfury"/>, <see cref="ProjectileID.FallingStar"/>, <see cref="HallowStar"/>, <see cref="StarWrath"/>, <see cref="ManaCloakStar"/>, <see cref="BeeCloakStar"/>, <see cref="StarVeilStar"/>, <see cref="StarCloakStar"/>, <see cref="StarCannonStar"/>, <see cref="MeteorWhipMeteor"/>, <see cref="MeteorStormMeteor"/>
	/// </summary>
	public const short FallingStar = 5;
	/// <summary>
	/// Used by: <see cref="PurificationPowder"/>, <see cref="VilePowder"/>, <see cref="ViciousPowder"/>, <see cref="Fertilizer"/>, <see cref="SuperFertilizer"/>
	/// </summary>
	public const short Powder = 6;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Hook"/>, <see cref="IvyWhip"/>, <see cref="DualHookBlue"/>, <see cref="DualHookRed"/>, <see cref="WebSlingerHook"/>, <see cref="GemHookAmethyst"/>, <see cref="GemHookTopaz"/>, <see cref="GemHookSapphire"/>, <see cref="GemHookEmerald"/>, <see cref="GemHookRuby"/>, <see cref="GemHookDiamond"/>, <see cref="SkeletronHand"/>, <see cref="BatHook"/>, <see cref="WoodHook"/>, <see cref="CandyCaneHook"/>, <see cref="ChristmasHook"/>, <see cref="FishHook"/>, <see cref="SlimeHook"/>, <see cref="TrackHook"/>, <see cref="AntiGravityHook"/>, <see cref="TendonHook"/>, <see cref="ThornHook"/>, <see cref="IlluminantHook"/>, <see cref="WormHook"/>, <see cref="LunarHookSolar"/>, <see cref="LunarHookVortex"/>, <see cref="LunarHookNebula"/>, <see cref="LunarHookStardust"/>, <see cref="StaticHook"/>, <see cref="AmberHook"/>, <see cref="SquirrelHook"/>, <see cref="QueenSlimeHook"/>
	/// </summary>
	public const short Hook = 7;
	/// <summary>
	/// Behavior: Includes the Flower of Fire, Waterbolt, Cursed Flame, and Meowmere projectiles<br/><br/>
	/// Used by: <see cref="BallofFire"/>, <see cref="WaterBolt"/>, <see cref="CursedFlameFriendly"/>, <see cref="CursedFlameHostile"/>, <see cref="BallofFrost"/>, <see cref="Fireball"/>, <see cref="Meowmere"/>
	/// </summary>
	public const short Bounce = 8;
	/// <summary>
	/// Behavior: Includes Flame Lash and Magic Missile<br/><br/>
	/// Used by: <see cref="ProjectileID.MagicMissile"/>, <see cref="Flamelash"/>, <see cref="RainbowRodBullet"/>, <see cref="FlyingKnife"/>
	/// </summary>
	public const short MagicMissile = 9;
	/// <summary>
	/// Falling tiles like Sand spawn falling tile projectiles with this aiStyle. Item drop and placed tile can customized using <see cref="Sets.FallingBlockTileItem" />.<br/><br/>
	/// Used by: <see cref="DirtBall"/>, <see cref="SandBallFalling"/>, <see cref="MudBall"/>, <see cref="AshBallFalling"/>, <see cref="SandBallGun"/>, <see cref="EbonsandBallFalling"/>, <see cref="EbonsandBallGun"/>, <see cref="PearlSandBallFalling"/>, <see cref="PearlSandBallGun"/>, <see cref="SiltBall"/>, <see cref="SnowBallHostile"/>, <see cref="SlushBall"/>, <see cref="CrimsandBallFalling"/>, <see cref="CrimsandBallGun"/>, <see cref="CopperCoinsFalling"/>, <see cref="SilverCoinsFalling"/>, <see cref="GoldCoinsFalling"/>, <see cref="PlatinumCoinsFalling"/>, <see cref="BlueDungeonDebris"/>, <see cref="GreenDungeonDebris"/>, <see cref="PinkDungeonDebris"/>, <see cref="ShellPileFalling"/>, <see cref="MudBallPlayer"/>
	/// </summary>
	public const short FallingTile = 10;
	/// <summary>
	/// Behavior: Includes Shadow Orb and Fairy pets<br/><br/>
	/// Used by: <see cref="ShadowOrb"/>, <see cref="BlueFairy"/>, <see cref="PinkFairy"/>, <see cref="GreenFairy"/>
	/// </summary>
	public const short FloatingFollow = 11;
	/// <summary>
	/// Behavior: Includes Aqua Scepter and Golden Shower projectiles<br/><br/>
	/// Used by: <see cref="WaterStream"/>, <see cref="GoldenShowerFriendly"/>, <see cref="GoldenShowerHostile"/>
	/// </summary>
	public const short Stream = 12;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Harpoon"/>, <see cref="GolemFist"/>, <see cref="BoxingGlove"/>, <see cref="ChainKnife"/>, <see cref="ChainGuillotine"/>
	/// </summary>
	public const short Harpoon = 13;
	/// <summary>
	/// Behavior: Includes most non-destructive Explosive, Glowstick, and Spike Ball projectiles<br/><br/>
	/// Used by: <see cref="SpikyBall"/>, <see cref="Glowstick"/>, <see cref="StickyGlowstick"/>, <see cref="Beenade"/>, <see cref="SpikyBallTrap"/>, <see cref="SmokeBomb"/>, <see cref="BoulderStaffOfEarth"/>, <see cref="ThornBall"/>, <see cref="GreekFire1"/>, <see cref="GreekFire2"/>, <see cref="GreekFire3"/>, <see cref="OrnamentHostile"/>, <see cref="Spike"/>, <see cref="SpiderEgg"/>, <see cref="MolotovFire"/>, <see cref="MolotovFire2"/>, <see cref="MolotovFire3"/>, <see cref="SaucerScrap"/>, <see cref="SpelunkerGlowstick"/>, <see cref="CursedDartFlame"/>, <see cref="SeedlerNut"/>, <see cref="BouncyGlowstick"/>, <see cref="Twinkle"/>, <see cref="FairyGlowstick"/>, <see cref="DripplerFlailExtraBall"/>, <see cref="RainbowGlowstick"/>, <see cref="ObsidianFire"/>, <see cref="ObsidianFire2"/>, <see cref="ObsidianFire3"/>
	/// </summary>
	public const short GroundProjectile = 14;
	/// <summary>
	/// Used by: <see cref="BallOHurt"/>, <see cref="BlueMoon"/>, <see cref="Sunfury"/>, <see cref="TheDaoofPow"/>, <see cref="TheMeatball"/>, <see cref="FlowerPow"/>, <see cref="DripplerFlail"/>, <see cref="Mace"/>, <see cref="FlamingMace"/>, <see cref="FlaironFlail"/>
	/// </summary>
	public const short Flail = 15;
	/// <summary>
	/// Note that projectiles using <see cref="Sets.Explosive" /> will utilize much of the custom logic corresponding to this aiStyle, allowing projectiles to behave like explosives without using this aiStyle directly.<br/><br/>
	/// Used by: <see cref="Bomb"/>, <see cref="Dynamite"/>, <see cref="Grenade"/>, <see cref="StickyBomb"/>, <see cref="HappyBomb"/>, <see cref="BombSkeletronPrime"/>, <see cref="Explosives"/>, <see cref="GrenadeI"/>, <see cref="RocketI"/>, <see cref="ProximityMineI"/>, <see cref="GrenadeII"/>, <see cref="RocketII"/>, <see cref="ProximityMineII"/>, <see cref="GrenadeIII"/>, <see cref="RocketIII"/>, <see cref="ProximityMineIII"/>, <see cref="GrenadeIV"/>, <see cref="RocketIV"/>, <see cref="ProximityMineIV"/>, <see cref="Landmine"/>, <see cref="RocketSkeleton"/>, <see cref="RocketSnowmanI"/>, <see cref="RocketSnowmanII"/>, <see cref="RocketSnowmanIII"/>, <see cref="RocketSnowmanIV"/>, <see cref="StickyGrenade"/>, <see cref="StickyDynamite"/>, <see cref="BouncyBomb"/>, <see cref="BouncyGrenade"/>, <see cref="BombFish"/>, <see cref="PartyGirlGrenade"/>, <see cref="BouncyDynamite"/>, <see cref="DD2GoblinBomb"/>, <see cref="ScarabBomb"/>, <see cref="ClusterRocketI"/>, <see cref="ClusterGrenadeI"/>, <see cref="ClusterMineI"/>, <see cref="ClusterFragmentsI"/>, <see cref="ClusterRocketII"/>, <see cref="ClusterGrenadeII"/>, <see cref="ClusterMineII"/>, <see cref="ClusterFragmentsII"/>, <see cref="WetRocket"/>, <see cref="WetGrenade"/>, <see cref="WetMine"/>, <see cref="LavaRocket"/>, <see cref="LavaGrenade"/>, <see cref="LavaMine"/>, <see cref="HoneyRocket"/>, <see cref="HoneyGrenade"/>, <see cref="HoneyMine"/>, <see cref="MiniNukeRocketI"/>, <see cref="MiniNukeGrenadeI"/>, <see cref="MiniNukeMineI"/>, <see cref="MiniNukeRocketII"/>, <see cref="MiniNukeGrenadeII"/>, <see cref="MiniNukeMineII"/>, <see cref="DryRocket"/>, <see cref="DryGrenade"/>, <see cref="DryMine"/>, <see cref="ClusterSnowmanRocketI"/>, <see cref="ClusterSnowmanRocketII"/>, <see cref="WetSnowmanRocket"/>, <see cref="LavaSnowmanRocket"/>, <see cref="HoneySnowmanRocket"/>, <see cref="MiniNukeSnowmanRocketI"/>, <see cref="MiniNukeSnowmanRocketII"/>, <see cref="DrySnowmanRocket"/>, <see cref="ClusterSnowmanFragmentsI"/>, <see cref="ClusterSnowmanFragmentsII"/>, <see cref="WetBomb"/>, <see cref="LavaBomb"/>, <see cref="HoneyBomb"/>, <see cref="DryBomb"/>, <see cref="DirtBomb"/>, <see cref="DirtStickyBomb"/>, <see cref="SantankMountRocket"/>, <see cref="TNTBarrel"/>, <see cref="FreezeBomb"/>, <see cref="SuperBomb"/>, <see cref="SuperStickyBomb"/>, <see cref="AcornSlingshotAcorn"/>
	/// </summary>
	public const short Explosive = 16;
	/// <summary>
	/// Used by: <see cref="Tombstone"/>, <see cref="ProjectileID.GraveMarker"/>, <see cref="CrossGraveMarker"/>, <see cref="Headstone"/>, <see cref="Gravestone"/>, <see cref="Obelisk"/>, <see cref="RichGravestone1"/>, <see cref="RichGravestone2"/>, <see cref="RichGravestone3"/>, <see cref="RichGravestone4"/>, <see cref="RichGravestone5"/>
	/// </summary>
	public const short GraveMarker = 17;
	/// <summary>
	/// Used by: <see cref="DemonSickle"/>, <see cref="DemonScythe"/>, <see cref="IceSickle"/>, <see cref="DeathSickle"/>, <see cref="LibrarianSkeletonBook"/>
	/// </summary>
	public const short Sickle = 18;
	/// <summary>
	/// Used by: <see cref="DarkLance"/>, <see cref="Trident"/>, <see cref="ProjectileID.Spear"/>, <see cref="MythrilHalberd"/>, <see cref="AdamantiteGlaive"/>, <see cref="CobaltNaginata"/>, <see cref="Gungnir"/>, <see cref="MushroomSpear"/>, <see cref="TheRottedFork"/>, <see cref="PalladiumPike"/>, <see cref="OrichalcumHalberd"/>, <see cref="TitaniumTrident"/>, <see cref="ChlorophytePartisan"/>, <see cref="NorthPoleWeapon"/>, <see cref="ObsidianSwordfish"/>, <see cref="Swordfish"/>, <see cref="ThunderSpear"/>, <see cref="JoustingLance"/>, <see cref="ShadowJoustingLance"/>, <see cref="HallowJoustingLance"/>, <see cref="SlimeSpear"/>
	/// </summary>
	public const short Spear = 19;
	/// <summary>
	/// Behavior: Includes Saws<br/><br/>
	/// Used by: <see cref="CobaltChainsaw"/>, <see cref="MythrilChainsaw"/>, <see cref="CobaltDrill"/>, <see cref="MythrilDrill"/>, <see cref="AdamantiteChainsaw"/>, <see cref="AdamantiteDrill"/>, <see cref="Hamdrax"/>, <see cref="PalladiumDrill"/>, <see cref="PalladiumChainsaw"/>, <see cref="OrichalcumDrill"/>, <see cref="OrichalcumChainsaw"/>, <see cref="TitaniumDrill"/>, <see cref="TitaniumChainsaw"/>, <see cref="ChlorophyteDrill"/>, <see cref="ChlorophyteChainsaw"/>, <see cref="ChlorophyteJackhammer"/>, <see cref="SawtoothShark"/>, <see cref="VortexChainsaw"/>, <see cref="VortexDrill"/>, <see cref="NebulaChainsaw"/>, <see cref="NebulaDrill"/>, <see cref="SolarFlareChainsaw"/>, <see cref="SolarFlareDrill"/>, <see cref="ButchersChainsaw"/>, <see cref="StardustDrill"/>, <see cref="StardustChainsaw"/>
	/// </summary>
	public const short Drill = 20;
	/// <summary>
	/// Used by: <see cref="QuarterNote"/>, <see cref="EighthNote"/>, <see cref="TiedEighthNote"/>
	/// </summary>
	public const short MusicNote = 21;
	/// <summary>
	/// Used by: <see cref="IceBlock"/>
	/// </summary>
	public const short IceRod = 22;
	/// <summary>
	/// Behavior: Includes Cursed Flames and Eye Fire<br/><br/>
	/// Used by: <see cref="EyeFire"/>, <see cref="FlamesTrap"/>
	/// </summary>
	public const short Flames = 23;
	/// <summary>
	/// Used by: <see cref="ProjectileID.CrystalShard"/>, <see cref="CrystalStorm"/>
	/// </summary>
	public const short CrystalShard = 24;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Boulder"/>, <see cref="BeeHive"/>, <see cref="RollingCactus"/>, <see cref="MiniBoulder"/>, <see cref="BouncyBoulder"/>, <see cref="LifeCrystalBoulder"/>, <see cref="MoonBoulder"/>, <see cref="RainbowBoulder"/>, <see cref="Poulder"/>, <see cref="LavaBoulder"/>, <see cref="SpiderBoulder"/>, <see cref="Ghoulder"/>, <see cref="BoulderThatSpawnsPet"/>
	/// </summary>
	public const short Boulder = 25;
	/// <summary>
	/// Behavior: Includes some minions with simple AI, such as the Baby Slime<br/><br/>
	/// Used by: <see cref="Bunny"/>, <see cref="Penguin"/>, <see cref="Turtle"/>, <see cref="BabyEater"/>, <see cref="Pygmy"/>, <see cref="Pygmy2"/>, <see cref="Pygmy3"/>, <see cref="Pygmy4"/>, <see cref="BabySkeletronHead"/>, <see cref="BabyHornet"/>, <see cref="TikiSpirit"/>, <see cref="PetLizard"/>, <see cref="Parrot"/>, <see cref="Truffle"/>, <see cref="Sapling"/>, <see cref="Wisp"/>, <see cref="BabyDino"/>, <see cref="BabySlime"/>, <see cref="EyeSpring"/>, <see cref="BabySnowman"/>, <see cref="Spider"/>, <see cref="Squashling"/>, <see cref="BlackCat"/>, <see cref="CursedSapling"/>, <see cref="Puppy"/>, <see cref="BabyGrinch"/>, <see cref="ZephyrFish"/>, <see cref="VenomSpider"/>, <see cref="JumperSpider"/>, <see cref="DangerousSpider"/>, <see cref="MiniMinotaur"/>, <see cref="BabyFaceMonster"/>, <see cref="SugarGlider"/>, <see cref="SharkPup"/>, <see cref="LilHarpy"/>, <see cref="FennecFox"/>, <see cref="GlitteryButterfly"/>, <see cref="BabyImp"/>, <see cref="BabyRedPanda"/>, <see cref="Plantero"/>, <see cref="DynamiteKitten"/>, <see cref="BabyWerewolf"/>, <see cref="ShadowMimic"/>, <see cref="VoltBunny"/>, <see cref="KingSlimePet"/>, <see cref="BrainOfCthulhuPet"/>, <see cref="SkeletronPet"/>, <see cref="QueenBeePet"/>, <see cref="SkeletronPrimePet"/>, <see cref="PlanteraPet"/>, <see cref="GolemPet"/>, <see cref="DukeFishronPet"/>, <see cref="MoonLordPet"/>, <see cref="EverscreamPet"/>, <see cref="MartianPet"/>, <see cref="DD2OgrePet"/>, <see cref="DD2BetsyPet"/>, <see cref="QueenSlimePet"/>, <see cref="BerniePet"/>, <see cref="DeerclopsPet"/>, <see cref="PigPet"/>, <see cref="ChesterPet"/>, <see cref="JunimoPet"/>, <see cref="BlueChickenPet"/>, <see cref="Spiffo"/>, <see cref="CavelingGardener"/>, <see cref="DeadCellsSwarmBiter"/>, <see cref="Pufferfish"/>, <see cref="PalworldMinionFoxsparks"/>, <see cref="PalworldPetChillet"/>, <see cref="PalworldPetChilletIgnis"/>
	/// </summary>
	public const short Pet = 26;
	/// <summary>
	/// Used by: <see cref="UnholyTridentFriendly"/>, <see cref="UnholyTridentHostile"/>, <see cref="SwordBeam"/>, <see cref="TerraBeam"/>, <see cref="LightBeam"/>, <see cref="NightBeam"/>, <see cref="EnchantedBeam"/>
	/// </summary>
	public const short Beam = 27;
	/// <summary>
	/// Behavior: Includes Ice Sword, Frost Hydra, Frost Bolt, and Icy Spit projectiles<br/><br/>
	/// Used by: <see cref="IceBolt"/>, <see cref="FrostBoltSword"/>, <see cref="FrostBlastHostile"/>, <see cref="RuneBlast"/>, <see cref="IcewaterSpit"/>, <see cref="FrostBlastFriendly"/>, <see cref="FrostBoltStaff"/>, <see cref="HoundiusShootiusFireball"/>, <see cref="DeadCellsBarnacleShot"/>
	/// </summary>
	public const short ColdBolt = 28;
	/// <summary>
	/// Used by: <see cref="AmethystBolt"/>, <see cref="TopazBolt"/>, <see cref="SapphireBolt"/>, <see cref="EmeraldBolt"/>, <see cref="RubyBolt"/>, <see cref="DiamondBolt"/>, <see cref="CrystalPulse"/>, <see cref="CrystalPulse2"/>, <see cref="AmberBolt"/>, <see cref="NebulaArcanumExplosionShot"/>, <see cref="NebulaArcanumExplosionShotShard"/>, <see cref="ThunderStaffShot"/>
	/// </summary>
	public const short GemStaffBolt = 29;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Mushroom"/>, <see cref="OrnamentFriendly"/>, <see cref="OrnamentStar"/>
	/// </summary>
	public const short Mushroom = 30;
	/// <summary>
	/// Used by: <see cref="PureSpray"/>, <see cref="HallowSpray"/>, <see cref="CorruptSpray"/>, <see cref="MushroomSpray"/>, <see cref="CrimsonSpray"/>, <see cref="SandSpray"/>, <see cref="SnowSpray"/>, <see cref="DirtSpray"/>
	/// </summary>
	public const short Spray = 31;
	/// <summary>
	/// Used by: <see cref="ProjectileID.BeachBall"/>
	/// </summary>
	public const short BeachBall = 32;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Flare"/>, <see cref="BlueFlare"/>, <see cref="SpelunkerFlare"/>, <see cref="CursedFlare"/>, <see cref="RainbowFlare"/>, <see cref="ShimmerFlare"/>
	/// </summary>
	public const short Flare = 33;
	/// <summary>
	/// Used by: <see cref="RocketFireworkRed"/>, <see cref="RocketFireworkGreen"/>, <see cref="RocketFireworkBlue"/>, <see cref="RocketFireworkYellow"/>, <see cref="RocketFireworksBoxRed"/>, <see cref="RocketFireworksBoxGreen"/>, <see cref="RocketFireworksBoxBlue"/>, <see cref="RocketFireworksBoxYellow"/>
	/// </summary>
	public const short FireWork = 34;
	/// <summary>
	/// Used by: <see cref="ProjectileID.RopeCoil"/>, <see cref="VineRopeCoil"/>, <see cref="SilkRopeCoil"/>, <see cref="WebRopeCoil"/>
	/// </summary>
	public const short RopeCoil = 35;
	/// <summary>
	/// Behavior: Includes Bee, Wasp, Tiny Eater, and Bat projectiles<br/><br/>
	/// Used by: <see cref="Bee"/>, <see cref="Wasp"/>, <see cref="TinyEater"/>, <see cref="Bat"/>, <see cref="GiantBee"/>
	/// </summary>
	public const short SmallFlying = 36;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SpearTrap"/>
	/// </summary>
	public const short SpearTrap = 37;
	/// <summary>
	/// Used by: <see cref="FlamethrowerTrap"/>
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
	/// Used by: <see cref="SporeCloud"/>, <see cref="ChlorophyteOrb"/>, <see cref="ThunderSpearShot"/>
	/// </summary>
	public const short MoveShort = 44;
	/// <summary>
	/// Used by: <see cref="RainCloudMoving"/>, <see cref="RainCloudRaining"/>, <see cref="RainFriendly"/>, <see cref="BloodCloudMoving"/>, <see cref="BloodCloudRaining"/>, <see cref="BloodRain"/>, <see cref="RainNimbus"/>
	/// </summary>
	public const short RainCloud = 45;
	/// <summary>
	/// Used by: <see cref="RainbowFront"/>, <see cref="RainbowBack"/>
	/// </summary>
	public const short Rainbow = 46;
	/// <summary>
	/// Used by: <see cref="MagnetSphereBall"/>
	/// </summary>
	public const short MagnetSphere = 47;
	/// <summary>
	/// Used by: <see cref="MagnetSphereBolt"/>, <see cref="HeatRay"/>, <see cref="ShadowBeamHostile"/>, <see cref="ShadowBeamFriendly"/>, <see cref="UFOLaser"/>
	/// </summary>
	public const short Ray = 48;
	/// <summary>
	/// Used by: <see cref="ProjectileID.ExplosiveBunny"/>
	/// </summary>
	public const short ExplosiveBunny = 49;
	/// <summary>
	/// Used by: <see cref="InfernoHostileBolt"/>, <see cref="InfernoHostileBlast"/>, <see cref="InfernoFriendlyBolt"/>, <see cref="InfernoFriendlyBlast"/>
	/// </summary>
	public const short Inferno = 50;
	/// <summary>
	/// Used by: <see cref="LostSoulHostile"/>, <see cref="LostSoulFriendly"/>
	/// </summary>
	public const short LostSoul = 51;
	/// <summary>
	/// Behavior: Includes Spirit Heal from the Spectre Hood and Vampire Heal from the Vampire Knives<br/><br/>
	/// Used by: <see cref="SpiritHeal"/>, <see cref="VampireHeal"/>
	/// </summary>
	public const short Heal = 52;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FrostHydra"/>, <see cref="SpiderHiver"/>, <see cref="HoundiusShootius"/>
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
	/// Used by: <see cref="WaterGun"/>, <see cref="SlimeGun"/>, <see cref="ShimmerGunStream"/>
	/// </summary>
	public const short WaterJet = 60;
	/// <summary>
	/// Used by: <see cref="BobberWooden"/>, <see cref="BobberReinforced"/>, <see cref="BobberFiberglass"/>, <see cref="BobberFisherOfSouls"/>, <see cref="BobberGolden"/>, <see cref="BobberMechanics"/>, <see cref="BobbersittingDuck"/>, <see cref="BobberFleshcatcher"/>, <see cref="BobberHotline"/>, <see cref="BobberBloody"/>, <see cref="BobberScarab"/>, <see cref="FishingBobber"/>, <see cref="FishingBobberGlowingStar"/>, <see cref="FishingBobberGlowingLava"/>, <see cref="FishingBobberGlowingKrypton"/>, <see cref="FishingBobberGlowingXenon"/>, <see cref="FishingBobberGlowingArgon"/>, <see cref="FishingBobberGlowingViolet"/>, <see cref="FishingBobberGlowingRainbow"/>
	/// </summary>
	public const short Bobber = 61;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Hornet"/>, <see cref="FlyingImp"/>, <see cref="Tempest"/>, <see cref="UFOMinion"/>, <see cref="StardustCellMinion"/>, <see cref="AbigailMinion"/>
	/// </summary>
	public const short Hornet = 62;
	/// <summary>
	/// Used by: <see cref="ProjectileID.BabySpider"/>
	/// </summary>
	public const short BabySpider = 63;
	/// <summary>
	/// Behavior: Includes Sharknado and Cthulunado projectiles<br/><br/>
	/// Used by: <see cref="Sharknado"/>, <see cref="Cthulunado"/>
	/// </summary>
	public const short Nado = 64;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SharknadoBolt"/>
	/// </summary>
	public const short SharknadoBolt = 65;
	/// <summary>
	/// Used by: <see cref="Retanimini"/>, <see cref="Spazmamini"/>, <see cref="DeadlySphere"/>
	/// </summary>
	public const short MiniTwins = 66;
	/// <summary>
	/// Behavior: Includes Mini Pirate, Crimson Heart, Companion Cube, Vampire Frog, and Desert Tiger projectiles<br/><br/>
	/// Used by: <see cref="OneEyedPirate"/>, <see cref="SoulscourgePirate"/>, <see cref="PirateCaptain"/>, <see cref="CrimsonHeart"/>, <see cref="CompanionCube"/>, <see cref="VampireFrog"/>, <see cref="StormTigerTier1"/>, <see cref="StormTigerTier2"/>, <see cref="StormTigerTier3"/>, <see cref="FlinxMinion"/>, <see cref="DirtiestBlock"/>, <see cref="DeadCellsMushroomBoiMinion"/>, <see cref="CobWhipSpider"/>, <see cref="BoulderPet"/>, <see cref="RainbowBoulderPet"/>, <see cref="PalworldMinionCattiva"/>
	/// </summary>
	public const short CommonFollow = 67;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MolotovCocktail"/>, <see cref="Ale"/>
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
	/// Used by: <see cref="FireworkFountainYellow"/>, <see cref="FireworkFountainRed"/>, <see cref="FireworkFountainBlue"/>, <see cref="FireworkFountainRainbow"/>
	/// </summary>
	public const short FireWorkFountain = 73;
	/// <summary>
	/// Used by: <see cref="ScutlixLaserFriendly"/>
	/// </summary>
	public const short ScutlixLaser = 74;
	/// <summary>
	/// Used by: <see cref="LaserMachinegun"/>, <see cref="LaserDrill"/>, <see cref="ChargedBlasterCannon"/>, <see cref="Arkhalis"/>, <see cref="PortalGun"/>, <see cref="SolarWhipSword"/>, <see cref="VortexBeater"/>, <see cref="Phantasm"/>, <see cref="LastPrism"/>, <see cref="DD2PhoenixBow"/>, <see cref="Celeb2Weapon"/>, <see cref="Terragrim"/>, <see cref="PiercingStarlight"/>
	/// </summary>
	public const short HeldProjectile = 75;
	/// <summary>
	/// Used by: <see cref="ScutlixLaserCrosshair"/>, <see cref="DrillMountCrosshair"/>
	/// </summary>
	public const short Crosshair = 76;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Electrosphere"/>, <see cref="SkyDragonsFuryElectrosphere"/>
	/// </summary>
	public const short Electrosphere = 77;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Xenopopper"/>
	/// </summary>
	public const short Xenopopper = 78;
	/// <summary>
	/// Used by: <see cref="SaucerDeathray"/>
	/// </summary>
	public const short MartianDeathRay = 79;
	/// <summary>
	/// Used by: <see cref="SaucerMissile"/>
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
	/// Used by: <see cref="PhantasmalDeathray"/>, <see cref="ChargedBlasterLaser"/>, <see cref="StardustSoldierLaser"/>, <see cref="LastPrismLaser"/>, <see cref="MoonlordTurretLaser"/>
	/// </summary>
	public const short ThickLaser = 84;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MoonLeech"/>
	/// </summary>
	public const short MoonLeech = 85;
	/// <summary>
	/// Used by: <see cref="CultistBossIceMist"/>
	/// </summary>
	public const short IceMist = 86;
	/// <summary>
	/// Used by: <see cref="ClingerStaff"/>
	/// </summary>
	public const short CursedFlameWall = 87;
	/// <summary>
	/// Used by: <see cref="CultistBossLightningOrb"/>, <see cref="CultistBossLightningOrbArc"/>, <see cref="VortexLightning"/>
	/// </summary>
	public const short LightningOrb = 88;
	/// <summary>
	/// Used by: <see cref="CultistRitual"/>
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
	/// Used by: <see cref="ProjectileID.ToxicCloud"/>, <see cref="ToxicCloud2"/>, <see cref="ToxicCloud3"/>, <see cref="GasTrap"/>
	/// </summary>
	public const short ToxicCloud = 92;
	/// <summary>
	/// Used by: <see cref="NailFriendly"/>
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
	/// Used by: <see cref="CultistBossParticle"/>
	/// </summary>
	public const short MysteriousTablet = 98;
	/// <summary>
	/// Used by: <see cref="Code1"/>, <see cref="WoodYoyo"/>, <see cref="CorruptYoyo"/>, <see cref="CrimsonYoyo"/>, <see cref="JungleYoyo"/>, <see cref="Cascade"/>, <see cref="Chik"/>, <see cref="Code2"/>, <see cref="Rally"/>, <see cref="Yelets"/>, <see cref="RedsYoyo"/>, <see cref="ValkyrieYoyo"/>, <see cref="Amarok"/>, <see cref="HelFire"/>, <see cref="Kraken"/>, <see cref="TheEyeOfCthulhu"/>, <see cref="BlackCounterweight"/>, <see cref="BlueCounterweight"/>, <see cref="GreenCounterweight"/>, <see cref="PurpleCounterweight"/>, <see cref="RedCounterweight"/>, <see cref="YellowCounterweight"/>, <see cref="FormatC"/>, <see cref="Gradient"/>, <see cref="Valor"/>, <see cref="Terrarian"/>, <see cref="HiveFive"/>, <see cref="PinkCounterweight"/>
	/// </summary>
	public const short Yoyo = 99;
	/// <summary>
	/// Used by: <see cref="MedusaHead"/>
	/// </summary>
	public const short MedusaRay = 100;
	/// <summary>
	/// Behavior: Includes Medusa Head Ray and Mechanical Cart Laser projectiles<br/><br/>
	/// Used by: <see cref="MedusaHeadRay"/>, <see cref="MinecartMechLaser"/>
	/// </summary>
	public const short HorizontalRay = 101;
	/// <summary>
	/// Behavior: Includes Flow Invader, Nebular Piercer, and Nebula Eye projectiles<br/><br/>
	/// Used by: <see cref="StardustJellyfishSmall"/>, <see cref="NebulaBolt"/>, <see cref="NebulaEye"/>
	/// </summary>
	public const short LunarProjectile = 102;
	/// <summary>
	/// Used by: <see cref="StardustTowerMark"/>
	/// </summary>
	public const short Starmark = 103;
	/// <summary>
	/// Used by: <see cref="BrainOfConfusion"/>
	/// </summary>
	public const short BrainofConfusion = 104;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SporeTrap"/>, <see cref="SporeTrap2"/>
	/// </summary>
	public const short SporeTrap = 105;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SporeGas"/>, <see cref="SporeGas2"/>, <see cref="SporeGas3"/>
	/// </summary>
	public const short SporeGas = 106;
	/// <summary>
	/// Behavior: Includes Desert Sprit's Curse<br/><br/>
	/// Used by: <see cref="ProjectileID.NebulaSphere"/>, <see cref="DesertDjinnCurse"/>
	/// </summary>
	public const short NebulaSphere = 107;
	/// <summary>
	/// Behavior: Includes Blood Tears<br/><br/>
	/// Used by: <see cref="VortexVortexLightning"/>, <see cref="VortexVortexPortal"/>, <see cref="BloodNautilusTears"/>
	/// </summary>
	public const short Vortex = 108;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MechanicWrench"/>
	/// </summary>
	public const short MechanicWrench = 109;
	/// <summary>
	/// Used by: <see cref="NurseSyringeHeal"/>
	/// </summary>
	public const short NurseSyringe = 110;
	/// <summary>
	/// Used by: <see cref="DryadsWardCircle"/>
	/// </summary>
	public const short DryadWard = 111;
	/// <summary>
	/// Behavior: Includes Truffle Spore, Rainbow Crystal Explosion, and Dandelion Seed projectiles<br/><br/>
	/// Used by: <see cref="TruffleSpore"/>, <see cref="RainbowCrystalExplosion"/>, <see cref="DandelionSeed"/>
	/// </summary>
	public const short SmallProximityExplosion = 112;
	/// <summary>
	/// Behavior: Includes Bone Javelin, Stardust Cell Shot, and Daybreak projectiles<br/><br/>
	/// Used by: <see cref="BoneJavelin"/>, <see cref="StardustCellMinionShot"/>, <see cref="Daybreak"/>, <see cref="TentacleSpike"/>, <see cref="BloodButcherer"/>, <see cref="DeadCellsKillingDeckCard"/>
	/// </summary>
	public const short StickProjectile = 113;
	/// <summary>
	/// Used by: <see cref="PortalGunGate"/>
	/// </summary>
	public const short PortalGate = 114;
	/// <summary>
	/// Used by: <see cref="ProjectileID.TerrarianBeam"/>
	/// </summary>
	public const short TerrarianBeam = 115;
	/// <summary>
	/// Used by: <see cref="SolarFlareRay"/>
	/// </summary>
	public const short DrakomiteFlare = 116;
	/// <summary>
	/// Behavior: Includes Solar Radience and Solar Eruption Explosion projectiles<br/><br/>
	/// Used by: <see cref="SolarCounter"/>, <see cref="SolarWhipSwordExplosion"/>, <see cref="StardustGuardianExplosion"/>, <see cref="DaybreakExplosion"/>, <see cref="DeadCellsMushroomBoiMinionExplosion"/>
	/// </summary>
	public const short SolarEffect = 117;
	/// <summary>
	/// Used by: <see cref="ProjectileID.NebulaArcanum"/>
	/// </summary>
	public const short NebulaArcanum = 118;
	/// <summary>
	/// Used by: <see cref="NebulaArcanumSubshot"/>
	/// </summary>
	public const short ArcanumSubShot = 119;
	/// <summary>
	/// Used by: <see cref="ProjectileID.StardustGuardian"/>
	/// </summary>
	public const short StardustGuardian = 120;
	/// <summary>
	/// Used by: <see cref="StardustDragon1"/>, <see cref="StardustDragon2"/>, <see cref="StardustDragon3"/>, <see cref="StardustDragon4"/>
	/// </summary>
	public const short StardustDragon = 121;
	/// <summary>
	/// Behavior: The effect displayed when killing a Lunar Event enemy while it's respective Celestial Pillar is alive, also used by Phantasm Arrow<br/><br/>
	/// Used by: <see cref="TowerDamageBolt"/>, <see cref="PhantasmArrow"/>
	/// </summary>
	public const short ReleasedEnergy = 122;
	/// <summary>
	/// Used by: <see cref="MoonlordTurret"/>, <see cref="RainbowCrystal"/>
	/// </summary>
	public const short LunarSentry = 123;
	/// <summary>
	/// Behavior: Includes Suspicious Looking Tentacle, Suspicious Eye, Rez and Spaz, Fairy Princess, Jack 'O Lantern, and Ice Queen pets<br/><br/>
	/// Used by: <see cref="SuspiciousTentacle"/>, <see cref="EyeOfCthulhuPet"/>, <see cref="TwinsPet"/>, <see cref="FairyQueenPet"/>, <see cref="PumpkingPet"/>, <see cref="IceQueenPet"/>, <see cref="GlommerPet"/>
	/// </summary>
	public const short FloatInFrontPet = 124;
	/// <summary>
	/// Used by: <see cref="ProjectileID.WireKite"/>
	/// </summary>
	public const short WireKite = 125;
	/// <summary>
	/// Used by: <see cref="GeyserTrap"/>, <see cref="DD2OgreStomp"/>
	/// </summary>
	public const short Geyser = 126;
	/// <summary>
	/// Used by: <see cref="SandnadoFriendly"/>, <see cref="SandnadoHostile"/>
	/// </summary>
	public const short AncientStorm = 127;
	/// <summary>
	/// Used by: <see cref="SandnadoHostileMark"/>
	/// </summary>
	public const short AncientStormMark = 128;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SpiritFlame"/>
	/// </summary>
	public const short SpiritFlame = 129;
	/// <summary>
	/// Used by: <see cref="DD2FlameBurstTowerT1"/>, <see cref="DD2FlameBurstTowerT2"/>, <see cref="DD2FlameBurstTowerT3"/>
	/// </summary>
	public const short DD2FlameBurst = 130;
	/// <summary>
	/// Used by: <see cref="DD2FlameBurstTowerT1Shot"/>, <see cref="DD2FlameBurstTowerT2Shot"/>, <see cref="DD2FlameBurstTowerT3Shot"/>
	/// </summary>
	public const short DD2FlameBurstShot = 131;
	/// <summary>
	/// Behavior: Eternia Crystal destroyed<br/><br/>
	/// Used by: <see cref="DD2ElderWins"/>
	/// </summary>
	public const short DD2GrimEnd = 132;
	/// <summary>
	/// Used by: <see cref="DD2DarkMageRaise"/>, <see cref="DD2DarkMageHeal"/>
	/// </summary>
	public const short DD2DarkSigil = 133;
	/// <summary>
	/// Used by: <see cref="DD2BallistraTowerT1"/>, <see cref="DD2BallistraTowerT2"/>, <see cref="DD2BallistraTowerT3"/>
	/// </summary>
	public const short DD2Ballista = 134;
	/// <summary>
	/// Behavior: Includes Ogre's Stomp and Geyser projectiles<br/><br/>
	/// Used by: <see cref="DD2OgreSmash"/>, <see cref="QueenSlimeSmash"/>
	/// </summary>
	public const short UpwardExpand = 135;
	/// <summary>
	/// Used by: <see cref="DD2BetsyFlameBreath"/>
	/// </summary>
	public const short DD2BetsysBreath = 136;
	/// <summary>
	/// Used by: <see cref="DD2LightningAuraT1"/>, <see cref="DD2LightningAuraT2"/>, <see cref="DD2LightningAuraT3"/>
	/// </summary>
	public const short DD2LightningAura = 137;
	/// <summary>
	/// Used by: <see cref="DD2ExplosiveTrapT1"/>, <see cref="DD2ExplosiveTrapT2"/>, <see cref="DD2ExplosiveTrapT3"/>
	/// </summary>
	public const short DD2ExplosiveTrap = 138;
	/// <summary>
	/// Used by: <see cref="DD2ExplosiveTrapT1Explosion"/>, <see cref="DD2ExplosiveTrapT2Explosion"/>, <see cref="DD2ExplosiveTrapT3Explosion"/>
	/// </summary>
	public const short DD2ExplosiveTrapExplosion = 139;
	/// <summary>
	/// Used by: <see cref="MonkStaffT1"/>, <see cref="MonkStaffT3"/>
	/// </summary>
	public const short SleepyOctopod = 140;
	/// <summary>
	/// Behavior: The effect created on use of the Sleepy Octopod<br/><br/>
	/// Used by: <see cref="MonkStaffT1Explosion"/>
	/// </summary>
	public const short PoleSmash = 141;
	/// <summary>
	/// Behavior: Use style of the Ghastly Glaive and Sky Dragon's Fury alt1<br/><br/>
	/// Used by: <see cref="MonkStaffT2"/>, <see cref="MonkStaffT3_Alt"/>
	/// </summary>
	public const short ForwardStab = 142;
	/// <summary>
	/// Used by: <see cref="MonkStaffT2Ghast"/>
	/// </summary>
	public const short Ghast = 143;
	/// <summary>
	/// Behavior: Includes the Hoardragon, Flickerwick, Estee, and Propeller Gato<br/><br/>
	/// Used by: <see cref="DD2PetDragon"/>, <see cref="DD2PetGhost"/>, <see cref="DD2PetGato"/>, <see cref="UpbeatStar"/>, <see cref="AxeFairyPet"/>
	/// </summary>
	public const short FloatBehindPet = 144;
	/// <summary>
	/// Used by: <see cref="DD2ApprenticeStorm"/>
	/// </summary>
	public const short WisdomWhirlwind = 145;
	/// <summary>
	/// Behavior: Old One's Army defeated<br/><br/>
	/// Used by: <see cref="DD2Win"/>
	/// </summary>
	public const short DD2Victory = 146;
	/// <summary>
	/// Used by: <see cref="Celeb2Rocket"/>, <see cref="Celeb2RocketExplosive"/>, <see cref="Celeb2RocketLarge"/>, <see cref="Celeb2RocketExplosiveLarge"/>
	/// </summary>
	public const short CelebrationMk2Shots = 147;
	/// <summary>
	/// Used by: <see cref="FallingStarSpawner"/>
	/// </summary>
	public const short FallingStarAnimation = 148;
	/// <summary>
	/// Used by: <see cref="DirtGolfBall"/>, <see cref="GolfBallDyedBlack"/>, <see cref="GolfBallDyedBlue"/>, <see cref="GolfBallDyedBrown"/>, <see cref="GolfBallDyedCyan"/>, <see cref="GolfBallDyedGreen"/>, <see cref="GolfBallDyedLimeGreen"/>, <see cref="GolfBallDyedOrange"/>, <see cref="GolfBallDyedPink"/>, <see cref="GolfBallDyedPurple"/>, <see cref="GolfBallDyedRed"/>, <see cref="GolfBallDyedSkyBlue"/>, <see cref="GolfBallDyedTeal"/>, <see cref="GolfBallDyedViolet"/>, <see cref="GolfBallDyedYellow"/>
	/// </summary>
	public const short GolfBall = 149;
	/// <summary>
	/// Used by: <see cref="GolfClubHelper"/>
	/// </summary>
	public const short GolfClub = 150;
	/// <summary>
	/// Used by: <see cref="ProjectileID.SuperStar"/>
	/// </summary>
	public const short SuperStar = 151;
	/// <summary>
	/// Used by: <see cref="SuperStarSlash"/>, <see cref="BladeOfGrass"/>, <see cref="Muramasa"/>, <see cref="MoonLordWhipProc"/>
	/// </summary>
	public const short SuperStarBeam = 152;
	/// <summary>
	/// Used by: <see cref="ProjectileID.ToiletEffect"/>
	/// </summary>
	public const short ToiletEffect = 153;
	/// <summary>
	/// Used by: <see cref="VoidLens"/>
	/// </summary>
	public const short VoidBag = 154;
	/// <summary>
	/// Used by: <see cref="MysticSnakeCoil"/>
	/// </summary>
	public const short SnakeCoil = 155;
	/// <summary>
	/// Behavior: Includes the Sanguine Bat<br/><br/>
	/// Used by: <see cref="BatOfLight"/>, <see cref="EmpressBlade"/>
	/// </summary>
	public const short Terraprisma = 156;
	/// <summary>
	/// Used by: <see cref="SharpTears"/>, <see cref="DeerclopsIceSpike"/>, <see cref="DeadCellsFlintShot"/>
	/// </summary>
	public const short BloodThorn = 157;
	/// <summary>
	/// Used by: <see cref="BabyBird"/>
	/// </summary>
	public const short Finch = 158;
	/// <summary>
	/// Used by: <see cref="PaperAirplaneA"/>, <see cref="PaperAirplaneB"/>
	/// </summary>
	public const short PaperPlane = 159;
	/// <summary>
	/// Used by: <see cref="KiteBlue"/>, <see cref="KiteBlueAndYellow"/>, <see cref="KiteRed"/>, <see cref="KiteRedAndYellow"/>, <see cref="KiteYellow"/>, <see cref="KiteWyvern"/>, <see cref="KiteBoneSerpent"/>, <see cref="KiteWorldFeeder"/>, <see cref="KiteBunny"/>, <see cref="KitePigron"/>, <see cref="KiteManEater"/>, <see cref="KiteJellyfishBlue"/>, <see cref="KiteJellyfishPink"/>, <see cref="KiteShark"/>, <see cref="KiteSandShark"/>, <see cref="KiteBunnyCorrupt"/>, <see cref="KiteBunnyCrimson"/>, <see cref="KiteGoldfish"/>, <see cref="KiteAngryTrapper"/>, <see cref="KiteKoi"/>, <see cref="KiteCrawltipede"/>, <see cref="KiteSpectrum"/>, <see cref="KiteWanderingEye"/>, <see cref="KiteUnicorn"/>
	/// </summary>
	public const short Kite = 160;
	/// <summary>
	/// Used by: <see cref="GladiusStab"/>, <see cref="RulerStab"/>, <see cref="CopperShortswordStab"/>, <see cref="TinShortswordStab"/>, <see cref="IronShortswordStab"/>, <see cref="LeadShortswordStab"/>, <see cref="SilverShortswordStab"/>, <see cref="TungstenShortswordStab"/>, <see cref="GoldShortswordStab"/>, <see cref="PlatinumShortswordStab"/>
	/// </summary>
	public const short ShortSword = 161;
	/// <summary>
	/// Used by: <see cref="WhiteTigerPounce"/>
	/// </summary>
	public const short DesertTiger = 162;
	/// <summary>
	/// Used by: <see cref="ChumBucket"/>
	/// </summary>
	public const short Chum = 163;
	/// <summary>
	/// Used by: <see cref="StormTigerGem"/>, <see cref="AbigailCounter"/>
	/// </summary>
	public const short DesertTigerBall = 164;
	/// <summary>
	/// Used by: <see cref="BlandWhip"/>, <see cref="SwordWhip"/>, <see cref="MaceWhip"/>, <see cref="ScytheWhip"/>, <see cref="CoolWhip"/>, <see cref="FireWhip"/>, <see cref="ThornWhip"/>, <see cref="RainbowWhip"/>, <see cref="BoneWhip"/>, <see cref="CobWhip"/>, <see cref="CorruptWhip"/>, <see cref="CrimsonWhip"/>, <see cref="MeteorWhip"/>, <see cref="FlowerWhip"/>, <see cref="EelWhip"/>, <see cref="ConstellationWhip"/>, <see cref="MoonLordWhip"/>, <see cref="SlimeWhip"/>
	/// </summary>
	public const short Whip = 165;
	/// <summary>
	/// Behavior: Includes Dove and Lantern projectiles<br/><br/>
	/// Used by: <see cref="ReleaseDoves"/>, <see cref="ReleaseLantern"/>
	/// </summary>
	public const short ReleasedProjectile = 166;
	/// <summary>
	/// Used by: <see cref="SparkleGuitar"/>
	/// </summary>
	public const short StellarTune = 167;
	/// <summary>
	/// Used by: <see cref="ProjectileID.FirstFractal"/>
	/// </summary>
	public const short FirstFractal = 168;
	/// <summary>
	/// Used by: <see cref="Smolstar"/>
	/// </summary>
	public const short EnchantedDagger = 169;
	/// <summary>
	/// Behavior: Only used when the Fairy GlowSticks's ai[1] is greater than 0<br/><br/>
	/// Used by: None
	/// </summary>
	public const short FairyGlowStick = 170;
	/// <summary>
	/// Behavior: Includes the Prismatic Bolt and Nightglow projectiles, these float for a second and then fly toward their target<br/><br/>
	/// Used by: <see cref="HallowBossRainbowStreak"/>, <see cref="FairyQueenMagicItemShot"/>, <see cref="ConstellationStar"/>
	/// </summary>
	public const short FloatAndFly = 171;
	/// <summary>
	/// Used by: <see cref="HallowBossSplitShotCore"/>
	/// </summary>
	public const short SplitShotCore = 172;
	/// <summary>
	/// Used by: <see cref="HallowBossLastingRainbow"/>
	/// </summary>
	public const short EverlastingRainbow = 173;
	/// <summary>
	/// Used by: <see cref="EaterOfWorldsPet"/>, <see cref="DestroyerPet"/>, <see cref="LunaticCultistPet"/>
	/// </summary>
	public const short WormPet = 174;
	/// <summary>
	/// Used by: <see cref="TitaniumStormShard"/>
	/// </summary>
	public const short TitaniumShard = 175;
	/// <summary>
	/// Behavior: The effect displayed when an enemy is hit with the Dark Harvest whip<br/><br/>
	/// Used by: <see cref="ScytheWhipProj"/>
	/// </summary>
	public const short Reaping = 176;
	/// <summary>
	/// Used by: <see cref="CoolWhipProj"/>, <see cref="WeatherPainShot"/>
	/// </summary>
	public const short CoolFlake = 177;
	/// <summary>
	/// Used by: <see cref="FireWhipProj"/>
	/// </summary>
	public const short FireCracker = 178;
	/// <summary>
	/// Used by: <see cref="FairyQueenLance"/>
	/// </summary>
	public const short EtherealLance = 179;
	/// <summary>
	/// Used by: <see cref="FairyQueenSunDance"/>
	/// </summary>
	public const short SunDance = 180;
	/// <summary>
	/// Used by: <see cref="FairyQueenRangedItemShot"/>
	/// </summary>
	public const short TwilightLance = 181;
	/// <summary>
	/// Used by: <see cref="FinalFractal"/>, <see cref="TrueCopperShortsword"/>
	/// </summary>
	public const short Zenith = 182;
	/// <summary>
	/// Used by: <see cref="ZoologistStrikeGreen"/>, <see cref="ZoologistStrikeRed"/>
	/// </summary>
	public const short ZoologistStike = 183;
	/// <summary>
	/// Behavior: The Torch God event, not the projectiles fired out of the torches<br/><br/>
	/// Used by: <see cref="ProjectileID.TorchGod"/>
	/// </summary>
	public const short TorchGod = 184;
	/// <summary>
	/// Used by: <see cref="SoulDrain"/>
	/// </summary>
	public const short LifeDrain = 185;
	/// <summary>
	/// Used by: <see cref="ProjectileID.PrincessWeapon"/>
	/// </summary>
	public const short PrincessWeapon = 186;
	/// <summary>
	/// Used by: <see cref="InsanityShadowFriendly"/>, <see cref="InsanityShadowHostile"/>
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
	/// Used by: <see cref="ProjectileID.NightsEdge"/>, <see cref="Excalibur"/>, <see cref="TrueExcalibur"/>, <see cref="TerraBlade2"/>, <see cref="TheHorsemansBlade"/>, <see cref="DeadCellsFlintSlash"/>
	/// </summary>
	public const short NightsEdge = 190;
	/// <summary>
	/// Used by: <see cref="ProjectileID.TrueNightsEdge"/>, <see cref="TerraBlade2Shot"/>
	/// </summary>
	public const short TrueNightsEdge = 191;
	/// <summary>
	/// Used by: <see cref="JuminoStardropAnimation"/>
	/// </summary>
	public const short JuminoAnimation = 192;
	/// <summary>
	/// Used by: <see cref="ProjectileID.Flames"/>, <see cref="PalworldMinionFoxsparksFlames"/>
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
	/// <summary>
	/// Used by: <see cref="FlowerWhipPetal"/>
	/// </summary>
	public const short Petal = 196;
	/// <summary>
	/// Used by: <see cref="DeadCellsBarnacle"/>
	/// </summary>
	public const short CeilingAndHoverTurret = 197;
	/// <summary>
	/// Used by: <see cref="DeadCellsFlint"/>
	/// </summary>
	public const short Flint = 198;
	/// <summary>
	/// Used by: <see cref="ProjectileID.MeteorOre"/>
	/// </summary>
	public const short MeteorOre = 199;
	/// <summary>
	/// Used by: <see cref="ProjectileID.BirdDroppings"/>
	/// </summary>
	public const short BirdDroppings = 200;
	/// <summary>
	/// Used by: <see cref="AntlionClaw"/>, <see cref="StylistKilLaKillScissorsIWish"/>
	/// </summary>
	public const short ThrownMelee = 201;
	/// <summary>
	/// Used by: <see cref="ProjectileID.TorchGodHelper"/>
	/// </summary>
	public const short TorchGodHelper = 202;
	/// <summary>
	/// Used by: <see cref="ProjectileID.StormLightning"/>
	/// </summary>
	public const short StormLightning = 203;
	/// <summary>
	/// Used by: <see cref="PalworldDigtoise"/>
	/// </summary>
	public const short Digtoise = 204;
	/// <summary>
	/// Used by: <see cref="ProjectileID.RemoteControlCar"/>
	/// </summary>
	public const short RemoteControlCar = 205;
}