using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class WaveSystem : MonoBehaviour
{
	[SerializeField] GameManager _gameManager;

	public static int EnemiesAlive = 0;

	[Header("settings")]
	[SerializeField] Wave[] waves;
	[SerializeField] Transform _spawnPoint;

	[Header("UI")]
	[SerializeField] Text _roundsSurvivedtxt;
	[SerializeField] Text _livestxt;


	private int _waveIndex = 0;

	private void Start()
	{
		EnemiesAlive = 0;
	}
	void Update()
	{
		_livestxt.text = PlayerStats.Lives.ToString() + " Lives";

		if (OptionsUI.optionsShown == true)
			return;

		if (EnemiesAlive > 0)
		{
			return;
		}

		if (_waveIndex == waves.Length)
		{
			_gameManager.WinLevel();
			this.enabled = false;
		}

		_roundsSurvivedtxt.text = PlayerStats.Rounds.ToString() + "/" + waves.Length.ToString();


		if (EnemiesAlive <= 0)
		{
			StartCoroutine(SpawnWave());
			return;
		}

	}

	IEnumerator SpawnWave()
	{
		PlayerStats.Rounds++;

		Wave wave = waves[_waveIndex];

		EnemiesAlive = wave.count;

		for (int i = 0; i < wave.count; i++)
		{
			SpawnEnemy(wave.enemy);
			yield return new WaitForSeconds(1f / wave.rate);
		}

		_waveIndex += 1;
	}

	void SpawnEnemy(GameObject enemy)
	{
		Instantiate(enemy, _spawnPoint.position, _spawnPoint.rotation);
	}

}