﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal static partial class User32
    {
        [DllImport(Libraries.User32)]
        public unsafe static extern BOOL ValidateRect(IntPtr hWnd, RECT* lpRect);

        public unsafe static BOOL ValidateRect(IHandle hWnd, RECT* lpRect)
        {
            BOOL result = ValidateRect(hWnd.Handle, lpRect);
            GC.KeepAlive(hWnd);
            return result;
        }
    }
}
