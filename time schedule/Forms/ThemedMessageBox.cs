using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Выводит сообщения в палитре TimeSchedule с сохранением результатов WinForms-диалогов.</summary>
    internal static class ThemedMessageBox
    {
        /// <summary>Показывает обычное информационное сообщение.</summary>
        internal static DialogResult Show(string text)
        {
            return ShowCore(null, text, "TimeSchedule", MessageBoxButtons.OK, MessageBoxIcon.None,
                MessageBoxDefaultButton.Button1, (MessageBoxOptions)0);
        }

        /// <summary>Показывает сообщение с заданным заголовком.</summary>
        internal static DialogResult Show(string text, string caption)
        {
            return ShowCore(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None,
                MessageBoxDefaultButton.Button1, (MessageBoxOptions)0);
        }

        /// <summary>Показывает сообщение с выбором действия и соответствующим значком.</summary>
        internal static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return ShowCore(null, text, caption, buttons, icon,
                MessageBoxDefaultButton.Button1, (MessageBoxOptions)0);
        }

        /// <summary>Учитывает указанную кнопку по умолчанию, в том числе безопасный ответ «Нет».</summary>
        internal static DialogResult Show(string text, string caption, MessageBoxButtons buttons,
            MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return ShowCore(null, text, caption, buttons, icon, defaultButton, (MessageBoxOptions)0);
        }

        /// <summary>Показывает сообщение модально относительно указанной формы.</summary>
        internal static DialogResult Show(IWin32Window owner, string text)
        {
            return ShowCore(owner, text, "TimeSchedule", MessageBoxButtons.OK, MessageBoxIcon.None,
                MessageBoxDefaultButton.Button1, (MessageBoxOptions)0);
        }

        /// <summary>Показывает сообщение с привязкой к владельцу и заданными кнопками.</summary>
        internal static DialogResult Show(IWin32Window owner, string text, string caption,
            MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return ShowCore(owner, text, caption, buttons, icon,
                MessageBoxDefaultButton.Button1, (MessageBoxOptions)0);
        }

        /// <summary>Сохраняет кнопку по умолчанию для диалогов подтверждения на форме-владельце.</summary>
        internal static DialogResult Show(IWin32Window owner, string text, string caption,
            MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            return ShowCore(owner, text, caption, buttons, icon, defaultButton, (MessageBoxOptions)0);
        }

        /// <summary>
        /// Сохраняет штатный Windows MessageBox для служебных уведомлений рабочего стола
        /// и не-STA потоков, которым недоступен модальный WinForms-диалог.
        /// </summary>
        private static DialogResult ShowCore(IWin32Window owner, string text, string caption,
            MessageBoxButtons buttons, MessageBoxIcon icon,
            MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
        {
            if (options != (MessageBoxOptions)0 ||
                !Enum.IsDefined(typeof(MessageBoxButtons), buttons) ||
                Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                return owner == null
                    ? System.Windows.Forms.MessageBox.Show(text, caption, buttons, icon, defaultButton, options)
                    : System.Windows.Forms.MessageBox.Show(owner, text, caption, buttons, icon, defaultButton, options);
            }

            IWin32Window dialogOwner = owner ?? Form.ActiveForm;
            Control ownerControl = dialogOwner as Control;
            if (ownerControl != null &&
                (ownerControl.IsDisposed || ownerControl.Disposing ||
                 !ownerControl.IsHandleCreated || ownerControl.InvokeRequired))
            {
                Form1 mainForm = Program.fmMain;
                dialogOwner = mainForm != null &&
                    !mainForm.IsDisposed && !mainForm.Disposing &&
                    mainForm.IsHandleCreated && !mainForm.InvokeRequired
                        ? (IWin32Window)mainForm
                        : null;
            }

            using (var window = new ThemedMessageDialog(text, caption, buttons, icon, defaultButton))
                return dialogOwner == null ? window.ShowDialog() : window.ShowDialog(dialogOwner);
        }
    }

    /// <summary>Общий модальный диалог с адаптивным размером и кнопками в палитре приложения.</summary>
    internal sealed class ThemedMessageDialog : StyledWindowForm
    {
        /// <summary>Поле прокручиваемого текста сообщения.</summary>
        private readonly Panel messageViewport;
        /// <summary>Знак типа сообщения.</summary>
        private readonly Label messageSymbol;
        /// <summary>Шрифт информационного знака, которым владеет диалог.</summary>
        private readonly Font messageSymbolFont;
        /// <summary>Горизонтальная линия над кнопками.</summary>
        private readonly Panel footerDivider;
        /// <summary>Кнопки, соответствующие MessageBoxButtons.</summary>
        private readonly ButtonForShedule[] actionButtons;
        /// <summary>Предпочтительная кнопка для клавиши Enter.</summary>
        private readonly ButtonForShedule defaultAction;
        /// <summary>Безопасный результат закрытия окна без выбора кнопки.</summary>
        private readonly DialogResult dismissResult;
        /// <summary>Тип сообщения, используемый только для оформления знака.</summary>
        private readonly MessageBoxIcon messageIcon;

        /// <summary>Создаёт окно с сохранением результатов, порядка и стандартных клавиш диалога.</summary>
        /// <param name="message">Отображаемое сообщение, включая переносы строк.</param>
        /// <param name="caption">Подпись собственного заголовка.</param>
        /// <param name="buttons">Набор действий, соответствующий WinForms MessageBox.</param>
        /// <param name="icon">Назначение информационного знака.</param>
        /// <param name="defaultButton">Кнопка, активируемая клавишей Enter.</param>
        internal ThemedMessageDialog(string message, string caption, MessageBoxButtons buttons,
            MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            Text = string.IsNullOrWhiteSpace(caption) ? "TimeSchedule" : caption;
            Font = SystemFonts.MessageBoxFont;
            AutoScaleMode = AutoScaleMode.Font;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            messageIcon = icon;

            DialogResult[] results = GetActions(buttons);
            dismissResult = GetDismissResult(results);
            bool hasSymbol = icon != MessageBoxIcon.None;
            int symbolSpace = hasSymbol ? 41 : 0;
            Rectangle workArea = Screen.FromPoint(Cursor.Position).WorkingArea;
            int availableWidth = Math.Max(260, workArea.Width - 40);
            int maxWidth = Math.Min(610, availableWidth);
            int preferredTextWidth = TextRenderer.MeasureText(message ?? string.Empty, Font).Width;
            int contentWidth = Math.Min(maxWidth, Math.Max(Math.Min(340, availableWidth),
                Math.Min(570, preferredTextWidth + symbolSpace + 40)));
            int textWidth = Math.Max(100, contentWidth - 38 - symbolSpace);
            Size measured = TextRenderer.MeasureText(message ?? string.Empty, Font,
                new Size(textWidth, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            int fullTextHeight = Math.Max(34, measured.Height + 12);
            int bodyHeight = Math.Min(Math.Max(66, workArea.Height - 155),
                Math.Min(330, Math.Max(66, fullTextHeight + 16)));
            ClientSize = new Size(contentWidth, bodyHeight + 57);

            messageViewport = new Panel
            {
                Location = new Point(18, 9), Size = new Size(contentWidth - 36, bodyHeight),
                AutoScroll = true, Anchor = AnchorStyles.Top | AnchorStyles.Left |
                    AnchorStyles.Right | AnchorStyles.Bottom
            };
            var messageLabel = new Label
            {
                Location = new Point(symbolSpace, 8), Size = new Size(textWidth, fullTextHeight),
                Text = message ?? string.Empty, UseMnemonic = false
            };
            messageViewport.Controls.Add(messageLabel);
            if (hasSymbol)
            {
                messageSymbolFont = new Font(Font.FontFamily, Font.Size + 6f, FontStyle.Bold);
                messageSymbol = new Label
                {
                    Text = GetSymbol(icon), Location = new Point(0, 3),
                    Size = new Size(30, 34), TextAlign = ContentAlignment.MiddleCenter,
                    Font = messageSymbolFont
                };
                messageViewport.Controls.Add(messageSymbol);
            }
            Controls.Add(messageViewport);

            var footer = new Panel
            {
                Location = new Point(0, bodyHeight + 9), Size = new Size(contentWidth, 48),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            footerDivider = new Panel { Dock = DockStyle.Top, Height = 1 };
            footer.Controls.Add(footerDivider);
            actionButtons = BuildButtons(results, footer);
            defaultAction = actionButtons[Math.Min(GetDefaultIndex(defaultButton), actionButtons.Length - 1)];
            defaultAction.ThemeAccent = true;
            AcceptButton = defaultAction;
            ButtonForShedule cancelAction = actionButtons.FirstOrDefault(button =>
                button.DialogResult == DialogResult.Cancel || button.DialogResult == DialogResult.No);
            if (cancelAction != null) CancelButton = cancelAction;
            Controls.Add(footer);

            ThemeManager.Changed += DialogThemeChanged;
            Disposed += DialogDisposed;
        }

        /// <summary>Возвращает порядок стандартных ответов без преобразования их семантики.</summary>
        private static DialogResult[] GetActions(MessageBoxButtons buttons)
        {
            switch (buttons)
            {
                case MessageBoxButtons.OK: return new[] { DialogResult.OK };
                case MessageBoxButtons.OKCancel: return new[] { DialogResult.OK, DialogResult.Cancel };
                case MessageBoxButtons.YesNo: return new[] { DialogResult.Yes, DialogResult.No };
                case MessageBoxButtons.YesNoCancel:
                    return new[] { DialogResult.Yes, DialogResult.No, DialogResult.Cancel };
                case MessageBoxButtons.RetryCancel: return new[] { DialogResult.Retry, DialogResult.Cancel };
                case MessageBoxButtons.AbortRetryIgnore:
                    return new[] { DialogResult.Abort, DialogResult.Retry, DialogResult.Ignore };
                default: return new[] { DialogResult.OK };
            }
        }

        /// <summary>Использует безопасный ответ при закрытии окна крестиком.</summary>
        private static DialogResult GetDismissResult(DialogResult[] results)
        {
            if (results.Contains(DialogResult.Cancel)) return DialogResult.Cancel;
            if (results.Contains(DialogResult.No)) return DialogResult.No;
            if (results.Contains(DialogResult.Ignore)) return DialogResult.Ignore;
            return results[0];
        }

        /// <summary>Преобразует стандартный ответ в понятный текст кнопки.</summary>
        private static string GetActionText(DialogResult result)
        {
            switch (result)
            {
                case DialogResult.OK: return "ОК";
                case DialogResult.Cancel: return "Отмена";
                case DialogResult.Yes: return "Да";
                case DialogResult.No: return "Нет";
                case DialogResult.Retry: return "Повторить";
                case DialogResult.Abort: return "Прервать";
                case DialogResult.Ignore: return "Пропустить";
                default: return "ОК";
            }
        }

        /// <summary>Определяет кнопку Enter в порядке, переданном WinForms.</summary>
        private static int GetDefaultIndex(MessageBoxDefaultButton defaultButton)
        {
            switch (defaultButton)
            {
                case MessageBoxDefaultButton.Button2: return 1;
                case MessageBoxDefaultButton.Button3: return 2;
                default: return 0;
            }
        }

        /// <summary>Собирает правую группу равномерных кнопок.</summary>
        private static ButtonForShedule[] BuildButtons(DialogResult[] results, Panel footer)
        {
            const int width = 98, gap = 8;
            int firstX = footer.Width - 16 - results.Length * width - (results.Length - 1) * gap;
            var buttons = new ButtonForShedule[results.Length];
            for (int i = 0; i < results.Length; i++)
            {
                buttons[i] = new ButtonForShedule
                {
                    Text = GetActionText(results[i]), DialogResult = results[i],
                    Size = new Size(width, 30), Location = new Point(firstX + i * (width + gap), 9),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    TabIndex = i, UseTheme = true
                };
                footer.Controls.Add(buttons[i]);
            }
            return buttons;
        }

        /// <summary>Возвращает ненавязчивое обозначение назначения сообщения.</summary>
        private static string GetSymbol(MessageBoxIcon icon)
        {
            if (icon == MessageBoxIcon.Error || icon == MessageBoxIcon.Hand || icon == MessageBoxIcon.Stop)
                return "×";
            if (icon == MessageBoxIcon.Warning || icon == MessageBoxIcon.Exclamation)
                return "!";
            if (icon == MessageBoxIcon.Question) return "?";
            return "i";
        }

        /// <summary>Применяет цвет значка и разделителя после общего оформления контролов.</summary>
        private void ApplyDialogAccent()
        {
            footerDivider.BackColor = ThemeManager.Palette.Grid;
            if (messageSymbol == null) return;
            if (messageIcon == MessageBoxIcon.Error || messageIcon == MessageBoxIcon.Hand ||
                messageIcon == MessageBoxIcon.Stop)
                messageSymbol.ForeColor = ThemeManager.Current == ThemeKind.Dark
                    ? Color.FromArgb(242, 137, 147) : Color.FromArgb(172, 57, 76);
            else if (messageIcon == MessageBoxIcon.Warning || messageIcon == MessageBoxIcon.Exclamation)
                messageSymbol.ForeColor = ThemeManager.Current == ThemeKind.Dark
                    ? Color.FromArgb(255, 190, 104) : Color.FromArgb(159, 89, 34);
            else
                messageSymbol.ForeColor = ThemeManager.Palette.Focus;
        }

        /// <summary>Обновляет декоративные цвета при смене темы.</summary>
        private void DialogThemeChanged(object sender, EventArgs e) { ApplyDialogAccent(); }

        /// <summary>После оформления базового окна устанавливает безопасную кнопку по умолчанию.</summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyDialogAccent();
            defaultAction.Select();
        }

        /// <summary>При закрытии без выбора возвращает отказ, а не подтверждение опасного действия.</summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.None) DialogResult = dismissResult;
            base.OnFormClosing(e);
        }

        /// <summary>Освобождает подписку на тему и собственный шрифт значка.</summary>
        private void DialogDisposed(object sender, EventArgs e)
        {
            ThemeManager.Changed -= DialogThemeChanged;
            if (messageSymbolFont != null) messageSymbolFont.Dispose();
            Disposed -= DialogDisposed;
        }
    }
}
