using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Threading;


namespace RelaySim
{
    class Workbench : Canvas
    {
        private readonly static string PATH = "Resources/";
        private string SavedFileName = "";
        private double SaveTime; // Save cursor for 2 secs when saving file
        private readonly static DispatcherTimer CursorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };

        private readonly static PictureBox BackGroungImage = new PictureBox(PATH + "board10x10.png", 310, 50);
        private readonly static CircuitBoard CBoard = new CircuitBoard();
        private readonly static TextBox TBButton = new TextBox();
        private readonly static TextBlock TBButtonText = new TextBlock();
        private readonly static TextBox TBRelay = new TextBox();
        private readonly static TextBlock TBRelayText = new TextBlock();
        private readonly static TextBox TBTime = new TextBox() { Text = "5" };
        private readonly static TextBlock TBTimeText = new TextBlock();
        private readonly static TextBox TBLight = new TextBox();
        private readonly static TextBlock TBLightText = new TextBlock();
        private readonly static ToolbarButton BtnEdit = new ToolbarButton("Edit_nappi");
        private readonly static ToolbarButton BtnRun = new ToolbarButton("Run_nappi");
        private readonly static ToolbarButton BtnNew = new ToolbarButton("Uusi_nappi");
        private readonly static ToolbarButton BtnLoad = new ToolbarButton("Avaa_nappi");
        private readonly static ToolbarButton BtnSave = new ToolbarButton("Tallenna_nappi");
        private readonly static ToolbarButton BtnSaveAs = new ToolbarButton("Tall.nim_nappi");
        private readonly static ToolbarButton BtnInfo = new ToolbarButton("Info_nappi");
        private readonly static PictureBox LightEdit = new PictureBox(PATH + "led_on.png",93,52);
        private readonly static PictureBox LightRun = new PictureBox(PATH + "led_off.png",93,82);
        private readonly static TextBox CommentBox = new TextBox();
        private readonly static AboutBox1 Abox = new AboutBox1();

        private readonly static Label LblSize = new Label();
        private readonly static Button Btn10x10 = new Button();
        private readonly static Button Btn15x15 = new Button();

        //private readonly static InfoWindow InfoWin = new InfoWindow();

        public Workbench()
        {
            // Define own parameters
            this.Margin = new Thickness(0, 0, 0, 0);
            this.Background = new SolidColorBrush(Color.FromRgb(235,235,235));
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            
            // Define children
            this.Children.Add(BackGroungImage);
            this.Children.Add(CBoard);
            CBoard.Margin = new Thickness(360, 100, 0, 0);
            CBoard.Width = 500;
            CBoard.Height = 500;

            CursorTimer.Tick += OnCursorTimer;

            CreateButtons();
            CreateTextBoxes();
            CreateModeButtons();
            CreateFileButtons();
        }

        public void SetBoard(int size) // Size is 10 or 15
        {
            if (size == 10)
            {
                CBoard.Width = 500;
                CBoard.Height = 500;
                BackGroungImage.SetResourceImage("board10x10");
                Application.Current.MainWindow.Width = 950;
                Application.Current.MainWindow.Height = 700;
            }
            if (size == 15)
            {
                BackGroungImage.SetResourceImage("board15x15");
                CBoard.Width = 750;
                CBoard.Height = 750;
                Application.Current.MainWindow.Width = 1200;
                Application.Current.MainWindow.Height = 950;

                double sWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                double sHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                //Rect workArea = System.Windows.SystemParameters.WorkArea;
                Application.Current.MainWindow.Left = (sWidth / 2) - (1200 / 2);
                Application.Current.MainWindow.Top = (sHeight / 2) - (950 / 2);
                
            }
        }

