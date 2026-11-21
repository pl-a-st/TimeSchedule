using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Выбирает исходный цвет задачи из семейств и ступеней насыщенности,
    /// не изменяя модель задачи и глобальный цвет до подтверждения выбора.
    /// </summary>
    internal sealed class TaskColorPaletteForm : StyledWindowForm
    {
        /// <summary>Названия девяти утверждённых цветовых семейств отображения задач.</summary>
        private static readonly string[] familyNames =
        {
            "Красный", "Оранжевый", "Жёлтый", "Зелёный", "Бирюзовый",
            "Синий", "Фиолетовый", "Розовый", "Серый"
        };

        /// <summary>Образцы исходных цветов по семействам и ступеням насыщенности.</summary>
        private readonly Button[,] swatches =
            new Button[familyNames.Length, ThemeVisuals.TaskPaletteToneCount];

        /// <summary>Все надписи, зависящие от темы интерфейса.</summary>
        private readonly List<Label> labels = new List<Label>();

        /// <summary>Подсказки для образцов с полным RGB.</summary>
        private readonly ToolTip swatchToolTip = new ToolTip();

        /// <summary>Описание выбранного исходного цвета.</summary>
        private readonly Label selectedLabel;

        /// <summary>Предпросмотр того же RGB в светлой и тёмной темах.</summary>
        private readonly Panel lightPreview, darkPreview;

        /// <summary>Кнопки единого матового стиля с исходными действиями и DialogResult.</summary>
        private readonly StyledButton selectButton, cancelButton, customButton;

        /// <summary>Шрифт, которым владеет окно палитры.</summary>
        private readonly Font paletteFont;

        /// <summary>Локальный исходный RGB до подтверждения редактором задачи.</summary>
        private Color selectedColor;

        /// <summary>Окно само защищает цвета образцов от общего оформления контролов.</summary>
        protected override bool AutoThemeStandardControls { get { return false; } }

        /// <summary>Исходный RGB, подтверждённый пользователем (либо первоначальный при отмене).</summary>
        internal Color SelectedColor { get { return selectedColor; } }

        /// <summary>Создаёт диалог с текущим цветом задачи и двумя независимыми предпросмотрами.</summary>
        /// <param name="currentColor">Сохранённый исходный RGB без преобразования темой.</param>
        internal TaskColorPaletteForm(Color currentColor)
        {
            if (familyNames.Length != ThemeVisuals.TaskPaletteFamilyCount)
                throw new InvalidOperationException("Число семейств палитры не совпадает с цветовой схемой задач.");
            selectedColor = currentColor;
            Text = "Цвет задачи";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            ClientSize = new Size(640, 478);
            paletteFont = (Font)Control.DefaultFont.Clone();
            Font = paletteFont;
            swatchToolTip.OwnerDraw = true;
            swatchToolTip.Popup += SwatchToolTipPopup;
            swatchToolTip.Draw += SwatchToolTipDraw;

            AddLabel("Выберите цвет задачи", 18, 11, 600, 24);
            AddLabel("Светлее", 164, 40, 95, 20);
            AddLabel("2", 269, 40, 95, 20);
            AddLabel("3", 374, 40, 95, 20);
            AddLabel("Насыщеннее", 479, 40, 110, 20);
            CreateSwatches();

            selectedLabel = AddLabel(string.Empty, 18, 316, 610, 22);
            lightPreview = CreatePreview(18, ThemeKind.Light);
            darkPreview = CreatePreview(327, ThemeKind.Dark);
            customButton = CreateActionButton("Другой цвет…", 18, 431, 145);
            selectButton = CreateActionButton("Выбрать", 365, 431, 118);
            cancelButton = CreateActionButton("Отмена", 493, 431, 120);
            customButton.TabIndex = familyNames.Length * ThemeVisuals.TaskPaletteToneCount;
            selectButton.TabIndex = customButton.TabIndex + 1;
            cancelButton.TabIndex = selectButton.TabIndex + 1;
            customButton.Click += ChooseCustomColor;
            selectButton.DialogResult = DialogResult.OK;
            cancelButton.DialogResult = DialogResult.Cancel;
            AcceptButton = selectButton;
            CancelButton = cancelButton;

            ThemeManager.Changed += PaletteThemeChanged;
            Disposed += PaletteDisposed;
            ApplyPaletteTheme();
            RefreshSelection();
        }

        /// <summary>
        /// Вычисляет окончательное положение до ShowDialog, пока окно ещё не отображалось.
        /// Учитывает высоту будущего заголовка, добавляемую базовой формой при Load.
        /// </summary>
        /// <param name="owner">Редактор задачи, относительно которого открывается палитра.</param>
        internal void PositionBeforeShow(Form owner)
        {
            Rectangle work = owner != null && !owner.IsDisposed
                ? Screen.FromControl(owner).WorkingArea
                : Screen.FromPoint(Cursor.Position).WorkingArea;
            Rectangle anchor = owner != null && !owner.IsDisposed ? owner.Bounds : work;
            int finalWidth = ClientSize.Width;
            int finalHeight = ClientSize.Height + WindowChromeHeight;
            int x = anchor.Left + (anchor.Width - finalWidth) / 2;
            int y = anchor.Top + (anchor.Height - finalHeight) / 2;
            Location = new Point(
                Math.Max(work.Left, Math.Min(x, work.Right - finalWidth)),
                Math.Max(work.Top, Math.Min(y, work.Bottom - finalHeight)));
        }

        /// <summary>Добавляет короткую подпись без создания собственных обработчиков рисования.</summary>
        private Label AddLabel(string title, int x, int y, int width, int height)
        {
            var label = new Label
            {
                Text = title,
                Left = x,
                Top = y,
                Width = width,
                Height = height,
                TextAlign = ContentAlignment.MiddleLeft
            };
            labels.Add(label);
            Controls.Add(label);
            return label;
        }

        /// <summary>Создаёт матрицу 9 × 4 с выбором мышью и стрелками клавиатуры.</summary>
        private void CreateSwatches()
        {
            for (int family = 0; family < familyNames.Length; family++)
            {
                AddLabel(familyNames[family], 18, 63 + family * 28, 141, 25);
                for (int tone = 0; tone < ThemeVisuals.TaskPaletteToneCount; tone++)
                {
                    Color sourceColor = ThemeVisuals.TaskPaletteSourceColor(family, tone);
                    var swatch = new Button
                    {
                        Left = 164 + tone * 105,
                        Top = 64 + family * 28,
                        Width = 99,
                        Height = 24,
                        BackColor = sourceColor,
                        Tag = sourceColor,
                        FlatStyle = FlatStyle.Flat,
                        UseVisualStyleBackColor = false,
                        AccessibleName = familyNames[family] + ", оттенок " + (tone + 1),
                        TabIndex = family * ThemeVisuals.TaskPaletteToneCount + tone
                    };
                    swatch.FlatAppearance.MouseOverBackColor = sourceColor;
                    swatch.FlatAppearance.MouseDownBackColor = sourceColor;
                    swatch.Click += SelectSwatch;
                    swatch.PreviewKeyDown += SwatchPreviewKeyDown;
                    swatch.KeyDown += SwatchKeyDown;
                    swatchToolTip.SetToolTip(swatch, familyNames[family] + ", оттенок " +
                        (tone + 1) + " — " + ToHex(sourceColor));
                    swatches[family, tone] = swatch;
                    Controls.Add(swatch);
                }
            }
        }

        /// <summary>Создаёт панель постоянного предпросмотра, независимо от активной темы.</summary>
        private Panel CreatePreview(int x, ThemeKind kind)
        {
            var preview = new Panel
            {
                Left = x,
                Top = 353,
                Width = 294,
                Height = 67,
                Tag = kind,
                TabStop = false
            };
            preview.Paint += PaintPreview;
            Controls.Add(preview);
            return preview;
        }

        /// <summary>Создаёт кнопку общего матового стиля без замены действий и DialogResult.</summary>
        private StyledButton CreateActionButton(string title, int x, int y, int width)
        {
            var button = new StyledButton
            {
                Text = title,
                Left = x,
                Top = y,
                Width = width,
                Height = 33
            };
            Controls.Add(button);
            return button;
        }

        /// <summary>Выбирает исходный RGB образца; сохранение выполняет только редактор задачи.</summary>
        private void SelectSwatch(object sender, EventArgs e)
        {
            selectedColor = (Color)((Button)sender).Tag;
            RefreshSelection();
        }

        /// <summary>Разрешает стрелкам выбирать образцы, сохраняя Tab и Enter штатными.</summary>
        private void SwatchPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Left || e.KeyCode == System.Windows.Forms.Keys.Right ||
                e.KeyCode == System.Windows.Forms.Keys.Up || e.KeyCode == System.Windows.Forms.Keys.Down)
                e.IsInputKey = true;
        }

        /// <summary>Перемещает выделение в цветовой матрице без закрытия диалога.</summary>
        private void SwatchKeyDown(object sender, KeyEventArgs e)
        {
            int index = ((Button)sender).TabIndex;
            int family = index / ThemeVisuals.TaskPaletteToneCount;
            int tone = index % ThemeVisuals.TaskPaletteToneCount;
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.Left: tone = Math.Max(0, tone - 1); break;
                case System.Windows.Forms.Keys.Right: tone = Math.Min(ThemeVisuals.TaskPaletteToneCount - 1, tone + 1); break;
                case System.Windows.Forms.Keys.Up: family = Math.Max(0, family - 1); break;
                case System.Windows.Forms.Keys.Down: family = Math.Min(familyNames.Length - 1, family + 1); break;
                default: return;
            }
            Button target = swatches[family, tone];
            selectedColor = (Color)target.Tag;
            target.Focus();
            RefreshSelection();
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        /// <summary>Сохраняет прежнюю возможность выбрать нестандартный RGB через системный диалог.</summary>
        private void ChooseCustomColor(object sender, EventArgs e)
        {
            using (var dialog = new ColorDialog
            {
                Color = selectedColor,
                AllowFullOpen = true,
                SolidColorOnly = true,
                CustomColors = new[]
                {
                    ToDialogColor(Color.FromArgb(169, 208, 142)),
                    ToDialogColor(Color.FromArgb(248, 203, 173)),
                    ToDialogColor(Color.FromArgb(132, 151, 176)),
                    ToDialogColor(Color.FromArgb(165, 165, 165)),
                    ToDialogColor(Color.FromArgb(255, 230, 153)),
                    ToDialogColor(Color.FromArgb(226, 239, 218)),
                    ToDialogColor(Color.FromArgb(142, 169, 219)),
                    ToDialogColor(Color.FromArgb(255, 217, 102)),
                    ToDialogColor(Color.FromArgb(221, 235, 247))
                }
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                selectedColor = dialog.Color;
                RefreshSelection();
            }
        }

        /// <summary>Обновляет выделение образца, описание и обе панели предпросмотра.</summary>
        private void RefreshSelection()
        {
            string caption = "Исходный цвет: " + ToHex(selectedColor) + " (RGB сохраняется без изменений)";
            for (int family = 0; family < familyNames.Length; family++)
                for (int tone = 0; tone < ThemeVisuals.TaskPaletteToneCount; tone++)
                {
                    Button swatch = swatches[family, tone];
                    bool selected = ((Color)swatch.Tag).ToArgb() == selectedColor.ToArgb();
                    swatch.FlatAppearance.BorderSize = selected ? 3 : 1;
                    swatch.FlatAppearance.BorderColor = selected ? ThemeManager.Palette.Focus :
                        ThemeManager.Palette.Grid;
                    if (selected)
                        caption = familyNames[family] + ", оттенок " + (tone + 1) + " — " +
                            ToHex(selectedColor);
                }
            selectedLabel.Text = caption;
            lightPreview.Invalidate();
            darkPreview.Invalidate();
        }

        /// <summary>Отображает пример полосы, используя тот же алгоритм, что и рабочий календарь.</summary>
        private void PaintPreview(object sender, PaintEventArgs e)
        {
            Panel panel = (Panel)sender;
            ThemeKind kind = (ThemeKind)panel.Tag;
            ThemePalette palette = ThemePalette.For(kind);
            e.Graphics.Clear(palette.Window);
            TextRenderer.DrawText(e.Graphics, kind == ThemeKind.Light ? "Светлая тема" : "Тёмная тема",
                Font, new Rectangle(8, 3, panel.Width - 16, 19), palette.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            Rectangle rect = new Rectangle(8, 27, panel.Width - 17, 28);
            using (GraphicsPath path = ThemeVisuals.TaskPath(rect))
            using (Brush fill = new SolidBrush(ThemeVisuals.TaskDisplayColorInTheme(selectedColor, kind)))
                e.Graphics.FillPath(fill, path);
            TextRenderer.DrawText(e.Graphics, "Пример задачи • 5 рабочих дней", Font, rect,
                ThemePalette.For(ThemeKind.Light).Text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            using (Pen border = new Pen(palette.Grid))
                e.Graphics.DrawRectangle(border, 0, 0, panel.Width - 1, panel.Height - 1);
        }

        /// <summary>Сохраняет кодировку девяти прежних пользовательских цветов ColorDialog.</summary>
        private static int ToDialogColor(Color color)
        {
            return color.R + color.G * 0x100 + color.B * 0x10000;
        }

        /// <summary>Формирует обозначение цвета без потери его численного значения.</summary>
        private static string ToHex(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        /// <summary>Определяет достаточную ширину общей подсказки без обрезки полного RGB.</summary>
        private void SwatchToolTipPopup(object sender, PopupEventArgs e)
        {
            if (e.AssociatedControl == null) return;
            string caption = swatchToolTip.GetToolTip(e.AssociatedControl);
            if (string.IsNullOrEmpty(caption)) return;
            Size textSize = TextRenderer.MeasureText(caption, Font);
            e.ToolTipSize = new Size(textSize.Width + 12, textSize.Height + 8);
        }

        /// <summary>Рисует подсказки оттенков тем же матовым оформлением, что и подсказки календаря.</summary>
        private void SwatchToolTipDraw(object sender, DrawToolTipEventArgs e)
        {
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(palette.Surface);
            using (Pen pen = new Pen(palette.Grid))
                e.Graphics.DrawRectangle(pen, 0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1);
            TextRenderer.DrawText(e.Graphics, e.ToolTipText, Font,
                new Rectangle(6, 4, e.Bounds.Width - 12, e.Bounds.Height - 8), palette.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        /// <summary>Поддерживает переключение тем без сброса выбранного оттенка.</summary>
        private void PaletteThemeChanged(object sender, EventArgs e) { ApplyPaletteTheme(); }

        /// <summary>Оформляет управляющие элементы, не перекрашивая семантические образцы RGB.</summary>
        private void ApplyPaletteTheme()
        {
            ThemePalette palette = ThemeManager.Palette;
            BackColor = palette.Window;
            ForeColor = palette.Text;
            foreach (Label label in labels)
                label.ForeColor = palette.Text;
            foreach (StyledButton button in new[] { selectButton, cancelButton, customButton })
            {
                button.IsAccent = button == selectButton;
                button.Invalidate();
            }
            RefreshSelection();
        }

        /// <summary>Снимает глобальную подписку и освобождает ресурсы закрытого диалога.</summary>
        private void PaletteDisposed(object sender, EventArgs e)
        {
            ThemeManager.Changed -= PaletteThemeChanged;
            Disposed -= PaletteDisposed;
            swatchToolTip.Popup -= SwatchToolTipPopup;
            swatchToolTip.Draw -= SwatchToolTipDraw;
            swatchToolTip.Dispose();
            paletteFont.Dispose();
        }
    }
}
