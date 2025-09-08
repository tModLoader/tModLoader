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

	public static void Deconstruct(this Point point, out int x, out int y)
	{
		x = point.X;
		y = point.Y;
	}

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
	/// Bit packs a BitArray into a Byte Array and then sends the byte array
	/// <include file = 'CommonDocs.xml' path='Common/BitArrayUsage' />
	/// </summary>
	public static void SendBitArray(BitArray arr, BinaryWriter writer)
	{
		byte[] result = new byte[(arr.Length - 1) / 8 + 1];
		arr.CopyTo(result, 0);
		writer.Write(result);
	}

	/// <summary>
	/// Receives the result of SendBitArray, and returns the corresponding BitArray
	/// <include file = 'CommonDocs.xml' path='Common/BitArrayUsage' />
	/// </summary>
	public static BitArray ReceiveBitArray(int BitArrLength, BinaryReader reader)
	{
		byte[] receive = new byte[(BitArrLength - 1) / 8 + 1];
		receive = reader.ReadBytes(receive.Length);
		return new BitArray(receive);
	}

	// TODO: Better options to SendBitArray/ReceiveBitArray that don't allocate a new bool[] or BitArray, most likely as extension methods in BinaryIO.cs

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

	private static void AddArgToDictionary(string text, ref string text2, ref Dictionary<string, string> dictionary)
	{
		if (text == null)
			return;

		// In case someone has a cli-ArgsConfig.txt for mod development and does host&play, we should TryAdd
		if (!dictionary.TryAdd(text.ToLower(), text2))
			Console.WriteLine($"Unexpected Issue with Launch Arguments: Duplicate Launch Arg \"{text}\"");

		text2 = "";
	}

	/// <summary>
	/// Creates a <see cref="Rectangle"/> from the provided corners. They do not need to be in a specific order.
	/// </summary>
	public static Rectangle CornerRectangle(Point pointA, Point pointB)
	{
		int left = Math.Min(pointA.X, pointB.X);
		int top = Math.Min(pointA.Y, pointB.Y);
		int width = Math.Abs(pointA.X - pointB.X);
		int height = Math.Abs(pointA.Y - pointB.Y);
		return new Rectangle(left, top, width, height);
	}

	/// <inheritdoc cref="CornerRectangle(Point, Point)"/>
	public static Rectangle CornerRectangle(Vector2 pointA, Vector2 pointB) => CornerRectangle(pointA.ToPoint(), pointB.ToPoint());

	/// <summary>
	/// Creates a <see cref="Rectangle"/> containing all of the provided points.
	/// </summary>
	public static Rectangle BoundingRectangle(Point[] points)
	{
		if (points.Length == 0)
			return new Rectangle();
		var rectangle = new Rectangle(points[0].X, points[0].Y, 0, 0);
		for (int i = 1; i < points.Length; i++)
			rectangle = rectangle.Including(points[i]);
		return rectangle;
	}

	/// <inheritdoc cref="BoundingRectangle(Point[])"/>
	public static Rectangle BoundingRectangle(Vector2[] vectors)
	{
		if (vectors.Length == 0)
			return new Rectangle();
		var rectangle = new Rectangle((int)vectors[0].X, (int)vectors[0].Y, 0, 0);
		for (int i = 1; i < vectors.Length; i++)
			rectangle = rectangle.Including(vectors[i]);
		return rectangle;
	}

	/// <summary>
	/// Expands the provided <paramref name="rect"/> to include <paramref name="point"/> and returns the newly expanded <see cref="Rectangle"/>.
	/// </summary>
	public static Rectangle Including(this Rectangle rect, Point point)
	{
		int l = Math.Min(rect.Left, point.X);
		int r = Math.Max(rect.Right, point.X);
		int t = Math.Min(rect.Top, point.Y);
		int b = Math.Max(rect.Bottom, point.Y);
		return new Rectangle(l, t, r - l, b - t);
	}

	/// <inheritdoc cref="Including(Rectangle, Point)"/>
	public static Rectangle Including(this Rectangle rect, Vector2 point) => rect.Including(point.ToPoint());
}
