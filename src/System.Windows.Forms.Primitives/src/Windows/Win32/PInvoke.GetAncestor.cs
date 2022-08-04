﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Win32
{
    internal static partial class PInvoke
    {
        public static HWND GetAncestor<T>(in T hwnd, GET_ANCESTOR_FLAGS flags) where T : IHandle<HWND>
        {
            HWND result = GetAncestor(hwnd.Handle, flags);
            GC.KeepAlive(hwnd);
            return result;
        }
    }
}