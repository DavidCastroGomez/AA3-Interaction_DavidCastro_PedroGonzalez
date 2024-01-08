using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using UnityEngine.XR;

namespace OctopusController
{
    public enum TentacleMode { LEG, TAIL, TENTACLE };

    public class MyOctopusController 
    {
        MyTentacleController[] _tentacles =new  MyTentacleController[4];

        GameObject _currentRegion;
        Transform _target;
        int currentRegion = 2;

        Transform[] _randomTargets;// = new Transform[4];

        //DEBUG
        Transform[][] positions = new Transform[4][];

        float _twistMin, _twistMax;
        float _swingMin, _swingMax;

        //CCD Variables
        float weight;
        float tolerance;
        int maxIterations;
        bool wasBallShot = false;
        int nearestTentacle = 0;
        Vector3[] targetPositions;
        bool[] alreadyLooped;
        int[] attempts;
        int maxAttempts;

        //Defines
        double angleComparative = 0.025;
        float loopedTolerance = .1f;
        float magnitudeTolerance = .001f;
        float angleMultiplier = 57.3f;

        #region public methods

        public float TwistMin { set => _twistMin = value; }
        public float TwistMax { set => _twistMax = value; }
        public float SwingMin {  set => _swingMin = value; }
        public float SwingMax { set => _swingMax = value; }
        
        public void TestLogging(string objectName)
        {
            Debug.Log("hello, I am initializing my Octopus Controller in object " + objectName);
        }

        public Transform[][] getPositions()
        {
            return positions;
        }

        public void Init(Transform[] tentacleRoots, Transform[] randomTargets)
        {
            alreadyLooped = new bool[4];
            targetPositions = new Vector3[4];
            attempts = new int[4];
            maxAttempts = 10;
            _tentacles = new MyTentacleController[tentacleRoots.Length];

            for(int i = 0;  i  < tentacleRoots.Length; i++)
            {
                _tentacles[i] = new MyTentacleController();
                _tentacles[i].LoadTentacleJoints(tentacleRoots[i],TentacleMode.TENTACLE);
                weight = 0.5f;
                tolerance = 0f;
                maxIterations = 1;
            }

            _randomTargets = randomTargets;
        }

        public void NotifyTarget(Transform target, Transform region)
        {
            _currentRegion = region;
            _target = target;

            if (wasBallShot)
            {
                float value = 0f;
                for (int iter = 0; iter < _tentacles.Length; iter++)
                {
                    if ((region.position - _tentacles[iter].Bones[_tentacles[iter].Bones.Length - 1].position).magnitude < value)
                    {
                        nearestTentacle = iter;
                    }
                }
            }
              
        public void NotifyTarget(Transform target, GameObject region)
        {
            _currentRegion = region;
            _target = target;

            switch (_currentRegion.name) {
                case "region1":
                    currentRegion = 0;
                    break;
                case "region2":
                    currentRegion = 1;
                    break;
                case "region3":
                    currentRegion = 2;
                    break;
                case "region4":
                    currentRegion = 3;
                    break;
            }
        }

        public void NotifyShoot() {
            //TODO. what happens here?
            Debug.Log("Shoot");

            _randomTargets[currentRegion] = _target;

            wasBallShot = true;
        }

        public void UpdateTentacles()
        {
            update_ccd();
        }
        #endregion


        #region private and internal methods

        void update_ccd() 
        {
            for (int firstIter = 0; firstIter < _tentacles.Length; firstIter++)
            {
                Vector3 targetDir;
                if (firstIter != nearestTentacle || !wasBallShot)
                {
                    targetDir = _randomTargets[firstIter].position;
                }
                else
                {
                    targetDir = _target.position;
                }

                if (!alreadyLooped[firstIter] && maxAttempts > attempts[firstIter])
                {
                    for (int secndIter = 0; secndIter < _tentacles[firstIter].Bones.Length; secndIter++)
                    {
                        Vector3 firstVal = _tentacles[firstIter].Bones[_tentacles[firstIter].Bones.Length - 1].position - _tentacles[firstIter].Bones[secndIter].position;
                        Vector3 scndVal = targetDir - _tentacles[firstIter].Bones[secndIter].position;

                        float prodValue = 0f;
                        Vector3 crossValue = Vector3.zero;
                        float magnitudeVal;

                        if (magnitudeTolerance >= firstVal.magnitude * scndVal.magnitude)
                        {
                            magnitudeVal = 0f;
                        }
                        else
                        {
                            prodValue = Vector3.Dot(firstVal, scndVal);
                            crossValue = Vector3.Cross(firstVal.normalized, scndVal.normalized);
                            magnitudeVal = crossValue.magnitude;
                        }

                        double angleVal = (Math.Acos(prodValue / (firstVal.magnitude * scndVal.magnitude)) % Math.PI) * (double)((magnitudeVal > 0f) ? 1 : (-1));

                        if (angleVal > angleComparative)
                        {
                            _tentacles[firstIter].Bones[secndIter].Rotate(crossValue.normalized, (float)angleVal * angleMultiplier, Space.World);
                        }

                        _tentacles[firstIter].Bones[secndIter].localRotation = GetSwing(_tentacles[firstIter].Bones[secndIter].localRotation);
                    }

                    attempts[firstIter]++;
                }

                alreadyLooped[firstIter] = (targetDir - _tentacles[firstIter].Bones[_tentacles[firstIter].Bones.Length - 1].position).magnitude <= loopedTolerance;

                if (targetDir != targetPositions[firstIter])
                {
                    attempts[firstIter] = 0;
                    targetPositions[firstIter] = targetDir;
                }
            }
        }

        public Quaternion GetTwist(Quaternion rot)
        {
            return GetSwing(rot) * CalculateTwist(rot);
        }

        private Quaternion CalculateTwist(Quaternion rot)
        {
            return new Quaternion(0f, rot.y, 0f, rot.w).normalized;
        }

        public Quaternion GetSwing(Quaternion rot)
        {
            rot = new Quaternion(rot.x, Mathf.Clamp(rot.y, 0f, 1f), Mathf.Clamp(rot.z, 0f, 10f), rot.w);
            (rot * Quaternion.Inverse(new Quaternion(0f, rot.y, 0f, rot.w).normalized)).ToAngleAxis(out var angle, out var axis);
            return Quaternion.AngleAxis(Mathf.Clamp(angle, 0f, _swingMax), axis);
        }
    }

    #endregion
}