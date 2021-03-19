namespace C_4
{
    partial class CFourm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CFourm));
            this.web_problem = new System.Windows.Forms.WebBrowser();
            this.btn_close = new System.Windows.Forms.Button();
            this.btn_minimize = new System.Windows.Forms.Button();
            this.btn_maximize = new System.Windows.Forms.Button();
            this.btn_upload = new System.Windows.Forms.Button();
            this.btn_reload = new System.Windows.Forms.Button();
            this.vsb_scroll = new System.Windows.Forms.VScrollBar();
            this.bgw_reload = new System.ComponentModel.BackgroundWorker();
            this.bgw_upload = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // web_problem
            // 
            this.web_problem.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.web_problem.IsWebBrowserContextMenuEnabled = false;
            this.web_problem.Location = new System.Drawing.Point(154, 29);
            this.web_problem.MinimumSize = new System.Drawing.Size(20, 20);
            this.web_problem.Name = "web_problem";
            this.web_problem.Size = new System.Drawing.Size(450, 375);
            this.web_problem.TabIndex = 6;
            this.web_problem.WebBrowserShortcutsEnabled = false;
            this.web_problem.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.web_problem_Navigating);
            // 
            // btn_close
            // 
            this.btn_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_close.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(12)))), ((int)(((byte)(4)))));
            this.btn_close.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.btn_close.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btn_close.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btn_close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_close.Font = new System.Drawing.Font("Arial Rounded MT Bold", 14F);
            this.btn_close.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btn_close.Location = new System.Drawing.Point(579, 2);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(25, 25);
            this.btn_close.TabIndex = 2;
            this.btn_close.Text = "×";
            this.btn_close.UseCompatibleTextRendering = true;
            this.btn_close.UseVisualStyleBackColor = false;
            this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
            // 
            // btn_minimize
            // 
            this.btn_minimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_minimize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(0)))));
            this.btn_minimize.FlatAppearance.BorderColor = System.Drawing.Color.Yellow;
            this.btn_minimize.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(0)))));
            this.btn_minimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(0)))));
            this.btn_minimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_minimize.Font = new System.Drawing.Font("Arial Rounded MT Bold", 14F);
            this.btn_minimize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(76)))), ((int)(((byte)(0)))));
            this.btn_minimize.Location = new System.Drawing.Point(521, 2);
            this.btn_minimize.Name = "btn_minimize";
            this.btn_minimize.Size = new System.Drawing.Size(25, 25);
            this.btn_minimize.TabIndex = 0;
            this.btn_minimize.Text = "−";
            this.btn_minimize.UseCompatibleTextRendering = true;
            this.btn_minimize.UseVisualStyleBackColor = false;
            this.btn_minimize.Click += new System.EventHandler(this.btn_minimize_Click);
            // 
            // btn_maximize
            // 
            this.btn_maximize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_maximize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(196)))), ((int)(((byte)(4)))));
            this.btn_maximize.FlatAppearance.BorderColor = System.Drawing.Color.Lime;
            this.btn_maximize.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkGreen;
            this.btn_maximize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(164)))), ((int)(((byte)(0)))));
            this.btn_maximize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_maximize.Font = new System.Drawing.Font("Arial Rounded MT Bold", 14F);
            this.btn_maximize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(76)))), ((int)(((byte)(0)))));
            this.btn_maximize.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btn_maximize.Location = new System.Drawing.Point(550, 2);
            this.btn_maximize.Name = "btn_maximize";
            this.btn_maximize.Size = new System.Drawing.Size(25, 25);
            this.btn_maximize.TabIndex = 1;
            this.btn_maximize.Text = "+";
            this.btn_maximize.UseCompatibleTextRendering = true;
            this.btn_maximize.UseVisualStyleBackColor = false;
            this.btn_maximize.Click += new System.EventHandler(this.btn_maximize_Click);
            // 
            // btn_upload
            // 
            this.btn_upload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_upload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(196)))), ((int)(((byte)(4)))));
            this.btn_upload.FlatAppearance.BorderColor = System.Drawing.Color.Lime;
            this.btn_upload.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkGreen;
            this.btn_upload.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(164)))), ((int)(((byte)(0)))));
            this.btn_upload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_upload.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.btn_upload.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(76)))), ((int)(((byte)(0)))));
            this.btn_upload.Location = new System.Drawing.Point(4, 379);
            this.btn_upload.Name = "btn_upload";
            this.btn_upload.Size = new System.Drawing.Size(150, 25);
            this.btn_upload.TabIndex = 5;
            this.btn_upload.Text = "UPLOAD CODE FILE";
            this.btn_upload.UseVisualStyleBackColor = false;
            this.btn_upload.Click += new System.EventHandler(this.btn_upload_Click);
            // 
            // btn_reload
            // 
            this.btn_reload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_reload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(0)))));
            this.btn_reload.FlatAppearance.BorderColor = System.Drawing.Color.Yellow;
            this.btn_reload.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(0)))));
            this.btn_reload.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(0)))));
            this.btn_reload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_reload.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.btn_reload.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(76)))), ((int)(((byte)(0)))));
            this.btn_reload.Location = new System.Drawing.Point(4, 354);
            this.btn_reload.Name = "btn_reload";
            this.btn_reload.Size = new System.Drawing.Size(150, 25);
            this.btn_reload.TabIndex = 4;
            this.btn_reload.Text = "RELOAD PROBLEMS";
            this.btn_reload.UseVisualStyleBackColor = false;
            this.btn_reload.Click += new System.EventHandler(this.btn_reload_Click);
            // 
            // vsb_scroll
            // 
            this.vsb_scroll.Location = new System.Drawing.Point(4, 29);
            this.vsb_scroll.Name = "vsb_scroll";
            this.vsb_scroll.Size = new System.Drawing.Size(20, 325);
            this.vsb_scroll.SmallChange = 3;
            this.vsb_scroll.TabIndex = 0;
            this.vsb_scroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vsb_scroll_Scroll);
            // 
            // backgroundWorker1
            // 
            this.bgw_reload.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgw_reload_DoWork);
            this.bgw_reload.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgw_reload_RunWorkerCompleted);
            // 
            // bgw_upload
            // 
            this.bgw_upload.WorkerReportsProgress = true;
            this.bgw_upload.WorkerSupportsCancellation = true;
            this.bgw_upload.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgw_upload_DoWork);
            this.bgw_upload.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgw_upload_ProgressChanged);
            this.bgw_upload.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgw_upload_RunWorkerCompleted);
            // 
            // CFourm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(76)))), ((int)(((byte)(76)))));
            this.ClientSize = new System.Drawing.Size(608, 408);
            this.Controls.Add(this.vsb_scroll);
            this.Controls.Add(this.btn_reload);
            this.Controls.Add(this.btn_upload);
            this.Controls.Add(this.btn_maximize);
            this.Controls.Add(this.btn_minimize);
            this.Controls.Add(this.btn_close);
            this.Controls.Add(this.web_problem);
            this.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(196)))), ((int)(((byte)(64)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(608, 408);
            this.Name = "CFourm";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.Text = "6";
            this.MouseCaptureChanged += new System.EventHandler(this.CFourm_MouseCaptureChanged);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CFourm_MouseDown);
            this.MouseEnter += new System.EventHandler(this.CFourm_MouseOver);
            this.MouseLeave += new System.EventHandler(this.CFourm_MouseCaptureChanged);
            this.MouseHover += new System.EventHandler(this.CFourm_MouseOver);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CFourm_MouseOver);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CFourm_MouseUp);
            this.Resize += new System.EventHandler(this.CFourm_Resize);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.WebBrowser web_problem;
        private System.Windows.Forms.Button btn_close;
        private System.Windows.Forms.Button btn_minimize;
        private System.Windows.Forms.Button btn_maximize;
        private System.Windows.Forms.Button btn_upload;
        private System.Windows.Forms.Button btn_reload;
        private System.Windows.Forms.VScrollBar vsb_scroll;
        private System.ComponentModel.BackgroundWorker bgw_reload;
        private System.ComponentModel.BackgroundWorker bgw_upload;
    }
}

