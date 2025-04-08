using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Reflection;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.CustomModType
{
	// This file shows implementing a custom ModType (or new content type).
	// There are many benefits to using a ModType class for making new content.
	// The main benefit is that loading the content is handled using the same logic as existing content types, so other modders will be able to use them as expected as if they were provided by tModLoader itself.
	// Other modders will simply need to inherit from the base class, no need for Mod.Call to interface with the mod providing the ModType. (They will, of course, also need to have a reference to this mod.)
	// This also includes loading a primary texture, automatic localization registration, being able to use ModContent.Find and ModContent.GetInstance, support for custom ID sets, and support for manually loading multiple instances of a class.

	// The ModVictoryPose class is the custom ModType that other mods will inherit from. Think of it like how modders inherit from ModItem to add a new item to the game. A Victory Pose is a special effect that plays after defeating a boss enemy.
	// HandsUpVictoryPose, HandsUpWithFireworksVictoryPose, and NonAutoloadVictoryPose are all "content" added by this mod. They are the default victory poses available unless other mods add their own.
	
	// VictoryPosePlayer handles applying and updating the active ModVictoryPose. 

	// VictoryPoseLoader handles tracking all registered ModVictoryPose

	// VictoryPoseAdditionalLoader manually loads 2 versions of NonAutoloadVictoryPose.

	// VictoryPoseID and VictoryPoseID.Sets manage the ID sets.

	public class VictoryPoseID
	{
		[ReinitializeDuringResizeArrays]
		public static class Sets
		{
			public static SetFactory Factory = new SetFactory(VictoryPoseLoader.VictoryPoseCount, "ExampleMod/VictoryPoseID", Search);

			public static bool[] NonBoss = Factory.CreateNamedSet("NonBoss")
				.Description("Victory poses in this set are options to be chosen when defeating a regular enemy")
				.RegisterBoolSet(false);
		}

		public static IdDictionary Search = IdDictionary.Create<VictoryPoseID, int>();
	}

	/// <summary>
	/// A pose the player will hold after defeating an enemy. A pose will be triggered for each boss kill and rarely for regular enemies. When triggered, a message with <see cref="VictoryCheer"/> will appear above the player and the associated texture will be displayed as well.
	/// <para/> Use <see cref="Update(Player)"/> to add additional effects, such as setting <see cref="Player.body"/>'s <c>Y</c> to specific animation frames to hold a "pose", spawning dust and projectiles, or playing sounds.
	/// <para/> ModVictoryPose is intended to teach various concepts about custom ModType implementations, it is a contrived example and we do not expect other mods to actually use it.
	/// </summary>
	public abstract class ModVictoryPose : ModTexturedType, ILocalizedModType // Note: ModTexturedType inherits from ModType.
	{
		/// <summary>
		/// The internal ID of this <see cref="ModVictoryPose"/>.
		/// While not required, ModTypes typically assign an ID since it is convenient to be able to reference content by a number, such as with arrays or network code.
		/// </summary>
		public int Type { get; internal set; }

		// Properties relating to the ModVictoryPose. Modders set PoseTime in SetStaticDefaults.
		/// <summary> How long the pose will last. Defaults to 60 (1 second). </summary>
		public int PoseTime { get; set; } = 60;
		/// <summary> How long the pose has been active </summary>
		public int ElapsedPoseTime { get; internal set; }

		// Since this implements ILocalizedModType, all keys from this ModType will default to using Mods.ModName.VictoryPoses.ClassName.KeyName. This should be unique to avoid conflicts with other mods
		public virtual string LocalizationCategory => "VictoryPoses";
		public virtual LocalizedText VictoryCheer => this.GetLocalization(nameof(VictoryCheer), PrettyPrintName);

		// When making a custom ModType, we override and seal Register.
		// In Register, we do all the "plumbing code" needed to load and register the content
		protected sealed override void Register() {
			ModTypeLookup<ModVictoryPose>.Register(this);
			Type = VictoryPoseLoader.Add(this);
		}

		// When making a custom ModType, we override and seal SetupContent.
		// In SetupContent, we do all the "plumbing code" needed to initialize and setup the content
		public sealed override void SetupContent() {
			ModContent.Request<Texture2D>(Texture); // Ensure that the texture exists. Doing this here means that the mod won't load rather than crash in-game.
			_ = VictoryCheer; // By calling this here, we ensure that the localization key is populated into the localization files.
			VictoryPoseID.Search.Add(FullName, Type); // Populate the Search IdDictionary
			SetStaticDefaults(); // Finally, we call SetStaticDefaults, where each ModVictoryPose class will implement their specific logic.
		}

		// These virtual methods are the "hooks" we provide that other modders can use to customize their ModVictoryPose behaviors.
		// It is useful to document these methods so that other mods using it can understand what each method does. They can view the documentation by hovering over the method name. Typing "///" in Visual Studio will generate a documentation stub, after that add any information you want to the "summary" section. See https://github.com/tModLoader/tModLoader/wiki/Why-Use-an-IDE#mod-documentation for more information about how to enable documentation support in your mod.
		// If your mod has a GitHub wiki, you might want to also document these methods there as well as an additional reference.

		/// <summary>
		/// Called when the pose starts for the given player.
		/// </summary>
		public virtual void OnStartPose(Player player) {
		}

		/// <summary>
		/// Called each game update while the pose is active.
		/// </summary>
		public virtual void Update(Player player) {
		}

		/// <summary>
		/// Called when the pose is ending.
		/// </summary>
		public virtual void OnEndPose(Player player) {
		}

		/// <summary>
		/// Use to control the frame of the texture to display.
		/// </summary>
		public virtual Rectangle? GetTextureFrame(Asset<Texture2D> texture) {
			return null;
		}

		// TODO: A real implementation might want to add support for conditions or customizable spawn rates
	}

	public class VictoryPosePlayer : ModPlayer
	{
		public ModVictoryPose activeVictoryPose;

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			// Detect when we defeat an enemy.
			if (!target.active) {
				// Start a victory pose
				StartRandomPose(target.boss);
			}
		}

		private void StartRandomPose(bool boss) {
			// Always pose after defeating a boss, pose 1 out of 100 times for normal enemies.
			if(!boss && Main.rand.NextBool(99, 100)) {
				return;
			}

			if (activeVictoryPose != null) {
				return; // Don't interrupt an ongoing pose
			}

			if (boss) {
				// Choose from all the poses
				activeVictoryPose = Main.rand.Next(VictoryPoseLoader.VictoryPoses);
			}
			else {
				// Choose from only the NonBoss set
				int randomNonBossVictoryPoseIndex = Main.rand.Next(VictoryPoseID.Sets.NonBoss.GetTrueIndexes());
				activeVictoryPose = VictoryPoseLoader.VictoryPoses[randomNonBossVictoryPoseIndex];
			}
			// Reset timers.
			activeVictoryPose.ElapsedPoseTime = 0;
			activeVictoryPose.OnStartPose(Player);

			ChatHelper.DisplayMessage(activeVictoryPose.VictoryCheer.ToNetworkText(), Color.White, (byte)Main.myPlayer);

			Asset<Texture2D> texture = ModContent.Request<Texture2D>(activeVictoryPose.Texture);
			Rectangle? frame = activeVictoryPose.GetTextureFrame(texture);
			Main.ParticleSystem_World_BehindPlayers.Add(new PoseIconParticle(texture, frame, new Vector2(Main.rand.NextFloat(-2, 2), -5f), Player.Center - new Vector2(0, 80), Main.rand.NextFloat(-0.03f, 0.03f)) {
				AccelerationPerFrame = new Vector2(0f, 0.16350001f),
				ScaleOffsetPerFrame = 1f / 60f,
			});

			// TODO: A real implementation would want to sync the pose to other clients with a ModPacket
		}

		public override void PostUpdate() {
			// Here we manage the lifetime of a victory pose
			if (activeVictoryPose == null) {
				return;
			}

			activeVictoryPose.Update(Player);
			activeVictoryPose.ElapsedPoseTime++;
			if (activeVictoryPose.ElapsedPoseTime >= activeVictoryPose.PoseTime) {
				activeVictoryPose.OnEndPose(Player);
				activeVictoryPose = null;
			}
		}
	}

	public class VictoryPoseLoader : ILoadable
	{
		internal static readonly List<ModVictoryPose> VictoryPoses = [];

		internal static int VictoryPoseCount { get; set; } = 0;

		internal static int Add(ModVictoryPose victoryPose) {
			VictoryPoses.Add(victoryPose);
			return VictoryPoseCount++;
			// or return VictoryPoses.Count - 1;
		}

		public void Load(Mod mod) {
		}

		public void Unload() {
		}
	}

	public class VictoryPoseAdditionalLoader : ILoadable
	{
		public void Load(Mod mod) {
			// Manually load additional ModVictoryPose from this mod.
			mod.AddContent(new NonAutoloadVictoryPose("ShortPose", 120));
			mod.AddContent(new NonAutoloadVictoryPose("LongPose", 180));
		}

		public void Unload() {
		}
	}

	public class HandsUpVictoryPose : ModVictoryPose
	{
		public override void SetStaticDefaults() {
			VictoryPoseID.Sets.NonBoss[Type] = true;
		}

		public override void OnStartPose(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				Main.blockMouse = true;
			}
		}

		public override void Update(Player player) {
			if (player.itemAnimation == 0) {
				player.bodyFrame.Y = player.bodyFrame.Height * 5; // 2 Hands up / falling
			}
		}
	}

	public class HandsUpWithFireworksVictoryPose : ModVictoryPose
	{
		public override void SetStaticDefaults() {
			PoseTime = 180;
		}

		public override void OnStartPose(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				Main.blockMouse = true;
			}
		}

		public override void Update(Player player) {
			if (player.itemAnimation == 0) {
				player.bodyFrame.Y = player.bodyFrame.Height * 5; // 2 Hands up / falling
			}

			if(ElapsedPoseTime == 30 || ElapsedPoseTime == 60) {
				SpawnFirework(player);
				SoundEngine.PlaySound(SoundID.Thunder);
			}
		}

		public override void OnEndPose(Player player) {
			for (int i = 0; i < 3; i++) {
				SpawnFirework(player);
			}
		}

		private void SpawnFirework(Player player) {
			if (player.whoAmI != Main.myPlayer) {
				return;
			}

			int fireworkProjectile = ProjectileID.RocketFireworksBoxRed + Main.rand.Next(4);
			Projectile.NewProjectile(player.GetSource_FromThis(), player.Top, new Vector2(Main.rand.NextFloat(-2, 2), -Main.rand.NextFloat(2f, 4f)).RotatedByRandom(0.3f), fireworkProjectile, 0, 0, Main.myPlayer);
		}
	}

	// This class doesn't autoload because it has a non-default constructor, it is loaded manually 2 times in VictoryPoseAdditionalLoader.Load
	public class NonAutoloadVictoryPose : ModVictoryPose
	{
		private readonly string nameOverride;
		private readonly int duration;

		public override string Name => nameOverride;

		public override string Texture => "ExampleMod/Content/Items/ExampleTooltipsItem"; // A texture with 4 smileys

		public NonAutoloadVictoryPose(string name, int duration) {
			this.nameOverride = name;
			this.duration = duration;
		}

		public override void SetStaticDefaults() {
			PoseTime = duration;

			VictoryPoseID.Sets.NonBoss[Type] = true;
		}

		public override Rectangle? GetTextureFrame(Asset<Texture2D> texture) {
			// Randomly choose one of the 4 faces in the texture.
			return texture.Frame(1, 4, 0, Main.rand.Next(4));
		}

		public override void OnStartPose(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				Main.blockMouse = true;
			}
		}

		public override void Update(Player player) {
			if (player.itemAnimation == 0) {
				player.bodyFrame.Y = player.bodyFrame.Height; // 1 Hand up
			}
		}
	}

	// A slightly tweaked CreativeSacrificeParticle. IParticle are similar to Dust, but can be customized completely. 
	public class PoseIconParticle : IParticle
	{
		public Vector2 AccelerationPerFrame;
		public Vector2 Velocity;
		public float rotationRate;
		public Vector2 LocalPosition;
		public float ScaleOffsetPerFrame;
		public float StopWhenAboveXScale;
		private Asset<Texture2D> texture;
		private Rectangle frame;
		private Vector2 origin;
		private float scale;
		private float rotation;

		public bool ShouldBeRemovedFromRenderer { get; private set; }

		public PoseIconParticle(Asset<Texture2D> textureAsset, Rectangle? frame, Vector2 initialVelocity, Vector2 initialLocalPosition, float rotationRate) {
			texture = textureAsset;
			this.frame = frame ?? texture.Frame();
			origin = this.frame.Size() / 2f;
			Velocity = initialVelocity;
			LocalPosition = initialLocalPosition;
			StopWhenAboveXScale = 3f;
			ShouldBeRemovedFromRenderer = false;
			scale = 0.6f;
			this.rotationRate = rotationRate;
		}

		public void Update(ref ParticleRendererSettings settings) {
			Velocity += AccelerationPerFrame;
			LocalPosition += Velocity;
			scale += ScaleOffsetPerFrame;
			rotation += rotationRate;
			if (scale >= StopWhenAboveXScale) {
				ShouldBeRemovedFromRenderer = true;
			}
		}

		public void Draw(ref ParticleRendererSettings settings, SpriteBatch spriteBatch) {
			Color color = Color.Lerp(Color.White, new Color(255, 255, 255, 0), Utils.Clamp(scale, 0, 1f));
			spriteBatch.Draw(texture.Value, settings.AnchorPosition + LocalPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0f);
		}
	}
}
