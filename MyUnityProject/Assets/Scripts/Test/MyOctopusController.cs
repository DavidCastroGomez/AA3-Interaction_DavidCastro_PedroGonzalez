using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
    public enum TentacleMode { LEG, TAIL, TENTACLE };

    public class MyOctopusController 
    {
        
        MyTentacleController[] _tentacles =new  MyTentacleController[4];

        Transform _currentRegion;
        Transform _target;

        Transform[] _randomTargets;// = new Transform[4];

        //DEBUG
        Transform[][] positions = new Transform[4][];


        float _twistMin, _twistMax;
        float _swingMin, _swingMax;

        //CCD Variables
        float weight;
        float tolerance;
        int maxIterations;

        #region public methods
        //DO NOT CHANGE THE PUBLIC METHODS!!

        public float TwistMin { set => _twistMin = value; }
        public float TwistMax { set => _twistMax = value; }
        public float SwingMin {  set => _swingMin = value; }
        public float SwingMax { set => _swingMax = value; }
        

        public void TestLogging(string objectName)
        {

           
            Debug.Log("hello, I am initializing my Octopus Controller in object "+objectName);

            
        }

        public Transform[][] getPositions()
        {
            return positions;
        }

        public void Init(Transform[] tentacleRoots, Transform[] randomTargets)
        {
            _tentacles = new MyTentacleController[tentacleRoots.Length];

            // foreach (Transform t in tentacleRoots)
            for(int i = 0;  i  < tentacleRoots.Length; i++)
            {

                _tentacles[i] = new MyTentacleController();
                _tentacles[i].LoadTentacleJoints(tentacleRoots[i],TentacleMode.TENTACLE);
                //TODO: initialize any variables needed in ccd
                weight = 0.5f;
                tolerance = 0f;
                maxIterations = 1;
            }

            _randomTargets = randomTargets;
            //TODO: use the regions however you need to make sure each tentacle stays in its region

        }

              
        public void NotifyTarget(Transform target, Transform region)
        {
            _currentRegion = region;
            _target = target;
        }

        public void NotifyShoot() {
            //TODO. what happens here?
            Debug.Log("Shoot");
        }


        public void UpdateTentacles()
        {
            //TODO: implement logic for the correct tentacle arm to stop the ball and implement CCD method
            update_ccd();
        }




        #endregion


        #region private and internal methods
        //todo: add here anything that you need

        void update_ccd() 
        {
           
            for(int i = 0; i < maxIterations; i++) {
                for (int tentacleNumber = 0; tentacleNumber < _tentacles.Length; tentacleNumber++)
                {
                    //Get Bones
                    Transform[] bones = new Transform[_tentacles[tentacleNumber].Bones.Length - 1];

                    for (int j = 0; j < _tentacles[tentacleNumber].Bones.Length - 1; j++)
                    {
                        bones[j] = _tentacles[tentacleNumber].Bones[j];
                    }

                    for (int currentBone = bones.Length - 2; currentBone > 1; currentBone--)
                    {
                        //Get vector from bone to target
                        Vector3 redVector = (bones[currentBone].position - _randomTargets[tentacleNumber].position).normalized;

                        Vector3 greenVector = (bones[currentBone].position - bones[bones.Length-1].position).normalized;

                        float angle = Vector3.Angle(greenVector, redVector);

                        bones[i].RotateAround(bones[0].position, Vector3.Cross(greenVector, redVector), angle);
                    }

                    for (int j = 0; j < _tentacles[tentacleNumber].Bones.Length - 1; j++)
                    {
                        _tentacles[tentacleNumber].Bones[j] = bones[j];
                    }

                    positions[tentacleNumber] = bones;

                    if (Vector3.Distance(bones[bones.Length - 1].position, _randomTargets[tentacleNumber].position) <= tolerance)
                    {
                        i = maxIterations;
                    }

                }

            }

        }
            
    }


        

        #endregion






}

