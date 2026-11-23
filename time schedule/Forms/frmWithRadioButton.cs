using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule
{
    public enum WasClickInfrmWithRadioButton
    {
        Ok,
        Cancel
    }
    public partial class frmWithRadioButton : StyledWindowForm
    {
        public WasClickInfrmWithRadioButton WasClickInfrmWithRadioButton
        {
            get;
            private set;
        } = WasClickInfrmWithRadioButton.Cancel;
        public int CoutnRaioButton
        {
            get;
            private set;
        } = 0;
        /// <summary>Динамические варианты выбора в порядке добавления.</summary>
        private readonly List<RadioButton> choiceButtons = new List<RadioButton>();

        /// <summary>Связывает Enter и Esc с существующими действиями подтверждения и отмены.</summary>
        public frmWithRadioButton()
        {
            InitializeComponent();
            AcceptButton = butOk;
            CancelButton = butCancel;
        }

        /// <summary>Учитывает итоговые размеры системного шрифта после оформления формы.</summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ArrangeChoiceControls();
        }

        private void frmWithRadioButton_Load(object sender, EventArgs e)
        {
            if (choiceButtons.Count > 0)
                choiceButtons[0].Checked = true;
        }

        public void SetLable(string text)
        {
            label1.Text = text;
            ArrangeChoiceControls();
        }

        public void AddRaioButton(string TextRadioButton)
        {
            RadioButton radioButton = new RadioButton
            {
                AutoSize = true,
                Text = TextRadioButton,
                TabIndex = CoutnRaioButton
            };
            WindowContentControls.Add(radioButton);
            choiceButtons.Add(radioButton);
            CoutnRaioButton++;
            ArrangeChoiceControls();
        }

        /// <summary>Расставляет динамические варианты и кнопки по фактическим размерам текста, включая масштабирование Windows.</summary>
        private void ArrangeChoiceControls()
        {
            if (label1 == null || butOk == null || butCancel == null) return;
            Control content = butOk.Parent;
            if (content == null) return;
            int availableWidth = Math.Max(120, content.ClientSize.Width - 24);
            label1.MaximumSize = new Size(availableWidth, 0);
            int nextTop = label1.Bottom + 9;
            foreach (RadioButton choice in choiceButtons)
            {
                choice.MaximumSize = new Size(availableWidth, 0);
                choice.Location = new Point(12, nextTop);
                nextTop = choice.Bottom + 5;
            }
            int buttonTop = choiceButtons.Count > 0
                ? nextTop + 7
                : Math.Max(44, label1.Bottom + 18);
            butOk.Top = buttonTop;
            butCancel.Top = buttonTop;
            butOk.TabIndex = choiceButtons.Count;
            butCancel.TabIndex = choiceButtons.Count + 1;
            int contentHeight = Math.Max(butOk.Bottom, butCancel.Bottom) + 11;
            int headerHeight = ReferenceEquals(content, this) ? 0 : WindowChromeHeight;
            int height = contentHeight + headerHeight;
            if (ClientSize.Height != height)
                ClientSize = new Size(ClientSize.Width, height);
        }
        public string GetTextSelectedRadio()
        {
            string textSelectedRadio = string.Empty;
            foreach (Control control in WindowContentControls)
            {
                if (control is RadioButton)
                {
                    if ((control as RadioButton).Checked)
                    {
                        textSelectedRadio = (control as RadioButton).Text;
                    }
                }
            }
            return textSelectedRadio;
        }
        private void butOk_Click(object sender, EventArgs e)
        {
            WasClickInfrmWithRadioButton = WasClickInfrmWithRadioButton.Ok;
            this.Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            WasClickInfrmWithRadioButton = WasClickInfrmWithRadioButton.Cancel;
            this.Close();
        }
    }
}
