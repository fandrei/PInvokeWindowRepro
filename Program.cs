using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PInvokeRepro;

internal static class Program
{
	[STAThread]
	static void Main()
	{
		var windowClass = "TestWindowClass";
		var hInstance = Marshal.GetHINSTANCE(typeof(Program).Module);

		var classInfo = new WNDCLASSEX
		{
			cbSize = Marshal.SizeOf(typeof(WNDCLASSEX)),
		};
		if (GetClassInfoEx(hInstance, windowClass, ref classInfo))
		{
			Debugger.Break();
		}

		classInfo = new WNDCLASSEX
		{
			cbSize = Marshal.SizeOf(typeof(WNDCLASSEX)),
			style = (int)(CS_HREDRAW | CS_VREDRAW | CS_DBLCLKS),
			lpfnWndProc = Marshal.GetFunctionPointerForDelegate((DelegateWndProc)WndProc),
			cbClsExtra = 0,
			cbWndExtra = 0,
			hInstance = hInstance,
			hIcon = IntPtr.Zero,
			hCursor = LoadCursor(IntPtr.Zero, (int)IDC_CROSS),
			hbrBackground = (IntPtr)COLOR_BACKGROUND + 1,
			lpszClassName = windowClass,
			hIconSm = IntPtr.Zero,
		};

		var classReg = RegisterClassEx(ref classInfo);
		if (classReg == 0)
			throw new Win32Exception();

		var hWnd = CreateWindowEx(0, classReg, "խխխ 3", WS_POPUP | WS_VISIBLE,
			100, 100, 200, 200, IntPtr.Zero, IntPtr.Zero, classInfo.hInstance, IntPtr.Zero);
		if (hWnd == IntPtr.Zero)
			throw new Win32Exception();

		ShowWindow(hWnd, 1);
		UpdateWindow(hWnd);

		Application.Run();
	}

	const string User32 = "user32";

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct WNDCLASSEX
	{
		[MarshalAs(UnmanagedType.U4)] public int cbSize;
		[MarshalAs(UnmanagedType.U4)] public int style;
		public IntPtr lpfnWndProc;
		public int cbClsExtra;
		public int cbWndExtra;
		public IntPtr hInstance;
		public IntPtr hIcon;
		public IntPtr hCursor;
		public IntPtr hbrBackground;
		[MarshalAs(UnmanagedType.LPStr)] public string lpszMenuName;
		[MarshalAs(UnmanagedType.LPStr)] public string lpszClassName;
		public IntPtr hIconSm;
	}

	[DllImport(User32)]
	public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

	[DllImport(User32, SetLastError = true, CharSet = CharSet.Unicode)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetClassInfoEx(IntPtr hInstance, [MarshalAs(UnmanagedType.LPStr)] string lpClassName, ref WNDCLASSEX lpWndClass);

	public const UInt32 CS_DBLCLKS = 8;
	public const UInt32 CS_VREDRAW = 1;
	public const UInt32 CS_HREDRAW = 2;

	[DllImport(User32, SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern UInt16 RegisterClassEx([In] ref WNDCLASSEX lpWndClass);

	[DllImport(User32, SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern IntPtr CreateWindowEx(
		int dwExStyle,
		nuint regResult,
		string lpWindowName,
		UInt32 dwStyle,
		int x,
		int y,
		int nWidth,
		int nHeight,
		IntPtr hWndParent,
		IntPtr hMenu,
		IntPtr hInstance,
		IntPtr lpParam);

	delegate IntPtr DelegateWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	public const UInt32 COLOR_BACKGROUND = 1;
	public const UInt32 IDC_CROSS = 32515;
	public const UInt32 WM_DESTROY = 2;
	public const UInt32 WM_PAINT = 0x0f;
	public const UInt32 WS_POPUP = 0x80000000;
	public const UInt32 WS_VISIBLE = 0x10000000;

	static IntPtr WndProc(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam)
	{
		switch (message)
		{
			case WM_PAINT:
				break;

			case WM_DESTROY:
				DestroyWindow(hWnd);
				PostQuitMessage(0);
				break;
		}

		var res = DefWindowProc(hWnd, message, wParam, lParam);
		return res;
	}

	[DllImport(User32)]
	public static extern bool UpdateWindow(IntPtr hWnd);

	[DllImport(User32)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport(User32, SetLastError = true)]
	public static extern bool DestroyWindow(IntPtr hWnd);

	[DllImport(User32)]
	public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

	[DllImport(User32)]
	public static extern void PostQuitMessage(int nExitCode);
}