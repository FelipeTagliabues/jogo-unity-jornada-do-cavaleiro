using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MedievalJourney
{
    [DisallowMultipleComponent]
    public sealed class GameUI : MonoBehaviour
    {
        [Header("Paineis")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private OpeningSequenceUI openingSequence;

        [Header("HUD")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text defeatedText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text messageText;

        [Header("Resultados")]
        [SerializeField] private Text victoryStatsText;
        [SerializeField] private Text defeatStatsText;

        private Coroutine messageRoutine;

        public void Configure(GameObject menu, GameObject hud, GameObject pause, GameObject victory,
            GameObject defeat, OpeningSequenceUI introduction, Text score, Text timer, Text health,
            Text defeated, Text objective, Text message, Text victoryStats, Text defeatStats)
        {
            mainMenuPanel = menu;
            hudPanel = hud;
            pausePanel = pause;
            victoryPanel = victory;
            defeatPanel = defeat;
            openingSequence = introduction;
            scoreText = score;
            timerText = timer;
            healthText = health;
            defeatedText = defeated;
            objectiveText = objective;
            messageText = message;
            victoryStatsText = victoryStats;
            defeatStatsText = defeatStats;
        }

        public void ShowPhase(GamePhase phase)
        {
            SetActive(mainMenuPanel, phase == GamePhase.MainMenu);
            SetActive(openingSequence != null ? openingSequence.gameObject : null,
                phase == GamePhase.Introduction);
            SetActive(hudPanel, phase == GamePhase.Playing || phase == GamePhase.Paused);
            SetActive(pausePanel, phase == GamePhase.Paused);
            SetActive(victoryPanel, phase == GamePhase.Won);
            SetActive(defeatPanel, phase == GamePhase.Lost);
        }

        public bool BeginIntroduction()
        {
            if (openingSequence == null)
            {
                return false;
            }

            openingSequence.Begin();
            return true;
        }

        public void SkipIntroduction()
        {
            openingSequence?.Skip();
        }

        public void RefreshAll(GameManager manager, HealthSystem playerHealth)
        {
            if (manager == null)
            {
                return;
            }

            if (scoreText != null)
            {
                scoreText.text = $"Pontos: {manager.Score:0000}";
            }

            if (defeatedText != null)
            {
                defeatedText.text = $"Inimigos: {manager.EnemiesDefeated}";
            }

            if (healthText != null && playerHealth != null)
            {
                healthText.text = $"Vida: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}";
                healthText.color = playerHealth.CurrentHealth <= 1
                    ? new Color(1f, 0.35f, 0.25f)
                    : new Color(1f, 0.91f, 0.55f);
            }

            UpdateTimer(manager.ElapsedTime);
        }

        public void UpdateTimer(float elapsedSeconds)
        {
            if (timerText == null)
            {
                return;
            }

            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds));
            timerText.text = $"Tempo: {totalSeconds / 60:00}:{totalSeconds % 60:00}";
        }

        public void SetObjective(string text)
        {
            if (objectiveText != null)
            {
                objectiveText.text = text;
            }
        }

        public void ShowTemporaryMessage(string text, float duration)
        {
            if (messageText == null)
            {
                return;
            }

            if (messageRoutine != null)
            {
                StopCoroutine(messageRoutine);
            }

            messageRoutine = StartCoroutine(MessageRoutine(text, duration));
        }

        public void SetFinalStats(int score, float time, int defeated, int expected)
        {
            int seconds = Mathf.Max(0, Mathf.FloorToInt(time));
            string stats = $"Pontuacao final: {score}\nTempo: {seconds / 60:00}:{seconds % 60:00}\n" +
                           $"Inimigos derrotados: {defeated}/{expected}";
            if (victoryStatsText != null)
            {
                victoryStatsText.text = stats;
            }

            if (defeatStatsText != null)
            {
                defeatStatsText.text = stats;
            }
        }

        private IEnumerator MessageRoutine(string text, float duration)
        {
            messageText.text = text;
            messageText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(duration);
            messageText.gameObject.SetActive(false);
            messageRoutine = null;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
