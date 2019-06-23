using Terraria.ModLoader;
using Terraria.Utilities;

namespace CompileFail2
{
	class CompileFail2 : Mod
	{
		void Method() {
			// only exists on windows
			FileOperationAPIWrapper.MoveToRecycleBin("");
		}
	}
}
