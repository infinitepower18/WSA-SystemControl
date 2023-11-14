using Microsoft.Win32;
using System.Reflection;
using System.Resources;

namespace WSA_System_Control
{
    partial class About
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
            ResourceManager rm = new ResourceManager("WSA_System_Control.Resources.Strings", Assembly.GetExecutingAssembly());
            button1 = new Button();
            button2 = new Button();
            linkLabel1 = new LinkLabel();
            label1 = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(398, 88);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = rm.GetString("OK");
            button1.UseVisualStyleBackColor = true;
            if (isDarkMode()) {
                button1.BackColor = ColorTranslator.FromHtml("#FF2D2D30");
            }
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(317, 88);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 1;
            button2.Text = rm.GetString("GitHub");
            button2.UseVisualStyleBackColor = true;
            if (isDarkMode())
            {
                button2.BackColor = ColorTranslator.FromHtml("#FF2D2D30");
            }
            button2.Click += button2_Click;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(12, 92);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(45, 15);
            linkLabel1.TabIndex = 2;
            linkLabel1.TabStop = true;
            linkLabel1.Text = rm.GetString("Donate");
            if (isDarkMode())
            {
                linkLabel1.LinkColor = Color.White;
            }
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 22);
            label1.Name = "label1";
            label1.Size = new Size(544, 45);
            label1.TabIndex = 3;
            label1.Text = rm.GetString("AboutDescription");
            Version appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            label1.Text = String.Format(label1.Text,appVersion.Major + "." + appVersion.Minor + "." + appVersion.Build);
            label1.MaximumSize = new Size(450, 0);
            // 
            // About
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(485, 118);
            Controls.Add(label1);
            Controls.Add(linkLabel1);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = rm.GetString("About");
            Text = rm.GetString("About");
            ResumeLayout(false);
            PerformLayout();
        }

        private bool isDarkMode()
        {
            int res = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", -1);
            if (res == 0)
            {
                return true;
            }
            return false;
        }

        #endregion

        private Button button1;
        private Button button2;
        private LinkLabel linkLabel1;
        private Label label1;
    }
}