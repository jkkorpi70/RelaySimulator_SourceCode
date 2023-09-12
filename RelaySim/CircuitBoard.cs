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
using System.Windows.Threading;

namespace RelaySim
{

    class CircuitBoard : Canvas
    {
        private readonly string PATH = "Resources/";
        private readonly static CircuitComponent[,] Component = new CircuitComponent[15, 15];
        private readonly static PictureBox[,] IDLabel = new PictureBox[15, 15];
        private string CurrentComponent;
        //private string CurrentID;
        private string ButtonID = "S0";
        private string RelayID = "K1";
        private string LightID = "H1";
        private double CurrentTime;
        private int Mode = 0; // 0 = editor mode, 1 = run mode

        private int TableSize = 10; // Table size (x and y) for circuit
        private int MaxX = 10; // Maximum X value for circuit running
        private int NLineInPos = 9;

        private readonly static DispatcherTimer Tim1 = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
        private readonly static DispatcherTimer TimRelay = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        
        private int TimeRelaysOn = 0; // Number of runnig time relays
        private readonly static ComponentInfo[] Relays = new ComponentInfo[10];
        private readonly static TextBlock[] CountDownBlock = new TextBlock[10];
        private readonly static TextBlock[] TimeBlock = new TextBlock[10];
        private readonly static ComponentInfo[] Lights = new ComponentInfo[10];
        private readonly static TextBlock InfoBlock = new TextBlock();

        public int FreeRelay { get; set; } 
        public int FreeLight { get; set; }
        public string CommentText { get; set; }

        public CircuitBoard()
        {
            this.Background = new SolidColorBrush(Colors.White);
            this.VerticalAlignment = VerticalAlignment.Top;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            CurrentComponent = "line_ldr";
            FreeRelay = 1;
            Tim1.Tick += OnTimer;
            TimRelay.Tick += OnRelayTimer;
            for (int i = 0; i < 10; i++)
            {
                Relays[i] = new ComponentInfo("K" + Convert.ToString(i)); 
                Lights[i] = new ComponentInfo("H" + Convert.ToString(i));
                CountDownBlock[i] = new TextBlock();// { Width = 30 };
                TimeBlock[i] = new TextBlock();
            }
            InfoBlock.Margin = new Thickness(0,-20,0,0);
            this.Children.Add(InfoBlock);
            FillWithEmpty();

            // Mouse events
            this.MouseLeftButtonDown += MouseLeftClick;
            this.MouseLeftButtonDown += MouseLeftDown;
            this.MouseLeftButtonUp += MouseLeftUp;
            this.MouseRightButtonDown += MouseRightClick;
            this.MouseMove += MouseMoving;
        }

        
        ~CircuitBoard() { }

        //------------------------------------------------------------------------------------------------------------
        public void SetBoardSize(int size)
        {
            if (size == 10)
            {
                TableSize = 10;
                MaxX = 10;
                NLineInPos = 9;
            }
            if (size == 15)
            {
                TableSize = 15;
                MaxX = 15;
                NLineInPos = 14;
            }
        }

        public int GetBoardSize() { return TableSize; }

        //------------------------------------------------------------------------------------------------------------
        public void ClearCircuit() { FillWithEmpty(); }

        // Fill Circuitboard with empty component (create all component as empty)
        private void FillWithEmpty()
        {
            for (int x = 0; x < TableSize; x++)
            {
                for (int y = 0; y < TableSize; y++)
                {
                    Component[x, y] = new CircuitComponent(0, 0, 0, 0)
                    {
                        IsConnectedPositive = 0,
                        IsConnectedNegative = 0,
                        ComponentFilelName = "line_empty",
                        Margin = new Thickness(x * 50, y * 50, 0, 0)
                    };
                    this.Children.Add(Component[x, y]);
                }
            }
            for (int j = 1; j < 10; j++) Relays[j].IsOnBoard = 0;
        }

        //------------------------------------------------------------------------------
        // Fill new exposed area with empty block
        //------------------------------------------------------------------------------
        public void FillWhenExpanded()
        {
            for (int x = 10; x < 15; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    Component[x, y] = new CircuitComponent(0, 0, 0, 0)
                    {
                        IsConnectedPositive = 0,
                        IsConnectedNegative = 0,
                        ComponentFilelName = "line_empty",
                        Margin = new Thickness(x * 50, y * 50, 0, 0)
                    };
                    this.Children.Add(Component[x, y]);
                }
            }
            for (int x = 0; x < 15; x++)
            {
                for (int y = 10; y < 15; y++)
                {
                    Component[x, y] = new CircuitComponent(0, 0, 0, 0)
                    {
                        IsConnectedPositive = 0,
                        IsConnectedNegative = 0,
                        ComponentFilelName = "line_empty",
                        Margin = new Thickness(x * 50, y * 50, 0, 0)
                    };
                    this.Children.Add(Component[x, y]);
                }
            }
        }
   
