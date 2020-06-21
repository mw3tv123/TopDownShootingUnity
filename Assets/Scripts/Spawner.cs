using System;
using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public Enemy Enemy;
    public Wave[] Waves;

    public event Action<int> OnNewWave;

    // Properties for avoid player camping
    private LivingEntity playerEntity;
    private Transform playerTransform;
    private float distanceBetweenCampingPosition = 1.5f;
    private float timeBetweenCampingChecks = 2f;
    private float nextCampCheckTime;
    private Vector3 oldCampingPosition;
    private bool isCamping;

    // Properties for spawn enemies
    private Wave currentWave;
    private int currentWaveNumber;
    private int enemiesRemainingToSpawn;
    private int enemiesRemainingAlive;
    private float spawnerDelayTime;
    private MapGenerator map;

    private bool isDisable;

    private void Start () {
        playerEntity = FindObjectOfType<Player> ();
        playerEntity.OnDeath += OnPlayerDeath;
        playerTransform = playerEntity.transform;

        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        oldCampingPosition = playerTransform.position;

        map = FindObjectOfType<MapGenerator> ();
        NextWave ();
    }

    private void Update () {
        // If Player dead then game is freeze
        if (isDisable) return;

        if (Time.time > nextCampCheckTime) {
            nextCampCheckTime = Time.time + timeBetweenCampingChecks;
            isCamping = Vector3.Distance (playerTransform.position, oldCampingPosition) < distanceBetweenCampingPosition;
            oldCampingPosition = playerTransform.position;
        }
        if ((enemiesRemainingToSpawn > 0 || currentWave.IsEndlessMode ) && Time.time > spawnerDelayTime) {
            enemiesRemainingToSpawn--;
            spawnerDelayTime = Time.time + currentWave.RespawnTime;
            StartCoroutine (SpawnEnemy ());
        }
    }

    private void OnPlayerDeath () => isDisable = true;

    private void OnEnemyDeath () {
        enemiesRemainingAlive--;
        if (enemiesRemainingAlive == 0) {
            NextWave ();
        }
    }

    private void NextWave () {
        currentWaveNumber++;
        if (currentWaveNumber - 1 < Waves.Length) {
            currentWave = Waves[currentWaveNumber - 1];

            enemiesRemainingToSpawn = currentWave.NumberOfEnemies;
            enemiesRemainingAlive = enemiesRemainingToSpawn;

            OnNewWave?.Invoke (currentWaveNumber);
            ResetPlayerPosition ();
        }
    }

    private IEnumerator SpawnEnemy () {
        const float spawnDelay = 1f;
        const float tileFlashSpeed = 4f;

        Transform spawnPosition = map.GetRandomOpenTile ();
        if (isCamping) {
            spawnPosition = map.GetTileFromPosition (playerTransform.position);
        }
        Material tileMaterial = spawnPosition.GetComponent<Renderer> ().material;
        Color originalColor = tileMaterial.color;
        Color flashColor = Color.red;
        float spawnTimer = 0f;
        while (spawnTimer < spawnDelay) {
            spawnTimer += Time.deltaTime;
            tileMaterial.color = Color.Lerp (originalColor, flashColor, Mathf.PingPong (spawnTimer * tileFlashSpeed, 1));
            yield return null;
        }
        Enemy spawedEnemy = Instantiate (Enemy, spawnPosition.position + Vector3.up, Quaternion.identity);
        spawedEnemy.OnDeath += OnEnemyDeath;
        spawedEnemy.SetCharacteristics (
            currentWave.DamageModifier,
            currentWave.HealthModifier,
            currentWave.MoveSpeedModifier,
            currentWave.SkinColor
            );
    }

    private void ResetPlayerPosition () {
        playerTransform.position = map.GetTileFromPosition (Vector3.zero).position + Vector3.up * 3;
    }

    [Serializable]
    public class Wave {
        public int NumberOfEnemies;
        public float RespawnTime;

        public float MoveSpeedModifier = 1f;
        public float DamageModifier = 1f;
        public float HealthModifier = 1f;

        public Color SkinColor;
        public bool IsEndlessMode;
    }
}
