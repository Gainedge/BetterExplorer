using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace CustomControls
{
    [TemplatePart(Name = "PART_RatingPath", Type = typeof(Path))]
    public sealed class RatingItem : ContentControl
    {
        public Rating ParentItem { get; set; }
        public RatingItemState State
        {
            get { return (RatingItemState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(RatingItemState), typeof(RatingItem), new PropertyMetadata(RatingItemState.NotActive));

        public RatingItem()
        {
            DefaultStyleKey = typeof(RatingItem);
        }

        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            this.State = RatingItemState.Active;
        }
        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            double value;
            Double.TryParse(this.Content.ToString(), out value);
            ParentItem.RatingValue = ++value;
        }
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            double value;
            this.State = RatingItemState.Pressed;
            Double.TryParse(this.Content.ToString(), out value);
            Rating.OldValue = ParentItem.RatingValue = ++value;
        }
    }
}
