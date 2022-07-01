using System.Collections.Generic;
using tModPorter.Rewriters;

namespace tModPorter;

public static partial class Config
{
	public static List<BaseRewriter> CreateRewriters() => new() {
		new RenameRewriter(),
		new MemberUseRewriter(),
		new InvokeRewriter(),
		new HookRewriter(),
		new RecipeRewriter(),
	};

	static Config() {
		AddModLoaderRefactors();
		AddTerrariaRefactors();
		AddTextureRenames();
	}
}