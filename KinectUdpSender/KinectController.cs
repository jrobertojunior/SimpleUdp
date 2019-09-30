using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace KinectUdpSender
{
    static class KinectController
    {
        private static KinectSensor kinectSensor;
        private static BodyFrameReader bodyFrameReader;
        private static Body[] bodies = null;

        public static event EventHandler OnNewFrame;

        public static int a = 10;


        public static Dictionary<JointType, Joint> Joints;


        public static void Init()
        {
            kinectSensor = KinectSensor.GetDefault();
            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            kinectSensor.Open();

            bodyFrameReader.FrameArrived += Update;
        }

        private static void Update(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (bodies == null)
                    {
                        bodies = new Body[bodyFrame.BodyCount];
                    }

                    bodyFrame.GetAndRefreshBodyData(bodies);
                    dataReceived = true;
                }

                if (dataReceived)
                {
                    foreach (Body body in bodies)
                    {
                        if (body.IsTracked)
                        {

                            KinectController.Joints = new Dictionary<JointType, Joint>();

                            foreach (JointType jointType in body.Joints.Keys)
                            {
                                Joints.Add(jointType, body.Joints[jointType]);
                            }

                            Microsoft.Kinect.Joint head = Joints[Microsoft.Kinect.JointType.Head];

                            if (head != null)
                            {
                                //Console.WriteLine(head.Position.X);
                            }

                            // raise new frame event
                            OnNewFrame?.Invoke(null, EventArgs.Empty);
                        }
                    }
                }
            }
        }
    }
}