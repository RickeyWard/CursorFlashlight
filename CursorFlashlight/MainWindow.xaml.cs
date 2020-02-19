using Ownskit.Utils;
using SnagFree.TrayApp.Core;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CursorFlashlight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Storyboard ZoomInStoryBoard;
        Storyboard ZoomOutStoryBoard;
        Timer timer;

        private System.Windows.Forms.NotifyIcon notifyIcon = null;

        public MainWindow()
        {
            InitializeComponent();

            //create IN animation.
            DoubleAnimation opacityAnimationx = new DoubleAnimation();
            opacityAnimationx.To = 90;
            opacityAnimationx.Duration = TimeSpan.FromSeconds(0.35);
            DoubleAnimation opacityAnimationy = new DoubleAnimation();
            opacityAnimationy.To = 90;
            opacityAnimationy.Duration = TimeSpan.FromSeconds(0.35);
            Storyboard.SetTargetName(opacityAnimationx, "FlashlightMask");
            Storyboard.SetTargetName(opacityAnimationy, "FlashlightMask");
            Storyboard.SetTargetProperty(
                opacityAnimationx, new PropertyPath(RadialGradientBrush.RadiusXProperty));
            Storyboard.SetTargetProperty(
                opacityAnimationy, new PropertyPath(RadialGradientBrush.RadiusYProperty));
            ZoomInStoryBoard = new Storyboard();
            ZoomInStoryBoard.Children.Add(opacityAnimationx);
            ZoomInStoryBoard.Children.Add(opacityAnimationy);

            //create OUT animation.
            DoubleAnimation opacityoutAnimationx = new DoubleAnimation();
            opacityoutAnimationx.To = 3000;
            opacityoutAnimationx.Duration = TimeSpan.FromSeconds(0.35);
            DoubleAnimation opacityoutAnimationy = new DoubleAnimation();
            opacityoutAnimationy.To = 3000;
            opacityoutAnimationy.Duration = TimeSpan.FromSeconds(0.35);
            Storyboard.SetTargetName(opacityoutAnimationx, "FlashlightMask");
            Storyboard.SetTargetName(opacityoutAnimationy, "FlashlightMask");
            Storyboard.SetTargetProperty(
                opacityoutAnimationx, new PropertyPath(RadialGradientBrush.RadiusXProperty));
            Storyboard.SetTargetProperty(
                opacityoutAnimationy, new PropertyPath(RadialGradientBrush.RadiusYProperty));
            ZoomOutStoryBoard = new Storyboard();
            ZoomOutStoryBoard.Children.Add(opacityoutAnimationx);
            ZoomOutStoryBoard.Children.Add(opacityoutAnimationy);
            ZoomOutStoryBoard.Completed += ZoomOutStoryBoard_Completed;

            //hotkeys
            SetupKeyboardHooks();

            //don't show on start.
            this.Visibility = Visibility.Hidden;

            //cursor position is gotten manually.
            timer = new Timer(20);
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = false; //default but explicit.


            //setup trayIcon
            //TODO - do this better, possibly add an about window with config
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            var iconHandle = CursorFlashlight.Properties.Resources.flashlight_roi_icon.Handle;
            this.notifyIcon.Icon = System.Drawing.Icon.FromHandle(iconHandle);
            notifyIcon.Visible = true;
            notifyIcon.BalloonTipText = "CursorFlashLight";
            System.Windows.Forms.ContextMenu nCM = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem cmi_exit = new System.Windows.Forms.MenuItem("Exit");
            cmi_exit.Click += Cmi_edit_Click;
            nCM.MenuItems.Add(new System.Windows.Forms.MenuItem("CursorFlashLight v0.1a"));
            nCM.MenuItems[0].Enabled = false;
            nCM.MenuItems.Add(new System.Windows.Forms.MenuItem("by DiamondDrake.com"));
            nCM.MenuItems[1].Enabled = false;
            nCM.MenuItems.Add(new System.Windows.Forms.MenuItem("-"));
            nCM.MenuItems.Add(new System.Windows.Forms.MenuItem("hold Lctrl + LWin"));
            nCM.MenuItems[3].Enabled = false;
            nCM.MenuItems.Add(new System.Windows.Forms.MenuItem("-"));
            nCM.MenuItems.Add(cmi_exit);
            notifyIcon.ContextMenu = nCM;
        }

        private void Cmi_edit_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        //get cursor.
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!isON)
                return;

            Application.Current.Dispatcher.Invoke(new Action(() => { /* Your code here */ 
                Point mouseLoc = MouseUtilities.CorrectGetPosition(this);
                FlashlightMask.Center = mouseLoc;
                FlashlightMask.GradientOrigin = mouseLoc;
            }));
        }

        //hide the form when OUT finishes,
        //this makes sure the bloom isn't always visible on high res displays.
        private void ZoomOutStoryBoard_Completed(object sender, EventArgs e)
        {
            if(!ctrldwn && !shiftdwn)
                this.Visibility = Visibility.Hidden;
        }

        //cover ENTIRE virtual screen
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Height = System.Windows.SystemParameters.VirtualScreenHeight;
            this.Width = System.Windows.SystemParameters.VirtualScreenWidth;
            this.Left = System.Windows.SystemParameters.VirtualScreenLeft;
            this.Top = System.Windows.SystemParameters.VirtualScreenTop;
        }
        
        //private GlobalKeyboardHook _globalKeyboardHook;
        KeyboardListener KListener = new KeyboardListener();

        bool isON = false;

        public void SetupKeyboardHooks()
        {
            //_globalKeyboardHook = new GlobalKeyboardHook();
            //_globalKeyboardHook.KeyboardPressed += OnKeyPressed;

            KListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown);
            KListener.KeyUp += new RawKeyEventHandler(KListener_KeyUp);
        }

        bool ctrldwn = false;
        bool shiftdwn = false;

        void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            KListener_Key(sender, args, true);
        }
        void KListener_KeyUp(object sender, RawKeyEventArgs args)
        {
            KListener_Key(sender, args, false);
        }

       
        void KListener_Key(object sender, RawKeyEventArgs args, bool isDown)
        {
            //Console.WriteLine(args.Key.ToString());
            //Console.WriteLine(args.VKCode.ToString()); // Prints the text of pressed button, takes in account big and small letters. E.g. "Shift+a" => "A"


            //capture if the ctrl key is down
            if (args.VKCode == 0xA2)
                if (isDown)
                    ctrldwn = true;
                else
                    ctrldwn = false;

            //capture if the shift key is down
            //if (args.VKCode == 160)
            //    if (isDown)
            //        shiftdwn = true;
            //    else
            //        shiftdwn = false;

            //capture L windows key
            if (args.VKCode == 91)
                if (isDown)
                    shiftdwn = true;
                else
                    shiftdwn = false;


            // both? flashlight!
            if (shiftdwn && ctrldwn && !isON)
            {
                ZoomOutStoryBoard.Stop(this);

                this.Visibility = Visibility.Visible;
                isON = true;
                ZoomInStoryBoard.Begin(this);
                timer.Start();
            }
            //not both? no flashlight!
            else if ((!shiftdwn || !ctrldwn) && isON)
            {
                ZoomInStoryBoard.Stop(this);

                ZoomOutStoryBoard.Begin(this);
                isON = false;
                timer.Stop();
            }
        }

        //not convinced on the keylistner yet
        //private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        //{ 

        //    //capture if the ctrl key is down
        //    if (e.KeyboardData.VirtualCode == 0xA2)
        //        if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
        //            ctrldwn = true;
        //        else
        //            ctrldwn = false;

        //    //capture if the shift key is down
        //    if (e.KeyboardData.VirtualCode == 160)
        //        if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
        //            shiftdwn = true;
        //        else
        //            shiftdwn = false;


        //    // both? flashlight!
        //    if(shiftdwn && ctrldwn && !isON)
        //    {
        //        ZoomOutStoryBoard.Stop(this);

        //        this.Visibility = Visibility.Visible;
        //        isON = true;
        //        ZoomInStoryBoard.Begin(this);
        //        timer.Start();
        //    }
        //    //not both? no flashlight!
        //    else if((!shiftdwn || !ctrldwn) && isON)
        //    {
        //        ZoomInStoryBoard.Stop(this);

        //        ZoomOutStoryBoard.Begin(this);
        //        isON = false;
        //        timer.Stop();
        //    }

        //}

        public void Dispose()
        {
            //_globalKeyboardHook?.Dispose();

            KListener?.Dispose();
        }
    }

}
