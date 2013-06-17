namespace StockNotification.Gui
{
    partial class frmMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnImport = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbEmail = new System.Windows.Forms.TextBox();
            this.tbUserName = new System.Windows.Forms.TextBox();
            this.tbStocks = new System.Windows.Forms.TextBox();
            this.lbvEmail = new System.Windows.Forms.Label();
            this.lbvStocks = new System.Windows.Forms.Label();
            this.lbvUserName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(172, 352);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(100, 23);
            this.btnImport.TabIndex = 3;
            this.btnImport.Text = "Import Stocks";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "User Name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "Email:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "Stocks:";
            // 
            // tbEmail
            // 
            this.tbEmail.Location = new System.Drawing.Point(81, 33);
            this.tbEmail.Name = "tbEmail";
            this.tbEmail.Size = new System.Drawing.Size(191, 21);
            this.tbEmail.TabIndex = 1;
            this.tbEmail.Text = "56472190@qq.com";
            // 
            // tbUserName
            // 
            this.tbUserName.Location = new System.Drawing.Point(81, 6);
            this.tbUserName.Name = "tbUserName";
            this.tbUserName.Size = new System.Drawing.Size(110, 21);
            this.tbUserName.TabIndex = 0;
            this.tbUserName.Text = "huangchao";
            this.tbUserName.Leave += new System.EventHandler(this.tbUserName_Leave);
            // 
            // tbStocks
            // 
            this.tbStocks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbStocks.Location = new System.Drawing.Point(81, 60);
            this.tbStocks.Multiline = true;
            this.tbStocks.Name = "tbStocks";
            this.tbStocks.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbStocks.Size = new System.Drawing.Size(191, 286);
            this.tbStocks.TabIndex = 2;
            this.tbStocks.Text = "^IXIC\r\n^GSPC\r\nYY\r\nDDD\r\nFLT\r\nLNKD\r\nNED\r\nQIHU\r\nTCK";
            // 
            // lbvEmail
            // 
            this.lbvEmail.ForeColor = System.Drawing.Color.Red;
            this.lbvEmail.Location = new System.Drawing.Point(278, 36);
            this.lbvEmail.Name = "lbvEmail";
            this.lbvEmail.Size = new System.Drawing.Size(23, 23);
            this.lbvEmail.TabIndex = 5;
            this.lbvEmail.Text = "*";
            this.lbvEmail.Visible = false;
            // 
            // lbvStocks
            // 
            this.lbvStocks.ForeColor = System.Drawing.Color.Red;
            this.lbvStocks.Location = new System.Drawing.Point(278, 63);
            this.lbvStocks.Name = "lbvStocks";
            this.lbvStocks.Size = new System.Drawing.Size(23, 23);
            this.lbvStocks.TabIndex = 6;
            this.lbvStocks.Text = "*";
            this.lbvStocks.Visible = false;
            // 
            // lbvUserName
            // 
            this.lbvUserName.ForeColor = System.Drawing.Color.Red;
            this.lbvUserName.Location = new System.Drawing.Point(197, 9);
            this.lbvUserName.Name = "lbvUserName";
            this.lbvUserName.Size = new System.Drawing.Size(23, 23);
            this.lbvUserName.TabIndex = 7;
            this.lbvUserName.Text = "*";
            this.lbvUserName.Visible = false;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(299, 387);
            this.Controls.Add(this.lbvUserName);
            this.Controls.Add(this.lbvStocks);
            this.Controls.Add(this.lbvEmail);
            this.Controls.Add(this.tbStocks);
            this.Controls.Add(this.tbUserName);
            this.Controls.Add(this.tbEmail);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnImport);
            this.MaximumSize = new System.Drawing.Size(315, 425);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "简易管理界面";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbEmail;
        private System.Windows.Forms.TextBox tbUserName;
        private System.Windows.Forms.TextBox tbStocks;
        private System.Windows.Forms.Label lbvEmail;
        private System.Windows.Forms.Label lbvStocks;
        private System.Windows.Forms.Label lbvUserName;
    }
}

