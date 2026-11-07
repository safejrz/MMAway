using MMA.Properties;
using System.Xml.Serialization;

namespace MMA
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            buttonOk = new Button();
            textBox = new TextBox();
            panel = new Panel();
            titleBar = new Panel();
            titleLabel = new Label();
            minimizeButton = new Button();
            closeButton = new Button();
            panel.SuspendLayout();
            titleBar.SuspendLayout();
            SuspendLayout();
            // 
            // buttonOk
            // 
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            buttonOk.BackColor = Color.FromArgb(0, 122, 204);
            buttonOk.FlatAppearance.BorderSize = 0;
            buttonOk.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 90, 158);
            buttonOk.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 140, 220);
            buttonOk.FlatStyle = FlatStyle.Flat;
            buttonOk.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            buttonOk.ForeColor = Color.White;
            buttonOk.Location = new Point(12, 418);
            buttonOk.Margin = new Padding(0);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new Size(326, 70);
            buttonOk.TabIndex = 1;
            buttonOk.Text = "Start";
            buttonOk.UseVisualStyleBackColor = false;
            buttonOk.Click += buttonOk_Click;
            // 
            // textBox
            // 
            textBox.AcceptsReturn = true;
            textBox.BackColor = Color.FromArgb(37, 37, 38);
            textBox.BorderStyle = BorderStyle.None;
            textBox.Dock = DockStyle.Fill;
            textBox.Font = new Font("Segoe UI", 11F);
            textBox.ForeColor = Color.FromArgb(204, 204, 204);
            textBox.Location = new Point(1, 1);
            textBox.Margin = new Padding(0);
            textBox.Multiline = true;
            textBox.Name = "textBox";
            textBox.ScrollBars = ScrollBars.None;
            textBox.Size = new Size(300, 300);
            textBox.TabIndex = 0;
            textBox.HideSelection = false;
            textBox.WordWrap = true;
            // 
            // panel
            // 
            panel.BackColor = Color.FromArgb(60, 60, 60);
            panel.BorderStyle = BorderStyle.None;
            panel.Controls.Add(textBox);
            panel.Location = new Point(12, 48);
            panel.Margin = new Padding(0);
            panel.Name = "panel";
            panel.Padding = new Padding(1);
            panel.Size = new Size(326, 352);
            panel.TabIndex = 0;
            // titleBar
            titleBar.BackColor = Color.FromArgb(37, 37, 38);
            titleBar.Controls.Add(titleLabel);
            titleBar.Controls.Add(minimizeButton);
            titleBar.Controls.Add(closeButton);
            titleBar.Location = new Point(0, 0);
            titleBar.Name = "titleBar";
            titleBar.Size = new Size(350, 32);
            titleBar.TabIndex = 2;
            titleBar.MouseDown += titleBar_MouseDown;
            // titleLabel
            titleLabel.Font = new Font("Segoe UI", 9F);
            titleLabel.ForeColor = Color.FromArgb(204, 204, 204);
            titleLabel.Location = new Point(100, 0);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(150, 32);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "MMAway";
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.MouseDown += titleBar_MouseDown;
            // minimizeButton
            minimizeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            minimizeButton.BackColor = Color.FromArgb(37, 37, 38);
            minimizeButton.FlatAppearance.BorderSize = 0;
            minimizeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(70, 70, 70);
            minimizeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60);
            minimizeButton.FlatStyle = FlatStyle.Flat;
            minimizeButton.Font = new Font("Segoe UI", 10F);
            minimizeButton.ForeColor = Color.FromArgb(204, 204, 204);
            minimizeButton.Location = new Point(270, 0);
            minimizeButton.Name = "minimizeButton";
            minimizeButton.Size = new Size(40, 32);
            minimizeButton.TabIndex = 1;
            minimizeButton.Text = "—";
            minimizeButton.UseVisualStyleBackColor = false;
            minimizeButton.Click += minimizeButton_Click;
            // closeButton
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeButton.BackColor = Color.FromArgb(37, 37, 38);
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(160, 35, 29);
            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(196, 43, 28);
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Font = new Font("Segoe UI", 17F);
            closeButton.ForeColor = Color.FromArgb(204, 204, 204);
            closeButton.Location = new Point(310, 0);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(40, 32);
            closeButton.TabIndex = 2;
            closeButton.Text = "×";
            closeButton.UseVisualStyleBackColor = false;
            closeButton.Click += closeButton_Click;
            // MainWindow
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(350, 500);
            Controls.Add(titleBar);
            Controls.Add(panel);
            Controls.Add(buttonOk);
            FormBorderStyle = FormBorderStyle.None;
            ForeColor = Color.FromArgb(204, 204, 204);
            Icon = Resources.MainIcon;
            Margin = new Padding(0);
            MaximizeBox = false;
            MaximumSize = new Size(350, 500);
            MinimumSize = new Size(350, 500);
            Name = "MainWindow";
            StartPosition = FormStartPosition.Manual;
            Text = "MMAway";
            panel.ResumeLayout(false);
            panel.PerformLayout();
            titleBar.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button buttonOk;
        private TextBox textBox;
        private Panel panel;
        private Panel titleBar;
        private Label titleLabel;
        private Button minimizeButton;
        private Button closeButton;
    }
}
