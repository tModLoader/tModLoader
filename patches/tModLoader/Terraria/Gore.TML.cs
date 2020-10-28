using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Gore
	{
		public Vector2 drawOffset;

		internal int realType;

		public ModGore ModGore { get; internal set; }

		internal void ResetNewFields() {
			drawOffset = Vector2.Zero;
			realType = 0;
		}
	}
}
