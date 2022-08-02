// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Text.Json;

namespace System.Windows.Forms.Primitives.RuntimeTargetFramework
{
    internal static partial class RuntimeTargetFramework
    {
        private static bool readConfig;
        private static TargetFramework? s_framework;
        public static TargetFramework? Framework
        {
            get
            {
                if (!readConfig)
                {
                    ReadTargetFrameworkFromConfig();
                }

                return s_framework;
            }
        }

        public static void ReadTargetFrameworkFromConfig()
        {
            string runtimeConfigPath = $"{Process.GetCurrentProcess().ProcessName}.runtimeconfig.json";

            var jsonDocumentOptions = new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            };

            using (var stream = File.OpenRead(runtimeConfigPath))
            using (JsonDocument doc = JsonDocument.Parse(stream, jsonDocumentOptions))
            {
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("runtimeOptions", out var runtimeOptionsRoot))
                {
                    if (runtimeOptionsRoot.TryGetProperty("framework", out var framework))
                    {
                        var runtimeConfigFramework = new TargetFramework();
                        string? name = null;
                        string? version = null;
                        foreach (var property in framework.EnumerateObject())
                        {
                            if (property.Name.Equals(nameof(name), StringComparison.OrdinalIgnoreCase))
                            {
                                name = property.Value.GetString();
                            }

                            if (property.Name.Equals(nameof(version), StringComparison.OrdinalIgnoreCase))
                            {
                                version = property.Value.GetString();
                            }
                        }

                        if (name == null || version == null)
                        {
                            s_framework = null;
                        }
                        else
                        {
                            s_framework = new TargetFramework
                            {
                                Name = name,
                                Version = version
                            };
                        }
                    }
                    else
                    {
                        s_framework = null;
                    }
                }

                readConfig = true;
            }
        }
    }
}
