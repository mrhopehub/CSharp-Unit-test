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
        //2.预览句柄
        private Int32 m_lRealHandle = -1;
        private DS_2CD9121A_SPreview preview;
        //====================================================================//

        //==========================4、报警变量======================//
        //报警句柄
        private Int32 m_lAlarmHandle = -1;
        private DS_2CD9121A_SAlarm alarm;
        //====================================================================//

        //==========================5、抓拍变量======================//
        private DS_2CD9121A_SSnap snap;
        //====================================================================//
        
        private uint iLastErr = 0;

        public Form1()
        {
            InitializeComponent();
            this.preview = new DS_2CD9121A_SPreview(this, RealPlayWnd.Handle);
            this.alarm = new DS_2CD9121A_SAlarm();
            this.snap = new DS_2CD9121A_SSnap();
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

                if (m_lAlarmHandle >= 0)
                {
                    DebugInfo("Please stop Alarm firstly"); //登出前先停止预览 Stop live view before logout
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

        private void btnAlarm_Click(object sender, EventArgs e)
        {
            if (m_lUserID < 0)
            {
                MessageBox.Show("Please login the device firstly!");
                return;
            }
            if (m_lAlarmHandle < 0)
            {
                m_lAlarmHandle = this.alarm.startAlarm(m_lUserID);
                if (m_lAlarmHandle < 0)
                {
                    string str;
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_SetupAlarmChan_V41, error code= " + iLastErr; //布防失败，输出错误号 failed to start live view, and output the error code.
                    DebugInfo(str);
                }
                else
                {
                    //布防成功
                    DebugInfo("NET_DVR_SetupAlarmChan_V41 succ!");
                    btnAlarm.Text = "停止布防";
                }
            }
            else
            {
                if (this.alarm.stopAlarm(m_lAlarmHandle))
                {
                    DebugInfo("NET_DVR_CloseAlarmChan_V30 succ!");
                    m_lAlarmHandle = -1;
                    btnAlarm.Text = "开始布防";
                }
                else
                {
                    string str;
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_CloseAlarmChan_V30 failed, error code= " + iLastErr;
                    DebugInfo(str);
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            //注销登录
            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
                m_lUserID = -1;
            }
            //停止预览
            if (m_lRealHandle >= 0)
            {
                this.preview.stopPreview(m_lRealHandle);
            }
            //撤防
            if (m_lAlarmHandle >= 0)
            {
                alarm.startAlarm(m_lAlarmHandle);
            }

            CHCNetSDK.NET_DVR_Cleanup();

            Application.Exit();
        }

        private void btnSnap_Click(object sender, EventArgs e)
        {
            if(m_lUserID < 0)
            {
                MessageBox.Show("请先登录设备");
                return;
            }
            if (snap.TriggerSnap(m_lUserID))
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
