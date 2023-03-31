using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ReLogic.Localization.IME.Windows;

internal class ImeWrapper
{
	private IntPtr _hWnd;

	public void ImeUi_Initialize(IntPtr hWnd, bool bDisabled = false)
	{
		_hWnd = hWnd;
		if (bDisabled) {
			ImeUi_Enable(false);
		}
	}

	public void ImeUi_Uninitialize()
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		NativeMethods.ImmDestroyContext(hImc);
	}

	public void ImeUi_Enable(bool bEnable)
	{
		if (bEnable) {
			IntPtr hImc = NativeMethods.ImmCreateContext();
			NativeMethods.ImmAssociateContext(_hWnd, hImc);
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
		}
		else {
			IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
			NativeMethods.ImmAssociateContext(_hWnd, IntPtr.Zero);
			NativeMethods.ImmDestroyContext(hImc);
		}
	}

	public void ImeUi_FinalizeString(bool bSend = false)
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		NativeMethods.ImmSetCompositionString(hImc, Imm.SCS_SETSTR, "", 0, null, 0);
		NativeMethods.ImmNotifyIME(hImc, Imm.NI_COMPOSITIONSTR, 0, 0);
		NativeMethods.ImmReleaseContext(_hWnd, hImc);
	}

	public string ImeUi_GetCompositionString()
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		int size = NativeMethods.ImmGetCompositionString(hImc, Imm.GCS_COMPSTR, IntPtr.Zero, 0);
		if (size == 0) {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
			return "";
		}

		IntPtr buffer = Marshal.AllocHGlobal(size);
		NativeMethods.ImmGetCompositionString(hImc, Imm.GCS_COMPSTR, buffer, size);
		NativeMethods.ImmReleaseContext(_hWnd, hImc);

		byte[] buf = new byte[size];
		Marshal.Copy(buffer, buf, 0, size);
		Marshal.FreeHGlobal(buffer);
		return Encoding.Unicode.GetString(buf, 0, size);;
	}

	public string ImeUi_GetCandidate(uint index)
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		int size = NativeMethods.ImmGetCandidateList(hImc, 0, IntPtr.Zero, 0);
		if (size == 0) {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
			return "";
		}

		IntPtr candListBuffer = Marshal.AllocHGlobal(size);
		NativeMethods.ImmGetCandidateList(hImc, 0, candListBuffer, size);
		NativeMethods.ImmReleaseContext(_hWnd, hImc);

		CandidateList candList = Marshal.PtrToStructure<CandidateList>(candListBuffer);
		byte[] buf = new byte[size];
		Marshal.Copy(candListBuffer, buf, 0, size);
		Marshal.FreeHGlobal(candListBuffer);

		if (index >= candList.dwCount) {
			return "";
		}

		uint offsetI = BitConverter.ToUInt32(buf, ((int)index + 6) * sizeof(uint));
		uint offsetJ = 0;
		if (index == candList.dwCount - 1) {
			offsetJ = candList.dwSize;
		}
		else {
			offsetJ = BitConverter.ToUInt32(buf, ((int)index + 7) * sizeof(uint));
		}

		int strLen = (int)(offsetJ - offsetI - 2);
		string candidate = Encoding.Unicode.GetString(buf, (int)offsetI, strLen);
		return candidate;
	}

	public uint ImeUi_GetCandidateSelection()
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		int size = NativeMethods.ImmGetCandidateList(hImc, 0, IntPtr.Zero, 0);
		if (size == 0) {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
			return 0;
		}

		IntPtr candListBuffer = Marshal.AllocHGlobal(size);
		NativeMethods.ImmGetCandidateList(hImc, 0, candListBuffer, size);
		NativeMethods.ImmReleaseContext(_hWnd, hImc);

		CandidateList candList = Marshal.PtrToStructure<CandidateList>(candListBuffer);
		return candList.dwSelection;
	}

	public uint ImeUi_GetCandidatePageSize()
	{
		IntPtr hImc = NativeMethods.ImmGetContext(_hWnd);
		int size = NativeMethods.ImmGetCandidateList(hImc, 0, IntPtr.Zero, 0);
		if (size == 0) {
			NativeMethods.ImmReleaseContext(_hWnd, hImc);
			return 0;
		}

		IntPtr candListBuffer = Marshal.AllocHGlobal(size);
		NativeMethods.ImmGetCandidateList(hImc, 0, candListBuffer, size);
		NativeMethods.ImmReleaseContext(_hWnd, hImc);

		CandidateList candList = Marshal.PtrToStructure<CandidateList>(candListBuffer);
		return candList.dwPageSize;
	}

	public bool ImeUi_IsCandidateListVisible()
	{
		return ImeUi_GetCandidate(0) != "";
	}
}