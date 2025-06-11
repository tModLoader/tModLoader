using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace Terraria.DataStructures;

public partial class PlayerDrawLayers
{
	// Draw Groups

	/// <summary> Adds <see cref="PlayerDrawSet.torsoOffset"/> to <see cref="PlayerDrawSet.Position"/> and <see cref="PlayerDrawSet.ItemLocation"/> vectors' Y axes. </summary>
	public static readonly Transformation TorsoGroup = new VanillaPlayerDrawTransform(DrawPlayer_extra_TorsoPlus, DrawPlayer_extra_TorsoMinus);

	/// <summary> Adds <see cref="PlayerDrawSet.mountOffSet"/>/2 to <see cref="PlayerDrawSet.Position"/> vector's Y axis. </summary>
	public static readonly Transformation MountGroup = new VanillaPlayerDrawTransform(DrawPlayer_extra_MountPlus, DrawPlayer_extra_MountMinus, TorsoGroup);

	// Normal layers

	/// <summary> Draws Jim's Cloak, if the player is wearing Jim's Leggings (a developer item). </summary>
	public static readonly PlayerDrawLayer JimsCloak = new VanillaPlayerDrawLayer(nameof(JimsCloak), DrawPlayer_01_2_JimsCloak, TorsoGroup);

	/// <summary> Draws the back textures of the player's mount. </summary>
	public static readonly PlayerDrawLayer MountBack = new VanillaPlayerDrawLayer(nameof(MountBack), DrawPlayer_02_MountBehindPlayer);

	/// <summary> Draws the Flying Carpet accessory, if the player has it equipped and is using it. </summary>
	public static readonly PlayerDrawLayer Carpet = new VanillaPlayerDrawLayer(nameof(Carpet), DrawPlayer_03_Carpet);

	/// <summary> Draws the Step Stool accessory, if the player has it equipped and is using it. </summary>
	public static readonly PlayerDrawLayer PortableStool = new VanillaPlayerDrawLayer(nameof(PortableStool), DrawPlayer_03_PortableStool);

	/// <summary> Draws the back textures of the Electrified debuff, if the player has it. </summary>
	public static readonly PlayerDrawLayer ElectrifiedDebuffBack = new VanillaPlayerDrawLayer(nameof(ElectrifiedDebuffBack), DrawPlayer_04_ElectrifiedDebuffBack, TorsoGroup);

	/// <summary> Draws the 'Forbidden Sign' if the player has a full 'Forbidden Armor' set equipped. </summary>
	public static readonly PlayerDrawLayer ForbiddenSetRing = new VanillaPlayerDrawLayer(nameof(ForbiddenSetRing), DrawPlayer_05_ForbiddenSetRing, TorsoGroup);

	/// <summary> Draws a sun above the player's head if they have "Safeman's Sunny Day" headgear equipped. </summary>
	public static readonly PlayerDrawLayer SafemanSun = new VanillaPlayerDrawLayer(nameof(SafemanSun), DrawPlayer_05_2_SafemanSun, TorsoGroup);

	/// <summary> Draws the back textures of the Webbed debuff, if the player has it. </summary>
	public static readonly PlayerDrawLayer WebbedDebuffBack = new VanillaPlayerDrawLayer(nameof(WebbedDebuffBack), DrawPlayer_06_WebbedDebuffBack, TorsoGroup);

	/// <summary> Draws effects of "Leinfors' Luxury Shampoo", if the player has it equipped. </summary>
	public static readonly PlayerDrawLayer LeinforsHairShampoo = new VanillaPlayerDrawLayer(nameof(LeinforsHairShampoo), DrawPlayer_07_LeinforsHairShampoo, TorsoGroup, isHeadLayer: true);

	/// <summary> Draws the player's held item's backpack. </summary>
	public static readonly PlayerDrawLayer Backpacks = new VanillaPlayerDrawLayer(nameof(Backpacks), DrawPlayer_08_Backpacks);

	/// <summary> Draws the player's tails vanities. </summary>
	public static readonly PlayerDrawLayer Tails = new VanillaPlayerDrawLayer(nameof(Tails), DrawPlayer_08_1_Tails, TorsoGroup);

