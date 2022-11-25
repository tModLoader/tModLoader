using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace Terraria;

public partial class Gore
{
	public Vector2 drawOffset;

	internal int realType;

	public ModGore ModGore { get; private set; }

	private void ResetNewFields()
	{
		drawOffset = Vector2.Zero;
		realType = 0;
	}
}
