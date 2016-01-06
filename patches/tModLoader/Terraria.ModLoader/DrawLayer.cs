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
