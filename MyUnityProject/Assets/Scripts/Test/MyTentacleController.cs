using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;




namespace OctopusController
{

    
    internal class MyTentacleController

    //MAINTAIN THIS CLASS AS INTERNAL
    {

        TentacleMode tentacleMode;
        Transform[] _bones;
        Transform _endEffectorSphere;

        public Transform[] Bones { get => _bones; }
        public Transform EndEffectorSphere { get => _endEffectorSphere; }

        //Exercise 1.
        public Transform[] LoadTentacleJoints(Transform root, TentacleMode mode)
        {
            //TODO: add here whatever is needed to find the bones forming the tentacle for all modes
            //you may want to use a list, and then convert it to an array and save it into _bones

            List<Transform> list = new List<Transform>();

            tentacleMode = mode;

            switch (tentacleMode)
            {
                case TentacleMode.LEG:

                    root = root.GetChild(0);
                    list.Add(root);

                    while (root.childCount >= 1)
                    {
                        list.Add(root.GetChild(1));
                        root = root.GetChild(1);
                    }

                    //TODO: in _endEffectorsphere you keep a reference to the base of the leg
                    _endEffectorSphere = root;

                    break;
                case TentacleMode.TAIL:

                    list.Add(root);

                    while (root.childCount >= 1)
                    {
                        list.Add(root.GetChild(1));
                        root = root.GetChild(1);
                    }

                    //TODO: in _endEffectorsphere you keep a reference to the red sphere                    
                    _endEffectorSphere = list.ElementAt(list.Count - 1);

                    list.RemoveAt(list.Count - 1);

                    break;
                case TentacleMode.TENTACLE:
                    while (root.childCount >= 1)
                    {
                        list.Add(root.GetChild(0));
                        root = root.GetChild(0);
                    }

                    //TODO: in _endEffectorphere you  keep a reference to the sphere with a collider attached to the endEffector

                    _endEffectorSphere = list.ElementAt(list.Count - 1);
                    break;
            }

            _bones = list.ToArray();
            return Bones;
        }
    }
}
