using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OctopusController;
using UnityEngine.UIElements;

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
    
    private const float LEG_VERTICAL_OFFSET = 10f;

    private const float BODY_HEIGHT = 0.5f;

    //DEBUG
    Vector3[] postions;

    // Start is called before the first frame update
    void Start()
    {
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

        if (animTime < animDuration)
        {

            SetBasesHeight();

            Body.position = Vector3.Lerp(StartPos.position, EndPos.position, animTime / animDuration);

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

    private void SetBasesHeight()
    {
        foreach (Transform t in futureLegBases)
        {
            Vector3 startRayPosition = t.position + new Vector3(0, LEG_VERTICAL_OFFSET, 0);

            Debug.DrawLine(startRayPosition, startRayPosition + (Vector3.down * LEG_VERTICAL_OFFSET), Color.blue, 0.016f);

            RaycastHit[] allHit = Physics.RaycastAll(startRayPosition, Vector3.down, LEG_VERTICAL_OFFSET * 3);

            if (allHit.Length > 0)
            {

                Vector3 upperPoint = Vector3.zero;

                foreach (RaycastHit hit in allHit)
                {
                    if (hit.point.y > upperPoint.y)
                    {
                        upperPoint = hit.point;
                    }
                }

                t.position = upperPoint;
            }
        }
    }

    private void SetBodyHeight()
    {
        float height = 0;

        height = (legs[0].GetChild(0).position.y + legs[1].GetChild(0).position.y);
        

        height /= 2;

        Body.position = new Vector3(Body.position.x, height + BODY_HEIGHT, Body.position.z);

    }

    private void RotateBody()
    {
        Roll();
        Pitch();
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

        Vector3 direction = right - left;

        float angle = Vector3.Angle(direction.normalized, Vector3.up);

        //Debug.Log(angle);

        Body.rotation = Quaternion.AngleAxis((angle - 90), Vector3.forward);

    }

    private void Pitch()
    {
        Vector3 front = Vector3.zero;
        Vector3 back = Vector3.zero;

        front = (legs[0].GetChild(0).position + legs[0].GetChild(0).position) / 2;
        back = (legs[legs.Length - 1].GetChild(0).position + legs[legs.Length - 2].GetChild(0).position) / 2;

        Vector3 direction = back - front;

        float angle = Vector3.Angle(direction.normalized, Vector3.up);

        Debug.Log(angle);

        Body.rotation = Quaternion.AngleAxis((angle - 90), Vector3.right);

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
        }*/

        Vector3[] bbb = _myController.bbb();

        foreach(Vector3 bb in bbb)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bb,0.5f);
        }

    }

}
