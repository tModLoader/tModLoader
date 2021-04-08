using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExampleMod.Common.Commands
{
	public class ExampleSummonCommand : ModCommand
	{
		// CommandType.World means that command can be used in Chat in SP and MP, but executes on the Server in MP
		public override CommandType Type
			=> CommandType.World;

		// The desired text to trigger this command
		public override string Command
			=> "summon";

		// A short usage explanation for this command
		public override string Usage
			=> "/summon type [[~]x] [[~]y] [number]" +
			"\n type - typeId of NPC." +
			"\n x and y - position of spawn." +
			"\n ~ - to use position relative to player." +
			"\n number - number of NPC's to spawn.";

		// A short description of this command
		public override string Description
			=> "Spawn a NPC's by typeId";

		public override void Action(CommandCaller caller, string input, string[] args) {
			// Checking input Arguments
			if (args.Length == 0)
				throw new UsageException("At least one argument was expected.");
			if (!int.TryParse(args[0], out int type))
				throw new UsageException(args[0] + " is not a correct integer value.");

			// Default values for spawn
			// Position - Player.Bottom, number of NPC - 1 
			int xSpawnPosition = (int)caller.Player.Bottom.X;
			int ySpawnPosition = (int)caller.Player.Bottom.Y;
			int numToSpawn = 1;
			bool relativeX = false;
			bool relativeY = false;


			// If command has X position argument
			if (args.Length > 1) {
				// X relative check
				if (args[1][0] == '~') {
					relativeX = true;
					args[1] = args[1].Substring(1);
				}
				// Parsing X position
				if (!int.TryParse(args[1], out xSpawnPosition)) 
					throw new UsageException(args[1] + " is not a correct X position(must be valid integer value).");
			}

			// If command has Y position argument
			if (args.Length > 2) {
				// Y relative check
				if (args[2][0] == '~') {
					relativeY = true;
					args[2] = args[2].Substring(1);
				}
				// Parsing Y position
				if (!int.TryParse(args[2], out ySpawnPosition))
					throw new UsageException(args[2] + " is not a correct Y position(must be valid integer value).");
			}

			// Adjusting the positions if they are relative
			if (relativeX)
				xSpawnPosition += (int)caller.Player.Bottom.X;
			if (relativeY)
				ySpawnPosition += (int)caller.Player.Bottom.Y;

			// If command has number argument
			if (args.Length > 3) {
				if (!int.TryParse(args[3], out numToSpawn))
					throw new UsageException(args[3] + " is not a correct number (must be valid integer value).");
			}


			for (int k = 0; k < numToSpawn; k++) {
				// Spawning numToSpawn NPCs with a given postions and type
				// NPC.NewNPC return 200(Main.maxNPCs) if there are not enough NPC slots to spawn
				int slot = NPC.NewNPC(xSpawnPosition, ySpawnPosition, type);

				// Sync of NPCs on the server in MP
				if (Main.netMode == NetmodeID.Server && slot < Main.maxNPCs)
					NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, slot);
			}
		}
	}
}
