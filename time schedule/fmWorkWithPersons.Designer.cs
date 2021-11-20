namespace time_schedule
{
    partial class fmWorkWithPersons
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.btnNewPerson = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnChangePerson = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 12);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(200, 95);
            this.listBox1.TabIndex = 0;
            // 
            // btnNewPerson
            // 
            this.btnNewPerson.Location = new System.Drawing.Point(232, 12);
            this.btnNewPerson.Name = "btnNewPerson";
            this.btnNewPerson.Size = new System.Drawing.Size(142, 29);
            this.btnNewPerson.TabIndex = 1;
            this.btnNewPerson.Text = "Создать исполнителя";
            this.btnNewPerson.UseVisualStyleBackColor = true;
            this.btnNewPerson.Click += new System.EventHandler(this.btnNewPerson_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(232, 47);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(142, 29);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "Удалить исполнителя";
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnChangePerson
            // 
            this.btnChangePerson.Location = new System.Drawing.Point(232, 82);
            this.btnChangePerson.Name = "btnChangePerson";
            this.btnChangePerson.Size = new System.Drawing.Size(142, 29);
            this.btnChangePerson.TabIndex = 3;
            this.btnChangePerson.Text = "Изменить исполнителя";
            this.btnChangePerson.UseVisualStyleBackColor = true;
            // 
            // fmWorkWithPersons
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 137);
            this.Controls.Add(this.btnChangePerson);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnNewPerson);
            this.Controls.Add(this.listBox1);
            this.Name = "fmWorkWithPersons";
            this.Text = "Список исполнителей";
            this.Load += new System.EventHandler(this.fmWorkWithPersons_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button btnNewPerson;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnChangePerson;
    }
}