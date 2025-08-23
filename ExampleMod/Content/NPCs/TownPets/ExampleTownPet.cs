using ExampleMod.Common.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace ExampleMod.Content.NPCs.TownPets
{
	[AutoloadHead]
	public class ExampleTownPet : ModNPC
	{
		// Where our additional head sprites will be stored.
		internal static int HeadIndex1;
		internal static int HeadIndex2;
		internal static int HeadIndex3;
		internal static int HeadIndex4;
		internal static int HeadIndex5;
		internal static int HeadIndex6;

		private static ITownNPCProfile NPCProfile;

		public override void Load() {
			// Adds our variant heads to the NPCHeadLoader.
			HeadIndex1 = Mod.AddNPCHeadTexture(Type, $"{Texture}_1_Head");
			HeadIndex2 = Mod.AddNPCHeadTexture(Type, $"{Texture}_2_Head");
			HeadIndex3 = Mod.AddNPCHeadTexture(Type, $"{Texture}_3_Head");
			HeadIndex4 = Mod.AddNPCHeadTexture(Type, $"{Texture}_4_Head");
			HeadIndex5 = Mod.AddNPCHeadTexture(Type, $"{Texture}_5_Head");
			HeadIndex6 = Mod.AddNPCHeadTexture(Type, $"{Texture}_6_Head");
		}

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 27; // The number of frames our sprite has.
			NPCID.Sets.ExtraFramesCount[Type] = 20; // The number of frames after the walking frames.
			NPCID.Sets.AttackFrameCount[Type] = 0; // Town Pets don't have any attacking frames.
			NPCID.Sets.DangerDetectRange[Type] = 250; // How far away the NPC will detect danger. Measured in pixels.
			NPCID.Sets.AttackType[Type] = -1; // Town Pets do not attack. The default for this set is -1, so it is safe to remove this line if you wish.
			NPCID.Sets.AttackTime[Type] = -1; // Town Pets do not attack. The default for this set is -1, so it is safe to remove this line if you wish.
			NPCID.Sets.AttackAverageChance[Type] = 1;  // Town Pets do not attack. The default for this set is 1, so it is safe to remove this line if you wish.
			NPCID.Sets.HatOffsetY[Type] = -2; // An offset for where the party hat sits on the sprite.
			NPCID.Sets.ShimmerTownTransform[Type] = false; // Town Pets don't have a Shimmer variant.
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true; // But they are still immune to Shimmer.
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true; // And Confused.
			NPCID.Sets.ExtraTextureCount[Type] = 0; // Even though we have several variation textures, we don't use this set. The default for this set is 0, so it is safe to remove this line if you wish.
			NPCID.Sets.NPCFramingGroup[Type] = 8; // How the party hat is animated to match the walking animation. Town Cat = 4, Town Dog = 5, Town Bunny = 6, Town Slimes = 7, No offset = 8

			NPCID.Sets.IsTownPet[Type] = true; // Our NPC is a Town Pet
			NPCID.Sets.CannotSitOnFurniture[Type] = false; // True by default which means they cannot sit in chairs. True means they can sit on furniture like the Town Cat.
			NPCID.Sets.TownNPCBestiaryPriority.Add(Type); // Puts our NPC with all of the other Town NPCs.
			NPCID.Sets.PlayerDistanceWhilePetting[Type] = 32; // Distance the player stands from the Town Pet to pet.
			NPCID.Sets.IsPetSmallForPetting[Type] = true; // If set to true, the player's arm will be angled down while petting.

			// Influences how the NPC looks in the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() {
				Velocity = 0.25f, // Draws the NPC in the bestiary as if its walking +0.25 tiles in the x direction
			};

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			NPCProfile = new ExampleTownPetProfile(); // Assign our profile.
		}

		public override void SetDefaults() {
			NPC.townNPC = true; // Town Pets are still considered Town NPCs
			NPC.friendly = true;
			NPC.width = 20;
			NPC.height = 20;
			NPC.aiStyle = NPCAIStyleID.Passive;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.knockBackResist = 0.5f;
			NPC.housingCategory = 1; // This means it can share a house with a normal Town NPC.
			AnimationType = NPCID.TownBunny; // This example matches the animations of the Town Bunny.
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(
			[
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("Mods.ExampleMod.Bestiary.ExampleTownPet")
			]);
		}

		public override bool CanTownNPCSpawn(int numTownNPCs) {
			// If we've used the License, our Town Pet can freely respawn.
			if (ExampleTownPetSystem.boughtExampleTownPet) {
				return true;
			}
			return false;
		}

		public override ITownNPCProfile TownNPCProfile() {
			// Vanilla Town Pets use Profiles.VariantNPCProfile() to set the variants, but that doesn't work for us because
			// it uses Main.Assets.Request<T>() which won't find mod assets (ModContent.Request<T>() is needed instead).
			// So, we make our own NPCProfile. (See Below)
			return NPCProfile;
		}

		// Create a bunch of lists for our names. Each variant gets its own list of names.
		// You can these lists as long or as short as you'd like. 
		public readonly List<string> NameList0 = new() {
			"Monochromatic", "Grayscale", "Unpainted"
		};
		public readonly List<string> NameList1 = new() {
			"Red", "Crimson", "Cinnabar", "Maroon", "Scarlet"
		};
		public readonly List<string> NameList3 = new() {
			"Blue", "Azure", "Sky", "Ultramarine", "Navy"
		};
		public readonly List<string> NameList4 = new() {
			"Cyan", "Turquoise", "Teal", "Aqua", "Keppel"
		};
		public readonly List<string> NameList5 = new() {
			"Yellow", "Gold", "Cream", "Lemon", "Solar"
		};
		public readonly List<string> NameList6 = new() {
			"Magenta", "Pink", "Purple", "Violet", "Fuchsia"
		};

		public override List<string> SetNPCNameList() {
			return NPC.townNpcVariationIndex switch { // Change the name based on the variation.
				0 => NameList0,
				1 => NameList1, // Variant 1 will be the Shimmered variant if your NPC has a shimmer variant.
				// The Green (2) variant shows one approach to localizing Town NPC names.
				// One additional benefit of this approach is a separate mod can add a Mods.ExampleMod.NPCs.ExampleTownPet.Names.Green.Emerald key and it will automatically be used as an name option.
				2 => Language.FindAll(Lang.CreateDialogFilter(this.GetLocalizationKey("Names.Green."))).Select(x => x.Value).ToList(),
				3 => NameList3,
				4 => NameList4,
				5 => NameList5,
				6 => NameList6,
				_ => NameList0
			};
		}

		public override string GetChat() {
			WeightedRandom<string> chat = new();

			chat.Add(Language.GetTextValue("Mods.ExampleMod.Dialogue.ExampleTownPet.StandardDialogue"));

			return chat;
		}

		public override void SetChatButtons(ref string button, ref string button2) {
			button = Language.GetTextValue("UI.PetTheAnimal"); // Automatically translated to say "Pet"
		}

		public override bool PreAI() {
			// If your Town Pet can sit in chairs with NPCID.Sets.CannotSitOnFurniture[Type] = false
			// We want to move the Town NPC up visually to match the height of the chair.
			// NPC.ai[0] is set to 5f for Town NPC AI when they are sitting in a chair.
			if (NPC.ai[0] == 5f) {
				DrawOffsetY = -10; // Remember: Negative Y is up. So, this is moving the NPC up visually by 10 pixels.
			}
			else {
				DrawOffsetY = 0; // Reset it back to 0 when not sitting in a chair.
			}
			// Do not try to add or subtract from the DrawOffsetY. It'll cause the sprite to change its height every frame which will make it go off of the screen.

			// If your Town Pet doesn't sit in furniture, you can remove this entire PreAI() method.

			return base.PreAI();
		}

		public override void ChatBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects) {
			// If your Town Pet can sit in chairs with NPCID.Sets.CannotSitOnFurniture[Type] = false
			// and you've done the above DrawOffsetY to raise it up to the chair's height,
			// you'll notice the chat bubble that appears when hovering over them doesn't get raised up.
			// So, let's move it up as well.
			if (NPC.ai[0] == 5f) { // (Sitting in a chair.)
				position.Y -= 18f; // Move upwards.
			}
		}

		/*
		public override void EmoteBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects) {
			// Here is an example of how we can modify the emote bubble.

			// Flip the emote bubble and move it.
			spriteEffects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			position.X += NPC.width * NPC.direction;

			// (Town Pets can't use emotes, so this example here won't actually do anything.)
			// (For a working example, see ExampleMod/Common/GlobalNPCs/GuideGlobalNPC.cs)
		}
		*/

		public override void PartyHatPosition(ref Vector2 position, ref SpriteEffects spriteEffects) {
			// With this hook, we have full control over the position of the party hat.
			// We have already set NPCID.Sets.HatOffsetY[Type] = -2 in SetStaticDefaults which will move the party hat up 2 pixels at all times.
			// We also set PCID.Sets.NPCFramingGroup[Type] = 8 in SetStaticDefaults.
			// NPCFramingGroup is used vertically offset the party hat to match the animations of the NPC.
			// Group 8 has no inherit offsets for the party hat.

			int frame = NPC.frame.Y / NPC.frame.Height; // The current frame.
			int xOffset = 8; // Move the party hat forward so it is actually on the Town Pet's head.
			// Then move the party hat left/right depending on the frame.
			// These numbers were achieved by measuring the sprite relative to the "normal" position of the party hat.
			switch (frame) {
				case 1:
				case 2:
				case 7:
				case 9:
					xOffset -= 2;
					break;
				case 3:
				case 8:
					xOffset -= 4;
					break;
				case 11:
				case 15:
				case 16:
				case 17:
				case 26:
					xOffset += 2;
					break;
				case 12:
				case 13:
				case 14:
				case 18:
				case 24:
				case 25:
					xOffset += 4;
					break;
				case 19:
				case 20:
				case 21:
				case 22:
				case 23:
					xOffset += 6;
					break;
				default:
					break;
			}
			position.X += xOffset * NPC.spriteDirection;

			// We set NPCID.Sets.HatOffsetY[Type] = -2 so that means every frame is moved up 2 additional units.
			int yOffset = 0;
			// Then move the party hat up/down depending on the frame.
			// These numbers were achieved by measuring the sprite relative to the "normal" position of the party hat.
			switch (frame) {
				case 3:
				case 4:
					yOffset -= 2;
					break;
				case 5:
				case 6:
				case 10:
				case 17:
				case 26:
					yOffset += 2;
					break;
				case 18:
				case 25:
					yOffset += 4;
					break;
				case 11:
				case 15:
				case 16:
				case 24:
					yOffset += 6;
					break;
				case 19:
				case 20:
				case 23:
					yOffset += 8;
					break;
				case 21:
				case 22:
					yOffset += 10;
					break;
				case 12:
				case 14:
					yOffset += 12;
					break;
				case 13:
					yOffset += 14;
					break;
				default:
					break;
			}
			position.Y += yOffset;

			// Move it up to match the location of the head when sitting in a chair.
			if (NPC.ai[0] == 5f) {
				position.Y += -10;
			}
		}
	}

	public class ExampleTownPetProfile : ITownNPCProfile
	{
		private static readonly string filePath = "ExampleMod/Content/NPCs/TownPets/ExampleTownPet"; // The path to our base texture.

		// Load all of our textures only one time during mod load time.
		private readonly Asset<Texture2D> variant0 = ModContent.Request<Texture2D>(filePath);
		private readonly Asset<Texture2D> variant1 = ModContent.Request<Texture2D>($"{filePath}_1");
		private readonly Asset<Texture2D> variant2 = ModContent.Request<Texture2D>($"{filePath}_2");
		private readonly Asset<Texture2D> variant3 = ModContent.Request<Texture2D>($"{filePath}_3");
		private readonly Asset<Texture2D> variant4 = ModContent.Request<Texture2D>($"{filePath}_4");
		private readonly Asset<Texture2D> variant5 = ModContent.Request<Texture2D>($"{filePath}_5");
		private readonly Asset<Texture2D> variant6 = ModContent.Request<Texture2D>($"{filePath}_6");
		private readonly int headIndex0 = ModContent.GetModHeadSlot($"{filePath}_Head");

		public int RollVariation() {
			int random = Main.rand.Next(7); // 7 variants; 0 through 6.

			// If your Town Pet has a shimmer variant:
			// townNpcVariationIndex of 1 makes it shimmered, even when it isn't.
			// if (random == 1) {
			//	random = 7; // So variation 1 becomes number 7.
			// }

			return random;
		}

		public string GetNameForVariant(NPC npc) => npc.getNewNPCName(); // Reroll the name each time the Town Pet spawns or changes variant.

		public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) {
			return npc.townNpcVariationIndex switch {
				0 => variant0,
				1 => variant1, // Variant 1 will be the Shimmered variant if your NPC has a shimmer variant.
				2 => variant2,
				3 => variant3,
				4 => variant4,
				5 => variant5,
				6 => variant6,
				_ => variant0
			};
		}

		public int GetHeadTextureIndex(NPC npc) {
			return npc.townNpcVariationIndex switch {
				0 => headIndex0,
				1 => ExampleTownPet.HeadIndex1, // Variant 1 will be the Shimmered variant if your NPC has a shimmer variant.
				2 => ExampleTownPet.HeadIndex2,
				3 => ExampleTownPet.HeadIndex3,
				4 => ExampleTownPet.HeadIndex4,
				5 => ExampleTownPet.HeadIndex5,
				6 => ExampleTownPet.HeadIndex6,
				_ => headIndex0
			};
		}
	}
}