using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

#nullable enable

namespace Terraria.ModLoader.Core;

// Unlike Terraria.ModLoader.Engine.ErrorReporting, this class mostly handles display of non-fatal errors in GUIs.
internal class ErrorDisplay
{
	public static string ErrorColorHex { get; } = Color.IndianRed.Hex3();
	public static string WarningColorHex { get; } = Color.Orange.Hex3();
	public static string HighlightColorHex { get; } = Color.LightSteelBlue.Hex3();

	public static string RemoveTextFormatting(string formattedMessage) {
		var sb = new StringBuilder();
		var textSnippets = ChatManager.ParseMessage(formattedMessage, Color.White);

		foreach (var snippet in textSnippets) {
			sb.Append(snippet.Text);
		}

		return sb.ToString();
	}

	public static void DisplayLoadError(string formattedMessage, Exception exception, bool fatal, bool continueIsRetry = false)
		=> DisplayLoadError(new StringBuilder(formattedMessage), exception, fatal, continueIsRetry);
	
	public static void DisplayLoadError(StringBuilder messageBuilder, Exception exception, bool fatal, bool continueIsRetry = false) {
		messageBuilder.AppendLine();
		messageBuilder.AppendLine();

		WriteException(messageBuilder, exception);

		string messageText = messageBuilder.ToString();

		if (Main.dedServ) {
			string unformattedMessage = RemoveTextFormatting(messageText);

			Console.ResetColor();
			Console.WriteLine(unformattedMessage);
			Console.ResetColor();

			if (fatal) {
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey();
				Environment.Exit(-1);
			}
			else {
				ModLoader.Reload();
			}
		}
		else {
			Interface.errorMessage.Show(
				messageText,
				gotoMenu: fatal ? -1 : Interface.reloadModsID,
				webHelpURL: exception.HelpLink,
				continueIsRetry: continueIsRetry,
				showSkip: !fatal
			);
		}
	}

	private static void WriteException(StringBuilder sb, Exception exception) {
		const string TypeColorHex = "4ec9b0";
		const string MethodColorHex = "dcdcaa";
		const string StringColorHex = "d68d5c";
		const string NumberColorHex = "a9cd9e";

		sb.AppendLine($"[c/{ErrorColorHex}:{exception.GetType().FullName}]: [c/{ErrorColorHex}:{exception.Message}]");

		if (exception.Data.Contains("hideStackTrace")) {
			return;
		}

		const string Tab = "    "; // Horrid.

		var stacktrace = new StackTrace(exception);
		var frames = stacktrace.GetFrames();

		if (frames.Length == 0) {
			sb.AppendLine($"{Tab}[c/{ErrorColorHex}:No stacktrace available.]");
			return;
		}

		foreach (var frame in frames) {
			var method = frame.GetMethod();

			sb.Append($"{Tab}at ");

			if (method != null) {
				if (method.DeclaringType != null) {
					if (method.DeclaringType.Namespace != null) {
						sb.Append($"{method.DeclaringType.Namespace}.");
					}

					sb.Append($"[c/{TypeColorHex}:{method.DeclaringType.Name}]");
				}

				string parameters = string.Join(", ", method.GetParameters().Select(p => $"[c/{TypeColorHex}:{p.ParameterType.Name}] {p.Name}"));

				sb.Append($".[c/{MethodColorHex}:{method.Name}]({parameters})");
			}
			else {
				sb.Append("unknown");
			}
			
			string? fileName = frame.GetFileName();
			int lineNumber = fileName != null ? frame.GetFileLineNumber() : 0;

			if (fileName != null) {
				sb.Append($"in [c/{StringColorHex}:{fileName}]:[c/{NumberColorHex}:{lineNumber}]");
			}

			sb.AppendLine();
		}
	}
}
