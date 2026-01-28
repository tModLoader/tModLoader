#if NETCORE
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SDL3;

namespace System.Windows.Forms;

public enum MessageBoxButtons
{
	OK,
	OKCancel,
	//AbortRetryIgnore,
	YesNoCancel,
	YesNo,
	RetryCancel
}

public enum MessageBoxIcon : uint
{
	None = 0,
	Error = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR,
	Hand = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR,
	Stop = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR,
	Exclamation = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_WARNING,
	Warning = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_WARNING,
	Asterisk = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_INFORMATION,
	Information = SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_INFORMATION
}

public enum DialogResult
{
	None,
	OK,
	Cancel,
	Abort,
	Retry,
	Ignore,
	Yes,
	No
}

public static class MessageBox
{
	private static unsafe readonly SDL.SDL_MessageBoxButtonData OKButton = new() {
		flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
		buttonID = (int)DialogResult.OK,
		text = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference("OK"u8))
	};

	private static unsafe readonly SDL.SDL_MessageBoxButtonData CancelButton = new() {
		flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT,
		buttonID = (int)DialogResult.Cancel,
		text = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference("Cancel"u8))
	};

	private static unsafe readonly SDL.SDL_MessageBoxButtonData YesButton = new() {
		flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
		buttonID = (int)DialogResult.Yes,
		text = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference("Yes"u8))
	};

	private static unsafe readonly SDL.SDL_MessageBoxButtonData NoButton = new() {
		buttonID = (int)DialogResult.No,
		text = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference("No"u8))
	};

	private static unsafe readonly SDL.SDL_MessageBoxButtonData RetryButton = new() {
		flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
		buttonID = (int)DialogResult.Retry,
		text = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference("Retry"u8))
	};

	public static unsafe DialogResult Show(string msg, string title, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
	{
		// stackalloc
		Span<SDL.SDL_MessageBoxButtonData> buttonsSpan = buttons switch {
			MessageBoxButtons.OK => [OKButton],
			MessageBoxButtons.OKCancel => [CancelButton, OKButton],
			MessageBoxButtons.YesNo => [NoButton, YesButton],
			MessageBoxButtons.YesNoCancel => [CancelButton, NoButton, YesButton],
			MessageBoxButtons.RetryCancel => [CancelButton, RetryButton],
			_ => throw new NotImplementedException(),
		};

		var msgBytes = Encoding.UTF8.GetBytes(msg + '\0');
		var titleBytes = Encoding.UTF8.GetBytes(title + '\0');
		fixed (byte* msgPtr = msgBytes, titlePtr = titleBytes) {
			var msgBox = new SDL.SDL_MessageBoxData {
				flags = (SDL.SDL_MessageBoxFlags)icon,
				message = msgPtr,
				title = titlePtr,
				buttons = (SDL.SDL_MessageBoxButtonData*)Unsafe.AsPointer(ref buttonsSpan.GetPinnableReference()),
				numbuttons = buttonsSpan.Length
			};
			SDL.SDL_ShowMessageBox(ref msgBox, out int buttonID);
			return (DialogResult)buttonID;
		}
	}
}
#endif