        private void CreateFileButtons()
        {
            BtnNew.Margin = new Thickness(10, 2, 0, 0);
            this.Children.Add(BtnNew);
            BtnNew.Click += BtnNew_Click;

            BtnLoad.Margin = new Thickness(94, 2, 0, 0);
            this.Children.Add(BtnLoad);
            BtnLoad.Click += BtnLoad_Click;

            BtnSave.Margin = new Thickness(178, 2, 0, 0);
            this.Children.Add(BtnSave);
            BtnSave.Click += BtnSave_Click;

            BtnSaveAs.Margin = new Thickness(262, 2, 0, 0);
            this.Children.Add(BtnSaveAs);
            BtnSaveAs.Click += BtnSaveAs_Click;

            BtnInfo.Margin = new Thickness(346, 2, 0, 0);
            this.Children.Add(BtnInfo);
            BtnInfo.Click += BtnInfo_Click;

            LblSize.HorizontalContentAlignment = HorizontalAlignment.Left;
            LblSize.VerticalAlignment = VerticalAlignment.Top;
            LblSize.Margin = new Thickness(430, 0, 0, 0);
            LblSize.FontSize = 16;
            LblSize.Content = "Piirikaavion koko:";
            this.Children.Add(LblSize);

            Btn10x10.Margin = new Thickness(570, 2, 0, 0);
            Btn10x10.Height = 27;
            Btn10x10.Content = "10x10";
            
            this.Children.Add(Btn10x10);
            Btn10x10.Click += ChangeSize10x10;

            Btn15x15.Margin = new Thickness(610, 2, 0, 0);
            Btn15x15.Height = 27;
            Btn15x15.Content = "15x15";
            this.Children.Add(Btn15x15);
            Btn15x15.Click += ChangeSize15x15;

        }

        private void ChangeSize10x10(object sender, RoutedEventArgs e)
        {
            if (CBoard.GetBoardSize() == 15)
            {
                if (MessageBox.Show("Kaikki komponentit poistetaan. Haluatko jatkaa?", "Piirikaavio pienennetään!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SetEditMode();
                    CBoard.SetBoardSize(10);
                    CBoard.ClearCircuit();
                    CBoard.Children.Clear();
                    SetBoard(10);
                }
            }
        }

        private void ChangeSize15x15(object sender, RoutedEventArgs e)
        {
            if (CBoard.GetBoardSize() == 10)
            {
                if (MessageBox.Show("Komponentteja ei poisteta. Haluatko jatkaa?", "Piirikaaviota suurennetaan!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SetEditMode();
                    CBoard.SetBoardSize(15);
                    CBoard.FillWhenExpanded();
                    SetBoard(15);
                }
            }
        }

        //---------------------------------------------------------------------------------------------------------------------------
        // OPTIONS
        //---------------------------------------------------------------------------------------------------------------------------

        private void OnCursorTimer(Object sender, EventArgs e)
        {
            SaveTime -= 0.5;
            if (SaveTime <= 0)
            {
                Cursor = Cursors.Arrow;
                CursorTimer.Stop();
            }
        }
        
        //---------------------------------------------------------------------------------------------------------------------------
        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Haluatko jatkaa?", "Piirikaavio tyhjennetään!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                CBoard.ClearCircuit();
                SetEditMode();
                SavedFileName = "";
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode();
         
            OpenFileDialog OpenDlg = new OpenFileDialog { Filter = "rsf files(*.rsf) | *.rsf" };
            OpenDlg.ShowDialog();
            string OFile = OpenDlg.FileName;
            //OpenDlg.Dispose();
            if (OFile == "") { return; }
            CBoard.Children.Clear();
            SetBoard(CBoard.LoadBoard(OFile));
            SavedFileName = OFile;
            CommentBox.Text = CBoard.CommentText;
         }

        private void BtnSave_Click(object sender, RoutedEventArgs e) { SetEditMode(); SavaDataToFile(); }
        private void BtnSaveAs_Click(object sender, RoutedEventArgs e) { SetEditMode(); SaveAsDataToFile(); }
        
        private void BtnInfo_Click(object sender, RoutedEventArgs e) { Abox.Show(); }

        private void SavaDataToFile()
        {
            String SFile = SavedFileName;
            if (SFile == "") SaveAsDataToFile();
            else
            {
                SaveTime = 1;
                Cursor = Cursors.Wait;
                CursorTimer.Start();
                CBoard.SaveBoard(SFile);
            }
        }

        private void SaveAsDataToFile()
        {
            SaveFileDialog SaveDlg = new SaveFileDialog { DefaultExt = "rsf", Filter = "rsf files(*.rsf) | *.rsf" };
            SaveDlg.ShowDialog();
            string SFile = SaveDlg.FileName;
            if (SFile == "") return;
            //SaveDlg.Dispose();
            SavedFileName = SFile;
            CBoard.SaveBoard(SFile);
        }

        //---------------------------------------------------------------------------------------------
        private void CreateModeButtons()
        {
            BtnEdit.Margin = new Thickness(10, 50, 0, 0);
            this.Children.Add(BtnEdit);
            BtnEdit.Click += BtnEdit_Click;

            this.Children.Add(LightEdit);

            BtnRun.Margin = new Thickness(10, 80, 0, 0);
            this.Children.Add(BtnRun);
            BtnRun.Click += BtnRun_Click;

            this.Children.Add(LightRun);

            CBoard.SetMode(0);
        }

        private void CreateTextBoxes()
        {
            TBButtonText.Margin = new Thickness(20,310,0,0);
            TBButtonText.Text = "Painonapin tunnus: S";
            this.Children.Add(TBButtonText);

            TBButton.Margin = new Thickness(135, 310, 0, 0);
            TBButton.Width = 20;
            TBButton.Text = "0";
            this.Children.Add(TBButton);
            TBButton.TextChanged += TBButton_Changed;

            TBRelayText.Margin = new Thickness(20, 370, 0, 0);
            TBRelayText.Text = "Releen tunnus:        K";
            this.Children.Add(TBRelayText);

            TBRelay.Margin = new Thickness(135, 370, 0, 0);
            TBRelay.Width = 20;
            TBRelay.Text = "1";
            this.Children.Add(TBRelay);
            TBRelay.TextChanged += TBRelay_Changed;

            TBTimeText.Margin = new Thickness(20, 425, 0, 0);
            TBTimeText.Text = "Aikarele (sek):";
            this.Children.Add(TBTimeText);

            TBTime.Margin = new Thickness(100, 425, 0, 0);
            TBTime.Width = 20;
            this.Children.Add(TBTime);
            TBTime.TextChanged += TBTime_Changed;

            TBLightText.Margin = new Thickness(20,490,0,0);
            TBLightText.Text = "Lamppu:";
            this.Children.Add(TBLightText);

            TBLight.Margin = new Thickness(100, 490, 0, 0);
            TBLight.Width = 20;
            TBLight.Text = "1";
            this.Children.Add(TBLight);
            TBLight.TextChanged += TBLight_Changed;

            CommentBox.Margin = new Thickness(20,540,0,0);
            CommentBox.Width = 274;
            CommentBox.Height = 110;
            CommentBox.AcceptsReturn = true;
            CommentBox.AppendText("Kirjoita kommentti.");
            this.Children.Add(CommentBox);
            CommentBox.TextChanged += CommentBox_TextChanged;
            
        }

        private void CommentBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CBoard.CommentText = CommentBox.Text;
        }

