#if NETCORE
// This enum is used to stub an expected value in CaptureCamera::SaveImage, but
// we give is a dummy forms namespace to reduce patches from the namespace no
// longer being available in .NET Core.

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Windows.Forms;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public enum ImageFormat { Png }
#endif