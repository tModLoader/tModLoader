using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.MSBuild;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Terraria.ModLoader.Core;

internal class tModPorterLaunch
{
	internal static void Launch(string[] args)
	{
		// The new MSBuild Workspaces uses a separate process to read the csproj file and execute tasks etc.
		// We need to point it to the location of the packaged BuildHost-netcore folder in tML
		//
		// https://github.com/dotnet/roslyn/blob/main/src/Workspaces/MSBuild/Core/MSBuild/BuildHostProcessManager.cs#L165

		using var _1 = new Hook(
			typeof(MSBuildWorkspace).Assembly.GetType("Microsoft.CodeAnalysis.MSBuild.BuildHostProcessManager").GetMethod("CreateDotNetCoreBuildHostStartInfo", BindingFlags.NonPublic | BindingFlags.Instance),
			new Func<Func<object, ProcessStartInfo>, object, ProcessStartInfo>((orig, self) => {
				var psi = orig(self);
				psi.FileName = Environment.ProcessPath;
				return psi;
			}));

		using var _2 = new ILHook(
			typeof(MSBuildWorkspace).Assembly.GetType("Microsoft.CodeAnalysis.MSBuild.BuildHostProcessManager").GetMethod("GetNetCoreBuildHostPath", BindingFlags.NonPublic | BindingFlags.Static),
			il => new ILCursor(il)
				.GotoNext(i => i.MatchLdstr("BuildHost-netcore"))
				.Remove()
				.EmitLdstr("../../BuildHost-netcore")
			);

		tModPorter.Program.Main(args).GetAwaiter().GetResult();
	}
}
