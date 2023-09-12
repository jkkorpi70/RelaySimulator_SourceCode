using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RelaySim
{
    
    class ToolbarButton : Button
    {

        public ToolbarButton()
        {
            InitButton();
        }

        public ToolbarButton(string Picture)
        {
            PictureBox ButtonImage = new PictureBox("Resources/" + Picture + ".png", 0, 0);
            this.Content = ButtonImage;
            InitButton();
        }

        private void InitButton()
        {
            this.Width = 82;
            this.Height = 27;

        }

        public void SetImage(string Picture)
        {
            PictureBox ButtonImage = new PictureBox("Resources/" + Picture + ".png", 0, 0);
            this.Content = ButtonImage;
        }

    }
    
}
