using System.Collections.Generic;
using tModPorter.Rewriters;

namespace tModPorter;

public static partial class Config
{
	public static List<BaseRewriter> CreateRewriters() => new() {
		new HookRewriter(), // Above RenameRewriter since RenameType would cause ChangeHookSignature->RenameParameter to skip renaming the method declaration parameters. (EditSpawnPool)
		new RenameRewriter(),
		new MemberTypeRewriter(),
		new MemberUseRewriter(),
		new InvokeRewriter(),
		new RecipeRewriter(),
		new HookGenRewriter(),
	};

	static Config() {
		AddModLoaderRefactors();
		AddTerrariaRefactors();
		AddTextureRenames();
	}
}