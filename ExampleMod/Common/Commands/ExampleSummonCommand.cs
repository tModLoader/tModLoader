using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Common.Commands
{
	public class ExampleSummonCommand : ModCommand
	{
		public static LocalizedText UsageText { get; private set; }
		public static LocalizedText DescriptionText { get; private set; }
		public static LocalizedText[] ErrorText { get; private set; }

		public override void SetStaticDefaults() {
			string key = $"Commands.{nameof(ExampleSummonCommand)}.";
			UsageText = Mod.GetLocalization($"{key}Usage");
			DescriptionText = Mod.GetLocalization($"{key}Description");
			ErrorText = Enumerable.Range(0, 5).Select(i => Mod.GetLocalization($"{key}Error_{i}")).ToArray();
		}

		// CommandType.World means that command can be used in Chat in SP and MP, but executes on the Server in MP
		public override CommandType Type
			=> CommandType.World;

		// The desired text to trigger this command
		public override string Command
			=> "summon";

		// A short usage explanation for this command
		public override string Usage
			=> UsageText.Value;

		// A short description of this command
		public override string Description
			=> DescriptionText.Value;

		public override void Action(CommandCaller caller, string input, string[] args) {
			// Checking input Arguments
			if (args.Length == 0) {
				throw new UsageException(ErrorText[0].Value);
			}
			if (!int.TryParse(args[0], out int type)) {
				throw new UsageException(ErrorText[1].Format(args[0]));
			}

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
				if (!int.TryParse(args[1], out xSpawnPosition)) {
					throw new UsageException(ErrorText[2].Format(args[1]));
				}
			}

			// If command has Y position argument
			if (args.Length > 2) {
				// Y relative check
				if (args[2][0] == '~') {
					relativeY = true;
					args[2] = args[2].Substring(1);
				}
				// Parsing Y position
				if (!int.TryParse(args[2], out ySpawnPosition)) {
					throw new UsageException(ErrorText[3].Format(args[2]));
				}
			}

			// Adjusting the positions if they are relative
			if (relativeX) {
				xSpawnPosition += (int)caller.Player.Bottom.X;
			}
			if (relativeY) {
				ySpawnPosition += (int)caller.Player.Bottom.Y;
			}

			// If command has number argument
			if (args.Length > 3) {
				if (!int.TryParse(args[3], out numToSpawn)) {
					throw new UsageException(ErrorText[4].Format(args[3]));
				}
			}

			// Spawning numToSpawn NPCs with a given position and type
			for (int k = 0; k < numToSpawn; k++) {
				// NPC.NewNPC return 200 (Main.maxNPCs) if there are not enough NPC slots to spawn
				int slot = NPC.NewNPC(new EntitySource_DebugCommand($"{nameof(ExampleMod)}_{nameof(ExampleSummonCommand)}"), xSpawnPosition, ySpawnPosition, type);

				// Sync of NPCs on the server in MP
				if (Main.netMode == NetmodeID.Server && slot < Main.maxNPCs) {
					NetMessage.SendData(MessageID.SyncNPC, number: slot);
				}
			}
		}
	}
}
