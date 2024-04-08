using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.Utilities;

namespace Terraria;

partial class Utils
{
	//Conversions

	/// <summary> <include file = 'CommonDocs.xml' path='Common/ToWorldCoordinates' /> </summary>
	public static Vector2 ToWorldCoordinates(this Point p, Vector2 autoAddXY)
		=> ToWorldCoordinates(p, autoAddXY.X, autoAddXY.Y);

	/// <summary> <include file = 'CommonDocs.xml' path='Common/ToWorldCoordinates' /> </summary>
	public static Vector2 ToWorldCoordinates(this Point16 p, Vector2 autoAddXY)
		=> p.ToVector2().ToWorldCoordinates(autoAddXY);

	/// <summary> <include file = 'CommonDocs.xml' path='Common/ToWorldCoordinates' /> </summary>
	public static Vector2 ToWorldCoordinates(this Vector2 v, float autoAddX = 8f, float autoAddY = 8f)
		=> v.ToWorldCoordinates(new Vector2(autoAddX, autoAddY));

	/// <summary> <include file = 'CommonDocs.xml' path='Common/ToWorldCoordinates' /> </summary>
	public static Vector2 ToWorldCoordinates(this Vector2 v, Vector2 autoAddXY)
		=> v * 16f + autoAddXY;

	public static Point ToPoint(this Point16 p)
		=> new Point(p.X, p.Y);

	/// <summary> Converts this Vector2 to a Point16, resulting in X and Y values rounded towards 0. If the intention is to convert to Tile coordinates from World coordinates, use <see cref="ToTileCoordinates16(Vector2)"/> instead. </summary>
	public static Point16 ToPoint16(this Vector2 v)
		=> new Point16((short)v.X, (short)v.Y);

