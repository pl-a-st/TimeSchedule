using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Неблокирующая матовая индикация неопределённого прогресса с цветом активной темы.
    /// Показывается над действующим WinForms ProgressBar, не изменяя работу загрузки.
    /// </summary>
    internal sealed class StyledMarqueeProgressIndicator : Control
    {
        /// <summary>Период сдвига ползунка в миллисекундах.</summary>
        private const int FrameInterval = 40;
        /// <summary>Таймер пользовательского интерфейса, работающий только при показе окна.</summary>
        private readonly Timer repaintTimer;
        /// <summary>Положение движущегося ползунка как доля полного прохода.</summary>
        private int frame;
        /// <summary>Число кадров движения через всю дорожку.</summary>
        private const int FrameCount = 80;
        /// <summary>Признак разрешённого движения после штатной загрузки формы.</summary>
        private bool started;

        /// <summary>Создаёт буферизованный индикатор с единственным UI-таймером.</summary>
        internal StyledMarqueeProgressIndicator()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            AccessibleRole = System.Windows.Forms.AccessibleRole.ProgressBar;
            AccessibleName = "Выполняется загрузка";
            repaintTimer = new Timer { Interval = FrameInterval };
            repaintTimer.Tick += RepaintTimerTick;
            ThemeManager.Changed += ThemeChanged;
        }

        /// <summary>Начинает индикацию после запуска прежнего ProgressBar.Marquee.</summary>
        internal void StartAnimation()
        {
            started = true;
            if (Visible) repaintTimer.Start();
            Invalidate();
        }

        /// <summary>Не расходует ресурсы на невидимую или закрытую форму.</summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (repaintTimer == null) return;
            if (Visible && started) repaintTimer.Start();
            else repaintTimer.Stop();
        }

        /// <summary>Сдвигает ползунок без синхронизации с рабочим потоком загрузки.</summary>
        private void RepaintTimerTick(object sender, EventArgs e)
        {
            frame = (frame + 1) % FrameCount;
            Invalidate();
        }

        /// <summary>Перерисовывает оттенок при смене общей темы.</summary>
        private void ThemeChanged(object sender, EventArgs e) { if (!IsDisposed) Invalidate(); }

        /// <summary>Рисует ползунок только внутри дорожки и поверх него — один непрерывный контур.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(palette.Window);
            if (Width < 12 || Height < 12) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle outer = new Rectangle(1, 1, Width - 3, Height - 3);
            Rectangle inner = Rectangle.Inflate(outer, -4, -5);
            using (GraphicsPath borderPath = ThemeVisuals.InputPath(outer))
            using (SolidBrush surface = new SolidBrush(palette.Surface))
            using (Pen border = new Pen(palette.Grid))
            {
                e.Graphics.FillPath(surface, borderPath);
                if (inner.Width > 8 && inner.Height > 4)
                {
                    int thumbWidth = Math.Max(28, inner.Width / 4);
                    int distance = inner.Width + thumbWidth;
                    int x = inner.Left - thumbWidth +
                        (int)Math.Round(distance * (double)frame / FrameCount);
                    Rectangle thumb = new Rectangle(x, inner.Top, thumbWidth, inner.Height);
                    GraphicsState state = e.Graphics.Save();
                    try
                    {
                        using (GraphicsPath clipPath = ThemeVisuals.InputPath(inner))
                        using (GraphicsPath thumbPath = ThemeVisuals.InputPath(thumb))
                        using (SolidBrush accent = new SolidBrush(palette.Focus))
                        {
                            e.Graphics.SetClip(clipPath);
                            e.Graphics.FillPath(accent, thumbPath);
                        }
                    }
                    finally { e.Graphics.Restore(state); }
                }
                e.Graphics.DrawPath(border, borderPath);
            }
        }

        /// <summary>Освобождает UI-таймер и подписку на палитру вместе с индикатором.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.Changed -= ThemeChanged;
                repaintTimer.Stop();
                repaintTimer.Tick -= RepaintTimerTick;
                repaintTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
