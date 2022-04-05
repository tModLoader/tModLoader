using System;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Terraria.Achievements
{
	/// <summary>
	/// Modded achievements should set their data in <see cref="ModType.SetStaticDefaults"/>.
	/// </summary>
	partial class Achievement
	{
		// Give vanilla achievements a normal name.
		public override string Name => IsModded() ? base.Name : _name;

		// Give vanilla achievements the normal massive atlas of textures.
		public override string Texture => IsModded() ? base.Texture : "Images/UI/Achievements";

		// Parameterless constructor for modded achievements.
		// Vanilla ones using Achievement(string) which is located in the main class.
		internal Achievement() {
		}

		// Implementation of Terraria.ModLoader.ModType
		protected override void Register() {
			int id = LoaderManager.Get<AchievementLoader>().Register(this);

			if (id != Id) {
				throw new Exception(
					$"An achievement ID mismatch occurred. Achievement \"{Name}\" was registered with an ID of {id}, but was instantiated with an ID of {Id}.");
			}

			FriendlyName = LocalizationLoader.GetOrCreateTranslation(Mod, $"AchievementName.{Name}");
			Description = LocalizationLoader.GetOrCreateTranslation(Mod, $"AchievementDescription.{Name}");

			Main.Achievements.Register(this);
		}

		/// <summary>
		/// Whether this achievement originates from a mod.
		/// </summary>
		/// <returns></returns>
		public bool IsModded() {
			return Id > AchievementID.Count;
		}

		internal string GetFriendlyName() {
			return FriendlyName.GetTranslation(LanguageManager.Instance.ActiveCulture);
		}

		internal string GetDescription() {
			return Description.GetTranslation(LanguageManager.Instance.ActiveCulture);
		}
	}
}