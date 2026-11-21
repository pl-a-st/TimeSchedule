using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace time_schedule
{
    public delegate void ForControl();
    public partial class fmProgressBar : Form
    {
        /// <summary>Цветной визуальный слой действующего нативного ProgressBar.Marquee.</summary>
        private readonly StyledMarqueeProgressIndicator marqueeIndicator;
        public delegate void LoadFinished();
        public event LoadFinished EventLoadFinished;
        public Action ActionForDoing { get; set; }
        public fmProgressBar()
        {
            InitializeComponent();
            marqueeIndicator = new StyledMarqueeProgressIndicator
            {
                Bounds = progressBar.Bounds,
                Anchor = progressBar.Anchor
            };
            Controls.Add(marqueeIndicator);
            marqueeIndicator.BringToFront();
            progressBar.Visible = false;
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            ThemeManager.Changed += ProgressThemeChanged;
            Disposed += ProgressDisposed;
            ApplyProgressTheme();
        }

        /// <summary>Оформляет окно загрузки, сохраняя штатный ProgressBar.Marquee и его скорость.</summary>
        private void ApplyProgressTheme()
        {
            if (IsDisposed) return;
            ThemePalette palette = ThemeManager.Palette;
            BackColor = palette.Window;
            ForeColor = palette.Text;
            label1.BackColor = palette.Window;
            label1.ForeColor = palette.Text;
            progressBar.BackColor = palette.Surface;
            progressBar.ForeColor = palette.Accent;
            marqueeIndicator.Invalidate();
            Invalidate();
        }

        /// <summary>Обновляет окно при смене темы без сброса состояния загрузки.</summary>
        private void ProgressThemeChanged(object sender, EventArgs e)
        {
            if (!IsDisposed) ApplyProgressTheme();
        }

        /// <summary>Удаляет подписку и не удерживает закрытое окно в памяти.</summary>
        private void ProgressDisposed(object sender, EventArgs e)
        {
            ThemeManager.Changed -= ProgressThemeChanged;
            Disposed -= ProgressDisposed;
        }

        /// <summary>Рисует один закруглённый контур целиком внутри окна, не обрезая крайние пиксели.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (ClientSize.Width < 5 || ClientSize.Height < 5) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath border = ThemeVisuals.InputPath(
                new Rectangle(1, 1, ClientSize.Width - 3, ClientSize.Height - 3)))
            using (Pen pen = new Pen(ThemeManager.Palette.Grid))
                e.Graphics.DrawPath(pen, border);
        }

        public void SetTextMessege(string textToMessege)
        {
            label1.Text = textToMessege;
        }
        public Form GetForm()
        {
            return this;
        }
        public Statuses.FormLoad StatusLoad { get; private set; } = Statuses.FormLoad.UnLoad;
        //ForControl ForControl = new ForControl(GetForm);
        private void fmProgressBar_Load(object sender, EventArgs e)
        {
                
            StatusLoad = Statuses.FormLoad.Load;
            progressBar.MarqueeAnimationSpeed = 10;
            progressBar.Style = ProgressBarStyle.Marquee;
            marqueeIndicator.StartAnimation();
            EventLoadFinished?.Invoke();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
