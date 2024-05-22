namespace Prism
{
    partial class Main
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
            ControlButton = new Button();
            LoggingSink = new ListBox();
            SuspendLayout();
            // 
            // ControlButton
            // 
            ControlButton.BackColor = Color.FromArgb(46, 52, 64);
            ControlButton.Dock = DockStyle.Bottom;
            ControlButton.FlatAppearance.BorderSize = 0;
            ControlButton.FlatStyle = FlatStyle.Flat;
            ControlButton.Font = new Font("Segoe MDL2 Assets", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ControlButton.ForeColor = Color.White;
            ControlButton.Location = new Point(0, 550);
            ControlButton.Name = "ControlButton";
            ControlButton.Size = new Size(400, 50);
            ControlButton.TabIndex = 2;
            ControlButton.Text = "Start";
            ControlButton.UseVisualStyleBackColor = false;
            ControlButton.Click += ChangeState;
            // 
            // LoggingSink
            // 
            LoggingSink.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LoggingSink.BackColor = Color.FromArgb(46, 52, 64);
            LoggingSink.BorderStyle = BorderStyle.None;
            LoggingSink.Font = new Font("Consolas", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            LoggingSink.ForeColor = Color.White;
            LoggingSink.FormattingEnabled = true;
            LoggingSink.HorizontalScrollbar = true;
            LoggingSink.ItemHeight = 19;
            LoggingSink.Location = new Point(0, 0);
            LoggingSink.Margin = new Padding(10);
            LoggingSink.Name = "LoggingSink";
            LoggingSink.Size = new Size(400, 551);
            LoggingSink.TabIndex = 0;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(46, 52, 64);
            ClientSize = new Size(400, 600);
            Controls.Add(ControlButton);
            Controls.Add(LoggingSink);
            Name = "Main";
            Text = "Prism";
            ResumeLayout(false);
        }

        #endregion
        private Button ControlButton;
        private ListBox LoggingSink;
    }
}
