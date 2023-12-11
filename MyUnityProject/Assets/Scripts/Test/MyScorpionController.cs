using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using TMPro.EditorUtilities;
using UnityEngine;


namespace OctopusController
{
  
    public class MyScorpionController
    {
        //TAIL
        Transform tailTarget;
        Transform tailEndEffector;
        MyTentacleController _tail;
        float animationRange;
        bool playTailAnimation;
        float modifyDelta;
        Transform newTransform;

        float[] tailBoneAngles;
        Vector3[] bonePosCopy;
        Vector3[] tailBoneInverseDirection;

        float tailDistanceToStop;


        //LEGS
        Transform[] legTargets;
        Transform[] legFutureBases;
        MyTentacleController[] _legs = new MyTentacleController[6];

        //fabrik
        bool[] movement = new bool[6];
        float distanceUpThreshold;
        float distanceDownThreshold;
        float lerpAlpha = 0.1f;
        Vector3[] storeFutureBases = new Vector3[6];
        List<float> distances = new List<float>();

        Vector3[] positions;
        Vector3[][] returnPos;

        #region public
        public Vector3[][] aaa()
        {
            return returnPos;
        }

        public Vector3[] bbb() { return storeFutureBases; }

        public void InitLegs(Transform[] LegRoots,Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            //Legs init
            for(int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                //TODO: initialize anything needed for the FABRIK implementation

                movement[i] = false;

                storeFutureBases[i] = _legs[i].Bones[0].position;
            }

            distanceUpThreshold = 0.1f;
            distanceDownThreshold = 0.01f;
            legFutureBases = LegFutureBases;
            legTargets = LegTargets;

            returnPos = new Vector3[6][];



            for (int i = 1; i < _legs[0].Bones.Length; i++)
            {
                distances.Add(Math.Abs(Vector3.Distance(_legs[0].Bones[i].position, _legs[0].Bones[i - 1].position)));
            }

        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            //TODO: Initialize anything needed for the Gradient Descent implementation

            tailEndEffector = _tail.EndEffectorSphere;

            animationRange = 5f;
            playTailAnimation = false;
            modifyDelta = 0.1f;
            tailDistanceToStop = 0.0001f;

            bonePosCopy = new Vector3[_tail.Bones.Length];
            tailBoneAngles = new float[_tail.Bones.Length];
            tailBoneInverseDirection = new Vector3[_tail.Bones.Length];

            Quaternion[] auxRotations = new Quaternion[_tail.Bones.Length];

            for (int i = 0; i < _tail.Bones.Length; ++i)
            {
                auxRotations[i] = _tail.Bones[i].rotation;
            }

            _tail.Bones[0].rotation = Quaternion.identity;
            tailBoneAngles[0] = _tail.Bones[0].localEulerAngles.z;

            for (int i = 1; i < _tail.Bones.Length; ++i)
            {
                _tail.Bones[i].rotation = Quaternion.identity;

                tailBoneAngles[i] = _tail.Bones[i].localEulerAngles.x;
                tailBoneInverseDirection[i - 1] = _tail.Bones[i].position - _tail.Bones[i - 1].position;

            }

            tailBoneInverseDirection[tailBoneInverseDirection.Length - 1] = tailEndEffector.position - _tail.Bones[_tail.Bones.Length - 1].position;

            for (int i = 0; i < _tail.Bones.Length; ++i)
            {
                _tail.Bones[i].rotation = auxRotations[i];
            }
        }

        //TODO: Check when to start the animation towards target and implement Gradient Descent method to move the joints.
        public void NotifyTailTarget(Transform target)
        {
            //Debug.Log(tailEndEffector.position);
            //Debug.Log(target.position);
            //Debug.Log(Vector3.Distance(target.position, tailEndEffector.position));

            if(Vector3.Distance(target.position, tailEndEffector.position) < animationRange && !playTailAnimation)
            {
                playTailAnimation = true;
                tailTarget = target;

                Debug.Log("in");
                return;
            }

            if (playTailAnimation && Vector3.Distance(target.position, tailEndEffector.position) > animationRange)
            {
                playTailAnimation = false;
            }
        }

