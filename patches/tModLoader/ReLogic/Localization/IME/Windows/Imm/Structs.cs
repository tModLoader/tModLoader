using System.Runtime.InteropServices;

namespace ReLogic.Localization.IME.Windows;

[StructLayout(LayoutKind.Sequential)]
public struct CandidateList
{
	public uint dwSize;
	public uint dwStyle;
	public uint dwCount;
	public uint dwSelection;
	public uint dwPageStart;
	public uint dwPageSize;
	public uint dwOffset;
}

[StructLayout(LayoutKind.Sequential)]
public struct Point
{
	public int x;
	public int y;
}

[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
	public int left;
	public int top;
	public int right;
	public int bottom;
}

[StructLayout(LayoutKind.Sequential)]
public struct CandidateForm
{
	public int dwIndex;
	public int dwStyle;
	public Point ptCurrentPos;
	public Rect rcArea;
}