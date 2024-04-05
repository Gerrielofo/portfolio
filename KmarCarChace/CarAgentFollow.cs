using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class CarAgentFollow : MonoBehaviour
{
    [SerializeField] NavMeshAgent _carAgent;

    [Header("Settings")]
    [SerializeField] GameObject _carAgentPrefab;

    [SerializeField] WheelCollider[] _wheelColliders;

    [SerializeField] Transform[] _wheelTransforms;

    [SerializeField] float _minSpeedForTurn = 2f;
    [SerializeField] float _maxSteerAngle;
    [SerializeField] float _maxWheelTorque;
    [SerializeField] float _brakeTorque;

    [SerializeField] int _maxReroutes = 5;
    [SerializeField] float _timeToDespawn = 10f;
    [SerializeField] float[] _slopeAngles;

    [SerializeField] MeshRenderer[] _colorChangeParts;

    [Header("Arrest Setting")]
    [SerializeField] float _maxArrestSpeed = 1f;
    [SerializeField] float _timeToArrest = 3f;
    float _arrestTimer;
    [SerializeField] float _arrestRange = 4f;
    [SerializeField] LayerMask _policeMask;


    [Header("Info")]
    [SerializeField] float _slopeMultiplier = 1f;
    [SerializeField] float _currentSpeed;
    [SerializeField] Vector3 localTarget;
    [SerializeField] float targetAngle;
    public bool isAlive;

    [SerializeField] float _preferredDistanceFromAgent;
    float _distanceFromAgent;

    float _despawnTimer;
    int _rerouteAttempts;

    RaycastHit _slopeHit;


    void Start()
    {
        _carAgent = Instantiate(_carAgentPrefab, transform.position + transform.forward * 2, transform.rotation).GetComponent<NavMeshAgent>();
        _carAgent.GetComponent<CarAgent>().carTransform = transform;
        _preferredDistanceFromAgent = _carAgent.GetComponent<CarAgent>().CarRange / 2.5f;
    }

    public void ChangeColor(Color newColor)
    {
        for (int i = 0; i < _colorChangeParts.Length; i++)
        {
            _colorChangeParts[i].material.color = newColor;
        }
    }

    void Update()
    {
        if (!isAlive)
            return;

        if (_currentSpeed < _maxArrestSpeed)
        {
            CheckArrest();
        }
        else
        {
            _arrestTimer = 0f;
        }

        CalculateSlopeSpeed();

        if (_currentSpeed < 0.1f)
        {
            _despawnTimer += Time.deltaTime;
            if (_rerouteAttempts > _maxReroutes)
            {
                Destroy(_carAgent.gameObject);
                Destroy(gameObject);
            }
            if (_despawnTimer > _timeToDespawn)
            {
                _carAgent.GetComponent<CarAgent>().GetNewRandomWaypoint();
                _rerouteAttempts++;
                _despawnTimer = 0f;
            }
        }
        else
        {
            _rerouteAttempts = 0;
            _despawnTimer = 0;
        }

        _distanceFromAgent = Vector3.Distance(transform.position, _carAgent.transform.position);
        _currentSpeed = GetComponent<Rigidbody>().velocity.magnitude;


        localTarget = transform.InverseTransformPoint(_carAgent.transform.position);
        targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        CalculateSteerAngle();
        HandleAcceleration();
    }

    void CheckArrest()
    {
        int amountOfPolice;
        Collider[] colliders = Physics.OverlapSphere(transform.position, _arrestRange, _policeMask);
        amountOfPolice = colliders.Length;

        if (_arrestTimer < _timeToArrest)
        {
            _arrestTimer += Time.deltaTime * amountOfPolice;
        }
        else
        {
            GetComponent<Enemy>().Die();
        }
    }

    void CalculateSteerAngle()
    {
        _wheelColliders[0].steerAngle = Mathf.Clamp(targetAngle, -_maxSteerAngle, _maxSteerAngle);
        _wheelColliders[1].steerAngle = Mathf.Clamp(targetAngle, -_maxSteerAngle, _maxSteerAngle);

        for (int i = 0; i < _wheelTransforms.Length; i++)
        {
            Vector3 pos;
            Quaternion rot;
            _wheelColliders[i].GetWorldPose(out pos, out rot);
            _wheelTransforms[i].position = pos;
            _wheelTransforms[i].rotation = rot;
        }
    }

    void HandleAcceleration()
    {
        if (targetAngle > _maxSteerAngle && _currentSpeed > _minSpeedForTurn)
        {
            Brake();
        }
        else if (_distanceFromAgent < _preferredDistanceFromAgent)
        {
            Brake();
        }
        else
        {
            if (_currentSpeed < _carAgent.speed)
            {
                UnBrake();
                _wheelColliders[2].motorTorque = _maxWheelTorque * _slopeMultiplier;
                _wheelColliders[3].motorTorque = _maxWheelTorque * _slopeMultiplier;
            }
            else
            {
                Idle(_currentSpeed / _carAgent.speed);
            }

        }
    }

    float GetSlopAngle()
    {
        float angle = 0;

        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit))
        {
            angle = Vector3.Angle(_slopeHit.normal, Vector3.up);
        }

        return angle;
    }

    void CalculateSlopeSpeed()
    {
        if (GetSlopAngle() > _slopeAngles[0])
        {
            if (GetSlopAngle() > _slopeAngles[1])
            {
                if (GetSlopAngle() > _slopeAngles[2])
                {
                    _slopeMultiplier = 2.5f;
                    return;
                }
                _slopeMultiplier = 2f;
                return;
            }
            _slopeMultiplier = 1.5f;
        }
        else
        {
            _slopeMultiplier = 1f;
        }
    }

    void Brake()
    {
        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            _wheelColliders[i].brakeTorque = _brakeTorque * _currentSpeed;
            _wheelColliders[i].motorTorque = 0;
        }
    }

    void UnBrake()
    {
        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            _wheelColliders[i].brakeTorque = 0;
        }
    }

    void Idle(float currentSpeed)
    {
        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            _wheelColliders[i].brakeTorque = 0;
            _wheelColliders[i].motorTorque = currentSpeed;
        }
    }

    public void Die()
    {
        isAlive = false;
        GetComponent<CarCrash>().crash = true;
        Destroy(_carAgent.gameObject);
        Destroy(gameObject, 5f);
    }
}
