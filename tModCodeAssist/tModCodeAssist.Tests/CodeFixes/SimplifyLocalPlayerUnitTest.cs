using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = tModCodeAssist.Tests.Verifier.Analyzer<tModCodeAssist.Analyzers.SimplifyLocalPlayerAnalyzer>.CodeFixer<tModCodeAssist.CodeFixes.SimplifyLocalPlayerCodeFixProvider>;

namespace tModCodeAssist.Tests.CodeFixes;

[TestClass]
public sealed class SimplifyLocalPlayerUnitTest
{
	[TestMethod]
	public async Task Test_MyPlayer_WithStaticMain()
	{
		await VerifyCS.Run(
			"""
			using Terraria;
			using static Terraria.Main;

			{
				_ = [|Main.player[Main.myPlayer]|];
				_ = [|player[myPlayer]|];
				_ = Main.LocalPlayer;
				_ = Main.player[0];
			}

			{
				var player = new Player[0];
				int myPlayer = 0;
				_ = player[myPlayer];
			}
			""",
			"""
			using Terraria;
			using static Terraria.Main;

			{
				_ = LocalPlayer;
				_ = LocalPlayer;
				_ = Main.LocalPlayer;
				_ = Main.player[0];
			}

			{
				var player = new Player[0];
				int myPlayer = 0;
				_ = player[myPlayer];
			}
			""");
	}

	[TestMethod]
	public async Task Test_MyPlayer_WithoutStaticMain()
	{
		await VerifyCS.Run(
			"""
			using Terraria;

			var player = new Player[0];
			int myPlayer = 0;
			_ = [|Main.player[Main.myPlayer]|];
			_ = player[myPlayer];
			_ = Main.LocalPlayer;
			_ = Main.player[0];
			""",
			"""
			using Terraria;

			var player = new Player[0];
			int myPlayer = 0;
			_ = Main.LocalPlayer;
			_ = player[myPlayer];
			_ = Main.LocalPlayer;
			_ = Main.player[0];
			""");
	}
}
