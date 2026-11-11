namespace Terraria.ModLoader.Default;

[Autoload(false)]
public class UnloadedAccessorySlot : ModAccessorySlot
{
	public override string Name { get; }

	internal UnloadedAccessorySlot(int slot, string oldName)
	{
		Type = slot;
		Name = oldName;
	}

	public override bool IsEnabled() => false;
}