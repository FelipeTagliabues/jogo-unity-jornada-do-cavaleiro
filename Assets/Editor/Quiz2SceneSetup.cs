using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Quiz2SceneSetup
{
    public const string MainScenePath = "Assets/Scenes/Quiz1_Movimentacao.unity";
    public const string PlatformName = "PlataformaTemporizada";

    private const string PlatformSpritePath =
        "Assets/Hero Knight - Pixel Art/Sprites/EnvironmentTiles.png";

    [MenuItem("Projeto Medieval/Configurar plataforma do Quiz 2")]
    public static void ConfigureFromMenu()
    {
        ConfigureScene();
    }

    public static void ConfigureFromCommandLine()
    {
        ConfigureScene();
        Quiz2SceneValidator.ValidateFromCommandLine();
    }

    private static void ConfigureScene()
    {
        Scene scene = OpenMainSceneSafely();
        GameObject platform = FindInScene(scene, PlatformName);

        if (platform == null)
        {
            platform = new GameObject(PlatformName);
            SceneManager.MoveGameObjectToScene(platform, scene);

            if (!Application.isBatchMode)
            {
                Undo.RegisterCreatedObjectUndo(platform, "Criar plataforma temporizada");
            }
        }

        ConfigurePlatformObject(platform);
        EnsureVisuals(platform.transform);
        EnsureMainSceneIsFirstInBuildSettings();

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
        {
            throw new System.InvalidOperationException(
                $"Nao foi possivel salvar a cena principal: {MainScenePath}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Quiz 2 configurado na cena existente: {MainScenePath}");
    }

    private static Scene OpenMainSceneSafely()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.path == MainScenePath)
        {
            return activeScene;
        }

        if (!Application.isBatchMode &&
            !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            throw new System.OperationCanceledException(
                "A configuracao foi cancelada para preservar alteracoes de cena nao salvas.");
        }

        return EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
    }

    private static void ConfigurePlatformObject(GameObject platform)
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer < 0)
        {
            throw new System.InvalidOperationException("A camada Ground nao existe.");
        }

        platform.name = PlatformName;
        platform.layer = groundLayer;
        platform.transform.SetParent(null);
        platform.transform.SetPositionAndRotation(new Vector3(2.2f, -0.15f, 0f), Quaternion.identity);
        platform.transform.localScale = Vector3.one;

        BoxCollider2D platformCollider = platform.GetComponent<BoxCollider2D>();
        if (platformCollider == null)
        {
            platformCollider = platform.AddComponent<BoxCollider2D>();
        }

        platformCollider.enabled = true;
        platformCollider.isTrigger = false;
        platformCollider.offset = Vector2.zero;
        platformCollider.size = new Vector2(2f, 0.45f);

        Rigidbody2D body = platform.GetComponent<Rigidbody2D>();
        if (body == null)
        {
            body = platform.AddComponent<Rigidbody2D>();
        }

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.sleepMode = RigidbodySleepMode2D.NeverSleep;

        TimedFallingPlatform2D controller = platform.GetComponent<TimedFallingPlatform2D>();
        if (controller == null)
        {
            controller = platform.AddComponent<TimedFallingPlatform2D>();
        }

        controller.Configure(-1f, 0.35f, 3f);
    }

    private static void EnsureVisuals(Transform platform)
    {
        Sprite[] availableSprites = AssetDatabase.LoadAllAssetsAtPath(PlatformSpritePath)
            .OfType<Sprite>()
            .ToArray();

        Sprite leftSprite = availableSprites.FirstOrDefault(sprite => sprite.name == "EnvironmentTiles_0");
        Sprite rightSprite = availableSprites.FirstOrDefault(sprite => sprite.name == "EnvironmentTiles_1");

        if (leftSprite == null || rightSprite == null)
        {
            throw new System.InvalidOperationException(
                $"Os sprites esperados nao foram encontrados em {PlatformSpritePath}.");
        }

        Transform visualRoot = platform.Find("Visual");
        if (visualRoot == null)
        {
            visualRoot = new GameObject("Visual").transform;
            visualRoot.SetParent(platform, false);
        }

        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;
        visualRoot.gameObject.layer = platform.gameObject.layer;

        ConfigureTile(visualRoot, "TileEsquerdo", leftSprite, new Vector3(-1f, -0.5f, 0f));
        ConfigureTile(visualRoot, "TileDireito", rightSprite, new Vector3(0f, -0.5f, 0f));
    }

    private static void ConfigureTile(
        Transform parent,
        string name,
        Sprite sprite,
        Vector3 localPosition)
    {
        Transform tile = parent.Find(name);
        if (tile == null)
        {
            tile = new GameObject(name).transform;
            tile.SetParent(parent, false);
        }

        tile.localPosition = localPosition;
        tile.localRotation = Quaternion.identity;
        tile.localScale = Vector3.one;
        tile.gameObject.layer = parent.gameObject.layer;

        SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = tile.gameObject.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = sprite;
        renderer.color = Color.white;
        renderer.sortingOrder = 5;
    }

    private static void EnsureMainSceneIsFirstInBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        EditorBuildSettingsScene mainScene = scenes.FirstOrDefault(scene => scene.path == MainScenePath);

        if (mainScene == null)
        {
            mainScene = new EditorBuildSettingsScene(MainScenePath, true);
        }
        else
        {
            scenes.Remove(mainScene);
            mainScene.enabled = true;
        }

        scenes.Insert(0, mainScene);

        if (!EditorBuildSettings.scenes.Select(scene => scene.path)
                .SequenceEqual(scenes.Select(scene => scene.path)) ||
            !EditorBuildSettings.scenes[0].enabled)
        {
            EditorBuildSettings.scenes = scenes.ToArray();
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
