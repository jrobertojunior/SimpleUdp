﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Kinect;

namespace KinectUdpSender
{
    class Program
    {
        static int hostPort = 20000;
        static string hostIp = "192.168.43.139";

        static UdpClient sender;

        static DateTime checkpoint;

        static Stopwatch stopwatch;

        static int fps;

        static void Main(string[] args)
        {
            sender = new UdpClient(19999);

            KinectController.Init();
            KinectController.OnNewFrame += Kinect_OnNewFrame;

            stopwatch = new Stopwatch();

            Console.ReadKey();
        }

        private static void Kinect_OnNewFrame(object sender, EventArgs e)
        {
            var message = Encoding.ASCII.GetBytes(DateTime.Now.ToString() + " " + BuildMessage(KinectController.Joints));

            var address = File.ReadAllLines("address.txt");
            Program.sender.Send(message, message.Length, address[0], int.Parse(address[1]));

            Console.WriteLine(string.Format("at [{0}] sent a frame with spineBase.X = {1}", DateTime.Now.ToString(), KinectController.Joints[JointType.SpineBase].Position.X));
        }

        public static string BuildMessage(IReadOnlyDictionary<JointType, Joint> joints)
        {
            string message = "";
            foreach (JointType jointType in joints.Keys)
            {
                string info = string.Format("{0} {1} {2} {3} ",
                    (int)joints[jointType].TrackingState, joints[jointType].Position.X, joints[jointType].Position.Y, joints[jointType].Position.Z);

                message += info;
            }

            return message;
        }
    }
}