        //--------------------------------------------------------------------------------------------
        public void SetMode(int mode)
        {
            Mode = mode;
            if (Mode == 1) { RunCircuit(); return; }

            else { Tim1.Stop(); TimRelay.Stop(); ResetCircuit(); }
        }

        public void SetComponent(string newImage)
        {
            CurrentComponent = newImage;
        }

        public void SetButtonID(int id) { ButtonID = "S" + id; }
        public void SetRelayID(int id) { RelayID = "K" + id; }
        public void SetLightID(int id) { LightID = "H" + id; }

        public void SetComponentBtn(string newImage, string newID)
        {
            CurrentComponent = newImage;
            ButtonID = "S" + newID;
        }

        public void SetComponentContact(string newImage, string newID)
        {
            CurrentComponent = newImage;
            RelayID = "K" + newID;
        }

        public void SetComponentLight(string newImage, string newID)
        {
            CurrentComponent = newImage;
            LightID = "H" + newID;
        }

        public void SetComponentCoil(string newImage, string newID, double time)
        {
            CurrentComponent = newImage;
            RelayID = "K" + newID;
            CurrentTime = time;
        }

        public void SetRelayTime(double time) { CurrentTime = time; }

        //========================================================================================================
        //                               MOUSE FUNCTIONS
        //========================================================================================================
        private void MouseLeftClick(object sender, MouseButtonEventArgs e)
        {
            if (Mode == 1) { ToggleComponentState(sender, e); return; } // Run mode
            //AddComponent(sender, e); // edit mode
        }

