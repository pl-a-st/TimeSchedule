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
            this.lBxPersons = new System.Windows.Forms.ListBox();
            this.btnNewPerson = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnChangePerson = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lBxPersons
            // 
            this.lBxPersons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lBxPersons.FormattingEnabled = true;
            this.lBxPersons.Location = new System.Drawing.Point(12, 12);
            this.lBxPersons.Name = "lBxPersons";
            this.lBxPersons.Size = new System.Drawing.Size(200, 186);
            this.lBxPersons.TabIndex = 0;
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
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnChangePerson
            // 
            this.btnChangePerson.Location = new System.Drawing.Point(232, 82);
            this.btnChangePerson.Name = "btnChangePerson";
            this.btnChangePerson.Size = new System.Drawing.Size(142, 29);
            this.btnChangePerson.TabIndex = 3;
            this.btnChangePerson.Text = "Изменить исполнителя";
            this.btnChangePerson.UseVisualStyleBackColor = true;
            this.btnChangePerson.Click += new System.EventHandler(this.btnChangePerson_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(244, 168);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // fmWorkWithPersons
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 239);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnChangePerson);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnNewPerson);
            this.Controls.Add(this.lBxPersons);
            this.Name = "fmWorkWithPersons";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Список исполнителей";
            this.Load += new System.EventHandler(this.fmWorkWithPersons_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lBxPersons;
        private System.Windows.Forms.Button btnNewPerson;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnChangePerson;
        private System.Windows.Forms.Button button1;
    }
}