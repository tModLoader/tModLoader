using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;
using Terraria.GameContent.Personalities;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a biome added by a mod. It exists to centralize various biome related hooks, handling a lot of biome boilerplate.
	/// </summary>
	public abstract class ModBiome : ModSceneEffect, IShoppingBiome
	{
		// Basic Biome information

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
		public override int Music => 0;

		internal int ZeroIndexType => Type; // - PrimaryBiomeID.Count;

		// Bestiary properties
		/// <summary>
		/// The display name for this biome in the bestiary.
		/// </summary>
		public ModTranslation DisplayName { get; private set; }
		/// <summary>
		/// The path to the 30x30 texture that will appear for this biome in the bestiary. Defaults to adding "_Icon" onto the usual namespace+classname derived texture path.
		/// <br/> Vanilla icons use a drop shadow at 40 percent opacity and the texture will be offset 1 pixel left and up from centered in the bestiary filter grid.
		/// </summary>
		public virtual string BestiaryIcon => (GetType().Namespace + "." + Name + "_Icon").Replace('.', '/');
		/// <summary>
		/// The path to the background texture that will appear for this biome behind npcs in the bestiary. Defaults to adding "_Background" onto the usual namespace+classname derived texture path.</summary>
		/// </summary>
		public virtual string BackgroundPath => (GetType().Namespace + "." + Name + "_Background").Replace('.', '/');
		/// <summary>
		/// The color of the bestiary background.
		/// </summary>
		public virtual Color? BackgroundColor => null;

		public GameContent.Bestiary.ModBiomeBestiaryInfoElement ModBiomeBestiaryInfoElement { get; internal set; }

		string IShoppingBiome.NameKey => Name;

		protected sealed override void Register() {
			Type = LoaderManager.Get<BiomeLoader>().Register(this);
			RegisterSceneEffect(this);

			DisplayName = LocalizationLoader.GetOrCreateTranslation(Mod, $"BiomeName.{Name}");
		}

		public sealed override void SetupContent() {
			if (DisplayName.IsDefault())
				DisplayName.SetDefault(Regex.Replace(Name, "([A-Z])", " $1").Trim());

			SetStaticDefaults();
			
			ModBiomeBestiaryInfoElement = new GameContent.Bestiary.ModBiomeBestiaryInfoElement(Mod, DisplayName.Key, BestiaryIcon, BackgroundPath, BackgroundColor);
		}

		/// <summary>
		/// IsSceneEffectActive is auto-forwarded to read the result of IsBiomeActive.
		/// Do not need to implement when creating your ModBiome.
		/// </summary>
		public sealed override bool IsSceneEffectActive(Player player) => player.modBiomeFlags[ZeroIndexType];

		/// <summary>
		/// This is where you can set values for DisplayName.
		/// </summary>
		public override void SetStaticDefaults() {
		}

		/// <summary>
		/// Return true if the player is in the biome.
		/// </summary>
		/// <returns></returns>
		public virtual bool IsBiomeActive(Player player) => false;

		/// <summary>
		/// Override this hook to make things happen when the player enters the biome.
		/// </summary>
		public virtual void OnEnter(Player player) {
		}

		/// <summary>
		/// Override this hook to make things happen when the player is in the biome.
		/// </summary>
		public virtual void OnInBiome(Player player) {}

		/// <summary>
		/// Override this hook to make things happen when the player leaves the biome.
		/// </summary>
		public virtual void OnLeave(Player player) {
		}

		bool IShoppingBiome.IsInBiome(Player player) => IsBiomeActive(player);
	}
}
