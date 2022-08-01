**Winforms Application Settings Configuration in .NET**

**Application configuration in .NET framework applications**

.NET framework winforms applications use app.config to define application wide settings used by Winforms applications, including AppContext switches to opt-in or opt out of the new features released in the latest .NET framework versions. Following are the various sections in the app.config that define Winforms application settings.

**Appcontext switches**

These settings are used to opt-in or opt-out of a perticular features from Winforms runtime.

```XML
<configuration>
   <runtime>
      <AppContextSwitchOverrides value="Switch.System.Globalization.NoAsyncCurrentCulture=true" />
   </runtime>
</configuration>
```
**System.Windows.Forms.ApplicationConfigurationSection**

This was introduced in .NET framework 4.6+ and is primarily used by Winforms runtime to enable HighDpi and accessibility improvements.

```XML
<configuration>
  <System.Windows.Forms.ApplicationConfigurationSection>
  ...
  </System.Windows.Forms.ApplicationConfigurationSection>
</configuration>
```

**App settings from Settings designer/editor page**

Unlike above sections, these settings are used by the user applications. These are defined by the developers developing Winforms applications and Visual Studio then serializes them into app.config file. ex: Settings designer.
```XML
 <userSettings>
        <WinFormsApp2.Properties.Settings>
            <setting name="Settingdfsd" serializeAs="String">
                <value>dfds</value>
            </setting>
        </WinFormsApp2.Properties.Settings>
    </userSettings>
    <applicationSettings>
        <WinFormsApp2.Properties.Settings>
            <setting name="dfsd" serializeAs="String">
                <value>sdfsdgs</value>
            </setting>
        </WinFormsApp2.Properties.Settings>
    </applicationSettings>
```

**Application configuration in .NET (Core) applications**

.NET Winforms applications currently have [limited application
configurations](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/whats-new/net60?view=netdesktop-6.0#new-application-bootstrap)
defined at build time via project file that are emitted into source code using source
generators at compile time. This document outlines expansion of those
application wide settings further.

**runtimeConfig.Json for Winforms .NET applications.**

app.config has limited support in .NET and goal is to move away from using it in .NET for
performance and reliability reasons. In this proposal, we are leveraging runtimeconfig.json to define and store Winforms application configurations.

**Goals:**

-   Replacement for `AppContextSwitchOverrides` and `System.Windows.Forms.ApplicationConfigurationSection` of app.config.

-   Users should be able to update/modify Winforms applications settings
    without recompiling the application

-   Existing applications should be able to seamlessly upgrade to this
    new model when tragetting to latest .NET.
    
-   Unify winforms application settings (No more build time properties
    in proj file?)

**Out of Goal:**

-  App settings from Settings designer/editor page.
-  Dynamic/real-time loading of configuration values from runtimeconfig.json.
-  Non-boolean type configurations are out of scope of this proposal (?)

**Syntax of Winforms application settings in runtimeConfig.Json**.

```xml
{
  "WinformsRuntimeConfigurations": {
    "EnableVisualStyles": "true",
    "EnableTextRendering": "true",
    "HighDpiImprovementConfigurations": {
      "HdpiImprovementsSystemAware": "true",
      "HdpiImprovementsPermonitor": "true"
    },
    "LayoutImprovementConfigurations": {
      "DpckingANdAcnhorImprovements": "true"
    },
    "AccessibilityImprovementConfigurations": {
      "AccessibilityImprovements": "true"
    }
  }
}
```

**Reading Winforms application configurations:**

There are two approaches here.

**At runtime:**

Read applications configurations always form runtimeconfig.json at runtime and
populate all associated properties that are later used at various levels
in the runtime. This approach would start throwing compile time error if
build properties ( i.e properties in proj file etc) contains any winforms settings and project is being targeted
with latest .NET SDK.

Ex: Currently source generator emits the `ApplicationConfiguartion.Initialize()` API into source code. We will
modify this to the following.

```cs
 ApplicationConfiguartion.Initialize()
 {
   If(ReadApplicationSettings())
    { 
      throw new InvalidSettingsException();
    }

   Application.ApplyWinformsApplicationSettings();
}
 
bool ApplicationConfiguartion.ReadApplicationSettings()
{
  IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("runtimeConfig.json",true).Build()");
  Application.SetWinformsSettings(configuration.GetSection("WinformsOptionalSettings").Get<WinformsOptionalSettings>())");
  ....
  return true;
}
```

**Read at compile-time:**

In order to improve performance, we can emit application settings known
at the design time into source by reading runtimeConfig.Json and thus,
deployed application may avoid distributing this settings file and
thereby avoid reading disk at runtime.

Source generator, read settings from the runtimeConfig.Json file and emits
code into source. Following is the snippet on how this looks. Analyzer
would throw compile time warnings (not errors?) if any winforms settings is defined as build time property( ex: via Project file)

In case any setting is defined both at build time and in runtimeConfig.Json file, setting from runtimeConfig.Json file will take precedence.
```cs
ApplicationConfiguartion.Initialize()
 {
   If(runtimeConfigJsonExists)
    {
      IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("runtimeConfig.json", true).Build()");
      Application.SetWinformsSettings(configuration.GetSection("WinformsOptionalSettings").Get<WinformsOptionalSettings>())"); 
    }
    
    If(EnableVisualStyles)
     {
        Application.EnableVisualStyles();
     }
     
    If(EnableTextRendering)
     {
       Application.SetCompatibleTextRenderingDefault(false);
     }

    If(HighDpiMode)
     {
       Application.SetHighDpiMode(HighDpiMode.SystemAware);
     }
    
    If(EnableHidpiImprovementSwitch)
     {
        AppContext.SetSwitch("EnableHighDpiImprovements", true);
     }
 }
```
**Environment specific AppSettings.Json:**

Compile time reading of application settings gives flexibility to add
environment specific Appsettings file. Here, we could make source
generator always use \`runtimeConfig.Dev.Json\` to read and emit source
code and at runtime, it application reads only \`runtimeConfig.Json\` if
present.

**Pros**:

-   Help developer control the settings being applied default.

-   Avoid deploying runtimeConfig.Json file in most cases while end user
    can still be able to alter the settings without recompiling
    applications.
