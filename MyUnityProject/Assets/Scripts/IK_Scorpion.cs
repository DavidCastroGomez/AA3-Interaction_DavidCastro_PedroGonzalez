using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OctopusController;
using UnityEngine.UIElements;
using System;
using UnityEngine.Assertions.Must;
using UnityEngine.Animations;
using UnityEngine.UI;

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

    public Transform upPoint;
    private float upPointHeight;

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


    [Header("Sliders")]
    public UnityEngine.UI.Slider forceSlider;
    public UnityEngine.UI.Slider magnusSlider;

    float forceSliderMultipier = 1000;
    float magnusSliderMultipier = 500;

    Vector3 originalDirection;
    Vector3 angleX, angleY, angleZ;

    Vector3[] normals;



    private const float LEG_VERTICAL_OFFSET = 10f;

    private const float BODY_HEIGHT = 0.5f;

    //DEBUG
    Vector3[] postions;

    // Start is called before the first frame update
    void Start()
    {
        originalDirection = -Body.forward;
        normals = new Vector3[legs.Length];
        upPointHeight = upPoint.localPosition.y;

        _myController.InitLegs(legs,futureLegBases,legTargets);
        _myController.InitTail(tail);
    }

    // Update is called once per frame
    void Update()
    {
        if(animPlaying)
            animTime += Time.deltaTime;

        NotifyTailTarget();

        if (Input.GetKey(KeyCode.Z))
        {
            magnusSlider.value -= Time.deltaTime * magnusSliderMultipier;
        }

        if (Input.GetKey(KeyCode.X))
        {
            magnusSlider.value += Time.deltaTime * magnusSliderMultipier;
        }

        if (Input.GetKey(KeyCode.Space) && !animPlaying)
        {
            forceSlider.value += Time.deltaTime * forceSliderMultipier;

            if (forceSlider.value <= 0 || forceSlider.value >= forceSlider.maxValue)
            {
                forceSliderMultipier *= -1;
            }

        }
        
        if (Input.GetKeyUp(KeyCode.Space) && !animPlaying)
        {
            NotifyStartWalk();
            animTime = 0;
            animPlaying = true;
        }

        if (animTime < animDuration && animTime > 0 && animPlaying)
        {

            SetBodyPosition();

            SetBasesHeight();

            SetBodyHeight();

            if (Vector3.Distance(pathPoints[pathPoints.Length - 1].position, Body.position) > arrivedToPathPoint)
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
        for(int i = 0; i < futureLegBases.Length; i++)
        {
            Vector3 startRayPosition = futureLegBases[i].position + new Vector3(0, LEG_VERTICAL_OFFSET, 0);

            //Debug.DrawLine(startRayPosition, startRayPosition + (Vector3.down * LEG_VERTICAL_OFFSET), Color.blue, 0.016f);

            RaycastHit hit;

            Physics.Raycast(startRayPosition, Vector3.down, out hit, LEG_VERTICAL_OFFSET * 3);

            futureLegBases[i].position = hit.point;

            normals[i] = hit.normal;
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
        Vector3 totalDirection = Vector3.zero;

        foreach (Vector3 v in normals)
        {
            totalDirection += v;
        }

        totalDirection.Normalize();

        Vector3 newPosition = Vector3.Lerp(upPoint.position, Body.position + totalDirection * upPointHeight, speed * Time.deltaTime);

        upPoint.position = new Vector3(newPosition.x, Body.position.y + upPointHeight, newPosition.z);

        Vector3 direction = (newPosition - Body.position).normalized;

        Vector3 cross = Vector3.Cross(direction, Body.up);

        float angle = Vector3.Angle(direction, Body.up);

        Body.rotation = Quaternion.Lerp(Body.rotation, Quaternion.LookRotation(Body.position - pathPoints[pathIndex].position , direction), speed * Time.deltaTime);
    }

    public void ResetScorpion(Vector3 resetPos)
    {
        magnusSlider.value = 0;
        forceSlider.value = 0;
        animPlaying = false;

        Body.position = resetPos;

        pathIndex = 0;

        SetBasesHeight();

        SetBodyHeight();

        _myController.ResetLegs();

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
