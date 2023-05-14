using ExampleMod.Content.Items.Placeable;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace ExampleMod.Content.Buffs
{
	/// <summary>
	/// This is similar to <see cref="ExampleTrap.ExampleTrapLoader"/> except it looks for all classes that inherit <see cref="MinecartBuffBase"/> and instantiates two instances for them, one for both directions
	/// </summary>
	public sealed class MinecartBuffLoader : ILoadable
	{
		private static IEnumerable<Type> FindTypes(Mod mod) {
			return AssemblyManager.GetLoadableTypes(mod.Code)
				.Where(t => !t.IsAbstract && t.IsClass && t.IsSubclassOf(typeof(MinecartBuffBase)));
		}

		private static MinecartBuffBase CreateInstance(Type type, bool left) {
			return (MinecartBuffBase)Activator.CreateInstance(type, new object[] { left });
		}

		public void Load(Mod mod) {
			foreach (var type in FindTypes(mod)) {
				mod.AddContent(CreateInstance(type, true));
				mod.AddContent(CreateInstance(type, false));
			}
		}

		public void Unload() {
		}
	}

	// Buffs inheriting from this class will generate two instances suffixed with _Left and _Right, use them accordingly with Mod.Find<ModBuff>(name).Type, ModContent.BuffType<name>() won't (!) work
	// You only need the classes in this file once if you add more than one minecart
	/// <summary>
	/// The base class for all buffs used for minecarts. Requires 
	/// <br/>See <see cref="ExampleTrap"/> for a more in-depth explanation of manual loading
	/// </summary>
	//[Autoload(false)] // This and inheriting classes do not have a parameterless constructor so they won't be autoloaded anyway
	public abstract class MinecartBuffBase : ModBuff
	{
		public bool Left { get; init; }

		public abstract int MountType { get; }

		public MinecartBuffBase(bool left) {
			Left = left;
		}

		public string Suffix => Left ? "Left" : "Right";

		public string TypeName => GetType().Name;

		public string LocalizationKey => Mod.GetLocalizationKey($"{LocalizationCategory}.{TypeName}");

		public override string Name => $"{TypeName}_{Suffix}";

		// Use the same texture for both variants
		public override string Texture => (GetType().Namespace + "." + TypeName).Replace('.', '/');

		// Use the vanilla DisplayName for both variants
		//public override LocalizedText DisplayName => Language.GetText($"BuffName.Minecart{Suffix}");

		// But for the sake of example, we want a custom name, hence why we choose a key that is the same for both variants
		public override LocalizedText DisplayName => Language.GetOrRegister($"{LocalizationKey}.DisplayName");

		// Use the vanilla Description for both variants
		public override LocalizedText Description => Language.GetText($"BuffDescription.Minecart{Suffix}");

		public sealed override void SetStaticDefaults() {
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;

			// Handles automatically mounting the player within Update (no need to write yourself like in ExampleMountBuff)
			BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
				mountID = MountType,
				faceLeft = Left
			};
		}
	}
}
