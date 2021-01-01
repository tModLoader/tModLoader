using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	//Showcases how to assign something to a vanilla NPC like you would do for a modded NPC using the SetStaticDefaults hook
	//In this case, setting a big progress bar
	public class BlueJellyfishBigProgressBarGlobalNPC : GlobalNPC
	{
		//SetupContent hook runs once per class, right after all content has loaded in and is functional
		public override void SetupContent() {
			//Custom "boss health bar", see the Content/BigProgressBars folder
			Main.BigBossProgressBar.AddBar(NPCID.BlueJellyfish, new Content.BigProgressBars.BlueJellyfishBigProgressBar());
		}
	}
}
