using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
namespace DS_2CD9121A_S
{
    public partial class Form1 : Form
    {
        //==========================1、SDK初始化变量======================//
        //SDK是否初始化成功
        private bool m_bInitSDK = false;
        //====================================================================//

        //==========================2、登录设备变量======================//
        //1.用户单击标示
        //2.是否登录设备成功
        private Int32 m_lUserID = -1;

        private string DVRIPAddress; //设备IP地址或者域名 Device IP
        private Int16 DVRPortNumber;//设备服务端口号 Device Port
        private string DVRUserName;//设备登录用户名 User name to login
        private string DVRPassword;//设备登录密码 Password to login

        public CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo;//登录时获取设备信息

        //获取视频通道
        private uint dwAChanTotalNum = 0;
        private int[] iChannelNum = new int[96];
        //====================================================================//

        //==========================3、预览变量======================//
        //1.用户单击标示
        //2.是否预览成功
        private Int32 m_lRealHandle = -1;
        private DS_2CD9121A_SPreview preview;
        //====================================================================//
        
        private uint iLastErr = 0;

        CHCNetSDK.NET_DVR_SETUPALARM_PARAM struAlarmParam;
        private CHCNetSDK.MSGCallBack m_falarmData = null;
        private Int32 m_lAlarmHandle;

        public Form1()
        {
            InitializeComponent();
            this.preview = new DS_2CD9121A_SPreview(this, RealPlayWnd.Handle);
            //1.SDK初始化
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                MessageBox.Show("NET_DVR_Init error!");
                return;
            }
            else
            {
                //保存SDK日志 To save the SDK log
                CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
            }
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

        public void DebugInfo(string str)
        {
            Action action = () =>
            {
                if (str.Length > 0)
                {
                    str += "\n";
                    txtInfo.AppendText(str);
                }
            };
            this.BeginInvoke(action);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (m_lUserID < 0)
            {
                string str;
                DVRIPAddress = txtIP.Text; //设备IP地址或者域名 Device IP
                DVRPortNumber = Int16.Parse(txtPort.Text);//设备服务端口号 Device Port
                DVRUserName = txtUserName.Text;//设备登录用户名 User name to login
                DVRPassword = txtPassword.Text;//设备登录密码 Password to login

                //2.登录设备 Login the device
                m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Login_V30 failed, error code= " + iLastErr; //登录失败，输出错误号 Failed to login and output the error code
                    DebugInfo(str);
                    return;
                }
                else
                {
                    //登录成功
                    DebugInfo("NET_DVR_Login_V30 succ!");
                    btnLogin.Text = "退出设备";
                    dwAChanTotalNum = (uint)DeviceInfo.byChanNum;
                    for (int i = 0; i < dwAChanTotalNum; i++)
                    {
                        iChannelNum[i] = i + (int)DeviceInfo.byStartChan;
                    }

                    struAlarmParam = new CHCNetSDK.NET_DVR_SETUPALARM_PARAM();
                    struAlarmParam.dwSize = (uint)Marshal.SizeOf(struAlarmParam);
                    struAlarmParam.byAlarmInfoType = 1;//智能交通设备有效
                    //一级布防最大连接数为1个，二级最大连接数为3个，三级最大连接数为5个，设备支持一级、二级和
                    //三级布防同时进行，一级布防优先上传信息
                    struAlarmParam.byLevel = 1;

                    m_falarmData = new CHCNetSDK.MSGCallBack(MsgCallback);
                    CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V30(m_falarmData, IntPtr.Zero);
                    m_lAlarmHandle = CHCNetSDK.NET_DVR_SetupAlarmChan_V41(m_lUserID, ref struAlarmParam);
                }
            }
            else
            {
                string str;
                //注销登录 Logout the device
                if (m_lRealHandle >= 0)
                {
                    DebugInfo("Please stop live view firstly"); //登出前先停止预览 Stop live view before logout
                    return;
                }

                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Logout failed, error code= " + iLastErr;
                    DebugInfo(str);
                    return;
                }
                DebugInfo("NET_DVR_Logout succ!");
                m_lUserID = -1;
                btnLogin.Text = "登录设备";
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (m_lUserID < 0)
            {
                MessageBox.Show("Please login the device firstly!");
                return;
            }
            if (m_lRealHandle < 0)
            {
                m_lRealHandle = this.preview.startPreview(m_lUserID, iChannelNum[0]);
                if (m_lRealHandle < 0)
                {
                    string str;
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //预览失败，输出错误号 failed to start live view, and output the error code.
                    DebugInfo(str);
                }
                else
                {
                    //预览成功
                    DebugInfo("NET_DVR_RealPlay_V40 succ!");
                    btnPreview.Text = "停止预览";
                }
            }
            else
            {
                if (this.preview.stopPreview(m_lRealHandle))
                {
                    DebugInfo("NET_DVR_StopRealPlay succ!");
                    m_lRealHandle = -1;
                    btnPreview.Text = "开始预览";
                    RealPlayWnd.Invalidate();//刷新窗口 refresh the window
                }
                else
                {
                    string str;
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                    DebugInfo(str);
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            //停止预览
            if (m_lRealHandle >= 0)
            {
                if (this.preview.stopPreview(m_lRealHandle))
                {
                    DebugInfo("NET_DVR_StopRealPlay succ!");
                    m_lRealHandle = -1;
                    btnPreview.Text = "开始预览";
                    RealPlayWnd.Invalidate();//刷新窗口 refresh the window
                }
                else
                {
                    DebugInfo("NET_DVR_StopRealPlay succ!");
                }
            }

            //注销登录
            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
                m_lUserID = -1;
            }
            //撤防
            if (m_lAlarmHandle >= 0)
            {
                CHCNetSDK.NET_DVR_CloseAlarmChan_V30(m_lAlarmHandle);
            }

            CHCNetSDK.NET_DVR_Cleanup();

            Application.Exit();
        }

        private void btnSnap_Click(object sender, EventArgs e)
        {
            if(m_lUserID < 0)
            {
                MessageBox.Show("请先登录设备");
            }
            else
            {
                CHCNetSDK.NET_DVR_SNAPCFG struSnapCfg = new CHCNetSDK.NET_DVR_SNAPCFG();
                struSnapCfg.dwSize = (uint)Marshal.SizeOf(struSnapCfg);

                struSnapCfg.bySnapTimes = 3;
                struSnapCfg.wSnapWaitTime = 1000;
                struSnapCfg.wIntervalTime  = new ushort[10];
                struSnapCfg.wIntervalTime[0] = 1000;
                struSnapCfg.wIntervalTime[1] = 1000;
                struSnapCfg.byRelatedDriveWay = 0;
                if (CHCNetSDK.NET_DVR_ContinuousShoot(m_lUserID, ref struSnapCfg))
                {
                    DebugInfo("抓拍成功");
                }
                else
                {
                    DebugInfo("抓拍不成功");
                }


            }
        }
    }
}
