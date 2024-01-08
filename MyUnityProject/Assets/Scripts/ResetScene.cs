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

    private Vector3 ballPosition;
    private Vector3 scorpionPosition;

    private IK_Scorpion iK_Scorpion;

    private void Awake()
    {
        ballPosition = ball.transform.position;
        scorpionPosition = scorpion.transform.position;

        iK_Scorpion = scorpion.GetComponent<IK_Scorpion>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ball.transform.position = ballPosition;
            scorpion.transform.position = scorpionPosition;

            iK_Scorpion.ResetScorpion(scorpionPosition);
            ball.GetComponent<MovingBall>().ResetBall();

        }
    }
}
