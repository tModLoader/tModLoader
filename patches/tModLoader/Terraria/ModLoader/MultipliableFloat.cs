namespace Terraria.ModLoader;

public readonly struct MultipliableFloat
{
	public static MultipliableFloat One = new(1f);

	public float Value { get; } = 1f;

	public MultipliableFloat() { }

	private MultipliableFloat(float f)
	{
		Value = f;
	}

	public static MultipliableFloat operator *(MultipliableFloat f1, MultipliableFloat f2) => new(f1.Value * f2.Value);
	public static MultipliableFloat operator *(MultipliableFloat f1, float f2) => new(f1.Value * f2);
	public static MultipliableFloat operator /(MultipliableFloat f1, float f2) => new(f1.Value / f2);
}