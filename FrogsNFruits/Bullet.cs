using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Transform _target;


    [Header("Bullet Options")]
    [SerializeField] GameObject _impactEffect;
    [SerializeField] float _speed = 70f;
    [SerializeField] int _damage = 50;
    [SerializeField] bool _doesDotDamg;
    [ToolTip("Keep this at 0 if the bullet is not explosive")]
    [SerializeField] float _explosionRadius = 0f;
    [SerializeField] float _dotDamgPerTick;
    [SerializeField] float _dotDuration = 5;

    public void Seek(Transform newTarget)
    {
        _target = newTarget;
    }

    void Update()
    {
        if (_target == null)
        {
            Destroy(gameObject, 3f);
            return;
        }

        Vector3 dir = _target.position - transform.position;
        float distanceThisFrame = _speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(_target);
    }

    void HitTarget()
    {
        GameObject effectIns = Instantiate(_impactEffect, transform.position, transform.rotation);
        Destroy(effectIns, 2f);

        Enemy enemy = _target.GetComponent<Enemy>();

        if (_explosionRadius > 0)
        {
            Explode();
        }
        else if (_doesDotDamg == false)
        {
            Damage(_damage, enemy);
        }
        if (_doesDotDamg == true)
        {
            if (enemy.hasDot == false)
            {
                enemy.StartCoroutine(enemy.DamageOverTime(_dotDuration, _dotDamgPerTick, this.gameObject));
            }
        }

        Destroy(gameObject);

    }
    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                Debug.Log("Explosion damage" + _damage);
                Damage(_damage, collider.GetComponent<EnemyMovement>());
            }
        }
    }
    void Damage(float damgToDo, EnemyMovement targetToDamg)
    {
        //EnemyMovement e = targetToDamg.GetComponent<EnemyMovement>();
        if (targetToDamg != null)
        {
            targetToDamg.TakeDamage(damgToDo);

        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}
