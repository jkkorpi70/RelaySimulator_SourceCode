using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;

namespace RelaySim
{
    class ComponentButton : Button
    {

        public ComponentButton()
        {
            InitButton();
        }            

        public ComponentButton(string Picture)
        {
            PictureBox ButtonImage = new PictureBox("Resources/" + Picture + ".png",0,0);
            this.Content = ButtonImage;

            InitButton();
        }

        private void InitButton()
        {
            this.Width = 54;
            this.Height = 54;

        }

        public void SetImage(string Picture)
        {
            PictureBox ButtonImage = new PictureBox("Resources/" + Picture + ".png", 0, 0);
            this.Content = ButtonImage;
        }

    }
}
