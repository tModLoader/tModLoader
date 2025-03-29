using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs;

// This is an example implementation of a stacking buff, similar to the buffs that Beetle Armor can apply
// Our buffs themselves are very simple, so we aren't using an abstract base class for them. However, if you're
//    creating a more complex buff with overrideable properties, you may wish to look at ExamplePrefix and
//    ExampleDerivedPrefix
public class ExampleStackingBuffOne : ModBuff
{
	public override void SetStaticDefaults() {
		Main.buffNoSave[Type] = true;
	}

	public override bool RightClick(int buffIndex) {
		return false;
	}
}

public class ExampleStackingBuffTwo : ModBuff
{
	public override void SetStaticDefaults() {
		Main.buffNoSave[Type] = true;
	}

	public override bool RightClick(int buffIndex) {
		return false;
	}
}

public class ExampleStackingBuffThree : ModBuff
{
	public override void SetStaticDefaults() {
		Main.buffNoSave[Type] = true;
	}

	public override bool RightClick(int buffIndex) {
		return false;
	}
}

public class ExampleStackingBuffPlayer : ModPlayer
{
	// This dictates how many stacks of our buff the player has
	private int exampleStackingBuffStacks = 0;

	public override void PreUpdateBuffs() {
		// This is a switch expression, if you wish to learn more, see here: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/switch-expression
		int buffType = exampleStackingBuffStacks switch {
			1 => ModContent.BuffType<ExampleStackingBuffOne>(),
			2 => ModContent.BuffType<ExampleStackingBuffTwo>(),
			3 => ModContent.BuffType<ExampleStackingBuffThree>(),
			_ => -1
		};

		// Alternatively, we could store a list of buff types and use our field to index it
		// storing the buff types as a field:  private int[] buffTypes = [...];
		// indexing it in a method:  int buffType = buffTypes[exampleStackingBuffStacks];

		if (buffType == -1) {
			return;
		}

		// Note the logic of this entire method, we are letting our field dictate what tier of buff to apply,
		//    then continuously applying that buff. An alternate approach you can see in vanilla code is to
		//    apply the buff for a longer time, then apply the tier above when the buff is reapplied, or the tier below when it expires
		// For more information on how to look through vanilla code, see this wiki page: https://github.com/tModLoader/tModLoader/wiki/Advanced-Prerequisites#tmodloader-source-code
		Player.AddBuff(buffType, 2);
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
		// We need a way to accumulate stacks, here we are going to have a chance to grant one when we kill an enemy
		if (!target.active && Main.rand.NextBool(5)) {
			exampleStackingBuffStacks++;
			if (exampleStackingBuffStacks > 3) {
				exampleStackingBuffStacks = 3;
			}
		}
	}

	public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource) {
		if (exampleStackingBuffStacks <= 0) {
			return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
		}

		// Here we apply the actual effect of our buff. We want to grant the player additional lives, so we:

		// Reduce the number of stacks of our buff
		exampleStackingBuffStacks--;

		// Heal the player 10% of their maximum health
		Player.Heal(Player.statLifeMax2 / 10);

		// Play a dramatic sound so the player knows something happened. We don't want to confuse other players, so we only play it for the player who died
		if (Main.myPlayer == Player.whoAmI) {
			SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion with { Pitch = -0.5f }, Player.Center);
		}

		// And finally return false to prevent the player from dying
		return false;
	}

	// The below code is simply for syncing our buff stacks
	// We must sync the changes made to our buff stacks field for two main reasons:
	// 1. The field itself is set in OnHitNPC, which is only ran on the client who hit the NPC, making it non-deterministic
	// 2. The field is used for our effect in PreKill, which needs to be accessed by all clients and the server
	// If we don't sync the change, we could have desync issues. It's important to evaluate your own usecase to see if syncing is necessary
	// For more information about networking, see this guide: https://github.com/tModLoader/tModLoader/wiki/Basic-Netcode
	public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
		ModPacket packet = Mod.GetPacket();
		// Note how we are writing bytes even though the data is an int
		// It's important to shrink the data sent as much as we can, we alway know these values will fit in a byte so we should send them as that
		packet.Write((byte)ExampleMod.MessageType.ExampleStackingBuffPlayerSync);
		packet.Write((byte)Player.whoAmI);
		packet.Write((byte)exampleStackingBuffStacks);
		packet.Send(toWho, fromWho);
	}

	// Called in ExampleMod.Networking.cs
	public void ReceivePlayerSync(BinaryReader reader) {
		exampleStackingBuffStacks = reader.ReadByte();
	}

	public override void CopyClientState(ModPlayer targetCopy) {
		ExampleStackingBuffPlayer clone = (ExampleStackingBuffPlayer)targetCopy;
		clone.exampleStackingBuffStacks = exampleStackingBuffStacks;
	}

	public override void SendClientChanges(ModPlayer clientPlayer) {
		ExampleStackingBuffPlayer clone = (ExampleStackingBuffPlayer)clientPlayer;

		if (exampleStackingBuffStacks != clone.exampleStackingBuffStacks) {
			SyncPlayer(-1, Main.myPlayer, false);
		}
	}
}
