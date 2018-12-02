using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a layer of the drawing of an object, using a certain type of InfoType to help with its drawing.
	/// </summary>
	/// <typeparam name="InfoType"></typeparam>
	public abstract class DrawLayer<InfoType>
	{
		/// <summary>
		/// The name of the mod to which this DrawLayer belongs.
		/// </summary>
		public readonly string mod;
		/// <summary>
		/// The name which identifies this DrawLayer.
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// The parent of this DrawLayer. If the parent is not drawn, this layer will not be drawn either. Defaults to null, which skips the parent check.
		/// </summary>
		public readonly DrawLayer<InfoType> parent;
		/// <summary>
		/// The delegate of this method, which can either do the actual drawing or add draw data, depending on what kind of layer this is.
		/// </summary>
		public readonly Action<InfoType> layer;
		/// <summary>
		/// Whether or not this DrawLayer should be drawn. For vanilla layers, this will be set to true before all drawing-related hooks are called. For modded layers, you must set this to true or false yourself.
		/// </summary>
		public bool visible = true;

		/// <summary>
		/// Creates a DrawLayer with the given mod name, identifier name, and drawing action.
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="name"></param>
		/// <param name="layer"></param>
		protected DrawLayer(string mod, string name, Action<InfoType> layer) {
			this.mod = mod;
			this.Name = name;
			this.parent = null;
			this.layer = layer;
		}

		/// <summary>
		/// Creates a DrawLayer with the given mod name, identifier name, parent layer, and drawing action.
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="layer"></param>
		protected DrawLayer(string mod, string name, DrawLayer<InfoType> parent, Action<InfoType> layer) {
			this.mod = mod;
			this.Name = name;
			this.parent = parent;
			this.layer = layer;
		}

		/// <summary>
		/// Whether or not this layer should be drawn. Returns false if visible is false. If layerList is of type List<DrawLayer<InfoType>> and parent is not null, this will also return false if the parent is not drawn.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="layers"></param>
		/// <returns></returns>
		public bool ShouldDraw<T>(IList<T> layers) where T : DrawLayer<InfoType> {
			if (!this.visible) {
				return false;
			}
			DrawLayer<InfoType> parentLayer = this.parent;
			while (parentLayer != null) {
				if (!parentLayer.visible || !layers.Contains((T)parentLayer)) {
					return false;
				}
				parentLayer = parentLayer.parent;
			}
			return true;
		}

		/// <summary>
		/// Invokes this DrawLayer's layer action.
		/// </summary>
		/// <param name="drawInfo"></param>
		public virtual void Draw(ref InfoType drawInfo) {
			this.layer(drawInfo);
		}
	}

	/// <summary>
	/// This class represents a DrawLayer for the player, and uses PlayerDrawInfo as its InfoType. Drawing should be done by adding Terraria.DataStructures.DrawData objects to Main.playerDrawData.
	/// </summary>
	public class PlayerLayer : DrawLayer<PlayerDrawInfo>
	{
		/// <summary>
		/// Draws the player's hair. To be honest this layer seems kind of useless.
		/// </summary>
		public static readonly PlayerLayer HairBack = CreateVanillaLayer("HairBack");
		/// <summary>
		/// Draws the back textures of the player's mount. Also draws the player's magic carpet.
		/// </summary>
		public static readonly PlayerLayer MountBack = CreateVanillaLayer("MountBack");
		/// <summary>
		/// Draws miscellaneous effects behind the player.
		/// </summary>
		public static readonly PlayerLayer MiscEffectsBack = CreateVanillaLayer("MiscEffectsBack");
		/// <summary>
		/// Draws the player's back accessory and held item's backpack.
		/// </summary>
		public static readonly PlayerLayer BackAcc = CreateVanillaLayer("BackAcc");
		/// <summary>
		/// Draws the layer's wings.
		/// </summary>
		public static readonly PlayerLayer Wings = CreateVanillaLayer("Wings");
		/// <summary>
		/// Draws the player's balloon accessory.
		/// </summary>
		public static readonly PlayerLayer BalloonAcc = CreateVanillaLayer("BalloonAcc");
		/// <summary>
		/// Draws the player's body and leg skin.
		/// </summary>
		public static readonly PlayerLayer Skin = CreateVanillaLayer("Skin");
		/// <summary>
		/// Draws the player's leg armor or pants and shoes.
		/// </summary>
		public static readonly PlayerLayer Legs = CreateVanillaLayer("Legs");
		/// <summary>
		/// Draws the player's shoe accessory.
		/// </summary>
		public static readonly PlayerLayer ShoeAcc = CreateVanillaLayer("ShoeAcc");
		/// <summary>
		/// Draws the player's body armor or shirts.
		/// </summary>
		public static readonly PlayerLayer Body = CreateVanillaLayer("Body");
		/// <summary>
		/// Draws the player's hand off accessory.
		/// </summary>
		public static readonly PlayerLayer HandOffAcc = CreateVanillaLayer("HandOffAcc");
		/// <summary>
		/// Draws the player's waist accessory.
		/// </summary>
		public static readonly PlayerLayer WaistAcc = CreateVanillaLayer("WaistAcc");
		/// <summary>
		/// Draws the player's neck accessory.
		/// </summary>
		public static readonly PlayerLayer NeckAcc = CreateVanillaLayer("NeckAcc");
		/// <summary>
		/// Draws the player's face and eyes.
		/// </summary>
		public static readonly PlayerLayer Face = CreateVanillaLayer("Face");
		/// <summary>
		/// Draws the player's hair.
		/// </summary>
		public static readonly PlayerLayer Hair = CreateVanillaLayer("Hair");
		/// <summary>
		/// Draws the player's head armor.
		/// </summary>
		public static readonly PlayerLayer Head = CreateVanillaLayer("Head");
		/// <summary>
		/// Draws the player's face accessory.
		/// </summary>
		public static readonly PlayerLayer FaceAcc = CreateVanillaLayer("FaceAcc");
		/// <summary>
		/// Draws the front textures of the player's mount. Also draws the pulley if the player is hanging on a rope.
		/// </summary>
		public static readonly PlayerLayer MountFront = CreateVanillaLayer("MountFront");
		/// <summary>
		/// Draws the player's shield accessory.
		/// </summary>
		public static readonly PlayerLayer ShieldAcc = CreateVanillaLayer("ShieldAcc");
		/// <summary>
		/// Draws the player's solar shield if the player has one.
		/// </summary>
		public static readonly PlayerLayer SolarShield = CreateVanillaLayer("SolarShield");
		/// <summary>
		/// Draws the player's held projectile if it should be drawn behind the held item and arms.
		/// </summary>
		public static readonly PlayerLayer HeldProjBack = CreateVanillaLayer("HeldProjBack");
		/// <summary>
		/// Draws the player's held item.
		/// </summary>
		public static readonly PlayerLayer HeldItem = CreateVanillaLayer("HeldItem");
		/// <summary>
		/// Draws the player's arms (including the armor's arms if applicable).
		/// </summary>
		public static readonly PlayerLayer Arms = CreateVanillaLayer("Arms");
		/// <summary>
		/// Draws the player's hand on accessory. Also draws the player's held item if the player is in the middle of using a claw item.
		/// </summary>
		public static readonly PlayerLayer HandOnAcc = CreateVanillaLayer("HandOnAcc");
		/// <summary>
		/// Draws the player's held projectile if it should be drawn in front of the held item and arms.
		/// </summary>
		public static readonly PlayerLayer HeldProjFront = CreateVanillaLayer("HeldProjFront");
		/// <summary>
		/// Draws the player's front accessory.
		/// </summary>
		public static readonly PlayerLayer FrontAcc = CreateVanillaLayer("FrontAcc");
		/// <summary>
		/// Draws miscellaneous effects in front of the player.
		/// </summary>
		public static readonly PlayerLayer MiscEffectsFront = CreateVanillaLayer("MiscEffectsFront");

		/// <summary>
		/// Creates a PlayerLayer with the given mod name, identifier name, and drawing action.
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="name"></param>
		/// <param name="layer"></param>
		public PlayerLayer(string mod, string name, Action<PlayerDrawInfo> layer)
			: base(mod, name, layer) {
		}

		/// <summary>
		/// Creates a PlayerLayer with the given mod name, identifier name, parent layer, and drawing action.
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="layer"></param>
		public PlayerLayer(string mod, string name, PlayerLayer parent, Action<PlayerDrawInfo> layer)
			: base(mod, name, parent, layer) {
		}

		private static PlayerLayer CreateVanillaLayer(string name) {
			return new PlayerLayer("Terraria", name, _ => { });
		}
	}

	/// <summary>
	/// This class represents a DrawLayer for the player's map icon, and uses PlayerDrawHeadInfo as its InfoType. Drawing should be done directly through drawInfo.spriteBatch.
	/// </summary>
	public class PlayerHeadLayer : DrawLayer<PlayerHeadDrawInfo>
	{
		/// <summary>
		/// Draws the player's face and eyes.
		/// </summary>
		public static readonly PlayerHeadLayer Head = CreateVanillaLayer("Head");
		/// <summary>
		/// Draws the player's hair.
		/// </summary>
		public static readonly PlayerHeadLayer Hair = CreateVanillaLayer("Hair");
		/// <summary>
		/// Draws the player's alternate (hat) hair.
		/// </summary>
		public static readonly PlayerHeadLayer AltHair = CreateVanillaLayer("AltHair");
		/// <summary>
		/// Draws the player's head armor.
		/// </summary>
		public static readonly PlayerHeadLayer Armor = CreateVanillaLayer("Armor");
		/// <summary>
		/// Draws the player's face accessory.
		/// </summary>
		public static readonly PlayerHeadLayer FaceAcc = CreateVanillaLayer("FaceAcc");

		/// <summary>
		/// Creates a PlayerHeadLayer with the given mod name, identifier name, and drawing action.
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="name"></param>
		/// <param name="layer"></param>
		public PlayerHeadLayer(string mod, string name, Action<PlayerHeadDrawInfo> layer)
			: base(mod, name, layer) {
		}

		/// <summary>
		/// Creates a PlayerHeadLayer with the given mod name, identifier name, parent layer, and drawing action.
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="layer"></param>
		public PlayerHeadLayer(string mod, string name, PlayerHeadLayer parent, Action<PlayerHeadDrawInfo> layer)
			: base(mod, name, parent, layer) {
		}

		private static PlayerHeadLayer CreateVanillaLayer(string name) {
			return new PlayerHeadLayer("Terraria", name, _ => { });
		}
	}
}
