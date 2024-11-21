# OxyPlotPlugin from SimHubPluginSdk
XY plot of *positive* property values
- Create a new WPF project named `OxyPlotPlugin` in Visual Studio, then quit.  
- delete (or move away) all but `OxyPlotPlugin.sln` and `OxyPlotPlugin.csproj`  
- copy all but .sln and .csproj from SimHubPluginSdk
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

### Done
- convert content from SimHubPluginSdk namespace `Sdk.Plugin` to `OxyPlotPlugin`
	Control.xaml (2x), Control.xaml.cs, Plugin.cs, Settings.cs
- in Plugin.cs, changed:  
```
    [PluginDescription("XY OxyPlot of paired SimHub properties")]
    [PluginAuthor("blekenbleu")]
    [PluginName("OxyPlot XY plugin")]
	...
	...
	public string LeftMenuTitle => "OxyPlot XY plugin";
```
- added OxyPlot stuff for scatter plot  
	![](Doc/pasted.png)  
- break out scatterplot method from keypress event
- update plot data from Plugin.cs  
	![](Doc/proto.png)  
- ping pong buffers for capturing high dynamic range sample sets  
- min/max X sample range sliders
- "autoexposure" `Refresh` button visible only for value captures based on sliders
- capture property names and validate values of selected properties  

### Many plot properties can change for any plot
- `new ScatterSeries` each time...

#### 10 Nov:&nbsp; better prompts and feedback
- better icon, simpler xaml, version number
- both X and Y autorange based on max property sample values
- **Replot** when min sample values < `X Below`, `Y Below` % of corresponding max values
#### 21 Nov:
- added a threshold slider for minimum useful X values.
- `View.Dispatcher.Invoke()` to auto `View.Replot()` *from* `DataUpdate()` *thread*

#### Property folder changes from default WPF project for SimHub plugins
<details><summary>click for differences</summary>
<ul>
<li>delete <code>Settings.Designer.cs<code> and <code>Settings.settings</code>
<li>copy <code>DesignTimeResources.xaml</code>
<li>in AssemblyInfo.cs, replace NeutralResourcesLanguage assembly lines with SimHubPluginSdk's one-liner
<li>in Resources.Designer.cs, add 10 lines for sdkmenuicon
<li>in Resources.resx, add 4 lines for sdkmenuicon; force othe lines to match
</ul>
</details>
