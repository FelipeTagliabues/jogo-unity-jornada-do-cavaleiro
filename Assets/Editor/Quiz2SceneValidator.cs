using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Quiz2SceneValidator
{
    [MenuItem("Projeto Medieval/Validar cena integrada (Quiz 1 + Quiz 2)")]
    public static void ValidateFromMenu()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != Quiz2SceneSetup.MainScenePath)
        {
            Debug.LogError(
                $"Abra a cena principal antes de validar: {Quiz2SceneSetup.MainScenePath}");
            return;
        }

        Report(CollectErrors(scene), false);
    }

    public static void ValidateFromCommandLine()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != Quiz2SceneSetup.MainScenePath)
        {
            scene = EditorSceneManager.OpenScene(
                Quiz2SceneSetup.MainScenePath,
                OpenSceneMode.Single);
        }

        Report(CollectErrors(scene), true);
    }

    private static List<string> CollectErrors(Scene scene)
    {
        var errors = new List<string>();

        GameObject player = FindInScene(scene, "Cavaleiro");
        ValidatePlayer(player, errors);

        TimedFallingPlatform2D[] platforms = Object
            .FindObjectsByType<TimedFallingPlatform2D>(
                FindObjectsInactive.Include)
            .Where(platform => platform.gameObject.scene == scene)
            .ToArray();

        if (platforms.Length != 1)
        {
            errors.Add(
                $"a cena deve conter exatamente uma plataforma temporizada; encontradas: {platforms.Length}.");
        }
        else
        {
            ValidatePlatform(platforms[0], player, errors);
        }

        EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
        if (buildScenes.Length == 0 ||
            !buildScenes[0].enabled ||
            buildScenes[0].path != Quiz2SceneSetup.MainScenePath)
        {
            errors.Add("a cena principal nao e a primeira cena habilitada nas Build Settings.");
        }

        string[] separateQuiz2Scenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => System.IO.Path.GetFileNameWithoutExtension(path)
                .StartsWith("Quiz2", System.StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (separateQuiz2Scenes.Length > 0)
        {
            errors.Add("foi encontrada uma cena separada de Quiz 2, contrariando a integracao progressiva.");
        }

        return errors;
    }

    private static void ValidatePlayer(GameObject player, List<string> errors)
    {
        if (player == null)
        {
            errors.Add("o objeto Cavaleiro do Quiz 1 nao foi encontrado.");
            return;
        }

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body == null || body.bodyType != RigidbodyType2D.Dynamic || body.gravityScale <= 0f)
        {
            errors.Add("o Cavaleiro perdeu a configuracao de Rigidbody2D dinamico com gravidade.");
        }

        if (player.GetComponent<Collider2D>() == null)
        {
            errors.Add("o Cavaleiro perdeu seu Collider2D.");
        }

        if (player.GetComponent<PlayerMovement2D>() == null)
        {
            errors.Add("o PlayerMovement2D nao esta associado ao Cavaleiro.");
        }

        if (player.transform.Find("GroundSensor") == null)
        {
            errors.Add("o GroundSensor do Quiz 1 nao foi encontrado.");
        }
    }

    private static void ValidatePlatform(
        TimedFallingPlatform2D platform,
        GameObject player,
        List<string> errors)
    {
        if (platform.name != Quiz2SceneSetup.PlatformName)
        {
            errors.Add($"a plataforma deve se chamar {Quiz2SceneSetup.PlatformName}.");
        }

        Rigidbody2D body = platform.GetComponent<Rigidbody2D>();
        if (body == null)
        {
            errors.Add("a plataforma nao possui Rigidbody2D.");
        }
        else
        {
            if (body.bodyType != RigidbodyType2D.Kinematic)
            {
                errors.Add("o estado inicial da plataforma deve ser Kinematic.");
            }

            if (!Mathf.Approximately(body.gravityScale, 0f))
            {
                errors.Add("a gravidade inicial da plataforma deve estar desativada.");
            }

            if (body.linearVelocity.sqrMagnitude > 0.0001f)
            {
                errors.Add("a plataforma deve iniciar sem velocidade.");
            }
        }

        Collider2D platformCollider = platform.GetComponent<Collider2D>();
        if (platformCollider == null || !platformCollider.enabled || platformCollider.isTrigger)
        {
            errors.Add("a plataforma precisa de um Collider2D fisico, habilitado e nao-trigger.");
        }

        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer < 0 || platform.gameObject.layer != groundLayer)
        {
            errors.Add("a plataforma deve estar na camada Ground.");
        }

        if (!Mathf.Approximately(TimedFallingPlatform2D.MovementDurationSeconds, 5f))
        {
            errors.Add("a duracao horizontal deve ser exatamente 5 segundos.");
        }

        if (Mathf.Abs(platform.HorizontalDirection) != 1f || platform.HorizontalSpeed <= 0f)
        {
            errors.Add("a direcao e a velocidade horizontal da plataforma sao invalidas.");
        }

        if (platform.FallingGravityScale <= 0f)
        {
            errors.Add("a gravidade de queda precisa ser positiva.");
        }

        if (platform.GetComponentInChildren<SpriteRenderer>() == null)
        {
            errors.Add("a plataforma nao possui representacao visual.");
        }

        if (player != null)
        {
            float horizontalDistance = Mathf.Abs(
                platform.transform.position.x - player.transform.position.x);
            if (horizontalDistance > 3.5f)
            {
                errors.Add("a plataforma foi posicionada longe demais do ponto inicial do Cavaleiro.");
            }
        }
    }

    private static void Report(List<string> errors, bool throwOnFailure)
    {
        if (errors.Count == 0)
        {
            Debug.Log(
                "Quiz 2 validado: Quiz 1 preservado, plataforma fisica configurada, " +
                "movimento de 5 segundos, queda e Build Settings corretos.");
            return;
        }

        foreach (string error in errors)
        {
            Debug.LogError($"Quiz 2: {error}");
        }

        if (throwOnFailure)
        {
            throw new System.InvalidOperationException(
                $"A validacao do Quiz 2 encontrou {errors.Count} erro(s).");
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
