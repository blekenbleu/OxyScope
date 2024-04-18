# OxyPlotPlugin from SimHubPluginSdk
- Create a new WPF project named `OxyPlotPlugin` in Visual Studio, then quit.  
- delete (or move away) all but `OxyPlotPlugin.sln` and `OxyPlotPlugin.csproj`  
- copy all but .sln and .csproj from SimHubPluginSdk
- in Plugin.cs, change:  
```
    [PluginDescription("An XY OxyPlot of 2 SimHub properties")]
    [PluginAuthor("blekenbleu")]
    [PluginName("OxyPlot plugin")]
```
- split diff edit OxyPlotPlugin.sln and SimHubPluginSdk.sln
	- delete `GlobalSection(ExtensibilityGlobals) = postSolution` from OxyPlotPlugin.sln
- split diff edit OxyPlotPlugin.csproj and SimHubPluginSdk.csproj; changes in OxyPlotPlugin.csproj:
	- change `OutputType` to `Library`; add `<AppDesignerFolder>Properties</AppDesignerFolder>`
	- delete last 4 lines in that PropertyGroup
	- delete `PlatformTarget`s; change OutputPath to $(SIMHUB_INSTALL_PATH)
	- insert unique SimHubPluginSdk PropertyGroups for Release and Debug
	- insert unique SimHubPluginSdk References
	- delete unique OxyPlotPlugin References
	- insert unique SimHubPluginSdk Compile ItemGroups
	- delete unique OxyPlotPlugin lines  

<details><summary>click for Properties folder differences</summary>
<ul>
<li>delete <code>Settings.Designer.cs<code> and <code>Settings.settings</code>
<li>copy <code>DesignTimeResources.xaml</code>
<li>in AssemblyInfo.cs, replace NeutralResourcesLanguage assembly lines with SimHubPluginSdk's one-liner
<li>in Resources.Designer.cs, add 10 lines for sdkmenuicon
<li>in Resources.resx, add 4 lines for sdkmenuicon; force othe lines to match
</ul>
</details>

- convert content from SimHubPluginSdk namespace `Sdk.Plugin` to `OxyPlotPlugin`
	Control.xaml (2x), Control.xaml.cs, Plugin.cs, Settings.cs
