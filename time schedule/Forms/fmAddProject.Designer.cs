﻿namespace time_schedule
{
    partial class fmAddProject
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
            this.tBxNameProject = new System.Windows.Forms.TextBox();
            this.btnChosePath = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tBxNameProject
            // 
            this.tBxNameProject.Location = new System.Drawing.Point(12, 30);
            this.tBxNameProject.Name = "tBxNameProject";
            this.tBxNameProject.Size = new System.Drawing.Size(283, 20);
            this.tBxNameProject.TabIndex = 0;
            // 
            // btnChosePath
            // 
            this.btnChosePath.Location = new System.Drawing.Point(12, 56);
            this.btnChosePath.Name = "btnChosePath";
            this.btnChosePath.Size = new System.Drawing.Size(130, 23);
            this.btnChosePath.TabIndex = 1;
            this.btnChosePath.Text = "Выбрать проект";
            this.btnChosePath.UseVisualStyleBackColor = true;
            this.btnChosePath.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(220, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Назначте условное обозначение проекта:";
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(148, 56);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(147, 23);
            this.Cancel.TabIndex = 3;
            this.Cancel.Text = "Отмена";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // fmAddProject
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(307, 87);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnChosePath);
            this.Controls.Add(this.tBxNameProject);
            this.Name = "fmAddProject";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Добавте проект";
            this.Load += new System.EventHandler(this.fmAddProject_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tBxNameProject;
        private System.Windows.Forms.Button btnChosePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Cancel;
    }
}