using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using EQ2008_DataStruct;

namespace EQ2013_1
{
    class EQ2013_1
    {
        public static int g_iCardNum = 1;      //控制卡地址
        public static int g_iGreen = 0xFF00; //绿色
        public static int g_iYellow = 0xFFFF; //黄色
        public static int g_iRed = 0x00FF; //红色
        public static int g_iProgramIndex = 0;//节目索引
        public static User_Text Text;//文本区
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns>true:初始化成功 false:初始化失败</returns>
        public static Boolean init()
        {
            Boolean result = true;
            ResetText();
            if (User_RealtimeConnect(g_iCardNum) == false)
            {
                Console.WriteLine("连接控制卡失败");
                return false;
            }
            if (User_OpenScreen(g_iCardNum) == false)
            {
                Console.WriteLine("初始化显示屏失败");
                return false;
            }
            if (User_DelAllProgram(g_iCardNum) == false)
            {
                Console.WriteLine("初始化节目失败");
                return false;
            }
            return result;
        }
        /// <summary>
        /// 重置文本区
        /// </summary>
        private static void ResetText()
        {
            Text.BkColor = 0;
            Text.chContent = "欢迎使用EQ2008型控制卡动态库!";

            Text.PartInfo.FrameColor = 0;
            Text.PartInfo.iFrameMode = 0;
            Text.PartInfo.iHeight = 32;
            Text.PartInfo.iWidth = 256;
            Text.PartInfo.iX = 0;
            Text.PartInfo.iY = 0;

            Text.FontInfo.bFontBold = false;
            Text.FontInfo.bFontItaic = false;
            Text.FontInfo.bFontUnderline = false;
            Text.FontInfo.colorFont = g_iRed;
            Text.FontInfo.iFontSize = 16;
            Text.FontInfo.strFontName = "宋体";
            Text.FontInfo.iAlignStyle = 1;
            Text.FontInfo.iVAlignerStyle = 0;
            Text.FontInfo.iRowSpace = 0;

            Text.MoveSet.bClear = false;
            Text.MoveSet.iActionSpeed = 5;
            Text.MoveSet.iActionType = 0;
            Text.MoveSet.iHoldTime = 20;
            Text.MoveSet.iClearActionType = 0;
            Text.MoveSet.iClearSpeed = 4;
            Text.MoveSet.iFrameTime = 20;
        }
        /// <summary>
        /// 显示字符串
        /// </summary>
        /// <param name="infoMsg">显示的内容</param>
        /// <param name="second">显示事件</param>
        /// <returns>true:显示成功 false:显示失败</returns>
        public static bool ShowInfo(string infoMsg, int second)
        {
            bool result = true;
            //1.添加节目获得节目编号
            g_iProgramIndex = User_AddProgram(g_iCardNum, false, second);
            Text.chContent = infoMsg;
            //2.向节目中添加文本区
            if (-1 == User_AddText(g_iCardNum, ref Text, g_iProgramIndex))
            {
                Console.WriteLine("添加文本失败！");
                return false;
            }
            //3.发送节目数据到控制卡
            if (User_SendToScreen(g_iCardNum) == false)
            {
                Console.WriteLine("发送节目失败！");
                return false;
            }
            //4.为了立即播放新节目，删除所有节目
            if (User_DelAllProgram(g_iCardNum) == false)
            {
                Console.WriteLine("删除节目失败");
                return false;
            }
            return result;
        }
        /// <summary>
        /// 显示车辆信息
        /// </summary>
        /// <param name="CPH">车牌号</param>
        /// <param name="ZZ">总重</param>
        /// <param name="ZS">轴数</param>
        /// <param name="SFCX">是否超限</param>
        /// <param name="CXL">超限率</param>
        /// <param name="second">显示时间</param>
        /// <returns>true:显示成功 false:显示失败</returns>
        public static bool ShowInfo(string CPH, int ZZ, int ZS, int SFCX, int CXL, int second)
        {
            bool result = false;
            return result;
        }

        //==========================1、节目操作函数======================//
        //添加节目
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddProgram(int CardNum, Boolean bWaitToEnd, int iPlayTime);
        //删除所有节目
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_DelAllProgram(int CardNum);

        //添加单行文本区
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddSingleText(int CardNum, ref User_SingleText pSingleText, int iProgramIndex);
        //添加文本区
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddText(int CardNum, ref User_Text pText, int iProgramIndex);
        //添加时间区
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddTime(int CardNum, ref User_DateTime pdateTime, int iProgramIndex);
        //添加图文区
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddBmpZone(int CardNum, ref User_Bmp pBmp, int iProgramIndex);
        //指定图像句柄添加图片
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern bool User_AddBmp(int CardNum, int iBmpPartNum, IntPtr hBitmap, ref User_MoveSet pMoveSet, int iProgramIndex);
        //指定图像路径添加图片
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern bool User_AddBmpFile(int CardNum, int iBmpPartNum, string strFileName, ref User_MoveSet pMoveSet, int iProgramIndex);

        //添加RTF区
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddRTF(int CardNum, ref User_RTF pRTF, int iProgramIndex);
        //添加计时区
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddTimeCount(int CardNum, ref User_Timer pTimeCount, int iProgramIndex);
        //添加温度区
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern int User_AddTemperature(int CardNum, ref User_Temperature pTemperature, int iProgramIndex);

        //发送数据
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_SendToScreen(int CardNum);
        //====================================================================//       

        //=======================2、实时发送数据（高频率发送）=================//
        //实时建立连接
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_RealtimeConnect(int CardNum);
        //实时发送图片数据
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_RealtimeSendData(int CardNum, int x, int y, int iWidth, int iHeight, IntPtr hBitmap);
        //实时发送图片文件
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_RealtimeSendBmpData(int CardNum, int x, int y, int iWidth, int iHeight, string strFileName);
        //实时发送文本
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_RealtimeSendText(int CardNum, int x, int y, int iWidth, int iHeight, string strText, ref User_FontSet pFontInfo);
        //实时关闭连接
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_RealtimeDisConnect(int CardNum);
        //实时发送清屏
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_RealtimeScreenClear(int CardNum);
        //====================================================================//

        //==========================3、显示屏控制函数组=======================//
        //校正时间
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_AdjustTime(int CardNum);
        //开屏
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_OpenScreen(int CardNum);
        //关屏
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_CloseScreen(int CardNum);
        //亮度调节
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern Boolean User_SetScreenLight(int CardNum, int iLightDegreen);
        //Reload参数文件
        [DllImport("EQ2008_Dll.dll", CharSet = CharSet.Ansi)]
        public static extern void User_ReloadIniFile(string strEQ2008_Dll_Set_Path);
        //====================================================================//
    }
}
