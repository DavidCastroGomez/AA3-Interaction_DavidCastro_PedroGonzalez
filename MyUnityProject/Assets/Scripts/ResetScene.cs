using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetScene : MonoBehaviour
{
    [SerializeField]
    private GameObject ball;

    [SerializeField]
    private GameObject scorpion;

    [SerializeField]
    private GameObject octopus;

    [SerializeField]
    private GameObject blueTarget;

    private Vector3 ballPosition;
    private Vector3 scorpionPosition;

    private IK_Scorpion iK_Scorpion;

    private void Awake()
    {
        ballPosition = ball.transform.position;
        scorpionPosition = scorpion.transform.position;

        iK_Scorpion = scorpion.GetComponent<IK_Scorpion>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            iK_Scorpion.ResetScorpion(scorpionPosition);

            ball.transform.position = ballPosition;
            ball.GetComponent<MovingBall>().ResetBall();

            octopus.GetComponent<IK_tentacles>()._myController.ResetTentacles();

            blueTarget.GetComponent<MovingTarget>().ResetTarget();
        }
    }
}
