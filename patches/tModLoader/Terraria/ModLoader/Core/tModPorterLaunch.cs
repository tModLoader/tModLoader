using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.MSBuild;
using MonoMod.RuntimeDetour;

namespace Terraria.ModLoader.Core;

internal class tModPorterLaunch
{
	internal static void Launch(string[] args)
	{
		// The new MSBuild Workspaces uses a separate process to read the csproj file and execute tasks etc.
		// We need to point it to the location of the packaged BuildHost-netcore folder in tML
		// 
		// https://github.com/dotnet/roslyn/blob/main/src/Workspaces/Core/MSBuild/MSBuild/BuildHostProcessManager.cs#L160
		var h = new Hook(
			typeof(MSBuildWorkspace).Assembly.GetType("Microsoft.CodeAnalysis.MSBuild.BuildHostProcessManager").GetMethod("CreateDotNetCoreBuildHostStartInfo", BindingFlags.NonPublic | BindingFlags.Instance),
			new Func<Func<object, ProcessStartInfo>, object, ProcessStartInfo>((orig, self) => {
				var psi = orig(self);
				psi.FileName = Environment.ProcessPath;

				var buildHostPath = psi.ArgumentList.First(s => s.EndsWith("BuildHost.dll"));
				int argIndex = psi.ArgumentList.IndexOf(buildHostPath);
				//var buildHostPath = Path.Combine(Path.GetDirectoryName(typeof(MSBuildWorkspace).Assembly.Location), "Microsoft.CodeAnalysis.Workspaces.MSBuild.BuildHost.dll");
				buildHostPath = Path.Combine(Path.GetDirectoryName(typeof(MSBuildWorkspace).Assembly.Location), "..", "..", "BuildHost-netcore", "Microsoft.CodeAnalysis.Workspaces.MSBuild.BuildHost.dll");
				psi.ArgumentList.RemoveAt(argIndex);
				psi.ArgumentList.Insert(argIndex, buildHostPath);
				return psi;
			}));


		tModPorter.Program.Main(args).GetAwaiter().GetResult();
	}
}
