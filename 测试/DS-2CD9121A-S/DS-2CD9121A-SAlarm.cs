using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace DS_2CD9121A_S
{
    public class DS_2CD9121A_SAlarm
    {
        private CHCNetSDK.NET_DVR_SETUPALARM_PARAM struAlarmParam;
        private CHCNetSDK.MSGCallBack m_falarmData = null;

        public int startAlarm(int m_lUserID)
        {
            struAlarmParam = new CHCNetSDK.NET_DVR_SETUPALARM_PARAM();
            struAlarmParam.dwSize = (uint)Marshal.SizeOf(struAlarmParam);
            struAlarmParam.byAlarmInfoType = 1;//智能交通设备有效
            //一级布防最大连接数为1个，二级最大连接数为3个，三级最大连接数为5个，设备支持一级、二级和
            //三级布防同时进行，一级布防优先上传信息
            struAlarmParam.byLevel = 1;

            m_falarmData = new CHCNetSDK.MSGCallBack(MsgCallback);
            CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V30(m_falarmData, IntPtr.Zero);
            int AlarmHandle = CHCNetSDK.NET_DVR_SetupAlarmChan_V41(m_lUserID, ref struAlarmParam);
            return AlarmHandle;
        }

        public bool stopAlarm(int AlarmHandle)
        {
            return CHCNetSDK.NET_DVR_CloseAlarmChan_V30(AlarmHandle);
        }

        public void DebugInfo(string str)
        {
            Console.WriteLine(str);
        }

        public void MsgCallback(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //通过lCommand来判断接收到的报警信息类型，不同的lCommand对应不同的pAlarmInfo内容
            switch (lCommand)
            {
                case CHCNetSDK.COMM_ITS_PLATE_RESULT://交通抓拍结果上传(新报警信息类型)
                    DebugInfo("新报警信息");
                    ProcessCommAlarm_ITSPlate(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                default:
                    DebugInfo("未知报警信息");
                    break;
            }
        }

        private void ProcessCommAlarm_ITSPlate(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_ITS_PLATE_RESULT struITSPlateResult = new CHCNetSDK.NET_ITS_PLATE_RESULT();
            uint dwSize = (uint)Marshal.SizeOf(struITSPlateResult);

            struITSPlateResult = (CHCNetSDK.NET_ITS_PLATE_RESULT)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_ITS_PLATE_RESULT));

            //保存抓拍图片
            for (int i = 0; i < struITSPlateResult.dwPicNum; i++)
            {
                if (struITSPlateResult.struPicInfo[i].dwDataLen != 0)
                {
                    string str = "D:/UserID_" + pAlarmer.lUserID + "_Pictype_" + struITSPlateResult.struPicInfo[i].byType + "_Num" + (i + 1) + ".jpg";
                    FileStream fs = new FileStream(str, FileMode.Create);
                    int iLen = (int)struITSPlateResult.struPicInfo[i].dwDataLen;
                    byte[] by = new byte[iLen];
                    Marshal.Copy(struITSPlateResult.struPicInfo[i].pBuffer, by, 0, iLen);
                    fs.Write(by, 0, iLen);
                    fs.Close();
                }
            }
        }
    }
}
