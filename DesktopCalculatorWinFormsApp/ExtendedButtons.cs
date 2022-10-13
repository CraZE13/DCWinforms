using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Input;

namespace DesktopCalculatorWinFormsApp
{
    public class MinimalButton : C1Button
    {
        public MinimalButton()
        {
            Font = new(StringResource.FontName, 12);
            Margin = new Padding(0);
            TabStop = false;
            FlatStyle = FlatStyle.Flat;
            Dock = DockStyle.Fill;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Color.SkyBlue;
        }
    }

    public class MinimalSplitButton : C1SplitButton
    {
        public MinimalSplitButton()
        {
            Font = new(StringResource.FontName, 12);
            Margin = new Padding(0);
            TabStop = false;
            FlatStyle = FlatStyle.Flat;
            Dock = DockStyle.Fill;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Color.SkyBlue;
        }
    }
}
