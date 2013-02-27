using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Windows.Threading;

namespace KinectSkeletonView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor kinectDevice;
        //private readonly Brush[] skeletonBrushes;//绘图笔刷
        private Skeleton[] frameSkeletons;

        private int peopleNumOld = 0;
        private int peopleNum = 0;

        private String defaultHintText = "Come closer and stand in front to know more about Engineering Product Development!!";
        private DispatcherTimer hintTextRollingTimer;
        private int rollingTimeInterval = 1;
        private int rollingStepDistance = 10;

        private String filepath = "./Test.avi";
        private bool isVideoPlaying = false;
        private DispatcherTimer delayTimer;
        private int delayTime = 20; //delay 20 scoends
        private bool isDelayTimerRunning = false;

        public MainWindow()
        {
            InitializeComponent();

            //skeletonBrushes = new Brush[] { Brushes.Black, Brushes.Crimson, Brushes.Indigo, Brushes.DodgerBlue, Brushes.Purple, Brushes.Pink };
             
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);

            videoElement.Source = new Uri(filepath, UriKind.Relative);
            hintTextLabel.Content = defaultHintText;

            //hint text rolling
            hintTextRollingTimer = new DispatcherTimer();
            hintTextRollingTimer.Tick += new EventHandler(hintTextRollingTimer_Tick);
            hintTextRollingTimer.Interval = new TimeSpan(0, 0, rollingTimeInterval);
            //hintTextRollingTimer.Start();

            //delay play timer
            delayTimer = new DispatcherTimer();
            delayTimer.Tick += new EventHandler(delayTimer_Tick);
            delayTimer.Interval = new TimeSpan(0, 0, delayTime);
        }
        
        public KinectSensor KinectDevice
        {
            get { return this.kinectDevice; }
            set
            {
                if (this.kinectDevice != value)
                {
                    //Uninitialize
                    if (this.kinectDevice != null)
                    {
                        this.kinectDevice.Stop();
                        this.kinectDevice.SkeletonFrameReady -= KinectDevice_SkeletonFrameReady;
                        this.kinectDevice.SkeletonStream.Disable();
                        this.frameSkeletons = null;
                    }

                    this.kinectDevice = value;

                    //Initialize
                    if (this.kinectDevice != null)
                    {
                        if (this.kinectDevice.Status == KinectStatus.Connected)
                        {
                            this.kinectDevice.SkeletonStream.Enable();
                            this.frameSkeletons = new Skeleton[this.kinectDevice.SkeletonStream.FrameSkeletonArrayLength];
                            this.kinectDevice.SkeletonFrameReady += KinectDevice_SkeletonFrameReady;
                            this.kinectDevice.Start();
                        }
                    }
                }
            }
        }


        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Initializing:
                case KinectStatus.Connected:
                case KinectStatus.NotPowered:
                case KinectStatus.NotReady:
                case KinectStatus.DeviceNotGenuine:
                    this.KinectDevice = e.Sensor;
                    break;
                case KinectStatus.Disconnected:
                    //TODO: Give the user feedback to plug-in a Kinect device.                    
                    this.KinectDevice = null;
                    break;
                default:
                    //TODO: Show an error state
                    break;
            }
        }



        private void KinectDevice_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    //Polyline figure;
                    //Brush userBrush;
                    Skeleton skeleton;

                    //LayoutRoot.Children.Clear();
                    frame.CopySkeletonDataTo(this.frameSkeletons);

                    //Skeleton[] dataSet2 = new Skeleton[this.frameSkeletons.Length];
                    //frame.CopySkeletonDataTo(dataSet2);

                    int notTrackedNum = 0;
                    int positionOnlyNum = 0;
                    int trackedNum = 0;

                    for (int i = 0; i < this.frameSkeletons.Length; i++)
                    {
                        skeleton = this.frameSkeletons[i];


                        if (skeleton.TrackingState == SkeletonTrackingState.NotTracked)
                        {
                            notTrackedNum += 1;
                        }


                        if (skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            positionOnlyNum += 1;
                        }
                       
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            trackedNum += 1;
                            
                            /*
                            Console.WriteLine(this.frameSkeletons.Length + "  num:   " + i % this.skeletonBrushes.Length);
                            userBrush = this.skeletonBrushes[i % this.skeletonBrushes.Length];

                            //绘制头和躯干
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.Head, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.Spine,
                                                                        JointType.ShoulderRight, JointType.ShoulderCenter, JointType.HipCenter
                                                                        });
                            LayoutRoot.Children.Add(figure);

                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipLeft, JointType.HipRight });
                            LayoutRoot.Children.Add(figure);

                            //绘制作腿
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipCenter, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft });
                            LayoutRoot.Children.Add(figure);

                            //绘制右腿
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipCenter, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight });
                            LayoutRoot.Children.Add(figure);

                            //绘制左臂
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft });
                            LayoutRoot.Children.Add(figure);

                            //绘制右臂
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight });
                            LayoutRoot.Children.Add(figure);
                             * 
                             * */
                        }

                    }

                    peopleNum = positionOnlyNum + trackedNum;
                    if( peopleNum != peopleNumOld )
                        Console.WriteLine("current people num = " + peopleNum);

                    if (peopleNum > 0 && peopleNumOld == 0)
                    {
                        this.onPeopleShowUp(peopleNum);
                    }
                    else if ( peopleNum == 0)
                    {
                        this.onPeopleNoneFrame();
                    }

                    peopleNumOld = peopleNum;
                }
            }
        }

        /*
        private Polyline CreateFigure(Skeleton skeleton, Brush brush, JointType[] joints)
        {
            Polyline figure = new Polyline();

            figure.StrokeThickness = 8;
            figure.Stroke = brush;

            for (int i = 0; i < joints.Length; i++)
            {
                figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]]));
            }

            return figure;
        }

        private Point GetJointPoint(Joint joint)
        {
            DepthImagePoint point = this.KinectDevice.MapSkeletonPointToDepth(joint.Position, this.KinectDevice.DepthStream.Format);
            point.X *= (int)this.LayoutRoot.ActualWidth / KinectDevice.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRoot.ActualHeight / KinectDevice.DepthStream.FrameHeight;

            return new Point(point.X, point.Y);
        }
         * 
         * */

        private void videoElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine("videoElement_SizeChanged " + this.Width + " " + this.Height);
        }

        private void grid1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine("grid1_SizeChanged " + this.Width + " " + this.Height);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine("Window_SizeChanged " + this.Width + " " + this.Height
                + " " + this.WindowState);

            this.LayoutRoot.Width = this.Width;
            this.LayoutRoot.Height = this.Height;
            this.videoElement.Width = this.Width;
            this.videoElement.Height = this.Height;
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }
            else
            {
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
            }
            
        }

        
        private void hintTextRollingTimer_Tick(object sender, EventArgs e)
        {
            hintTextLabel.Margin = new Thickness(hintTextLabel.Margin.Left - rollingStepDistance,0,0,0);

            if (hintTextLabel.Margin.Left <= -this.Width)
            {
                hintTextLabel.Margin = new Thickness(this.Width, 0, 0, 0);
            }
        }

        private void rollHintText()
        {
            this.hintTextRollingTimer.Start();
        }

        private void playVideoAndHideHint()
        {
            hintTextLabel.Visibility = Visibility.Hidden;
            this.videoElement.Play();
            isVideoPlaying = true;
        }

        private void stopVideoAndShowHint()
        {

            hintTextLabel.Visibility = Visibility.Visible;
            this.videoElement.Stop();
            this.videoElement.Close();
            isVideoPlaying = false;
        }

        private void delayTimer_Tick(object sender, EventArgs e)
        {
            if (isVideoPlaying)
            {
                Console.WriteLine("delay time out, stop video");
                this.stopVideoAndShowHint();
            }

            //stop timer
            this.delayTimer.Stop();
            isDelayTimerRunning = false;
        }

        private void onPeopleShowUp(int num)
        {
            if (isDelayTimerRunning)
            {   //stop timer
                this.delayTimer.Stop();
                isDelayTimerRunning = false;
            }
            this.playVideoAndHideHint();
        }

        private void onPeopleNoneFrame()
        {
            if (!isDelayTimerRunning)
            {
                delayTimer.Start();     //delay to stop video
                isDelayTimerRunning = true;
            }
        }

        private void videoElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            this.stopVideoAndShowHint();
        }

    }
}
