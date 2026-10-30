using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MedievalJourney
{
    public enum GamePhase
    {
        MainMenu,
        Introduction,
        Playing,
        Paused,
        Won,
        Lost
    }

    [DisallowMultipleComponent]
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private HealthSystem playerHealth;
        [SerializeField] private GameUI gameUI;
        [SerializeField] private GameAudio gameAudio;
        [SerializeField, Min(0)] private int expectedEnemies = 5;

        private static bool startImmediatelyAfterReload;

        public static GameManager Instance { get; private set; }
        public GamePhase Phase { get; private set; }
        public bool GameplayActive => Phase == GamePhase.Playing;
        public int Score { get; private set; }
        public int EnemiesDefeated { get; private set; }
        public float ElapsedTime { get; private set; }
        public bool BossDefeated { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Phase = GamePhase.MainMenu;
            Time.timeScale = 0f;
        }

        private void Start()
        {
            if (playerHealth != null)
            {
                playerHealth.Died += OnPlayerDied;
                playerHealth.HealthChanged += OnPlayerHealthChanged;
            }

            gameUI?.RefreshAll(this, playerHealth);
            gameUI?.ShowPhase(Phase);

            if (startImmediatelyAfterReload)
            {
                startImmediatelyAfterReload = false;
                StartGameImmediately();
            }
            else
            {
                gameAudio?.HandlePhase(Phase);
            }
        }

        private void Update()
        {
            if (Phase == GamePhase.Playing)
            {
                ElapsedTime += Time.deltaTime;
                gameUI?.UpdateTimer(ElapsedTime);
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (Phase == GamePhase.Playing)
                {
                    PauseGame();
                }
                else if (Phase == GamePhase.Paused)
                {
                    ResumeGame();
                }
            }
        }

        public void Configure(HealthSystem player, GameUI ui, GameAudio audio, int enemyCount)
        {
            playerHealth = player;
            gameUI = ui;
            gameAudio = audio;
            expectedEnemies = Mathf.Max(0, enemyCount);
        }

        public void StartGame()
        {
            ResetRunState();
            SetPhase(GamePhase.Introduction);
            if (gameUI == null || !gameUI.BeginIntroduction())
            {
                CompleteIntroduction();
            }
        }

        public void SkipIntroduction()
        {
            if (Phase == GamePhase.Introduction)
            {
                gameUI?.SkipIntroduction();
            }
        }

        public void CompleteIntroduction()
        {
            if (Phase != GamePhase.Introduction)
            {
                return;
            }

            BeginGameplay();
        }

        private void StartGameImmediately()
        {
            ResetRunState();
            BeginGameplay();
        }

        private void ResetRunState()
        {
            Score = 0;
            EnemiesDefeated = 0;
            ElapsedTime = 0f;
            BossDefeated = false;
            gameUI?.RefreshAll(this, playerHealth);
        }

        private void BeginGameplay()
        {
            SetPhase(GamePhase.Playing);
            gameUI?.SetObjective("Atravesse a aldeia, derrote os bandidos e enfrente o lider.");
            gameUI?.RefreshAll(this, playerHealth);
        }

        public void PauseGame()
        {
            if (Phase != GamePhase.Playing)
            {
                return;
            }

            SetPhase(GamePhase.Paused);
        }

        public void ResumeGame()
        {
            if (Phase != GamePhase.Paused)
            {
                return;
            }

            SetPhase(GamePhase.Playing);
        }

        public void RestartGame()
        {
            startImmediatelyAfterReload = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ReturnToMenu()
        {
            startImmediatelyAfterReload = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void RegisterEnemyDefeated(BanditAI2D enemy)
        {
            if (enemy == null || Phase != GamePhase.Playing)
            {
                return;
            }

            Score += enemy.ScoreReward;
            EnemiesDefeated++;
            if (enemy.IsBoss)
            {
                BossDefeated = true;
                gameUI?.SetObjective("Lider derrotado! Alcance o estandarte dourado.");
            }

            gameUI?.RefreshAll(this, playerHealth);
        }

        public void RegisterHealingCollected(int restoredHealth)
        {
            if (restoredHealth <= 0 || Phase != GamePhase.Playing)
            {
                return;
            }

            Score += restoredHealth * 25;
            gameUI?.ShowTemporaryMessage($"Provisoes encontradas: +{restoredHealth} vida", 1.8f);
            gameUI?.RefreshAll(this, playerHealth);
        }

        public void NotifyAttackBlocked()
        {
            if (Phase == GamePhase.Playing)
            {
                gameUI?.ShowTemporaryMessage("Ataque bloqueado!", 0.65f);
            }
        }

        public void NotifySwordAttack()
        {
            if (Phase == GamePhase.Playing)
            {
                gameAudio?.PlaySwordAttack();
            }
        }

        public void NotifyShieldDefense()
        {
            if (Phase == GamePhase.Playing)
            {
                gameAudio?.PlayShieldDefense();
            }
        }

        public void TryCompleteMission()
        {
            if (Phase != GamePhase.Playing)
            {
                return;
            }

            if (!BossDefeated)
            {
                gameUI?.ShowTemporaryMessage("O lider bandido ainda bloqueia a passagem!", 2.5f);
                return;
            }

            Score += 250;
            SetPhase(GamePhase.Won);
            gameUI?.SetFinalStats(Score, ElapsedTime, EnemiesDefeated, expectedEnemies);
        }

        private void OnPlayerDied(HealthSystem _)
        {
            gameAudio?.PlayDeathBell();
            StartCoroutine(ShowDefeatAfterDelay());
        }

        private IEnumerator ShowDefeatAfterDelay()
        {
            yield return new WaitForSecondsRealtime(1.1f);
            if (Phase == GamePhase.Playing)
            {
                SetPhase(GamePhase.Lost);
                gameUI?.SetFinalStats(Score, ElapsedTime, EnemiesDefeated, expectedEnemies);
            }
        }

        private void OnPlayerHealthChanged(int _, int __)
        {
            gameUI?.RefreshAll(this, playerHealth);
        }

        private void SetPhase(GamePhase phase)
        {
            Phase = phase;
            Time.timeScale = phase == GamePhase.Playing ? 1f : 0f;
            gameAudio?.HandlePhase(phase);
            gameUI?.ShowPhase(phase);
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= OnPlayerDied;
                playerHealth.HealthChanged -= OnPlayerHealthChanged;
            }

            if (Instance == this)
            {
                Instance = null;
            }

            Time.timeScale = 1f;
        }
    }
}
