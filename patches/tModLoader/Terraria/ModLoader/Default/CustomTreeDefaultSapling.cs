using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default;

public class CustomTreeDefaultSapling : CustomTreeSapling
{
	public override string Name => Tree.Name + "Sapling";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		AddMapEntry(new Color(163, 116, 81), DefaultMapNameLocalization);
	}
}
