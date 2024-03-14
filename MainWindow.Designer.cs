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
            SuspendLayout();
            // 
            // buttonOk
            // 
            buttonOk.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonOk.Font = new Font("Segoe UI", 48F, FontStyle.Bold);
            buttonOk.Location = new Point(12, 126);
            buttonOk.Margin = new Padding(2);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new Size(260, 92);
            buttonOk.TabIndex = 0;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += buttonOk_Click;
            // 
            // textBox
            // 
            textBox.Font = new Font("Segoe UI", 16F);
            textBox.Location = new Point(12, 12);
            textBox.Multiline = true;
            textBox.Name = "textBox";
            textBox.Size = new Size(259, 109);
            textBox.TabIndex = 1;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(283, 229);
            Controls.Add(textBox);
            Controls.Add(buttonOk);
            Margin = new Padding(2);
            MaximizeBox = false;
            MaximumSize = new Size(299, 268);
            MinimumSize = new Size(299, 268);
            Name = "MainWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Main";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonOk;
        private TextBox textBox;
    }
}
