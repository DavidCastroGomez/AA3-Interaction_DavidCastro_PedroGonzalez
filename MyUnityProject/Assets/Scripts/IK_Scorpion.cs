using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OctopusController;
using UnityEngine.UIElements;
using System;
using UnityEngine.Assertions.Must;

public class IK_Scorpion : MonoBehaviour
{
    MyScorpionController _myController= new MyScorpionController();

    public IK_tentacles _myOctopus;

    [Header("Body")]
    float animTime;
    public float animDuration = 5;
    bool animPlaying = false;
    public Transform Body;
    public Transform StartPos;
    public Transform EndPos;

    [Header("Tail")]
    public Transform tailTarget;
    public Transform tail;

    [Header("Legs")]
    public Transform[] legs;
    public Transform[] legTargets;
    public Transform[] futureLegBases;

    [Header("Path")]
    public Transform[] pathPoints;
    int pathIndex = 0;
    float speed = 5f;
    float arrivedToPathPoint = 0.5f;

    Vector3 originalDirection;
    Vector3 angleX, angleY, angleZ;


    private const float LEG_VERTICAL_OFFSET = 10f;

    private const float BODY_HEIGHT = 0.5f;

    //DEBUG
    Vector3[] postions;

    // Start is called before the first frame update
    void Start()
    {
        originalDirection = -Body.forward;
        _myController.InitLegs(legs,futureLegBases,legTargets);
        _myController.InitTail(tail);
    }

    // Update is called once per frame
    void Update()
    {
        if(animPlaying)
            animTime += Time.deltaTime;

        NotifyTailTarget();
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NotifyStartWalk();
            animTime = 0;
            animPlaying = true;
        }

        if (animTime < animDuration && animTime > 0)
        {

            SetBodyPosition();

            SetBasesHeight();

            SetBodyHeight();

            RotateBody();
        }
        else if (animTime >= animDuration && animPlaying)
        {
            Body.position = EndPos.position;
            animPlaying = false;
        }

        _myController.UpdateIK();
    }

    private void SetBodyPosition()
    {
        if (pathIndex <= pathPoints.Length - 1)
        {
            Body.transform.position = Vector3.MoveTowards(Body.transform.position, pathPoints[pathIndex].transform.position, speed * Time.deltaTime);
            if (Vector3.Distance(Body.transform.position, pathPoints[pathIndex].transform.position) < arrivedToPathPoint && pathIndex < pathPoints.Length - 1)
            {
                pathIndex++;
            }
        }
    }

    private void SetBasesHeight()
    {
        foreach (Transform t in futureLegBases)
        {
            Vector3 startRayPosition = t.position + new Vector3(0, LEG_VERTICAL_OFFSET, 0);

            //Debug.DrawLine(startRayPosition, startRayPosition + (Vector3.down * LEG_VERTICAL_OFFSET), Color.blue, 0.016f);

            RaycastHit hit;

            Physics.Raycast(startRayPosition, Vector3.down, out hit, LEG_VERTICAL_OFFSET * 3);

            t.position = hit.point;
        }
    }

    private void SetBodyHeight()
    {
        float height = (futureLegBases[2].position.y + futureLegBases[3].position.y);
        
        height /= 2;

        Body.position = new Vector3(Body.position.x, height + BODY_HEIGHT, Body.position.z);

    }

    private void RotateBody()
    {
        Roll();
        Pitch();
        Yaw();

        Vector3 totalDirection = angleX + angleY + angleZ;

        totalDirection.Normalize();

        Body.rotation = Quaternion.FromToRotation(originalDirection, totalDirection);
    }
    private void Roll()
    {

        Vector3 right = Vector3.zero;
        Vector3 left = Vector3.zero;

        for (int i = 0; i < legs.Length; i++)
        {
            if (i % 2 == 0)
            {
                right += legs[i].GetChild(0).position;
            }
            else
            {
                left += legs[i].GetChild(0).position;
            }
        }

        right /= legs.Length / 2;
        left /= legs.Length / 2;

        angleZ = right - left;

        //angleZ = Vector3.Angle(direction.normalized, Vector3.up);

    }


    private void Yaw()
    {
        angleY = pathPoints[pathIndex].position - Body.position;

        //angleY = Vector3.Angle(direction.normalized, Vector3.forward);
    }

    private void Pitch()
    {
        Vector3 front = Vector3.zero;
        Vector3 back = Vector3.zero;

        front = (legs[0].GetChild(0).position + legs[0].GetChild(0).position) / 2;
        back = (legs[legs.Length - 1].GetChild(0).position + legs[legs.Length - 2].GetChild(0).position) / 2;

        angleX = front - back;

        //angleX = Vector3.Angle(direction.normalized, Vector3.up);
    }

    //Function to send the tail target transform to the dll
    public void NotifyTailTarget()
    {
        _myController
            .NotifyTailTarget(tailTarget);
    }

    //Trigger Function to start the walk animation
    public void NotifyStartWalk()
    {

        _myController.NotifyStartWalk();
    }

    private void OnDrawGizmos()
    {
        /*
        Vector3[][] aaa = _myController.aaa();

        foreach (Vector3[] v in aaa)
        {
            for (int i = 1; i < v.Length; i++)
            {
                Gizmos.DrawLine(v[i], v[i - 1]);
            }
        }

        Vector3[] bbb = _myController.bbb();

        foreach(Vector3 bb in bbb)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bb,0.5f);
        }
        */

        foreach (Transform t in futureLegBases) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(t.position, 0.3f);
        }


    }

}
