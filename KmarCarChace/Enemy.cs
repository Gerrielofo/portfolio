using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    CarAgentFollow _carAI;
    [Header("Enemy Settings")]
    [SerializeField] float _startHealth;
    [SerializeField] float _startSpeed;
    [SerializeField] float _damageMultiplier = 5f;
    [SerializeField] float _maxPointGain;
    [SerializeField] float _damageInterval;

    [Header("UI")]
    [SerializeField] Transform _healthCanvas;
    [SerializeField] Slider _healthSlider;
    [SerializeField] float _healthShowTime = 3f;
    float _healthTimer;

    [Header("Info")]
    [SerializeField] float _health;
    [SerializeField] float _currentSpeed;
    [SerializeField] bool _isAlive;
    [SerializeField] float _timeAlive;

    void Start()
    {
        _carAI = GetComponent<CarAgentFollow>();
        if (GameOptionsManager.Instance)
        {
            _health = _startHealth * (GameOptionsManager.Instance.enemyHealthPrc / 100);
            _healthSlider.maxValue = _startHealth * (GameOptionsManager.Instance.enemyHealthPrc / 100);
        }
        else
        {
            _health = _startHealth;
            _healthSlider.maxValue = _startHealth;
        }
        _healthSlider.value = _health;
        _carAI.isAlive = true;
        _isAlive = true;
    }

    void Update()
    {
        _timeAlive += Time.deltaTime;
        if (_healthTimer > 0)
        {
            _healthTimer -= Time.deltaTime;
            _healthCanvas.LookAt(Camera.main.transform);
        }
        else if (_healthCanvas.gameObject.activeSelf)
        {
            _healthCanvas.gameObject.SetActive(false);
        }

        _currentSpeed = GetComponent<Rigidbody>().velocity.magnitude;
    }

    public void TakeDamage(float playerSpeed)
    {
        if (!_isAlive)
        {
            return;
        }

        _healthCanvas.gameObject.SetActive(true);
        _healthTimer = _healthShowTime;

        _health -= playerSpeed * _damageMultiplier;
        _healthSlider.value = _health;
        if (_health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        _isAlive = false;

        float pointMultiplier = _maxPointGain / (_timeAlive / 80);
        int pointsToGain = (int)pointMultiplier;

        if (pointsToGain > _maxPointGain)
        {
            pointsToGain = (int)_maxPointGain;
        }

        _carAI.isAlive = false;
        _healthCanvas.gameObject.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPoints(pointsToGain);
        }

        FindObjectOfType<EnemyManager>().RemoveEnemy(this);

        GetComponent<CarCrash>().Crash();

        GetComponent<CarAgentFollow>().Die();
    }
}
