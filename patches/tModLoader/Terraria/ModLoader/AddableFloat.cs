namespace Terraria.ModLoader;

public readonly struct AddableFloat
{
	public static AddableFloat Zero = new(0f);

	public float Value { get; }

	private AddableFloat(float f)
	{
		Value = f;
	}

	public static AddableFloat operator +(AddableFloat f1, AddableFloat f2) => new(f1.Value + f2.Value);
	public static AddableFloat operator +(AddableFloat f1, float f2) => new(f1.Value + f2);
}