using Terraria.ModLoader;

namespace ExampleMod.Content.DamageClasses
{
	public class ExampleDamageClass : DamageClass
	{
		public override string ClassName => "example"; // Make weapons with this damage type have a tooltip of 'X example damage'.
	}
}
