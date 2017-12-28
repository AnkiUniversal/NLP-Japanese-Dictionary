using NLPJapaneseDictionary.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NLPJapaneseDictionary.Windows
{
    public partial class SnippingTool : Form
    {
        public static bool IsCapturing { get; private set; }

        public static event EventHandler Cancel;
        public static event EventHandler AreaSelected;
        public static Bitmap Bitmap { get; set; }

        private const int MIN_CAPTURE_WIDTH = 5;
        private const int MIN_CAPTURE_HEIGHT = 5;
        private const int MAX_CAPTURE_WIDTH = 1920;
        private const int MAX_CAPTURE_HEIGHT = 1080;

        private static SnippingTool[] forms;
        private Rectangle rectSelection;
        private Point pointStart;

        public SnippingTool(Image screenShot, int x, int y, int width, int height)
        {
            InitializeComponent();
            Opacity = 0.15;
            BackgroundImageLayout = ImageLayout.Stretch;
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            SetBounds(x, y, width, height);
            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;
            Cursor = Cursors.Cross;
            TopMost = true;
        }

        private static void OnCancel(EventArgs e)
        {
            Cancel?.Invoke(null, e);
        }

        private void OnAreaSelected(EventArgs e)
        {
            AreaSelected?.Invoke(this, e);
        }

        private static void CloseForms()
        {
            for (int i = 0; i < forms.Length; i++)
            {
                forms[i].Dispose();
            }
            IsCapturing = false;
        }

        public static void Snip()
        {
            if (IsCapturing)
                return;

            if (Bitmap != null)
            {
                Bitmap.Dispose();
                GC.Collect();
            }
            IsCapturing = true;

            var screens = ScreenHelper.GetMonitorsInfo();
            forms = new SnippingTool[screens.Count];
            for (int i = 0; i < screens.Count; i++)
            {
                int hRes = screens[i].HorizontalResolution;
                int vRes = screens[i].VerticalResolution;
                int top = screens[i].MonitorArea.Top;
                int left = screens[i].MonitorArea.Left;
                var bmp = new Bitmap(hRes, vRes, PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(left, top, 0, 0, bmp.Size);
                }
                forms[i] = new SnippingTool(bmp, left, top, hRes, vRes);
                forms[i].Show();
            }
        }

        #region Overrides
        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Start the snip on mouse down
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            pointStart = e.Location;
            rectSelection = new Rectangle(e.Location, new Size(0, 0));
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Modify the selection on mouse move
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            int x1 = Math.Min(e.X, pointStart.X);
            int y1 = Math.Min(e.Y, pointStart.Y);
            int x2 = Math.Max(e.X, pointStart.X);
            int y2 = Math.Max(e.Y, pointStart.Y);
            rectSelection = new Rectangle(x1, y1, x2 - x1, y2 - y1);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            try
            {
                if (rectSelection.Width <= 0 || rectSelection.Height <= 0)
                {
                    OnCancel(new EventArgs());
                    return;
                }
                foreach (var f in forms)
                    f.Opacity = 0;

                var point = PointToScreen(rectSelection.Location);
                if (rectSelection.Width < MIN_CAPTURE_WIDTH || rectSelection.Height < MIN_CAPTURE_HEIGHT)
                {
                    MessageBox.Show("Capture region is too small.", "Invalid Capture Size");
                }
                else if (rectSelection.Width > MAX_CAPTURE_WIDTH || rectSelection.Height > MAX_CAPTURE_HEIGHT)
                {
                    MessageBox.Show("Capture region is too large.", "Invalid Capture Size");
                }
                else
                {
                    Bitmap = ScreenHelper.CaptureScreen(point, rectSelection.Width, rectSelection.Height);
                    OnAreaSelected(new EventArgs());
                }
            }
            finally
            {
                CloseForms();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw the current selection
            using (Brush br = new SolidBrush(Color.FromArgb(120, Color.White)))
            {
                int x1 = rectSelection.X;
                int x2 = rectSelection.X + rectSelection.Width;
                int y1 = rectSelection.Y;
                int y2 = rectSelection.Y + rectSelection.Height;
                e.Graphics.FillRectangle(br, new Rectangle(0, 0, x1, Height));
                e.Graphics.FillRectangle(br, new Rectangle(x2, 0, Width - x2, Height));
                e.Graphics.FillRectangle(br, new Rectangle(x1, 0, x2 - x1, y1));
                e.Graphics.FillRectangle(br, new Rectangle(x1, y2, x2 - x1, Height - y2));
            }
            using (Pen pen = new Pen(Color.Green, 2))
            {
                e.Graphics.DrawRectangle(pen, rectSelection);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Allow canceling the snip with the Escape key
            if (keyData == Keys.Escape)
            {
                CancelSnipping();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public static void CancelSnipping()
        {
            if (Bitmap != null)
            {
                Bitmap.Dispose();
                GC.Collect();
            }

            Bitmap = null;
            CloseForms();
            OnCancel(new EventArgs());
        }
        #endregion
    }
}