        //TODO: Notifies the start of the walking animation
        public void NotifyStartWalk()
        {

        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()
        {
            if (playTailAnimation)
            {
                updateTail();
            }
            updateLegPos();
        }
        #endregion


        #region private
        //TODO: Implement the leg base animations and logic
        private void updateLegPos()
        {
            //check for the distance to the futureBase, then if it's too far away start moving the leg towards the future base position
            //

            for(int i = 0; i < _legs.Length; i++)
            {
                if (Vector3.Distance(_legs[i].Bones[0].position, storeFutureBases[i]) > distanceUpThreshold && !movement[i])
                {
                    movement[i] = true;
                    storeFutureBases[i] = legFutureBases[i].position;
                    
                }
                else if(movement[i] && Vector3.Distance(_legs[i].Bones[0].position, storeFutureBases[i]) < distanceDownThreshold)
                {
                    movement[i] = false;
                    storeFutureBases[i] = _legs[i].Bones[0].position;

                }

                updateLegs(_legs[i].Bones, legTargets[i], storeFutureBases[i], i);


            }


        }
        //TODO: implement Gradient Descent method to move tail if necessary
        private void updateTail()
        {

            if (Vector3.Distance(tailTarget.position, tailEndEffector.position) > tailDistanceToStop)
            {
                CCD();
            }

        }
        //TODO: implement fabrik method to move legs 
        private void updateLegs(Transform[] bones, Transform legBase, Vector3 futureBase, int index)
        {
            positions = new Vector3[bones.Length];  

            for(int k = 0; k < bones.Length; k++)
            {
                positions[k] = new Vector3(bones[k].position.x, bones[k].position.y, bones[k].position.z);
            }

            
            
            positions[0] = Vector3.LerpUnclamped(positions[0], futureBase, 0.4f);

            
            for (int j = 1; j < positions.Length; j++)
            {
                Vector3 direction = positions[j] - positions[j - 1];

                direction.Normalize();

                positions[j] = positions[j - 1] + direction * distances[j - 1];
            }

            positions[positions.Length - 1] = legBase.position;

            distances.Reverse();
            
            for (int j = positions.Length - 2; j >= 0; j--)
            {
                Vector3 direction = positions[j] - positions[j + 1];

                direction.Normalize();


                positions[j] = positions[j + 1] + direction * distances[j];
            }

            distances.Reverse();

            for (int i = 0; i < bones.Length - 1; i++)
            {

                Vector3 oldDir = (bones[i + 1].position - bones[i].position).normalized;
                Vector3 newDir = (positions[i + 1] - positions[i]).normalized;

                Vector3 axis = Vector3.Cross(oldDir, newDir).normalized;
                float angle = Mathf.Acos(Vector3.Dot(oldDir, newDir)) * Mathf.Rad2Deg;

                if (angle > 0.1)
                {
                    bones[i].rotation = Quaternion.AngleAxis(angle, axis) * bones[i].rotation;
                }
            }

            for (int k = 0; k < bones.Length; k++)
            {
                bones[k].position = positions[k];

            }

            returnPos[index] = positions;
        }

        private void CCD()
        {
            for (int i = 0; i < tailBoneAngles.Length; ++i)
            {
                float distanceToTarget1 = Vector3.Distance(TheMath(), tailTarget.position);

                tailBoneAngles[i] += modifyDelta;
                float distanceToTarget2 = Vector3.Distance(TheMath(), tailTarget.position);
                tailBoneAngles[i] -= modifyDelta;

                tailBoneAngles[i] = tailBoneAngles[i] - 50 * ((distanceToTarget2 - distanceToTarget1) / modifyDelta);
            }
            
            Quaternion rotation = _tail.Bones[0].rotation;
            
            for (int i = 0; i < tailBoneAngles.Length; i++)
            {

                if (i == 0)
                {
                    rotation *= Quaternion.AngleAxis(tailBoneAngles[i], Vector3.forward);
                }
                else
                {
                    rotation *= Quaternion.AngleAxis(tailBoneAngles[i], Vector3.left);
                }

                _tail.Bones[i].rotation = rotation;
            }
            
        }

        public Vector3 TheMath()
        {
            Vector3 prevPoint = _tail.Bones[0].position;
            Quaternion rotation = _tail.Bones[0].rotation;

            for (int i = 0; i < _tail.Bones.Length; ++i)
            {
                if (i == 0)
                {
                    rotation *= Quaternion.AngleAxis(tailBoneAngles[i], Vector3.forward);
                }
                else
                {
                    rotation *= Quaternion.AngleAxis(tailBoneAngles[i], Vector3.left);
                }

                prevPoint += rotation * tailBoneInverseDirection[i];

                //_tail.Bones[i].position = prevPoint;

            }

           //tailEndEffector.position = prevPoint;

            return prevPoint;
        }


        private Quaternion DivideByEscalar(Quaternion quat, float escalar)
        {
            quat.x /= escalar;
            quat.y /= escalar;
            quat.z /= escalar;
            quat.w /= escalar;
            return quat;
        }
        #endregion

    }

}
