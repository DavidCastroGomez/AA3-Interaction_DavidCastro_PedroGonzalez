using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using static UnityEditor.Progress;

class OurRB
{
    public Vector3 acceleration;
    public Vector3 gravity;
    public Vector3 velocity;
    public Vector3 position;
    public float magnus;

    public OurRB()
    {
        acceleration = new Vector3();
        gravity = new Vector3();
        velocity = new Vector3();
        position = new Vector3();
    }

    public void Update()
    {
        velocity += (acceleration + new Vector3(-magnus, 0, 0)) * Time.deltaTime;
        position += velocity * Time.deltaTime;
    }

    public Vector3 UpdateTrajectory(Vector3 pos, int magnusMultiplier)
    {
        velocity += (acceleration + new Vector3(-magnus * magnusMultiplier, 0, 0)) * Time.deltaTime;
        return pos + velocity * Time.deltaTime;
    }
}

public class MovingBall : MonoBehaviour
{
    [SerializeField]
    IK_tentacles _myOctopus;

    [SerializeField]
    IK_Scorpion _scorpion;

    [Range(-1.0f, 5.0f)]
    [SerializeField]
    private float _movementSpeed = 2.5f;

    [SerializeField]
    private Transform directionTarget;

    [SerializeField]
    private GameObject trajectoryWithMagnus;
    [SerializeField]
    private GameObject trajectoryWithoutMagnus;

    [SerializeField]
    private GameObject _goal;

    [SerializeField]
    GameObject canvasText;

    Vector3 _dir;
    Vector3 _ogPosition;

    static int numOfRB = 3;
    OurRB[] RB = new OurRB[numOfRB];

    public bool _isMoving = false;
    public bool _showTrajectory = false;

    [SerializeField]
    GameObject instantaneousVelocity;

    [SerializeField]
    GameObject gravityForce;

    [SerializeField]
    GameObject magnusForce;

    void Start()
    {
        _ogPosition = transform.position;

        for (int iter = 0; iter < numOfRB; iter++)
        {
            RB[iter] = new OurRB();
            RB[iter].position = transform.position;
            RB[iter].gravity.y = -0.981f;
        }

        UpdateRBValues();

        instantaneousVelocity.GetComponent<Renderer>().enabled = false;
        gravityForce.GetComponent<Renderer>().enabled = false;
        magnusForce.GetComponent<Renderer>().enabled = false;
    }

    void Update()
    {
        UpdateDir();

        instantaneousVelocity.transform.localScale = new Vector3(
            instantaneousVelocity.transform.localScale.x,
            instantaneousVelocity.transform.localScale.y, 
            _scorpion.forceSlider.value);

        gravityForce.transform.localScale = new Vector3(
            gravityForce.transform.localScale.x,
            gravityForce.transform.localScale.y,
            -RB[0].gravity.y);

        magnusForce.transform.rotation = Quaternion.AngleAxis(_scorpion.magnusSlider.value * 5 + 180, Vector3.up);

        if (_isMoving)
        {
            RB[0].Update();
            transform.position = RB[0].position;
            canvasText.GetComponent<Text>().text = RB[0].velocity.x.ToString() + " degrees/second";
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.I))
            {
                _showTrajectory = !_showTrajectory;
                instantaneousVelocity.GetComponent<Renderer>().enabled = _showTrajectory;
                gravityForce.GetComponent<Renderer>().enabled = _showTrajectory;
                magnusForce.GetComponent<Renderer>().enabled = _showTrajectory;
            }
            if (_showTrajectory)
            {
                trajectoryWithMagnus.transform.position = RB[1].UpdateTrajectory(trajectoryWithMagnus.transform.position, 1);
                trajectoryWithoutMagnus.transform.position = RB[2].UpdateTrajectory(trajectoryWithoutMagnus.transform.position, 0);
            }
            if (trajectoryWithMagnus.transform.position.z < _goal.transform.position.z)
            {
                trajectoryWithMagnus.GetComponent<ParticleSystem>().Clear();
                trajectoryWithMagnus.transform.position = _ogPosition;
                trajectoryWithMagnus.GetComponent<ParticleSystem>().Clear();
                UpdateMagnusRBValues();
            }
            if (trajectoryWithoutMagnus.transform.position.z < _goal.transform.position.z)
            {
                trajectoryWithoutMagnus.GetComponent<ParticleSystem>().Clear();
                trajectoryWithoutMagnus.transform.position = _ogPosition;
                trajectoryWithoutMagnus.GetComponent<ParticleSystem>().Clear();
                UpdateNoMagnusRBValues();
            }

            canvasText.GetComponent<Text>().text = "Shoot to see angular velocity.";
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("TailTop"))
        {
            _isMoving = true;
            _myOctopus.NotifyShoot();
            UpdateRBValues();

            trajectoryWithMagnus.transform.position = _ogPosition;
            trajectoryWithMagnus.GetComponent<Renderer>().enabled = false;

            trajectoryWithoutMagnus.transform.position = _ogPosition;
            trajectoryWithoutMagnus.GetComponent<Renderer>().enabled = false;

            _scorpion._myController.playTailAnimation = false;
        }

        if (collision.transform.CompareTag("TentacleTop"))
        {
            RB[0].acceleration = Vector3.zero;
            RB[0].velocity = Vector3.zero;
            RB[0].magnus = 0;
        }
    }

    public void ResetBall()
    {
        _isMoving = false;
        for (int iter = 0; iter < numOfRB; iter++)
        {
            RB[iter].position = transform.position;
            RB[iter].acceleration = Vector3.zero;
            RB[iter].velocity = Vector3.zero;
            RB[iter].magnus = 0;
        }
        
        trajectoryWithMagnus.GetComponent<Renderer>().enabled = true;
        trajectoryWithoutMagnus.GetComponent<Renderer>().enabled = true;
        UpdateRBValues();
    }

    public void UpdateRBValues()
    {
        UpdateDir();
        for (int iter = 0; iter < numOfRB; iter++)
        {
            RB[iter].velocity = _scorpion.forceSlider.value * _dir;
            RB[iter].magnus = _scorpion.magnusSlider.value;
        }
    }

    private void UpdateDir()
    {
        _dir = directionTarget.position - transform.position;
        _dir.Normalize();
    }

    public void UpdateMagnusRBValues()
    {
        RB[1].velocity = _scorpion.forceSlider.value * _dir;
        RB[1].magnus = _scorpion.magnusSlider.value;
    }

    public void UpdateNoMagnusRBValues()
    {
        RB[2].velocity = _scorpion.forceSlider.value * _dir;
        RB[2].magnus = _scorpion.magnusSlider.value;
    }
}