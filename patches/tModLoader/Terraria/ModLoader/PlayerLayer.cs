using System.Collections.Generic;
using Terraria.DataStructures;
using static Terraria.DataStructures.PlayerDrawLayers;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a DrawLayer for the player, and uses PlayerDrawInfo as its InfoType. Drawing should be done by adding Terraria.DataStructures.DrawData objects to Main.playerDrawData.
	/// </summary>
	[Autoload]
	public abstract class PlayerLayer : DrawLayer<PlayerDrawSet>
	{
		//Technical layers

		/// <summary> Technical layer. Adds <see cref="PlayerDrawSet.torsoOffset"/> to <see cref="PlayerDrawSet.Position"/> and <see cref="PlayerDrawSet.ItemLocation"/> vectors' Y axes. </summary>
		public static readonly PlayerLayer TorsoIndent = CreateVanillaLayer(nameof(TorsoIndent), DrawPlayer_extra_TorsoPlus);
		
		/// <summary> Technical layer. Subtracts <see cref="PlayerDrawSet.torsoOffset"/> from <see cref="PlayerDrawSet.Position"/> and <see cref="PlayerDrawSet.ItemLocation"/> vectors' Y axes. </summary>
		public static readonly PlayerLayer TorsoUnindent = CreateVanillaLayer(nameof(TorsoUnindent), DrawPlayer_extra_TorsoMinus);
		
		/// <summary> Technical layer. Adds <see cref="PlayerDrawSet.mountOffSet"/>/2 to <see cref="PlayerDrawSet.Position"/> vector's Y axis. </summary>
		public static readonly PlayerLayer MountIndent = CreateVanillaLayer(nameof(MountIndent), DrawPlayer_extra_MountPlus);

		/// <summary> Technical layer. Subtracts <see cref="PlayerDrawSet.mountOffSet"/>/2 from <see cref="PlayerDrawSet.Position"/> vector's Y axis. </summary>
		public static readonly PlayerLayer MountUnindent = CreateVanillaLayer(nameof(MountUnindent), DrawPlayer_extra_MountMinus);

		//Normal layers

		/// <summary> Draws the player's under-headgear hair. </summary>
		public static readonly PlayerLayer HairBack = CreateVanillaLayer(nameof(HairBack), DrawPlayer_01_BackHair);

		/// <summary> Draws Jim's Cloak, if the player is wearing Jim's Leggings (a developer item). </summary>
		public static readonly PlayerLayer JimsCloak = CreateVanillaLayer(nameof(JimsCloak), DrawPlayer_01_2_JimsCloak);

		/// <summary> Draws the back textures of the player's head, including armor. </summary>
		public static readonly PlayerLayer HeadBack = CreateVanillaLayer(nameof(HeadBack), DrawPlayer_01_3_BackHead);

		/// <summary> Draws the back textures of the player's mount. </summary>
		public static readonly PlayerLayer MountBack = CreateVanillaLayer(nameof(MountBack), DrawPlayer_02_MountBehindPlayer);
		
		/// <summary> Draws the Flying Carpet accessory, if the player has it equipped and is using it. </summary>
		public static readonly PlayerLayer Carpet = CreateVanillaLayer(nameof(Carpet), DrawPlayer_03_Carpet);
		
		/// <summary> Draws the Step Stool accessory, if the player has it equipped and is using it. </summary>
		public static readonly PlayerLayer PortableStool = CreateVanillaLayer(nameof(PortableStool), DrawPlayer_03_PortableStool);
		
		/// <summary> Draws the back textures of the Electrified debuff, if the player has it. </summary>
		public static readonly PlayerLayer ElectrifiedDebuffBack = CreateVanillaLayer(nameof(ElectrifiedDebuffBack), DrawPlayer_04_ElectrifiedDebuffBack);
		
		/// <summary> Draws the 'Forbidden Sign' if the player has a full 'Forbidden Armor' set equipped. </summary>
		public static readonly PlayerLayer ForbiddenSetRing = CreateVanillaLayer(nameof(ForbiddenSetRing), DrawPlayer_05_ForbiddenSetRing);
		
		/// <summary> Draws a sun above the player's head if they have "Safeman's Sunny Day" headgear equipped. </summary>
		public static readonly PlayerLayer SafemanSun = CreateVanillaLayer(nameof(SafemanSun), DrawPlayer_05_2_SafemanSun);

		/// <summary> Draws the back textures of the Webbed debuff, if the player has it. </summary>
		public static readonly PlayerLayer WebbedDebuffBack = CreateVanillaLayer(nameof(WebbedDebuffBack), DrawPlayer_06_WebbedDebuffBack);
		
		/// <summary> Draws effects of "Leinfors' Luxury Shampoo", if the player has it equipped. </summary>
		public static readonly PlayerLayer LeinforsHairShampoo = CreateVanillaLayer(nameof(LeinforsHairShampoo), DrawPlayer_07_LeinforsHairShampoo);

		/// <summary> Draws the player's held item's backpack. </summary>
		public static readonly PlayerLayer Backpacks = CreateVanillaLayer(nameof(Backpacks), DrawPlayer_08_Backpacks);

		/// <summary> Draws the player's back accessories. </summary>
		public static readonly PlayerLayer BackAcc = CreateVanillaLayer(nameof(BackAcc), DrawPlayer_09_BackAc);

		/// <summary> Draws the player's wings. </summary>
		public static readonly PlayerLayer Wings = CreateVanillaLayer(nameof(Wings), DrawPlayer_10_Wings);

		/// <summary> Draws the player's balloon accessory, if they have one. </summary>
		public static readonly PlayerLayer BalloonAcc = CreateVanillaLayer(nameof(BalloonAcc), DrawPlayer_11_Balloons);

		/// <summary> Draws the player's held item. </summary>
		public static readonly PlayerLayer HeldItem = CreateVanillaLayer(nameof(HeldItem), DrawPlayer_27_HeldItem);

		/// <summary> Draws the player's body and leg skin. </summary>
		public static readonly PlayerLayer Skin = CreateVanillaLayer(nameof(Skin), DrawPlayer_12_Skin);

		/// <summary> Draws the player's shoes. </summary>
		public static readonly PlayerLayer Shoes = CreateVanillaLayer(nameof(Shoes), DrawPlayer_14_Shoes);

		/// <summary> Draws the player's leg armor or pants and shoes. </summary>
		public static readonly PlayerLayer Leggings = CreateVanillaLayer(nameof(Leggings), DrawPlayer_13_Leggings);
		
		/// <summary> Draws the longcoat default clothing style, if the player has it. </summary>
		public static readonly PlayerLayer SkinLongCoat = CreateVanillaLayer(nameof(SkinLongCoat), DrawPlayer_15_SkinLongCoat);
		
		/// <summary> Draws the currently equipped armor's longcoat, if it has one. </summary>
		public static readonly PlayerLayer ArmorLongCoat = CreateVanillaLayer(nameof(ArmorLongCoat), DrawPlayer_16_ArmorLongCoat);

		/// <summary> Draws the player's body armor or shirts. </summary>
		public static readonly PlayerLayer Torso = CreateVanillaLayer(nameof(Torso), DrawPlayer_17_Torso);

		/// <summary> Draws the player's off-hand accessory. </summary>
		public static readonly PlayerLayer OffhandAcc = CreateVanillaLayer(nameof(OffhandAcc), DrawPlayer_18_OffhandAcc);

		/// <summary> Draws the player's waist accessory. </summary>
		public static readonly PlayerLayer WaistAcc = CreateVanillaLayer(nameof(WaistAcc), DrawPlayer_19_WaistAcc);

		/// <summary> Draws the player's neck accessory. </summary>
		public static readonly PlayerLayer NeckAcc = CreateVanillaLayer(nameof(NeckAcc), DrawPlayer_20_NeckAcc);

		/// <summary> Draws the player's head, including hair, armor, and etc. </summary>
		public static readonly PlayerLayer Head = CreateVanillaLayer(nameof(Head), DrawPlayer_21_Head);

		/// <summary> Draws the player's face accessory. </summary>
		public static readonly PlayerLayer FaceAcc = CreateVanillaLayer(nameof(FaceAcc), DrawPlayer_22_FaceAcc);

		/// <summary> Draws the player's front accessory. </summary>
		public static readonly PlayerLayer FrontAcc = CreateVanillaLayer(nameof(FrontAcc), DrawPlayer_32_FrontAcc);

		/// <summary> Draws the front textures of the player's mount. </summary>
		public static readonly PlayerLayer MountFront = CreateVanillaLayer(nameof(MountFront), DrawPlayer_23_MountFront);

		/// <summary> Draws the pulley if the player is hanging on a rope. </summary>
		public static readonly PlayerLayer Pulley = CreateVanillaLayer(nameof(Pulley), DrawPlayer_24_Pulley);

		/// <summary> Draws the player's shield accessory. </summary>
		public static readonly PlayerLayer Shield = CreateVanillaLayer(nameof(Shield), DrawPlayer_25_Shield);

		/// <summary> Draws the player's solar shield if the player has one. </summary>
		public static readonly PlayerLayer SolarShield = CreateVanillaLayer(nameof(SolarShield), DrawPlayer_26_SolarShield);

		/// <summary> Draws the player's main arm (including the armor's if applicable), when it should appear over the held item. </summary>
		public static readonly PlayerLayer ArmOverItem = CreateVanillaLayer(nameof(ArmOverItem), DrawPlayer_28_ArmOverItem);

		/// <summary> Draws the player's hand on accessory. </summary>
		public static readonly PlayerLayer HandOnAcc = CreateVanillaLayer(nameof(HandOnAcc), DrawPlayer_29_OnhandAcc);
		
		/// <summary> Draws the Bladed Glove item, if the player is currently using it. </summary>
		public static readonly PlayerLayer BladedGlove = CreateVanillaLayer(nameof(BladedGlove), DrawPlayer_30_BladedGlove);

		/// <summary> Draws the player's held projectile, if it should be drawn in front of the held item and arms. </summary>
		public static readonly PlayerLayer ProjectileOverArm = CreateVanillaLayer(nameof(ProjectileOverArm), DrawPlayer_31_ProjectileOverArm);

		/// <summary> Draws the front textures of either Frozen or Webbed debuffs, if the player has one of them. </summary>
		public static readonly PlayerLayer FrozenOrWebbedDebuff = CreateVanillaLayer(nameof(FrozenOrWebbedDebuff), DrawPlayer_33_FrozenOrWebbedDebuff);
		
		/// <summary> Draws the front textures of the Electrified debuff, if the player has it. </summary>
		public static readonly PlayerLayer ElectrifiedDebuffFront = CreateVanillaLayer(nameof(ElectrifiedDebuffFront), DrawPlayer_34_ElectrifiedDebuffFront);
		
		/// <summary> Draws the textures of the Ice Barrier buff, if the player has it. </summary>
		public static readonly PlayerLayer IceBarrier = CreateVanillaLayer(nameof(IceBarrier), DrawPlayer_35_IceBarrier);
		
		/// <summary> Draws a big gem above the player, if the player is currently in possession of a 'Capture The Gem' gem item. </summary>
		public static readonly PlayerLayer CaptureTheGem = CreateVanillaLayer(nameof(CaptureTheGem), DrawPlayer_36_CTG);
		
		/// <summary> Draws the effects of Beetle Armor's Set buffs, if the player currently has any. </summary>
		public static readonly PlayerLayer BeetleBuff = CreateVanillaLayer(nameof(BeetleBuff), DrawPlayer_37_BeetleBuff);

		/// <summary> Used for automatically setting up the layer every time a player is drawn. Set <see cref="DrawLayer.depth"/>, and return whether or not you want the layer to be added at this moment in time. </summary>
		/// <param name="drawPlayer"> The player that's currently being drawn. </param>
		/// <param name="vanillaLayers"> A readonly list of to-be-rendered vanilla layers. </param>
		public abstract bool Setup(Player drawPlayer, IReadOnlyList<PlayerLayer> vanillaLayers);

		protected override void Register() => PlayerLayerHooks.Add(this);

		private static PlayerLayer CreateVanillaLayer(string name, LayerFunction layer) => new LegacyPlayerLayer(null, name, layer);
	}
}
