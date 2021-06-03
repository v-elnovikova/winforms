// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms
{
    public sealed partial class Application
    {
        public class ApplicationDefaults
        {
            internal const string DefaultPadding = nameof(DefaultPadding);
            internal const string DefaultMargin = nameof(DefaultMargin);
            internal const string DefaultRightToLeft = nameof(DefaultRightToLeft);
            internal const string DefaultSize = nameof(DefaultSize);

            internal const string DefaultStartPosition = nameof(DefaultStartPosition);
            internal const string DefaultShowInTaskbar = nameof(DefaultShowInTaskbar);

            internal ApplicationDefaults()
            {
                DefaultProperties = new Dictionary<(Type, string), object>();

                // Setting up the Default-Defaults.
                TryAddDefaultValue<Control>(DefaultPadding, Padding.Empty);
                TryAddDefaultValue<Control>(DefaultMargin, CommonProperties.DefaultMargin);
                TryAddDefaultValue<Control>(DefaultRightToLeft, RightToLeft.No);
                TryAddDefaultValue<Control>(DefaultSize, RightToLeft.No);

                TryAddDefaultValue<GroupBox>(DefaultPadding, new Padding(3));
                TryAddDefaultValue<GroupBox>(DefaultSize, new Size(200, 200));

                TryAddDefaultValue<Form>(DefaultStartPosition, FormStartPosition.WindowsDefaultLocation);
                TryAddDefaultValue<Form>(DefaultShowInTaskbar, true);
            }

            private Dictionary<(Type, string), object> DefaultProperties { get; }

            internal bool TryAddDefaultValue<ComponentType>(string propertyName, object value) where ComponentType : Component
                => DefaultProperties.TryAdd((typeof(ComponentType), propertyName), value);

            internal bool TryGetDefaultValue<ComponentType>(string propertyName, out object value) where ComponentType : Component
            {
                if (DefaultProperties.TryGetValue((typeof(ComponentType), propertyName), out var valueInternal))
                {
                    value = valueInternal;
                    return true;
                }

                value = null;
                return false;
            }

            internal bool TryAddDefaultValue(string propertyName, object value)
                => DefaultProperties.TryAdd((typeof(Control), propertyName), value);

            internal bool TryGetDefaultValue(string propertyName, out object value)
            {
                if (DefaultProperties.TryGetValue((typeof(Control), propertyName), out var valueInternal))
                {
                    value = valueInternal;
                    return true;
                }

                value = null;
                return false;
            }
            
            internal T GetValueOrDefault<T>(string propertyName)
                => (T)DefaultProperties.GetValueOrDefault((typeof(Control), propertyName));

            internal T GetValueOrDefault<T>(
                Type controlType,
                Type fallbackControlType,
                [CallerMemberName] string propertyName = null)
            {
                if (DefaultProperties.TryGetValue((controlType, propertyName), out var value))
                {
                    return (T)value;
                }
                else
                {
                    return fallbackControlType is null
                           ? default(T)
                           : (T)DefaultProperties.GetValueOrDefault((fallbackControlType, propertyName));
                }
            }

            internal T GetValueOrDefault<ControlType, T>(string propertyName) where ControlType : Component
                => (T)DefaultProperties.GetValueOrDefault((typeof(ControlType), propertyName));

            internal void Remove(string propertyName)
                => DefaultProperties.Remove((typeof(Control), propertyName));
        }
    }
}
