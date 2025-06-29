/*	Generate data for 3D Scatter Plot Visualization
 ;	https://blekenbleu.github.io/SimHub/MBAI.htm
 ;	SaveFileDialog:  https://www.c-sharpcorner.com/uploadfile/mahesh/savefiledialog-in-C-Sharp/
 ;	- default to TMP folder
 */
using System.IO;
using System.Windows.Forms;

namespace blekenbleu.OxyScope
{
	public partial class Control
	{
		void D3()	// 2 or 3 Y properties
		{
			string Text = M.PropName[3];
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Title = "Save 3D Visualization data as .txt",
				Filter = "Text file (*.txt)|*.txt",
				FileName = Text,
				InitialDirectory = Path.GetTempPath()
			};
			ushort y1 = 1, y2;
			double y3;
			if (M.axis[1])
				y2 = (ushort)(M.axis[2] ? 2 : 3);
			else y2 = (ushort)(1 + (y1 = 2));
			bool x = 2 == y2; 
			if (DialogResult.OK == saveFileDialog.ShowDialog()
			 && "" != saveFileDialog.FileName)
			{
				Text +=	";\n::"+M.PropName[0] +
						"::"+M.PropName[y1] +
						"::"+M.PropName[y2] + ";\n";
				ushort s = start;
				ushort stop = (ushort)(s +  Length);
				for (; s < stop; s++)
				{
					y3 = x ? O.x[3,s] : s - start;
					Text += $"#P{s:000}::{O.x[0,s]:0.000}::{O.x[y1,s]:0.000}::"
						 + $"{O.x[y2,s]:0.000}::{y3:0.000}::3::A::1::0::0::0::0;\n";
				}
				File.WriteAllText(saveFileDialog.FileName, Text);
			}
		}
	}
}
