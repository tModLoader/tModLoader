using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems
{
	// Showcases using Mod.Call of other mods to facilitate mod integration/compatibility/support
	// Mod.Call is explained here https://github.com/tModLoader/tModLoader/wiki/Expert-Cross-Mod-Content#call-aka-modcall-intermediate
	// This only showcases one way to implement such integrations, you are free to explore your own options and other mods examples

	// You need to look for resources the mod developers provide regarding how they want you to add mod compatibility
	// This can be their homepage, workshop page, wiki, github, discord, other contacts etc.
	// If the mod is open source, you can visit its code distribution platform (usually GitHub) and look for "Call" in its Mod class
	public class ModIntegrationsSystem : ModSystem
	{
		public override void PostSetupContent() {
			// Most often, mods require you to use the PostSetupContent hook to call their methods. This guarantees various data is initialized and set up properly

			// Census Mod allows us to add spawn information to the town NPCs UI:
			// https://forums.terraria.org/index.php?threads/.74786/
			DoCensusIntegration();

			// Boss Checklist shows comprehensive information about bosses in its own UI. We can customize it:
			// https://forums.terraria.org/index.php?threads/.50668/
			DoBossChecklistIntegration();

			// We can integrate with other mods here by following the same pattern. Some modders may prefer a ModSystem for each mod they integrate with, or some other design.
		}

		private void DoCensusIntegration() {
			// We figured out how to add support by looking at it's Call method: https://github.com/JavidPack/Census/blob/1.4/Census.cs
			// Census also has a wiki, where the Call methods are better explained: https://github.com/JavidPack/Census/wiki/Support-using-Mod-Call

			if (!ModLoader.TryGetMod("Census", out Mod censusMod)) {
				// TryGetMod returns false if the mod is not currently loaded, so if this is the case, we just return early
				return;
			}

			// The "TownNPCCondition" method allows us to write out the spawn condition (which is coded via CanTownNPCSpawn), it requires an NPC type and a message
			int npcType = ModContent.NPCType<Content.NPCs.ExamplePerson>();

			// The message makes use of chat tags to make the item appear directly, making it more fancy
			string message = $"Have either an Example Item [i:{ModContent.ItemType<Content.Items.ExampleItem>()}] or an Example Block [i:{ModContent.ItemType<Content.Items.Placeable.ExampleBlock>()}] in your inventory";

			// Finally, call the desired method
			censusMod.Call("TownNPCCondition", npcType, message);

			// Additional calls can be made here for other Town NPCs in our mod
		}

		private void DoBossChecklistIntegration() {
			// The mods homepage links to its own wiki where the calls are explained: https://github.com/JavidPack/BossChecklist/wiki/Support-using-Mod-Call
			// If we navigate the wiki, we can find the "AddBoss" method, which we want in this case

			if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklistMod)) {
				return;
			}

			// For some messages, mods might not have them at release, so we need to verify when the last iteration of the method variation was first added to the mod, in this case 1.3.1
			// Usually mods either provide that information themselves in some way, or it's found on the github through commit history/blame
			if (bossChecklistMod.Version < new Version(1, 3, 1)) {
				return;
			}

			// The "AddBoss" method requires many parameters, defined separately below:

			// The name used for the title of the page
			string bossName = "Minion Boss";

			// The NPC type of the boss
			int bossType = ModContent.NPCType<Content.NPCs.MinionBoss.MinionBossBody>();

			// Value inferred from boss progression, see the wiki for details
			float weight = 0.7f;

			// Used for tracking checklist progress
			Func<bool> downed = () => DownedBossSystem.downedMinionBoss;

			// If the boss should show up on the checklist in the first place and when (here, always)
			Func<bool> available = () => true;

			// "collectibles" like relic, trophy, mask, pet
			List<int> collection = new List<int>()
			{
				ModContent.ItemType<Content.Items.Placeable.Furniture.MinionBossRelic>(),
				ModContent.ItemType<Content.Pets.MinionBossPet.MinionBossPetItem>(),
				ModContent.ItemType<Content.Items.Placeable.Furniture.MinionBossTrophy>(),
				ModContent.ItemType<Content.Items.Armor.Vanity.MinionBossMask>()
			};

			// The item used to summon the boss with (if available)
			int summonItem = ModContent.ItemType<Content.Items.Consumables.MinionBossSummonItem>();

			// Information for the player so he knows how to encounter the boss
			string spawnInfo = $"Use a [i:{summonItem}]";

			// The boss does not have a custom despawn message, so we omit it
			string despawnInfo = null;

			// By default, it draws the first frame of the boss, omit if you don't need custom drawing
			// But we want to draw the bestiary texture instead, so we create the code for that to draw centered on the intended location
			var customBossPortrait = (SpriteBatch sb, Rectangle rect, Color color) => {
				Texture2D texture = ModContent.Request<Texture2D>("ExampleMod/Assets/Textures/Bestiary/MinionBoss_Preview").Value;
				Vector2 centered = new Vector2(rect.X + (rect.Width / 2) - (texture.Width / 2), rect.Y + (rect.Height / 2) - (texture.Height / 2));
				sb.Draw(texture, centered, color);
			};

			bossChecklistMod.Call(
				"AddBoss",
				Mod,
				bossName,
				bossType,
				weight,
				downed,
				available,
				collection,
				summonItem,
				spawnInfo,
				despawnInfo,
				customBossPortrait
			);

			// Other bosses or additional Mod.Call can be made here.
		}
	}
}
