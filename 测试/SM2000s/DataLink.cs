using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SM2000s
{
    public class DataLink
    {
        public delegate void frame_received_Del(byte[] framebuffer, int size);
        //接收到完整帧时的委托
        public frame_received_Del frame_received;
        private int maxLen;
        //接收到的字节个数
        private int length;
        //状态标识
        private int flags;
        //存放接收到的数据
        private byte[] frame;
        public DataLink(int maxLen)
        {
            this.maxLen = maxLen;
            length = 0;
            flags = 0;
            frame = new byte[maxLen];
        }
        /// <summary>
        /// 准备
        /// </summary>
        public void rx_ready()
        {
            length = 0;
            flags = 0;
            flags |= Convert.ToInt32(StateEnum.READY);
            for (int i = 0; i < maxLen; i++)
            {
                frame[i] = 0x00;
            }
        }
        /// <summary>
        /// 处理一字节数据
        /// </summary>
        /// <param name="b">收到的字节数据</param>
        /// <returns>0表示成功处理</returns>
        public int receive(byte b)
        {
            //是否准备好接收数据
            if (Convert.ToBoolean(flags & Convert.ToInt32(StateEnum.READY)))
            {
                //溢出处理
                if (length >= maxLen)
                {
                    Console.WriteLine("溢出错误");
                    rx_ready();
                    return 1;
                }
                //是否在结尾状态
                if (Convert.ToBoolean(flags & Convert.ToInt32(StateEnum.END)))
                {
                    //是否为0xaa
                    if (b == 0xaa)
                    {
                        flags &= ~Convert.ToInt32(StateEnum.END);
                        frame[length] = 0xaa;
                        length++;
                        if(check(frame, length))
                        {
                            frame_received(frame, length);
                            rx_ready();
                            return 0;
                        }
                        else
                        {
                            Console.WriteLine("数据帧校验错误");
                            flags &= ~Convert.ToInt32(StateEnum.END);
                            rx_ready();
                            return 1;
                        }
                    }
                    else
                    {
                        Console.WriteLine("结尾错误");
                        flags &= ~Convert.ToInt32(StateEnum.END);
                        rx_ready();
                        return 1;
                    }
                }
                    //是否在转义状态
                else if (Convert.ToBoolean(flags & Convert.ToInt32(StateEnum.ESCAPED)))
                {
                    //并没有真正的转义，补充丢失的数据
                    if (b != 0xaa)
                    {
                        flags &= ~Convert.ToInt32(StateEnum.ESCAPED);
                        frame[length] = 0x55;
                        length++;
                        return receive(b);
                    }
                        //真正的转义
                    else
                    {
                        flags &= ~Convert.ToInt32(StateEnum.ESCAPED);
                        frame[length] = 0xaa;
                        length++;
                        return 0;
                    }
                }
                else if (b == 0x55)//进入转义状态
                {
                    flags |= Convert.ToInt32(StateEnum.ESCAPED);
                    return 0;
                }
                else if (b == 0xaa)//进入结尾状态
                {
                    flags |= Convert.ToInt32(StateEnum.END);
                    frame[length] = 0xaa;
                    length++;
                    return 0;
                }
                else//接收到正常数据
                {
                    frame[length] = b;
                    length++;
                    return 0;
                }
            }
            else
            {
                Console.WriteLine("还没有准备好");
                return 1;
            }
        }
        /// <summary>
        /// 数据帧校验
        /// </summary>
        /// <param name="framebuffer">帧数据</param>
        /// <param name="size">帧长度</param>
        /// <returns>true表示校验正确</returns>
        private bool check(byte[] framebuffer, int size)
        {
            bool result = false;
            //转换为十进制数
            int tmp = Convert.ToInt32(framebuffer[0]);
            //取十位
            int axlesum = (tmp / 10) % 10;
            int correctSize = 1 + 1 + 2 + 4 + 1 + 2 * axlesum*2 + (axlesum - 1)*2 + 1 + 2;
            if (correctSize == size)
            {
                result = true;
            }
            return result;
        }
    }
}
