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
            panel.SuspendLayout();
            SuspendLayout();
            // 
            // buttonOk
            // 
            buttonOk.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            buttonOk.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonOk.FlatAppearance.BorderSize = 5;
            buttonOk.Font = new Font("Segoe UI", 45F, FontStyle.Bold);
            buttonOk.Location = new Point(12, 160);
            buttonOk.Margin = new Padding(0);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new Size(252, 91);
            buttonOk.TabIndex = 0;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += buttonOk_Click;
            // 
            // textBox
            // 
            textBox.BorderStyle = BorderStyle.None;
            textBox.Dock = DockStyle.Fill;
            textBox.Font = new Font("Segoe UI", 16F);
            textBox.Location = new Point(1, 1);
            textBox.Margin = new Padding(0);
            textBox.Multiline = true;
            textBox.Name = "textBox";
            textBox.Size = new Size(246, 128);
            textBox.TabIndex = 1;
            // 
            // panel
            // 
            panel.BackColor = SystemColors.HotTrack;
            panel.BorderStyle = BorderStyle.Fixed3D;
            panel.Controls.Add(textBox);
            panel.Location = new Point(12, 12);
            panel.Margin = new Padding(0);
            panel.Name = "panel";
            panel.Padding = new Padding(1);
            panel.Size = new Size(252, 134);
            panel.TabIndex = 2;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(283, 260);
            Controls.Add(panel);
            Controls.Add(buttonOk);
            Margin = new Padding(2);
            MaximizeBox = false;
            MaximumSize = new Size(299, 299);
            MinimumSize = new Size(299, 299);
            Name = "MainWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Main";
            panel.ResumeLayout(false);
            panel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button buttonOk;
        private TextBox textBox;
        private Panel panel;
    }
}
