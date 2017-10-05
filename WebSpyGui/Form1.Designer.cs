namespace WebSpyGui
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.searchPanel = new System.Windows.Forms.Panel();
            this.searchTb = new System.Windows.Forms.TextBox();
            this.searchDropDown = new System.Windows.Forms.ListBox();
            this.searchPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(306, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Search";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // searchPanel
            // 
            this.searchPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.searchPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.searchPanel.Controls.Add(this.searchDropDown);
            this.searchPanel.Controls.Add(this.button1);
            this.searchPanel.Controls.Add(this.searchTb);
            this.searchPanel.Location = new System.Drawing.Point(198, 147);
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Size = new System.Drawing.Size(394, 152);
            this.searchPanel.TabIndex = 6;
            // 
            // searchTb
            // 
            this.searchTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.searchTb.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.searchTb.Location = new System.Drawing.Point(16, 16);
            this.searchTb.Name = "searchTb";
            this.searchTb.Size = new System.Drawing.Size(284, 20);
            this.searchTb.TabIndex = 0;
            this.searchTb.TextChanged += new System.EventHandler(this.onTextChanged);
            // 
            // searchDropDown
            // 
            this.searchDropDown.FormattingEnabled = true;
            this.searchDropDown.Location = new System.Drawing.Point(16, 38);
            this.searchDropDown.Name = "searchDropDown";
            this.searchDropDown.Size = new System.Drawing.Size(284, 95);
            this.searchDropDown.TabIndex = 3;
            this.searchDropDown.Visible = false;
            this.searchDropDown.SelectedIndexChanged += new System.EventHandler(this.searchDropDown_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(833, 458);
            this.Controls.Add(this.searchPanel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.searchPanel.ResumeLayout(false);
            this.searchPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox searchTb;
        private System.Windows.Forms.Panel searchPanel;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ListBox searchDropDown;
    }
}

