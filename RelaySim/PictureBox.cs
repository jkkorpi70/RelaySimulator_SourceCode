using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace RelaySim
{
    class PictureBox : Image
    {
        public string FileSuffix = ".png";
        

        public double X
        {
            get { return this.Margin.Left; }
            set
            {
                Thickness tempMargin = this.Margin;
                tempMargin.Left = value;
                this.Margin = tempMargin;
            }
        }

        public double Y
        {
            get { return this.Margin.Top; }
            set
            {
                Thickness tempMargin = this.Margin;
                tempMargin.Top = value;
                this.Margin = tempMargin;
            }
        }

        //public object MouseButtons { get; private set; }

        public PictureBox()
        {
            SetInitialPosition(0, 0);
        }

        public PictureBox(string FileName)
        {
            SetImage(FileName);
            SetInitialPosition(0, 0);
        }

        public PictureBox(double X, double Y)
        {
            SetInitialPosition(X, Y);
        }

        public PictureBox(string FileName, double X, double Y)
        {
            SetImage(FileName);
            SetInitialPosition(X, Y);
        }

        ~PictureBox() { }

        //-------------------------------------------------------------------------------------

        private void SetInitialPosition(double X, double Y)
        {
            this.Margin = new Thickness(X, Y, 0, 0);
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
        }

        //-------------------------------------------------------------------------------------

        public void SetImage(string FileName)
        {
            BitmapImage BoxImage = new BitmapImage();
            BoxImage.BeginInit();
            BoxImage.UriSource = new Uri(FileName, UriKind.RelativeOrAbsolute);
            BoxImage.EndInit();
            this.Stretch = System.Windows.Media.Stretch.None;
            this.Source = BoxImage;
        }

        public void SetResourceImage(string ImageName)
        {
            SetImage("Resources/" + ImageName + FileSuffix);
        }

        public void SetCoordinates(double X, double Y)
        {
            Thickness tempMargin = this.Margin;
            tempMargin.Left = X;
            tempMargin.Top = Y;
            this.Margin = tempMargin;
        }

        public void SetDimensions(double width, double height)
        {
            this.Width = width;
            this.Height = height;
            this.Stretch = System.Windows.Media.Stretch.Uniform;
        }
    }
}
