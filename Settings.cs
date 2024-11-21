namespace OxyPlotPlugin
{
	/// <summary>
	/// Settings class, make sure it can be correctly serialized using JSON.net
	/// </summary>
	public class Settings
	{
		public int Low = 3, Min = 30;
		public double ThresVal = 99, FilterX = 1, FilterY = 1;
		public bool ThresBool = true, LinFit = true;
		public string X = "ShakeITBSV3Plugin.Export.ProxyS.FrontLeft",
					  Y = "ShakeITBSV3Plugin.Export.Grip.FrontLeft";
	}
}
