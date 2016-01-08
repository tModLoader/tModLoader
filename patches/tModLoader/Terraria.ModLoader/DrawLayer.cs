using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public abstract class DrawLayer<InfoType>
	{
		public readonly string mod;
		public readonly string Name;
		public readonly DrawLayer<InfoType> parent;
		public readonly Action<InfoType> layer;
		public bool visible = true;

		public DrawLayer(string mod, string name, Action<InfoType> layer)
		{
			this.mod = mod;
			this.Name = name;
			this.parent = null;
			this.layer = layer;
		}

		public DrawLayer(string mod, string name, DrawLayer<InfoType> parent, Action<InfoType> layer)
		{
			this.mod = mod;
			this.Name = name;
			this.parent = parent;
			this.layer = layer;
		}

		public bool ShouldDraw(object layerList)
		{
			if (!this.visible)
			{
				return false;
			}
			List<DrawLayer<InfoType>> layers = layerList as List<DrawLayer<InfoType>>;
			if (layers == null)
			{
				return true;
			}
			DrawLayer<InfoType> parentLayer = this.parent;
			while (parentLayer != null)
			{
				if (!parentLayer.visible || !layers.Contains(parentLayer))
				{
					return false;
				}
				parentLayer = parentLayer.parent;
			}
			return true;
		}

		public void Draw(InfoType drawInfo)
		{
			this.layer(drawInfo);
		}

		internal static void EmptyDelegate(InfoType drawInfo)
		{
		}
	}

	public class PlayerLayer : DrawLayer<PlayerDrawInfo>
	{
		public static readonly PlayerLayer HairBack = CreateVanillaLayer("HairBack");
		public static readonly PlayerLayer MountBack = CreateVanillaLayer("MountBack");
		public static readonly PlayerLayer MiscEffectsBack = CreateVanillaLayer("MiscEffectsBack");
		public static readonly PlayerLayer BackAcc = CreateVanillaLayer("BackAcc");
		public static readonly PlayerLayer Wings = CreateVanillaLayer("Wings");
		public static readonly PlayerLayer BalloonAcc = CreateVanillaLayer("BalloonAcc");
		public static readonly PlayerLayer Skin = CreateVanillaLayer("Skin");
		public static readonly PlayerLayer Legs = CreateVanillaLayer("Legs");
		public static readonly PlayerLayer ShoeAcc = CreateVanillaLayer("ShoeAcc");
		public static readonly PlayerLayer Body = CreateVanillaLayer("Body");
		public static readonly PlayerLayer HandOffAcc = CreateVanillaLayer("HandOffAcc");
		public static readonly PlayerLayer WaistAcc = CreateVanillaLayer("WaistAcc");
		public static readonly PlayerLayer NeckAcc = CreateVanillaLayer("NeckAcc");
		public static readonly PlayerLayer Face = CreateVanillaLayer("Face");
		public static readonly PlayerLayer Hair = CreateVanillaLayer("Hair");
		public static readonly PlayerLayer Head = CreateVanillaLayer("Head");
		public static readonly PlayerLayer FaceAcc = CreateVanillaLayer("FaceAcc");
		public static readonly PlayerLayer MountFront = CreateVanillaLayer("MountFront");
		public static readonly PlayerLayer ShieldAcc = CreateVanillaLayer("ShieldAcc");
		public static readonly PlayerLayer SolarShield = CreateVanillaLayer("SolarShield");
		public static readonly PlayerLayer HeldProjBack = CreateVanillaLayer("HeldProjBack");
		public static readonly PlayerLayer HeldItem = CreateVanillaLayer("HeldItem");
		public static readonly PlayerLayer Arms = CreateVanillaLayer("Arms");
		public static readonly PlayerLayer HandOnAcc = CreateVanillaLayer("HandOnAcc");
		public static readonly PlayerLayer HeldProjFront = CreateVanillaLayer("HeldProjFront");
		public static readonly PlayerLayer FrontAcc = CreateVanillaLayer("FrontAcc");
		public static readonly PlayerLayer MiscEffectsFront = CreateVanillaLayer("MiscEffectsFront");

		public PlayerLayer(string mod, string name, Action<PlayerDrawInfo> layer)
			: base(mod, name, layer)
		{
		}

		public PlayerLayer(string mod, string name, PlayerLayer parent, Action<PlayerDrawInfo> layer)
			: base(mod, name, parent, layer)
		{
		}

		private static PlayerLayer CreateVanillaLayer(string name)
		{
			return new PlayerLayer("Terraria", name, EmptyDelegate);
		}
	}

	public class PlayerHeadLayer : DrawLayer<PlayerHeadDrawInfo>
	{
		public static readonly PlayerHeadLayer Head = CreateVanillaLayer("Head");
		public static readonly PlayerHeadLayer Hair = CreateVanillaLayer("Hair");
		public static readonly PlayerHeadLayer AltHair = CreateVanillaLayer("AltHair");
		public static readonly PlayerHeadLayer Armor = CreateVanillaLayer("Armor");
		public static readonly PlayerHeadLayer FaceAcc = CreateVanillaLayer("FaceAcc");

		public PlayerHeadLayer(string mod, string name, Action<PlayerHeadDrawInfo> layer)
			: base(mod, name, layer)
		{
		}

		public PlayerHeadLayer(string mod, string name, PlayerHeadLayer parent, Action<PlayerHeadDrawInfo> layer)
			: base(mod, name, parent, layer)
		{
		}

		private static PlayerHeadLayer CreateVanillaLayer(string name)
		{
			return new PlayerHeadLayer("Terraria", name, EmptyDelegate);
		}
	}
}
