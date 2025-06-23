/*	Generate data for 3D Scatter Plot Visualization
 ;	https://blekenbleu.github.io/SimHub/MBAI.htm
 ;	SaveFileDialog:  https://www.c-sharpcorner.com/uploadfile/mahesh/savefiledialog-in-C-Sharp/
 ;	- default to TMP folder
 ;  - use property names after last dot for title and axes labels
 */
using System.IO;
using System.Windows.Forms;

namespace blekenbleu.OxyScope
{
	public partial class Control
	{
		void D3()
		{
			string Text = M.PropName[3];
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
				Title = "Save 3D Visualization data as .txt",
                Filter = "Text file (*.txt)|*.txt",
				FileName = Text,
                InitialDirectory = Path.GetTempPath()
            };
			if (DialogResult.OK == saveFileDialog.ShowDialog()
			 && "" != saveFileDialog.FileName)
			{
				Text +=	";\n::"+M.PropName[0] +
						"::"+M.PropName[1] +
						"::"+M.PropName[2] + ";\n";
				ushort s = M.start[M.which];
				ushort stop = (ushort)(s +  M.length);
				for (; s < stop; s++)
					Text += $"#P{s:000}::{O.x[0,s]:0.000}::{O.x[1,s]:0.000}::"
						 + $"{O.x[2,s]:0.000}::{O.x[3,s]:0.000}::3::A::1::0::0::0::0;\n";
				File.WriteAllText(saveFileDialog.FileName, Text);
			}
		}
	}
}
