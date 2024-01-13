using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTarget: MonoBehaviour
{
    [SerializeField]
    IK_tentacles _myOctopus;

    enum MovingMode {RANDOM, USERTARGET };

    [SerializeField]
    public int id = -1;

    [SerializeField]
    MovingMode _mode;

    [Range(0f,3.0f)]
    [SerializeField]
    private float _movementSpeed = 5f;

    [SerializeField]
    GameObject _region;
    float _xMin, _xMax, _yMin, _yMax;

    public Vector3 _dir;

    GameObject _goalRegion;

    [SerializeField]
    GameObject theBall;

    MovingBall myBall;

    Vector3 _ogPosition;

    void Start()
    {
        if (_mode == MovingMode.RANDOM) {
            if (_region == null)
            {
                Debug.LogError("moving targets in random mode need to have a region assigned to");
            }
            else 
            {
                _xMin = _region.transform.position.x - _region.transform.localScale.x / 2;
                _xMax = _region.transform.position.x + _region.transform.localScale.x / 2;
                _yMin = _region.transform.position.y - _region.transform.localScale.y / 2;
                _yMax = _region.transform.position.y + _region.transform.localScale.y / 2;
                float a = Random.Range(0.0f,1.0f);
                _dir = new Vector3(a, 1 - a, 0);
            }
        }

        if (_mode == MovingMode.USERTARGET)
        {
            myBall = theBall.GetComponent<MovingBall>();
        }

        _ogPosition = transform.position;
    }

    public void ResetTarget()
    {
        transform.position = _ogPosition;
    }

    void Update()
    {
        transform.rotation = Quaternion.identity;

        if (_mode == MovingMode.USERTARGET)
        {
            if (!myBall._isMoving)
            {
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");

                transform.position = transform.position + new Vector3(-horizontalInput * _movementSpeed * Time.deltaTime, verticalInput * _movementSpeed * Time.deltaTime, 0);
            }
            else
            {
                transform.position = new Vector3(theBall.transform.position.x, theBall.transform.position.y, transform.position.z);
            }
        }
        else if (_mode == MovingMode.RANDOM) 
        {
            Vector3 pos = new Vector3(_region.transform.position.x + 0.4f* _region.transform.localScale.z * Mathf.Cos(2.0f * Mathf.PI * _movementSpeed * Time.time),
                                      _region.transform.position.y + 0.4f *  _region.transform.localScale.y * Mathf.Sin(2.0f * Mathf.PI * _movementSpeed * Time.time), transform.position.z);

            transform.position = pos;
        } 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(_mode == MovingMode.USERTARGET)
            _myOctopus.NotifyTarget(transform, collision.collider.gameObject);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (_mode == MovingMode.USERTARGET)
            _myOctopus.NotifyTarget(transform, collision.collider.gameObject);
        else if(_mode == MovingMode.RANDOM){}
    }
}