using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terraria.ID;

namespace Terraria.ModLoader;

[TestClass]
public class PrefixRollTests
{
	[ClassInitialize]
	public static void ClassInitialize(TestContext context)
	{
		Program.SavePath = ".";
	}

	// Regression test for #5301: 1.4.5 defaults Item.maxStack to CommonMaxStack (9999), so the
	// removed "#StackablePrefixWeapons" guard (maxStack > 1) blocked random prefix rolls for every
	// crafted or dropped weapon. A vanilla weapon must remain prefixable.
	[TestMethod]
	public void VanillaWeaponCanRollPrefixes()
	{
		Item item = new() {
			type = ItemID.IronBroadsword,
			maxStack = Item.CommonMaxStack
		};

		Assert.IsTrue(item.CanHavePrefixes());
		Assert.IsTrue(item.Prefix(-3));
	}
}
