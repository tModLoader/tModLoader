using System;
using Terraria.ID;
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

			Main.Achievements.Register(this);
		}

		/// <summary>
		/// Whether this achievement originates from a mod.
		/// </summary>
		/// <returns></returns>
		public bool IsModded() {
			return Id > AchievementID.Count;
		}
	}
}