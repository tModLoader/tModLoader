using System;
using System.Runtime.CompilerServices;

namespace Terraria.ModLoader;

public static class EntityArrays<T> where T : Entity
{
	public static T[] Array { get; }
	public static int Max { get; }
	
	static EntityArrays() {
		if (typeof(T) == typeof(NPC)) {
			Array = (T[])(object)Main.npc;
			Max = Main.maxNPCs;
		}
		else if (typeof(T) == typeof(Projectile)) {
			Array = (T[])(object)Main.projectile;
			Max = Main.maxProjectiles;
		}
		else if (typeof(T) == typeof(Player)) {
			Array = (T[])(object)Main.player;
			Max = Main.maxPlayers;
		}
		else {
			Throw();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static void Throw()
		{
			throw new ArgumentException($"Not supported type {typeof(T)}", nameof(T));
		}
	}
}
