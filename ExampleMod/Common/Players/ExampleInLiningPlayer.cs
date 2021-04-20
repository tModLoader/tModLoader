using Terraria;
using Terraria.ModLoader;
using System;
using System.Linq;

namespace ExampleMod.Common.Players
{
	public class ExampleInLiningPlayer : ModPlayer
	{
		public void DoStuff(int context) {
			//Mod.Logger.Debug($"Regen Hook called. ExampleInventoryPlayer hashcode: {typeof(ExampleInventoryPlayer).GetHashCode()}, context: {context}");
			var t = ModContent.GetInstance<ExampleInventoryPlayer>();
			var n = t.FullName;
		}

		public static Action<int> DoingStuff = (context) => {
			//Logging.publicLogger.Debug($"Regen Hook called. ExampleInventoryPlayer hashcode: {typeof(ExampleInventoryPlayer).GetHashCode()}, context: {context}");
			var t = ModContent.GetInstance<ExampleInventoryPlayer>();
			var n = t.FullName;
		};

		public Action<int> DidStuff = (context) => {
			//Logging.publicLogger.Debug($"Regen Hook called. ExampleInventoryPlayer hashcode: {typeof(ExampleInventoryPlayer).GetHashCode()}, context: {context}");
			var t = ModContent.GetInstance<ExampleInventoryPlayer>();
			var n = t.FullName;
		};

		public static void DoneStuff(int context) {
			//Logging.publicLogger.Debug($"Regen Hook called. ExampleInventoryPlayer hashcode: {typeof(ExampleInventoryPlayer).GetHashCode()}, context: {context}");
			var t = ModContent.GetInstance<ExampleInventoryPlayer>();
			var n = t.FullName;
		}
		public Action<int> Do_DoneStuff = (context) => DoneStuff(context);

		public override void Load() {
			var context = typeof(ExampleInventoryPlayer).GetHashCode();
			//Logging.publicLogger.Debug($"Registering hook. ExampleInventoryPlayer hashcode: {context}");

			Action<int> lambda = (con) => {
				//Mod.Logger.Debug($"Biome Hook called. ExampleInventoryPlayer hashcode: {typeof(ExampleInventoryPlayer).GetHashCode()}, context: {context}");
				var t = ModContent.GetInstance<ExampleInventoryPlayer>();
				var n = t.FullName;
			};

			// This local variable version works fine. No null issues.
			On.Terraria.Player.UpdateBiomes += (orig, player) => lambda(context);

			// This class method version does not work. It tosses an error as t is returned as null.
			//On.Terraria.Player.UpdateLifeRegen += (orig, player) => DoStuff(context);

			// This static field variant is also invalid. 
			//On.Terraria.Player.UpdateLifeRegen += (orig, player) => DoingStuff(context);

			// However, these field variants are VALID
			On.Terraria.Player.UpdateLifeRegen += (orig, player) => DidStuff(context);
			On.Terraria.Player.UpdateManaRegen += (orig, player) => Do_DoneStuff(context);

			// This compact style does not work. It tosses an error as t is returned as null.
			//On.Terraria.Player.UpdateManaRegen += (orig, player) => {
			//	Logging.Terraria.Debug($"Mana Hook called. ExampleInventoryPlayer hashcode: {typeof(ExampleInventoryPlayer).GetHashCode()}, context: {context}");
			//	var t = ModContent.GetInstance<ExampleInventoryPlayer>();
			//	var n = t.FullName;
			//};
		}
	}
}