	public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
	{
		// Unix timestamp is seconds past epoch
		return DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp).UtcDateTime;
	}

	//Struct extensions

	public static T NextEnum<T>(this T src) where T : struct
	{
		if(!typeof(T).IsEnum)
			throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");

		T[] Arr = (T[])Enum.GetValues(src.GetType());
		int j = Array.IndexOf(Arr, src) + 1;

		return Arr.Length == j ? Arr[0] : Arr[j];
	}

	public static T PreviousEnum<T>(this T src) where T : struct
	{
		if(!typeof(T).IsEnum)
			throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");

		T[] Arr = (T[])Enum.GetValues(src.GetType());
		int j = Array.IndexOf(Arr, src) - 1;

		return j < 0 ? Arr[Arr.Length - 1] : Arr[j];
	}

	public static Version MajorMinor(this Version v) => new(v.Major, v.Minor);

	public static Version MajorMinorBuild(this Version v) => v.Build < 0 ? v.MajorMinor() : new(v.Major, v.Minor, v.Build);

	//Random extensions

	/// <summary>
	/// Returns a random element from the provided array.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="r"></param>
	/// <param name="array"></param>
	/// <returns></returns>
	public static T Next<T>(this UnifiedRandom r, T[] array)
		=> array[r.Next(array.Length)];

	/// <summary>
	/// Returns a random element from the provided list.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="r"></param>
	/// <param name="list"></param>
	/// <returns></returns>
	public static T Next<T>(this UnifiedRandom r, IList<T> list)
		=> list[r.Next(list.Count)];

	/// <summary>
	/// Generates a random value between 0f (inclusive) and <paramref name="maxValue"/> (exclusive). <br/>It will not return <paramref name="maxValue"/>.
	/// </summary>
	/// <param name="r"></param>
	/// <param name="maxValue"></param>
	/// <returns></returns>
	public static float NextFloat(this UnifiedRandom r, float maxValue)
		=> (float)r.NextDouble() * maxValue;

	/// <summary>
	/// Generates a random value between <paramref name="minValue"/> (inclusive) and <paramref name="maxValue"/> (exclusive). <br/>It will not return <paramref name="maxValue"/>.
	/// </summary>
	/// <param name="r"></param>
	/// <param name="minValue"></param>
	/// <param name="maxValue"></param>
	/// <returns></returns>
	public static float NextFloat(this UnifiedRandom r, float minValue, float maxValue)
		=> (float)r.NextDouble() * (maxValue - minValue) + minValue;

	/// <summary>
	/// Returns true or false randomly with equal chance.
	/// </summary>
	/// <param name="r"></param>
	/// <returns></returns>
	public static bool NextBool(this UnifiedRandom r)
		=> r.NextDouble() < .5;

	/// <summary> Returns true 1 out of X times. </summary>
	public static bool NextBool(this UnifiedRandom r, int consequent)
	{
		if(consequent < 1)
			throw new ArgumentOutOfRangeException(nameof(consequent), "consequent must be greater than or equal to 1.");

		return r.Next(consequent) == 0;
	}

	/// <summary> Returns true X out of Y times. </summary>
	public static bool NextBool(this UnifiedRandom r, int antecedent, int consequent)
	{
		if(antecedent > consequent)
			throw new ArgumentOutOfRangeException(nameof(antecedent), "antecedent must be less than or equal to consequent.");

		return r.Next(consequent) < antecedent;
	}

	public static int Repeat(int value, int length) => value >= 0 ? value % length : (value % length) + length;

	/// <summary>
	/// Bit packs a BitArray in to a Byte Array and then sends the byte array
	/// </summary>
	public static void SendBitArray(BitArray arr, BinaryWriter writer)
	{
		byte[] result = new byte[(arr.Length - 1) / 8 + 1];
		arr.CopyTo(result, 0);
		writer.Write(result);
	}

	/// <summary>
	/// Receives the result of SendBitArray, and returns the corresponding BitArray
	/// </summary>
	public static BitArray ReceiveBitArray(int BitArrLength, BinaryReader reader)
	{
		byte[] receive = new byte[(BitArrLength - 1) / 8 + 1];
		receive = reader.ReadBytes(receive.Length);
		return new BitArray(receive);
	}

	// Common Blocks

	public static void OpenToURL(string url)
	{
		// Do not attempt to shorten this, no universal works-everywhere call exists.
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			// Windows
			Process.Start("explorer.exe", $@"""{url}""");
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
			// MacOS
			Process.Start("open", $@"""{url}""");
		}
		else {
			// Linux & FreeBSD
			Process.Start("xdg-open", $@"""{url}""");
		}
	}

	public static void ShowFancyErrorMessage(string message, int returnToMenu, UIState returnToState = null)
	{
		if (!Main.dedServ) {
			Logging.tML.Error(message);
			Interface.errorMessage.Show(message, returnToMenu, returnToState);
		}
		else
			LogAndConsoleErrorMessage(message);
	}

	public static void LogAndChatAndConsoleInfoMessage(string message)
	{
		Logging.tML.Info(message);

		if (Main.dedServ)
			Console.WriteLine(message);
		else
			Main.NewText(message);
	}

	public static void LogAndConsoleInfoMessage(string message)
	{
		Logging.tML.Info(message);

		if (Main.dedServ) {
			Console.WriteLine(message);
		}
	}

	public static void LogAndConsoleInfoMessageFormat(string format, params object[] args)
	{
		LogAndConsoleInfoMessage(string.Format(format, args));
	}

	public static void LogAndConsoleErrorMessage(string message)
	{
		Logging.tML.Error(message);

		if (Main.dedServ) {
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine("ERROR: " + message);
			Console.ResetColor();
		}
	}

	internal static string CleanChatTags(string text)
	{
		return string.Join("", ChatManager.ParseMessage(text, Color.White)
				.Where(x => x.GetType() == typeof(TextSnippet))
				.Select(x => x.Text));
	}

	internal static void HandleSaveErrorMessageLogging(NetworkText message, bool broadcast)
	{
		Utils.LogAndConsoleInfoMessage(message.ToString());
		if (Main.gameMenu && Main.menuMode == 10) {
			// Save and Quit. Due to multithreading we need to queue up the message window instead of Interface.errorMessage.Show immediately.
			Interface.pendingErrorMessages.Push(message.ToString());
		}
		else if (!Main.gameMenu) {
			// In-game autosave
			if (broadcast)
				ChatHelper.BroadcastChatMessage(message, Color.OrangeRed); // Handles SP and Server cases.
			else
				Main.NewText(message, Color.OrangeRed);
		}
	}

	internal static NetworkText CreateSaveErrorMessage(string localizationKey, Dictionary<string, string> errors, bool doubleNewline = false)
	{
		string separator = doubleNewline ? "\n\n" : "\n";
		return NetworkText.FromKey(localizationKey, separator + string.Join(separator, errors.Select(x => $"{x.Key}:\n{x.Value}")));
	}
}
