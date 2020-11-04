using Terraria.DataStructures;
using static Terraria.DataStructures.PlayerDrawLayers;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a DrawLayer for the player, and uses PlayerDrawInfo as its InfoType. Drawing should be done by adding Terraria.DataStructures.DrawData objects to Main.playerDrawData.
	/// </summary>
	[Autoload]
	public abstract class PlayerDrawLayer : DrawLayer<PlayerDrawSet>
	{
		//Technical layers

		/// <summary> Technical layer. Adds <see cref="PlayerDrawSet.torsoOffset"/> to <see cref="PlayerDrawSet.Position"/> and <see cref="PlayerDrawSet.ItemLocation"/> vectors' Y axes. </summary>
		public static readonly PlayerDrawLayer TorsoIndent = new LegacyPlayerDrawLayer(nameof(TorsoIndent), false, DrawPlayer_extra_TorsoPlus);
		
		/// <summary> Technical layer. Subtracts <see cref="PlayerDrawSet.torsoOffset"/> from <see cref="PlayerDrawSet.Position"/> and <see cref="PlayerDrawSet.ItemLocation"/> vectors' Y axes. </summary>
		public static readonly PlayerDrawLayer TorsoUnindent = new LegacyPlayerDrawLayer(nameof(TorsoUnindent), false, DrawPlayer_extra_TorsoMinus);
		
		/// <summary> Technical layer. Adds <see cref="PlayerDrawSet.mountOffSet"/>/2 to <see cref="PlayerDrawSet.Position"/> vector's Y axis. </summary>
		public static readonly PlayerDrawLayer MountIndent = new LegacyPlayerDrawLayer(nameof(MountIndent), false, DrawPlayer_extra_MountPlus);

		/// <summary> Technical layer. Subtracts <see cref="PlayerDrawSet.mountOffSet"/>/2 from <see cref="PlayerDrawSet.Position"/> vector's Y axis. </summary>
		public static readonly PlayerDrawLayer MountUnindent = new LegacyPlayerDrawLayer(nameof(MountUnindent), false, DrawPlayer_extra_MountMinus);

		//Normal layers

		/// <summary> Draws the player's under-headgear hair. </summary>
		public static readonly PlayerDrawLayer HairBack = new LegacyPlayerDrawLayer(nameof(HairBack), true, DrawPlayer_01_BackHair);

		/// <summary> Draws Jim's Cloak, if the player is wearing Jim's Leggings (a developer item). </summary>
		public static readonly PlayerDrawLayer JimsCloak = new LegacyPlayerDrawLayer(nameof(JimsCloak), false, DrawPlayer_01_2_JimsCloak);

		/// <summary> Draws the back textures of the player's head, including armor. </summary>
		public static readonly PlayerDrawLayer HeadBack = new LegacyPlayerDrawLayer(nameof(HeadBack), true, DrawPlayer_01_3_BackHead);

		/// <summary> Draws the back textures of the player's mount. </summary>
		public static readonly PlayerDrawLayer MountBack = new LegacyPlayerDrawLayer(nameof(MountBack), false, DrawPlayer_02_MountBehindPlayer);
		
		/// <summary> Draws the Flying Carpet accessory, if the player has it equipped and is using it. </summary>
		public static readonly PlayerDrawLayer Carpet = new LegacyPlayerDrawLayer(nameof(Carpet), false, DrawPlayer_03_Carpet);
		
		/// <summary> Draws the Step Stool accessory, if the player has it equipped and is using it. </summary>
		public static readonly PlayerDrawLayer PortableStool = new LegacyPlayerDrawLayer(nameof(PortableStool), false, DrawPlayer_03_PortableStool);
		
		/// <summary> Draws the back textures of the Electrified debuff, if the player has it. </summary>
		public static readonly PlayerDrawLayer ElectrifiedDebuffBack = new LegacyPlayerDrawLayer(nameof(ElectrifiedDebuffBack), false, DrawPlayer_04_ElectrifiedDebuffBack);
		
		/// <summary> Draws the 'Forbidden Sign' if the player has a full 'Forbidden Armor' set equipped. </summary>
		public static readonly PlayerDrawLayer ForbiddenSetRing = new LegacyPlayerDrawLayer(nameof(ForbiddenSetRing), false, DrawPlayer_05_ForbiddenSetRing);
		
		/// <summary> Draws a sun above the player's head if they have "Safeman's Sunny Day" headgear equipped. </summary>
		public static readonly PlayerDrawLayer SafemanSun = new LegacyPlayerDrawLayer(nameof(SafemanSun), false, DrawPlayer_05_2_SafemanSun);

		/// <summary> Draws the back textures of the Webbed debuff, if the player has it. </summary>
		public static readonly PlayerDrawLayer WebbedDebuffBack = new LegacyPlayerDrawLayer(nameof(WebbedDebuffBack), false, DrawPlayer_06_WebbedDebuffBack);
		
		/// <summary> Draws effects of "Leinfors' Luxury Shampoo", if the player has it equipped. </summary>
		public static readonly PlayerDrawLayer LeinforsHairShampoo = new LegacyPlayerDrawLayer(nameof(LeinforsHairShampoo), true, DrawPlayer_07_LeinforsHairShampoo);

		/// <summary> Draws the player's held item's backpack. </summary>
		public static readonly PlayerDrawLayer Backpacks = new LegacyPlayerDrawLayer(nameof(Backpacks), false, DrawPlayer_08_Backpacks);

		/// <summary> Draws the player's back accessories. </summary>
		public static readonly PlayerDrawLayer BackAcc = new LegacyPlayerDrawLayer(nameof(BackAcc), false, DrawPlayer_10_BackAcc);

		/// <summary> Draws the player's wings. </summary>
		public static readonly PlayerDrawLayer Wings = new LegacyPlayerDrawLayer(nameof(Wings), false, DrawPlayer_09_Wings);

		/// <summary> Draws the player's balloon accessory, if they have one. </summary>
		public static readonly PlayerDrawLayer BalloonAcc = new LegacyPlayerDrawLayer(nameof(BalloonAcc), false, DrawPlayer_11_Balloons);

		/// <summary> Draws the player's held item. </summary>
		public static readonly PlayerDrawLayer HeldItemBehindBackArm = new LegacyPlayerDrawLayer(nameof(HeldItemBehindBackArm), false, DrawPlayer_27_HeldItem, drawinfo => drawinfo.weaponDrawOrder == WeaponDrawOrder.BehindBackArm);

		/// <summary> Draws the player's held item. </summary>
		public static readonly PlayerDrawLayer HeldItemBehindFrontArm = new LegacyPlayerDrawLayer(nameof(HeldItemBehindFrontArm), false, DrawPlayer_27_HeldItem, drawinfo => drawinfo.weaponDrawOrder == WeaponDrawOrder.BehindFrontArm);

		/// <summary> Draws the player's held item. </summary>
		public static readonly PlayerDrawLayer HeldItemOverFrontArm = new LegacyPlayerDrawLayer(nameof(HeldItemOverFrontArm), false, DrawPlayer_27_HeldItem, drawinfo => drawinfo.weaponDrawOrder == WeaponDrawOrder.OverFrontArm);

		/// <summary> Draws the player's body and leg skin. </summary>
		public static readonly PlayerDrawLayer Skin = new LegacyPlayerDrawLayer(nameof(Skin), false, DrawPlayer_12_Skin);

		/// <summary> Draws the player's shoes. </summary>
		public static readonly PlayerDrawLayer Shoes = new LegacyPlayerDrawLayer(nameof(Shoes), false, DrawPlayer_14_Shoes);

		/// <summary> Draws the player's leg armor or pants and shoes. </summary>
		public static readonly PlayerDrawLayer Leggings = new LegacyPlayerDrawLayer(nameof(Leggings), false, DrawPlayer_13_Leggings, drawinfo => !(drawinfo.drawPlayer.wearsRobe && drawinfo.drawPlayer.body != 166));

		/// <summary> Draws the player's robe. </summary>
		public static readonly PlayerDrawLayer Robe = new LegacyPlayerDrawLayer(nameof(Robe), false, DrawPlayer_13_Leggings, drawinfo => drawinfo.drawPlayer.wearsRobe && drawinfo.drawPlayer.body != 166);

		/// <summary> Draws the longcoat default clothing style, if the player has it. </summary>
		public static readonly PlayerDrawLayer SkinLongCoat = new LegacyPlayerDrawLayer(nameof(SkinLongCoat), false, DrawPlayer_15_SkinLongCoat);
		
		/// <summary> Draws the currently equipped armor's longcoat, if it has one. </summary>
		public static readonly PlayerDrawLayer ArmorLongCoat = new LegacyPlayerDrawLayer(nameof(ArmorLongCoat), false, DrawPlayer_16_ArmorLongCoat);

		/// <summary> Draws the player's body armor or shirts. </summary>
		public static readonly PlayerDrawLayer Torso = new LegacyPlayerDrawLayer(nameof(Torso), false, DrawPlayer_17_Torso);

		/// <summary> Draws the player's off-hand accessory. </summary>
		public static readonly PlayerDrawLayer OffhandAcc = new LegacyPlayerDrawLayer(nameof(OffhandAcc), false, DrawPlayer_18_OffhandAcc);

		/// <summary> Draws the player's waist accessory. </summary>
		public static readonly PlayerDrawLayer WaistAcc = new LegacyPlayerDrawLayer(nameof(WaistAcc), false, DrawPlayer_19_WaistAcc);

		/// <summary> Draws the player's neck accessory. </summary>
		public static readonly PlayerDrawLayer NeckAcc = new LegacyPlayerDrawLayer(nameof(NeckAcc), false, DrawPlayer_20_NeckAcc);

		/// <summary> Draws the player's head, including hair, armor, and etc. </summary>
		public static readonly PlayerDrawLayer Head = new LegacyPlayerDrawLayer(nameof(Head), true, DrawPlayer_21_Head);

		/// <summary> Draws the player's face accessory. </summary>
		public static readonly PlayerDrawLayer FaceAcc = new LegacyPlayerDrawLayer(nameof(FaceAcc), false, DrawPlayer_22_FaceAcc);

		/// <summary> Draws the front part of player's front accessory. </summary>
		public static readonly PlayerDrawLayer FrontAccFront = new LegacyPlayerDrawLayer(nameof(FrontAccFront), false, DrawPlayer_32_FrontAcc_FrontPart, drawinfo => !drawinfo.drawFrontAccInNeckAccLayer);

		/// <summary> Draws the front part of player's front accessory. </summary>
		public static readonly PlayerDrawLayer FrontAccFrontNeck = new LegacyPlayerDrawLayer(nameof(FrontAccFrontNeck), false, DrawPlayer_32_FrontAcc_FrontPart, drawinfo => drawinfo.drawFrontAccInNeckAccLayer);

		/// <summary> Draws the back part of player's front accessory. </summary>
		public static readonly PlayerDrawLayer FrontAccBack = new LegacyPlayerDrawLayer(nameof(FrontAccBack), false, DrawPlayer_32_FrontAcc_BackPart);

		/// <summary> Draws the front textures of the player's mount. </summary>
		public static readonly PlayerDrawLayer MountFront = new LegacyPlayerDrawLayer(nameof(MountFront), false, DrawPlayer_23_MountFront);

		/// <summary> Draws the pulley if the player is hanging on a rope. </summary>
		public static readonly PlayerDrawLayer Pulley = new LegacyPlayerDrawLayer(nameof(Pulley), false, DrawPlayer_24_Pulley);

		/// <summary> Draws the player's shield accessory. </summary>
		public static readonly PlayerDrawLayer Shield = new LegacyPlayerDrawLayer(nameof(Shield), false, DrawPlayer_25_Shield);

		/// <summary> Draws the player's solar shield if the player has one. </summary>
		public static readonly PlayerDrawLayer SolarShield = new LegacyPlayerDrawLayer(nameof(SolarShield), false, DrawPlayer_26_SolarShield);

		/// <summary> Draws the player's main arm (including the armor's if applicable), when it should appear over the held item. </summary>
		public static readonly PlayerDrawLayer ArmOverItem = new LegacyPlayerDrawLayer(nameof(ArmOverItem), false, DrawPlayer_28_ArmOverItem);

		/// <summary> Draws the player's hand on accessory. </summary>
		public static readonly PlayerDrawLayer HandOnAcc = new LegacyPlayerDrawLayer(nameof(HandOnAcc), false, DrawPlayer_29_OnhandAcc);
		
		/// <summary> Draws the Bladed Glove item, if the player is currently using it. </summary>
		public static readonly PlayerDrawLayer BladedGlove = new LegacyPlayerDrawLayer(nameof(BladedGlove), false, DrawPlayer_30_BladedGlove);

		/// <summary> Draws the player's held projectile, if it should be drawn in front of the held item and arms. </summary>
		public static readonly PlayerDrawLayer ProjectileOverArm = new LegacyPlayerDrawLayer(nameof(ProjectileOverArm), false, DrawPlayer_31_ProjectileOverArm);

		/// <summary> Draws the front textures of either Frozen or Webbed debuffs, if the player has one of them. </summary>
		public static readonly PlayerDrawLayer FrozenOrWebbedDebuff = new LegacyPlayerDrawLayer(nameof(FrozenOrWebbedDebuff), false, DrawPlayer_33_FrozenOrWebbedDebuff);
		
		/// <summary> Draws the front textures of the Electrified debuff, if the player has it. </summary>
		public static readonly PlayerDrawLayer ElectrifiedDebuffFront = new LegacyPlayerDrawLayer(nameof(ElectrifiedDebuffFront), false, DrawPlayer_34_ElectrifiedDebuffFront);
		
		/// <summary> Draws the textures of the Ice Barrier buff, if the player has it. </summary>
		public static readonly PlayerDrawLayer IceBarrier = new LegacyPlayerDrawLayer(nameof(IceBarrier), false, DrawPlayer_35_IceBarrier);
		
		/// <summary> Draws a big gem above the player, if the player is currently in possession of a 'Capture The Gem' gem item. </summary>
		public static readonly PlayerDrawLayer CaptureTheGem = new LegacyPlayerDrawLayer(nameof(CaptureTheGem), false, DrawPlayer_36_CTG);
		
		/// <summary> Draws the effects of Beetle Armor's Set buffs, if the player currently has any. </summary>
		public static readonly PlayerDrawLayer BeetleBuff = new LegacyPlayerDrawLayer(nameof(BeetleBuff), false, DrawPlayer_37_BeetleBuff);

		/// <summary> Returns whether or not this layer should be rendered for the minimap icon. </summary>
		public virtual bool IsHeadLayer => false;

		/// <summary> Returns the layer's default depth and visibility. This is usually called as a layer is queued for drawing, but modders can call it too for information. </summary>
		/// <param name="drawPlayer"> The player that's currently being drawn. </param>
		/// <param name="visible"> Whether or not this layer will be visible by default. Modders can hide and unhide layers later, if needed. </param>
		/// <param name="constraint"> The constraint that this layer should use by default. Use this to make the layer appear before or after another specific layer. </param>
		public abstract void GetDefaults(PlayerDrawSet drawInfo, out bool visible, out LayerConstraint constraint);

		protected override void Register() => PlayerDrawLayerHooks.Add(this);
	}
}
