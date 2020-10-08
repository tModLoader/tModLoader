using ExampleMod.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles
{
	public class ExampleLastPrismHoldout : ModProjectile
	{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.LastPrism;

		// The vanilla Last Prism is an animated item with 5 frames of animation. We copy that here.
		private const int NumAnimationFrames = 5;
		
		// This controls how many individual beams are fired by the Prism.
		public const int NumBeams = 10;

		// This value controls how many frames it takes for the Prism to reach "max charge". 60 frames = 1 second.
		public const float MaxCharge = 180f;

		// This value controls how many frames it takes for the beams to begin dealing damage. Before then they can't hit anything.
		public const float DamageStart = 30f;

		// This value controls how sluggish the Prism turns while being used. Vanilla Last Prism is 0.08f.
		// Higher values make the Prism turn faster.
		private const float AimResponsiveness = 0.08f;

		// This value controls how frequently the Prism emits sound once it's firing.
		private const int SoundInterval = 20;

		// These values place caps on the mana consumption rate of the Prism.
		// When first used, the Prism consumes mana once every MaxManaConsumptionDelay frames.
		// Every time mana is consumed, the pace becomes one frame faster, meaning mana consumption smoothly increases.
		// When capped out, the Prism consumes mana once every MinManaConsumptionDelay frames.
		private const float MaxManaConsumptionDelay = 15f;
		private const float MinManaConsumptionDelay = 5f;

		// This property encloses the internal AI variable projectile.ai[0]. It makes the code easier to read.
		private float FrameCounter {
			get => projectile.ai[0];
			set => projectile.ai[0] = value;
		}

		// This property encloses the internal AI variable projectile.ai[1].
		private float NextManaFrame {
			get => projectile.ai[1];
			set => projectile.ai[1] = value;
		}

		// This property encloses the internal AI variable projectile.localAI[0].
		// localAI is not automatically synced over the network, but that does not cause any problems in this case.
		private float ManaConsumptionRate {
			get => projectile.localAI[0];
			set => projectile.localAI[0] = value;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Example Last Prism");
			Main.projFrames[projectile.type] = NumAnimationFrames;

			// Signals to Terraria that this projectile requires a unique identifier beyond its index in the projectile array.
			// This prevents the issue with the vanilla Last Prism where the beams are invisible in multiplayer.
			ProjectileID.Sets.NeedsUUID[projectile.type] = true;
		}

		public override void SetDefaults()
		{
			// Use CloneDefaults to clone all basic projectile statistics from the vanilla Last Prism.
			projectile.CloneDefaults(ProjectileID.LastPrism);
		}

		public override void AI()
		{
			Player player = Main.player[projectile.owner];
			Vector2 rrp = player.RotatedRelativePoint(player.MountedCenter, true);

			// Update the Prism's damage every frame so that it is dynamically affected by Mana Sickness.
			UpdateDamageForManaSickness(player);

			// Update the frame counter.
			FrameCounter += 1f;

			// Update projectile visuals and sound.
			UpdateAnimation();
			PlaySounds();

			// Update the Prism's position in the world and relevant variables of the player holding it.
			UpdatePlayerVisuals(player, rrp);

			// Update the Prism's behavior: project beams on frame 1, consume mana, and despawn if out of mana.
			if (projectile.owner == Main.myPlayer)
			{
				// Slightly re-aim the Prism every frame so that it gradually sweeps to point towards the mouse.
				UpdateAim(rrp, player.HeldItem.shootSpeed);

				// player.CheckMana returns true if the mana cost can be paid. Since the second argument is true, the mana is actually consumed.
				// If mana shouldn't consumed this frame, the || operator short-circuits its evaluation player.CheckMana never executes.
				bool manaIsAvailable = !ShouldConsumeMana() || player.CheckMana(player.HeldItem.mana, true, false);

				// The Prism immediately stops functioning if the player is Cursed (player.noItems) or "Crowd Controlled", e.g. the Frozen debuff.
				// player.channel indicates whether the player is still holding down the mouse button to use the item.
				bool stillInUse = player.channel && manaIsAvailable && !player.noItems && !player.CCed;

				// Spawn in the Prism's lasers on the first frame if the player is capable of using the item.
				if (stillInUse && FrameCounter == 1f) {
					FireBeams();
				}

				// If the Prism cannot continue to be used, then destroy it immediately.
				else if (!stillInUse) {
					projectile.Kill();
				}
			}

			// This ensures that the Prism never times out while in use.
			projectile.timeLeft = 2;
		}

		private void UpdateDamageForManaSickness(Player player)
		{
			float ownerCurrentMagicDamage = player.allDamage + (player.magicDamage - 1f);
			projectile.damage = (int)(player.HeldItem.damage * ownerCurrentMagicDamage);
		}

		private void UpdateAnimation()
		{
			projectile.frameCounter++;

			// As the Prism charges up and focuses the beams, its animation plays faster.
			int framesPerAnimationUpdate = FrameCounter >= MaxCharge ? 2 : FrameCounter >= (MaxCharge * 0.66f) ? 3 : 4;

			// If necessary, change which specific frame of the animation is displayed.
			if (projectile.frameCounter >= framesPerAnimationUpdate) {
				projectile.frameCounter = 0;
				if (++projectile.frame >= NumAnimationFrames) {
					projectile.frame = 0;
				}
			}
		}

		private void PlaySounds()
		{
			// The Prism makes sound intermittently while in use, using the vanilla projectile variable soundDelay.
			if (projectile.soundDelay <= 0) {
				projectile.soundDelay = SoundInterval;

				// On the very first frame, the sound playing is skipped. This way it doesn't overlap the starting hiss sound.
				if (FrameCounter > 1f) {
					Main.PlaySound(SoundID.Item15, projectile.position);
				}
			}
		}

		private void UpdatePlayerVisuals(Player player, Vector2 playerHandPos)
		{
			// Place the Prism directly into the player's hand at all times.
			projectile.Center = playerHandPos;
			// The beams emit from the tip of the Prism, not the side. As such, rotate the sprite by pi/2 (90 degrees).
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
			projectile.spriteDirection = projectile.direction;

			// The Prism is a holdout projectile, so change the player's variables to reflect that.
			// Constantly resetting player.itemTime and player.itemAnimation prevents the player from switching items or doing anything else.
			player.ChangeDir(projectile.direction);
			player.heldProj = projectile.whoAmI;
			player.itemTime = 2;
			player.itemAnimation = 2;

			// If you do not multiply by projectile.direction, the player's hand will point the wrong direction while facing left.
			player.itemRotation = (projectile.velocity * projectile.direction).ToRotation();
		}

		private bool ShouldConsumeMana()
		{
			// If the mana consumption timer hasn't been initialized yet, initialize it and consume mana on frame 1.
			if (ManaConsumptionRate == 0f)
			{
				NextManaFrame = ManaConsumptionRate = MaxManaConsumptionDelay;
				return true;
			}

			// Should mana be consumed this frame?
			bool consume = FrameCounter == NextManaFrame;

			// If mana is being consumed this frame, update the rate of mana consumption and write down the next frame mana will be consumed.
			if(consume)
			{
				// MathHelper.Clamp(X,A,B) guarantees that A <= X <= B. If X is outside the range, it will be set to A or B accordingly.
				ManaConsumptionRate = MathHelper.Clamp(ManaConsumptionRate - 1f, MinManaConsumptionDelay, MaxManaConsumptionDelay);
				NextManaFrame += ManaConsumptionRate;
			}
			return consume;
		}

		private void UpdateAim(Vector2 source, float speed)
		{
			// Get the player's current aiming direction as a normalized vector.
			Vector2 aim = Vector2.Normalize(Main.MouseWorld - source);
			if (aim.HasNaNs()) {
				aim = -Vector2.UnitY;
			}

			// Change a portion of the Prism's current velocity so that it points to the mouse. This gives smooth movement over time.
			aim = Vector2.Normalize(Vector2.Lerp(Vector2.Normalize(projectile.velocity), aim, AimResponsiveness));
			aim *= speed;

			if (aim != projectile.velocity) {
				projectile.netUpdate = true;
			}
			projectile.velocity = aim;
		}

		private void FireBeams()
		{
			// If for some reason the beam velocity can't be correctly normalized, set it to a default value.
			Vector2 beamVelocity = Vector2.Normalize(projectile.velocity);
			if (beamVelocity.HasNaNs()) {
				beamVelocity = -Vector2.UnitY;
			}

			// This UUID will be the same between all players in multiplayer, ensuring that the beams are properly anchored on the Prism on everyone's screen.
			int uuid = Projectile.GetByUUID(projectile.owner, projectile.whoAmI);

			int damage = projectile.damage;
			float knockback = projectile.knockBack;
			for (int b = 0; b < NumBeams; ++b) {
				Projectile.NewProjectile(projectile.Center, beamVelocity, ModContent.ProjectileType<ExampleLastPrismBeam>(), damage, knockback, projectile.owner, b, uuid);
			}

			// After creating the beams, mark the Prism as having an important network event. This will make Terraria sync its data to other players ASAP.
			projectile.netUpdate = true;
		}

		// Because the Prism is a holdout projectile and stays glued to its user, it needs custom drawcode.
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			SpriteEffects effects = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Texture2D texture = Main.projectileTexture[projectile.type];
			int frameHeight = texture.Height / Main.projFrames[projectile.type];
			int spriteSheetOffset = frameHeight * projectile.frame;
			Vector2 sheetInsertPosition = (projectile.Center + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition).Floor();

			// The Prism is always at full brightness, regardless of the surrounding light. This is equivalent to it being its own glowmask.
			// It is drawn in a non-white color to distinguish it from the vanilla Last Prism.
			Color drawColor = ExampleLastPrism.OverrideColor;
			spriteBatch.Draw(texture, sheetInsertPosition, new Rectangle?(new Rectangle(0, spriteSheetOffset, texture.Width, frameHeight)), drawColor, projectile.rotation, new Vector2(texture.Width / 2f, frameHeight / 2f), projectile.scale, effects, 0f);
			return false;
		}
	}
}
