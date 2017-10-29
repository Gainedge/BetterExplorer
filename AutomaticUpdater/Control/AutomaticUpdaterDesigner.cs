using System.Drawing;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace wyDay.Controls
{
#if NET_2_0
    internal
#else
    public
#endif
    class AutomaticUpdaterDesigner : ControlDesigner
    {
        public override SelectionRules SelectionRules
        {
            get
            {
                if (!((AutomaticUpdater)Control).Animate)
                    return base.SelectionRules;

                return base.SelectionRules & ~(SelectionRules.AllSizeable);
            }
        }

#if NET_2_0
        readonly Bitmap bmpNotify = new Bitmap(typeof(AutomaticUpdater), "update-notify.png");
#else
        readonly Bitmap bmpNotify = Resource.update_notify;
#endif

        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            pe.Graphics.FillRectangle(SystemBrushes.Control, Control.ClientRectangle);

            pe.Graphics.DrawImage(bmpNotify, new Rectangle(0, 0, 16, 16), new Rectangle(0, 0, 16, 16), GraphicsUnit.Pixel);
            
            base.OnPaintAdornments(pe);
        }
    }
}