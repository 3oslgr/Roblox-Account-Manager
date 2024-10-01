using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RBX_Alt_Manager.Classes
{
    public class BorderedRichTextBox : RichTextBox // https://github.com/r-aghaei/TextBoxBorderColor/blob/master/src/TextBoxBorderColor/MyTextBox.cs
    {
        const int WM_NCPAINT = 0x85;
        const uint RDW_INVALIDATE = 0x1;
        const uint RDW_IUPDATENOW = 0x100;
        const uint RDW_FRAME = 0x400;
        
        Color borderColor = Color.FromArgb(0x7A7A7A);

        public static uint SPIF_SENDCHANGE = 0x02;
        public static uint SPI_SETANIMATION = 0x0049;

        private Pen opaquePen;
        private Pen borderPen;

        public BorderedRichTextBox() : base() =>
            KeyDown += (s, e) =>
            {
                if ((e.Modifiers == Keys.Control && e.KeyCode == Keys.V) || (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Insert))
                    if (Clipboard.ContainsImage())
                        e.Handled = true;
            };

        public Color BorderColor
        {
            get { return borderColor; }
            set
            {
                borderColor = value;
                opaquePen = new Pen(new SolidBrush(Color.FromArgb(255, value.R, value.G, value.B)));
                borderPen = new Pen(BorderColor, 1f);

                Natives.RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero, RDW_FRAME | RDW_IUPDATENOW | RDW_INVALIDATE);
            }
        }

        protected override void WndProc(ref Message m)
        {
            WM msg = (WM)m.Msg;

            if (msg == WM.MOUSELEAVE || msg == WM.MOUSEHOVER)
                return;

            base.WndProc(ref m);

            if ((m.Msg == WM_NCPAINT || msg == WM.PAINT) && BorderColor != Color.Transparent && BorderStyle == BorderStyle.Fixed3D)
            {
                var hdc = Natives.GetWindowDC(Handle);

                using (var g = Graphics.FromHdcInternal(hdc))
                {
                    if (opaquePen == null || borderPen == null)
                        BorderColor = borderColor;

                    g.DrawRectangle(opaquePen, new Rectangle(0, 0, Width - 1, Height - 1));
                    g.DrawRectangle(borderPen, new Rectangle(1, 1, Width - 3, Height - 3));
                }

                Natives.ReleaseDC(Handle, hdc);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            Natives.RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero, RDW_FRAME | RDW_IUPDATENOW | RDW_INVALIDATE);
        }
    }
}