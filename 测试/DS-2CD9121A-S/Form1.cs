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
        //2.是否预览成功
        private Int32 m_lRealHandle = -1;

        private CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo;

        private IntPtr m_ptrRealHandle;//绘图句柄

        //================================预览回调======================//
        private CHCNetSDK.REALDATACALLBACK RealData = null;//预览回调函数
        public delegate void MyDebugInfo(string str);
        private PlayCtrl.DECCBFUN m_fDisplayFun = null;

        private Int32 m_lPort = -1;
        //==============================================================//
        
        //====================================================================//
        
        private uint iLastErr = 0;
        //预览方式
        private int previewMode = 1;

        public Form1()
        {
            InitializeComponent();
            m_ptrRealHandle = RealPlayWnd.Handle;
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
            if (str.Length > 0)
            {
                str += "\n";
                txtInfo.AppendText(str);
            }
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
                string str;
                lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.hPlayWnd = m_ptrRealHandle;//预览窗口 live view window
                lpPreviewInfo.lChannel = iChannelNum[0];//预览的设备通道 the device channel number
                lpPreviewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流

                IntPtr pUser = IntPtr.Zero;//用户数据 user data

                if (previewMode == 0)
                {
                    m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null, pUser);
                }
                else
                {
                    lpPreviewInfo.hPlayWnd = IntPtr.Zero;//预览窗口 live view window
                    RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);//预览实时流回调函数 real-time stream callback function 
                    //3.实时预览
                    m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, RealData, pUser);
                }

                if (m_lRealHandle < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //预览失败，输出错误号 failed to start live view, and output the error code.
                    DebugInfo(str);
                    return;
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
                string str;
                //停止预览 Stop live view 
                if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                    DebugInfo(str);
                    return;
                }
                if ((previewMode == 1) && (m_lPort >= 0))
                {
                    if (!PlayCtrl.PlayM4_Stop(m_lPort))
                    {
                        iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                        str = "PlayM4_Stop failed, error code= " + iLastErr;
                        DebugInfo(str);
                    }
                    if (!PlayCtrl.PlayM4_CloseStream(m_lPort))
                    {
                        iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                        str = "PlayM4_CloseStream failed, error code= " + iLastErr;
                        DebugInfo(str);
                    }
                    if (!PlayCtrl.PlayM4_FreePort(m_lPort))
                    {
                        iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                        str = "PlayM4_FreePort failed, error code= " + iLastErr;
                        DebugInfo(str);
                    }
                    m_lPort = -1;
                }

                DebugInfo("NET_DVR_StopRealPlay succ!");
                m_lRealHandle = -1;
                btnPreview.Text = "开始预览";
                RealPlayWnd.Invalidate();//刷新窗口 refresh the window
            }
        }

        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
            string str;
            //下面数据处理建议使用委托的方式
            MyDebugInfo AlarmInfo = new MyDebugInfo(DebugInfo);
            switch (dwDataType)
            {
                case CHCNetSDK.NET_DVR_SYSHEAD:     // sys head
                    if (dwBufSize > 0)
                    {
                        //获取播放句柄 Get the port to play
                        if (!PlayCtrl.PlayM4_GetPort(ref m_lPort))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_GetPort failed, error code= " + iLastErr;
                            this.BeginInvoke(AlarmInfo, str);
                            break;
                        }

                        //设置流播放模式 Set the stream mode: real-time stream mode
                        if (!PlayCtrl.PlayM4_SetStreamOpenMode(m_lPort, PlayCtrl.STREAME_REALTIME))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "Set STREAME_REALTIME mode failed, error code= " + iLastErr;
                            this.BeginInvoke(AlarmInfo, str);
                        }

                        //打开码流，送入头数据 Open stream
                        if (!PlayCtrl.PlayM4_OpenStream(m_lPort, pBuffer, dwBufSize, 2 * 1024 * 1024))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_OpenStream failed, error code= " + iLastErr;
                            this.BeginInvoke(AlarmInfo, str);
                            break;
                        }


                        //设置显示缓冲区个数 Set the display buffer number
                        if (!PlayCtrl.PlayM4_SetDisplayBuf(m_lPort, 15))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_SetDisplayBuf failed, error code= " + iLastErr;
                            this.BeginInvoke(AlarmInfo, str);
                        }

                        //设置显示模式 Set the display mode
                        if (!PlayCtrl.PlayM4_SetOverlayMode(m_lPort, 0, 0/* COLORREF(0)*/)) //play off screen 
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_SetOverlayMode failed, error code= " + iLastErr;
                            this.BeginInvoke(AlarmInfo, str);
                        }

                        //设置解码回调函数，获取解码后音视频原始数据 Set callback function of decoded data
                        m_fDisplayFun = new PlayCtrl.DECCBFUN(DecCallbackFUN);
                        if (!PlayCtrl.PlayM4_SetDecCallBackEx(m_lPort, m_fDisplayFun, IntPtr.Zero, 0))
                        {
                            this.BeginInvoke(AlarmInfo, "PlayM4_SetDisplayCallBack fail");
                        }

                        //开始解码 Start to play                       
                        if (!PlayCtrl.PlayM4_Play(m_lPort, m_ptrRealHandle))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_Play failed, error code= " + iLastErr;
                            this.BeginInvoke(AlarmInfo, str);
                            break;
                        }
                    }
                    break;
                case CHCNetSDK.NET_DVR_STREAMDATA:     // video stream data
                    if (dwBufSize > 0 && m_lPort != -1)
                    {
                        for (int i = 0; i < 999; i++)
                        {
                            //送入码流数据进行解码 Input the stream data to decode
                            if (!PlayCtrl.PlayM4_InputData(m_lPort, pBuffer, dwBufSize))
                            {
                                iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                                str = "PlayM4_InputData failed, error code= " + iLastErr;
                                Thread.Sleep(2);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
                default:
                    if (dwBufSize > 0 && m_lPort != -1)
                    {
                        //送入其他数据 Input the other data
                        for (int i = 0; i < 999; i++)
                        {
                            if (!PlayCtrl.PlayM4_InputData(m_lPort, pBuffer, dwBufSize))
                            {
                                iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                                str = "PlayM4_InputData failed, error code= " + iLastErr;
                                Thread.Sleep(2);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
            }
        }

        //解码回调函数
        private void DecCallbackFUN(int nPort, IntPtr pBuf, int nSize, ref PlayCtrl.FRAME_INFO pFrameInfo, int nReserved1, int nReserved2)
        {
            // 将pBuf解码后视频输入写入文件中（解码后YUV数据量极大，尤其是高清码流，不建议在回调函数中处理）
            if (pFrameInfo.nType == 3) //#define T_YV12	3
            {
                //    FileStream fs = null;
                //    BinaryWriter bw = null;
                //    try
                //    {
                //        fs = new FileStream("DecodedVideo.yuv", FileMode.Append);
                //        bw = new BinaryWriter(fs);
                //        byte[] byteBuf = new byte[nSize];
                //        Marshal.Copy(pBuf, byteBuf, 0, nSize);
                //        bw.Write(byteBuf);
                //        bw.Flush();
                //    }
                //    catch (System.Exception ex)
                //    {
                //        MessageBox.Show(ex.ToString());
                //    }
                //    finally
                //    {
                //        bw.Close();
                //        fs.Close();
                //    }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            //停止预览
            if (m_lRealHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
                m_lRealHandle = -1;
            }

            //注销登录
            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
                m_lUserID = -1;
            }

            CHCNetSDK.NET_DVR_Cleanup();

            Application.Exit();
        }
    }
}
