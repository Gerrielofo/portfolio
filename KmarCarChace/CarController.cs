using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using Unity.XR.CoreUtils;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput = null;
    public PlayerInput PlayerInput => playerInput;

    Animator _animator;

    [SerializeField] Transform _helicopterFollowPoint;
    [SerializeField] Vector3 _helicopterOffset = new Vector3(10, 40, 0);

    [Header("Wheel Settings")]
    #region Wheel Settings
    [SerializeField] WheelCollider _leftFront;
    [SerializeField] WheelCollider _rightFront;
    [SerializeField] WheelCollider _leftBack;
    [SerializeField] WheelCollider _rightBack;

    [SerializeField] float _averageRpm;

    [SerializeField] bool _frontWheelDrive;

    [SerializeField] float _minAngle;
    [SerializeField] float _maxAngle;

    [SerializeField] float _steerSensitivity;
    [SerializeField] float _steerAngle;
    [SerializeField] Transform _steeringWheelRotation;
    [SerializeField] GameObject _steeringWheelHolder;
    #endregion

    [Header("Stats")]
    #region Stats
    [SerializeField] float _startHealth;
    [SerializeField] float _health;
    [SerializeField] Slider _healthSlider;
    [SerializeField] float _defence;
    #endregion

    [Header("Inputs")]
    #region Inputs
    [SerializeField] float _isMove;
    [SerializeField] float _isExtraMove;
    [SerializeField] float _isTurn;
    #endregion

    [Header("Engine Options")]
    #region Engine Options
    [SerializeField] float _reverseDelay = 1f;
    [SerializeField] bool _canReverse;
    [SerializeField] float _brakeForce = 1000f;
    [SerializeField] Gear[] _gears;
    [SerializeField] int _currentGear;
    float _reverseTimer;
    CarAudioController _carAudioController;
    #endregion

    [SerializeField] Vector3 _respawnHeightOffset = new Vector3(0, 1, 0);

    [SerializeField] float _velocity;

    [SerializeField] TMP_Text _gearTxt;



    [SerializeField] Transform _respawnTransform;
    [SerializeField] bool _isReset;

    [SerializeField] float _speed;

    [SerializeField] ParticleSystem _smokeEffect;

    [Header("Siren")]
    #region Siren
    [SerializeField] AudioSource _sirenAudio;
    [SerializeField] Animator _sirenAnimator;
    [SerializeField] GameObject _redLightGO;
    [SerializeField] GameObject _blueLightGO;
    #endregion


    [Header("Powerups")]
    #region Powerups
    [SerializeField] float _speedBoost = 1;
    [SerializeField] float _speedMultiplier = 1;
    [SerializeField] float _damageMultiplier = 1;

    [SerializeField] GameObject _spikestripPrefab;
    [SerializeField] Transform _spikestripSpawnPoint;
    #endregion

    [Header("Upgrades")]
    #region Upgrades
    [SerializeField] GameObject[] _carMods1;
    [SerializeField] GameObject[] _carMods2;
    [SerializeField] GameObject[] _carMods3;
    #endregion

    [SerializeField] ParticleSystem _speedParticle;
    [SerializeField] float _minSpeedForParticle = 10;

    private void OnEnable()
    {
        playerInput.actions.FindAction("GasBrake").started += OnGas;
        playerInput.actions.FindAction("GasBrake").performed += OnGas;
        playerInput.actions.FindAction("GasBrake").canceled += OnGas;

        playerInput.actions.FindAction("Turn").started += OnSteer;
        playerInput.actions.FindAction("Turn").performed += OnSteer;
        playerInput.actions.FindAction("Turn").canceled += OnSteer;

        playerInput.actions.FindAction("ExtraGasBrake").started += OnExtraMove;
        playerInput.actions.FindAction("ExtraGasBrake").performed += OnExtraMove;
        playerInput.actions.FindAction("ExtraGasBrake").canceled += OnExtraMove;

        playerInput.actions.FindAction("Reset").started += OnRespawm;
        playerInput.actions.FindAction("Reset").canceled += OnRespawm;
    }

    private void OnDisable()
    {
        playerInput.actions.FindAction("GasBrake").started -= OnGas;
        playerInput.actions.FindAction("GasBrake").performed -= OnGas;
        playerInput.actions.FindAction("GasBrake").canceled -= OnGas;

        playerInput.actions.FindAction("Turn").started -= OnSteer;
        playerInput.actions.FindAction("Turn").performed -= OnSteer;
        playerInput.actions.FindAction("Turn").canceled -= OnSteer;

        playerInput.actions.FindAction("ExtraGasBrake").started -= OnExtraMove;
        playerInput.actions.FindAction("ExtraGasBrake").performed -= OnExtraMove;
        playerInput.actions.FindAction("ExtraGasBrake").canceled -= OnExtraMove;

        playerInput.actions.FindAction("Reset").started -= OnRespawm;
        playerInput.actions.FindAction("Reset").canceled -= OnRespawm;
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();

        _carAudioController = GetComponent<CarAudioController>();

        _health = _startHealth;
        _healthSlider.maxValue = _startHealth;
        UpdateHealthSlider(_health);

        switch (PlayerPrefs.GetInt("carModIndex"))
        {
            case 0:
                _defence = 1f;
                break;
            case 1:
                _defence = 1.5f;
                break;
            case 2:
                _defence = 2f;
                break;
            case 3:
                _defence = 3f;
                break;
        }

        for (int i = 0; i < PlayerPrefs.GetInt("carModIndex"); i++)
        {
            if (i == 0)
            {
                for (int one = 0; one < _carMods1.Length; one++)
                {
                    _carMods1[one].SetActive(true);
                }
            }

            if (i == 1)
            {
                for (int two = 0; two < _carMods1.Length; two++)
                {
                    _carMods2[two].SetActive(true);
                }
            }

            if (i == 2)
            {
                for (int three = 0; three < _carMods1.Length; three++)
                {
                    _carMods3[three].SetActive(true);
                }
            }

        }
    }

    private void OnGas(InputAction.CallbackContext context)
    {
        _isMove = context.ReadValue<float>();

        if (_isMove > 0.5f)
        {
            _isMove = 1;
        }
        if (_isMove < -0.5f)
        {
            _isMove = -1;
        }
    }

    public void SpawnSpikeStrip(float duration)
    {
        GameObject spike = Instantiate(_spikestripPrefab, _spikestripSpawnPoint.position, _spikestripSpawnPoint.rotation);
        spike.GetComponent<SpikeStrip>().duration = duration;
    }

    private void OnSteer(InputAction.CallbackContext context)
    {
        _isTurn = context.ReadValue<Vector2>().x;
    }

    private void OnExtraMove(InputAction.CallbackContext context)
    {
        _isExtraMove = context.ReadValue<Vector2>().y;
        _isExtraMove /= 2;
    }

    bool _canRespawn = true;
    private void OnRespawm(InputAction.CallbackContext context)
    {
        if (_canRespawn)
        {
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        _canRespawn = false;
        transform.position = transform.position + _respawnHeightOffset; ;
        transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.y, 0));
        yield return new WaitForSeconds(1);
        _canRespawn = true;
    }

    private void Update()
    {
        _helicopterFollowPoint.position = transform.position + _helicopterOffset;

        float speed = _velocity;
        _healthSlider.value = _health;

        _animator.SetFloat("Speed", speed);

        if (_frontWheelDrive)
        {
            _averageRpm = (_leftBack.rpm + _leftFront.rpm + _rightBack.rpm + _rightFront.rpm) / 4;
        }
        else
        {
            _averageRpm = (_leftBack.rpm + _rightBack.rpm) / 2;
        }
        _carAudioController.rpm = _averageRpm;

        if (_velocity > _minSpeedForParticle)
        {
            EmitParticles();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.SetPositionAndRotation(_respawnTransform.position, _respawnTransform.rotation);
        }

        #region Wheel Control
        if (_isMove >= 0)
        {
            if (_canReverse)
            {
                _leftBack.brakeTorque = _brakeForce;
                _rightBack.brakeTorque = _brakeForce;

                _leftBack.motorTorque = 0;
                _rightBack.motorTorque = 0;

                if (_frontWheelDrive)
                {
                    _leftFront.brakeTorque = _brakeForce;
                    _rightFront.brakeTorque = _brakeForce;

                    _leftFront.motorTorque = 0;
                    _rightFront.motorTorque = 0;
                }

                if (MyApproximation(_velocity, 0, 0.5f))
                {
                    _canReverse = false;

                }
            }
            else
            {
                _leftBack.brakeTorque = 0;
                _rightBack.brakeTorque = 0;

                if (_frontWheelDrive)
                {
                    _leftFront.brakeTorque = 0;
                    _rightFront.brakeTorque = 0;
                }

                _leftBack.motorTorque = _speed;
                _rightBack.motorTorque = _speed;

                if (_frontWheelDrive)
                {
                    _leftFront.motorTorque = _speed;
                    _rightFront.motorTorque = _speed;
                }
            }
        }
        else
        {
            if (MyApproximation(_velocity, 0, 0.5f) && _reverseTimer <= 0f && !_canReverse)
            {
                _reverseTimer = _reverseDelay;
            }
            else if (_reverseTimer > 0)
            {
                _reverseTimer -= Time.deltaTime;
                if (_reverseTimer <= 0)
                {
                    _canReverse = true;
                }
            }

            if (_canReverse)
            {
                _leftBack.motorTorque = _speed;
                _rightBack.motorTorque = _speed;

                if (_frontWheelDrive)
                {
                    _leftFront.motorTorque = _speed;
                    _rightFront.motorTorque = _speed;
                }

                _leftBack.brakeTorque = 0;
                _rightBack.brakeTorque = 0;

                if (_frontWheelDrive)
                {
                    _leftFront.brakeTorque = 0;
                    _rightFront.brakeTorque = 0;
                }
            }
            else
            {
                _leftBack.brakeTorque = _brakeForce;
                _rightBack.brakeTorque = _brakeForce;

                if (_frontWheelDrive)
                {
                    _leftFront.brakeTorque = _brakeForce;
                    _rightFront.brakeTorque = _brakeForce;
                }

                _leftBack.motorTorque = 0;
                _rightBack.motorTorque = 0;

                if (_frontWheelDrive)
                {
                    _leftFront.motorTorque = 0;
                    _rightFront.motorTorque = 0;
                }
            }
        }
        #endregion

        _steerAngle = _steeringWheelRotation.eulerAngles.z / _steerSensitivity;
        Mathf.Clamp(_steerAngle, _minAngle, _maxAngle);

        _leftFront.steerAngle = -_steerAngle;
        _rightFront.steerAngle = -_steerAngle;

        _velocity = GetComponent<Rigidbody>().velocity.magnitude;

        if (_gearTxt != null)
            _gearTxt.text = (_currentGear + 1).ToString();

        HandleSpeed();
    }


    void HandleSpeed()
    {
        if (_currentGear + 1 == _gears.Length)
        {
            if (_velocity < _gears[_currentGear].minimumSpeed)
            {
                _currentGear--;
            }
            _speed = _gears[_currentGear].speed * _isMove * (1 + _isExtraMove) * _speedMultiplier * _speedBoost;
            return;
        }
        if (_currentGear != 0 && _velocity < _gears[_currentGear].minimumSpeed)
        {
            _currentGear--;
        }
        else if (_velocity >= _gears[_currentGear + 1].minimumSpeed)
        {
            _currentGear++;
        }

        _speed = _gears[_currentGear].speed * _isMove * (1 + _isExtraMove) * _speedMultiplier * _speedBoost;
    }

    void Die()
    {
        GameManager.Instance.EndGame();
    }

    private bool MyApproximation(float a, float b, float tolerance)
    {
        return Mathf.Abs(a - b) < tolerance;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Enemy"))
        {
            other.transform.GetComponent<Enemy>().TakeDamage(_velocity * _damageMultiplier);
            if (_defence < 3)
            {
                TakeDamage(_velocity);
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            Die();
        }
        else if (other.transform.CompareTag("Obstacle"))
        {
            TakeDamage(_velocity);
        }
    }

    void TakeDamage(float speed)
    {
        float damage = speed * 5;

        damage /= _defence;

        _health -= damage;
        UpdateHealthSlider(_health);

        if (_health < _startHealth / 2)
        {
            _smokeEffect.Play();
        }
        else
        {
            _smokeEffect.Stop();
        }

        if (_health < 0)
        {
            Die();
        }
    }

    void UpdateHealthSlider(float health)
    {
        _healthSlider.value = health;
    }

    void EmitParticles()
    {
        int emitionAmount = (int)(_speed / 500);
        _speedParticle.Emit(emitionAmount);
    }

    public void ToggleSiren()
    {
        if (_sirenAudio.isPlaying)
        {
            _sirenAudio.Stop();
            _sirenAnimator.SetBool("Siren", false);
            _redLightGO.SetActive(false);
            _blueLightGO.SetActive(false);
        }
        else
        {
            _sirenAudio.Play();
            _sirenAnimator.SetBool("Siren", true);
        }
    }

    public void SpeedBoost(float multiplier, float duration)
    {
        _speedBoost = multiplier / 100;
        StartCoroutine(StopSpeedBoost(duration));
    }

    public void SetSpeedMultiplier(float prc)
    {
        _speedBoost = prc / 100;
    }

    public void SetDamageMutliplier(float prc)
    {
        _damageMultiplier = prc / 100;
    }

    private IEnumerator StopSpeedBoost(float duration)
    {
        yield return new WaitForSeconds(duration);
        _speedMultiplier = 1;
    }

}
