using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace ScreenOCRTranslator
{
    public partial class SelectionForm : Form
    {
        private Point startPoint;
        private Rectangle selectedRect;
        private Rectangle selectedRectScreen;    // 螢幕絕對座標（回傳用）
        private bool isDragging = false;
        public event Action<Bitmap, Rectangle> OnSelectionCompleted;

        // ✅ 改：回傳螢幕絕對座標
        public Rectangle SelectedRectangle => selectedRectScreen;

        public SelectionForm()
        {
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Opacity = 0.25;
            this.BackColor = Color.Gray;
            this.TopMost = true;
            this.Cursor = Cursors.Cross;
            this.ShowInTaskbar = false;

            var screen = Screen.FromPoint(Cursor.Position);
            this.StartPosition = FormStartPosition.Manual;
            this.WindowState = FormWindowState.Normal;
            this.Bounds = screen.Bounds;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                startPoint = e.Location;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDragging)
            {
                selectedRect = new Rectangle(
                    Math.Min(startPoint.X, e.X),
                    Math.Min(startPoint.Y, e.Y),
                    Math.Abs(startPoint.X - e.X),
                    Math.Abs(startPoint.Y - e.Y)
                );
                this.Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!isDragging) return;
            isDragging = false;

            if (selectedRect.Width < 5 || selectedRect.Height < 5)
            {
                this.Close();
                return;
            }

            // ✅ 這個才是螢幕絕對座標（可為負值、可超過主螢幕寬度）
            var absoluteRect = new Rectangle(
                this.Bounds.Left + selectedRect.Left,
                this.Bounds.Top + selectedRect.Top,
                selectedRect.Width,
                selectedRect.Height
            );

            selectedRectScreen = absoluteRect;

            Bitmap capture = new Bitmap(absoluteRect.Width, absoluteRect.Height);
            using (Graphics g = Graphics.FromImage(capture))
            {
                g.CopyFromScreen(absoluteRect.Location, Point.Empty, absoluteRect.Size);
            }

            OnSelectionCompleted?.Invoke(capture, absoluteRect);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (selectedRect != Rectangle.Empty)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, selectedRect);
                }
            }
        }
    }
}
