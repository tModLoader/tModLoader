using ExampleMod.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod
{
	public class TestDamageClass : DamageClass { }
	public class TestInfoDisplay : InfoDisplay
	{
		public override string Texture => ModContent.GetInstance<ExampleInfoDisplay>().Texture;
		public override string DisplayValue() {
			return "Hi.";
		}
	}
	public class TestModResourceDisplaySet : ModResourceDisplaySet { }

	public class LocalizedSystem : ModSystem, ILocalizedModType
	{
		public string LocalizationCategory => "LocalizedSystemCategory";

		public LocalizedText SomethingInLocalizedSystem => this.GetLocalization(nameof(SomethingInLocalizedSystem), PrettyPrintName);
		public LocalizedText SomethingElseInLocalizedSystem => this.GetLocalization(nameof(SomethingElseInLocalizedSystem), () => "");
	}

	public class NonLocalizedSystem : ModSystem
	{
		LocalizedText Test1 => Language.GetOrRegister(Mod.GetLocalizationKey("DownedBossSystemTest1"), null);
		LocalizedText Test2 => Language.GetText(Mod.GetLocalizationKey("DownedBossSystemTest2"));
		LocalizedText Test3 => Language.GetOrRegister(Mod.GetLocalizationKey("DownedBossSystemTest3"), PrettyPrintName);
		LocalizedText Test4 => Language.GetOrRegister(Mod.GetLocalizationKey("DownedBossSystemTest4"), () => "");
		LocalizedText Test5 => Language.GetOrRegister(Mod.GetLocalizationKey("Cat.DownedBossSystemTest5"), () => "");
	}
}
