using Terraria.ModLoader;

public class ExtraJumpTest : ModPlayer
{
	public void MethodA()
	{
		Player.GetJumpState(ExtraJump.CloudInABottle).Enabled = true;
		Player.GetJumpState(ExtraJump.SandstormInABottle).Enabled = true;
		Player.GetJumpState(ExtraJump.BlizzardInABottle).Enabled = true;
		Player.GetJumpState(ExtraJump.FartInAJar).Enabled = true;
		Player.GetJumpState(ExtraJump.TsunamiInABottle).Enabled = true;
		Player.GetJumpState(ExtraJump.UnicornMount).Enabled = true;
		Player.GetJumpState(ExtraJump.SantankMount).Enabled = true;
		Player.GetJumpState(ExtraJump.GoatMount).Enabled = true;
		Player.GetJumpState(ExtraJump.BasiliskMount).Enabled = true;

		Player.GetJumpState(ExtraJump.CloudInABottle).JumpAvailable = true;
		Player.GetJumpState(ExtraJump.SandstormInABottle).JumpAvailable = true;
		Player.GetJumpState(ExtraJump.BlizzardInABottle).JumpAvailable = true;
		Player.GetJumpState(ExtraJump.FartInAJar).JumpAvailable = true;
		Player.GetJumpState(ExtraJump.TsunamiInABottle).JumpAvailable = true;
		Player.GetJumpState(ExtraJump.UnicornMount).JumpAvailable = true;
		Player.GetJumpState(ExtraJump.SantankMount).JumpAvailable = true;
		Player.GetJumpState(ExtraJump.GoatMount).JumpAvailable = true;
		Player.GetJumpState(ExtraJump.BasiliskMount).JumpAvailable = true;

#if COMPILE_ERROR
		// Cannot port assignment to isPerformingJump_X
		Player.GetJumpState(ExtraJump.CloudInABottle).PerformingJump = true/* tModPorter Suggestion: Remove. PerformingJump cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.SandstormInABottle).PerformingJump = true/* tModPorter Suggestion: Remove. PerformingJump cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.BlizzardInABottle).PerformingJump = true/* tModPorter Suggestion: Remove. PerformingJump cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.FartInAJar).PerformingJump = true/* tModPorter Suggestion: Remove. PerformingJump cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.TsunamiInABottle).PerformingJump = true/* tModPorter Suggestion: Remove. PerformingJump cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.UnicornMount).PerformingJump = true/* tModPorter Suggestion: Remove. PerformingJump cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.SantankMount).PerformingJump = true/* tModPorter Suggestion: Remove. PerformingJump cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.GoatMount).PerformingJump = true/* tModPorter Suggestion: Remove. PerformingJump cannot be assigned a value. */;
		Player.GetJumpState(ExtraJump.BasiliskMount).PerformingJump = true/* tModPorter Suggestion: Remove. PerformingJump cannot be assigned a value. */;

		// Cannot port conditional setter, emit a suggestion
		Player.GetJumpState(ExtraJump.CloudInABottle).Enabled/* tModPorter Suggestion: Player.GetJumpState(ExtraJump.CloudInABottle).Enabled = ... */ = 1 > 2;
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

		bool canJumpAgain_Cloud = Player.GetJumpState(ExtraJump.CloudInABottle).JumpAvailable;
		bool canJumpAgain_Sandstorm = Player.GetJumpState(ExtraJump.SandstormInABottle).JumpAvailable;
		bool canJumpAgain_Blizzard = Player.GetJumpState(ExtraJump.BlizzardInABottle).JumpAvailable;
		bool canJumpAgain_Fart = Player.GetJumpState(ExtraJump.FartInAJar).JumpAvailable;
		bool canJumpAgain_Sail = Player.GetJumpState(ExtraJump.TsunamiInABottle).JumpAvailable;
		bool canJumpAgain_Unicorn = Player.GetJumpState(ExtraJump.UnicornMount).JumpAvailable;
		bool canJumpAgain_Santank = Player.GetJumpState(ExtraJump.SantankMount).JumpAvailable;
		bool canJumpAgain_WallOfFleshGoat = Player.GetJumpState(ExtraJump.GoatMount).JumpAvailable;
		bool canJumpAgain_Basilisk = Player.GetJumpState(ExtraJump.BasiliskMount).JumpAvailable;

		bool isPerformingJump_Cloud = Player.GetJumpState(ExtraJump.CloudInABottle).PerformingJump;
		bool isPerformingJump_Sandstorm = Player.GetJumpState(ExtraJump.SandstormInABottle).PerformingJump;
		bool isPerformingJump_Blizzard = Player.GetJumpState(ExtraJump.BlizzardInABottle).PerformingJump;
		bool isPerformingJump_Fart = Player.GetJumpState(ExtraJump.FartInAJar).PerformingJump;
		bool isPerformingJump_Sail = Player.GetJumpState(ExtraJump.TsunamiInABottle).PerformingJump;
		bool isPerformingJump_Unicorn = Player.GetJumpState(ExtraJump.UnicornMount).PerformingJump;
		bool isPerformingJump_Santank = Player.GetJumpState(ExtraJump.SantankMount).PerformingJump;
		bool isPerformingJump_WallOfFleshGoat = Player.GetJumpState(ExtraJump.GoatMount).PerformingJump;
		bool isPerformingJump_Basilisk = Player.GetJumpState(ExtraJump.BasiliskMount).PerformingJump;
	}

	public void MethodB(AnotherPlayer other)
	{
		Player.GetJumpState(ExtraJump.CloudInABottle).Enabled = true;

		other.hasJumpOption_Cloud = true;

		bool canJumpAgain_Sandstorm = false;
		canJumpAgain_Sandstorm = true;
	}
}
