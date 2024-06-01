using Terraria.ModLoader;

public class ExtraJumpTest : ModPlayer
{
	public void MethodA()
	{
		player.canJumpAgain_Cloud = true;
		player.canJumpAgain_Sandstorm = true;
		player.canJumpAgain_Blizzard = true;
		player.canJumpAgain_Fart = true;
		player.canJumpAgain_Sail = true;
		player.canJumpAgain_Unicorn = true;
		player.canJumpAgain_Santank = true;
		player.canJumpAgain_WallOfFleshGoat = true;
		player.canJumpAgain_Basilisk = true;

		// Cannot port assignment to hasJumpOption_X
		player.hasJumpOption_Cloud = true;
		player.hasJumpOption_Sandstorm = true;
		player.hasJumpOption_Blizzard = true;
		player.hasJumpOption_Fart = true;
		player.hasJumpOption_Sail = true;
		player.hasJumpOption_Unicorn = true;
		player.hasJumpOption_Santank = true;
		player.hasJumpOption_WallOfFleshGoat = true;
		player.hasJumpOption_Basilisk = true;

		// Cannot port assignment to isPerformingJump_X
		player.isPerformingJump_Cloud = true;
		player.isPerformingJump_Sandstorm = true;
		player.isPerformingJump_Blizzard = true;
		player.isPerformingJump_Fart = true;
		player.isPerformingJump_Sail = true;
		player.isPerformingJump_Unicorn = true;
		player.isPerformingJump_Santank = true;
		player.isPerformingJump_WallOfFleshGoat = true;
		player.isPerformingJump_Basilisk = true;

		// Cannot port conditional setter, emit a suggestion
		player.canJumpAgain_Cloud = 1 > 2;

		bool hasJumpOption_Cloud = player.hasJumpOption_Cloud;
		bool hasJumpOption_Sandstorm = player.hasJumpOption_Sandstorm;
		bool hasJumpOption_Blizzard = player.hasJumpOption_Blizzard;
		bool hasJumpOption_Fart = player.hasJumpOption_Fart;
		bool hasJumpOption_Sail = player.hasJumpOption_Sail;
		bool hasJumpOption_Unicorn = player.hasJumpOption_Unicorn;
		bool hasJumpOption_Santank = player.hasJumpOption_Santank;
		bool hasJumpOption_WallOfFleshGoat = player.hasJumpOption_WallOfFleshGoat;
		bool hasJumpOption_Basilisk = player.hasJumpOption_Basilisk;

		bool canJumpAgain_Cloud = player.canJumpAgain_Cloud;
		bool canJumpAgain_Sandstorm = player.canJumpAgain_Sandstorm;
		bool canJumpAgain_Blizzard = player.canJumpAgain_Blizzard;
		bool canJumpAgain_Fart = player.canJumpAgain_Fart;
		bool canJumpAgain_Sail = player.canJumpAgain_Sail;
		bool canJumpAgain_Unicorn = player.canJumpAgain_Unicorn;
		bool canJumpAgain_Santank = player.canJumpAgain_Santank;
		bool canJumpAgain_WallOfFleshGoat = player.canJumpAgain_WallOfFleshGoat;
		bool canJumpAgain_Basilisk = player.canJumpAgain_Basilisk;

		bool isPerformingJump_Cloud = player.isPerformingJump_Cloud;
		bool isPerformingJump_Sandstorm = player.isPerformingJump_Sandstorm;
		bool isPerformingJump_Blizzard = player.isPerformingJump_Blizzard;
		bool isPerformingJump_Fart = player.isPerformingJump_Fart;
		bool isPerformingJump_Sail = player.isPerformingJump_Sail;
		bool isPerformingJump_Unicorn = player.isPerformingJump_Unicorn;
		bool isPerformingJump_Santank = player.isPerformingJump_Santank;
		bool isPerformingJump_WallOfFleshGoat = player.isPerformingJump_WallOfFleshGoat;
		bool isPerformingJump_Basilisk = player.isPerformingJump_Basilisk;
	}

	public void MethodB(AnotherPlayerClass other)
	{
		player.hasJumpOption_Cloud = true;

		other.hasJumpOption_Cloud = true;

		bool canJumpAgain_Sandstorm = false;
		canJumpAgain_Sandstorm = true;
	}
}
