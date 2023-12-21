using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{

	[HideInInspector]
	public float speed;
	public float health;

	[SerializeField] int _worth = 50;

	[Header("Unity Setup")]
	[SerializeField] float _startHealth = 100;
	[SerializeField] float _startSpeed = 10f;
	[SerializeField] Image _healthBar;
	[SerializeField] GameObject _deathEffect;

	private bool _isDead = false;

	public bool hasDot;

	void Start()
	{
		speed = _startSpeed;
		health = _startHealth;
	}

	public void TakeDamage(float amount)
	{
		health -= amount;

		_healthBar.fillAmount = health / _startHealth;


		if (health <= 0 && !_isDead)
		{
			Die();
		}
	}
	public IEnumerator DamageOverTime(float dotDuration, float dotDamgPerTick)
	{
		hasDot = true;
		float dotTimer = 0;

		while (dotTimer < dotDuration)
		{
			dotTimer += 1;
			TakeDamage(dotDamgPerTick);
			yield return new WaitForSeconds(1);
		}
		hasDot = false;
	}

	public void Slow(float pct)
	{
		speed = _startSpeed * (1f - pct);
	}

	void Die()
	{
		_isDead = true;

		PlayerStats.Money += _worth;

		GameObject effect = (GameObject)Instantiate(_deathEffect, transform.position, Quaternion.identity);
		Destroy(effect, 5f);

		WaveSystem.EnemiesAlive--;

		Destroy(GetComponent<WaypointSystem>().emptyParent);
	}

}
