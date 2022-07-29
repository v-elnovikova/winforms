**Winforms Application Settings Configuration in .NET**

.NET framework winforms applications use app.config to define application wide settings used by Winforms applications and Winforms runtime, including AppContext switches to opt-in or opt out of the new features released in the latest .NET framework versions. Following are the various sections in app.config that you see storing Winforms application settings.

**Appcontext switches**

These settings are used to opt-in or opt-out of a perticular features from Winforms runtime.

```XML
<configuration>
   <runtime>
      <AppContextSwitchOverrides value="Switch.System.Globalization.NoAsyncCurrentCulture=true" />
   </runtime>
</configuration>
```
**App settings from Settings designer/editor page**

These are settings serialized from User application into app.config file. ex: Settings designer.
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
**System.Windows.Forms.ApplicationConfigurationSection**

Thsi section is primarily used by Winforms runtime to enable HighDpi and accessibility settings and was introduced in .NET framework 4.6+.

```XML
<configuration>
  <System.Windows.Forms.ApplicationConfigurationSection>
  ...
  </System.Windows.Forms.ApplicationConfigurationSection>
</configuration>
```

.NET Winforms applications currently have [limited application
settings](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/whats-new/net60?view=netdesktop-6.0#new-application-bootstrap)
defined at build time via project file that are emitted into source code using source
generators at compile time. This document outlines expansion of those
application wide settings further and cover the scenarios that were using `System.Windows.Forms.ApplicationConfigurationSection` and  `AppContextSwitchOverrides` in .NET framework applications.

**runtimeConfig.Json for Winforms .NET applications.**

.NET framework applications used app.config to define application
settings. However, in .NET we are moving away from app.config for
performance and reliability reasons and runtimeconfig.Json to define and store Winforms application settings.

**Goals:**

-   Replacement for `AppContextSwitchOverrides` and `System.Windows.Forms.ApplicationConfigurationSection`
-   Users should be able to update/modify Winforms applications settings
    without recompiling the application

-   Existing applications should be able to seamlessly upgrade to this
    new model.

-   Support existing applications settings from the project file.

-   Unify winforms application settings (No more build time properties
    in proj file?)

-   Prevent performance loss due to disk read of runtimeConfig.Json.

**Out of Goal:**

-  App settings from Settings designer/editor page.

-    Validations around the combination of Winforms settings defined in
    runtimeConfig.Json.

-   Dynamic/real-time load of Appsettings.

**Syntax of Winforms application settings in runtimeConfig.Json**.

```xml
{
  "WinformsApplicationSettings": {
    "ApplicationDefaultFont": "",
    "ApplicationDpiMode": "SystemAware",
    "EnableVisualStyles": "true",
    "EnableTextRendering": "true",
    "HighDpiImprovementSettings": {
      "HdpiImprovementsSystemAware": "true",
      "HdpiImprovementsPermonitor": "true"
    },
    "LayoutImprovementSettings": {
      "DpckingANdAcnhorImprovements": "true"
    }
  }
}
```

**Reading Appsettings:**

There are two approaches here.

**At runtime:**

Read applications settings always form runtimeConfig.Json at runtime and
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
