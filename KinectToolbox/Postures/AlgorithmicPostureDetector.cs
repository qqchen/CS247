﻿using System;
using Kinect.Toolbox.Record;
using Microsoft.Research.Kinect.Nui;

namespace Kinect.Toolbox
{
    public class AlgorithmicPostureDetector : PostureDetector
    {
        const float Epsilon = 0.1f;
        const float MaxRange = 0.25f;

        public AlgorithmicPostureDetector() : base(10)
        {
            
        }

        public override void TrackPostures(ReplaySkeletonData skeleton)
        {
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                return;

            Vector3? headPosition = null;
            Vector3? leftHandPosition = null;
            Vector3? rightHandPosition = null;

            foreach (Joint joint in skeleton.Joints)
            {
                if (joint.Position.W < 0.8f || joint.TrackingState != JointTrackingState.Tracked)
                    continue;

                switch (joint.ID)
                {
                    case JointID.Head:
                        headPosition = joint.Position.ToVector3();
                        break;
                    case JointID.HandLeft:
                        leftHandPosition = joint.Position.ToVector3();
                        break;
                    case JointID.HandRight:
                        rightHandPosition = joint.Position.ToVector3();
                        break;
                }
            }

            // HandsJoined
            if (CheckHandsJoined(rightHandPosition, leftHandPosition))
                return;

            // LeftHandOverHead
            if (CheckHandOverHead(headPosition, leftHandPosition))
            {
                RaisePostureDetected("LeftHandOverHead");
                return;
            }

            // RightHandOverHead
            if (CheckHandOverHead(headPosition, rightHandPosition))
            {
                RaisePostureDetected("RightHandOverHead");
                return;
            }

            // LeftHello
            if (CheckHello(headPosition, leftHandPosition))
            {
                RaisePostureDetected("LeftHello");
                return;
            }

            // RightHello
            if (CheckHello(headPosition, rightHandPosition))
            {
                RaisePostureDetected("RightHello");
                return;
            }

            Reset();
        }

        bool CheckHandOverHead(Vector3? headPosition, Vector3? handPosition)
        {
            if (!handPosition.HasValue || !headPosition.HasValue)
                return false;

            if (handPosition.Value.Y < headPosition.Value.Y)
                return false;

            if (Math.Abs(handPosition.Value.X - headPosition.Value.X) > MaxRange)
                return false;

            if (Math.Abs(handPosition.Value.Z - headPosition.Value.Z) > MaxRange)
                return false;

            return true;
        }


        bool CheckHello(Vector3? headPosition, Vector3? handPosition)
        {
            if (!handPosition.HasValue || !headPosition.HasValue)
                return false;

            if (Math.Abs(handPosition.Value.X - headPosition.Value.X) < MaxRange)
                return false;

            if (Math.Abs(handPosition.Value.Y - headPosition.Value.Y) > MaxRange)
                return false;

            if (Math.Abs(handPosition.Value.Z - headPosition.Value.Z) > MaxRange)
                return false;

            return true;
        }

        bool CheckHandsJoined(Vector3? leftHandPosition, Vector3? rightHandPosition)
        {
            if (!leftHandPosition.HasValue || !rightHandPosition.HasValue)
                return false;

            float distance = (leftHandPosition.Value - rightHandPosition.Value).Length;

            if (distance > Epsilon)
                return false;

            RaisePostureDetected("HandsJoined");
            return true;
        }      
    }
}
