using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VerifyCS = tModCodeAssist.Tests.Verifier.Analyzer<tModCodeAssist.ChangeMagicNumberToID.ChangeMagicNumberToIDAnalyzer>.CodeFixer<tModCodeAssist.ChangeMagicNumberToID.ChangeMagicNumberToIDCodeFixProvider>;

namespace tModCodeAssist.Tests.CodeFixers;

[TestClass]
public sealed class ChangeMagicNumberToIDUnitTest
{
	private readonly List<(string fileName, SourceText content)> additionalFiles = [];

	[TestInitialize]
	public void Initialize()
	{
		// TODO: Find a better way
		const string path = "../../../../tModCodeAssist/ChangeMagicNumberToID/Data";

		foreach (string file in Directory.GetFiles(path, "ChangeMagicNumberToID*.json", SearchOption.TopDirectoryOnly)) {
			string content = File.ReadAllText(file);

			additionalFiles.Add((Path.GetFileName(file), SourceText.From(content, Encoding.UTF8)));
		}
	}

	[TestCleanup]
	public void Cleanup()
	{
		additionalFiles.Clear();
	}

	[TestMethod]
	public async Task Test_Parameter()
	{
		await VerifyCS.Run(
			"""
			using Terraria;

			_ = new Item([|1|]);
			""",
			"""
			using Terraria;
			using Terraria.ID;
			
			_ = new Item(ItemID.IronPickaxe);
			""")
			.WithAdditionalFiles(additionalFiles);
	}

	[TestMethod]
	public async Task Test_Assignment()
	{
		await VerifyCS.Run(
			"""
			using Terraria;

			new Item().type = [|1|];
			""",
			"""
			using Terraria;
			using Terraria.ID;
			
			new Item().type = ItemID.IronPickaxe;
			""")
			.WithAdditionalFiles(additionalFiles);
	}

	[TestMethod]
	public async Task Test_Binary()
	{
		await VerifyCS.Run(
			"""
			using Terraria;

			_ = new Item().type == [|1|];
			""",
			"""
			using Terraria;
			using Terraria.ID;
			
			_ = new Item().type == ItemID.IronPickaxe;
			""")
			.WithAdditionalFiles(additionalFiles);
	}

	[TestMethod]
	public async Task Test_SwitchCase()
	{
		await VerifyCS.Run(
			"""
			using Terraria;

			switch (new Item().type) {
				case [|1|]:
					break;
			}
			""",
			"""
			using Terraria;
			using Terraria.ID;
			
			switch (new Item().type) {
				case ItemID.IronPickaxe:
					break;
			}
			""")
			.WithAdditionalFiles(additionalFiles);
	}
}
