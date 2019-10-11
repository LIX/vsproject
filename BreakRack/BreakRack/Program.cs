using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BreakRack
{
	static class Program
	{
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}
}

//union u_1x
//{
//	int64_t u;
//	__packed struct
//	{


//		int16_t Speed:16;
//		uint16_t	:16;
//		uint16_t Toque:10;
//		uint8_t	:2;
//		uint8_t ModeDem:1;
//		uint8_t HVPowerDem:1;
//		uint8_t DischargeDem:1;
//		uint8_t EnableDem:1;
//		uint8_t counter:4;
//		uint8_t	:4;
//		uint8_t CheckValue:8;
//	}s;
//}* p_1x;

//union u_2x
//{
//	int64_t u;
//	__packed struct
//	{


//		int16_t SpeedBack:16;
//		uint16_t TorqueBack:10;
//		uint16_t MaxPosTorque:10;
//		uint8_t counter:2;
//		uint16_t MaxNegTorque:10;
//		uint16_t SpeedAtCurrentTorque;
//	}s;	
//}* p_2x;

//union u_3x
//{
//	int64_t u;
//	__packed struct
//	{
//		int8_t InitDone:1;
//		int8_t HVReady:1;
//		int8_t SystemEnable:1;
//		int8_t HVDownReady:1;
//		int8_t WorkingState:2;
//		int8_t WorkingMode:2;
//		int8_t SpeedValid:1;
//		int8_t TorqueValid:1;
//		int8_t FaultLevel:2;
//		int8_t counter:4;
//		int32_t :32;
//		int32_t :16;
//		int8_t CheckValvue;
//	}s;	
//}* p_3x;

//union u_4x
//{
//	int64_t u;
//	__packed struct
//	{
//		int16_t IdCurrent:10;
//		int8_t :2;
//		int16_t IqCurrent:10;
//		int16_t BusCurrent:10;
//		uint16_t BusVoltage:9;
//		int8_t :1;
//		int16_t PhaseCurrent:10;
//		uint8_t counter:4;
//		uint8_t CheckValvue;
//	}s;	
//}* p_4x;

//union u_5x
//{
//	int64_t u;
//	__packed struct
//	{
//		uint8_t MotorTemp;
//uint8_t ControllerTemp;
//uint8_t IGBTTemp;
//uint8_t ColdWaterTemp;
//int8_t SubVersion;
//int8_t MainVersion;
//uint8_t counter:4;
//		int8_t CheckValvue;
//	}s;	
//}* p_5x;

//union u_6x
//{
//	int64_t u;
//	__packed struct
//	{
//		int16_t ErrorCode_1;
//int16_t ErrorCode_2;
//int16_t ErrorCode_3;
//int16_t ErrorCode_4;
//	}s;	
//}* p_6x;