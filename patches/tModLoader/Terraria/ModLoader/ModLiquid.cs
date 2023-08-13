using System;
using Terraria.GameContent.Liquid;
using Terraria.ID;

namespace Terraria.ModLoader;

public abstract class ModLiquid : ModBlockType
{
	public int WaterfallLength {
		get => LiquidRenderer.WATERFALL_LENGTH[Type];
		set {
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), "Waterfall Length must not be inferior to 0.");
			LiquidRenderer.WATERFALL_LENGTH[Type] = value;
		}
	}

	public float DefaultOpacity {
		get => LiquidRenderer.DEFAULT_OPACITY[Type];
		set {
			if (value < 0 || value > 1)
				throw new ArgumentOutOfRangeException(nameof(value), "Default opacity must be between 0 and 1.");
			LiquidRenderer.DEFAULT_OPACITY[Type] = value;
		}
	}

	public byte WaveMaskStrength {
		get => LiquidRenderer.WAVE_MASK_STRENGTH[Type + 1];
		set => LiquidRenderer.WAVE_MASK_STRENGTH[Type + 1] = value;
	}
	
	public byte ViscosityMask {
		get => LiquidRenderer.VISCOSITY_MASK[Type + 1];
		set => LiquidRenderer.VISCOSITY_MASK[Type + 1] = value;
	}

	public override string LocalizationCategory => "Liquids";

	protected sealed override void Register()
	{
		ModTypeLookup<ModLiquid>.Register(this);
		Type = (ushort)LiquidLoader.ReserveLiquidID();
		LiquidLoader.liquids.Add(this);
	}

	public sealed override void SetupContent()
	{
		SetStaticDefaults();

		LiquidID.Search.Add(FullName, Type);
	}

	public override void SetStaticDefaults()
	{
		WaterfallLength = 10;
		DefaultOpacity = 0.6f;
	}
}