using System;
using System.Collections.Generic;
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

namespace Odyssey.Controls
{

    public class AeroChrome : ContentControl
    {
        static AeroChrome()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AeroChrome), new FrameworkPropertyMetadata(typeof(AeroChrome)));
        }


        public bool RenderPressed
        {
            get { return (bool)GetValue(RenderPressedProperty); }
            set { SetValue(RenderPressedProperty, value); }
        }

        public static readonly DependencyProperty RenderPressedProperty =
            DependencyProperty.Register("RenderPressed", typeof(bool), typeof(AeroChrome), new UIPropertyMetadata(false));


        public bool RenderMouseOver
        {
            get { return (bool)GetValue(RenderMouseOverProperty); }
            set { SetValue(RenderMouseOverProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RenderMouseOver.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RenderMouseOverProperty =
            DependencyProperty.Register("RenderMouseOver", typeof(bool), typeof(AeroChrome), new UIPropertyMetadata(false));



        public Brush MouseOverBackground
        {
            get { return (Brush)GetValue(MouseOverBackgroundProperty); }
            set { SetValue(MouseOverBackgroundProperty, value); }
        }

        public static readonly DependencyProperty MouseOverBackgroundProperty =
            DependencyProperty.Register("MouseOverBackground", typeof(Brush), typeof(AeroChrome), new UIPropertyMetadata(null));



        public Brush MousePressedBackground
        {
            get { return (Brush)GetValue(MousePressedBackgroundProperty); }
            set { SetValue(MousePressedBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MousePressedBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MousePressedBackgroundProperty =
            DependencyProperty.Register("MousePressedBackground", typeof(Brush), typeof(AeroChrome), new UIPropertyMetadata(null));


    }
}
