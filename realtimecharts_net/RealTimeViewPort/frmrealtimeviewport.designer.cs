namespace ChartDirectorSampleCode
{
    partial class FrmRealTimeViewPort
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmRealTimeViewPort));
            this.winChartViewer1 = new ChartDirector.WinChartViewer();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.savePB = new System.Windows.Forms.Button();
            this.pointerPB = new System.Windows.Forms.RadioButton();
            this.zoomInPB = new System.Windows.Forms.RadioButton();
            this.zoomOutPB = new System.Windows.Forms.RadioButton();
            this.separator = new System.Windows.Forms.Label();
            this.topLabel = new System.Windows.Forms.Label();
            this.chartUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.viewPortControl1 = new ChartDirector.WinViewPortControl(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.winChartViewer1)).BeginInit();
            this.leftPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.viewPortControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // winChartViewer1
            // 
            this.winChartViewer1.Location = new System.Drawing.Point(123, 27);
            this.winChartViewer1.Name = "winChartViewer1";
            this.winChartViewer1.Size = new System.Drawing.Size(640, 350);
            this.winChartViewer1.TabIndex = 27;
            this.winChartViewer1.TabStop = false;
            this.winChartViewer1.MouseMovePlotArea += new System.Windows.Forms.MouseEventHandler(this.winChartViewer1_MouseMovePlotArea);
            this.winChartViewer1.ViewPortChanged += new ChartDirector.WinViewPortEventHandler(this.winChartViewer1_ViewPortChanged);
            // 
            // leftPanel
            // 
            this.leftPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.leftPanel.Controls.Add(this.savePB);
            this.leftPanel.Controls.Add(this.pointerPB);
            this.leftPanel.Controls.Add(this.zoomInPB);
            this.leftPanel.Controls.Add(this.zoomOutPB);
            this.leftPanel.Controls.Add(this.separator);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPanel.Location = new System.Drawing.Point(0, 24);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Size = new System.Drawing.Size(120, 431);
            this.leftPanel.TabIndex = 25;
            // 
            // savePB
            // 
            this.savePB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.savePB.Image = ((System.Drawing.Image)(resources.GetObject("savePB.Image")));
            this.savePB.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.savePB.Location = new System.Drawing.Point(0, 109);
            this.savePB.Name = "savePB";
            this.savePB.Size = new System.Drawing.Size(120, 28);
            this.savePB.TabIndex = 3;
            this.savePB.Text = "      Save";
            this.savePB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.savePB.Click += new System.EventHandler(this.savePB_Click);
            // 
            // pointerPB
            // 
            this.pointerPB.Appearance = System.Windows.Forms.Appearance.Button;
            this.pointerPB.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pointerPB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pointerPB.Image = ((System.Drawing.Image)(resources.GetObject("pointerPB.Image")));
            this.pointerPB.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.pointerPB.Location = new System.Drawing.Point(0, -1);
            this.pointerPB.Name = "pointerPB";
            this.pointerPB.Size = new System.Drawing.Size(120, 28);
            this.pointerPB.TabIndex = 0;
            this.pointerPB.Text = "      Pointer";
            this.pointerPB.CheckedChanged += new System.EventHandler(this.pointerPB_CheckedChanged);
            // 
            // zoomInPB
            // 
            this.zoomInPB.Appearance = System.Windows.Forms.Appearance.Button;
            this.zoomInPB.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.zoomInPB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.zoomInPB.Image = ((System.Drawing.Image)(resources.GetObject("zoomInPB.Image")));
            this.zoomInPB.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.zoomInPB.Location = new System.Drawing.Point(0, 26);
            this.zoomInPB.Name = "zoomInPB";
            this.zoomInPB.Size = new System.Drawing.Size(120, 28);
            this.zoomInPB.TabIndex = 1;
            this.zoomInPB.Text = "      Zoom In";
            this.zoomInPB.CheckedChanged += new System.EventHandler(this.zoomInPB_CheckedChanged);
            // 
            // zoomOutPB
            // 
            this.zoomOutPB.Appearance = System.Windows.Forms.Appearance.Button;
            this.zoomOutPB.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.zoomOutPB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.zoomOutPB.Image = ((System.Drawing.Image)(resources.GetObject("zoomOutPB.Image")));
            this.zoomOutPB.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.zoomOutPB.Location = new System.Drawing.Point(0, 53);
            this.zoomOutPB.Name = "zoomOutPB";
            this.zoomOutPB.Size = new System.Drawing.Size(120, 28);
            this.zoomOutPB.TabIndex = 2;
            this.zoomOutPB.Text = "      Zoom Out";
            this.zoomOutPB.CheckedChanged += new System.EventHandler(this.zoomOutPB_CheckedChanged);
            // 
            // separator
            // 
            this.separator.BackColor = System.Drawing.Color.Black;
            this.separator.Dock = System.Windows.Forms.DockStyle.Right;
            this.separator.Location = new System.Drawing.Point(119, 0);
            this.separator.Name = "separator";
            this.separator.Size = new System.Drawing.Size(1, 431);
            this.separator.TabIndex = 31;
            // 
            // topLabel
            // 
            this.topLabel.BackColor = System.Drawing.Color.Navy;
            this.topLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topLabel.Font = new System.Drawing.Font("Arial", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.topLabel.ForeColor = System.Drawing.Color.Yellow;
            this.topLabel.Location = new System.Drawing.Point(0, 0);
            this.topLabel.Name = "topLabel";
            this.topLabel.Size = new System.Drawing.Size(765, 24);
            this.topLabel.TabIndex = 26;
            this.topLabel.Text = "Advanced Software Engineering";
            this.topLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chartUpdateTimer
            // 
            this.chartUpdateTimer.Enabled = true;
            this.chartUpdateTimer.Tick += new System.EventHandler(this.chartUpdateTimer_Tick);
            // 
            // viewPortControl1
            // 
            this.viewPortControl1.Location = new System.Drawing.Point(123, 383);
            this.viewPortControl1.Name = "viewPortControl1";
            this.viewPortControl1.Size = new System.Drawing.Size(640, 60);
            this.viewPortControl1.TabIndex = 28;
            this.viewPortControl1.TabStop = false;
            this.viewPortControl1.Viewer = this.winChartViewer1;
            // 
            // FrmRealTimeViewPort
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(765, 455);
            this.Controls.Add(this.viewPortControl1);
            this.Controls.Add(this.winChartViewer1);
            this.Controls.Add(this.leftPanel);
            this.Controls.Add(this.topLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmRealTimeViewPort";
            this.Text = "Real-Time Chart with Viewport Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmRealTimeViewPort_FormClosing);
            this.Load += new System.EventHandler(this.FrmRealtimeViewPort_Load);
            ((System.ComponentModel.ISupportInitialize)(this.winChartViewer1)).EndInit();
            this.leftPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.viewPortControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ChartDirector.WinChartViewer winChartViewer1;
        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.Label separator;
        private System.Windows.Forms.Label topLabel;
        private System.Windows.Forms.RadioButton pointerPB;
        private System.Windows.Forms.RadioButton zoomInPB;
        private System.Windows.Forms.RadioButton zoomOutPB;
        internal System.Windows.Forms.Timer chartUpdateTimer;
        private System.Windows.Forms.Button savePB;
        private ChartDirector.WinViewPortControl viewPortControl1;
    }
}