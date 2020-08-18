using Terraria.ModLoader;

namespace ExampleMod.Content.DamageClasses
{
	public class ExampleDamageClass : ModDamageClass
	{
		public override string ClassName => "example"; // Make weapons with this damage type have a tooltip of 'X example damage'.
	}
}
