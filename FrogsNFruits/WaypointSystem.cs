using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyMovement))]
public class WaypointSystem : MonoBehaviour
{
	private Transform _target;
	private int _waypointindex = 0;

	private EnemyMovement _enemy;
	[SerializeField] GameObject _emptyParent;

	void Start()
	{
		_enemy = GetComponent<EnemyMovement>();

		_target = Waypoint.points[0];
	}

	void Update()
	{
		if (OptionsUI.wantOptions || GameManager.GameIsOver)
			return;

		Vector3 dir = _target.position - transform.position;
		transform.Translate(dir.normalized * _enemy.speed * Time.deltaTime, Space.World);

		if (Vector3.Distance(transform.position, _target.position) <= 0.4f)
		{
			GetNextWaypoint();
		}

		_enemy.speed = _enemy.startSpeed;
		transform.LookAt(_target);
	}

	void GetNextWaypoint()
	{
		if (_waypointindex >= Waypoint.points.Length - 1)
		{
			EndPath();
			return;
		}

		_waypointindex++;
		_target = Waypoint.points[_waypointindex];
	}

	void EndPath()
	{
		PlayerStats.Lives--;
		WaveSystem.EnemiesAlive--;
		Destroy(_emptyParent);
	}

}
