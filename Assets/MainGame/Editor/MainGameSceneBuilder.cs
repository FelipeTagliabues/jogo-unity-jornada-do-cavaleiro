using System;
using System.Collections.Generic;
using System.Linq;
using MedievalJourney;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace MedievalJourneyEditor
{
    public static class MainGameSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/MainGame.unity";

        private const string HeroPrefabPath =
            "Assets/Hero Knight - Pixel Art/Demo/HeroKnight.prefab";
        private const string LightBanditPrefabPath =
            "Assets/Bandits - Pixel Art/Demo/LightBandit.prefab";
        private const string HeavyBanditPrefabPath =
            "Assets/Bandits - Pixel Art/Demo/HeavyBandit.prefab";
        private const string HeroSpritePath =
            "Assets/Hero Knight - Pixel Art/Sprites/HeroKnight.png";
        private const string LightBanditSpritePath =
            "Assets/Bandits - Pixel Art/Sprites/LightBandit.png";
        private const string HeavyBanditSpritePath =
            "Assets/Bandits - Pixel Art/Sprites/HeavyBandit.png";
        private const string CainosPrefabDirectory =
            "Assets/Cainos/Pixel Art Platformer - Village Props/Prefab";
        private const string GroundTileDirectory =
            "Assets/Cainos/Pixel Art Platformer - Village Props/Tileset Palette/TP Ground";
        private const string BoardSpritePath =
            "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/UI board Large  parchment.png";
        private const string ButtonSpritePath =
            "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Big.png";
        private const string ButtonPressedSpritePath =
            "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Big_Pressed.png";

        private static readonly Color Gold = new(1f, 0.88f, 0.48f, 1f);
        private static readonly Color Ink = new(0.19f, 0.09f, 0.045f, 1f);

        [InitializeOnLoadMethod]
        private static void BuildMissingSceneWhenEditorIsReady()
        {
            EditorApplication.delayCall += TryAutomaticBuild;
        }

        private static void TryAutomaticBuild()
        {
            if (!SceneNeedsRebuild())
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.playModeStateChanged += ContinueAfterPlayMode;
                EditorApplication.isPlaying = false;
                return;
            }

            if (!EditorSceneManager.SaveOpenScenes())
            {
                Debug.LogError("MainGame automatico cancelado: nao foi possivel salvar as cenas abertas.");
                return;
            }

            BuildScene();
            MainGameSceneValidator.ValidateFromCommandLine();
        }

        private static bool SceneNeedsRebuild()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                return true;
            }

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool openedForCheck = !scene.isLoaded;
            if (openedForCheck)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            }

            bool hasDefense = scene.GetRootGameObjects()
                .Any(root => root.GetComponentInChildren<PlayerDefense2D>(true) != null);
            bool hasIntroduction = scene.GetRootGameObjects()
                .Any(root => root.GetComponentInChildren<OpeningSequenceUI>(true) != null);
            if (openedForCheck)
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            return !hasDefense || !hasIntroduction;
        }

        private static void ContinueAfterPlayMode(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            EditorApplication.playModeStateChanged -= ContinueAfterPlayMode;
            EditorApplication.delayCall += TryAutomaticBuild;
        }

        [MenuItem("Projeto Medieval/Construir jogo completo/MainGame")]
        public static void BuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            BuildScene();
        }

        public static void BuildFromCommandLine()
        {
            BuildScene();
            MainGameSceneValidator.ValidateFromCommandLine();
        }

        private static void BuildScene()
        {
            ValidateRequiredAssets();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0)
            {
                throw new InvalidOperationException("A camada Ground, usada pelos quizzes, nao existe.");
            }

            TileBase[] groundTiles = Enumerable.Range(0, 9)
                .Select(index => AssetDatabase.LoadAssetAtPath<TileBase>(
                    $"{GroundTileDirectory}/TX Tileset Ground_{index}.asset"))
                .ToArray();

            GameObject world = new("Mundo Medieval");
            SceneManager.MoveGameObjectToScene(world, scene);
            CreateDistantScenery(world.transform);
            Transform terrain = CreateTerrain(scene, world.transform, groundTiles, groundLayer);
            CreateCainosPlatforms(terrain, groundLayer);
            CreateWorldBoundary(terrain, new Vector2(-1f, 1f), new Vector2(1f, 18f), groundLayer);
            CreateWorldBoundary(terrain, new Vector2(336f, 1f), new Vector2(1f, 18f), groundLayer);
            CreateDeathZone(terrain);

            GameObject player = CreatePlayer(scene, new Vector3(1.5f, -1.95f, 0f), groundLayer);
            List<GameObject> enemies = new()
            {
                CreateBandit(scene, "Bandido Aprendiz", LightBanditPrefabPath,
                    new Vector3(26f, -1.95f, 0f), 23f, 31f, false, 2, 1, 100, groundLayer),
                CreateBandit(scene, "Saqueador da Praca", LightBanditPrefabPath,
                    new Vector3(47f, -0.95f, 0f), 42f, 55f, false, 2, 1, 100, groundLayer),
                CreateBandit(scene, "Sentinela da Aldeia", LightBanditPrefabPath,
                    new Vector3(59f, -0.95f, 0f), 56f, 61f, false, 2, 1, 120, groundLayer),
                CreateBandit(scene, "Bandido do Bosque I", LightBanditPrefabPath,
                    new Vector3(78f, -1.95f, 0f), 73f, 87f, false, 2, 1, 120, groundLayer),
                CreateBandit(scene, "Bandido do Bosque II", LightBanditPrefabPath,
                    new Vector3(96f, -1.95f, 0f), 91f, 101f, false, 2, 1, 120, groundLayer),
                CreateBandit(scene, "Guarda da Colina", HeavyBanditPrefabPath,
                    new Vector3(112f, -0.95f, 0f), 106f, 126f, false, 3, 1, 160, groundLayer),
                CreateBandit(scene, "Arqueiro sem Arco", LightBanditPrefabPath,
                    new Vector3(136f, -1.95f, 0f), 132f, 145f, false, 2, 1, 140, groundLayer),
                CreateBandit(scene, "Bandido da Ponte", HeavyBanditPrefabPath,
                    new Vector3(162f, -1.95f, 0f), 155f, 173f, false, 3, 1, 180, groundLayer),
                CreateBandit(scene, "Batedor do Acampamento", LightBanditPrefabPath,
                    new Vector3(184f, -0.95f, 0f), 179f, 195f, false, 3, 1, 160, groundLayer),
                CreateBandit(scene, "Saqueador do Acampamento", HeavyBanditPrefabPath,
                    new Vector3(207f, -1.95f, 0f), 201f, 215f, false, 4, 1, 200, groundLayer),
                CreateBandit(scene, "Guarda da Garganta", LightBanditPrefabPath,
                    new Vector3(229f, -0.95f, 0f), 223f, 241f, false, 3, 1, 170, groundLayer),
                CreateBandit(scene, "Veterano da Subida", HeavyBanditPrefabPath,
                    new Vector3(252f, 0.05f, 0f), 247f, 264f, false, 4, 1, 220, groundLayer),
                CreateBandit(scene, "Sentinela das Ruinas", LightBanditPrefabPath,
                    new Vector3(276f, -0.95f, 0f), 270f, 286f, false, 3, 1, 180, groundLayer),
                CreateBandit(scene, "Carrasco da Arena I", HeavyBanditPrefabPath,
                    new Vector3(300f, -1.95f, 0f), 294f, 307f, false, 4, 1, 220, groundLayer),
                CreateBandit(scene, "Carrasco da Arena II", HeavyBanditPrefabPath,
                    new Vector3(313f, -1.95f, 0f), 309f, 318f, false, 4, 1, 220, groundLayer),
                CreateBandit(scene, "Lider dos Bandidos", HeavyBanditPrefabPath,
                    new Vector3(324f, -1.95f, 0f), 320f, 330f, true, 10, 2, 750, groundLayer)
            };

            CreateDecorations(world.transform);
            CreateHealingItems(scene);
            CreateGoal(scene, new Vector3(332f, -1.45f, 0f));
            CreateCamera(scene, player.transform);

            GameObject managerObject = new("GameManager");
            SceneManager.MoveGameObjectToScene(managerObject, scene);
            GameManager gameManager = managerObject.AddComponent<GameManager>();
            GameUI gameUI = CreateInterface(scene, gameManager);
            gameManager.Configure(player.GetComponent<HealthSystem>(), gameUI, enemies.Count);

            CreateEventSystem(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Nao foi possivel salvar {ScenePath}.");
            }

            AssetDatabase.Refresh();
            EnsureSceneFirstInBuildSettings();
            AssetDatabase.SaveAssets();
            Selection.activeObject = player;
            Debug.Log("MainGame expandido: 7 areas, terreno Cainos, exploracao, 16 bandidos, chefe, itens de vida e UI configurados.");
        }

        private static void ValidateRequiredAssets()
        {
            string[] required =
            {
                HeroPrefabPath,
                LightBanditPrefabPath,
                HeavyBanditPrefabPath,
                HeroSpritePath,
                LightBanditSpritePath,
                HeavyBanditSpritePath,
                $"{GroundTileDirectory}/TX Tileset Ground_0.asset",
                $"{GroundTileDirectory}/TX Tileset Ground_8.asset",
                $"{CainosPrefabDirectory}/PF Village Props - Tree 01.prefab",
                $"{CainosPrefabDirectory}/PF Village Props - Stall.prefab",
                $"{CainosPrefabDirectory}/PF Village Props - White Bottle.prefab",
                BoardSpritePath,
                ButtonSpritePath,
                ButtonPressedSpritePath
            };

            foreach (string path in required)
            {
                if (AssetDatabase.LoadMainAssetAtPath(path) == null)
                {
                    throw new InvalidOperationException($"Asset obrigatorio nao encontrado: {path}");
                }
            }
        }

        private static GameObject CreatePlayer(Scene scene, Vector3 position, int groundLayer)
        {
            GameObject player = InstantiateAndUnpack(HeroPrefabPath, scene);
            player.name = "Cavaleiro";
            player.tag = "Player";
            player.transform.SetPositionAndRotation(position, Quaternion.identity);
            player.transform.localScale = Vector3.one;
            StripDemoCharacterScripts(player);

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            body.gravityScale = 2.7f;
            body.mass = 1f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            HealthSystem health = player.AddComponent<HealthSystem>();
            health.Configure(5, CombatTeam.Player, 0.7f);

            Transform groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(player.transform, false);
            groundCheck.localPosition = new Vector3(0f, 0.02f, 0f);

            PlayerController2D controller = player.AddComponent<PlayerController2D>();
            controller.Configure(groundCheck, 1 << groundLayer, 5.2f, 9.5f);
            player.AddComponent<PlayerDefense2D>();
            PlayerCombat2D combat = player.AddComponent<PlayerCombat2D>();
            combat.Configure(1, 1.05f, 0.35f);
            return player;
        }

        private static GameObject CreateBandit(Scene scene, string name, string prefabPath,
            Vector3 position, float patrolLeft, float patrolRight, bool boss, int healthPoints,
            int attackDamage, int score, int groundLayer)
        {
            GameObject bandit = InstantiateAndUnpack(prefabPath, scene);
            bandit.name = name;
            bandit.tag = "Untagged";
            bandit.transform.SetPositionAndRotation(position, Quaternion.identity);
            bandit.transform.localScale = boss ? Vector3.one * 1.22f : Vector3.one;
            StripDemoCharacterScripts(bandit);

            Rigidbody2D body = bandit.GetComponent<Rigidbody2D>();
            body.gravityScale = 2.7f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            HealthSystem health = bandit.AddComponent<HealthSystem>();
            health.Configure(healthPoints, CombatTeam.Enemy, boss ? 0.3f : 0.22f);
            BanditAI2D ai = bandit.AddComponent<BanditAI2D>();
            ai.Configure(patrolLeft, patrolRight, 1 << groundLayer, boss, attackDamage, score,
                boss ? 8f : 6f);

            SpriteRenderer renderer = bandit.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = boss ? 12 : 10;
            return bandit;
        }

        private static GameObject InstantiateAndUnpack(string prefabPath, Scene scene)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);
            return instance;
        }

        private static void StripDemoCharacterScripts(GameObject character)
        {
            foreach (MonoBehaviour behaviour in character.GetComponents<MonoBehaviour>())
            {
                if (behaviour != null && (behaviour.GetType().Name == "HeroKnight" ||
                                          behaviour.GetType().Name == "Bandit"))
                {
                    UnityEngine.Object.DestroyImmediate(behaviour);
                }
            }

            List<GameObject> sensorObjects = new();
            foreach (Transform child in character.GetComponentsInChildren<Transform>(true))
            {
                if (child != character.transform && child.name.Contains("Sensor", StringComparison.Ordinal))
                {
                    sensorObjects.Add(child.gameObject);
                }
            }

            foreach (GameObject sensor in sensorObjects)
            {
                UnityEngine.Object.DestroyImmediate(sensor);
            }
        }

        private static void CreateDistantScenery(Transform parent)
        {
            Transform root = new GameObject("Paisagem Distante Cainos").transform;
            root.SetParent(parent);
            int[] positions = { 5, 18, 36, 53, 74, 91, 111, 129, 157, 181, 205, 232, 256, 279, 301, 326 };
            for (int i = 0; i < positions.Length; i++)
            {
                string tree = i % 3 == 0 ? "PF Village Props - Tree 02.prefab" :
                    "PF Village Props - Tree 01.prefab";
                Color tint = i % 2 == 0
                    ? new Color(0.22f, 0.28f, 0.27f, 0.72f)
                    : new Color(0.17f, 0.23f, 0.25f, 0.66f);
                CreateDecoration(root, tree, new Vector3(positions[i], -1.1f, 3f),
                    i % 2 == 0 ? 1.35f : 1.1f, -30, tint);
            }
        }

        private static Transform CreateTerrain(Scene scene, Transform parent, TileBase[] tiles, int layer)
        {
            if (tiles.Any(tile => tile == null))
            {
                throw new InvalidOperationException("Os nove tiles-base do terreno Cainos nao foram encontrados.");
            }

            GameObject gridObject = new("Terreno Cainos - Grid 32px");
            gridObject.transform.SetParent(parent);
            Grid grid = gridObject.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            GameObject tilemapObject = new("Superficie Medieval Continua");
            tilemapObject.layer = layer;
            tilemapObject.transform.SetParent(gridObject.transform);
            Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
            TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = 0;

            (string name, int start, int end, int top, int bottom)[] sections =
            {
                ("01 Entrada da Aldeia", 0, 34, -2, -7),
                ("02 Praca e Mercado", 34, 62, -1, -7),
                ("03 Saida da Aldeia", 62, 68, -2, -7),
                ("04 Bosque Inicial", 71, 103, -2, -8),
                ("05 Colinas Verdes", 103, 130, -1, -8),
                ("06 Caminho da Ponte", 130, 147, -2, -8),
                ("07 Floresta Profunda", 153, 176, -2, -8),
                ("08 Acampamento Bandido", 176, 198, -1, -8),
                ("09 Desfiladeiro", 198, 218, -2, -9),
                ("10 Garganta Rochosa", 221, 245, -1, -9),
                ("11 Subida das Ruinas", 245, 267, 0, -9),
                ("12 Muralha Abandonada", 267, 290, -1, -9),
                ("13 Arena do Lider", 290, 336, -2, -9)
            };

            Transform colliders = new GameObject("Colisores Continuos do Terreno").transform;
            colliders.SetParent(gridObject.transform);
            foreach ((string name, int start, int end, int top, int bottom) section in sections)
            {
                PaintTerrainSection(tilemap, tiles, section.start, section.end, section.top, section.bottom);
                float width = section.end - section.start;
                float height = section.top - section.bottom + 1;
                CreateSolid(colliders, section.name,
                    new Vector2(section.start + width * 0.5f, section.bottom + height * 0.5f),
                    new Vector2(width, height), layer);
            }

            tilemap.CompressBounds();
            return gridObject.transform;
        }

        private static void PaintTerrainSection(Tilemap tilemap, TileBase[] tiles, int start, int end,
            int top, int bottom)
        {
            for (int x = start; x < end; x++)
            {
                bool left = x == start;
                bool right = x == end - 1;
                for (int y = bottom; y <= top; y++)
                {
                    bool topRow = y == top;
                    bool bottomRow = y == bottom;
                    int index = topRow
                        ? left ? 0 : right ? 2 : 1
                        : bottomRow
                            ? left ? 6 : right ? 8 : 7
                            : left ? 3 : right ? 5 : 4;
                    tilemap.SetTile(new Vector3Int(x, y, 0), tiles[index]);
                }
            }
        }

        private static void CreateCainosPlatforms(Transform parent, int layer)
        {
            (string name, string prefab, Vector3 position, float scale)[] platforms =
            {
                ("Plataforma do Poco", "PF Village Props - Platform 02 X4.prefab", new Vector3(51f, 0.75f, 0f), 1f),
                ("Plataforma do Bosque A", "PF Village Props - Platform 02 X3.prefab", new Vector3(80f, -0.1f, 0f), 1f),
                ("Degrau do Bosque", "PF Village Props - Platform 02 X3.prefab", new Vector3(84f, 0.65f, 0f), 1f),
                ("Plataforma do Bosque B", "PF Village Props - Platform 02 X4.prefab", new Vector3(88f, 1.35f, 0f), 1f),
                ("Plataforma das Colinas", "PF Village Props - Platform 02 X4.prefab", new Vector3(119f, 0.8f, 0f), 1f),
                ("Ponte de Madeira", "PF Village Props - Wooden Bridge X6.prefab", new Vector3(147f, -1.2f, 0f), 1f),
                ("Plataforma da Floresta", "PF Village Props - Platform 02 X4.prefab", new Vector3(165f, -0.15f, 0f), 1f),
                ("Vigia do Acampamento", "PF Village Props - Platform 02 X3.prefab", new Vector3(188f, 0.8f, 0f), 1f),
                ("Salto do Desfiladeiro", "PF Village Props - Platform 02 X4.prefab", new Vector3(211f, -0.1f, 0f), 1f),
                ("Travessia da Garganta", "PF Village Props - Platform 02 X3.prefab", new Vector3(218f, -0.3f, 0f), 1f),
                ("Muralha Baixa", "PF Village Props - Obstacle Platform X32.prefab", new Vector3(259f, 0.5f, 0f), 1f),
                ("Plataforma das Ruinas", "PF Village Props - Platform 02 X4.prefab", new Vector3(276f, 0.75f, 0f), 1f),
                ("Posto da Arena", "PF Village Props - Platform 02 X4.prefab", new Vector3(305f, -0.2f, 0f), 1f)
            };

            foreach ((string name, string prefab, Vector3 position, float scale) entry in platforms)
            {
                GameObject platform = InstantiateCainosPrefab(entry.prefab, parent);
                platform.name = entry.name;
                platform.transform.position = entry.position;
                platform.transform.localScale = Vector3.one * entry.scale;
                SetLayerRecursively(platform, layer);
                DisablePackageBehaviours(platform);
                foreach (SpriteRenderer sprite in platform.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    sprite.sortingOrder += 3;
                }
            }
        }

        private static GameObject CreateSolid(Transform parent, string name, Vector2 center,
            Vector2 size, int layer)
        {
            GameObject solid = new(name);
            solid.layer = layer;
            solid.transform.SetParent(parent);
            solid.transform.position = center;
            BoxCollider2D collider = solid.AddComponent<BoxCollider2D>();
            collider.size = size;
            return solid;
        }

        private static void CreateWorldBoundary(Transform parent, Vector2 center, Vector2 size, int layer)
        {
            CreateSolid(parent, "Limite do Mundo", center, size, layer);
        }

        private static void CreateDeathZone(Transform parent)
        {
            GameObject zone = new("Abismo");
            zone.transform.SetParent(parent);
            zone.transform.position = new Vector3(167.5f, -12f, 0f);
            BoxCollider2D collider = zone.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(345f, 3f);
            zone.AddComponent<DeathZone2D>();
        }

        private static void CreateCamera(Scene scene, Transform player)
        {
            GameObject cameraObject = new("Main Camera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(8.9f, 0f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.05f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.16f, 0.24f, 0.29f);
            cameraObject.AddComponent<AudioListener>();
            CameraFollow2D follow = cameraObject.AddComponent<CameraFollow2D>();
            follow.Configure(player, new Vector2(8.9f, -0.2f), new Vector2(327f, 4.5f),
                new Vector3(3.1f, 1.05f, -10f), 0.16f);
        }

        private static void CreateDecorations(Transform parent)
        {
            Transform decorationRoot = new GameObject("Vila, Floresta e Acampamentos Cainos").transform;
            decorationRoot.SetParent(parent);

            (string prefab, Vector3 position, float scale, int order)[] props =
            {
                ("PF Village Props - Road Sign 01.prefab", new Vector3(4f, -1.5f, 0f), 1f, 4),
                ("PF Village Props - Fence A.prefab", new Vector3(9f, -1.5f, 0f), 1f, 1),
                ("PF Village Props - Fence B.prefab", new Vector3(13f, -1.5f, 0f), 1f, 1),
                ("PF Village Props - Barrel.prefab", new Vector3(17f, -1.5f, 0f), 1f, 4),
                ("PF Village Props - Crate Small.prefab", new Vector3(19f, -1.5f, 0f), 1f, 4),
                ("PF Village Props - Log Bench.prefab", new Vector3(23f, -1.5f, 0f), 1f, 3),
                ("PF Village Props - Wheelbarrow.prefab", new Vector3(30f, -1.5f, 0f), 1f, 3),
                ("PF Village Props - Well.prefab", new Vector3(37f, -0.5f, 0f), 1f, 2),
                ("PF Village Props - Stall.prefab", new Vector3(43f, -0.5f, 0f), 1f, 2),
                ("PF Village Props - Basket.prefab", new Vector3(47f, -0.5f, 0f), 1f, 4),
                ("PF Village Props - Grain Box.prefab", new Vector3(49f, -0.5f, 0f), 1f, 3),
                ("PF Village Props - Road Lamp.prefab", new Vector3(55f, -0.5f, 0f), 1f, 3),
                ("PF Village Props - Hay Bale.prefab", new Vector3(60f, -0.5f, 0f), 1f, 3),
                ("PF Village Props - Scarecrow 01.prefab", new Vector3(64f, -1.5f, 0f), 1f, 2),
                ("PF Village Props - Tree 02.prefab", new Vector3(75f, -1.5f, 0f), 1f, 0),
                ("PF Village Props - Bush 01.prefab", new Vector3(82f, -1.5f, 0f), 1f, 2),
                ("PF Village Props - Stump.prefab", new Vector3(86f, -1.5f, 0f), 1f, 3),
                ("PF Village Props - Tree 01.prefab", new Vector3(93f, -1.5f, 0f), 1f, 0),
                ("PF Village Props - Bush 03.prefab", new Vector3(101f, -1.5f, 0f), 1f, 2),
                ("PF Village Props - Sunflower 01.prefab", new Vector3(108f, -0.5f, 0f), 1f, 2),
                ("PF Village Props - Sunflower 03.prefab", new Vector3(111f, -0.5f, 0f), 1f, 2),
                ("PF Village Props - Rock 02.prefab", new Vector3(125f, -0.5f, 0f), 1f, 2),
                ("PF Village Props - Road Sign 02.prefab", new Vector3(133f, -1.5f, 0f), 1f, 4),
                ("PF Village Props - Wood Logs.prefab", new Vector3(140f, -1.5f, 0f), 1f, 3),
                ("PF Village Props - Tree 02.prefab", new Vector3(158f, -1.5f, 0f), 1f, 0),
                ("PF Village Props - Bush 02.prefab", new Vector3(169f, -1.5f, 0f), 1f, 2),
                ("PF Village Props - Barricade.prefab", new Vector3(179f, -0.5f, 0f), 1f, 4),
                ("PF Village Props - Campfire 01.prefab", new Vector3(183f, -0.5f, 0f), 1f, 5),
                ("PF Village Props - Hay Pile.prefab", new Vector3(191f, -0.5f, 0f), 1f, 1),
                ("PF Village Props - Weapon Rack.prefab", new Vector3(197f, -1.5f, 0f), 1f, 3),
                ("PF Village Props - Gunny Bag 01.prefab", new Vector3(203f, -1.5f, 0f), 1f, 3),
                ("PF Village Props - Crate Large.prefab", new Vector3(215f, -1.5f, 0f), 1f, 3),
                ("PF Village Props - Rock 03.prefab", new Vector3(227f, -0.5f, 0f), 1f, 1),
                ("PF Village Props - Torch.prefab", new Vector3(240f, -0.5f, 0f), 1f, 5),
                ("PF Village Props - Brick Wall 01.prefab", new Vector3(249f, 0.5f, 0f), 1f, 1),
                ("PF Village Props - Statue.prefab", new Vector3(258f, 0.5f, 0f), 1f, 2),
                ("PF Village Props - Gravestone 01.prefab", new Vector3(271f, -0.5f, 0f), 1f, 2),
                ("PF Village Props - Gravestone 03.prefab", new Vector3(280f, -0.5f, 0f), 1f, 2),
                ("PF Village Props - Fire Bowl.prefab", new Vector3(289f, -0.5f, 0f), 1f, 5),
                ("PF Village Props - Banner.prefab", new Vector3(295f, -1.5f, 0f), 1f, 4),
                ("PF Village Props - Archery Target 01.prefab", new Vector3(303f, -1.5f, 0f), 1f, 3),
                ("PF Village Props - Heavy Sword.prefab", new Vector3(311f, -1.5f, 0f), 1f, 4),
                ("PF Village Props - Banner.prefab", new Vector3(319f, -1.5f, 0f), 1f, 4),
                ("PF Village Props - Fire Bowl.prefab", new Vector3(328f, -1.5f, 0f), 1f, 5)
            };

            foreach ((string prefab, Vector3 position, float scale, int order) in props)
            {
                CreateDecoration(decorationRoot, prefab, position, scale, order, null);
            }

            CreateAreaMarker(decorationRoot, "ENTRADA DA ALDEIA", 6f, -1.5f);
            CreateAreaMarker(decorationRoot, "ALDEIA MEDIEVAL", 36f, -0.5f);
            CreateAreaMarker(decorationRoot, "BOSQUE", 72f, -1.5f);
            CreateAreaMarker(decorationRoot, "PONTE VELHA", 132f, -1.5f);
            CreateAreaMarker(decorationRoot, "ACAMPAMENTO BANDIDO", 177f, -0.5f);
            CreateAreaMarker(decorationRoot, "RUINAS", 246f, 0.5f);
            CreateAreaMarker(decorationRoot, "ARENA DO LIDER", 291f, -1.5f);
        }

        private static GameObject CreateDecoration(Transform parent, string prefabName, Vector3 position,
            float scale, int sortingOrder, Color? tint)
        {
            GameObject prop = InstantiateCainosPrefab(prefabName, parent);
            prop.transform.position = position;
            prop.transform.localScale = Vector3.one * scale;
            foreach (Collider2D collider in prop.GetComponentsInChildren<Collider2D>(true))
            {
                collider.enabled = false;
            }

            foreach (Rigidbody2D body in prop.GetComponentsInChildren<Rigidbody2D>(true))
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
                body.simulated = false;
            }

            DisablePackageBehaviours(prop);
            foreach (SpriteRenderer renderer in prop.GetComponentsInChildren<SpriteRenderer>(true))
            {
                renderer.sortingOrder += sortingOrder;
                if (tint.HasValue)
                {
                    renderer.color = tint.Value;
                }
            }

            return prop;
        }

        private static GameObject InstantiateCainosPrefab(string prefabName, Transform parent)
        {
            string path = $"{CainosPrefabDirectory}/{prefabName}";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Prefab Cainos nao encontrado: {path}");
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(parent);
            return instance;
        }

        private static void DisablePackageBehaviours(GameObject target)
        {
            foreach (MonoBehaviour behaviour in target.GetComponentsInChildren<MonoBehaviour>(true))
            {
                behaviour.enabled = false;
            }
        }

        private static void SetLayerRecursively(GameObject target, int layer)
        {
            foreach (Transform child in target.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = layer;
            }
        }

        private static void CreateAreaMarker(Transform parent, string label, float x, float groundY)
        {
            GameObject marker = CreateDecoration(parent, "PF Village Props - Sign 01.prefab",
                new Vector3(x, groundY, 0f), 0.9f, 5, null);
            TextMesh text = new GameObject($"Titulo - {label}").AddComponent<TextMesh>();
            text.transform.SetParent(marker.transform, false);
            text.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            text.text = label;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 42;
            text.characterSize = 0.045f;
            text.color = new Color(1f, 0.9f, 0.55f);
            text.GetComponent<MeshRenderer>().sortingOrder = 8;
        }

        private static void CreateHealingItems(Scene scene)
        {
            Transform root = new GameObject("Provisoes de Vida").transform;
            SceneManager.MoveGameObjectToScene(root.gameObject, scene);
            (string prefab, Vector3 position, int amount)[] items =
            {
                ("PF Village Props - Apple 01.prefab", new Vector3(53f, 1.2f, 0f), 1),
                ("PF Village Props - White Bottle.prefab", new Vector3(91f, 1.8f, 0f), 2),
                ("PF Village Props - Apple 01.prefab", new Vector3(121f, 1.25f, 0f), 1),
                ("PF Village Props - White Bottle.prefab", new Vector3(166.5f, 0.3f, 0f), 2),
                ("PF Village Props - Apple 01.prefab", new Vector3(190f, 1.25f, 0f), 1),
                ("PF Village Props - White Bottle.prefab", new Vector3(213f, 0.35f, 0f), 2),
                ("PF Village Props - Apple 01.prefab", new Vector3(278f, 1.2f, 0f), 1),
                ("PF Village Props - White Bottle.prefab", new Vector3(307f, 0.25f, 0f), 2)
            };

            foreach ((string prefab, Vector3 position, int amount) item in items)
            {
                GameObject pickup = InstantiateCainosPrefab(item.prefab, root);
                pickup.name = $"Provisao de Vida +{item.amount}";
                pickup.transform.position = item.position;
                pickup.transform.localScale = Vector3.one * 1.15f;
                foreach (Collider2D collider in pickup.GetComponentsInChildren<Collider2D>(true))
                {
                    collider.enabled = false;
                }

                DisablePackageBehaviours(pickup);
                foreach (SpriteRenderer renderer in pickup.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    renderer.sortingOrder += 9;
                    renderer.color = new Color(1f, 0.92f, 0.72f, 1f);
                }

                CircleCollider2D trigger = pickup.AddComponent<CircleCollider2D>();
                trigger.isTrigger = true;
                trigger.radius = 0.55f;
                HealingItem2D healing = pickup.AddComponent<HealingItem2D>();
                healing.Configure(item.amount);
            }
        }

        private static void CreateGoal(Scene scene, Vector3 position)
        {
            GameObject goal = new("Estandarte de Saida");
            SceneManager.MoveGameObjectToScene(goal, scene);
            goal.transform.position = position;
            BoxCollider2D trigger = goal.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(2f, 3f);
            trigger.offset = new Vector2(0f, 1.3f);
            goal.AddComponent<GameGoal2D>();

            GameObject banner = InstantiateCainosPrefab("PF Village Props - Banner.prefab", goal.transform);
            banner.name = "Bandeira Dourada Cainos";
            banner.transform.localPosition = new Vector3(0f, 0f, 0f);
            banner.transform.localScale = Vector3.one * 1.25f;
            foreach (Collider2D collider in banner.GetComponentsInChildren<Collider2D>(true))
            {
                collider.enabled = false;
            }
            DisablePackageBehaviours(banner);
            foreach (SpriteRenderer renderer in banner.GetComponentsInChildren<SpriteRenderer>(true))
            {
                renderer.sortingOrder += 7;
                renderer.color = new Color(1f, 0.78f, 0.25f, 1f);
            }

            TextMesh label = new GameObject("Rotulo do Objetivo").AddComponent<TextMesh>();
            label.transform.SetParent(goal.transform, false);
            label.transform.localPosition = new Vector3(0f, 3.25f, 0f);
            label.text = "SAIDA";
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 48;
            label.characterSize = 0.07f;
            label.color = Gold;
        }

        private static GameUI CreateInterface(Scene scene, GameManager manager)
        {
            GameObject canvasObject = new("Interface");
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            GameUI ui = canvasObject.AddComponent<GameUI>();

            Sprite boardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BoardSpritePath);
            Sprite buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath);
            Sprite pressedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonPressedSpritePath);

            GameObject hud = CreateHud(canvasObject.transform, out Text score, out Text timer,
                out Text health, out Text defeated, out Text objective, out Text message);
            GameObject menu = CreateMenuPanel(canvasObject.transform, "Menu Inicial", boardSprite,
                "A JORNADA DO CAVALEIRO", "Uma aldeia sitiada. Um lider cruel alem dos caminhos.\n\n" +
                "Atenda ao chamado e devolva a paz a estas terras.");
            CreateButton(menu.transform, "Iniciar", new Vector2(0f, -210f), buttonSprite, pressedSprite,
                manager.StartGame);

            OpeningSequenceUI introduction = CreateOpeningSequence(canvasObject.transform, manager,
                boardSprite, buttonSprite, pressedSprite);

            GameObject pause = CreateResultPanel(canvasObject.transform, "Menu de Pausa", boardSprite,
                "JOGO PAUSADO", "A jornada aguarda seu retorno.", out _);
            CreateButton(pause.transform, "Continuar", new Vector2(0f, -70f), buttonSprite, pressedSprite,
                manager.ResumeGame);
            CreateButton(pause.transform, "Reiniciar", new Vector2(0f, -180f), buttonSprite, pressedSprite,
                manager.RestartGame);
            CreateButton(pause.transform, "Menu inicial", new Vector2(0f, -290f), buttonSprite, pressedSprite,
                manager.ReturnToMenu);

            GameObject victory = CreateResultPanel(canvasObject.transform, "Tela de Vitoria", boardSprite,
                "MISSAO CUMPRIDA!", string.Empty, out Text victoryStats);
            CreateButton(victory.transform, "Jogar novamente", new Vector2(0f, -185f), buttonSprite,
                pressedSprite, manager.RestartGame);
            CreateButton(victory.transform, "Menu inicial", new Vector2(0f, -295f), buttonSprite,
                pressedSprite, manager.ReturnToMenu);

            GameObject defeat = CreateResultPanel(canvasObject.transform, "Tela de Derrota", boardSprite,
                "O CAVALEIRO CAIU", string.Empty, out Text defeatStats);
            CreateButton(defeat.transform, "Tentar novamente", new Vector2(0f, -185f), buttonSprite,
                pressedSprite, manager.RestartGame);
            CreateButton(defeat.transform, "Menu inicial", new Vector2(0f, -295f), buttonSprite,
                pressedSprite, manager.ReturnToMenu);

            ui.Configure(menu, hud, pause, victory, defeat, introduction, score, timer, health, defeated,
                objective, message, victoryStats, defeatStats);
            hud.SetActive(false);
            introduction.gameObject.SetActive(false);
            pause.SetActive(false);
            victory.SetActive(false);
            defeat.SetActive(false);
            return ui;
        }

        private static OpeningSequenceUI CreateOpeningSequence(Transform parent, GameManager manager,
            Sprite boardSprite, Sprite buttonSprite, Sprite pressedSprite)
        {
            GameObject panel = CreateOverlayPanel("Introducao Narrativa", parent);
            panel.GetComponent<Image>().color = new Color(0.015f, 0.025f, 0.055f, 0.82f);
            CanvasGroup screenGroup = panel.AddComponent<CanvasGroup>();
            OpeningSequenceUI sequence = panel.AddComponent<OpeningSequenceUI>();

            CreateText(panel.transform, "Marca do Prologo", "A JORNADA DO CAVALEIRO", 25, Gold,
                TextAnchor.MiddleCenter, new Vector2(0f, 420f), new Vector2(900f, 46f),
                new Vector2(0.5f, 0.5f), FontStyle.Bold);

            GameObject horizon = CreateRectObject("Luz sobre a Aldeia", panel.transform);
            RectTransform horizonRect = horizon.GetComponent<RectTransform>();
            horizonRect.anchorMin = horizonRect.anchorMax = new Vector2(0.5f, 0.5f);
            horizonRect.sizeDelta = new Vector2(1920f, 430f);
            horizonRect.anchoredPosition = new Vector2(0f, 175f);
            Image horizonImage = horizon.AddComponent<Image>();
            horizonImage.color = new Color(0.15f, 0.1f, 0.16f, 0.42f);
            horizonImage.raycastTarget = false;

            CreatePortrait(panel.transform, "Retrato do Cavaleiro",
                LoadSpriteByName(HeroSpritePath, "HeroKnight_0"), new Vector2(-470f, 190f),
                new Vector2(500f, 390f), false);
            CreatePortrait(panel.transform, "Bandido Invasor",
                LoadSpriteByName(LightBanditSpritePath, "LightBandit_0"), new Vector2(385f, 195f),
                new Vector2(350f, 315f), true);
            Image leader = CreatePortrait(panel.transform, "Lider dos Bandidos",
                LoadSpriteByName(HeavyBanditSpritePath, "HeavyBandit_0"), new Vector2(590f, 185f),
                new Vector2(430f, 365f), true);
            leader.color = new Color(0.78f, 0.68f, 0.68f, 1f);

            CreateText(panel.transform, "Legenda Cavaleiro", "O CAVALEIRO", 21, Color.white,
                TextAnchor.MiddleCenter, new Vector2(-470f, 40f), new Vector2(330f, 45f),
                new Vector2(0.5f, 0.5f), FontStyle.Bold);
            CreateText(panel.transform, "Legenda Ameaca", "A AMEACA", 21,
                new Color(1f, 0.55f, 0.42f), TextAnchor.MiddleCenter, new Vector2(500f, 40f),
                new Vector2(330f, 45f), new Vector2(0.5f, 0.5f), FontStyle.Bold);

            GameObject contentObject = CreateRectObject("Conteudo da Historia", panel.transform);
            Stretch(contentObject.GetComponent<RectTransform>());
            CanvasGroup contentGroup = contentObject.AddComponent<CanvasGroup>();
            GameObject board = CreateBoard(contentObject.transform, boardSprite, new Vector2(1240f, 430f));
            board.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -285f);

            Text kicker = CreateText(board.transform, "Capitulo", string.Empty, 22,
                new Color(0.55f, 0.24f, 0.1f), TextAnchor.MiddleCenter, new Vector2(0f, 130f),
                new Vector2(960f, 38f), new Vector2(0.5f, 0.5f), FontStyle.Bold);
            Text title = CreateText(board.transform, "Titulo Narrativo", string.Empty, 38, Ink,
                TextAnchor.MiddleCenter, new Vector2(0f, 75f), new Vector2(1050f, 62f),
                new Vector2(0.5f, 0.5f), FontStyle.Bold);
            Text story = CreateText(board.transform, "Narrativa", string.Empty, 27, Ink,
                TextAnchor.MiddleCenter, new Vector2(0f, -18f), new Vector2(1010f, 125f),
                new Vector2(0.5f, 0.5f));
            story.horizontalOverflow = HorizontalWrapMode.Wrap;
            Text page = CreateText(board.transform, "Pagina", string.Empty, 19,
                new Color(0.45f, 0.24f, 0.12f), TextAnchor.MiddleCenter, new Vector2(0f, -142f),
                new Vector2(200f, 32f), new Vector2(0.5f, 0.5f), FontStyle.Bold);

            GameObject controlsAccent = CreateRectObject("Destaque dos Controles", board.transform);
            RectTransform accentRect = controlsAccent.GetComponent<RectTransform>();
            accentRect.anchorMin = accentRect.anchorMax = new Vector2(0.5f, 0.5f);
            accentRect.sizeDelta = new Vector2(900f, 3f);
            accentRect.anchoredPosition = new Vector2(0f, -91f);
            Image accentImage = controlsAccent.AddComponent<Image>();
            accentImage.color = new Color(0.62f, 0.31f, 0.12f, 0.75f);
            accentImage.raycastTarget = false;

            Button nextButton = CreateButton(panel.transform, "Continuar introducao",
                new Vector2(640f, -330f), buttonSprite, pressedSprite, sequence.AdvancePage);
            RectTransform nextRect = nextButton.GetComponent<RectTransform>();
            nextRect.sizeDelta = new Vector2(310f, 76f);
            Text nextLabel = nextButton.transform.Find("Texto").GetComponent<Text>();
            nextLabel.rectTransform.sizeDelta = nextRect.sizeDelta;
            nextLabel.fontSize = 25;

            Button skipButton = CreateButton(panel.transform, "Pular introducao",
                new Vector2(760f, 420f), buttonSprite, pressedSprite, sequence.Skip);
            RectTransform skipRect = skipButton.GetComponent<RectTransform>();
            skipRect.sizeDelta = new Vector2(300f, 64f);
            Text skipLabel = skipButton.transform.Find("Texto").GetComponent<Text>();
            skipLabel.text = "PULAR  [S / ESC]";
            skipLabel.fontSize = 21;
            skipLabel.rectTransform.sizeDelta = skipRect.sizeDelta;

            CreateText(panel.transform, "Dica de Navegacao", "ESPACO / ENTER  •  avancar", 18,
                new Color(1f, 1f, 1f, 0.72f), TextAnchor.MiddleLeft, new Vector2(-895f, -335f),
                new Vector2(430f, 38f), new Vector2(0f, 0.5f));

            sequence.Configure(screenGroup, contentGroup, kicker, title, story, page, nextLabel,
                controlsAccent, manager);
            return sequence;
        }

        private static Image CreatePortrait(Transform parent, string name, Sprite sprite,
            Vector2 position, Vector2 size, bool flipX)
        {
            GameObject portrait = CreateRectObject(name, parent);
            RectTransform rect = portrait.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localScale = new Vector3(flipX ? -1f : 1f, 1f, 1f);
            Image image = portrait.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private static GameObject CreateHud(Transform parent, out Text score, out Text timer,
            out Text health, out Text defeated, out Text objective, out Text message)
        {
            GameObject hud = CreateRectObject("HUD", parent);
            Stretch(hud.GetComponent<RectTransform>());

            GameObject topBar = CreateRectObject("Barra Superior", hud.transform);
            RectTransform barRect = topBar.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 1f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.sizeDelta = new Vector2(0f, 95f);
            Image barImage = topBar.AddComponent<Image>();
            barImage.color = new Color(0.08f, 0.04f, 0.02f, 0.82f);
            barImage.raycastTarget = false;

            score = CreateText(topBar.transform, "Pontos", "Pontos: 0000", 30, Gold,
                TextAnchor.MiddleLeft, new Vector2(32f, -18f), new Vector2(320f, 48f),
                new Vector2(0f, 1f));
            health = CreateText(topBar.transform, "Vida", "Vida: 5/5", 30, Gold,
                TextAnchor.MiddleLeft, new Vector2(350f, -18f), new Vector2(260f, 48f),
                new Vector2(0f, 1f));
            defeated = CreateText(topBar.transform, "Inimigos", "Inimigos: 0", 30, Gold,
                TextAnchor.MiddleLeft, new Vector2(650f, -18f), new Vector2(300f, 48f),
                new Vector2(0f, 1f));
            timer = CreateText(topBar.transform, "Tempo", "Tempo: 00:00", 30, Gold,
                TextAnchor.MiddleRight, new Vector2(-32f, -18f), new Vector2(320f, 48f),
                new Vector2(1f, 1f));
            objective = CreateText(topBar.transform, "Objetivo", "Derrote o lider bandido.", 24,
                Color.white, TextAnchor.MiddleCenter, new Vector2(0f, -65f), new Vector2(1050f, 36f),
                new Vector2(0.5f, 1f));
            message = CreateText(hud.transform, "Mensagem", string.Empty, 32, Gold,
                TextAnchor.MiddleCenter, new Vector2(0f, -135f), new Vector2(1050f, 70f),
                new Vector2(0.5f, 1f));
            message.gameObject.AddComponent<Outline>().effectColor = Color.black;
            message.gameObject.SetActive(false);
            return hud;
        }

        private static GameObject CreateMenuPanel(Transform parent, string name, Sprite boardSprite,
            string title, string description)
        {
            GameObject panel = CreateOverlayPanel(name, parent);
            GameObject board = CreateBoard(panel.transform, boardSprite, new Vector2(1040f, 700f));
            CreateText(board.transform, "Titulo", title, 56, Ink, TextAnchor.MiddleCenter,
                new Vector2(0f, 220f), new Vector2(850f, 100f), new Vector2(0.5f, 0.5f), FontStyle.Bold);
            Text body = CreateText(board.transform, "Instrucoes", description, 29, Ink,
                TextAnchor.MiddleCenter, new Vector2(0f, 20f), new Vector2(820f, 300f),
                new Vector2(0.5f, 0.5f));
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Overflow;
            return panel;
        }

        private static GameObject CreateResultPanel(Transform parent, string name, Sprite boardSprite,
            string title, string defaultStats, out Text stats)
        {
            GameObject panel = CreateOverlayPanel(name, parent);
            GameObject board = CreateBoard(panel.transform, boardSprite, new Vector2(900f, 680f));
            CreateText(board.transform, "Titulo", title, 50, Ink, TextAnchor.MiddleCenter,
                new Vector2(0f, 205f), new Vector2(760f, 90f), new Vector2(0.5f, 0.5f), FontStyle.Bold);
            stats = CreateText(board.transform, "Estatisticas", defaultStats, 30, Ink,
                TextAnchor.MiddleCenter, new Vector2(0f, 40f), new Vector2(720f, 220f),
                new Vector2(0.5f, 0.5f));
            return panel;
        }

        private static GameObject CreateOverlayPanel(string name, Transform parent)
        {
            GameObject panel = CreateRectObject(name, parent);
            Stretch(panel.GetComponent<RectTransform>());
            Image overlay = panel.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.72f);
            return panel;
        }

        private static GameObject CreateBoard(Transform parent, Sprite sprite, Vector2 size)
        {
            GameObject board = CreateRectObject("Painel de Madeira", parent);
            RectTransform rect = board.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            Image image = board.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            return board;
        }

        private static Button CreateButton(Transform panel, string label, Vector2 panelPosition,
            Sprite normalSprite, Sprite pressedSprite, UnityAction action)
        {
            Transform board = panel.Find("Painel de Madeira");
            Transform targetParent = board != null ? board : panel;
            GameObject buttonObject = CreateRectObject($"Botao {label}", targetParent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = panelPosition;
            rect.sizeDelta = new Vector2(360f, 88f);

            Image image = buttonObject.AddComponent<Image>();
            image.sprite = normalSprite;
            image.type = Image.Type.Sliced;
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            SpriteState state = button.spriteState;
            state.pressedSprite = pressedSprite;
            state.selectedSprite = pressedSprite;
            button.spriteState = state;
            button.transition = Selectable.Transition.SpriteSwap;
            UnityEventTools.AddPersistentListener(button.onClick, action);

            CreateText(buttonObject.transform, "Texto", label, 30, Color.white,
                TextAnchor.MiddleCenter, Vector2.zero, rect.sizeDelta, new Vector2(0.5f, 0.5f),
                FontStyle.Bold);
            return button;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize,
            Color color, TextAnchor alignment, Vector2 anchoredPosition, Vector2 size,
            Vector2 anchor, FontStyle style = FontStyle.Normal)
        {
            GameObject textObject = CreateRectObject(name, parent);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreateRectObject(string name, Transform parent)
        {
            GameObject result = new(name, typeof(RectTransform));
            result.transform.SetParent(parent, false);
            return result;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void CreateEventSystem(Scene scene)
        {
            GameObject eventSystemObject = new("EventSystem");
            SceneManager.MoveGameObjectToScene(eventSystemObject, scene);
            eventSystemObject.AddComponent<EventSystem>();
            InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }

        private static Sprite LoadSpriteByName(string assetPath, string spriteName)
        {
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<Sprite>()
                .FirstOrDefault(candidate => candidate.name == spriteName);
            if (sprite == null)
            {
                throw new InvalidOperationException($"Sprite {spriteName} nao encontrado em {assetPath}.");
            }

            return sprite;
        }

        private static void EnsureSceneFirstInBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(scene => scene.path == ScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
