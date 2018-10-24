using Terraria.ModLoader;
using System;

namespace PostSetupContentFail
{
	class PostSetupContentFail : Mod
	{
		public override void PostSetupContent() {
			throw new Exception("PostSetupContent Failed");
		}
	}
}
