using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class WinOsUtil 
{
    //https://www.pinvoke.net/default.aspx/user32.GetWindow
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowType uCmd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    // sets the given window to the foreground window
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    // returns the foreground window
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    // focuses the given window
    [DllImport("user32.dll")]
    public static extern bool SetFocus(IntPtr hWnd);

    //code for removing window borders
    //https://www.codeproject.com/Questions/413778/Removing-a-Window-Border-No-Winform
    //Import window changing function
    [DllImport("USER32.DLL")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    [DllImport("user32.dll")]
    static extern bool DrawMenuBar(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, uint wFlags);

    // cycles through every window and calls callback
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool EnumWindows(EnumWindowsProc callback, IntPtr extraData);

    [DllImport("USER32.DLL")]
    public static extern IntPtr GetShellWindow();


    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    //static extern IntPtr SetFocus(HandleRef hWnd);
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("USER32.DLL")]
    public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [DllImport("USER32.DLL")]
    public static extern int GetWindowTextLength(IntPtr hWnd);




    // Returnsthe process ID associated with the window
    /*[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);
	*/
    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint lpdwProcessId);

    // ???
    [DllImport("user32.dll")]
    public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    ///////////
    // [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_BASIC_INFORMATION
    {
        public int ExitStatus;
        public int PebBaseAddress;
        public int AffinityMask;
        public int BasePriority;
        public uint UniqueProcessId;
        public uint InheritedFromUniqueProcessId;
    }
    [DllImport("kernel32.dll")]
    public static extern bool TerminateProcess(uint hProcess, int exitCode);

    [DllImport("ntdll.dll")]
    public static extern int NtQueryInformationProcess(
        IntPtr hProcess,
        int processInformationClass /* 0 */,
        ref PROCESS_BASIC_INFORMATION processBasicInformation,
        uint processInformationLength,
        out uint returnLength
        );


    public const int GWL_STYLE = -16;              //hex public constant for style changing
    public const uint WS_BORDER = 0x800000;

    /// <summary>The window has a title bar (includes the WS_BORDER style).</summary>
    public const uint WS_CAPTION = 0xc00000;

    /// <summary>The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.</summary>
    public const uint WS_CHILD = 0x40000000;

    /// <summary>Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.</summary>
    public const uint WS_CLIPCHILDREN = 0x2000000;

    /// <summary>
    ///   Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated.
    ///   If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
    /// </summary>
    public const uint WS_CLIPSIBLINGS = 0x4000000;

    /// <summary>The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.</summary>
    public const uint WS_DISABLED = 0x8000000;

    /// <summary>The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.</summary>
    public const uint WS_DLGFRAME = 0x400000;

    /// <summary>
    ///   The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the WS_GROUP style.
    ///   The first control in each group usually has the WS_TABSTOP style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys.
    ///   You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
    /// </summary>
    public const uint WS_GROUP = 0x20000;

    /// <summary>The window has a horizontal scroll bar.</summary>
    public const uint WS_HSCROLL = 0x100000;

    /// <summary>The window is initially maximized.</summary>
    public const uint WS_MAXIMIZE = 0x1000000;

    /// <summary>The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
    public const uint WS_MAXIMIZEBOX = 0x10000;

    /// <summary>The window is initially minimized.</summary>
    public const uint WS_MINIMIZE = 0x20000000;

    /// <summary>The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
    public const uint WS_MINIMIZEBOX = 0x20000;

    /// <summary>The window is an overlapped window. An overlapped window has a title bar and a border.</summary>
    public const uint WS_OVERLAPPED = 0x0;

    /// <summary>The window is an overlapped window.</summary>
    public const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;

    /// <summary>The window is a pop-up window. This style cannot be used with the WS_CHILD style.</summary>
    public const uint WS_POPUP = 0x80000000u;

    /// <summary>The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.</summary>
    public const uint WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU;

    /// <summary>The window has a sizing border.</summary>
    public const uint WS_SIZEFRAME = 0x40000;

    /// <summary>The window has a window menu on its title bar. The WS_CAPTION style must also be specified.</summary>
    public const uint WS_SYSMENU = 0x80000;

    /// <summary>
    ///   The window is a control that can receive the keyboard focus when the user presses the TAB key.
    ///   Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style.
    ///   You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
    ///   For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
    /// </summary>
    public const uint WS_TABSTOP = 0x10000;

    /// <summary>The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.</summary>
    public const uint WS_VISIBLE = 0x10000000;

    /// <summary>The window has a vertical scroll bar.</summary>
    public const uint WS_VSCROLL = 0x200000;

    public const int SWP_ASYNCWINDOWPOS = 0x4000;
    public const int SWP_DEFERERASE = 0x2000;
    public const int SWP_DRAWFRAME = 0x0020;
    public const int SWP_FRAMECHANGED = 0x0020;
    public const int SWP_HIDEWINDOW = 0x0080;
    public const int SWP_NOACTIVATE = 0x0010;
    public const int SWP_NOCOPYBITS = 0x0100;
    public const int SWP_NOMOVE = 0x0002;
    public const int SWP_NOOWNERZORDER = 0x0200;
    public const int SWP_NOREDRAW = 0x0008;
    public const int SWP_NOREPOSITION = 0x0200;
    public const int SWP_NOSENDCHANGING = 0x0400;
    public const int SWP_NOSIZE = 0x0001;
    public const int SWP_NOZORDER = 0x0004;
    public const int SWP_SHOWWINDOW = 0x0040;

    public const int HWND_TOP = 0;
    public const int HWND_BOTTOM = 1;
    public const int HWND_TOPMOST = -1;
    public const int HWND_NOTOPMOST = -2;


}
public enum GetWindowType : uint
{
    /// <summary>
    /// The retrieved handle identifies the window of the same type that is highest in the Z order.
    /// <para/>
    /// If the specified window is a topmost window, the handle identifies a topmost window.
    /// If the specified window is a top-level window, the handle identifies a top-level window.
    /// If the specified window is a child window, the handle identifies a sibling window.
    /// </summary>
    GW_HWNDFIRST = 0,
    /// <summary>
    /// The retrieved handle identifies the window of the same type that is lowest in the Z order.
    /// <para />
    /// If the specified window is a topmost window, the handle identifies a topmost window.
    /// If the specified window is a top-level window, the handle identifies a top-level window.
    /// If the specified window is a child window, the handle identifies a sibling window.
    /// </summary>
    GW_HWNDLAST = 1,
    /// <summary>
    /// The retrieved handle identifies the window below the specified window in the Z order.
    /// <para />
    /// If the specified window is a topmost window, the handle identifies a topmost window.
    /// If the specified window is a top-level window, the handle identifies a top-level window.
    /// If the specified window is a child window, the handle identifies a sibling window.
    /// </summary>
    GW_HWNDNEXT = 2,
    /// <summary>
    /// The retrieved handle identifies the window above the specified window in the Z order.
    /// <para />
    /// If the specified window is a topmost window, the handle identifies a topmost window.
    /// If the specified window is a top-level window, the handle identifies a top-level window.
    /// If the specified window is a child window, the handle identifies a sibling window.
    /// </summary>
    GW_HWNDPREV = 3,
    /// <summary>
    /// The retrieved handle identifies the specified window's owner window, if any.
    /// </summary>
    GW_OWNER = 4,
    /// <summary>
    /// The retrieved handle identifies the child window at the top of the Z order,
    /// if the specified window is a parent window; otherwise, the retrieved handle is NULL.
    /// The function examines only child windows of the specified window. It does not examine descendant windows.
    /// </summary>
    GW_CHILD = 5,
    /// <summary>
    /// The retrieved handle identifies the enabled popup window owned by the specified window (the
    /// search uses the first such window found using GW_HWNDNEXT); otherwise, if there are no enabled
    /// popup windows, the retrieved handle is that of the specified window.
    /// </summary>
    GW_ENABLEDPOPUP = 6
}