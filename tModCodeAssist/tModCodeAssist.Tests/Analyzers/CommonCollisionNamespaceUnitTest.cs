using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = tModCodeAssist.Tests.Verifier.Analyzer<tModCodeAssist.Analyzers.CommonCollisionNamespaceAnalyzer>;

namespace tModCodeAssist.Tests.Analyzers;

[TestClass]
public class CommonCollisionNamespaceUnitTest
{
	[TestMethod]
	public async Task Test()
	{
		await VerifyCS.Run(
			"""
			public static class Program {
				public static void Main() {
				}
			}

			namespace [|Mod|] { }
			namespace [|Mod|] { }
			namespace EtherealMod.[|Item|] { }
			namespace EtherealMod.Items { }
			namespace EtherealMod.[|Item|].Cosmetics { }
			"""
			);
	}
}
