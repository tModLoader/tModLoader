using Terraria.ModLoader;

public class ExtraJumpTest : ModPlayer
{
	public void MethodA()
	{
		Player.GetJumpState(ExtraJump.CloudInABottle).Available = true;
		Player.GetJumpState(ExtraJump.SandstormInABottle).Available = true;
		Player.GetJumpState(ExtraJump.BlizzardInABottle).Available = true;
		Player.GetJumpState(ExtraJump.FartInAJar).Available = true;
		Player.GetJumpState(ExtraJump.TsunamiInABottle).Available = true;
		Player.GetJumpState(ExtraJump.UnicornMount).Available = true;
		Player.GetJumpState(ExtraJump.SantankMount).Available = true;
		Player.GetJumpState(ExtraJump.GoatMount).Available = true;
		Player.GetJumpState(ExtraJump.BasiliskMount).Available = true;

#if COMPILE_ERROR
		// Cannot port assignment to hasJumpOption_X
		Player.GetJumpState(ExtraJump.CloudInABottle).Enabled = true/* tModPorter Suggestion: Call Enable() if setting this to true, otherwise call Disable(). */;
		Player.GetJumpState(ExtraJump.SandstormInABottle).Enabled = true/* tModPorter Suggestion: Call Enable() if setting this to true, otherwise call Disable(). */;
		Player.GetJumpState(ExtraJump.BlizzardInABottle).Enabled = true/* tModPorter Suggestion: Call Enable() if setting this to true, otherwise call Disable(). */;
		Player.GetJumpState(ExtraJump.FartInAJar).Enabled = true/* tModPorter Suggestion: Call Enable() if setting this to true, otherwise call Disable(). */;
		Player.GetJumpState(ExtraJump.TsunamiInABottle).Enabled = true/* tModPorter Suggestion: Call Enable() if setting this to true, otherwise call Disable(). */;
		Player.GetJumpState(ExtraJump.UnicornMount).Enabled = true/* tModPorter Suggestion: Call Enable() if setting this to true, otherwise call Disable(). */;
		Player.GetJumpState(ExtraJump.SantankMount).Enabled = true/* tModPorter Suggestion: Call Enable() if setting this to true, otherwise call Disable(). */;
		Player.GetJumpState(ExtraJump.GoatMount).Enabled = true/* tModPorter Suggestion: Call Enable() if setting this to true, otherwise call Disable(). */;
		Player.GetJumpState(ExtraJump.BasiliskMount).Enabled = true/* tModPorter Suggestion: Call Enable() if setting this to true, otherwise call Disable(). */;

		// Cannot port assignment to isPerformingJump_X
		Player.GetJumpState(ExtraJump.CloudInABottle).Active = true/* tModPorter Suggestion: Remove. Active cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.SandstormInABottle).Active = true/* tModPorter Suggestion: Remove. Active cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.BlizzardInABottle).Active = true/* tModPorter Suggestion: Remove. Active cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.FartInAJar).Active = true/* tModPorter Suggestion: Remove. Active cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.TsunamiInABottle).Active = true/* tModPorter Suggestion: Remove. Active cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.UnicornMount).Active = true/* tModPorter Suggestion: Remove. Active cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.SantankMount).Active = true/* tModPorter Suggestion: Remove. Active cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.GoatMount).Active = true/* tModPorter Suggestion: Remove. Active cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.BasiliskMount).Active = true/* tModPorter Suggestion: Remove. Active cannot be assigned a value. */;

		// Cannot port conditional setter, emit a suggestion
		Player.GetJumpState(ExtraJump.CloudInABottle).Available/* tModPorter Suggestion: Player.GetJumpState(ExtraJump.CloudInABottle).Available = ... */ = 1 > 2;
#endif

		bool hasJumpOption_Cloud = Player.GetJumpState(ExtraJump.CloudInABottle).Enabled;
		bool hasJumpOption_Sandstorm = Player.GetJumpState(ExtraJump.SandstormInABottle).Enabled;
		bool hasJumpOption_Blizzard = Player.GetJumpState(ExtraJump.BlizzardInABottle).Enabled;
		bool hasJumpOption_Fart = Player.GetJumpState(ExtraJump.FartInAJar).Enabled;
		bool hasJumpOption_Sail = Player.GetJumpState(ExtraJump.TsunamiInABottle).Enabled;
		bool hasJumpOption_Unicorn = Player.GetJumpState(ExtraJump.UnicornMount).Enabled;
		bool hasJumpOption_Santank = Player.GetJumpState(ExtraJump.SantankMount).Enabled;
		bool hasJumpOption_WallOfFleshGoat = Player.GetJumpState(ExtraJump.GoatMount).Enabled;
		bool hasJumpOption_Basilisk = Player.GetJumpState(ExtraJump.BasiliskMount).Enabled;

		bool canJumpAgain_Cloud = Player.GetJumpState(ExtraJump.CloudInABottle).Available;
		bool canJumpAgain_Sandstorm = Player.GetJumpState(ExtraJump.SandstormInABottle).Available;
		bool canJumpAgain_Blizzard = Player.GetJumpState(ExtraJump.BlizzardInABottle).Available;
		bool canJumpAgain_Fart = Player.GetJumpState(ExtraJump.FartInAJar).Available;
		bool canJumpAgain_Sail = Player.GetJumpState(ExtraJump.TsunamiInABottle).Available;
		bool canJumpAgain_Unicorn = Player.GetJumpState(ExtraJump.UnicornMount).Available;
		bool canJumpAgain_Santank = Player.GetJumpState(ExtraJump.SantankMount).Available;
		bool canJumpAgain_WallOfFleshGoat = Player.GetJumpState(ExtraJump.GoatMount).Available;
		bool canJumpAgain_Basilisk = Player.GetJumpState(ExtraJump.BasiliskMount).Available;

		bool isPerformingJump_Cloud = Player.GetJumpState(ExtraJump.CloudInABottle).Active;
		bool isPerformingJump_Sandstorm = Player.GetJumpState(ExtraJump.SandstormInABottle).Active;
		bool isPerformingJump_Blizzard = Player.GetJumpState(ExtraJump.BlizzardInABottle).Active;
		bool isPerformingJump_Fart = Player.GetJumpState(ExtraJump.FartInAJar).Active;
		bool isPerformingJump_Sail = Player.GetJumpState(ExtraJump.TsunamiInABottle).Active;
		bool isPerformingJump_Unicorn = Player.GetJumpState(ExtraJump.UnicornMount).Active;
		bool isPerformingJump_Santank = Player.GetJumpState(ExtraJump.SantankMount).Active;
		bool isPerformingJump_WallOfFleshGoat = Player.GetJumpState(ExtraJump.GoatMount).Active;
		bool isPerformingJump_Basilisk = Player.GetJumpState(ExtraJump.BasiliskMount).Active;
	}

	public void MethodB(AnotherPlayerClass other)
	{
#if COMPILE_ERROR
		Player.GetJumpState(ExtraJump.CloudInABottle).Enabled = true/* tModPorter Suggestion: Call Enable() if setting this to true, otherwise call Disable(). */;
#endif

		other.hasJumpOption_Cloud = true;

		bool canJumpAgain_Sandstorm = false;
		canJumpAgain_Sandstorm = true;
	}
}
