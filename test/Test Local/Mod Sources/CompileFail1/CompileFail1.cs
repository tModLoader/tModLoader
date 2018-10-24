using Terraria.ModLoader;
using System;

namespace CompileFail1
{
	class CompileFail1 : Mod
	{
		[Obsolete]
		void ObsoleteMethod() {}
		
		void Method() {
			ObsoleteMethod();
			return 2;
		}
		
		void Method2() {
			error();
		}
	}
}
