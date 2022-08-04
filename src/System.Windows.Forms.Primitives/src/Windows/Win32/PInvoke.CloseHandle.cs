﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.Win32.Foundation;

namespace Windows.Win32
{
    internal static partial class PInvoke
    {
        public static BOOL CloseHandle(IHandle handle)
        {
            BOOL result = CloseHandle((HANDLE)handle.Handle);
            GC.KeepAlive(handle);
            return result;
        }
    }
}
