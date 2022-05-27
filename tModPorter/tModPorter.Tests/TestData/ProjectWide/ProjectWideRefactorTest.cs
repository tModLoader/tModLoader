using System;
using Terraria.ModLoader;

public class ProjectWideRefactorTest : Mod
{
	public void UseRenamedMethods(ModTileTest tileTest, Action<Func<int, int, bool>> accept) {
		tileTest.NewRightClick(0, 0);
		tileTest?.NewRightClick(0, 0);
		accept(tileTest.NewRightClick);
		Console.Write(tileTest.NewRightClick);
		((Func<int, int, bool>)tileTest.NewRightClick)(0, 0);
		((Func<int, int, bool>)tileTest.NewRightClick)?.Invoke(0, 0);
	}
}