using Terraria;

public class IsJourneyModeTest
{
	void Method()
	{
		bool isJourney = Main.GameModeInfo.IsJourneyMode;
		bool isJourneyQualified = Terraria.Main.GameModeInfo.IsJourneyMode;
		bool isJourneyGlobalQualified = global::Terraria.Main.GameModeInfo.IsJourneyMode;
	}
}
