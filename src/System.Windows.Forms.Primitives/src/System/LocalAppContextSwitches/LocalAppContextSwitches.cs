// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Windows.Forms.Primitives.LocalAppContextSwitches
{
    /// <summary>
    /// Class reads and caches WinForm specific feature-falg values specified in the runtimeconfig.json file.
    /// </summary>
    internal static partial class LocalAppContextSwitches
    {
        // Cache value for the diable-cache test switch setting.
        private static bool s_disableCache => AppContext.TryGetSwitch("TestSwitch.LocalAppContext.DisableCaching", out bool disableCaching)
            && disableCaching;

        private static int s_scaleTopLevelFormMinMaxSizeWithDpi;
        public static bool ScaleTopLevelFormMinMaxSizeWithDpi
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetCachedSwitchValue("Switch.System.Windows.Forms.ScaleTopLevelFormMinMaxSizeWithDpi", ref s_scaleTopLevelFormMinMaxSizeWithDpi);
        }

        // Returns value of given switch using provided cache.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool GetCachedSwitchValue(string switchName, ref int cachedSwitchValue)
        {
            // The cached switch value has 3 states: 0 - unknown, 1 - true, -1 - false
            if (cachedSwitchValue < 0)
                return false;
            if (cachedSwitchValue > 0)
                return true;

            return GetCachedSwitchValueInternal(switchName, ref cachedSwitchValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool GetCachedSwitchValueInternal(string switchName, ref int cachedSwitchValue)
        {
            bool hasSwitch = AppContext.TryGetSwitch(switchName, out bool isSwitchEnabled);
            if (!hasSwitch)
            {
                var switchDefaultValue = (bool?)GetSwitchDefaultValue(switchName);
                isSwitchEnabled = switchDefaultValue ?? false;
            }

            if (!s_disableCache)
            {
                cachedSwitchValue = isSwitchEnabled ? 1 /*true*/ : -1 /*false*/;
            }

            return isSwitchEnabled;
        }

        // Provides default values for switches if they're not always false by default
        private static object? GetSwitchDefaultValue(string switchName)
        {
            if (!OsVersion.IsWindows10_1703OrGreater)
            {
                return default;
            }

            var tfm = RuntimeTargetFramework.Framework;
            if (tfm is not null && tfm.Name == "Microsoft.NETCore.App" && string.Compare(tfm.Version, "8.0", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (switchName == "Switch.System.Windows.Forms.ScaleTopLevelFormMinMaxSizeWithDpi")
                {
                    return true;
                }
            }

            return default;
        }
    }
}