	/// <summary> Draws the player's wings. </summary>
	public static readonly PlayerDrawLayer Wings = new VanillaPlayerDrawLayer(nameof(Wings), DrawPlayer_09_Wings);

	/// <summary> Draws the player's under-headgear hair. </summary>
	public static readonly PlayerDrawLayer HairBack = new VanillaPlayerDrawLayer(nameof(HairBack), DrawPlayer_01_BackHair, TorsoGroup, isHeadLayer: true);

	/// <summary> Draws the player's back accessories. </summary>
	public static readonly PlayerDrawLayer BackAcc = new VanillaPlayerDrawLayer(nameof(BackAcc), DrawPlayer_10_BackAcc, TorsoGroup);

	/// <summary> Draws the back textures of the player's head, including armor. </summary>
	public static readonly PlayerDrawLayer HeadBack = new VanillaPlayerDrawLayer(nameof(HeadBack), DrawPlayer_01_3_BackHead, TorsoGroup, isHeadLayer: true);

	/// <summary> Draws the player's balloon accessory, if they have one. </summary>
	public static readonly PlayerDrawLayer BalloonAcc = new VanillaPlayerDrawLayer(nameof(BalloonAcc), DrawPlayer_11_Balloons);

	/// <summary> Draws the player's body and leg skin. </summary>
	public static readonly PlayerDrawLayer Skin = new VanillaPlayerDrawLayer(nameof(Skin), DrawPlayer_12_Skin);

	/// <summary> Draws the player's leg armor or pants and shoes. </summary>
	public static readonly PlayerDrawLayer Leggings = new VanillaPlayerDrawLayer(nameof(Leggings), DrawPlayer_13_Leggings, condition: drawinfo => !(drawinfo.drawPlayer.wearsRobe && drawinfo.drawPlayer.body != 166));

	/// <summary> Draws the player's shoes. </summary>
	public static readonly PlayerDrawLayer Shoes = new VanillaPlayerDrawLayer(nameof(Shoes), DrawPlayer_14_Shoes);

	/// <summary> Draws the player's robe. </summary>
	public static readonly PlayerDrawLayer Robe = new VanillaPlayerDrawLayer(nameof(Robe), DrawPlayer_13_Leggings, condition: drawinfo => drawinfo.drawPlayer.wearsRobe && drawinfo.drawPlayer.body != 166);

	/// <summary> Draws the longcoat default clothing style, if the player has it. </summary>
	public static readonly PlayerDrawLayer SkinLongCoat = new VanillaPlayerDrawLayer(nameof(SkinLongCoat), DrawPlayer_15_SkinLongCoat, TorsoGroup);

	/// <summary> Draws the currently equipped armor's longcoat, if it has one. </summary>
	public static readonly PlayerDrawLayer ArmorLongCoat = new VanillaPlayerDrawLayer(nameof(ArmorLongCoat), DrawPlayer_16_ArmorLongCoat, TorsoGroup);

	/// <summary> Draws the player's body armor or shirts. </summary>
	public static readonly PlayerDrawLayer Torso = new VanillaPlayerDrawLayer(nameof(Torso), DrawPlayer_17_Torso, TorsoGroup);

	/// <summary> Draws the player's off-hand accessory. </summary>
	public static readonly PlayerDrawLayer OffhandAcc = new VanillaPlayerDrawLayer(nameof(OffhandAcc), DrawPlayer_18_OffhandAcc, TorsoGroup);

	/// <summary> Draws the player's waist accessory. </summary>
	public static readonly PlayerDrawLayer WaistAcc = new VanillaPlayerDrawLayer(nameof(WaistAcc), DrawPlayer_19_WaistAcc, TorsoGroup);

	/// <summary> Draws the player's neck accessory. </summary>
	public static readonly PlayerDrawLayer NeckAcc = new VanillaPlayerDrawLayer(nameof(NeckAcc), DrawPlayer_20_NeckAcc, TorsoGroup);

	/// <summary> Draws the player's head, including hair, armor, and etc. </summary>
	public static readonly PlayerDrawLayer Head = new VanillaPlayerDrawLayer(nameof(Head), DrawPlayer_21_Head, TorsoGroup, isHeadLayer: true);

