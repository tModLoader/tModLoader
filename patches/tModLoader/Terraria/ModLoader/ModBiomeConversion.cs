using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace Terraria.ModLoader;

/// <summary>
/// This class allows you to define a modded conversion to work with <see cref="ModBlockType.Convert"/> and <see cref="TileLoader.RegisterConversion"/> / <see cref="WallLoader.RegisterConversion"/> <br/>
/// For the moment, this class does not provide any functional hooks, but it can still be used by mods in combination with the aforementioned hooks to create custom conversion behavior
/// </summary>
public abstract class ModBiomeConversion : ModType
{
	/// <summary>
	/// The ID of the biome conversion, equivalent to <see cref="ID.BiomeConversionID"/>.
	/// </summary>
	public int Type { get; internal set; }

	protected sealed override void Register()
	{
		ModTypeLookup<ModBiomeConversion>.Register(this);
		Type = BiomeConversionLoader.Register(this);
	}

	public sealed override void SetupContent()
	{
		SetStaticDefaults();
	}

	/// <summary>
	/// Allows you to run code after the mod's content has been setup (ID sets have been filled, etc). <br/>
	/// Use <see cref="TileLoader.RegisterConversion"/> and <see cref="WallLoader.RegisterConversion"/> in this hook if you want to implement conversions that rely on <see cref="ID.TileID.Sets.Conversion"/> and <see cref="ID.WallID.Sets.Conversion"/> sets being fully populated.
	/// </summary>
	public virtual void PostSetupContent()
	{
	}
}