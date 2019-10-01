using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using Microsoft.Kinect;

namespace KinectUdpSender
{
    class Program
    {
        // address that the message will be sent
        static string hostIp;
        static int hostPort;

        static int defaultPort = 20000;
        static string defaultIp = "localhost";

        static UdpClient sender;

        static DateTime checkpoint;

        static Stopwatch stopwatch;

        static int fps;

        static void Main(string[] args)
        {
            try
            {
                var address = File.ReadAllLines(@"address.txt");
                hostIp = address[0];
                hostPort = int.Parse(address[1]);
            }
            catch
            {
                hostIp = defaultIp;
                hostPort = defaultPort;
            }

            Console.WriteLine(string.Format("Sending to {0}:{1}", hostIp, hostPort));

            sender = new UdpClient(19999);

            KinectController.Init();
            KinectController.OnNewFrame += Kinect_OnNewFrame;

            stopwatch = new Stopwatch();

            Console.ReadKey();
        }

        private static void Kinect_OnNewFrame(object sender, EventArgs e)
        {
            var message = Encoding.ASCII.GetBytes(DateTime.Now.ToString() + " " + BuildMessage(KinectController.Joints, KinectController.JointPoints));

            Program.sender.Send(message, message.Length, hostIp, hostPort);

            Console.WriteLine(string.Format("at [{0}] sent a frame with spineBase.X = {1}", DateTime.Now.ToString(), KinectController.Joints[JointType.SpineBase].Position.X));
        }

        public static string BuildMessage(IReadOnlyDictionary<JointType, Joint> joints, Dictionary<JointType, Point> jointPoints)
        {
            string message = "";
            foreach (JointType jointType in joints.Keys)
            {
                string info = string.Format("{0} {1} {2} {3} {4} {5} ",
                    (int)joints[jointType].TrackingState, joints[jointType].Position.X, joints[jointType].Position.Y, joints[jointType].Position.Z,
                    jointPoints[jointType].X, jointPoints[jointType].Y);

                message += info;
            }

            return message;
        }
    }
}