	/// <summary> Draws a finch nest on the player's head, if the player has a finch summoned. </summary>
	public static readonly PlayerDrawLayer FinchNest = new VanillaPlayerDrawLayer(nameof(FinchNest), DrawPlayer_21_2_FinchNest, TorsoGroup, isHeadLayer: false);

	/// <summary> Draws the player's face accessory. </summary>
	public static readonly PlayerDrawLayer FaceAcc = new VanillaPlayerDrawLayer(nameof(FaceAcc), DrawPlayer_22_FaceAcc, TorsoGroup, isHeadLayer: true);

	/// <summary> Draws the front textures of the player's mount. </summary>
	public static readonly PlayerDrawLayer MountFront = new VanillaPlayerDrawLayer(nameof(MountFront), DrawPlayer_23_MountFront, TorsoGroup);

	/// <summary> Draws the pulley if the player is hanging on a rope. </summary>
	public static readonly PlayerDrawLayer Pulley = new VanillaPlayerDrawLayer(nameof(Pulley), DrawPlayer_24_Pulley, TorsoGroup);

	/// <summary> Draws the pulley if the player is hanging on a rope. </summary>
	public static readonly PlayerDrawLayer JimsDroneRadio = new VanillaPlayerDrawLayer(nameof(JimsDroneRadio), DrawPlayer_JimsDroneRadio, TorsoGroup);

	/// <summary> Draws the back part of player's front accessory. </summary>
	public static readonly PlayerDrawLayer FrontAccBack = new VanillaPlayerDrawLayer(nameof(FrontAccBack), DrawPlayer_32_FrontAcc_BackPart, TorsoGroup);

	/// <summary> Draws the player's shield accessory. </summary>
	public static readonly PlayerDrawLayer Shield = new VanillaPlayerDrawLayer(nameof(Shield), DrawPlayer_25_Shield, TorsoGroup);

	/// <summary> Draws the player's solar shield if the player has one. </summary>
	public static readonly PlayerDrawLayer SolarShield = new VanillaPlayerDrawLayer(nameof(SolarShield), DrawPlayer_26_SolarShield, MountGroup);

	/// <summary> Draws the player's main arm (including the armor's if applicable), when it should appear over the held item. </summary>
	public static readonly PlayerDrawLayer ArmOverItem = new VanillaPlayerDrawLayer(nameof(ArmOverItem), DrawPlayer_28_ArmOverItem, TorsoGroup);

	/// <summary> Draws the player's hand on accessory. </summary>
	public static readonly PlayerDrawLayer HandOnAcc = new VanillaPlayerDrawLayer(nameof(HandOnAcc), DrawPlayer_29_OnhandAcc, TorsoGroup);

	/// <summary> Draws the Bladed Glove item, if the player is currently using it. </summary>
	public static readonly PlayerDrawLayer BladedGlove = new VanillaPlayerDrawLayer(nameof(BladedGlove), DrawPlayer_30_BladedGlove, TorsoGroup);

	/// <summary> Draws the player's held projectile, if it should be drawn in front of the held item and arms. </summary>
	public static readonly PlayerDrawLayer ProjectileOverArm = new VanillaPlayerDrawLayer(nameof(ProjectileOverArm), DrawPlayer_31_ProjectileOverArm);

	/// <summary> Draws the front textures of either Frozen or Webbed debuffs, if the player has one of them. </summary>
	public static readonly PlayerDrawLayer FrozenOrWebbedDebuff = new VanillaPlayerDrawLayer(nameof(FrozenOrWebbedDebuff), DrawPlayer_33_FrozenOrWebbedDebuff);

	/// <summary> Draws the front textures of the Electrified debuff, if the player has it. </summary>
	public static readonly PlayerDrawLayer ElectrifiedDebuffFront = new VanillaPlayerDrawLayer(nameof(ElectrifiedDebuffFront), DrawPlayer_34_ElectrifiedDebuffFront);

	/// <summary> Draws the textures of the Ice Barrier buff, if the player has it. </summary>
	public static readonly PlayerDrawLayer IceBarrier = new VanillaPlayerDrawLayer(nameof(IceBarrier), DrawPlayer_35_IceBarrier);

	/// <summary> Draws a big gem above the player, if the player is currently in possession of a 'Capture The Gem' gem item. </summary>
	public static readonly PlayerDrawLayer CaptureTheGem = new VanillaPlayerDrawLayer(nameof(CaptureTheGem), DrawPlayer_36_CTG);

