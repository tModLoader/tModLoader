using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.NPCs
{
	internal class ExampleWormHead : ExampleWorm
	{
		public override string Texture => "Terraria/Images/NPC_" + NPCID.DiggerHead;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Worm");
	
			var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { //Influences how the NPC looks in the Bestiary
				CustomTexturePath = "ExampleMod/Content/NPCs/ExampleWorm_Bestiary", //If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(40f, 24f),
				PortraitPositionXOverride = 0f,
				PortraitPositionYOverride = 12f
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
		}

		public override void SetDefaults() {
			// Head is 10 defence, body 20, tail 30.
			NPC.CloneDefaults(NPCID.DiggerHead);
			NPC.aiStyle = -1;
			NPC.color = Color.Aqua;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,

				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement("Looks like a Digger fell into some aqua-colored paint. Oh well.")
			});
		}

		public override void Init() {
			base.Init();
			head = true;
		}

		private int attackCounter;
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(attackCounter);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			attackCounter = reader.ReadInt32();
		}

		public override void CustomBehavior() {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				if (attackCounter > 0) {
					attackCounter--;
				}

				Player target = Main.player[NPC.target];
				if (attackCounter <= 0 && Vector2.Distance(NPC.Center, target.Center) < 200 && Collision.CanHit(NPC.Center, 1, 1, target.Center, 1, 1)) {
					Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
					direction = direction.RotatedByRandom(MathHelper.ToRadians(10));

					int projectile = Projectile.NewProjectile(NPC.GetProjectileSpawnSource(), NPC.Center, direction * 1, ProjectileID.ShadowBeamHostile, 5, 0, Main.myPlayer);
					Main.projectile[projectile].timeLeft = 300;
					attackCounter = 500;
					NPC.netUpdate = true;
				}
			}
		}
	}

	internal class ExampleWormBody : ExampleWorm
	{
		public override string Texture => "Terraria/Images/NPC_" + NPCID.DiggerBody;

		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
				Hide = true //Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
		}

		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerBody);
			NPC.aiStyle = -1;
			NPC.color = Color.Aqua;
		}
	}

	internal class ExampleWormTail : ExampleWorm
	{
		public override string Texture => "Terraria/Images/NPC_" + NPCID.DiggerTail;

		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
				Hide = true //Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
		}

		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerTail);
			NPC.aiStyle = -1;
			NPC.color = Color.Aqua;
		}

		public override void Init() {
			base.Init();
			tail = true;
		}
	}

	// I made this 2nd base class to limit code repetition.
	public abstract class ExampleWorm : Worm
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Worm");
		}

		public override void Init() {
			minLength = 6;
			maxLength = 12;
			tailType = ModContent.NPCType<ExampleWormTail>();
			bodyType = ModContent.NPCType<ExampleWormBody>();
			headType = ModContent.NPCType<ExampleWormHead>();
			speed = 5.5f;
			turnSpeed = 0.045f;
		}
	}

	//ported from my tAPI mod because I'm lazy
	// This abstract class can be used for non splitting worm type NPC.
	public abstract class Worm : ModNPC
	{
		/* ai[0] = follower
		* ai[1] = following
		* ai[2] = distanceFromTail
		* ai[3] = head
		*/
		public bool head;
		public bool tail;
		public int minLength;
		public int maxLength;
		public int headType;
		public int bodyType;
		public int tailType;
		public bool flies = false;
		public bool directional = false;
		public float speed;
		public float turnSpeed;

		public override void AI() {
			if (NPC.localAI[1] == 0f) {
				NPC.localAI[1] = 1f;
				Init();
			}
			if (NPC.ai[3] > 0f) {
				NPC.realLife = (int)NPC.ai[3];
			}
			if (!head && NPC.timeLeft < 300) {
				NPC.timeLeft = 300;
			}
			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead) {
				NPC.TargetClosest(true);
			}
			if (Main.player[NPC.target].dead && NPC.timeLeft > 300) {
				NPC.timeLeft = 300;
			}
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				if (!tail && NPC.ai[0] == 0f) {
					if (head) {
						NPC.ai[3] = (float)NPC.whoAmI;
						NPC.realLife = NPC.whoAmI;
						NPC.ai[2] = (float)Main.rand.Next(minLength, maxLength + 1);
						NPC.ai[0] = (float)NPC.NewNPC((int)(NPC.position.X + (float)(NPC.width / 2)), (int)(NPC.position.Y + (float)NPC.height), bodyType, NPC.whoAmI);
					}
					else if (NPC.ai[2] > 0f) {
						NPC.ai[0] = (float)NPC.NewNPC((int)(NPC.position.X + (float)(NPC.width / 2)), (int)(NPC.position.Y + (float)NPC.height), NPC.type, NPC.whoAmI);
					}
					else {
						NPC.ai[0] = (float)NPC.NewNPC((int)(NPC.position.X + (float)(NPC.width / 2)), (int)(NPC.position.Y + (float)NPC.height), tailType, NPC.whoAmI);
					}
					Main.npc[(int)NPC.ai[0]].ai[3] = NPC.ai[3];
					Main.npc[(int)NPC.ai[0]].realLife = NPC.realLife;
					Main.npc[(int)NPC.ai[0]].ai[1] = (float)NPC.whoAmI;
					Main.npc[(int)NPC.ai[0]].ai[2] = NPC.ai[2] - 1f;
					NPC.netUpdate = true;
				}
				if (!head && (!Main.npc[(int)NPC.ai[1]].active || Main.npc[(int)NPC.ai[1]].type != headType && Main.npc[(int)NPC.ai[1]].type != bodyType)) {
					NPC.life = 0;
					NPC.HitEffect(0, 10.0);
					NPC.active = false;
				}
				if (!tail && (!Main.npc[(int)NPC.ai[0]].active || Main.npc[(int)NPC.ai[0]].type != bodyType && Main.npc[(int)NPC.ai[0]].type != tailType)) {
					NPC.life = 0;
					NPC.HitEffect(0, 10.0);
					NPC.active = false;
				}
				if (!NPC.active && Main.netMode == NetmodeID.Server) {
					NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, NPC.whoAmI, -1f, 0f, 0f, 0, 0, 0);
				}
			}
			int num180 = (int)(NPC.position.X / 16f) - 1;
			int num181 = (int)((NPC.position.X + (float)NPC.width) / 16f) + 2;
			int num182 = (int)(NPC.position.Y / 16f) - 1;
			int num183 = (int)((NPC.position.Y + (float)NPC.height) / 16f) + 2;
			if (num180 < 0) {
				num180 = 0;
			}
			if (num181 > Main.maxTilesX) {
				num181 = Main.maxTilesX;
			}
			if (num182 < 0) {
				num182 = 0;
			}
			if (num183 > Main.maxTilesY) {
				num183 = Main.maxTilesY;
			}
			bool flag18 = flies;
			if (!flag18) {
				for (int num184 = num180; num184 < num181; num184++) {
					for (int num185 = num182; num185 < num183; num185++) {
						if (Main.tile[num184, num185] != null && (Main.tile[num184, num185].IsActiveUnactuated && (Main.tileSolid[(int)Main.tile[num184, num185].type] || Main.tileSolidTop[(int)Main.tile[num184, num185].type] && Main.tile[num184, num185].frameY == 0) || Main.tile[num184, num185].LiquidAmount > 64)) {
							Vector2 vector17;
							vector17.X = (float)(num184 * 16);
							vector17.Y = (float)(num185 * 16);
							if (NPC.position.X + (float)NPC.width > vector17.X && NPC.position.X < vector17.X + 16f && NPC.position.Y + (float)NPC.height > vector17.Y && NPC.position.Y < vector17.Y + 16f) {
								flag18 = true;
								if (Main.rand.NextBool(100) && NPC.behindTiles && Main.tile[num184, num185].IsActiveUnactuated) {
									WorldGen.KillTile(num184, num185, true, true, false);
								}
								if (Main.netMode != NetmodeID.MultiplayerClient && Main.tile[num184, num185].type == 2) {
									ushort arg_BFCA_0 = Main.tile[num184, num185 - 1].type;
								}
							}
						}
					}
				}
			}
			if (!flag18 && head) {
				Rectangle rectangle = new Rectangle((int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height);
				int num186 = 1000;
				bool flag19 = true;
				for (int num187 = 0; num187 < 255; num187++) {
					if (Main.player[num187].active) {
						Rectangle rectangle2 = new Rectangle((int)Main.player[num187].position.X - num186, (int)Main.player[num187].position.Y - num186, num186 * 2, num186 * 2);
						if (rectangle.Intersects(rectangle2)) {
							flag19 = false;
							break;
						}
					}
				}
				if (flag19) {
					flag18 = true;
				}
			}
			if (directional) {
				if (NPC.velocity.X < 0f) {
					NPC.spriteDirection = 1;
				}
				else if (NPC.velocity.X > 0f) {
					NPC.spriteDirection = -1;
				}
			}
			float num188 = speed;
			float num189 = turnSpeed;
			Vector2 vector18 = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
			float num191 = Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2);
			float num192 = Main.player[NPC.target].position.Y + (float)(Main.player[NPC.target].height / 2);
			num191 = (float)((int)(num191 / 16f) * 16);
			num192 = (float)((int)(num192 / 16f) * 16);
			vector18.X = (float)((int)(vector18.X / 16f) * 16);
			vector18.Y = (float)((int)(vector18.Y / 16f) * 16);
			num191 -= vector18.X;
			num192 -= vector18.Y;
			float num193 = (float)System.Math.Sqrt((double)(num191 * num191 + num192 * num192));
			if (NPC.ai[1] > 0f && NPC.ai[1] < (float)Main.npc.Length) {
				try {
					vector18 = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
					num191 = Main.npc[(int)NPC.ai[1]].position.X + (float)(Main.npc[(int)NPC.ai[1]].width / 2) - vector18.X;
					num192 = Main.npc[(int)NPC.ai[1]].position.Y + (float)(Main.npc[(int)NPC.ai[1]].height / 2) - vector18.Y;
				}
				catch {
				}
				NPC.rotation = (float)System.Math.Atan2((double)num192, (double)num191) + 1.57f;
				num193 = (float)System.Math.Sqrt((double)(num191 * num191 + num192 * num192));
				int num194 = NPC.width;
				num193 = (num193 - (float)num194) / num193;
				num191 *= num193;
				num192 *= num193;
				NPC.velocity = Vector2.Zero;
				NPC.position.X = NPC.position.X + num191;
				NPC.position.Y = NPC.position.Y + num192;
				if (directional) {
					if (num191 < 0f) {
						NPC.spriteDirection = 1;
					}
					if (num191 > 0f) {
						NPC.spriteDirection = -1;
					}
				}
			}
			else {
				if (!flag18) {
					NPC.TargetClosest(true);
					NPC.velocity.Y = NPC.velocity.Y + 0.11f;
					if (NPC.velocity.Y > num188) {
						NPC.velocity.Y = num188;
					}
					if ((double)(System.Math.Abs(NPC.velocity.X) + System.Math.Abs(NPC.velocity.Y)) < (double)num188 * 0.4) {
						if (NPC.velocity.X < 0f) {
							NPC.velocity.X = NPC.velocity.X - num189 * 1.1f;
						}
						else {
							NPC.velocity.X = NPC.velocity.X + num189 * 1.1f;
						}
					}
					else if (NPC.velocity.Y == num188) {
						if (NPC.velocity.X < num191) {
							NPC.velocity.X = NPC.velocity.X + num189;
						}
						else if (NPC.velocity.X > num191) {
							NPC.velocity.X = NPC.velocity.X - num189;
						}
					}
					else if (NPC.velocity.Y > 4f) {
						if (NPC.velocity.X < 0f) {
							NPC.velocity.X = NPC.velocity.X + num189 * 0.9f;
						}
						else {
							NPC.velocity.X = NPC.velocity.X - num189 * 0.9f;
						}
					}
				}
				else {
					if (!flies && NPC.behindTiles && NPC.soundDelay == 0) {
						float num195 = num193 / 40f;
						if (num195 < 10f) {
							num195 = 10f;
						}
						if (num195 > 20f) {
							num195 = 20f;
						}
						NPC.soundDelay = (int)num195;
						SoundEngine.PlaySound(SoundID.Roar, NPC.position, 1);
					}
					num193 = (float)System.Math.Sqrt((double)(num191 * num191 + num192 * num192));
					float num196 = System.Math.Abs(num191);
					float num197 = System.Math.Abs(num192);
					float num198 = num188 / num193;
					num191 *= num198;
					num192 *= num198;
					if (ShouldRun()) {
						bool flag20 = true;
						for (int num199 = 0; num199 < 255; num199++) {
							if (Main.player[num199].active && !Main.player[num199].dead && Main.player[num199].ZoneCorrupt) {
								flag20 = false;
							}
						}
						if (flag20) {
							if (Main.netMode != NetmodeID.MultiplayerClient && (double)(NPC.position.Y / 16f) > (Main.rockLayer + (double)Main.maxTilesY) / 2.0) {
								NPC.active = false;
								int num200 = (int)NPC.ai[0];
								while (num200 > 0 && num200 < 200 && Main.npc[num200].active && Main.npc[num200].aiStyle == NPC.aiStyle) {
									int num201 = (int)Main.npc[num200].ai[0];
									Main.npc[num200].active = false;
									NPC.life = 0;
									if (Main.netMode == NetmodeID.Server) {
										NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num200, 0f, 0f, 0f, 0, 0, 0);
									}
									num200 = num201;
								}
								if (Main.netMode == NetmodeID.Server) {
									NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI, 0f, 0f, 0f, 0, 0, 0);
								}
							}
							num191 = 0f;
							num192 = num188;
						}
					}

					bool flag21 = false;

					if (!flag21) {
						if (NPC.velocity.X > 0f && num191 > 0f || NPC.velocity.X < 0f && num191 < 0f || NPC.velocity.Y > 0f && num192 > 0f || NPC.velocity.Y < 0f && num192 < 0f) {
							if (NPC.velocity.X < num191) {
								NPC.velocity.X = NPC.velocity.X + num189;
							}
							else {
								if (NPC.velocity.X > num191) {
									NPC.velocity.X = NPC.velocity.X - num189;
								}
							}
							if (NPC.velocity.Y < num192) {
								NPC.velocity.Y = NPC.velocity.Y + num189;
							}
							else {
								if (NPC.velocity.Y > num192) {
									NPC.velocity.Y = NPC.velocity.Y - num189;
								}
							}
							if ((double)System.Math.Abs(num192) < (double)num188 * 0.2 && (NPC.velocity.X > 0f && num191 < 0f || NPC.velocity.X < 0f && num191 > 0f)) {
								if (NPC.velocity.Y > 0f) {
									NPC.velocity.Y = NPC.velocity.Y + num189 * 2f;
								}
								else {
									NPC.velocity.Y = NPC.velocity.Y - num189 * 2f;
								}
							}
							if ((double)System.Math.Abs(num191) < (double)num188 * 0.2 && (NPC.velocity.Y > 0f && num192 < 0f || NPC.velocity.Y < 0f && num192 > 0f)) {
								if (NPC.velocity.X > 0f) {
									NPC.velocity.X = NPC.velocity.X + num189 * 2f;
								}
								else {
									NPC.velocity.X = NPC.velocity.X - num189 * 2f;
								}
							}
						}
						else {
							if (num196 > num197) {
								if (NPC.velocity.X < num191) {
									NPC.velocity.X = NPC.velocity.X + num189 * 1.1f;
								}
								else if (NPC.velocity.X > num191) {
									NPC.velocity.X = NPC.velocity.X - num189 * 1.1f;
								}
								if ((double)(System.Math.Abs(NPC.velocity.X) + System.Math.Abs(NPC.velocity.Y)) < (double)num188 * 0.5) {
									if (NPC.velocity.Y > 0f) {
										NPC.velocity.Y = NPC.velocity.Y + num189;
									}
									else {
										NPC.velocity.Y = NPC.velocity.Y - num189;
									}
								}
							}
							else {
								if (NPC.velocity.Y < num192) {
									NPC.velocity.Y = NPC.velocity.Y + num189 * 1.1f;
								}
								else if (NPC.velocity.Y > num192) {
									NPC.velocity.Y = NPC.velocity.Y - num189 * 1.1f;
								}
								if ((double)(System.Math.Abs(NPC.velocity.X) + System.Math.Abs(NPC.velocity.Y)) < (double)num188 * 0.5) {
									if (NPC.velocity.X > 0f) {
										NPC.velocity.X = NPC.velocity.X + num189;
									}
									else {
										NPC.velocity.X = NPC.velocity.X - num189;
									}
								}
							}
						}
					}
				}
				NPC.rotation = (float)System.Math.Atan2((double)NPC.velocity.Y, (double)NPC.velocity.X) + 1.57f;
				if (head) {
					if (flag18) {
						if (NPC.localAI[0] != 1f) {
							NPC.netUpdate = true;
						}
						NPC.localAI[0] = 1f;
					}
					else {
						if (NPC.localAI[0] != 0f) {
							NPC.netUpdate = true;
						}
						NPC.localAI[0] = 0f;
					}
					if ((NPC.velocity.X > 0f && NPC.oldVelocity.X < 0f || NPC.velocity.X < 0f && NPC.oldVelocity.X > 0f || NPC.velocity.Y > 0f && NPC.oldVelocity.Y < 0f || NPC.velocity.Y < 0f && NPC.oldVelocity.Y > 0f) && !NPC.justHit) {
						NPC.netUpdate = true;
						return;
					}
				}
			}
			CustomBehavior();
		}

		public virtual void Init() {
		}

		public virtual bool ShouldRun() {
			return false;
		}

		public virtual void CustomBehavior() {
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			return head ? (bool?)null : false;
		}
	}
}