        private void MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (Mode == 1) return;
            AddComponent(sender, e); // edit mode
        }

        private void MouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            if (Mode == 0) return; // edit mode - do nothing
            ResetButtonState(sender, e);
        }

        private void MouseRightClick(object sender, MouseButtonEventArgs e)
        {
            if (Mode == 1) return; // Run mode -> exit
            RemoveComponent(sender, e);
            string temp = CurrentComponent;
            CurrentComponent = "line_empty";
            AddComponent(sender, e);
            CurrentComponent = temp;
        }
        
        private void MouseMoving(object sender, MouseEventArgs e)
        {
            if (Mode == 1) return; // Run mode -> exit
            System.Windows.Point Pnt = e.GetPosition(this);
            double X = Pnt.X;
            double Y = Pnt.Y;
            int X1 = (int)Math.Floor(X / 50);
            int Y1 = (int)Math.Floor(Y / 50);
            if (Component[X1, Y1].ComponentFilelName.Contains("ontimer") || Component[X1, Y1].ComponentFilelName.Contains("offtimer"))
            {
                this.Children.Remove(InfoBlock);
                InfoBlock.Text = "Aikarele " + Component[X1, Y1].ComponentID + " : " + Convert.ToString(Component[X1, Y1].DelayTime) + " sek";
                this.Children.Add(InfoBlock);
            }
            else InfoBlock.Text = "";
        }
        
        //========================================================================================================
        //                                      EDIT MODE FUNCTIONS
        //========================================================================================================

        private void AddComponent(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point Pnt = e.GetPosition(this);
            double X = Pnt.X;
            double Y = Pnt.Y;
            int X1 = (int)Math.Floor(X / 50);
            int Y1 = (int)Math.Floor(Y / 50);
            AddComponent(X1, Y1);
        }

        private void AddComponent(int X1, int Y1)
        {
            int ReleEhto = 1; // If trying to make new relay with same ID and cancel action ReleEhto = 0;

            if (X1 < TableSize && Y1 < TableSize)
            {
                string CName = ComponentName(CurrentComponent);
                string CType = ComponentType(CurrentComponent);
                if (CName == "line") CreateLine(CType, X1, Y1);
                if (CName == "btn") CreateButton(CType, X1, Y1);
                if (CName == "contact") CreateContact(CType, X1, Y1);
                if (CName == "coil") ReleEhto = CreateCoil(CType, X1, Y1);
                if (CName == "light") CreateLight(CType, X1, Y1);

                if (ReleEhto == 0) return;

                Component[X1, Y1].ComponentFilelName = CurrentComponent;
                Component[X1, Y1].Margin = new Thickness(X1 * 50, Y1 * 50, 0, 0);
                this.Children.Add(Component[X1, Y1]);

                for (int j = 1; j < 10; j++) { if (Relays[j].IsOnBoard == 0) { FreeRelay = j; break; } }

                // Adding Id's. eg. S0, S1, K1...
                if (CName != "line")
                {
                    if (CName == "coil") IDLabel[X1, Y1] = new PictureBox(PATH + RelayID + ".png", X1 * 50, Y1 * 50 + 20);
                    else if (CName == "btn") IDLabel[X1, Y1] = new PictureBox(PATH + ButtonID + ".png", X1 * 50, Y1 * 50 + 5);
                    else if (CName == "light") IDLabel[X1, Y1] = new PictureBox(PATH + LightID + ".png", X1 * 50 + 5, Y1 * 50 + 5);
                    else IDLabel[X1, Y1] = new PictureBox(PATH + RelayID + ".png", X1 * 50 + 5, Y1 * 50 + 20); // For contact
                    this.Children.Add(IDLabel[X1, Y1]);
                }
            }
        }

        private void RemoveComponent(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point Pnt = e.GetPosition(this);
            double X = Pnt.X;
            double Y = Pnt.Y;
            int X1 = (int)Math.Floor(X / 50);
            int Y1 = (int)Math.Floor(Y / 50);
            RemoveComponent(X1, Y1);
        }

        private void RemoveComponent(int X, int Y)
        { 
            if (Component[X, Y].Component.EndsWith("coil") == true)
            {
                int i = Convert.ToInt16(Component[X,Y].ComponentID.Remove(0, 1));
                Relays[i].IsOnBoard = 0;
            }

            if (Component[X, Y].Component.EndsWith("light") == true)
            {
                int i = Convert.ToInt16(Component[X, Y].ComponentID.Remove(0, 1));
                Lights[i].IsOnBoard = 0;
            }
        }

        private void CreateLine(string type, int x, int y)
        {
            RemoveComponent(x, y);
            int u = 0;
            int l = 0;
            int d = 0;
            int r = 0;
            if (type.Contains('u') == true) u = 1;
            if (type.Contains('l') == true) l = 1;
            if (type.Contains('d') == true) d = 1;
            if (type.Contains('r') == true) r = 1;
            Component[x, y] = new CircuitComponent(u, l, d, r);
        }

        private void CreateButton(string type, int x, int y)
        {
            RemoveComponent(x, y);
            int t = 0;
            if (type == "NC") t = 1;
            Component[x, y] = new CircuitComponent("button", t, ButtonID);
        }

        private void CreateContact(string type, int x, int y)
        {
            RemoveComponent(x, y);
            int t = 0;
            if (type == "NC") t = 1;
            Component[x, y] = new CircuitComponent("contact", t, RelayID);
        }

        private int CreateCoil(string type, int x, int y)
        {
            double time = CurrentTime;
            if (type == "norm") time = 0;
            int i = Convert.ToInt16(RelayID.Remove(0, 1));
            // Search in table if current ID already exists
            int Ehto = 1;

            if (Relays[i].IsOnBoard == 1)
            {
                int OldX = Relays[i].X;
                int OldY = Relays[i].Y;
                if (MessageBox.Show("Korvataanko uudella komponentilla", "Rele " +  RelayID + " on jo olemassa.", MessageBoxButton.YesNo) == MessageBoxResult.No) Ehto = 0;
                if (Ehto == 1)
                {
                    string temp = CurrentComponent;
                    CurrentComponent = "line_empty";
                    AddComponent(OldX, OldY);
                    Component[OldX, OldY].SetResourceImage("line_empty");
                    CurrentComponent = temp;
                }
                for (int j = 1; j < 10; j++) { if (Relays[j].IsOnBoard == 0) FreeRelay = j; break; }
            }

            if (Ehto == 1)
            {
                RemoveComponent(x, y); // Remove possible old relay in this position
                Component[x, y] = new CircuitComponent(time, RelayID, type);
                Relays[i].ComponentID = RelayID;
                Relays[i].DelayTime = (int)time;
                Relays[i].ComponentType = type;
                Relays[i].IsOnBoard = 1;
                Relays[i].X = x;
                Relays[i].Y = y;
                /*
                if (type == "ontimer" || type == "offtimer")
                {
                    TimeBlock[i].Text = Convert.ToString(time);
                    TimeBlock[i].Margin = new Thickness(x * 50, y * 50,0,0);
                    this.Children.Add(TimeBlock[i]);
                }*/
            }
            return Ehto;
        }

        private void CreateLight(string type, int x, int y)
        {
            RemoveComponent(x, y);
            int i = Convert.ToInt16(LightID.Remove(0, 1));
            // First delete old light with same index/ID
            if (Lights[i].IsOnBoard == 1)
            {
                int OldX = Lights[i].X;
                int OldY = Lights[i].Y;
                string temp = CurrentComponent;
                CurrentComponent = "line_empty";
                AddComponent(OldX, OldY);
                Component[OldX, OldY].SetResourceImage("line_empty");
                CurrentComponent = temp;
            }
            // Then create new light
            Component[x, y] = new CircuitComponent(LightID, type);
            Lights[i].ComponentID = LightID;
            Lights[i].ComponentType = type;
            Lights[i].IsOnBoard = 1;
            Lights[i].X = x;
            Lights[i].Y = y;
            for (int j = 1; j < 10; j++) { if (Lights[j].IsOnBoard == 0) FreeLight = j; break; }
        }

        private string ComponentName(string component)
        {
            string[] SubStrings = component.Split('_');
            return SubStrings[0];
        }

        private string ComponentType(string component)
        {
            string[] SubStrings = component.Split('_');
            return SubStrings[1];
        }

        //========================================================================================================
        //                                      RUN MODE FUNCTIONS
        //========================================================================================================

        private void RunCircuit()
        {
            if (Component[0, 0].IsConnectedLeft != 1) { MessageBox.Show("L / + ei ole kytketty"); return; }
            if (Component[0, NLineInPos].IsConnectedLeft != 1) { MessageBox.Show("N / - ei ole kytketty"); return; }
            MaxX = DetermineMaxXValue();
            Tim1.Start();
        }

        private int DetermineMaxXValue()
        {
            int x; int y; int Empty;
            for (x = 0; x < TableSize; x++)
            {
                Empty = 0;
                for (y = 0; y < TableSize; y++)
                {
                    if (Component[x, y].ComponentFilelName != "line_empty") Empty++;
                }
                if (Empty == 0) return x+1;
            }
            return TableSize;
        }

        // -------------------------------------------- TIMERS --------------------------------------------------------
        private void OnTimer(Object sender, EventArgs e)
        {
            ScanBoard();
            ReadRelays();
            ToggleLights();
        }

        private void OnRelayTimer(Object sender, EventArgs e)
        {
            for (int i = 1; i < 10; i++)
            {
                if (Relays[i].TimeActivated == 1)
                {
                    TimeSpan ElapsedTime = DateTime.Now - Relays[i].StartTime;
                    CountDownBlock[i].Text = Convert.ToString(Relays[i].DelayTime - ElapsedTime.Seconds);
                    if (ElapsedTime.Seconds >= Relays[i].DelayTime)
                    {
                        Relays[i].TimeActivated = 0;
                        Relays[i].CountdownFinished = 1;
                        TimeRelaysOn -= 1;
                        this.Children.Remove(CountDownBlock[i]);
                    }

                }
            }
            if (TimeRelaysOn <= 0) { TimeRelaysOn = 0; TimRelay.Stop(); }
        }
        //-------------------------------------------------------------------------------------------------------------

        private void ScanBoard()
        {
            ClearConnections();
            TracePosLine(0, 0, 1);
            TraceNegLine(0, NLineInPos, 1);
        }

        private void ClearConnections()
        {
            int x;
            int y;
            for (y = 0; y < TableSize; y++)
            {
                for ( x = 0; x < MaxX; x++)
                {
                    Component[x, y].IsConnectedPositive = 0;
                    AddOnLineL(x, y, 0);
                    Component[x, y].IsConnectedNegative = 0;
                    AddOnLineN(x, y, 0);
                }
            }
        }

        // Component x, y and direction entered from (0,1,2,3) up, left, down, right
        private void TracePosLine(int x, int y, int entered)
        {
            Component[x, y].IsConnectedPositive = 1;
            if (Component[x, y].IsConductive != 1) return;
            AddOnLineL(x, y, 1);
            if (Component[x, y].Component.Contains("coil") || Component[x, y].Component.Contains("light")) return;

            if (Component[x, y].IsConnectedLeft == 1
                && x > 0
                && entered != 1
                && Component[x - 1, y].IsConnectedRight == 1
                && Component[x - 1, y].IsConnectedPositive != 1)
            { TracePosLine(x - 1, y, 3); }

            if (Component[x, y].IsConnectedDown == 1
                && y < TableSize - 1
                && entered != 2
                && Component[x, y + 1].IsConnectedUp == 1
                && Component[x, y + 1].IsConnectedPositive != 1)
            { TracePosLine(x, y + 1, 0); }

            if (Component[x, y].IsConnectedRight == 1
                && x < TableSize - 1
                && entered != 3
                && Component[x + 1, y].IsConnectedLeft == 1
                && Component[x + 1, y].IsConnectedPositive != 1)
            { TracePosLine(x + 1, y, 1); }

            if (Component[x, y].IsConnectedUp == 1
                && y > 0
                && entered != 0
                && Component[x, y - 1].IsConnectedDown == 1
                && Component[x, y - 1].IsConnectedPositive != 1)
            { TracePosLine(x, y - 1, 2); }
        }

        private void TraceNegLine(int x, int y, int entered)
        {
            Component[x, y].IsConnectedNegative = 1;
            AddOnLineN(x, y, 1);
            if (Component[x, y].IsConductive != 1) return;
            if (Component[x, y].Component.Contains("coil") || Component[x, y].Component.Contains("light")) return;

            if (Component[x, y].IsConnectedLeft == 1
                && x > 0
                && entered != 1
                && Component[x - 1, y].IsConnectedRight == 1
                && Component[x - 1, y].IsConnectedNegative != 1)
            { TraceNegLine(x - 1, y, 3); }

            if (Component[x, y].IsConnectedDown == 1
                && y < TableSize - 1
                && entered != 2
                && Component[x, y + 1].IsConnectedUp == 1
                && Component[x, y + 1].IsConnectedNegative != 1)
            { TraceNegLine(x, y + 1, 0); }

            if (Component[x, y].IsConnectedRight == 1
                && x < TableSize - 1
                && entered != 3
                && Component[x + 1, y].IsConnectedLeft == 1
                && Component[x + 1, y].IsConnectedNegative != 1)
            { TraceNegLine(x + 1, y, 1); }

            if (Component[x, y].IsConnectedUp == 1
                && y > 0
                && entered != 0
                && Component[x, y - 1].IsConnectedDown == 1
                && Component[x, y - 1].IsConnectedNegative != 1)
            { TraceNegLine(x, y - 1, 2); }
        }

        private void AddOnLineL(int x, int y, int OnOff)
        {
            string CompFile = Component[x, y].ComponentFilelName;
            string CompName = Component[x, y].Component;
            int conductive = Component[x, y].IsConductive;
            int InPos = Component[x, y].IsConnectedPositive;

            if (CompName == "wire")
            {
                if (OnOff == 0 && CompFile.EndsWith("_r") == true) CompFile.Replace("_r", "");
                if (OnOff == 1 && CompFile.EndsWith("_r") == false) CompFile += "_r";
                Component[x, y].SetResourceImage(CompFile); // Do not change to .ComponentFileName()
            }

            if (CompName == "buttonNC")
            {
                if (OnOff == 1 && conductive == 1 && InPos == 1 && CompFile != "btn_NC_o") Component[x, y].ComponentFilelName = "btn_NC_c_r";  // Must be for init state image 
                if (OnOff == 0 && CompFile != "btn_NC_o") Component[x, y].ComponentFilelName = "btn_NC_c"; // this for clearing circuit
            }

            if (CompName == "contactNO")
            {
                if (OnOff == 0 && InPos == 1 && conductive == 1) Component[x, y].ComponentFilelName = "contact_NO_c";  // Remove red from closed not connected in positive 
            }
        }

        private void AddOnLineN(int x, int y, int OnOff)
        {
            string CompFile = Component[x, y].ComponentFilelName;
            string CompName = Component[x, y].Component;

            if (CompName == "wire")
            {
                if (OnOff == 0 && CompFile.EndsWith("_b") == true) CompFile.Replace("_b", "");
                if (OnOff == 1 && CompFile.EndsWith("_b") == false) CompFile += "_b";
                Component[x, y].SetResourceImage(CompFile);
            }
        }

        //===================================================================================================
        // BUTTON FUNCTIONS - RUN MODE
        //===================================================================================================
        private void ToggleComponentState(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point Pnt = e.GetPosition(this);
            double X = Pnt.X;
            double Y = Pnt.Y;

            int X1 = (int)Math.Floor(X / 50);
            int Y1 = (int)Math.Floor(Y / 50);

            if (Component[X1, Y1].Component == "buttonNO") ButtonNOClose(X1, Y1);
            if (Component[X1, Y1].Component == "buttonNC") ButtonNCOpen(X1, Y1);

            string id = Component[X1, Y1].ComponentID;
            for (int y = 1; y < TableSize - 1; y++) // Start from line 2 (there should be no buttons on upmost line. Neither bottom)
            {
                for (int x = 0; x < MaxX; x++)
                {
                    if (Component[x, y].ComponentID == id && ( x != X1 || y != Y1))
                    {
                        if (Component[x, y].Component == "buttonNO") ButtonNOClose(x, y);
                        if (Component[x, y].Component == "buttonNC") ButtonNCOpen(x, y);
                    }
                }
            }
        }

        private void ResetButtonState(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point Pnt = e.GetPosition(this);
            double X = Pnt.X;
            double Y = Pnt.Y;

            int X1 = (int)Math.Floor(X / 50);
            int Y1 = (int)Math.Floor(Y / 50);

            if (Component[X1, Y1].Component == "buttonNO") ButtonNOOpen(X1, Y1);
            if (Component[X1, Y1].Component == "buttonNC") ButtonNCClose(X1, Y1);

            string id = Component[X1, Y1].ComponentID;
            for (int y = 1; y < TableSize - 1; y++) // Start from line 2 (there should be no buttons on upmost line. Neither bottom)
            {
                for (int x = 0; x < MaxX; x++)
                {
                    if (Component[x, y].ComponentID == id &&  (x != X1 || y != Y1))
                    {
                        if (Component[x, y].Component == "buttonNO") ButtonNOOpen(x, y);
                        if (Component[x, y].Component == "buttonNC") ButtonNCClose(x, y);
                    }
                }
            }
        }

        private void ButtonNOClose(int x, int y)
        {
            if (Component[x, y].IsConnectedPositive == 1) Component[x, y].ComponentFilelName = "btn_NO_c_r";
            else Component[x, y].ComponentFilelName = "btn_NO_c";
            Component[x, y].IsConductive = 1;
        }

        private void ButtonNOOpen(int x, int y)
        {
            Component[x, y].ComponentFilelName = "btn_NO_o";
            Component[x, y].IsConductive = 0;
        }

        private void ButtonNCClose(int x, int y)
        {
            if (Component[x, y].IsConnectedPositive == 1) Component[x, y].ComponentFilelName = "btn_NC_c_r";
            else Component[x, y].ComponentFilelName = "btn_NC_c";
            Component[x, y].IsConductive = 1;
        }

        private void ButtonNCOpen(int x, int y)
        {
            Component[x, y].ComponentFilelName = "btn_NC_o";
            Component[x, y].IsConductive = 0;
        }

        //===================================================================================================
        // RELAY FUNCTIONS - RUN MODE
        //===================================================================================================
        private void ReadRelays() 
        {
            int x;  
            int y;

            for (int i = 1; i < 10; i++)
            {
                x = Relays[i].X;
                y = Relays[i].Y;
                if (Relays[i].IsOnBoard == 1)
                {
                    // NORMAL RELAY
                    if (Component[x, y].Component == "relaycoil")
                    {
                        if (Component[x, y].IsConnectedNegative == 1 && Component[x, y].IsConnectedPositive == 1)
                        {
                            SetContacts(Component[x, y].ComponentID);
                            Component[x, y].ComponentFilelName = "coil_norm_r";
                        }
                        if (Component[x, y].IsConnectedNegative == 0 || Component[x, y].IsConnectedPositive == 0)
                        {
                            ResetContacts(Component[x, y].ComponentID);
                            Component[x, y].ComponentFilelName = "coil_norm";
                        }
                    }
                    // ONTIMER RELAY
                    if (Component[x, y].Component == "ontimercoil")
                    {
                        if (Component[x, y].IsConnectedNegative == 1 && Component[x, y].IsConnectedPositive == 1 && Relays[i].TimeActivated == 0 && Relays[i].TimeReseted == 1)
                        {
                            ResetContacts(Component[x, y].ComponentID);
                            Component[x, y].ComponentFilelName = "coil_ontimer_off";
                            Relays[i].RunningTime = Component[x, y].DelayTime;
                            Relays[i].TimeActivated = 1;
                            Relays[i].TimeReseted = 0;
                            TimeRelaysOn += 1;
                            CountDownBlock[i].Margin = new Thickness(x * 50 + 35, y * 50 + 30, 0, 0);
                            this.Children.Add(CountDownBlock[i]);
                            Relays[i].StartTime = DateTime.Now;
                            TimRelay.Start();
                        }
                        if (Relays[i].CountdownFinished == 1)
                        {
                            Component[x, y].ComponentFilelName = "coil_ontimer_r";
                            Relays[i].TimeActivated = 0;
                            SetContacts(Component[x, y].ComponentID);
                        }
                        if (Component[x, y].IsConnectedNegative == 0 || Component[x, y].IsConnectedPositive == 0)
                        {
                            ResetContacts(Component[x, y].ComponentID);
                            Component[x, y].ComponentFilelName = "coil_ontimer";
                            if (Relays[i].TimeActivated == 1) this.Children.Remove(CountDownBlock[i]);
                            Relays[i].CountdownFinished = 0;
                            Relays[i].TimeActivated = 0;
                            Relays[i].TimeReseted = 1;
                        }
                    }

                    // OFFTIMER RELAY
                    if (Component[x, y].Component == "offtimercoil")
                    {
                        if (Component[x, y].IsConnectedNegative == 1 && Component[x, y].IsConnectedPositive == 1 && Relays[i].TimeActivated == 0 && Relays[i].TimeReseted == 1)
                        {
                            SetContacts(Component[x, y].ComponentID);
                            Component[x, y].ComponentFilelName = "coil_offtimer_r";
                            Relays[i].RunningTime = Component[x, y].DelayTime;
                            Relays[i].TimeActivated = 1;
                            Relays[i].TimeReseted = 0;
                            TimeRelaysOn += 1;
                            CountDownBlock[i].Margin = new Thickness(x * 50 + 35, y * 50 + 30, 0, 0);
                            this.Children.Add(CountDownBlock[i]);
                            Relays[i].StartTime = DateTime.Now;
                            TimRelay.Start();
                        }
                        if (Relays[i].CountdownFinished == 1)
                        {
                            ResetContacts(Component[x, y].ComponentID);
                            Component[x, y].ComponentFilelName = "coil_offtimer_off";
                            Relays[i].TimeActivated = 0;
                        }
                        if (Component[x, y].IsConnectedNegative == 0 || Component[x, y].IsConnectedPositive == 0)
                        {
                            ResetContacts(Component[x, y].ComponentID);
                            Component[x, y].ComponentFilelName = "coil_offtimer";
                            if (Relays[i].TimeActivated == 1) this.Children.Remove(CountDownBlock[i]);
                            Relays[i].CountdownFinished = 0;
                            Relays[i].TimeActivated = 0;
                            Relays[i].TimeReseted = 1;
                        }
                    }
                }
            }
        }

        private void SetContacts(string id) 
        {
            int x; int y;
            for (y = 1; y < TableSize - 1; y++) // First and last rows can't contain relay contacts
            {
                for (x = 0; x < MaxX; x++)
                {
                    if (Component[x, y].ComponentID == id)
                    {
                        if (Component[x, y].Component == "contactNO") ContactNOClose(x, y);
                        if (Component[x, y].Component == "contactNC") ContactNCOpen(x, y);
                    }
                }
            }
        }

        private void ResetContacts(string id)
        {
            int x; int y;
            for (y = 1; y < TableSize - 1; y++)
            {
                for (x = 0; x < MaxX; x++)
                {
                    if (Component[x, y].ComponentID == id)
                    {
                        if (Component[x, y].Component == "contactNO") ContactNOOpen(x, y);
                        if (Component[x, y].Component == "contactNC") ContactNCClose(x, y);
                    }
                }
            }
        }

        private void ResetCircuit()
        {
            int x;
            int y;
            for (y = 0; y < TableSize; y++)
            {
                for (x = 0; x < MaxX; x++)
                {
                    if (Component[x, y].Component == "relaycoil") { ResetContacts(Component[x, y].ComponentID); Component[x, y].ComponentFilelName = "coil_norm"; }
                    if (Component[x, y].Component == "ontimercoil") { ResetContacts(Component[x, y].ComponentID); Component[x, y].ComponentFilelName = "coil_ontimer"; }
                    if (Component[x, y].Component == "offtimercoil") { ResetContacts(Component[x, y].ComponentID); Component[x, y].ComponentFilelName = "coil_offtimer"; }
                    if (Component[x, y].Component == "buttonNC") Component[x, y].ComponentFilelName = "btn_NC_c";
                    if (Component[x, y].Component == "buttonNO") Component[x, y].ComponentFilelName = "btn_NO_o";
                    if (Component[x, y].Component == "greenlight") Component[x, y].ComponentFilelName = "light_g_off";
                    if (Component[x, y].Component == "yellowlight") Component[x, y].ComponentFilelName = "light_y_off";
                    if (Component[x, y].Component == "redlight") Component[x, y].ComponentFilelName = "light_r_off";
                    Component[x, y].IsConnectedPositive = 0;
                    AddOnLineL(x, y, 0);
                    Component[x, y].IsConnectedNegative = 0;
                    AddOnLineN(x, y, 0);
                }
            }
            for (int j = 0; j < 10; j++) this.Children.Remove(CountDownBlock[j]);
        }

        private void ContactNOClose(int x, int y)
        {
            if (Component[x, y].IsConnectedPositive == 1) Component[x, y].ComponentFilelName = "contact_NO_c_r";
            else Component[x, y].ComponentFilelName = "contact_NO_c";
            Component[x, y].IsConductive = 1;
        }

        private void ContactNOOpen(int x, int y)
        {
            Component[x, y].ComponentFilelName = "contact_NO_o";
            Component[x, y].IsConductive = 0;
        }

        private void ContactNCClose(int x, int y)
        {
            if (Component[x, y].IsConnectedPositive == 1) Component[x, y].ComponentFilelName = "contact_NC_c_r";
            else Component[x, y].ComponentFilelName = "contact_NC_c";
            Component[x, y].IsConductive = 1;
        }

        private void ContactNCOpen(int x, int y)
        {
            Component[x, y].ComponentFilelName = "contact_NC_o";
            Component[x, y].IsConductive = 0;
        }

        //==================================================================================================
        //   LIGHT FUNCTIONS
        //==================================================================================================
        private void ToggleLights()
        {
            int x;
            int y;

            for (int i = 1; i < 10; i++)
            {
                x = Lights[i].X;
                y = Lights[i].Y;
                if (Lights[i].IsOnBoard == 1)
                {
                    if (Component[x, y].Component == "greenlight")
                    {
                        if (Component[x, y].IsConnectedNegative == 1 && Component[x, y].IsConnectedPositive == 1)
                            Component[x, y].ComponentFilelName = "light_g_on";
                        else Component[x, y].ComponentFilelName = "light_g_off";
                    }
                    if (Component[x, y].Component == "yellowlight")
                    {
                        if (Component[x, y].IsConnectedNegative == 1 && Component[x, y].IsConnectedPositive == 1)
                            Component[x, y].ComponentFilelName = "light_y_on";
                        else Component[x, y].ComponentFilelName = "light_y_off";
                    }
                    if (Component[x, y].Component == "redlight")
                    {
                        if (Component[x, y].IsConnectedNegative == 1 && Component[x, y].IsConnectedPositive == 1)
                            Component[x, y].ComponentFilelName = "light_r_on";
                        else Component[x, y].ComponentFilelName = "light_r_off";
                    }

                }
            }
        }
        
        //==================================================================================================
        //   FILE HANDLING
        //==================================================================================================
        public int LoadBoard(string OFile)
        {
            string[] lines = System.IO.File.ReadAllLines(OFile);
            //int LinesCount = lines.Length;
            int i = 0;
            int x; int y;
            string[] Oneline;
            for (int k = 0; k < 10; k++) { Relays[k].IsOnBoard = 0; }

            try
            {
                // Read first line
                Oneline = lines[i].Split('!');
                string size = Oneline[0];
                if (size == "10")
                {
                    TableSize = 10;
                    MaxX = 10;
                    NLineInPos = 9;
                }
                else
                {
                    TableSize = 15;
                    MaxX = 15;
                    NLineInPos = 14;
                }
                i++;

                for (y = 0; y < TableSize; y++)
                {
                    for (x = 0; x < TableSize; x++)
                    {

                        Oneline = lines[i].Split('!');

                        string comp = Oneline[0];
                        string id = Oneline[1];
                        int up = Convert.ToInt16(Oneline[2]);
                        int left = Convert.ToInt16(Oneline[3]);
                        int down = Convert.ToInt16(Oneline[4]);
                        int right = Convert.ToInt16(Oneline[5]);
                        int conductive = Convert.ToInt16(Oneline[6]);
                        string Fname = Oneline[7];
                        int dTime = Convert.ToInt16(Oneline[8]);

                        Component[x, y] = new CircuitComponent(comp, id, up, left, down, right, conductive)
                        {
                            ComponentFilelName = Fname,
                            IsConnectedPositive = 0,
                            IsConnectedNegative = 0,
                            Margin = new Thickness(x * 50, y * 50, 0, 0),
                            DelayTime = dTime
                        };
                        this.Children.Add(Component[x, y]);

                        if (comp != "line")
                        {
                            if (comp.Contains("coil")) IDLabel[x, y] = new PictureBox(PATH + id + ".png", x * 50, y * 50 + 20);
                            else if (comp == "buttonNO" || comp == "buttonNC") IDLabel[x, y] = new PictureBox(PATH + id + ".png", x * 50, y * 50 + 5);
                            else if (comp.Contains("light")) IDLabel[x, y] = new PictureBox(PATH + id + ".png", x * 50 + 5, y * 50 + 5);
                            else IDLabel[x, y] = new PictureBox(PATH + id + ".png", x * 50 + 5, y * 50 + 20);
                            this.Children.Add(IDLabel[x, y]);
                        }
                        
                        if (comp.EndsWith("coil") == true)
                        {
                            int j = Convert.ToInt16(id.Remove(0, 1));
                            Relays[j].ComponentType = comp.Replace("coil", "");
                            Relays[j].ComponentID = id;
                            Relays[j].IsOnBoard = 1;
                            Relays[j].X = x;
                            Relays[j].Y = y;
                            Relays[j].DelayTime = dTime;
                        }

                        if (comp.EndsWith("light") == true)
                        {
                            int k = Convert.ToInt16(id.Remove(0, 1));
                            Lights[k].ComponentType = comp.Replace("light", "");
                            Lights[k].ComponentID = id;
                            Lights[k].IsOnBoard = 1;
                            Lights[k].X = x;
                            Lights[k].Y = y;
                        }
                        i++;
                    }
                }
                
            }
            catch { MessageBox.Show("Tiedoston lataus epäonnistui", "Tiedostovirhe"); FillWithEmpty(); return 0; }
            try {
                Oneline = lines[i].Split('!');
                CommentText = Oneline[0];
            }
            catch { CommentText = ""; }
            return TableSize;
        }

        public void SaveBoard(string SFile)
        {
            int x; int y;
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(SFile))
                {
                    file.WriteLine(Convert.ToString(TableSize) + "!res");
                    for (y = 0; y < TableSize; y++)
                    {
                        for (x = 0; x < TableSize; x++)
                        {
                            file.WriteLine(
                                Component[x, y].Component + "!" +
                                Component[x, y].ComponentID + "!" +
                                Component[x, y].IsConnectedUp + "!" +
                                Component[x, y].IsConnectedLeft + "!" +
                                Component[x, y].IsConnectedDown + "!" +
                                Component[x, y].IsConnectedRight + "!" +
                                Component[x, y].IsConductive + "!" +
                                Component[x, y].ComponentFilelName + "!" +
                                Component[x, y].DelayTime);
                        }
                    }
                    file.WriteLine(CommentText);
                }
            }
            catch { MessageBox.Show("Tiedoston tallennus epäonnistui", "Tiedostovirhe"); return; }
        }
    }
}