        private void CreateButtons()
        {
            ComponentButton CB0 = new ComponentButton("line_uldr") { Margin = new Thickness(130, 50, 0, 0) };
            this.Children.Add(CB0);
            CB0.Click += CB0_Click;

            ComponentButton CB1 = new ComponentButton("line_ldr") { Margin = new Thickness(240, 50, 0, 0) };
            this.Children.Add(CB1);
            CB1.Click += CB1_Click;

            ComponentButton CB2 = new ComponentButton("line_ulr") { Margin = new Thickness(185, 105, 0, 0) };
            this.Children.Add(CB2);
            CB2.Click += CB2_Click;

            ComponentButton CB3 = new ComponentButton("line_udr") { Margin = new Thickness(185, 50, 0, 0) };
            this.Children.Add(CB3);
            CB3.Click += CB3_Click;

            ComponentButton CB4 = new ComponentButton("line_uld") { Margin = new Thickness(240, 105, 0, 0) };
            this.Children.Add(CB4);
            CB4.Click += CB4_Click;

            //------------------Corners---------------------------
            ComponentButton CB5 = new ComponentButton("line_dr") { Margin = new Thickness(185, 170, 0, 0) };
            this.Children.Add(CB5);
            CB5.Click += CB5_Click;

            ComponentButton CB6 = new ComponentButton("line_ur") { Margin = new Thickness(185, 225, 0, 0) };
            this.Children.Add(CB6);
            CB6.Click += CB6_Click;

            ComponentButton CB7 = new ComponentButton("line_ld") { Margin = new Thickness(240, 170, 0, 0) };
            this.Children.Add(CB7);
            CB7.Click += CB7_Click;

            ComponentButton CB8 = new ComponentButton("line_ul") { Margin = new Thickness(240, 225, 0, 0) };
            this.Children.Add(CB8);
            CB8.Click += CB8_Click;

            //------------------Straght lines---------------------------
            ComponentButton CB9 = new ComponentButton("line_ud") { Margin = new Thickness(130, 170, 0, 0) };
            this.Children.Add(CB9);
            CB9.Click += CB9_Click;

            ComponentButton CB10 = new ComponentButton("line_lr") { Margin = new Thickness(130, 225, 0, 0) };
            this.Children.Add(CB10);
            CB10.Click += CB10_Click;

            //---------------------Buttons-------------------------------
            ComponentButton CB11 = new ComponentButton("btn_NC_c") { Margin = new Thickness(185, 290, 0, 0) };
            this.Children.Add(CB11);
            CB11.Click += CB11_Click;

            ComponentButton CB12 = new ComponentButton("btn_NO_o") { Margin = new Thickness(240, 290, 0, 0) };
            this.Children.Add(CB12);
            CB12.Click += CB12_Click;

            //---------------------Contacts-------------------------------
            ComponentButton CB13 = new ComponentButton("contact_NC_c") { Margin = new Thickness(185, 355, 0, 0) };
            this.Children.Add(CB13);
            CB13.Click += CB13_Click;

            ComponentButton CB14 = new ComponentButton("contact_NO_o") { Margin = new Thickness(240, 355, 0, 0) };
            this.Children.Add(CB14);
            CB14.Click += CB14_Click;

            //---------------------Coils-------------------------------
            ComponentButton CB15 = new ComponentButton("coil_norm") { ToolTip = "Normaali rele", Margin = new Thickness(130, 410, 0, 0) };
            this.Children.Add(CB15);
            CB15.Click += CB15_Click;

            ComponentButton CB16 = new ComponentButton("coil_ontimer") { ToolTip = "Vetohidastettu rele", Margin = new Thickness(185, 410, 0, 0) };
            this.Children.Add(CB16);
            CB16.Click += CB16_Click;

            ComponentButton CB17 = new ComponentButton("coil_offtimer") { ToolTip = "Päästöhidastettu rele", Margin = new Thickness(240, 410, 0, 0) };
            this.Children.Add(CB17);
            CB17.Click += CB17_Click;
            
            //---------------------Lights-------------------------------
            ComponentButton CB18 = new ComponentButton("light_g_off") { ToolTip = "Vihreä lamppu", Margin = new Thickness(130, 475, 0, 0) };
            this.Children.Add(CB18);
            CB18.Click += CB18_Click;

            ComponentButton CB19 = new ComponentButton("light_y_off") { ToolTip = "Keltainen lamppu", Margin = new Thickness(185, 475, 0, 0) };
            this.Children.Add(CB19);
            CB19.Click += CB19_Click;

            ComponentButton CB20 = new ComponentButton("light_r_off") { ToolTip = "Punainen lamppu", Margin = new Thickness(240, 475, 0, 0) };
            this.Children.Add(CB20);
            CB20.Click += CB20_Click;
        }

