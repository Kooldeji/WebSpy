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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.searchButton = new System.Windows.Forms.Button();
            this.searchPanel = new System.Windows.Forms.Panel();
            this.searchDropDown = new System.Windows.Forms.ListBox();
            this.searchTb = new System.Windows.Forms.TextBox();
            this.searchPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // searchButton
            // 
            this.searchButton.Font = new System.Drawing.Font("Diavlo", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchButton.Location = new System.Drawing.Point(363, 16);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(75, 27);
            this.searchButton.TabIndex = 2;
            this.searchButton.Text = "Search";
            this.searchButton.UseVisualStyleBackColor = true;
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // searchPanel
            // 
            this.searchPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.searchPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.searchPanel.BackColor = System.Drawing.Color.Transparent;
            this.searchPanel.Controls.Add(this.searchDropDown);
            this.searchPanel.Controls.Add(this.searchButton);
            this.searchPanel.Controls.Add(this.searchTb);
            this.searchPanel.Location = new System.Drawing.Point(193, 155);
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Size = new System.Drawing.Size(452, 275);
            this.searchPanel.TabIndex = 6;
            this.searchPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.searchPanel_Paint);
            // 
            // searchDropDown
            // 
            this.searchDropDown.Font = new System.Drawing.Font("Montserrat", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchDropDown.FormattingEnabled = true;
            this.searchDropDown.ItemHeight = 26;
            this.searchDropDown.Location = new System.Drawing.Point(15, 45);
            this.searchDropDown.Name = "searchDropDown";
            this.searchDropDown.Size = new System.Drawing.Size(342, 212);
            this.searchDropDown.TabIndex = 3;
            this.searchDropDown.Visible = false;
            this.searchDropDown.SelectedIndexChanged += new System.EventHandler(this.searchDropDown_SelectedIndexChanged);
            this.searchDropDown.Leave += new System.EventHandler(this.searchTb_Leave);
            // 
            // searchTb
            // 
            this.searchTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.searchTb.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.searchTb.Font = new System.Drawing.Font("Montserrat", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchTb.Location = new System.Drawing.Point(16, 16);
            this.searchTb.Name = "searchTb";
            this.searchTb.Size = new System.Drawing.Size(341, 29);
            this.searchTb.TabIndex = 0;
            this.searchTb.TextChanged += new System.EventHandler(this.onTextChanged);
            this.searchTb.Leave += new System.EventHandler(this.searchTb_Leave);
            this.searchTb.MouseLeave += new System.EventHandler(this.searchTb_MouseLeave);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(833, 458);
            this.Controls.Add(this.searchPanel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Click += new System.EventHandler(this.Form1_Click);
            this.searchPanel.ResumeLayout(false);
            this.searchPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.TextBox searchTb;
        private System.Windows.Forms.Panel searchPanel;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ListBox searchDropDown;
    }
}

