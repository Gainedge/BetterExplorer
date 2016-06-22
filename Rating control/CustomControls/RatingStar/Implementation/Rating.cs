using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CustomControls
{
    public class Rating : ItemsControl
    {
        public static double OldValue { get; set; }

        public int RatingItemLimit
        {
            get { return (int)GetValue(RatingItemLimitProperty); }
            set { SetValue(RatingItemLimitProperty, value); }
        }

        public double RatingValue
        {
            get { return (double)GetValue(RatingValueProperty); }
            set { SetValue(RatingValueProperty, value); }
        }

        public Brush RatingItemBackground
        {
            get { return (Brush)GetValue(RatingItemBackgroundProperty); }
            set { SetValue(RatingItemBackgroundProperty, value); }
        }

        public Brush RatingItemHighlightColor
        {
            get { return (Brush)GetValue(RatingItemHighlightColorProperty); }
            set { SetValue(RatingItemHighlightColorProperty, value); }
        }

        public Brush RatingItemMouseDownColor
        {
            get { return (Brush)GetValue(RatingItemMouseDownColorProperty); }
            set { SetValue(RatingItemMouseDownColorProperty, value); }
        }

        public static readonly DependencyProperty RatingItemMouseDownColorProperty =
            DependencyProperty.Register("RatingItemMouseDownColor", typeof(Brush), typeof(Rating), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty RatingItemHighlightColorProperty =
              DependencyProperty.Register("RatingItemHighlightColor", typeof(Brush), typeof(Rating), new PropertyMetadata(Brushes.Red));

        public static readonly DependencyProperty RatingItemBackgroundProperty =
            DependencyProperty.Register("RatingItemBackground", typeof(Brush), typeof(Rating), new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty RatingValueProperty =
            DependencyProperty.Register("RatingValue", typeof(double), typeof(Rating), new PropertyMetadata(0.1, OnRatingValueChanged));

        public static readonly DependencyProperty RatingItemLimitProperty =
         DependencyProperty.Register("RatingItemLimit", typeof(int), typeof(Rating), new PropertyMetadata(0, OnRatingItemLimitChanged));

        private static void OnRatingValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnRatingvalueChanged(d);
        }

        private static void OnRatingItemLimitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Rating rating = (Rating)d;
            rating.ItemsSource = Enumerable.Range(0, rating.RatingItemLimit);
        }

        public Rating()
        {
            DefaultStyleKey = typeof(Rating);
            this.Loaded += this.Rating_Loaded;
        }

        ~Rating()
        {
            this.Loaded -= this.Rating_Loaded;
        }

        private static void OnRatingvalueChanged(object sender)
        {
            Rating rating = (Rating)sender;
            if (rating.RatingValue <= rating.RatingItemLimit && rating.RatingValue >= 0 &&
               ((ItemContainerGenerator)rating.ItemContainerGenerator).Status != GeneratorStatus.NotStarted)
            {
                int position = 0;
                double value = Double.Parse((rating.RatingValue - Math.Truncate(rating.RatingValue)).ToString("N1"));
                foreach (var item in rating.Items)
                {
                    position++;
                    var ratingItem = (RatingItem)rating.ItemContainerGenerator.ContainerFromItem(item);
                    var path = ratingItem.Template.FindName("PART_RatingPath", ratingItem) as Path;
                    ratingItem.State = position - 1 < rating.RatingValue ? RatingItemState.Active : RatingItemState.NotActive;
                    path.Clip = null;
                    if (position - 1 < Math.Floor(rating.RatingValue))
                    {
                        path.Clip = new RectangleGeometry(new Rect(0, 0, ratingItem.ActualWidth, ratingItem.ActualHeight));
                    }
                    else if (position - 1 == Math.Floor(rating.RatingValue) && value > 0)
                    {
                        path.Clip = new RectangleGeometry(new Rect(0, 0, value * ratingItem.ActualWidth, ratingItem.ActualHeight));
                    }
                }

                if (!rating.IsMouseOver)
                {
                    OldValue = rating.RatingValue;
                }
            }
        }

        private void Rating_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.Rating_Loaded;
            OnRatingvalueChanged(sender);
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            if (OldValue > 0)
            {
                this.RatingValue = OldValue;
            }
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is RatingItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RatingItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            RatingItem ratingItem = (RatingItem)element;
            ratingItem.Content = item;
            ratingItem.ContentTemplate = this.ItemTemplate;
            ratingItem.ParentItem = this;
        }
    }
}
