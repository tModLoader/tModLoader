#if !WINDOWS

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Windows.Forms;

internal class Control
{
	internal static Form FromHandle(IntPtr handle)
	{
		throw new NotImplementedException();
	}
}
#endif