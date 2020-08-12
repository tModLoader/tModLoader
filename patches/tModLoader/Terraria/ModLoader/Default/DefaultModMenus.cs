using Terraria.ID;

namespace Terraria.ModLoader.Default
{
	/// <summary>
	/// This is the default modmenu - the one that tML uses and the default one upon entering the game for the first time.
	/// </summary>
	internal class MenutML : ModMenu
	{
	}

	/// <summary>
	/// The Journey's End theme converted into a ModMenu, so that it better fits with the new system.
	/// </summary>
	internal class MenuJourneysEnd : ModMenu
	{
		public override string NameOnMenu => "Journey's End";
	}

	/// <summary>
	/// The Terraria 1.3.5.3 theme converted into a ModMenu, so that it better fits with the new system.
	/// </summary>
	internal class MenuOldVanilla : ModMenu
	{
		public override bool IsAvailable => Main.instance.playOldTile;

		public override string NameOnMenu => "Terraria 1.3.5.3";

		public override int Music => MusicID.Title;
	}
}
