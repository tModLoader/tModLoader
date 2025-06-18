using ExampleMod.Common.Players;
using ExampleMod.Common.Systems;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod
{
	// This is a partial class, meaning some of its parts were split into other files. See ExampleMod.*.cs for other portions.
	// The class is partial to organize similar code together to clarify what is related.
	// This class extends from the Mod class as seen in ExampleMod.cs. Make sure to extend from the mod class, ": Mod", in your own code if using this file as a template for you mods Mod class.
	partial class ExampleMod
	{
		// The following code allows other mods to "call" Example Mod data.
		// This allows mod developers to access Example Mod's data without having to set it a reference.
		// Mod calls are not exposed by default, so it will be up to you to publish appropriate calls for your mod, and what values they return.
		public override object Call(params object[] args) {
			// Make sure the call doesn't include anything that could potentially cause exceptions.
			if (args is null) {
				throw new ArgumentNullException(nameof(args));
			}

			if (args.Length == 0) {
				throw new ArgumentException("Arguments cannot be empty!");
			}

			// This check makes sure that the argument is a string using pattern matching.
			// Since we only need one parameter, we'll take only the first item in the array..
			if (args[0] is string content) {
				// ..And treat it as a command type.
				switch (content) {
					case "downedMinionBoss":
						// Returns the value provided by downedMinionBoss, if the argument calls for it.
						return DownedBossSystem.downedMinionBoss;
					case "showMinionCount":
						// Returns the value provided by showMinionCount, if the argument calls for it.
						return Main.LocalPlayer.GetModPlayer<ExampleInfoDisplayPlayer>().showMinionCount;
					case "setMinionCount":
						// We need to make sure the call is provided with a value to set the field to.
						if (args[1] is not bool minionSet) {
							// If it's not the type we need, we can't continue
							// Tell the developer what type we need, and what we got instead.
							throw new Exception($"Expected an argument of type bool when setting minion count, but got type {args[1].GetType().Name} instead.");
						}

						// We'll set the value to what the argument provided.
						// Optionally, you can return a value indicating that the assignment was successful.
						// return true;
						Main.LocalPlayer.GetModPlayer<ExampleInfoDisplayPlayer>().showMinionCount = minionSet;

						// Return a 'true' boolean as one of the many ways to tell that the operation succeeded.
						return true;
				}
			}

			// We can also do this with different data types.
			if (args[0] is int contentInt && contentInt == 4) {
				return ModContent.GetInstance<ExampleBiomeTileCount>().exampleBlockCount;
			}

			// If the arguments provided don't match anything we wanted to return a value for, we'll return a 'false' boolean.
			// This value can be anything you would like to provide as a default value.
			return false;
		}
	}
}