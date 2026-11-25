using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.UI;

namespace Terraria.GameContent.UI.States;

public partial class UIWorldLoad : UIState
{
	private void FinishGenerationTextChange()
	{
		string modText = _progressMessage.Text;
		Color modColor = (WorldGen.tenthAnniversaryWorldGen && !Main.zenithWorld) ? Color.HotPink : Color.White;
		SeedLoader.ModifyWorldProgressText(ref modText, ref modColor);
		_progressMessage.Text = modText;
		_progressMessage.CustomColor = modColor;
	}
}
