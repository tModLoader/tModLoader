#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Windows.Forms;

// Keep the namespace declaration outside of the conditional so the game will
// still compile nicely with references to the namespace, even if no types in
// it are present.
#if !WINDOWS
internal class Control
{
	internal static Form FromHandle(IntPtr handle)
	{
		throw new NotImplementedException();
	}
}
#endif