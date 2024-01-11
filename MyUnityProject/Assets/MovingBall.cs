using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBall : MonoBehaviour
{
    [SerializeField]
    IK_tentacles _myOctopus;

    [SerializeField]
    IK_Scorpion _scorpion;

    //movement speed in units per second
    [Range(-1.0f, 5.0f)]
    [SerializeField]
    private float _movementSpeed = 2.5f;

    [SerializeField]
    private Transform directionTarget;

    Vector3 _dir;

    bool _isMoving = false;

    int multiplier = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.identity;

        //get the Input from Horizontal axis
        float horizontalInput = Input.GetAxis("Horizontal");
        //get the Input from Vertical axis
        float verticalInput = Input.GetAxis("Vertical");

        //update the position
        //transform.position = transform.position + new Vector3(-horizontalInput * _movementSpeed * Time.deltaTime, verticalInput * _movementSpeed * Time.deltaTime, 0);

        if (_isMoving)
        {
            transform.position += _scorpion.forceSlider.value * _dir * Time.deltaTime * multiplier;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("TailTop"))
        {
            _isMoving = true;
            _dir = directionTarget.position - transform.position;
            _dir.Normalize();
            _myOctopus.NotifyShoot();
        }

        if (collision.transform.CompareTag("TentacleTop"))
        {
            multiplier = 0;
        }
    }

    public void ResetBall()
    {
        _isMoving = false;
        multiplier = 1;
    }
}
