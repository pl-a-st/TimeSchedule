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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.lblPersonPlace = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lBxPersons
            // 
            this.lBxPersons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lBxPersons.FormattingEnabled = true;
            this.lBxPersons.Location = new System.Drawing.Point(12, 12);
            this.lBxPersons.Name = "lBxPersons";
            this.lBxPersons.Size = new System.Drawing.Size(243, 212);
            this.lBxPersons.TabIndex = 0;
            // 
            // btnNewPerson
            // 
            this.btnNewPerson.Location = new System.Drawing.Point(261, 12);
            this.btnNewPerson.Name = "btnNewPerson";
            this.btnNewPerson.Size = new System.Drawing.Size(142, 29);
            this.btnNewPerson.TabIndex = 1;
            this.btnNewPerson.Text = "Создать исполнителя";
            this.btnNewPerson.UseVisualStyleBackColor = true;
            this.btnNewPerson.Click += new System.EventHandler(this.btnNewPerson_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(261, 47);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(142, 29);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "Удалить исполнителя";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnChangePerson
            // 
            this.btnChangePerson.Location = new System.Drawing.Point(261, 82);
            this.btnChangePerson.Name = "btnChangePerson";
            this.btnChangePerson.Size = new System.Drawing.Size(142, 29);
            this.btnChangePerson.TabIndex = 3;
            this.btnChangePerson.Text = "Изменить исполнителя";
            this.btnChangePerson.UseVisualStyleBackColor = true;
            this.btnChangePerson.Click += new System.EventHandler(this.btnChangePerson_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(30, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "▲";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblPersonPlace);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Location = new System.Drawing.Point(261, 134);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(142, 90);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 57);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(30, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "▼";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // lblPersonPlace
            // 
            this.lblPersonPlace.AutoSize = true;
            this.lblPersonPlace.Location = new System.Drawing.Point(42, 38);
            this.lblPersonPlace.Name = "lblPersonPlace";
            this.lblPersonPlace.Size = new System.Drawing.Size(105, 13);
            this.lblPersonPlace.TabIndex = 5;
            this.lblPersonPlace.Text = "Заменяемый текст";
            // 
            // fmWorkWithPersons
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 239);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnChangePerson);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnNewPerson);
            this.Controls.Add(this.lBxPersons);
            this.Name = "fmWorkWithPersons";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Список исполнителей";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.fmWorkWithPersons_FormClosed);
            this.Load += new System.EventHandler(this.fmWorkWithPersons_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lBxPersons;
        private System.Windows.Forms.Button btnNewPerson;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnChangePerson;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label lblPersonPlace;
    }
}