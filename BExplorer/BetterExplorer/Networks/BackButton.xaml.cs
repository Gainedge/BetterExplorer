using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BetterExplorer.Networks
{
    /// <summary>
    /// Interaction logic for BackButton.xaml
    /// </summary>
    public partial class BackButton : UserControl
    {
        public BackButton()
        {
            InitializeComponent();
        }

        private Brush hl = new SolidColorBrush(Color.FromRgb(200, 240, 235));
        private Brush bk = new SolidColorBrush(Color.FromRgb(255, 255, 255));

        /// <summary>
        /// The name of the service.
        /// </summary>
        [Category("Common Items")]
        public string Text
        {
            get
            {
                return txtTitle.Text;
            }
            set
            {
                txtTitle.Text = value;
            }
        }

        /// <summary>
        /// An icon to represent the service.
        /// </summary>
        [Category("Common Items")]
        public ImageSource Icon
        {
            get
            {
                return imgIcon.Source;
            }
            set
            {
                imgIcon.Source = value;
            }
        }

        /// <summary>
        /// The color when this control has focus.
        /// </summary>
        [Category("Brushes")]
        public Brush Highlight
        {
            get
            {
                return hl;
            }
            set
            {
                hl = value;
            }
        }

        /// <summary>
        /// The color when this control does not have focus.
        /// </summary>
        [Category("Brushes")]
        new public Brush Background
        {
            get
            {
                return bk;
            }
            set
            {
                bk = value;
            }
        }

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event EventHandler Click;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnItemSelected()
        {
            if (Click != null)
                Click(this, EventArgs.Empty);
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            base.Background = hl;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            base.Background = bk;
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnItemSelected();
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                OnItemSelected();
            }
        }

        private void UserControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            base.Background = hl;
        }

        private void UserControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            base.Background = bk;
        }

    }
}
