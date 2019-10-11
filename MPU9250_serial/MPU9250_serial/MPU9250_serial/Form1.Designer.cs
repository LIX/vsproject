namespace MPU9250_serial
{
	partial class Form1
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
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
			this.comboBox_ser = new System.Windows.Forms.ComboBox();
			this.button_ser = new System.Windows.Forms.Button();
			this.button_rtbClear = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// richTextBox1
			// 
			this.richTextBox1.Location = new System.Drawing.Point(16, 74);
			this.richTextBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(602, 238);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = "";
			// 
			// comboBox_ser
			// 
			this.comboBox_ser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox_ser.FormattingEnabled = true;
			this.comboBox_ser.Location = new System.Drawing.Point(28, 15);
			this.comboBox_ser.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.comboBox_ser.Name = "comboBox_ser";
			this.comboBox_ser.Size = new System.Drawing.Size(64, 23);
			this.comboBox_ser.TabIndex = 1;
			this.comboBox_ser.MouseClick += new System.Windows.Forms.MouseEventHandler(this.comboBox_ser_MouseClick);
			// 
			// button_ser
			// 
			this.button_ser.Location = new System.Drawing.Point(117, 12);
			this.button_ser.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.button_ser.Name = "button_ser";
			this.button_ser.Size = new System.Drawing.Size(100, 29);
			this.button_ser.TabIndex = 2;
			this.button_ser.Text = "打开";
			this.button_ser.UseVisualStyleBackColor = true;
			this.button_ser.Click += new System.EventHandler(this.button_ser_Click);
			// 
			// button_rtbClear
			// 
			this.button_rtbClear.Location = new System.Drawing.Point(239, 12);
			this.button_rtbClear.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.button_rtbClear.Name = "button_rtbClear";
			this.button_rtbClear.Size = new System.Drawing.Size(100, 29);
			this.button_rtbClear.TabIndex = 3;
			this.button_rtbClear.Text = "清空";
			this.button_rtbClear.UseVisualStyleBackColor = true;
			this.button_rtbClear.Click += new System.EventHandler(this.button_rtbClear_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(649, 328);
			this.Controls.Add(this.button_rtbClear);
			this.Controls.Add(this.button_ser);
			this.Controls.Add(this.comboBox_ser);
			this.Controls.Add(this.richTextBox1);
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.IO.Ports.SerialPort serialPort1;
		private System.Windows.Forms.ComboBox comboBox_ser;
		private System.Windows.Forms.Button button_ser;
		private System.Windows.Forms.Button button_rtbClear;
	}
}

