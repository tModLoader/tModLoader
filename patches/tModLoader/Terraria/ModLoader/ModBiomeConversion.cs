using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace Terraria.ModLoader;

/// <summary>
/// This class allows you to define a modded conversion to work with <see cref="ModBlockType.Convert"/> and <see cref="TileLoader.RegisterConversion"/> / <see cref="WallLoader.RegisterConversion"/> <br/>
/// For the moment, this class does not provide any functional hooks, but it can still be used by mods in combination with the aforementionned hooks to create custom conversion behavior
/// </summary>
public abstract class ModBiomeConversion : ModType
{
	/// <summary>
	/// The ID of the biome conversion
	/// </summary>
	public int Type { get; internal set; }

	protected sealed override void Register()
	{
		Type = BiomeConversionLoader.Register(this);
	}

	public sealed override void SetupContent()
	{
		SetStaticDefaults();
	}
}