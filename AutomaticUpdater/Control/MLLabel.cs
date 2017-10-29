using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace wyDay.Controls
{
#if AUPDATE
    internal class MLLabel : Control
#else
    public class MLLabel : Control
#endif
    {
        Rectangle textRect;
        int prevWidth = -1;

#if AUPDATE
        const bool m_Multiline = true;
        const TextFormatFlags flags = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix;
#else
        bool m_Multiline = true;
        TextFormatFlags flags = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix;


        [DefaultValue(true)]
        public bool Multiline
        {
            get { return m_Multiline; }
            set 
            {
                if (m_Multiline != value)
                {
                    m_Multiline = value;

                    if (m_Multiline)
                    {
                        flags = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix;
                    }
                    else
                    {
                        flags = TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;
                    }

                    RefreshTextRect();
                    Invalidate();
                }
                
            }
        }

        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
#endif
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;

                //recalculate text size
                RefreshTextRect();

                Invalidate();
            }
        }


        public MLLabel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Selectable, false);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            //recalculate the text size
            RefreshTextRect();

            base.OnFontChanged(e);
        }

        private void RefreshTextRect()
        {
            prevWidth = Width;

            textRect = new Rectangle(Point.Empty, TextRenderer.MeasureText(Text, Font, Size, flags));

            Height = textRect.Height;

            //set the width for single line label
            if (!m_Multiline)
                Width = textRect.Width;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;

            //text
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, ForeColor, flags);
        }

        protected override void OnResize(EventArgs e)
        {
            if (m_Multiline && prevWidth != Width)
            {
                RefreshTextRect();
            }

            base.OnResize(e);
        }
    }

}
