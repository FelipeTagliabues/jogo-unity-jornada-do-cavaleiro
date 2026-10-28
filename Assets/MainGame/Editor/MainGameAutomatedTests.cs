using System;
using System.IO;
using System.Linq;
using MedievalJourney;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MedievalJourneyEditor
{
    [InitializeOnLoad]
    public static class MainGameAutomatedTests
    {
        private const string RunningKey = "MedievalJourney.Smoke.Running";
        private const string ResultKey = "MedievalJourney.Smoke.Result";
        private const string ExitAfterRunKey = "MedievalJourney.Smoke.ExitAfterRun";
        private static int stage;
        private static double stageStartedAt;
        private static bool smokeLoopActive;
        private static double playRequestStartedAt;
        private static HealthSystem smokeEnemyHealth;

        static MainGameAutomatedTests()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (SessionState.GetBool(RunningKey, false))
            {
                EditorApplication.update -= ResumePendingSmoke;
                EditorApplication.update += ResumePendingSmoke;
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    playRequestStartedAt = EditorApplication.timeSinceStartup;
                    EditorApplication.update -= WaitBeforeEnteringPlay;
                    EditorApplication.update += WaitBeforeEnteringPlay;
                }
            }
        }

        [MenuItem("Projeto Medieval/Testar MainGame em Play Mode")]
        public static void RunPlayModeSmokeFromMenu()
        {
            BeginSmokeTest(false);
        }

        public static void RunPlayModeSmokeFromCommandLine()
        {
            BeginSmokeTest(true);
        }

        private static void BeginSmokeTest(bool exitAfterRun)
        {
            SessionState.SetBool(RunningKey, true);
            SessionState.SetString(ResultKey, string.Empty);
            SessionState.SetBool(ExitAfterRunKey, exitAfterRun);
            EditorSceneManager.OpenScene(MainGameSceneBuilder.ScenePath, OpenSceneMode.Single);
            playRequestStartedAt = EditorApplication.timeSinceStartup;
            EditorApplication.update -= WaitBeforeEnteringPlay;
            EditorApplication.update += WaitBeforeEnteringPlay;
        }

        public static void BuildWindowsValidationFromCommandLine()
        {
            string buildDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp",
                "MainGameValidationBuild"));
            BuildConfiguredScenes(buildDirectory, BuildOptions.Development, "Build de validacao aprovado");
        }

        [MenuItem("Projeto Medieval/Gerar build Windows de entrega")]
        public static void BuildWindowsReleaseFromCommandLine()
        {
            string buildDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds",
                "AJornadaDoCavaleiro"));
            BuildConfiguredScenes(buildDirectory, BuildOptions.None, "Build de entrega aprovado");
        }

        private static void BuildConfiguredScenes(string buildDirectory, BuildOptions options, string successLabel)
        {
            Directory.CreateDirectory(buildDirectory);
            string executable = Path.Combine(buildDirectory, "AJornadaDoCavaleiro.exe");
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0 || scenes[0] != MainGameSceneBuilder.ScenePath)
            {
                throw new InvalidOperationException("MainGame nao e a primeira cena habilitada para build.");
            }

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = executable,
                target = BuildTarget.StandaloneWindows64,
                options = options
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Build de validacao falhou: {report.summary.result}, {report.summary.totalErrors} erro(s).");
            }

            Debug.Log($"{successLabel}: {executable} ({report.summary.totalSize} bytes).");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(RunningKey, false))
            {
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                BeginSmokeLoop();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= RunSmokeStage;
                smokeLoopActive = false;
                string result = SessionState.GetString(ResultKey, "FALHA: resultado ausente.");
                SessionState.SetBool(RunningKey, false);
                if (result.StartsWith("APROVADO", StringComparison.Ordinal))
                {
                    Debug.Log(result);
                    if (SessionState.GetBool(ExitAfterRunKey, false))
                    {
                        EditorApplication.delayCall += () => EditorApplication.Exit(0);
                    }
                }
                else
                {
                    Debug.LogError(result);
                    if (SessionState.GetBool(ExitAfterRunKey, false))
                    {
                        EditorApplication.delayCall += () => EditorApplication.Exit(1);
                    }
                }
            }
        }

        private static void ResumePendingSmoke()
        {
            if (!SessionState.GetBool(RunningKey, false))
            {
                EditorApplication.update -= ResumePendingSmoke;
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                return;
            }

            EditorApplication.update -= ResumePendingSmoke;
            BeginSmokeLoop();
        }

        private static void WaitBeforeEnteringPlay()
        {
            if (!SessionState.GetBool(RunningKey, false))
            {
                EditorApplication.update -= WaitBeforeEnteringPlay;
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || EditorApplication.timeSinceStartup - playRequestStartedAt < 5d)
            {
                return;
            }

            EditorApplication.update -= WaitBeforeEnteringPlay;
            EditorApplication.EnterPlaymode();
        }

        private static void BeginSmokeLoop()
        {
            if (smokeLoopActive)
            {
                return;
            }

            smokeLoopActive = true;
            stage = 0;
            stageStartedAt = EditorApplication.timeSinceStartup;
            EditorApplication.update -= RunSmokeStage;
            EditorApplication.update += RunSmokeStage;
        }

        private static void RunSmokeStage()
        {
            try
            {
                GameManager manager = GameManager.Instance;
                if (manager == null)
                {
                    if (EditorApplication.timeSinceStartup - stageStartedAt > 10d)
                    {
                        throw new InvalidOperationException("GameManager nao iniciou em Play Mode.");
                    }

                    return;
                }

                if (stage == 0)
                {
                    Require(manager.Phase == GamePhase.MainMenu, "o jogo nao iniciou no menu principal.");
                    Require(Mathf.Approximately(Time.timeScale, 0f), "o menu nao pausou a simulacao inicial.");
                    manager.StartGame();
                    Require(manager.Phase == GamePhase.Introduction,
                        "StartGame nao abriu a introducao narrativa.");
                    Require(Mathf.Approximately(Time.timeScale, 0f),
                        "a introducao permitiu que a simulacao avancasse.");
                    Require(manager.Score == 0 && manager.EnemiesDefeated == 0 &&
                            Mathf.Approximately(manager.ElapsedTime, 0f),
                        "os sistemas da partida avancaram durante a introducao.");
                    OpeningSequenceUI introduction = UnityEngine.Object.FindAnyObjectByType<OpeningSequenceUI>();
                    Require(introduction != null && introduction.gameObject.activeInHierarchy &&
                            introduction.PageCount == 4,
                        "a apresentacao visual da introducao nao foi exibida corretamente.");
                    manager.SkipIntroduction();
                    stage = 1;
                    stageStartedAt = EditorApplication.timeSinceStartup;
                    return;
                }

                if (stage == 1 && EditorApplication.timeSinceStartup - stageStartedAt > 0.6d)
                {
                    Require(manager.Phase == GamePhase.Playing,
                        "pular a introducao nao iniciou a partida.");
                    Require(Mathf.Approximately(Time.timeScale, 1f),
                        "o gameplay permaneceu congelado apos a introducao.");
                    GameObject introductionPanel = GameObject.Find("Introducao Narrativa");
                    Require(introductionPanel == null || !introductionPanel.activeInHierarchy,
                        "a introducao permaneceu sobre o gameplay.");
                    stage = 2;
                    stageStartedAt = EditorApplication.timeSinceStartup;
                    return;
                }

                if (stage == 2 && EditorApplication.timeSinceStartup - stageStartedAt > 1.2d)
                {
                    PlayerController2D player = UnityEngine.Object.FindAnyObjectByType<PlayerController2D>();
                    Require(player != null, "o jogador nao existe em Play Mode.");
                    Require(player.transform.position.y > -3.5f && player.transform.position.y < 1f,
                        "o jogador nao estabilizou sobre o terreno.");
                    HealthSystem playerHealth = player.GetComponent<HealthSystem>();
                    Require(playerHealth.CurrentHealth == 5, "a vida inicial do jogador nao e 5.");

                    CameraFollow2D cameraFollow = UnityEngine.Object.FindAnyObjectByType<CameraFollow2D>();
                    Require(cameraFollow != null, "a camera de acompanhamento nao foi encontrada.");
                    Vector3 initialPosition = player.transform.position;
                    player.transform.position = new Vector3(180f, 0f, 0f);
                    cameraFollow.SnapToTarget();
                    Require(cameraFollow.transform.position.x > 175f &&
                            cameraFollow.transform.position.x < 190f,
                        "a camera nao acompanhou o jogador no trecho central do mapa.");
                    player.transform.position = initialPosition;
                    cameraFollow.SnapToTarget();

                    manager.PauseGame();
                    Require(manager.Phase == GamePhase.Paused && Mathf.Approximately(Time.timeScale, 0f),
                        "o sistema de pausa falhou.");
                    manager.ResumeGame();
                    Require(manager.Phase == GamePhase.Playing && Mathf.Approximately(Time.timeScale, 1f),
                        "o sistema de continuar falhou.");

                    playerHealth.Kill();
                    stage = 3;
                    stageStartedAt = EditorApplication.timeSinceStartup;
                    return;
                }

                if (stage == 3 && EditorApplication.timeSinceStartup - stageStartedAt > 1.25d)
                {
                    Require(manager.Phase == GamePhase.Lost, "a morte do jogador nao gerou derrota.");
                    GameObject defeatPanel = GameObject.Find("Tela de Derrota");
                    Require(defeatPanel != null && defeatPanel.activeInHierarchy,
                        "a tela de derrota nao foi exibida.");
                    stage = 4;
                    stageStartedAt = EditorApplication.timeSinceStartup;
                    manager.RestartGame();
                    return;
                }

                if (stage == 4)
                {
                    if (manager.Phase != GamePhase.Playing)
                    {
                        if (EditorApplication.timeSinceStartup - stageStartedAt > 10d)
                        {
                            throw new InvalidOperationException("o reinicio nao iniciou uma nova partida.");
                        }

                        return;
                    }

                    PlayerController2D player = UnityEngine.Object.FindAnyObjectByType<PlayerController2D>();
                    BanditAI2D enemy = UnityEngine.Object.FindObjectsByType<BanditAI2D>(
                            FindObjectsInactive.Exclude)
                        .FirstOrDefault(candidate => !candidate.IsBoss &&
                                                     candidate.GetComponent<HealthSystem>().MaxHealth == 2);
                    Require(player != null && enemy != null, "jogador ou bandido ausente apos reinicio.");
                    HealthSystem playerHealth = player.GetComponent<HealthSystem>();
                    Require(playerHealth.TakeDamage(2, CombatTeam.Enemy),
                        "nao foi possivel preparar o teste de recuperacao de vida.");
                    HealingItem2D healingItem = UnityEngine.Object.FindObjectsByType<HealingItem2D>(
                            FindObjectsInactive.Exclude)
                        .OrderByDescending(item => item.HealingAmount)
                        .FirstOrDefault();
                    Require(healingItem != null && healingItem.TryCollect(playerHealth),
                        "o item de vida nao foi coletado.");
                    Require(playerHealth.CurrentHealth == 5 && healingItem.IsCollected,
                        "o item nao recuperou vida ate o limite maximo ou permitiu coleta repetida.");
                    PlayerDefense2D defense = player.GetComponent<PlayerDefense2D>();
                    Require(defense != null, "o sistema de escudo nao foi encontrado.");
                    defense.SetBlocking(true);
                    Require(defense.TryBlockAttack(player.transform.position + Vector3.right),
                        "o escudo nao bloqueou um ataque frontal.");
                    Require(!defense.TryBlockAttack(player.transform.position + Vector3.left),
                        "o escudo bloqueou incorretamente um ataque pelas costas.");
                    Require(playerHealth.CurrentHealth == 5,
                        "o bloqueio frontal alterou a vida do jogador.");
                    defense.SetBlocking(false);
                    enemy.transform.position = player.transform.position + new Vector3(0.8f, 0f, 0f);
                    enemy.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                    smokeEnemyHealth = enemy.GetComponent<HealthSystem>();
                    Require(smokeEnemyHealth.CurrentHealth == 2, "a vida inicial do bandido nao e 2.");
                    Require(player.GetComponent<PlayerCombat2D>().TryAttack(), "o ataque do jogador nao iniciou.");
                    stage = 5;
                    stageStartedAt = EditorApplication.timeSinceStartup;
                    return;
                }

                if (stage == 5 && EditorApplication.timeSinceStartup - stageStartedAt > 0.25d)
                {
                    Require(smokeEnemyHealth != null && smokeEnemyHealth.CurrentHealth == 1,
                        "o golpe da espada nao causou exatamente um ponto de dano.");
                    Require(manager.EnemiesDefeated == 0, "o primeiro golpe contabilizou derrota indevida.");
                    stage = 6;
                    stageStartedAt = EditorApplication.timeSinceStartup;
                    return;
                }

                if (stage == 6 && EditorApplication.timeSinceStartup - stageStartedAt > 0.3d)
                {
                    PlayerCombat2D combat = UnityEngine.Object.FindAnyObjectByType<PlayerCombat2D>();
                    Require(combat != null && combat.TryAttack(), "o segundo ataque nao respeitou o cooldown.");
                    stage = 7;
                    stageStartedAt = EditorApplication.timeSinceStartup;
                    return;
                }

                if (stage == 7 && EditorApplication.timeSinceStartup - stageStartedAt > 0.25d)
                {
                    Require(manager.EnemiesDefeated == 1, "a derrota de inimigo nao foi contabilizada uma vez.");
                    Require(manager.Score >= 100, "a pontuacao do inimigo nao foi adicionada.");
                    BanditAI2D boss = UnityEngine.Object.FindObjectsByType<BanditAI2D>(
                            FindObjectsInactive.Exclude)
                        .FirstOrDefault(candidate => candidate.IsBoss);
                    Require(boss != null, "o lider bandido nao foi encontrado.");
                    boss.GetComponent<HealthSystem>().TakeDamage(999, CombatTeam.Player);
                    stage = 8;
                    stageStartedAt = EditorApplication.timeSinceStartup;
                    return;
                }

                if (stage == 8 && EditorApplication.timeSinceStartup - stageStartedAt > 0.25d)
                {
                    Require(manager.BossDefeated, "a derrota do chefe nao liberou o objetivo.");
                    manager.TryCompleteMission();
                    Require(manager.Phase == GamePhase.Won, "o objetivo final nao gerou vitoria.");
                    GameObject victoryPanel = GameObject.Find("Tela de Vitoria");
                    Require(victoryPanel != null && victoryPanel.activeInHierarchy,
                        "a tela de vitoria nao foi exibida.");
                    SessionState.SetString(ResultKey,
                        "APROVADO: menu, introducao, pulo da abertura, gameplay, fisica, camera, cura, escudo, pausa, derrota, reinicio direto, combate, pontuacao, chefe e vitoria funcionaram em Play Mode.");
                    EditorApplication.update -= RunSmokeStage;
                    EditorApplication.ExitPlaymode();
                }
            }
            catch (Exception exception)
            {
                SessionState.SetString(ResultKey, $"FALHA no smoke test: {exception}");
                EditorApplication.update -= RunSmokeStage;
                EditorApplication.ExitPlaymode();
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
