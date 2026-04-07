using Terraria;

public class AddParameterTest
{
	void Method()
	{
		Projectile projectile = Main.projectile[0];
		Player owner = Main.player[Projectile.owner];
		projectile.Center = Main.GetPlayerArmPosition(projectile, default /* tModPorter Note: New parameter: Now expects a Player object, try 'player'. */);
	}
}