	/// <summary> Draws the effects of Beetle Armor's Set buffs, if the player currently has any. </summary>
	public static readonly PlayerDrawLayer BeetleBuff = new VanillaPlayerDrawLayer(nameof(BeetleBuff), DrawPlayer_37_BeetleBuff);

	/// <summary> Draws the effects of Eyebrella Cloud, if the player currently has it. </summary>
	public static readonly PlayerDrawLayer EyebrellaCloud = new VanillaPlayerDrawLayer(nameof(EyebrellaCloud), DrawPlayer_38_EyebrellaCloud);

	// Mobile layers

	/// <summary> Draws the front part of player's front accessory. </summary>
	public static readonly PlayerDrawLayer FrontAccFront = new VanillaPlayerDrawLayer(nameof(FrontAccFront), DrawPlayer_32_FrontAcc_FrontPart,
		position: new Multiple() {
			{ new Between(FaceAcc, MountFront), drawinfo => drawinfo.drawFrontAccInNeckAccLayer },
			{ new Between(BladedGlove, ProjectileOverArm), drawinfo => !drawinfo.drawFrontAccInNeckAccLayer }
		});

	/// <summary> Draws the player's held item. </summary>
	public static readonly PlayerDrawLayer HeldItem = new VanillaPlayerDrawLayer(nameof(HeldItem), DrawPlayer_27_HeldItem,
		position: new Multiple() {
			{ new Between(BalloonAcc, Skin), drawinfo => drawinfo.weaponDrawOrder == WeaponDrawOrder.BehindBackArm },
			{ new Between(SolarShield, ArmOverItem), drawinfo => drawinfo.weaponDrawOrder == WeaponDrawOrder.BehindFrontArm },
			{ new Between(BladedGlove, ProjectileOverArm), drawinfo => drawinfo.weaponDrawOrder == WeaponDrawOrder.OverFrontArm }
		});


	// Compare this with the vanilla layer drawing call order to make sure it's accurate when updating
	internal static IReadOnlyList<PlayerDrawLayer> FixedVanillaLayers => new [] {
		JimsCloak,
		MountBack,
		Carpet,
		PortableStool,
		ElectrifiedDebuffBack,
		ForbiddenSetRing,
		SafemanSun,
		WebbedDebuffBack,
		LeinforsHairShampoo,
		Backpacks,
		Tails,
		Wings,
		HairBack,
		BackAcc,
		HeadBack,
		BalloonAcc,
		Skin,
		Leggings, // !(drawinfo.drawPlayer.wearsRobe && drawinfo.drawPlayer.body != 166)
		Shoes,
		Robe, // drawinfo.drawPlayer.wearsRobe && drawinfo.drawPlayer.body != 166
		SkinLongCoat,
		ArmorLongCoat,
		Torso,
		OffhandAcc,
		WaistAcc,
		NeckAcc,
		Head,
		FinchNest,
		FaceAcc,
		MountFront,
		Pulley,
		JimsDroneRadio,
		FrontAccBack,
		Shield,
		SolarShield,
		ArmOverItem,
		HandOnAcc,
		BladedGlove,
		ProjectileOverArm,
		FrozenOrWebbedDebuff,
		ElectrifiedDebuffFront,
		IceBarrier,
		CaptureTheGem,
		BeetleBuff,
		EyebrellaCloud
	};

	internal static IReadOnlyList<PlayerDrawLayer> VanillaLayers = FixedVanillaLayers.Concat(new[] { FrontAccFront, HeldItem }).ToArray();

	public static PlayerDrawLayer FirstVanillaLayer => FixedVanillaLayers[0];
	public static PlayerDrawLayer LastVanillaLayer => FixedVanillaLayers[^1];

	/// <summary>
	/// Use to order this layer before the first vanilla layer. This layer will draw behind all vanilla layers.
	/// </summary>
	public static Between BeforeFirstVanillaLayer => new Between(null, FirstVanillaLayer);
	/// <summary>
	/// Use to order this layer after the last vanilla layer. This layer will draw after all vanilla layers.
	/// </summary>
	public static Between AfterLastVanillaLayer => new Between(LastVanillaLayer, null);
}
