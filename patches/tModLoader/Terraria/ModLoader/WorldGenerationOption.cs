using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader;

internal class WorldGenerationOption : AWorldGenerationOption
{
	protected override string KeyName { get; }
	public override string ServerConfigName { get; }

	public WorldGenerationOption(IEnumerable<string> specialSeedNames, IEnumerable<int> specialSeedNumbers, LocalizedText description, LocalizedText title, Asset<Texture2D> texture)
	{
		SpecialSeedNames = specialSeedNames.ToArray();
		SpecialSeedValues = specialSeedNumbers.ToArray();
		Description = description;
		Title = title;
		Texture = texture;
	}
}