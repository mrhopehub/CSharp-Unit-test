using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SM2000s
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] data = { 0x16, 0x12, 0x85, 0x09, 0xD4, 0x0D, 0x00, 0x00, 0x01, 0x55, 0x55, 0xAA, 0x1D, 0x04, 0xAB, 0x02, 0xEC, 0x02, 0x85, 0x09, 0x01, 0xAA, 0xAA };
            DataLink frame = new DataLink(100);
            frame.frame_received = frame_received;
            frame.rx_ready();
            foreach(byte b in data)
            {
                frame.receive(b);
            }
            Console.ReadKey();
        }
        static void frame_received(byte[] framebuffer, int size)
        {
            /* 当接收到完整帧时调用该函数 */
            Console.Write("收到完整帧：");
            for (int i = 0; i < size; i++)
            {
                Console.Write(framebuffer[i].ToString("X2"));
                Console.Write(" ");
            }
            Console.WriteLine();
        }
    }
}
