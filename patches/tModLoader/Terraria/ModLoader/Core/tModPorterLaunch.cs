using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.MSBuild;
using MonoMod.RuntimeDetour;

namespace Terraria.ModLoader.Core;

internal class tModPorterLaunch
{
	internal static void Launch(string[] args)
	{
		// Because we package all our libraries in subfolder according to their nuget package, the default BuildHost-netcore folder resolution fails
		// We need to point it at the right folder for us
		//
		// https://github.com/dotnet/roslyn/blob/main/src/Workspaces/MSBuild/Core/MSBuild/BuildHostProcessManager.cs#L35

		var workspaceDirProperty = typeof(MSBuildWorkspace).Assembly.GetType("Microsoft.CodeAnalysis.MSBuild.BuildHostProcessManager").GetProperty("MSBuildWorkspaceDirectory", BindingFlags.NonPublic | BindingFlags.Static);
		using var _ = new Hook(workspaceDirProperty.GetGetMethod(true), new Func<Func<string>, string>((orig) =>
			Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
		));

		tModPorter.Program.Main(args).GetAwaiter().GetResult();
	}
}
