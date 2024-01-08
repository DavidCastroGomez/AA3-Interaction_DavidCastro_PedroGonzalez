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
        float[] originalTailBoneAngles;
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
        public Vector3[] storeFutureBases = new Vector3[6];
        public Vector3[] initialStepPosition = new Vector3[6];
        public float[] stepTimePerLeg = new float[6];
        public float timeToMoveLeg;

        List<float> distanceBetweenBones = new List<float>();

        bool walking = false;

        //Make foot go up and down
        float[] halfDistanceToNextBasePosition;
        float[] actualHeight;
        float stepHeight;

        Vector3[] positions;
        Vector3[][] returnPos;

        //Magnus values
        float magnus = .0f;
        float force = .0f;

        #region public
        public Vector3[][] aaa()
        {
            return returnPos;
        }

        public Vector3[] bbb() { return storeFutureBases; }

        public void InitLegs(Transform[] LegRoots,Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];

            legFutureBases = LegFutureBases;
            legTargets = LegTargets;

            actualHeight = new float[legFutureBases.Length];
            halfDistanceToNextBasePosition = new float[legFutureBases.Length];
            returnPos = new Vector3[6][];

            for (int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);

                movement[i] = false;

                storeFutureBases[i] = _legs[i].Bones[0].position;
                actualHeight[i] = storeFutureBases[i].y;
                halfDistanceToNextBasePosition[i] = 0;
            }

            distanceUpThreshold = 0.9f;
            distanceDownThreshold = 0.5f;

            timeToMoveLeg = 0.2f;
            stepHeight = 0.33f;


            for (int i = 1; i < _legs[0].Bones.Length; i++)
            {
                distanceBetweenBones.Add(Math.Abs(Vector3.Distance(_legs[0].Bones[i].position, _legs[0].Bones[i - 1].position)));
            }

        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);

            TailSetting();
        }

        void TailSetting()
        {
            tailEndEffector = _tail.EndEffectorSphere;

            animationRange = 5f;
            playTailAnimation = false;
            modifyDelta = 0.1f;
            tailDistanceToStop = 0.0001f;

            bonePosCopy = new Vector3[_tail.Bones.Length];
            tailBoneAngles = new float[_tail.Bones.Length];
            originalTailBoneAngles = new float[_tail.Bones.Length];
            tailBoneInverseDirection = new Vector3[_tail.Bones.Length];

            Quaternion[] auxRotations = new Quaternion[_tail.Bones.Length];

            for (int i = 0; i < _tail.Bones.Length; ++i)
            {
                auxRotations[i] = _tail.Bones[i].rotation;
            }

            _tail.Bones[0].rotation = Quaternion.identity;
            tailBoneAngles[0] = _tail.Bones[0].localEulerAngles.z;
            originalTailBoneAngles[0] = _tail.Bones[0].localEulerAngles.z;

            for (int i = 1; i < _tail.Bones.Length; ++i)
            {
                _tail.Bones[i].rotation = Quaternion.identity;

                tailBoneAngles[i] = _tail.Bones[i].localEulerAngles.x;
                originalTailBoneAngles[i] = _tail.Bones[i].localEulerAngles.x;
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
            if(Vector3.Distance(target.position, tailEndEffector.position) < animationRange && !playTailAnimation)
            {
                playTailAnimation = true;
                tailTarget = target;

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
            walking = true;
        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()
        {
            if (playTailAnimation)
            {
                updateTail();
            }

            if(walking)
                updateLegPos();
        }
        #endregion


        #region private
        //TODO: Implement the leg base animations and logic
        private void updateLegPos()
        {
            for(int i = 0; i < _legs.Length; i++)
            {
                if (Vector3.Distance(_legs[i].Bones[0].position, legFutureBases[i].position) > distanceUpThreshold && !movement[i])
                {
                    movement[i] = true;
                    storeFutureBases[i] = legFutureBases[i].position;
                    initialStepPosition[i] = _legs[i].Bones[0].position;
                    actualHeight[i] = legFutureBases[i].position.y;
                    halfDistanceToNextBasePosition[i] = Vector3.Distance(storeFutureBases[i], _legs[i].Bones[0].position) / 2;
                    stepTimePerLeg[i] = 0;
                }
                else if (Vector3.Distance(_legs[i].Bones[0].position, storeFutureBases[i]) < distanceDownThreshold)
                {
                    movement[i] = false;
                }

                updateLegs(_legs[i].Bones, legTargets[i], storeFutureBases[i], i);
            }
        }

        public void SetFromSliderValues(float _magnus, float _force)
        {
            magnus = _magnus;
            force = _force;
        }

        

        //TODO: implement Gradient Descent method to move tail if necessary
        private void updateTail()
        {
            if (Vector3.Distance(tailTarget.position, tailEndEffector.position) > tailDistanceToStop)
            {
                CCD(new Vector3(magnus * -0.5f, 0f, 0f) + tailTarget.position);
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

            stepTimePerLeg[index] += Time.deltaTime;

            positions[0] = Vector3.Lerp(initialStepPosition[index], storeFutureBases[index], stepTimePerLeg[index] / timeToMoveLeg);

            if (halfDistanceToNextBasePosition[index] != 0)
            {
                float heightModifier = 1 - Math.Abs(Vector3.Distance(positions[0], futureBase) - halfDistanceToNextBasePosition[index]) / halfDistanceToNextBasePosition[index]; //Transform distance to next position to a rate which modifies the height 

                positions[0].y = futureBase.y + heightModifier * stepHeight;
            }


            for (int j = 1; j < positions.Length; j++)
            { 
                Vector3 direction = positions[j] - positions[j - 1];

                direction.Normalize();

                positions[j] = positions[j - 1] + direction * distanceBetweenBones[j - 1];
            }

            positions[positions.Length - 1] = legBase.position;

            distanceBetweenBones.Reverse();
            
            for (int j = positions.Length - 2; j >= 0; j--)
            {
                Vector3 direction = positions[j] - positions[j + 1];

                direction.Normalize();


                positions[j] = positions[j + 1] + direction * distanceBetweenBones[j];
            }

            distanceBetweenBones.Reverse();

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

        private void CCD(Vector3 magnusTarget)
        {
            for (int i = 0; i < tailBoneAngles.Length; ++i)
            {
                float distanceToTarget1 = Vector3.Distance(magnusTarget, TheMath());
                tailBoneAngles[i] += modifyDelta;

                float distanceToTarget2 = Vector3.Distance(magnusTarget, TheMath());
                tailBoneAngles[i] -= modifyDelta;

                tailBoneAngles[i] -= force * ((distanceToTarget2 - distanceToTarget1) / modifyDelta);
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

                if (Vector3.Distance(magnusTarget, TheMath()) < .25f)
                {
                    break;
                }
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
            }

            return prevPoint;
        }

        public void ResetLegs()
        {
            for(int i = 0; i < legFutureBases.Length; i++)
            {
                storeFutureBases[i] = legFutureBases[i].position;
            }
        }

        public void ResetTail()
        {
            TailSetting();
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