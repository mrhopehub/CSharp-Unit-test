using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DS_2CD9121A_S
{
    public class DS_2CD9121A_SSnap
    {
        private CHCNetSDK.NET_DVR_SNAPCFG struSnapCfg;
        public DS_2CD9121A_SSnap()
        {
            struSnapCfg = new CHCNetSDK.NET_DVR_SNAPCFG();
            struSnapCfg.dwSize = (uint)Marshal.SizeOf(struSnapCfg);

            struSnapCfg.bySnapTimes = 3;
            struSnapCfg.wSnapWaitTime = 1000;
            struSnapCfg.wIntervalTime = new ushort[10];
            struSnapCfg.wIntervalTime[0] = 1000;
            struSnapCfg.wIntervalTime[1] = 1000;
            struSnapCfg.byRelatedDriveWay = 0;
        }
        public bool TriggerSnap(int m_lUserID)
        {
            return CHCNetSDK.NET_DVR_ContinuousShoot(m_lUserID, ref struSnapCfg);
        }
    }
}
