using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using static Terraria.DataStructures.PlayerDrawLayers;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a DrawLayer for the player, and uses PlayerDrawInfo as its InfoType. Drawing should be done by adding Terraria.DataStructures.DrawData objects to Main.playerDrawData.
	/// </summary>
	[Autoload]
	public abstract class PlayerDrawLayer : DrawLayer<PlayerDrawLayer, PlayerDrawSet>
	{
		// Groups

		/// <summary> Adds <see cref="PlayerDrawSet.torsoOffset"/> to <see cref="PlayerDrawSet.Position"/> and <see cref="PlayerDrawSet.ItemLocation"/> vectors' Y axes. </summary>
		public static readonly PhysicalGroup TorsoGroup = new LegacyPlayerDrawGroup(DrawPlayer_extra_TorsoPlus, DrawPlayer_extra_TorsoMinus);
		
		/// <summary> Adds <see cref="PlayerDrawSet.mountOffSet"/>/2 to <see cref="PlayerDrawSet.Position"/> vector's Y axis. </summary>
		public static readonly PhysicalGroup MountGroup = new LegacyPlayerDrawGroup(DrawPlayer_extra_MountPlus, DrawPlayer_extra_MountMinus, TorsoGroup);

		// Normal layers

		/// <summary> Draws Jim's Cloak, if the player is wearing Jim's Leggings (a developer item). </summary>
		public static readonly PlayerDrawLayer JimsCloak = new LegacyPlayerDrawLayer(nameof(JimsCloak), DrawPlayer_01_2_JimsCloak, TorsoGroup);

		/// <summary> Draws the back textures of the player's mount. </summary>
		public static readonly PlayerDrawLayer MountBack = new LegacyPlayerDrawLayer(nameof(MountBack), DrawPlayer_02_MountBehindPlayer);
		
		/// <summary> Draws the Flying Carpet accessory, if the player has it equipped and is using it. </summary>
		public static readonly PlayerDrawLayer Carpet = new LegacyPlayerDrawLayer(nameof(Carpet), DrawPlayer_03_Carpet);
		
		/// <summary> Draws the Step Stool accessory, if the player has it equipped and is using it. </summary>
		public static readonly PlayerDrawLayer PortableStool = new LegacyPlayerDrawLayer(nameof(PortableStool), DrawPlayer_03_PortableStool);
		
		/// <summary> Draws the back textures of the Electrified debuff, if the player has it. </summary>
		public static readonly PlayerDrawLayer ElectrifiedDebuffBack = new LegacyPlayerDrawLayer(nameof(ElectrifiedDebuffBack), DrawPlayer_04_ElectrifiedDebuffBack, TorsoGroup);
		
		/// <summary> Draws the 'Forbidden Sign' if the player has a full 'Forbidden Armor' set equipped. </summary>
		public static readonly PlayerDrawLayer ForbiddenSetRing = new LegacyPlayerDrawLayer(nameof(ForbiddenSetRing), DrawPlayer_05_ForbiddenSetRing, TorsoGroup);
		
		/// <summary> Draws a sun above the player's head if they have "Safeman's Sunny Day" headgear equipped. </summary>
		public static readonly PlayerDrawLayer SafemanSun = new LegacyPlayerDrawLayer(nameof(SafemanSun), DrawPlayer_05_2_SafemanSun, TorsoGroup);

		/// <summary> Draws the back textures of the Webbed debuff, if the player has it. </summary>
		public static readonly PlayerDrawLayer WebbedDebuffBack = new LegacyPlayerDrawLayer(nameof(WebbedDebuffBack), DrawPlayer_06_WebbedDebuffBack, TorsoGroup);
		
		/// <summary> Draws effects of "Leinfors' Luxury Shampoo", if the player has it equipped. </summary>
		public static readonly PlayerDrawLayer LeinforsHairShampoo = new LegacyPlayerDrawLayer(nameof(LeinforsHairShampoo), DrawPlayer_07_LeinforsHairShampoo, TorsoGroup, isHeadLayer: true);

		/// <summary> Draws the player's held item's backpack. </summary>
		public static readonly PlayerDrawLayer Backpacks = new LegacyPlayerDrawLayer(nameof(Backpacks), DrawPlayer_08_Backpacks);

		/// <summary> Draws the player's wings. </summary>
		public static readonly PlayerDrawLayer Wings = new LegacyPlayerDrawLayer(nameof(Wings), DrawPlayer_09_Wings);

		/// <summary> Draws the player's back accessories. </summary>
		public static readonly PlayerDrawLayer BackAcc = new LegacyPlayerDrawLayer(nameof(BackAcc), DrawPlayer_10_BackAcc);

		/// <summary> Draws the player's under-headgear hair. </summary>
		public static readonly PlayerDrawLayer HairBack = new LegacyPlayerDrawLayer(nameof(HairBack), DrawPlayer_01_BackHair, isHeadLayer: true);

		/// <summary> Draws the back textures of the player's head, including armor. </summary>
		public static readonly PlayerDrawLayer HeadBack = new LegacyPlayerDrawLayer(nameof(HeadBack), DrawPlayer_01_3_BackHead, isHeadLayer: true);

		/// <summary> Draws the player's balloon accessory, if they have one. </summary>
		public static readonly PlayerDrawLayer BalloonAcc = new LegacyPlayerDrawLayer(nameof(BalloonAcc), DrawPlayer_11_Balloons);

		/// <summary> Draws the player's held item. </summary>
		public static readonly PlayerDrawLayer HeldItemBehindBackArm = new LegacyPlayerDrawLayer(nameof(HeldItemBehindBackArm), DrawPlayer_27_HeldItem, condition: drawinfo => drawinfo.weaponDrawOrder == WeaponDrawOrder.BehindBackArm);

		/// <summary> Draws the player's body and leg skin. </summary>
		public static readonly PlayerDrawLayer Skin = new LegacyPlayerDrawLayer(nameof(Skin), DrawPlayer_12_Skin);

		/// <summary> Draws the player's leg armor or pants and shoes. </summary>
		public static readonly PlayerDrawLayer Leggings = new LegacyPlayerDrawLayer(nameof(Leggings), DrawPlayer_13_Leggings, condition: drawinfo => !(drawinfo.drawPlayer.wearsRobe && drawinfo.drawPlayer.body != 166));

		/// <summary> Draws the player's shoes. </summary>
		public static readonly PlayerDrawLayer Shoes = new LegacyPlayerDrawLayer(nameof(Shoes), DrawPlayer_14_Shoes);

		/// <summary> Draws the player's robe. </summary>
		public static readonly PlayerDrawLayer Robe = new LegacyPlayerDrawLayer(nameof(Robe), DrawPlayer_13_Leggings, condition: drawinfo => drawinfo.drawPlayer.wearsRobe && drawinfo.drawPlayer.body != 166);

		/// <summary> Draws the longcoat default clothing style, if the player has it. </summary>
		public static readonly PlayerDrawLayer SkinLongCoat = new LegacyPlayerDrawLayer(nameof(SkinLongCoat), DrawPlayer_15_SkinLongCoat, TorsoGroup);
		
		/// <summary> Draws the currently equipped armor's longcoat, if it has one. </summary>
		public static readonly PlayerDrawLayer ArmorLongCoat = new LegacyPlayerDrawLayer(nameof(ArmorLongCoat), DrawPlayer_16_ArmorLongCoat, TorsoGroup);

		/// <summary> Draws the player's body armor or shirts. </summary>
		public static readonly PlayerDrawLayer Torso = new LegacyPlayerDrawLayer(nameof(Torso), DrawPlayer_17_Torso, TorsoGroup);

		/// <summary> Draws the player's off-hand accessory. </summary>
		public static readonly PlayerDrawLayer OffhandAcc = new LegacyPlayerDrawLayer(nameof(OffhandAcc), DrawPlayer_18_OffhandAcc, TorsoGroup);

		/// <summary> Draws the player's waist accessory. </summary>
		public static readonly PlayerDrawLayer WaistAcc = new LegacyPlayerDrawLayer(nameof(WaistAcc), DrawPlayer_19_WaistAcc, TorsoGroup);

		/// <summary> Draws the player's neck accessory. </summary>
		public static readonly PlayerDrawLayer NeckAcc = new LegacyPlayerDrawLayer(nameof(NeckAcc), DrawPlayer_20_NeckAcc, TorsoGroup);

		/// <summary> Draws the player's head, including hair, armor, and etc. </summary>
		public static readonly PlayerDrawLayer Head = new LegacyPlayerDrawLayer(nameof(Head), DrawPlayer_21_Head, TorsoGroup, isHeadLayer: true);

		/// <summary> Draws the player's face accessory. </summary>
		public static readonly PlayerDrawLayer FaceAcc = new LegacyPlayerDrawLayer(nameof(FaceAcc), DrawPlayer_22_FaceAcc, TorsoGroup);

		/// <summary> Draws the front part of player's front accessory. </summary>
		public static readonly PlayerDrawLayer FrontAccFrontNeck = new LegacyPlayerDrawLayer(nameof(FrontAccFrontNeck), DrawPlayer_32_FrontAcc_FrontPart, condition: drawinfo => drawinfo.drawFrontAccInNeckAccLayer);

		/// <summary> Draws the front textures of the player's mount. </summary>
		public static readonly PlayerDrawLayer MountFront = new LegacyPlayerDrawLayer(nameof(MountFront), DrawPlayer_23_MountFront, TorsoGroup);

		/// <summary> Draws the pulley if the player is hanging on a rope. </summary>
		public static readonly PlayerDrawLayer Pulley = new LegacyPlayerDrawLayer(nameof(Pulley), DrawPlayer_24_Pulley, TorsoGroup);

		/// <summary> Draws the back part of player's front accessory. </summary>
		public static readonly PlayerDrawLayer FrontAccBack = new LegacyPlayerDrawLayer(nameof(FrontAccBack), DrawPlayer_32_FrontAcc_BackPart, TorsoGroup);

		/// <summary> Draws the player's shield accessory. </summary>
		public static readonly PlayerDrawLayer Shield = new LegacyPlayerDrawLayer(nameof(Shield), DrawPlayer_25_Shield, TorsoGroup);

		/// <summary> Draws the player's solar shield if the player has one. </summary>
		public static readonly PlayerDrawLayer SolarShield = new LegacyPlayerDrawLayer(nameof(SolarShield), DrawPlayer_26_SolarShield, MountGroup);

		/// <summary> Draws the player's held item. </summary>
		public static readonly PlayerDrawLayer HeldItemBehindFrontArm = new LegacyPlayerDrawLayer(nameof(HeldItemBehindFrontArm), DrawPlayer_27_HeldItem, TorsoGroup, condition: drawinfo => drawinfo.weaponDrawOrder == WeaponDrawOrder.BehindFrontArm);

		/// <summary> Draws the player's main arm (including the armor's if applicable), when it should appear over the held item. </summary>
		public static readonly PlayerDrawLayer ArmOverItem = new LegacyPlayerDrawLayer(nameof(ArmOverItem), DrawPlayer_28_ArmOverItem, TorsoGroup);

		/// <summary> Draws the player's hand on accessory. </summary>
		public static readonly PlayerDrawLayer HandOnAcc = new LegacyPlayerDrawLayer(nameof(HandOnAcc), DrawPlayer_29_OnhandAcc, TorsoGroup);
		
		/// <summary> Draws the Bladed Glove item, if the player is currently using it. </summary>
		public static readonly PlayerDrawLayer BladedGlove = new LegacyPlayerDrawLayer(nameof(BladedGlove), DrawPlayer_30_BladedGlove, TorsoGroup);

		/// <summary> Draws the front part of player's front accessory. </summary>
		public static readonly PlayerDrawLayer FrontAccFront = new LegacyPlayerDrawLayer(nameof(FrontAccFront), DrawPlayer_32_FrontAcc_FrontPart, condition: drawinfo => !drawinfo.drawFrontAccInNeckAccLayer);

		/// <summary> Draws the player's held item. </summary>
		public static readonly PlayerDrawLayer HeldItemOverFrontArm = new LegacyPlayerDrawLayer(nameof(HeldItemOverFrontArm), DrawPlayer_27_HeldItem, condition: drawinfo => drawinfo.weaponDrawOrder == WeaponDrawOrder.OverFrontArm);

		/// <summary> Draws the player's held projectile, if it should be drawn in front of the held item and arms. </summary>
		public static readonly PlayerDrawLayer ProjectileOverArm = new LegacyPlayerDrawLayer(nameof(ProjectileOverArm), DrawPlayer_31_ProjectileOverArm);

		/// <summary> Draws the front textures of either Frozen or Webbed debuffs, if the player has one of them. </summary>
		public static readonly PlayerDrawLayer FrozenOrWebbedDebuff = new LegacyPlayerDrawLayer(nameof(FrozenOrWebbedDebuff), DrawPlayer_33_FrozenOrWebbedDebuff);
		
		/// <summary> Draws the front textures of the Electrified debuff, if the player has it. </summary>
		public static readonly PlayerDrawLayer ElectrifiedDebuffFront = new LegacyPlayerDrawLayer(nameof(ElectrifiedDebuffFront), DrawPlayer_34_ElectrifiedDebuffFront);
		
		/// <summary> Draws the textures of the Ice Barrier buff, if the player has it. </summary>
		public static readonly PlayerDrawLayer IceBarrier = new LegacyPlayerDrawLayer(nameof(IceBarrier), DrawPlayer_35_IceBarrier);
		
		/// <summary> Draws a big gem above the player, if the player is currently in possession of a 'Capture The Gem' gem item. </summary>
		public static readonly PlayerDrawLayer CaptureTheGem = new LegacyPlayerDrawLayer(nameof(CaptureTheGem), DrawPlayer_36_CTG);
		
		/// <summary> Draws the effects of Beetle Armor's Set buffs, if the player currently has any. </summary>
		public static readonly PlayerDrawLayer BeetleBuff = new LegacyPlayerDrawLayer(nameof(BeetleBuff), DrawPlayer_37_BeetleBuff);

		/// <summary> Returns whether or not this layer should be rendered for the minimap icon. </summary>
		public virtual bool IsHeadLayer => false;

		protected override void Register() {
			ModTypeLookup<PlayerDrawLayer>.Register(this);
			PlayerDrawLayerHooks.Add(this);
		}
	}
}
