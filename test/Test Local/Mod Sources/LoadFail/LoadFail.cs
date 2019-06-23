using Terraria.ModLoader;
using System;

namespace LoadFail
{
	class LoadFail : Mod
	{
		public override void Load() {
			throw new Exception("Loading Failed");
		}
	}
}
