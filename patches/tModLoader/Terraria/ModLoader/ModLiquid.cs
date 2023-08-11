namespace Terraria.ModLoader;

public abstract class ModLiquid : ModBlockType
{
	private int waterfallLength = 10;
	private float defaultOpacity = 0.6f;

	public int WaterfallLength {
		get => waterfallLength;
		set {
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), "Waterfall Length must not be inferior to 0.");
			waterfallLength = value;
		}
	}

	public float DefaultOpacity {
		get => defaultOpacity;
		set {
			if (value < 0 || value > 1)
				throw new ArgumentOutOfRangeException(nameof(value), "Default opacity must be between 0 and 1.");
			defaultOpacity = value;
		}
	}

	public byte WaveMaskStrength { get; set; } = 0;
	
	public byte ViscosityMask { get; set; } = 0;

	public override string LocalizationCategory => "Liquids";

	protected sealed override void Register()
	{
		ModTypeLookup<ModLiquid>.Register(this);
		Type = (ushort)LiquidLoader.ReserveLiquidID();
		LiquidLoader.liquids.Add(this);
	}
}