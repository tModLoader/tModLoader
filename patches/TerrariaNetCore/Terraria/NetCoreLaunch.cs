namespace Terraria;

internal static class NetCoreLaunch
{
	private static void Main(string[] args)
		=> Program.LaunchGame(args, monoArgs: true);
}
