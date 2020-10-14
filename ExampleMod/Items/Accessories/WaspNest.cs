using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Mono.Cecil.Cil;

namespace ExampleMod.Items.Accessories
{
	[AutoloadEquip(EquipType.Back)]
	class WaspNest : ModItem
	{
		public override bool Autoload(ref string name)
        {
            IL.Terraria.Player.beeType += HookBeeType;
            return base.Autoload(ref name);
		}

		// This IL editing (Intermediate Language editing) example is walked through in the guide: https://github.com/tModLoader/tModLoader/wiki/Expert-IL-Editing#example---hive-pack-upgrade
		private static int implementation;
		private void HookBeeType(ILContext il)
		{
			var c = new ILCursor(il);

			// Try to find where 566 is placed onto the stack
			if (!c.TryGotoNext(i => i.MatchLdcI4(566)))
				return; // Patch unable to be applied

            // Showcase different patch approaches
            if (implementation == 0)
			{
				// Move the cursor after 566 and onto the ret op.
				c.Index++;
				// Push the Player instance onto the stack
				c.Emit(OpCodes.Ldarg_0);
				// Call a delegate using the int and Player from the stack.
				c.EmitDelegate<Func<int, Player, int>>((returnValue, player) =>
				{
					// Regular c# code
					if (player.GetModPlayer<ExamplePlayer>().strongBeesUpgrade && Main.rand.NextBool(10) && Main.ProjectileUpdateLoopIndex == -1)
						return ProjectileID.Beenade;
					return returnValue;
				});
			}
			else if (implementation == 1)
			{
				// Make a label to use later
				var label = c.DefineLabel();
				// Push the Player instance onto the stack
				c.Emit(OpCodes.Ldarg_0);
				// Call a delegate popping the Player from the stack and pushing a bool
				c.EmitDelegate<Func<Player, bool>>(player => player.GetModPlayer<ExamplePlayer>().strongBeesUpgrade && Main.rand.NextBool(10) && Main.ProjectileUpdateLoopIndex == -1);
				// if the bool on the stack is false, jump to label
				c.Emit(OpCodes.Brfalse, label);
				// Otherwise, push ProjectileID.Beenade and return
				c.Emit(OpCodes.Ldc_I4, ProjectileID.Beenade);
				c.Emit(OpCodes.Ret);
				// Set the label to the current cursor, which is still the instruction pushing 566 (which is followed by Ret)
				c.MarkLabel(label);
			}
			else
			{
				var label = c.DefineLabel();

				// Here we simply adapt the dnSpy output. This approach is tedious but easier if you don't understand IL instructions.
				c.Emit(OpCodes.Ldarg_0);
				// We need to make sure to pass in FieldInfo or MethodInfo into Call instructions.
				// Here we show how to retrieve a generic version of a MethodInfo
				c.Emit(OpCodes.Call, typeof(Player).GetMethod("GetModPlayer", new Type[] { }).MakeGenericMethod(typeof(ExamplePlayer)));
				// nameof helps avoid spelling mistakes
				c.Emit(OpCodes.Ldfld, typeof(ExamplePlayer).GetField(nameof(ExamplePlayer.strongBeesUpgrade)));
				c.Emit(OpCodes.Brfalse_S, label);
				c.Emit(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.rand)));
				// Ldc_I4_S expects an int8, aka an sbyte. Failure to cast correctly will crash the game
				c.Emit(OpCodes.Ldc_I4_S, (sbyte)10);
				c.Emit(OpCodes.Call, typeof(Utils).GetMethod("NextBool", new Type[] { typeof(Terraria.Utilities.UnifiedRandom), typeof(int) }));
				c.Emit(OpCodes.Brfalse_S, label);
				// You may be tempted to write c.Emit(Ldsfld, Main.ProjectileUpdateLoopIndex);, this won't work and will simply use the value of the field at patch time. That will crash.
				c.Emit(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.ProjectileUpdateLoopIndex)));
				c.Emit(OpCodes.Ldc_I4_M1);
				c.Emit(OpCodes.Bne_Un_S, label);
				c.Emit(OpCodes.Ldc_I4, ProjectileID.Beenade);
				c.Emit(OpCodes.Ret);
				// As Emit has been inserting and advancing the cursor index, we are still pointing at the 566 instruction. 
				// All the branches in the dnspy output jumped to this instruction, so we set the label to this instruction.
				c.MarkLabel(label);
			}


            // change implementation every time the mod is reloaded
            implementation = (implementation+1)%3;
        }

        public override void SetStaticDefaults()
		{
			// We can use vanilla language keys to copy the tooltip from HiveBackpack
			Tooltip.SetDefault("{$ItemTooltip.HiveBackpack}");
		}

		public override void SetDefaults()
		{
			sbyte realBackSlot = item.backSlot;
			item.CloneDefaults(ItemID.HiveBackpack);
			item.value = Item.sellPrice(0, 5, 0, 0);
			// CloneDefaults will clear out the autoloaded Back slot, so we need to preserve it this way.
			item.backSlot = realBackSlot;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			// The original Hive Pack sets strongBees.
			player.strongBees = true;
			// Here we add an additional effect
			player.GetModPlayer<ExamplePlayer>().strongBeesUpgrade = true;
		}
	}
}
