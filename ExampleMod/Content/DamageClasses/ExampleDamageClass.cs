using Terraria.ModLoader;

namespace ExampleMod.Content.DamageClasses
{
	public class ExampleDamageClass : DamageClass
	{
		public override void SetupContent() {
			// Make weapons with this damage type have a tooltip of 'X example damage'.
			ClassName.SetDefault("example damage");
		}
	}
}
