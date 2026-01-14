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
        private bool isDragging = false;
        public event Action<Bitmap, Rectangle> OnSelectionCompleted;

        public Rectangle SelectedRectangle => selectedRect;

        public SelectionForm()
        {
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Opacity = 0.25;
            this.BackColor = Color.Gray;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.Cursor = Cursors.Cross;
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
            if (isDragging)
            {
                isDragging = false;

                if (selectedRect.Width < 5 || selectedRect.Height < 5)
                {
                    this.Close();
                    return;
                }

                // 擷取圖片
                Bitmap capture = new Bitmap(selectedRect.Width, selectedRect.Height);
                using (Graphics g = Graphics.FromImage(capture))
                {
                    g.CopyFromScreen(this.PointToScreen(selectedRect.Location), Point.Empty, selectedRect.Size);
                }

                OnSelectionCompleted?.Invoke(capture, selectedRect); // 回傳給主畫面

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
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