        private void CB0_Click(object sender, RoutedEventArgs e) { CBoard.SetComponent("line_uldr"); }
        private void CB1_Click(object sender, RoutedEventArgs e) { CBoard.SetComponent("line_ldr"); }
        private void CB2_Click(object sender, RoutedEventArgs e) { CBoard.SetComponent("line_ulr"); }
        private void CB3_Click(object sender, RoutedEventArgs e) { CBoard.SetComponent("line_udr"); }
        private void CB4_Click(object sender, RoutedEventArgs e) { CBoard.SetComponent("line_uld"); }
        // Corners
        private void CB5_Click(object sender, RoutedEventArgs e) { CBoard.SetComponent("line_dr"); }
        private void CB6_Click(object sender, RoutedEventArgs e) { CBoard.SetComponent("line_ur"); }
        private void CB7_Click(object sender, RoutedEventArgs e) { CBoard.SetComponent("line_ld"); }
        private void CB8_Click(object sender, RoutedEventArgs e) { CBoard.SetComponent("line_ul"); }
        // Straigth lines
        private void CB9_Click(object sender, RoutedEventArgs e) { CBoard.SetComponent("line_ud"); }
        private void CB10_Click(object sender, RoutedEventArgs e) { CBoard.SetComponent("line_lr"); }
        // Buttons
        private void CB11_Click(object sender, RoutedEventArgs e) { CBoard.SetComponentBtn("btn_NC_c", TBButton.Text); }
        private void CB12_Click(object sender, RoutedEventArgs e) { CBoard.SetComponentBtn("btn_NO_o", TBButton.Text); }
        // Contacts
        private void CB13_Click(object sender, RoutedEventArgs e) { CBoard.SetComponentContact("contact_NC_c", TBRelay.Text); }
        private void CB14_Click(object sender, RoutedEventArgs e) { CBoard.SetComponentContact("contact_NO_o", TBRelay.Text); }
        // Coil
        private void CB15_Click(object sender, RoutedEventArgs e) { CBoard.SetComponentCoil("coil_norm", TBRelay.Text, Convert.ToDouble(TBTime.Text)); }
        private void CB16_Click(object sender, RoutedEventArgs e) { CBoard.SetComponentCoil("coil_ontimer", TBRelay.Text, Convert.ToDouble(TBTime.Text)); }
        private void CB17_Click(object sender, RoutedEventArgs e) { CBoard.SetComponentCoil("coil_offtimer", TBRelay.Text, Convert.ToDouble(TBTime.Text)); }
        // Light
        private void CB18_Click(object sender, RoutedEventArgs e) { CBoard.SetComponentLight("light_g_off", TBLight.Text); }
        private void CB19_Click(object sender, RoutedEventArgs e) { CBoard.SetComponentLight("light_y_off", TBLight.Text); }
        private void CB20_Click(object sender, RoutedEventArgs e) { CBoard.SetComponentLight("light_r_off", TBLight.Text); }

