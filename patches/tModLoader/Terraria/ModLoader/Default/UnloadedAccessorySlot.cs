namespace Terraria.ModLoader.Default
{
	[Autoload(false)]
	public class UnloadedAccessorySlot : ModAccessorySlot
	{
		public override string Name { get; }

		internal UnloadedAccessorySlot(int slot, string oldName) {
			type = slot;
			Name = oldName;
		}

		public override bool IsSlotValid() { 
			return false;
		}

		// Only display unloaded slots if in singleplayer. Don't need people carrying items in to multiplayer they shouldn't have :(
		public override bool IsSlotVisibleButNotValid() {
			if (Main.netMode == 1)
				return false;

			return base.IsSlotVisibleButNotValid();
		}
	}
}