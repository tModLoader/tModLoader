using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public class ModInvasion : ModEvent
	{
		public virtual Texture2D Icon { get; }

		public float Progress { get; set; }

		IDictionary<int, float> spawnPool = new Dictionary<int, float>();


	}
}
