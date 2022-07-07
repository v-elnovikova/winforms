// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms
{
    public sealed partial class Application
    {
        public sealed class WinformsOptionalSettings
        {
            public HighDpiSettings? DpiOptions { get; set; }
            public AccessibilitySettings? AccessibilityOptions { get; set; }
            public ThemingSettings? ThemingOptions { get; set; }

            public sealed class HighDpiSettings
            {
                public bool HighDpiImprovementsOne { get; set; }
                public bool HighDpiImprovementsTwo { get; set; }
            }

            public sealed class AccessibilitySettings
            {
                public bool AccessibilityImprovementsOne { get; set; }
                public bool AccessibilityImprovementsTwo { get; set; }
            }

            public sealed class ThemingSettings
            {
                public bool ThemingImprovementsOne { get; set; }
                public bool ThemingImprovementsTwo { get; set; }
            }
        }
    }
}
