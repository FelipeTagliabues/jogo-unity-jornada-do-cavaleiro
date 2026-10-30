using System;
using System.Collections.Generic;
using System.Linq;
using MedievalJourney;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace MedievalJourneyEditor
{
    public static class MainGameSceneValidator
    {
        [MenuItem("Projeto Medieval/Validar jogo completo/MainGame")]
        public static void ValidateFromMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != MainGameSceneBuilder.ScenePath)
            {
                Debug.LogError($"Abra a cena {MainGameSceneBuilder.ScenePath} antes de validar.");
                return;
            }

            Report(CollectErrors(scene), false);
        }

        public static void ValidateFromCommandLine()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != MainGameSceneBuilder.ScenePath)
            {
                scene = EditorSceneManager.OpenScene(MainGameSceneBuilder.ScenePath, OpenSceneMode.Single);
            }

            Report(CollectErrors(scene), true);
        }

        private static List<string> CollectErrors(Scene scene)
        {
            List<string> errors = new();
            GameObject player = FindInScene(scene, "Cavaleiro");
            Require(player, "Cavaleiro", errors);
            if (player != null)
            {
                RequireComponent<Rigidbody2D>(player, errors);
                RequireComponent<Collider2D>(player, errors);
                RequireComponent<Animator>(player, errors);
                RequireComponent<HealthSystem>(player, errors);
                RequireComponent<PlayerController2D>(player, errors);
                RequireComponent<PlayerCombat2D>(player, errors);
                RequireComponent<PlayerDefense2D>(player, errors);
                if (player.transform.Find("GroundCheck") == null)
                {
                    errors.Add("o Cavaleiro nao possui GroundCheck.");
                }
            }

            BanditAI2D[] enemies = UnityEngine.Object.FindObjectsByType<BanditAI2D>(
                    FindObjectsInactive.Include)
                .Where(enemy => enemy.gameObject.scene == scene)
                .ToArray();
            if (enemies.Length != 16)
            {
                errors.Add($"eram esperados 16 inimigos; encontrados: {enemies.Length}.");
            }

            if (enemies.Count(enemy => enemy.IsBoss) != 1)
            {
                errors.Add("a cena deve conter exatamente um lider bandido.");
            }

            Require(FindInScene(scene, "GameManager"), "GameManager", errors);
            Require(FindInScene(scene, "Interface"), "Interface", errors);
            GameObject introduction = FindInScene(scene, "Introducao Narrativa");
            Require(introduction, "Introducao Narrativa", errors);
            if (introduction != null)
            {
                RequireComponent<OpeningSequenceUI>(introduction, errors);
                Require(FindInScene(scene, "Retrato do Cavaleiro"), "Retrato do Cavaleiro", errors);
                Require(FindInScene(scene, "Lider dos Bandidos"), "Lider dos Bandidos", errors);
            }
            Require(FindInScene(scene, "Estandarte de Saida"), "Estandarte de Saida", errors);
            Require(FindInScene(scene, "Abismo"), "Abismo", errors);
            Require(FindInScene(scene, "Main Camera"), "Main Camera", errors);
            Require(FindInScene(scene, "Superficie Medieval Continua"),
                "Superficie Medieval Continua", errors);

            HealingItem2D[] healingItems = UnityEngine.Object.FindObjectsByType<HealingItem2D>(
                    FindObjectsInactive.Include)
                .Where(item => item.gameObject.scene == scene)
                .ToArray();
            if (healingItems.Length != 8)
            {
                errors.Add($"eram esperados 8 itens de vida; encontrados: {healingItems.Length}.");
            }

            if (healingItems.Any(item => item.transform.position.y > 2f))
            {
                errors.Add("ha provisoes posicionadas acima da rota alcancavel de plataformas.");
            }

            GameObject decorationRoot = FindInScene(scene, "Vila, Floresta e Acampamentos Cainos");
            if (decorationRoot != null && decorationRoot.GetComponentsInChildren<Rigidbody2D>(true)
                    .Any(body => body.simulated))
            {
                errors.Add("ha props decorativos com Rigidbody2D ainda ativos e sujeitos a queda.");
            }

            Tilemap tilemap = UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Include)
                .FirstOrDefault(candidate => candidate.gameObject.scene == scene);
            if (tilemap == null || tilemap.cellBounds.size.x < 330 || tilemap.GetUsedTilesCount() < 9)
            {
                errors.Add("o Tilemap Cainos nao cobre a extensao planejada ou nao usa os tiles de terreno.");
            }

            GameObject manager = FindInScene(scene, "GameManager");
            if (manager != null)
            {
                RequireComponent<GameManager>(manager, errors);
                GameAudio audio = manager.GetComponent<GameAudio>();
                RequireComponent<GameAudio>(manager, errors);
                if (audio != null)
                {
                    if (audio.MusicSource == null || audio.EffectsSource == null)
                    {
                        errors.Add("o sistema de audio nao possui as duas fontes configuradas.");
                    }
                    else if (!audio.MusicSource.loop || audio.EffectsSource.loop)
                    {
                        errors.Add("o loop das fontes de musica e efeitos esta configurado incorretamente.");
                    }

                    if (audio.MenuMusic == null || audio.GameplayMusic == null ||
                        audio.SwordAttack == null || audio.ShieldDefense == null || audio.DeathBell == null)
                    {
                        errors.Add("um ou mais dos cinco audios obrigatorios nao estao referenciados.");
                    }
                }
            }

            GameObject ui = FindInScene(scene, "Interface");
            if (ui != null)
            {
                RequireComponent<GameUI>(ui, errors);
                int buttonCount = ui.GetComponentsInChildren<Button>(true).Length;
                if (buttonCount < 10)
                {
                    errors.Add($"a interface deveria ter ao menos 10 botoes funcionais; encontrados: {buttonCount}.");
                }
            }

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0)
            {
                errors.Add("a camada Ground nao existe.");
            }
            else
            {
                int solidGround = UnityEngine.Object.FindObjectsByType<Collider2D>(
                        FindObjectsInactive.Include)
                    .Count(collider => collider.gameObject.scene == scene && !collider.isTrigger &&
                                       collider.gameObject.layer == groundLayer);
                if (solidGround < 20)
                {
                    errors.Add($"ha poucos colisores de terreno/plataforma: {solidGround}.");
                }
            }

            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            if (buildScenes.Length == 0 || buildScenes[0].path != MainGameSceneBuilder.ScenePath ||
                !buildScenes[0].enabled)
            {
                errors.Add("MainGame precisa ser a primeira cena habilitada nas Build Settings.");
            }

            GameObject interfaceObject = FindInScene(scene, "Interface");
            if (interfaceObject != null)
            {
                foreach (Button button in interfaceObject.GetComponentsInChildren<Button>(true))
                {
                    if (button.onClick.GetPersistentEventCount() == 0)
                    {
                        errors.Add($"o botao {button.name} nao possui acao persistente.");
                    }
                }
            }

            return errors;
        }

        private static void Report(List<string> errors, bool commandLine)
        {
            if (errors.Count == 0)
            {
                Debug.Log("MainGame validado: introducao, cinco audios, Tilemap Cainos, 7 areas, 16 inimigos, 8 itens de vida, chefe, UI, camera e build settings OK.");
                return;
            }

            foreach (string error in errors)
            {
                Debug.LogError($"MainGame: {error}");
            }

            if (commandLine)
            {
                throw new InvalidOperationException(
                    $"A validacao de MainGame encontrou {errors.Count} problema(s).");
            }
        }

        private static void Require(GameObject target, string label, ICollection<string> errors)
        {
            if (target == null)
            {
                errors.Add($"o objeto {label} nao foi encontrado.");
            }
        }

        private static void RequireComponent<T>(GameObject target, ICollection<string> errors)
            where T : Component
        {
            if (target.GetComponent<T>() == null)
            {
                errors.Add($"{target.name} nao possui {typeof(T).Name}.");
            }
        }

        private static GameObject FindInScene(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform match = FindRecursively(root.transform, objectName);
                if (match != null)
                {
                    return match.gameObject;
                }
            }

            return null;
        }

        private static Transform FindRecursively(Transform current, string objectName)
        {
            if (current.name == objectName)
            {
                return current;
            }

            foreach (Transform child in current)
            {
                Transform match = FindRecursively(child, objectName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }
}
