#if NETCORE
using SDL2;

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
	private static SDL.SDL_MessageBoxButtonData OKButton = new SDL.SDL_MessageBoxButtonData {
		flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
		buttonid = (int)DialogResult.OK,
		text = "OK"
	};

	private static SDL.SDL_MessageBoxButtonData CancelButton = new SDL.SDL_MessageBoxButtonData {
		flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT,
		buttonid = (int)DialogResult.Cancel,
		text = "Cancel"
	};

	private static SDL.SDL_MessageBoxButtonData YesButton = new SDL.SDL_MessageBoxButtonData {
		flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
		buttonid = (int)DialogResult.Yes,
		text = "Yes"
	};

	private static SDL.SDL_MessageBoxButtonData NoButton = new SDL.SDL_MessageBoxButtonData {
		buttonid = (int)DialogResult.No,
		text = "No"
	};

	private static SDL.SDL_MessageBoxButtonData RetryButton = new SDL.SDL_MessageBoxButtonData {
		flags = SDL.SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
		buttonid = (int)DialogResult.Retry,
		text = "Retry"
	};

	public static DialogResult Show(string msg, string title, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
	{
		var msgBox = new SDL.SDL_MessageBoxData {
			flags = (SDL.SDL_MessageBoxFlags)icon,
			message = msg,
			title = title,
			buttons = buttons switch {
				MessageBoxButtons.OK => new[] { OKButton },
				MessageBoxButtons.OKCancel => new[] { CancelButton, OKButton },
				MessageBoxButtons.YesNo => new[] { NoButton, YesButton },
				MessageBoxButtons.YesNoCancel => new[] { CancelButton, NoButton, YesButton },
				MessageBoxButtons.RetryCancel => new[] { CancelButton, RetryButton },
				_ => throw new NotImplementedException(),
			}
		};
		msgBox.numbuttons = msgBox.buttons.Length;

		SDL.SDL_ShowMessageBox(ref msgBox, out int buttonid);
		return (DialogResult)buttonid;
	}
}
#endif
