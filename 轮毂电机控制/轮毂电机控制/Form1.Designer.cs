namespace 轮毂电机控制
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
			this.button_connect = new System.Windows.Forms.Button();
			this.comboBox_CANIndex = new System.Windows.Forms.ComboBox();
			this.textBox_AccCode = new System.Windows.Forms.TextBox();
			this.textBox_AccMask = new System.Windows.Forms.TextBox();
			this.comboBox_Baudrate = new System.Windows.Forms.ComboBox();
			this.comboBox_Filter = new System.Windows.Forms.ComboBox();
			this.comboBox_Mode = new System.Windows.Forms.ComboBox();
			this.timer_rec = new System.Windows.Forms.Timer(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.textBox_speed_LF = new System.Windows.Forms.TextBox();
			this.textBox_speed_LM = new System.Windows.Forms.TextBox();
			this.textBox_speed_LR = new System.Windows.Forms.TextBox();
			this.textBox_speed_RF = new System.Windows.Forms.TextBox();
			this.textBox_speed_RR = new System.Windows.Forms.TextBox();
			this.textBox_speed_RM = new System.Windows.Forms.TextBox();
			this.textBox_tq_LF = new System.Windows.Forms.TextBox();
			this.textBox_tq_LM = new System.Windows.Forms.TextBox();
			this.textBox_tq_RF = new System.Windows.Forms.TextBox();
			this.textBox_tq_RM = new System.Windows.Forms.TextBox();
			this.textBox_tq_LR = new System.Windows.Forms.TextBox();
			this.textBox_tq_RR = new System.Windows.Forms.TextBox();
			this.button_stop_LF = new System.Windows.Forms.Button();
			this.button_stop_LM = new System.Windows.Forms.Button();
			this.button_stop_LR = new System.Windows.Forms.Button();
			this.button_stop_RF = new System.Windows.Forms.Button();
			this.button_stop_RM = new System.Windows.Forms.Button();
			this.button_stop_RR = new System.Windows.Forms.Button();
			this.textBox_Data = new System.Windows.Forms.TextBox();
			this.button_send = new System.Windows.Forms.Button();
			this.listBox_Info = new System.Windows.Forms.ListBox();
			this.textBox_ID = new System.Windows.Forms.TextBox();
			this.button_Clear = new System.Windows.Forms.Button();
			this.label6_ = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// button_connect
			// 
			this.button_connect.Location = new System.Drawing.Point(402, 27);
			this.button_connect.Name = "button_connect";
			this.button_connect.Size = new System.Drawing.Size(75, 27);
			this.button_connect.TabIndex = 0;
			this.button_connect.Text = "连接";
			this.button_connect.UseVisualStyleBackColor = true;
			this.button_connect.Click += new System.EventHandler(this.button_connect_Click);
			// 
			// comboBox_CANIndex
			// 
			this.comboBox_CANIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox_CANIndex.FormattingEnabled = true;
			this.comboBox_CANIndex.Location = new System.Drawing.Point(98, 78);
			this.comboBox_CANIndex.Name = "comboBox_CANIndex";
			this.comboBox_CANIndex.Size = new System.Drawing.Size(70, 23);
			this.comboBox_CANIndex.TabIndex = 2;
			// 
			// textBox_AccCode
			// 
			this.textBox_AccCode.Location = new System.Drawing.Point(282, 77);
			this.textBox_AccCode.Name = "textBox_AccCode";
			this.textBox_AccCode.Size = new System.Drawing.Size(80, 25);
			this.textBox_AccCode.TabIndex = 3;
			// 
			// textBox_AccMask
			// 
			this.textBox_AccMask.Location = new System.Drawing.Point(281, 110);
			this.textBox_AccMask.Name = "textBox_AccMask";
			this.textBox_AccMask.Size = new System.Drawing.Size(81, 25);
			this.textBox_AccMask.TabIndex = 3;
			// 
			// comboBox_Baudrate
			// 
			this.comboBox_Baudrate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox_Baudrate.FormattingEnabled = true;
			this.comboBox_Baudrate.Location = new System.Drawing.Point(98, 111);
			this.comboBox_Baudrate.Name = "comboBox_Baudrate";
			this.comboBox_Baudrate.Size = new System.Drawing.Size(70, 23);
			this.comboBox_Baudrate.TabIndex = 1;
			// 
			// comboBox_Filter
			// 
			this.comboBox_Filter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox_Filter.FormattingEnabled = true;
			this.comboBox_Filter.Location = new System.Drawing.Point(476, 78);
			this.comboBox_Filter.Name = "comboBox_Filter";
			this.comboBox_Filter.Size = new System.Drawing.Size(131, 23);
			this.comboBox_Filter.TabIndex = 1;
			// 
			// comboBox_Mode
			// 
			this.comboBox_Mode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox_Mode.FormattingEnabled = true;
			this.comboBox_Mode.Location = new System.Drawing.Point(476, 111);
			this.comboBox_Mode.Name = "comboBox_Mode";
			this.comboBox_Mode.Size = new System.Drawing.Size(70, 23);
			this.comboBox_Mode.TabIndex = 1;
			// 
			// timer_rec
			// 
			this.timer_rec.Interval = 5;
			this.timer_rec.Tick += new System.EventHandler(this.timer_rec_Tick);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(193, 82);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(83, 15);
			this.label1.TabIndex = 4;
			this.label1.Text = "验收码：0x";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(192, 115);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(83, 15);
			this.label2.TabIndex = 4;
			this.label2.Text = "屏蔽码：0x";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(23, 115);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(52, 15);
			this.label3.TabIndex = 4;
			this.label3.Text = "波特率";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(399, 82);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(67, 15);
			this.label4.TabIndex = 4;
			this.label4.Text = "滤波方式";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(401, 115);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(67, 15);
			this.label5.TabIndex = 4;
			this.label5.Text = "工作模式";
			// 
			// textBox_speed_LF
			// 
			this.textBox_speed_LF.Location = new System.Drawing.Point(211, 207);
			this.textBox_speed_LF.Name = "textBox_speed_LF";
			this.textBox_speed_LF.Size = new System.Drawing.Size(58, 25);
			this.textBox_speed_LF.TabIndex = 3;
			// 
			// textBox_speed_LM
			// 
			this.textBox_speed_LM.Location = new System.Drawing.Point(211, 270);
			this.textBox_speed_LM.Name = "textBox_speed_LM";
			this.textBox_speed_LM.Size = new System.Drawing.Size(58, 25);
			this.textBox_speed_LM.TabIndex = 3;
			// 
			// textBox_speed_LR
			// 
			this.textBox_speed_LR.Location = new System.Drawing.Point(211, 333);
			this.textBox_speed_LR.Name = "textBox_speed_LR";
			this.textBox_speed_LR.Size = new System.Drawing.Size(58, 25);
			this.textBox_speed_LR.TabIndex = 3;
			// 
			// textBox_speed_RF
			// 
			this.textBox_speed_RF.Location = new System.Drawing.Point(539, 207);
			this.textBox_speed_RF.Name = "textBox_speed_RF";
			this.textBox_speed_RF.Size = new System.Drawing.Size(60, 25);
			this.textBox_speed_RF.TabIndex = 3;
			// 
			// textBox_speed_RR
			// 
			this.textBox_speed_RR.Location = new System.Drawing.Point(539, 333);
			this.textBox_speed_RR.Name = "textBox_speed_RR";
			this.textBox_speed_RR.Size = new System.Drawing.Size(60, 25);
			this.textBox_speed_RR.TabIndex = 3;
			// 
			// textBox_speed_RM
			// 
			this.textBox_speed_RM.Location = new System.Drawing.Point(539, 270);
			this.textBox_speed_RM.Name = "textBox_speed_RM";
			this.textBox_speed_RM.Size = new System.Drawing.Size(60, 25);
			this.textBox_speed_RM.TabIndex = 3;
			// 
			// textBox_tq_LF
			// 
			this.textBox_tq_LF.Location = new System.Drawing.Point(26, 207);
			this.textBox_tq_LF.Name = "textBox_tq_LF";
			this.textBox_tq_LF.Size = new System.Drawing.Size(68, 25);
			this.textBox_tq_LF.TabIndex = 3;
			this.textBox_tq_LF.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textBox_tq_LF.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_tq_LF_KeyDown);
			// 
			// textBox_tq_LM
			// 
			this.textBox_tq_LM.Location = new System.Drawing.Point(26, 270);
			this.textBox_tq_LM.Name = "textBox_tq_LM";
			this.textBox_tq_LM.Size = new System.Drawing.Size(68, 25);
			this.textBox_tq_LM.TabIndex = 3;
			this.textBox_tq_LM.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textBox_tq_LM.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_tq_LM_KeyDown);
			// 
			// textBox_tq_RF
			// 
			this.textBox_tq_RF.Location = new System.Drawing.Point(337, 207);
			this.textBox_tq_RF.Name = "textBox_tq_RF";
			this.textBox_tq_RF.Size = new System.Drawing.Size(65, 25);
			this.textBox_tq_RF.TabIndex = 3;
			this.textBox_tq_RF.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textBox_tq_RF.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_tq_RF_KeyDown);
			// 
			// textBox_tq_RM
			// 
			this.textBox_tq_RM.Location = new System.Drawing.Point(337, 270);
			this.textBox_tq_RM.Name = "textBox_tq_RM";
			this.textBox_tq_RM.Size = new System.Drawing.Size(65, 25);
			this.textBox_tq_RM.TabIndex = 3;
			this.textBox_tq_RM.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textBox_tq_RM.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_tq_RM_KeyDown);
			// 
			// textBox_tq_LR
			// 
			this.textBox_tq_LR.Location = new System.Drawing.Point(26, 333);
			this.textBox_tq_LR.Name = "textBox_tq_LR";
			this.textBox_tq_LR.Size = new System.Drawing.Size(68, 25);
			this.textBox_tq_LR.TabIndex = 3;
			this.textBox_tq_LR.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textBox_tq_LR.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_tq_LR_KeyDown);
			// 
			// textBox_tq_RR
			// 
			this.textBox_tq_RR.Location = new System.Drawing.Point(337, 333);
			this.textBox_tq_RR.Name = "textBox_tq_RR";
			this.textBox_tq_RR.Size = new System.Drawing.Size(65, 25);
			this.textBox_tq_RR.TabIndex = 3;
			this.textBox_tq_RR.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textBox_tq_RR.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_tq_RR_KeyDown);
			// 
			// button_stop_LF
			// 
			this.button_stop_LF.Location = new System.Drawing.Point(129, 207);
			this.button_stop_LF.Name = "button_stop_LF";
			this.button_stop_LF.Size = new System.Drawing.Size(53, 25);
			this.button_stop_LF.TabIndex = 0;
			this.button_stop_LF.Text = "停止";
			this.button_stop_LF.UseVisualStyleBackColor = true;
			this.button_stop_LF.Click += new System.EventHandler(this.button_stop_LF_Click);
			// 
			// button_stop_LM
			// 
			this.button_stop_LM.Location = new System.Drawing.Point(129, 270);
			this.button_stop_LM.Name = "button_stop_LM";
			this.button_stop_LM.Size = new System.Drawing.Size(53, 25);
			this.button_stop_LM.TabIndex = 0;
			this.button_stop_LM.Text = "停止";
			this.button_stop_LM.UseVisualStyleBackColor = true;
			this.button_stop_LM.Click += new System.EventHandler(this.button_stop_LM_Click);
			// 
			// button_stop_LR
			// 
			this.button_stop_LR.Location = new System.Drawing.Point(129, 333);
			this.button_stop_LR.Name = "button_stop_LR";
			this.button_stop_LR.Size = new System.Drawing.Size(53, 25);
			this.button_stop_LR.TabIndex = 0;
			this.button_stop_LR.Text = "停止";
			this.button_stop_LR.UseVisualStyleBackColor = true;
			this.button_stop_LR.Click += new System.EventHandler(this.button_stop_LR_Click);
			// 
			// button_stop_RF
			// 
			this.button_stop_RF.Location = new System.Drawing.Point(446, 207);
			this.button_stop_RF.Name = "button_stop_RF";
			this.button_stop_RF.Size = new System.Drawing.Size(53, 25);
			this.button_stop_RF.TabIndex = 0;
			this.button_stop_RF.Text = "停止";
			this.button_stop_RF.UseVisualStyleBackColor = true;
			this.button_stop_RF.Click += new System.EventHandler(this.button_stop_RF_Click);
			// 
			// button_stop_RM
			// 
			this.button_stop_RM.Location = new System.Drawing.Point(446, 270);
			this.button_stop_RM.Name = "button_stop_RM";
			this.button_stop_RM.Size = new System.Drawing.Size(53, 25);
			this.button_stop_RM.TabIndex = 0;
			this.button_stop_RM.Text = "停止";
			this.button_stop_RM.UseVisualStyleBackColor = true;
			this.button_stop_RM.Click += new System.EventHandler(this.button_stop_RM_Click);
			// 
			// button_stop_RR
			// 
			this.button_stop_RR.Location = new System.Drawing.Point(446, 333);
			this.button_stop_RR.Name = "button_stop_RR";
			this.button_stop_RR.Size = new System.Drawing.Size(53, 25);
			this.button_stop_RR.TabIndex = 0;
			this.button_stop_RR.Text = "停止";
			this.button_stop_RR.UseVisualStyleBackColor = true;
			this.button_stop_RR.Click += new System.EventHandler(this.button_stop_RR_Click);
			// 
			// textBox_Data
			// 
			this.textBox_Data.Location = new System.Drawing.Point(166, 431);
			this.textBox_Data.Name = "textBox_Data";
			this.textBox_Data.Size = new System.Drawing.Size(254, 25);
			this.textBox_Data.TabIndex = 3;
			this.textBox_Data.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_Data_KeyDown);
			// 
			// button_send
			// 
			this.button_send.Location = new System.Drawing.Point(458, 430);
			this.button_send.Name = "button_send";
			this.button_send.Size = new System.Drawing.Size(53, 26);
			this.button_send.TabIndex = 0;
			this.button_send.Text = "发送";
			this.button_send.UseVisualStyleBackColor = true;
			this.button_send.Click += new System.EventHandler(this.button_send_Click);
			// 
			// listBox_Info
			// 
			this.listBox_Info.FormattingEnabled = true;
			this.listBox_Info.ItemHeight = 15;
			this.listBox_Info.Location = new System.Drawing.Point(677, 47);
			this.listBox_Info.Name = "listBox_Info";
			this.listBox_Info.Size = new System.Drawing.Size(370, 424);
			this.listBox_Info.TabIndex = 5;
			// 
			// textBox_ID
			// 
			this.textBox_ID.Location = new System.Drawing.Point(56, 431);
			this.textBox_ID.Name = "textBox_ID";
			this.textBox_ID.Size = new System.Drawing.Size(73, 25);
			this.textBox_ID.TabIndex = 3;
			// 
			// button_Clear
			// 
			this.button_Clear.Location = new System.Drawing.Point(563, 430);
			this.button_Clear.Name = "button_Clear";
			this.button_Clear.Size = new System.Drawing.Size(53, 26);
			this.button_Clear.TabIndex = 0;
			this.button_Clear.Text = "清空";
			this.button_Clear.UseVisualStyleBackColor = true;
			this.button_Clear.Click += new System.EventHandler(this.button_Clear_Click);
			// 
			// label6_
			// 
			this.label6_.AutoSize = true;
			this.label6_.Location = new System.Drawing.Point(23, 82);
			this.label6_.Name = "label6_";
			this.label6_.Size = new System.Drawing.Size(61, 15);
			this.label6_.TabIndex = 6;
			this.label6_.Text = "CAN通道";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(96, 212);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(23, 15);
			this.label6.TabIndex = 7;
			this.label6.Text = "Nm";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(96, 275);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(23, 15);
			this.label7.TabIndex = 7;
			this.label7.Text = "Nm";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(96, 338);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(23, 15);
			this.label8.TabIndex = 7;
			this.label8.Text = "Nm";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(406, 212);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(23, 15);
			this.label9.TabIndex = 7;
			this.label9.Text = "Nm";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(406, 275);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(23, 15);
			this.label10.TabIndex = 7;
			this.label10.Text = "Nm";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(406, 338);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(23, 15);
			this.label11.TabIndex = 7;
			this.label11.Text = "Nm";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(273, 212);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(31, 15);
			this.label12.TabIndex = 7;
			this.label12.Text = "rpm";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(273, 275);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(31, 15);
			this.label13.TabIndex = 7;
			this.label13.Text = "rpm";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(273, 338);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(31, 15);
			this.label14.TabIndex = 7;
			this.label14.Text = "rpm";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(602, 212);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(31, 15);
			this.label15.TabIndex = 7;
			this.label15.Text = "rpm";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(602, 275);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(31, 15);
			this.label16.TabIndex = 7;
			this.label16.Text = "rpm";
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(602, 338);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(31, 15);
			this.label17.TabIndex = 7;
			this.label17.Text = "rpm";
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.panel1.Location = new System.Drawing.Point(312, 182);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1, 197);
			this.panel1.TabIndex = 8;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1121, 528);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label17);
			this.Controls.Add(this.label14);
			this.Controls.Add(this.label16);
			this.Controls.Add(this.label15);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.label12);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label6_);
			this.Controls.Add(this.listBox_Info);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBox_tq_RR);
			this.Controls.Add(this.textBox_speed_RR);
			this.Controls.Add(this.textBox_tq_LR);
			this.Controls.Add(this.textBox_Data);
			this.Controls.Add(this.textBox_ID);
			this.Controls.Add(this.textBox_speed_LR);
			this.Controls.Add(this.textBox_tq_RM);
			this.Controls.Add(this.textBox_speed_RM);
			this.Controls.Add(this.textBox_tq_RF);
			this.Controls.Add(this.textBox_speed_RF);
			this.Controls.Add(this.textBox_tq_LM);
			this.Controls.Add(this.textBox_tq_LF);
			this.Controls.Add(this.textBox_speed_LM);
			this.Controls.Add(this.textBox_speed_LF);
			this.Controls.Add(this.textBox_AccMask);
			this.Controls.Add(this.textBox_AccCode);
			this.Controls.Add(this.comboBox_CANIndex);
			this.Controls.Add(this.comboBox_Mode);
			this.Controls.Add(this.comboBox_Filter);
			this.Controls.Add(this.comboBox_Baudrate);
			this.Controls.Add(this.button_stop_RR);
			this.Controls.Add(this.button_Clear);
			this.Controls.Add(this.button_send);
			this.Controls.Add(this.button_stop_LR);
			this.Controls.Add(this.button_stop_RM);
			this.Controls.Add(this.button_stop_RF);
			this.Controls.Add(this.button_stop_LM);
			this.Controls.Add(this.button_stop_LF);
			this.Controls.Add(this.button_connect);
			this.Name = "Form1";
			this.Text = "轮毂电机";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button_connect;
		private System.Windows.Forms.ComboBox comboBox_CANIndex;
		private System.Windows.Forms.TextBox textBox_AccCode;
		private System.Windows.Forms.TextBox textBox_AccMask;
		private System.Windows.Forms.ComboBox comboBox_Baudrate;
		private System.Windows.Forms.ComboBox comboBox_Filter;
		private System.Windows.Forms.ComboBox comboBox_Mode;
		private System.Windows.Forms.Timer timer_rec;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox textBox_speed_LF;
		private System.Windows.Forms.TextBox textBox_speed_LM;
		private System.Windows.Forms.TextBox textBox_speed_LR;
		private System.Windows.Forms.TextBox textBox_speed_RF;
		private System.Windows.Forms.TextBox textBox_speed_RR;
		private System.Windows.Forms.TextBox textBox_speed_RM;
		private System.Windows.Forms.TextBox textBox_tq_LF;
		private System.Windows.Forms.TextBox textBox_tq_LM;
		private System.Windows.Forms.TextBox textBox_tq_RF;
		private System.Windows.Forms.TextBox textBox_tq_RM;
		private System.Windows.Forms.TextBox textBox_tq_LR;
		private System.Windows.Forms.TextBox textBox_tq_RR;
		private System.Windows.Forms.Button button_stop_LF;
		private System.Windows.Forms.Button button_stop_LM;
		private System.Windows.Forms.Button button_stop_LR;
		private System.Windows.Forms.Button button_stop_RF;
		private System.Windows.Forms.Button button_stop_RM;
		private System.Windows.Forms.Button button_stop_RR;
		private System.Windows.Forms.TextBox textBox_Data;
		private System.Windows.Forms.Button button_send;
		private System.Windows.Forms.ListBox listBox_Info;
		private System.Windows.Forms.TextBox textBox_ID;
		private System.Windows.Forms.Button button_Clear;
		private System.Windows.Forms.Label label6_;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Panel panel1;
	}
}

