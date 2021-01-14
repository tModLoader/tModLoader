using ExampleMod.Common;
using ExampleMod.Content.Items;
using ExampleMod.Content.Items.Consumables;
using ExampleMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.NPCs.MinionBoss
{
	//The main part of the boss, usually refered to as "body"
	[AutoloadBossHead] //This attribute looks for a texture called "ClassName_Head_Boss" and automatically registers it as the NPC boss head icon
	public class MinionBossBody : ModNPC
	{
		//This boss has a second phase and we want to give it a second boss head icon, this variable keeps track of the registered texture from Load().
		//It is applied in the BossHeadSlot hook when the boss is in its second stage
		public static int secondStageHeadSlot = -1;

		//This code here is called a property: It acts like a variable, but can modify other things. In this case it uses the npc.ai[] array that has four entries.
		//We use properties because it makes code more readable ("if (SecondStage)" vs "if (npc.ai[0] == 1f)").
		//We use npc.ai[] because in combination with npc.netUpdate we can make it multiplayer compatible. Otherwise (making our own fields) we would have to write extra code to make it work (not covered here)
		public bool SecondStage {
			get => npc.ai[0] == 1f;
			set => npc.ai[0] = value ? 1f : 0f;
		}

		//More advanced usage of a property, used to wrap around to floats to act as a Vector2
		public Vector2 FirstStageDestination {
			get => new Vector2(npc.ai[1], npc.ai[2]);
			set {
				npc.ai[1] = value.X;
				npc.ai[2] = value.Y;
			}
		}

		//Auto property, acts exactly like a variable by using a hidden backing field
		public Vector2 LastFirstStageDestination { get; set; } = Vector2.Zero;

		//This property uses npc.localAI[] instead which doesn't get synced, but because SpawnedMinions is only used on spawn as a flag, this will get set by all parties to true.
		//Knowing what side (client, server, all) is in charge of a variable is important as npc.ai[] only has four entries
		public bool SpawnedMinions {
			get => npc.localAI[0] == 1f;
			set => npc.localAI[0] = value ? 1f : 0f;
		}

		private const int FirstStageTimerMax = 90;
		public ref float FirstStageTimer => ref npc.localAI[1];

		public ref float RemainingShields => ref npc.localAI[2];

		//We could also repurpose FirstStageTimer since it's unused in the second stage, or write "=> ref FirstStageTimer", but then we have to reset the timer when the state switch happens
		public ref float SecondStageTimer_SpawnEyes => ref npc.localAI[3];

		//Do NOT try to use npc.ai[4]/npc.localAI[4] or higher indexes, it only accepts 0, 1, 2 and 3!

		//Helper method to determine the minion type
		public static int MinionType() {
			return ModContent.NPCType<MinionBossMinion>();
		}

		//Helper method to determine the amount of minions summoned
		public static int MinionCount() {
			int count = 15;

			if (Main.expertMode) {
				count += 5; //Increase by 5 if expert or master mode
			}

			if (Main.getGoodWorld) {
				count += 5; //Increase by 5 if using the "For The Worthy" seed
			}

			return count;
		}

		public override void Load() {
			//We want to give it a second boss head icon, so we register one
			string texture = BossHeadTexture + "_SecondStage"; //Our texture is called "ClassName_Head_Boss_SecondStage"
			Mod.AddBossHeadTexture(texture, -1); //-1 because we already have one registered via the [AutoloadBossHead] attribute, it would overwrite it otherwise
			secondStageHeadSlot = ModContent.GetModBossHeadSlot(texture);
		}

		public override void BossHeadSlot(ref int index) {
			int slot = secondStageHeadSlot;
			if (SecondStage && slot != -1) {
				//If the boss is in its second stage, display the other head icon instead
				index = slot;
			}
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Minion Boss");
			Main.npcFrameCount[Type] = 6;

			//Add this in for bosses that have a summon item, requires corresponding code in the item (See MinionBossSummonItem.cs)
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			//Automatically group with other bosses
			NPCID.Sets.BossBestiaryPriority.Add(Type);

			//Specify the debuffs it is immune to
			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData {
				SpecificallyImmuneTo = new int[] {
					BuffID.Confused
				}
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

			// Influences how the NPC looks in the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
				CustomTexturePath = "ExampleMod/Assets/Textures/Bestiary/MinionBoss_Preview",
				PortraitScale = 0.6f,
				PortraitPositionYOverride = 0f,
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults() {
			npc.width = 110;
			npc.height = 110;
			npc.damage = 12;
			npc.defense = 10;
			npc.lifeMax = 2000;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath1;
			npc.knockBackResist = 0f;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.value = Item.buyPrice(gold: 5);
			npc.SpawnWithHigherTime(30);
			npc.boss = true;
			npc.npcSlots = 10f;

			//Don't set immunities like this as of 1.4:
			//npc.buffImmune[BuffID.Confused] = true;
			//immunities are handled via dictionaries through NPCID.Sets.DebuffImmunitySets

			npc.aiStyle = -1; //Custom AI, 0 has some very basic spriteDirection code we don't need

			//Important if this boss has a treasure bag
			bossBag = ModContent.ItemType<MinionBossBag>();
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// Sets the description of this NPC that is listed in the bestiary
			bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
				new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), //Plain black background
				new FlavorTextBestiaryInfoElement("Example Minion Boss that spawns minions on spawn, summoned with a spawn item. Showcases boss minion handling, multiplayer conciderations, and custom boss bar.")
			});
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			//Do NOT misuse the ModifyNPCLoot and OnKill hooks: the former is only used for registering drops, the latter for everything else

			//Master mode drops go here

			//All our drops here are based on "not expert", meaning we use .OnSuccess() to add them into the rule, which then gets added
			LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());

			int itemType = ModContent.ItemType<ExampleItem>();

			//We make 12-15 ExampleItems spawn randomly in all directions, like the lunar pillar fragments. Hereby we need the DropOneByOne rule,
			//which requires these parameters to be defined
			var parameters = new DropOneByOne.Parameters() {
				DropsXOutOfYTimes_TheX = 1,
				DropsXOutOfYTimes_TheY = 1,
				MinimumStackPerChunkBase = 1,
				MaximumStackPerChunkBase = 1,
				MinimumItemDropsCount = 12,
				MaximumItemDropsCount = 15,
			};

			//Notice we use notExpertRule.OnSuccess instead of npcLoot.Add so it only applies in normal mode
			notExpertRule.OnSuccess(new DropOneByOne(itemType, parameters));

			//Finally add the leading rule
			npcLoot.Add(notExpertRule);
		}

		public override void OnKill() {
			//This sets downedMinionBoss to true, and if it was false before, it initiates a lantern night
			NPC.SetEventFlagCleared(ref DownedBossWorld.downedMinionBoss, -1);

			//Since this hook is only ran in singleplayer and serverside, we would have to sync it manually.
			//Thankfully, vanilla sends the MessageID.WorldData packet if a BOSS was killed automatically, shortly after this hook is ran

			//If your NPC is not a boss and you need to sync the world (which includes ModWorlds, check DownedBossWorld), use this code:
			/*
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.WorldData);
			}
			*/
		}

		public override void FindFrame(int frameHeight) {
			//This NPC animates with a simple "go from start frame to final frame, and loop back to start frame" rule
			int startFrame = 0;
			int finalFrame = 2;

			if (SecondStage) {
				startFrame = 3;
				finalFrame = Main.npcFrameCount[npc.type] - 1;

				if (npc.frame.Y < startFrame * frameHeight) {
					//If we were animating the first stage frames and then switch to second stage, immediately change to the start frame of the second stage
					npc.frame.Y = startFrame * frameHeight;
				}
			}

			int frameSpeed = 5; //How long it stays on a frame in ticks
			npc.frameCounter++;
			npc.frameCounter += npc.velocity.Length() / 10f;
			if (npc.frameCounter > frameSpeed) {
				npc.frameCounter = 0;
				npc.frame.Y += frameHeight;

				if (npc.frame.Y > finalFrame * frameHeight) {
					npc.frame.Y = startFrame * frameHeight;
				}
			}
		}

		public override void AI() {
			if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead || !Main.player[npc.target].active) {
				npc.TargetClosest();
			}

			Player player = Main.player[npc.target];

			if (player.dead) {
				npc.velocity.Y -= 0.04f;
				//This method makes it so when the boss is in "despawn range" (outside of the screen), it despawns in 10 ticks
				npc.EncourageDespawn(10);
				return;
			}

			SpawnMinions();

			CheckSecondStage();

			//Be invulnerable during the first stage
			npc.dontTakeDamage = !SecondStage;

			if (SecondStage) {
				DoSecondStage(player);
			}
			else {
				DoFirstStage(player);
			}
		}

		private void SpawnMinions() {
			if (!SpawnedMinions) {
				SpawnedMinions = true;

				if (Main.netMode != NetmodeID.MultiplayerClient) {
					int count = MinionCount();

					for (int i = 0; i < count; i++) {
						int index = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<MinionBossMinion>(), npc.whoAmI);
						NPC minionNPC = Main.npc[index];

						if (minionNPC.modNPC is MinionBossMinion minion) {
							minion.ParentIndex = npc.whoAmI;
							minion.PositionIndex = i;
						}

						if (Main.netMode == NetmodeID.Server && index < Main.maxNPCs) {
							NetMessage.SendData(MessageID.SyncNPC, number: index);
						}
					}
				}
			}
		}

		private void CheckSecondStage() {
			if (SecondStage) {
				//No point checking if the NPC is already in its second stage
				return;
			}

			float remainingShieldsSum = 0f;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC otherNPC = Main.npc[i];
				if (otherNPC.active && otherNPC.type == MinionType() && otherNPC.modNPC is MinionBossMinion minion) {
					if (minion.ParentIndex == npc.whoAmI) {
						remainingShieldsSum += (float)otherNPC.life / otherNPC.lifeMax;
					}
				}
			}

			//We reference this in the MinionBossBossBar
			RemainingShields = remainingShieldsSum / MinionCount();

			if (RemainingShields <= 0 && Main.netMode != NetmodeID.MultiplayerClient) {
				//If we have no shields (aka "no minions alive"), we initiate the second stage, and notify other players that this NPC has reached its second stage
				//by setting npc.netUpdate to true in this tick. It will send important data like position, velocity and the npc.ai[] array to all connected clients

				//Because SecondStage is a property using npc.ai[], it will get synced this way
				SecondStage = true;
				npc.netUpdate = true;
			}
		}

		private void DoFirstStage(Player player) {
			//Each time the timer is 0, pick a random position a fixed distance away from the player but towards the opposite side
			//The NPC moves directly towards it with fixed speed, while displaying its trajectory as a telegraph

			FirstStageTimer++;
			if (FirstStageTimer > FirstStageTimerMax) {
				FirstStageTimer = 0;
			}

			float distance = 200; //Distance in pixels behind the player

			if (FirstStageTimer == 0) {
				Vector2 fromPlayer = npc.Center - player.Center;

				if (Main.netMode != NetmodeID.MultiplayerClient) {
					//Important multiplayer concideration: drastic change in behavior (that is also decided by randomness) like this requires
					//to be executed on the server (or singleplayer) to keep the boss in sync

					float angle = fromPlayer.ToRotation();
					float twelfth = MathHelper.Pi / 6;

					angle += MathHelper.Pi + Main.rand.NextFloat(-twelfth, twelfth);
					if (angle > MathHelper.TwoPi) {
						angle -= MathHelper.TwoPi;
					}
					else if (angle < 0) {
						angle += MathHelper.TwoPi;
					}

					Vector2 relativeDestination = angle.ToRotationVector2() * distance;

					FirstStageDestination = player.Center + relativeDestination;
					npc.netUpdate = true;
				}
			}

			//Move along the vector
			Vector2 toDestination = FirstStageDestination - npc.Center;
			Vector2 toDestinationNormalized = Vector2.Normalize(toDestination);
			float speed = Math.Min(distance, toDestination.Length());
			npc.velocity = toDestinationNormalized * speed / 30;

			if (FirstStageDestination != LastFirstStageDestination) {
				//If destination changed
				npc.TargetClosest(); //Pick the closest player target again

				//"Why is this not in the same code that sets FirstStageDestination?" Because in multiplayer it's ran by the server.
				//The client has to know when the destination changes a different way. Keeping track of the previous ticks' destination is one way
				if (Main.netMode != NetmodeID.Server) {
					//For visuals regarding NPC position, netOffset has to be concidered to make visuals align properly
					npc.position += npc.netOffset;

					//Draw a line between the NPC and its destination, represented as dusts every 20 pixels
					Dust.QuickDustLine(npc.Center + toDestinationNormalized * npc.width, FirstStageDestination, toDestination.Length() / 20f, Color.Yellow);

					npc.position -= npc.netOffset;
				}
			}
			LastFirstStageDestination = FirstStageDestination;

			//No damage during first phase
			npc.damage = 0;

			//Fade in based on remaining total minion life
			npc.alpha = (int)(RemainingShields * 255);

			npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;
		}

		private void DoSecondStage(Player player) {
			Vector2 toPlayer = player.Center - npc.Center;

			float offsetX = 200f;

			Vector2 abovePlayer = player.Top + new Vector2(npc.direction * offsetX, -npc.height);

			Vector2 toAbovePlayer = abovePlayer - npc.Center;
			Vector2 toAbovePlayerNormalized = Vector2.Normalize(toAbovePlayer);

			//The NPC tries to go towards the offsetX position, but most likely it will never get there exactly, or close to if the player is moving
			//This checks if the npc is "70% there", and then changes direction
			float changeDirOffset = offsetX * 0.7f;

			if (npc.direction == -1 && npc.Center.X - changeDirOffset < abovePlayer.X ||
				npc.direction == 1 && npc.Center.X + changeDirOffset > abovePlayer.X) {
				npc.direction *= -1;
			}

			float speed = 8f;
			float inertia = 40f;

			//If the boss is somehow below the player, move faster to catch up
			if (npc.Top.Y > player.Bottom.Y) {
				speed = 12f;
			}

			Vector2 moveTo = toAbovePlayerNormalized * speed;
			npc.velocity = (npc.velocity * (inertia - 1) + moveTo) / inertia;

			DoSecondStage_SpawnEyes(player);

			npc.damage = npc.defDamage;

			npc.alpha = 0;

			npc.rotation = toPlayer.ToRotation() - MathHelper.PiOver2;
		}

		private void DoSecondStage_SpawnEyes(Player player) {
			//At 100% health, spawn every 90 ticks
			//Drops down until 33% health to spawn every 30 ticks
			float timerMax = Utils.Clamp((float)npc.life / npc.lifeMax, 0.33f, 1f) * 90;

			SecondStageTimer_SpawnEyes++;
			if (SecondStageTimer_SpawnEyes > timerMax) {
				SecondStageTimer_SpawnEyes = 0;
			}

			if (npc.HasValidTarget && SecondStageTimer_SpawnEyes == 0 && Main.netMode != NetmodeID.MultiplayerClient) {
				//Spawn projectile randomly below player, based on horizontal velocity to make kiting harder, starting velocity 1f upwards
				//(The projectiles accelerate from their initial velocity)

				float kitingOffsetX = Utils.Clamp(player.velocity.X * 16, -100, 100);
				Vector2 position = player.Bottom + new Vector2(kitingOffsetX + Main.rand.Next(-100, 100), Main.rand.Next(50, 100));

				int type = ModContent.ProjectileType<MinionBossEye>();
				int damage = npc.damage / 2;
				Projectile.NewProjectile(position, -Vector2.UnitY, type, damage, 0f, Main.myPlayer);
			}
		}
	}
}
