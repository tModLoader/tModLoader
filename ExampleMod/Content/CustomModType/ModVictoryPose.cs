using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.CustomModType
{
	// Note: To fully understand this example, please start by reading https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Content/CustomModType/README.md

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

		/// <inheritdoc cref="VictoryPosePlayer.ElapsedPoseTime"/>
		public static float ElapsedPoseTime(Player player) => player.GetModPlayer<VictoryPosePlayer>().ElapsedPoseTime;

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

		// TODO: A real implementation might want to add hooks to support additional conditions or customizable spawn rates
	}
}
