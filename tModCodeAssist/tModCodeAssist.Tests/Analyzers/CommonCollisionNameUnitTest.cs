using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = tModCodeAssist.Tests.Verifier.Analyzer<tModCodeAssist.Analyzers.CommonCollisionNameAnalyzer>;

namespace tModCodeAssist.Tests.Analyzers;

[TestClass]
public class CommonCollisionNameUnitTest
{
	[TestMethod]
	public async Task Test()
	{
		await VerifyCS.Run(
			"""
			public static class Program {
				class [|NPC|] { }

				public static void Main() {
				}
			}

			class [|NPC|] { }

			namespace [|Mod|] { }
			namespace [|Mod|] { }
			namespace EtherealMod.[|Item|] { }
			namespace EtherealMod.Items { }
			namespace EtherealMod.[|Item|].Cosmetics { }
			"""
			);
	}
}