        // Mode buttons
        private void BtnEdit_Click(object sender, RoutedEventArgs e) { SetEditMode();}

        private void BtnRun_Click(object sender, RoutedEventArgs e) { SetRunMode(); }

        private void TBButton_Changed(object sender, TextChangedEventArgs e)
        {
            try
            {
                int temp = Convert.ToInt16(TBButton.Text);
                if (temp < 0 || temp > 9) { MessageBox.Show("Lukuarvon pitää olla väliltä 0-9.", "Virheellinen Syöte!"); temp = 0; TBButton.Text = "0"; }
                CBoard.SetButtonID(temp);
            }
            catch { MessageBox.Show("Kenttään kelpaa vain lukuarvot", "Virheellinen Syöte!"); TBButton.Text = "0"; }
        }
        private void TBRelay_Changed(object sender, TextChangedEventArgs e)
        {
            try
            {
                int temp = Convert.ToInt16(TBRelay.Text);
                if (temp < 1 || temp > 9) { MessageBox.Show("Lukuarvon pitää olla väliltä 1-9.", "Virheellinen Syöte!"); temp = CBoard.FreeRelay; TBRelay.Text = Convert.ToString(CBoard.FreeRelay); }
                CBoard.SetRelayID(temp);
            }
            catch { MessageBox.Show("Kenttään kelpaa vain lukuarvot", "Virheellinen Syöte!"); TBRelay.Text = Convert.ToString(CBoard.FreeRelay); }
        }
        private void TBTime_Changed(object sender, TextChangedEventArgs e)
        {
            try { CBoard.SetRelayTime(Convert.ToDouble(TBTime.Text));}
            catch { MessageBox.Show("Kenttään kelpaa vain lukuarvot", "Virheellinen Syöte!"); TBTime.Text = "1";}
        }

        private void TBLight_Changed(object sender, TextChangedEventArgs e)
        {
            try
            {
                int temp = Convert.ToInt16(TBLight.Text);
                if (temp < 1 || temp > 9) { MessageBox.Show("Lukuarvon pitää olla väliltä 1-9.", "Virheellinen Syöte!"); temp = 1; TBLight.Text = "1"; }
                CBoard.SetLightID(temp);
            }
            catch { MessageBox.Show("Kenttään kelpaa vain lukuarvot", "Virheellinen Syöte!"); TBLight.Text = Convert.ToString(CBoard.FreeLight); }
        }

        private void SetEditMode()
        {
            CBoard.SetMode(0);
            LightEdit.SetResourceImage("led_on");
            LightRun.SetResourceImage("led_off");
            TBButton.IsEnabled = true;
            TBRelay.IsEnabled = true;
            TBTime.IsEnabled = true;
        }

        private void SetRunMode()
        {
            CBoard.SetMode(1);
            LightEdit.SetResourceImage("led_off");
            LightRun.SetResourceImage("led_on");
            TBButton.IsEnabled = false;
            TBRelay.IsEnabled = false;
            TBTime.IsEnabled = false;
        }
    }
}