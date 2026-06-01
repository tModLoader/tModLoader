namespace Terraria.DataStructures;

/// <summary>
/// Contains all the parameters defining how a particular player will be rendered in the current lighting, copied from the <see cref="drawPlayer"/>.
/// <br/><br/> First, the color of each body part is calculated and all of the fields are populated. Next, this data is used by each <see cref="ModLoader.PlayerDrawLayer"/> to populate <see cref="DrawDataCache"/> with <see cref="DrawData"/> corresponding to each individual layer making up the visuals of the player. Finally, every <see cref="DrawDataCache"/> is actually drawn.
/// <br/><br/> In terms of modded logic, <see cref="ModLoader.ModPlayer.DrawEffects"/> runs before body part color values are calculated, <see cref="ModLoader.ModPlayer.ModifyDrawInfo"/> runs right before each <see cref="ModLoader.PlayerDrawLayer"/> runs, then each <see cref="ModLoader.PlayerDrawLayer"/> runs, and finally <see cref="ModLoader.ModPlayer.TransformDrawData"/> runs.
/// </summary>
public partial struct PlayerDrawSet
{
	public bool headOnlyRender;
	public bool isBottomOverriden;
}
