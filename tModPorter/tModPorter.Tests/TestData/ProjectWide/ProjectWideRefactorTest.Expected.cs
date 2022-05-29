using System;
using Terraria.ModLoader;

public class ProjectWideRefactorTest : Mod
{
	public void UseRenamedMethods(ModTileTest tileTest, Action<Func<int, int, bool>> accept) {
		tileTest.RightClick(0, 0);
		tileTest?.RightClick(0, 0);
		accept(tileTest.RightClick);
		Console.Write(tileTest.RightClick);
		((Func<int, int, bool>)tileTest.RightClick)(0, 0);
		((Func<int, int, bool>)tileTest.RightClick)?.Invoke(0, 0);
	}
}