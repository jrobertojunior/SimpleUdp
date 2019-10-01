using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Kinect;

namespace KinectUdpSender
{
    static class KinectController
    {
        private static KinectSensor kinectSensor;
        private static BodyFrameReader bodyFrameReader;
        private static CoordinateMapper coordinateMapper = null;

        private static Body[] bodies = null;

        public static event EventHandler OnNewFrame;

        public static int a = 10;


        public static Dictionary<JointType, Joint> Joints;
        public static Dictionary<JointType, Point> JointPoints;


        public static void Init()
        {
            kinectSensor = KinectSensor.GetDefault();
            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            coordinateMapper = kinectSensor.CoordinateMapper;

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

                            KinectController.JointPoints = new Dictionary<JointType, Point>();

                            foreach (JointType jointType in KinectController.Joints.Keys)
                            {
                                // sometimes the depth(Z) of an inferred joint may show as negative
                                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                                CameraSpacePoint position = KinectController.Joints[jointType].Position;
                                if (position.Z < 0)
                                {
                                    position.Z = 0.1f;
                                }

                                DepthSpacePoint depthSpacePoint = coordinateMapper.MapCameraPointToDepthSpace(position);
                                JointPoints.Add(jointType, new Point(depthSpacePoint.X, depthSpacePoint.Y));
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