using System;
using System.Runtime.InteropServices;

internal class Program
{
	[DllImport("kernel32.dll")]
	static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
	[DllImport("kernel32.dll")]
	static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);
	[DllImport("kernel32.dll")]
	static extern IntPtr GetStdHandle(int handle);

	const int STD_INPUT_HANDLE = -10;
	const int ENABLE_EXTENDED_FLAGS = 0x80;
	const int QUICK_EDIT_MODE = 0x40;

	static void Main(string[] args)
	{
		IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);
		GetConsoleMode(handle, out int mode);
		if ((mode & QUICK_EDIT_MODE) == 0 || (mode & ENABLE_EXTENDED_FLAGS) == 0) {
			Console.WriteLine("Quick Edit Not Enabled");
		}
		else {
			SetConsoleMode(handle, mode &= ~QUICK_EDIT_MODE);
			Console.WriteLine("Quick Edit Disabled");
		}
	}
}
