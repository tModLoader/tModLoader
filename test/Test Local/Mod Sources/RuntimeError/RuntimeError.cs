using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics;
using System;

namespace RuntimeError
{
	class RuntimeError : Mod
	{
	}
	
	class RuntimeErrorWorld : ModWorld
	{
		public override void PreUpdate() {
			try {
				throw new Exception("Failed to do a thing.");
			}
			catch (Exception) {}
		}
	}
}
