using System;

namespace Terraria.ModLoader
{
	public class AutoloadHead : Attribute
	{
		public readonly string texture;

		public AutoloadHead(string texture = null)
		{
			this.texture = texture;
		}
	}
	
	public class AutoloadBossHead : Attribute
	{
		public readonly string texture;

		public AutoloadBossHead(string texture = null)
		{
			this.texture = texture;
		}
	}
}
