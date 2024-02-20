namespace MMA
{
    partial class Form1
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
            SuspendLayout();
            // 
            // buttonOk
            // 
            buttonOk.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonOk.Dock = DockStyle.Fill;
            buttonOk.Font = new Font("Segoe UI", 48F, FontStyle.Bold);
            buttonOk.Location = new Point(0, 0);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new Size(398, 364);
            buttonOk.TabIndex = 0;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += buttonOk_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(398, 364);
            Controls.Add(buttonOk);
            MaximizeBox = false;
            MaximumSize = new Size(420, 420);
            MinimumSize = new Size(420, 420);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button buttonOk;
    }
}
