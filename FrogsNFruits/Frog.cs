using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frog : MonoBehaviour
{
    [SerializeField] Transform _target;

    [Header("Attributes")]

    [SerializeField] float _range = 15f;
    [SerializeField] float _fireRate = 1f;
    [SerializeField] float _fireCountdown = 0f;

    [Header("Unity Setup Fields")]

    [SerializeField] Transform _partToRotate;
    [SerializeField] float _turnSpeed = 10f;

    [SerializeField] GameObject _bulletPrefab;
    [SerializeField] Transform _firePoint;

    public int sellAmount;
    public int upgradeCost;

    [SerializeField] string _enemyTag = "Enemy";
    [SerializeField] bool _enableDot = false;


    void Update()
    {
        if (GameManager.GameIsOver == true)
        {
            return;
        }
        else if (_target != null)
        {
            //Target lock system
            Vector3 dir = _target.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            Vector3 rotation = Quaternion.Lerp(_partToRotate.rotation, lookRotation, Time.deltaTime * _turnSpeed).eulerAngles;
            _partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);

            if (_fireCountdown <= 0f)
            {
                Shoot();
                _fireCountdown = 1f / _fireRate;
            }
            _fireCountdown -= Time.deltaTime;
        }

        UpdateTarget();

    }
    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(_enemyTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }
        if (nearestEnemy != null && shortestDistance <= _range)
        {
            _target = nearestEnemy.transform;
        }
        else
        {
            _target = null;
        }
    }
    void Shoot()
    {
        GameObject bulletGO = (GameObject)Instantiate(_bulletPrefab, _firePoint.position, _firePoint.rotation);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        bullet.doesDotDamg = _enableDot;
        bullet.Seek(_target);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, _range);
    }
}
