using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

[TestClass]
public class ResetStaticMembersTest
{
	private static class Nested
	{
		public const float DefaultF32 = 48573.2f;
		public static float f32 = DefaultF32;
	}

	private const int DefaultI32 = 1000;
	private const string DefaultText = "Default";

	private static string text = DefaultText;
	private static int i32 = DefaultI32;
	private static readonly double randomized;

	static ResetStaticMembersTest()
	{
		randomized = new Random().NextDouble() * 999d + 1d;
	}

	[TestMethod]
	public void TestResetStaticMembers()
	{
		Assert.AreEqual(i32, DefaultI32);
		Assert.AreEqual(text, DefaultText);
		Assert.AreNotEqual(randomized, 0d);

		double previousRandomized = randomized;
		i32 = 5723948;
		text = "Modified";
		Nested.f32 = 0.5389238f;

		LoaderUtils.ResetStaticMembers(typeof(ResetStaticMembersTest), recursive: true);

		Assert.AreEqual(i32, DefaultI32);
		Assert.AreEqual(text, DefaultText);
		Assert.AreNotEqual(randomized, previousRandomized);
	}
}
