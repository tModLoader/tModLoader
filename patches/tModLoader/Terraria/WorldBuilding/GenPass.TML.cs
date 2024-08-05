namespace Terraria.WorldBuilding;

partial class GenPass
{
	public bool Enabled { get; private set; } = true;

	public void Disable() => Enabled = false;

	internal void Reset() { Enabled = true; }
}
