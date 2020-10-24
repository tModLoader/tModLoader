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
		public static readonly PlayerDrawLayer TorsoIndent = CreateVanillaLayer(nameof(TorsoIndent), false, DrawPlayer_extra_TorsoPlus);
		
		/// <summary> Technical layer. Subtracts <see cref="PlayerDrawSet.torsoOffset"/> from <see cref="PlayerDrawSet.Position"/> and <see cref="PlayerDrawSet.ItemLocation"/> vectors' Y axes. </summary>
		public static readonly PlayerDrawLayer TorsoUnindent = CreateVanillaLayer(nameof(TorsoUnindent), false, DrawPlayer_extra_TorsoMinus);
		
		/// <summary> Technical layer. Adds <see cref="PlayerDrawSet.mountOffSet"/>/2 to <see cref="PlayerDrawSet.Position"/> vector's Y axis. </summary>
		public static readonly PlayerDrawLayer MountIndent = CreateVanillaLayer(nameof(MountIndent), false, DrawPlayer_extra_MountPlus);

		/// <summary> Technical layer. Subtracts <see cref="PlayerDrawSet.mountOffSet"/>/2 from <see cref="PlayerDrawSet.Position"/> vector's Y axis. </summary>
		public static readonly PlayerDrawLayer MountUnindent = CreateVanillaLayer(nameof(MountUnindent), false, DrawPlayer_extra_MountMinus);

		//Normal layers

		/// <summary> Draws the player's under-headgear hair. </summary>
		public static readonly PlayerDrawLayer HairBack = CreateVanillaLayer(nameof(HairBack), true, DrawPlayer_01_BackHair);

		/// <summary> Draws Jim's Cloak, if the player is wearing Jim's Leggings (a developer item). </summary>
		public static readonly PlayerDrawLayer JimsCloak = CreateVanillaLayer(nameof(JimsCloak), false, DrawPlayer_01_2_JimsCloak);

		/// <summary> Draws the back textures of the player's head, including armor. </summary>
		public static readonly PlayerDrawLayer HeadBack = CreateVanillaLayer(nameof(HeadBack), true, DrawPlayer_01_3_BackHead);

		/// <summary> Draws the back textures of the player's mount. </summary>
		public static readonly PlayerDrawLayer MountBack = CreateVanillaLayer(nameof(MountBack), false, DrawPlayer_02_MountBehindPlayer);
		
		/// <summary> Draws the Flying Carpet accessory, if the player has it equipped and is using it. </summary>
		public static readonly PlayerDrawLayer Carpet = CreateVanillaLayer(nameof(Carpet), false, DrawPlayer_03_Carpet);
		
		/// <summary> Draws the Step Stool accessory, if the player has it equipped and is using it. </summary>
		public static readonly PlayerDrawLayer PortableStool = CreateVanillaLayer(nameof(PortableStool), false, DrawPlayer_03_PortableStool);
		
		/// <summary> Draws the back textures of the Electrified debuff, if the player has it. </summary>
		public static readonly PlayerDrawLayer ElectrifiedDebuffBack = CreateVanillaLayer(nameof(ElectrifiedDebuffBack), false, DrawPlayer_04_ElectrifiedDebuffBack);
		
		/// <summary> Draws the 'Forbidden Sign' if the player has a full 'Forbidden Armor' set equipped. </summary>
		public static readonly PlayerDrawLayer ForbiddenSetRing = CreateVanillaLayer(nameof(ForbiddenSetRing), false, DrawPlayer_05_ForbiddenSetRing);
		
		/// <summary> Draws a sun above the player's head if they have "Safeman's Sunny Day" headgear equipped. </summary>
		public static readonly PlayerDrawLayer SafemanSun = CreateVanillaLayer(nameof(SafemanSun), false, DrawPlayer_05_2_SafemanSun);

		/// <summary> Draws the back textures of the Webbed debuff, if the player has it. </summary>
		public static readonly PlayerDrawLayer WebbedDebuffBack = CreateVanillaLayer(nameof(WebbedDebuffBack), false, DrawPlayer_06_WebbedDebuffBack);
		
		/// <summary> Draws effects of "Leinfors' Luxury Shampoo", if the player has it equipped. </summary>
		public static readonly PlayerDrawLayer LeinforsHairShampoo = CreateVanillaLayer(nameof(LeinforsHairShampoo), true, DrawPlayer_07_LeinforsHairShampoo);

		/// <summary> Draws the player's held item's backpack. </summary>
		public static readonly PlayerDrawLayer Backpacks = CreateVanillaLayer(nameof(Backpacks), false, DrawPlayer_08_Backpacks);

		/// <summary> Draws the player's back accessories. </summary>
		public static readonly PlayerDrawLayer BackAcc = CreateVanillaLayer(nameof(BackAcc), false, DrawPlayer_10_BackAcc);

		/// <summary> Draws the player's wings. </summary>
		public static readonly PlayerDrawLayer Wings = CreateVanillaLayer(nameof(Wings), false, DrawPlayer_09_Wings);

		/// <summary> Draws the player's balloon accessory, if they have one. </summary>
		public static readonly PlayerDrawLayer BalloonAcc = CreateVanillaLayer(nameof(BalloonAcc), false, DrawPlayer_11_Balloons);

		/// <summary> Draws the player's held item. </summary>
		public static readonly PlayerDrawLayer HeldItem = CreateVanillaLayer(nameof(HeldItem), false, DrawPlayer_27_HeldItem);

		/// <summary> Draws the player's body and leg skin. </summary>
		public static readonly PlayerDrawLayer Skin = CreateVanillaLayer(nameof(Skin), false, DrawPlayer_12_Skin);

		/// <summary> Draws the player's shoes. </summary>
		public static readonly PlayerDrawLayer Shoes = CreateVanillaLayer(nameof(Shoes), false, DrawPlayer_14_Shoes);

		/// <summary> Draws the player's leg armor or pants and shoes. </summary>
		public static readonly PlayerDrawLayer Leggings = CreateVanillaLayer(nameof(Leggings), false, DrawPlayer_13_Leggings);
		
		/// <summary> Draws the longcoat default clothing style, if the player has it. </summary>
		public static readonly PlayerDrawLayer SkinLongCoat = CreateVanillaLayer(nameof(SkinLongCoat), false, DrawPlayer_15_SkinLongCoat);
		
		/// <summary> Draws the currently equipped armor's longcoat, if it has one. </summary>
		public static readonly PlayerDrawLayer ArmorLongCoat = CreateVanillaLayer(nameof(ArmorLongCoat), false, DrawPlayer_16_ArmorLongCoat);

		/// <summary> Draws the player's body armor or shirts. </summary>
		public static readonly PlayerDrawLayer Torso = CreateVanillaLayer(nameof(Torso), false, DrawPlayer_17_Torso);

		/// <summary> Draws the player's off-hand accessory. </summary>
		public static readonly PlayerDrawLayer OffhandAcc = CreateVanillaLayer(nameof(OffhandAcc), false, DrawPlayer_18_OffhandAcc);

		/// <summary> Draws the player's waist accessory. </summary>
		public static readonly PlayerDrawLayer WaistAcc = CreateVanillaLayer(nameof(WaistAcc), false, DrawPlayer_19_WaistAcc);

		/// <summary> Draws the player's neck accessory. </summary>
		public static readonly PlayerDrawLayer NeckAcc = CreateVanillaLayer(nameof(NeckAcc), false, DrawPlayer_20_NeckAcc);

		/// <summary> Draws the player's head, including hair, armor, and etc. </summary>
		public static readonly PlayerDrawLayer Head = CreateVanillaLayer(nameof(Head), true, DrawPlayer_21_Head);

		/// <summary> Draws the player's face accessory. </summary>
		public static readonly PlayerDrawLayer FaceAcc = CreateVanillaLayer(nameof(FaceAcc), false, DrawPlayer_22_FaceAcc);

		/// <summary> Draws the front part of player's front accessory. </summary>
		public static readonly PlayerDrawLayer FrontAccFront = CreateVanillaLayer(nameof(FrontAccFront), false, DrawPlayer_32_FrontAcc_FrontPart);

		/// <summary> Draws the back part of player's front accessory. </summary>
		public static readonly PlayerDrawLayer FrontAccBack = CreateVanillaLayer(nameof(FrontAccBack), false, DrawPlayer_32_FrontAcc_BackPart);

		/// <summary> Draws the front textures of the player's mount. </summary>
		public static readonly PlayerDrawLayer MountFront = CreateVanillaLayer(nameof(MountFront), false, DrawPlayer_23_MountFront);

		/// <summary> Draws the pulley if the player is hanging on a rope. </summary>
		public static readonly PlayerDrawLayer Pulley = CreateVanillaLayer(nameof(Pulley), false, DrawPlayer_24_Pulley);

		/// <summary> Draws the player's shield accessory. </summary>
		public static readonly PlayerDrawLayer Shield = CreateVanillaLayer(nameof(Shield), false, DrawPlayer_25_Shield);

		/// <summary> Draws the player's solar shield if the player has one. </summary>
		public static readonly PlayerDrawLayer SolarShield = CreateVanillaLayer(nameof(SolarShield), false, DrawPlayer_26_SolarShield);

		/// <summary> Draws the player's main arm (including the armor's if applicable), when it should appear over the held item. </summary>
		public static readonly PlayerDrawLayer ArmOverItem = CreateVanillaLayer(nameof(ArmOverItem), false, DrawPlayer_28_ArmOverItem);

		/// <summary> Draws the player's hand on accessory. </summary>
		public static readonly PlayerDrawLayer HandOnAcc = CreateVanillaLayer(nameof(HandOnAcc), false, DrawPlayer_29_OnhandAcc);
		
		/// <summary> Draws the Bladed Glove item, if the player is currently using it. </summary>
		public static readonly PlayerDrawLayer BladedGlove = CreateVanillaLayer(nameof(BladedGlove), false, DrawPlayer_30_BladedGlove);

		/// <summary> Draws the player's held projectile, if it should be drawn in front of the held item and arms. </summary>
		public static readonly PlayerDrawLayer ProjectileOverArm = CreateVanillaLayer(nameof(ProjectileOverArm), false, DrawPlayer_31_ProjectileOverArm);

		/// <summary> Draws the front textures of either Frozen or Webbed debuffs, if the player has one of them. </summary>
		public static readonly PlayerDrawLayer FrozenOrWebbedDebuff = CreateVanillaLayer(nameof(FrozenOrWebbedDebuff), false, DrawPlayer_33_FrozenOrWebbedDebuff);
		
		/// <summary> Draws the front textures of the Electrified debuff, if the player has it. </summary>
		public static readonly PlayerDrawLayer ElectrifiedDebuffFront = CreateVanillaLayer(nameof(ElectrifiedDebuffFront), false, DrawPlayer_34_ElectrifiedDebuffFront);
		
		/// <summary> Draws the textures of the Ice Barrier buff, if the player has it. </summary>
		public static readonly PlayerDrawLayer IceBarrier = CreateVanillaLayer(nameof(IceBarrier), false, DrawPlayer_35_IceBarrier);
		
		/// <summary> Draws a big gem above the player, if the player is currently in possession of a 'Capture The Gem' gem item. </summary>
		public static readonly PlayerDrawLayer CaptureTheGem = CreateVanillaLayer(nameof(CaptureTheGem), false, DrawPlayer_36_CTG);
		
		/// <summary> Draws the effects of Beetle Armor's Set buffs, if the player currently has any. </summary>
		public static readonly PlayerDrawLayer BeetleBuff = CreateVanillaLayer(nameof(BeetleBuff), false, DrawPlayer_37_BeetleBuff);

		/// <summary> Returns whether or not this layer should be rendered for the minimap icon. </summary>
		public virtual bool IsHeadLayer => false;

		/// <summary> Returns the layer's default depth and visibility. This is usually called as a layer is queued for drawing, but modders can call it too for information. </summary>
		/// <param name="drawPlayer"> The player that's currently being drawn. </param>
		/// <param name="visible"> Whether or not this layer will be visible by default. Modders can hide and unhide layers later, if needed. </param>
		/// <param name="constraint"> The constraint that this layer should use by default. Use this to make the layer appear before or after another specific layer. </param>
		public abstract void GetDefaults(PlayerDrawSet drawInfo, out bool visible, out LayerConstraint constraint);

		protected override void Register() => PlayerDrawLayerHooks.Add(this);

		private static PlayerDrawLayer CreateVanillaLayer(string name, bool isHeadLayer, LayerFunction layer) => new LegacyPlayerDrawLayer(null, name, isHeadLayer, layer);
	}
}
