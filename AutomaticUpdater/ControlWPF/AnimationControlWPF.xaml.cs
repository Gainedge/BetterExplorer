using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wyDay.Controls
{
    /// <summary>
    /// Interaction logic for AnimationControlWPF.xaml
    /// </summary>
    public partial class AnimationControlWPF : UserControl
    {
        string m_BaseImage;
        int m_Rows = 1;
        int m_Columns = 1;
        bool m_SkipFirstFrame;

        readonly System.Timers.Timer aniTimer = new System.Timers.Timer();
        int m_AnimationInterval = 1000;

        //used in animation
        int columnOn = 1;
        int rowOn = 1;

        double frameWidth = 16;
        double frameHeight = 16;

        //for static images
        bool staticImage;

        readonly float[][] ptsArray ={ 
            new float[] {1, 0, 0, 0, 0},
            new float[] {0, 1, 0, 0, 0},
            new float[] {0, 0, 1, 0, 0},
            new float[] {0, 0, 0, 0, 0}, 
            new float[] {0, 0, 0, 0, 1}};

        readonly ImageAttributes imgAttributes = new ImageAttributes();

        public int AnimationInterval
        {
            get { return m_AnimationInterval; }
            set
            {
                m_AnimationInterval = value;
                aniTimer.Interval = m_AnimationInterval;
            }
        }

        public string BaseImage
        {
            get { return m_BaseImage; }
            set
            {
                m_BaseImage = value;
            }
        }

        public int Columns
        {
            get { return m_Columns; }
            set { m_Columns = value; }
        }

        public int Rows
        {
            get { return m_Rows; }
            set { m_Rows = value; }
        }

        public bool StaticImage
        {
            get { return staticImage; }
            set { staticImage = value; }
        }

        public bool CurrentlyAnimating
        {
            get
            {
                return aniTimer.Enabled;
            }
        }

        public bool SkipFirstFrame
        {
            get { return m_SkipFirstFrame; }
            set { m_SkipFirstFrame = value; }
        }

        public AnimationControlWPF()
        {
            InitializeComponent();
            aniTimer.Enabled = false;
            aniTimer.Elapsed += aniTimer_Tick;
        }

        void aniTimer_Tick(object sender, EventArgs e)
        {
            if (staticImage)
            {
                //if no transparency at all, stop the timer.
                if (ptsArray[3][3] >= 1f)
                {
                    StopAnimation();
                    ptsArray[3][3] = 1f;
                }
                else
                {
                    ptsArray[3][3] += .05f;
                }
            }
            else
            {
                if (columnOn == m_Columns)
                {
                    if (rowOn == m_Rows)
                    {
                        columnOn = m_SkipFirstFrame ? 2 : 1;

                        rowOn = 1;
                    }
                    else
                    {
                        columnOn = 1;
                        rowOn++;
                    }
                }
                else
                {
                    columnOn++;
                }
            }

            Refresh();
        }

        public void StartAnimation()
        {
            //if the timer isn't already running
            if (aniTimer.Enabled)
                return;

            aniTimer.Start();

            if (staticImage)
                ptsArray[3][3] = .05f;
            else
                columnOn++;

            Refresh();
        }

        public void StopAnimation()
        {
            aniTimer.Stop();
            columnOn = 1;
            rowOn = 1;
            Refresh();
            ptsArray[3][3] = 0; //reset to complete transparency
        }

        public void Refresh()
        {
            if (staticImage)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                          (ThreadStart)(() =>
                          {
                              imgAni.Source = new ImageSourceConverter().ConvertFromString(m_BaseImage) as ImageSource;// m_BaseImage;
                          }));
            }
            else
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                          (ThreadStart)(() =>
                          {
                              var y = new ImageSourceConverter().ConvertFromString(m_BaseImage) as BitmapSource;
                              imgAni.Source = CutImage(new ImageSourceConverter().ConvertFromString(m_BaseImage) as BitmapSource, (columnOn - 1) * (int)frameWidth, (rowOn - 1) * (int)frameHeight);
                          }));
            }
        }

        private BitmapSource CutImage(BitmapSource img , int x, int y)
        {
            int count = 0;
            MemoryStream mStream= new MemoryStream();

            PngBitmapEncoder jEncoder = new PngBitmapEncoder();

            jEncoder.Frames.Add(BitmapFrame.Create(new CroppedBitmap(img, new Int32Rect(x, y, 16, 16))));  //the croppedBitmap is a CroppedBitmap object 
            jEncoder.Save(mStream); 
 
            BitmapImage image = new BitmapImage(); 
            image.BeginInit(); 
            image.StreamSource = mStream; 
            image.EndInit();
            return image;

        }
    }
}
