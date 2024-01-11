using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

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
        Vector3 newVelocity = acceleration * Time.deltaTime;
        velocity += newVelocity + new Vector3(-magnus, 0, 0);
        position += velocity * Time.deltaTime;
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

    Vector3 _dir;

    public bool _isMoving = false;

    OurRB thisRB = new OurRB();

    void Start()
    {
        thisRB.position = transform.position;
        thisRB.gravity.y = -0.981f;
    }

    void Update()
    {
        if (_isMoving)
        {
            thisRB.Update();
            transform.position = thisRB.position;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("TailTop"))
        {
            _isMoving = true;

            _dir = directionTarget.position - transform.position;
            _dir.Normalize();

            thisRB.acceleration = thisRB.gravity;
            thisRB.velocity = _scorpion.forceSlider.value * _dir;
            thisRB.magnus = _scorpion.magnusSlider.value;

            _myOctopus.NotifyShoot();
        }

        if (collision.transform.CompareTag("TentacleTop"))
        {
            thisRB.acceleration = Vector3.zero;
            thisRB.velocity = Vector3.zero;
            thisRB.magnus = 0;
        }
    }

    public void ResetBall()
    {
        _isMoving = false;
        thisRB.position = transform.position;
        thisRB.acceleration = Vector3.zero;
        thisRB.velocity = Vector3.zero;
        thisRB.magnus = 0;
    }
}