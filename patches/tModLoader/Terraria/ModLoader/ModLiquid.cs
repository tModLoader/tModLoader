using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Liquid;
using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>
/// This class represents a type of liquid that can be added by a mod. Only one instance of this class will ever exist for each type of liquid that is added. Any hooks that are called will be called by the instance corresponding to the liquid type. This is to prevent the game from using a massive amount of memory storing liquid instances.<br />
/// </summary>
public abstract class ModLiquid : ModBlockType
{
	/// <summary> The block texture of this liquid. Used for some slope framing and when liquids are rendered in retro lighting modes. </summary>
	public virtual string BlockTexture => Texture + "_Block";

	/// <summary> The slope texture of this liquid. Used for rendering the liquids next to tile slopes. </summary>
	public virtual string SlopeTexture => Texture + "_Slope";

	protected sealed override void Register()
	{
		ModTypeLookup<ModLiquid>.Register(this);
		Type = (ushort)LiquidLoader.ReserveLiquidID();
		LiquidLoader.liquids.Add(this);
	}

	public sealed override void SetupContent()
	{
		LiquidLoader.LiquidAssets[Type] = ModContent.Request<Texture2D>(Texture);
		LiquidLoader.LiquidBlockAssets[Type] = ModContent.Request<Texture2D>(BlockTexture);
		LiquidLoader.LiquidSlopeAssets[Type] = ModContent.Request<Texture2D>(SlopeTexture);
		LiquidRenderer.WATERFALL_LENGTH[Type] = 10;
		LiquidRenderer.DEFAULT_OPACITY[Type] = 0.6f;
		LiquidRenderer.VISCOSITY_MASK[Type] = 0;
		SetStaticDefaults();
	}

	#region ModBlockTypeOverrides
	public override string LocalizationCategory => "Liquids";

	/// <summary>
	/// Allows you to modify the properties after initial loading has completed.
	/// <br /> This is where you would set the properties of this liquid. Many properties are stored as arrays throughout Terraria's code.
	/// <br /> For example:
	/// <list type="bullet">
	/// <item> LiquidRenderer.WATERFALL_LENGTH[Type] = 10; </item>
	/// <item> LiquidRenderer.DEFAULT_OPACITY[Type] = 0.6f; </item>
	/// <item> LiquidRenderer.VISCOSITY_MASK[Type] = 0; </item>
	/// </list>
	/// </summary>
	public override void SetStaticDefaults()
	{
	}

	/// <inheritdoc/>
	public override void AddMapEntry(Color color, LocalizedText name = null, Func<string, int, int, string> nameFunc = null)
	{
		AddMapEntryToMapLoader(MapLoader.liquidEntries, color, name, nameFunc);
	}
	#endregion
}
