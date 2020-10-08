namespace Terraria.ModLoader
{
	public abstract class ItemCreationContext
	{
	}

	public class RecipeCreationContext : ItemCreationContext
	{
		public Recipe recipe;
	}

	public class InitializationContext : ItemCreationContext
	{
		
	}
}