using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering.Universal;
using NexaQuest.Brasil;

// Ferramenta de montagem usada uma vez. Nao roda automaticamente e nao entra no build.
public static class BrasilSetup
{
    const string ScenePath = "Assets/Projeto/Scenes/Brasil.unity";
    const string Generated = "Assets/Projeto/Sprites/Quest/Organizados/";
    const string Animations = "Assets/Projeto/Animations/Quest/";
    [Serializable] class Sheet { public float pixelsPerUnit; public Frame[] frames; }
    [Serializable] class Frame { public string name; public int x, y, width, height; public float pivotX, pivotY; }

    [MenuItem("Nexa Quest/Preparar Brasil")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (SceneManager.GetActiveScene().path != ScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
        }
        if (UnityEngine.Object.FindObjectOfType<BrazilWorld>(true) != null)
        {
            Debug.Log("Brasil ja preparada. A montagem nao sobrescreve a cena existente.");
            return;
        }
        Directory.CreateDirectory(Animations);
        Directory.CreateDirectory("Assets/Projeto/Prefabs/Brasil");
        Directory.CreateDirectory("Assets/Projeto/Materials/Brasil");
        AssetDatabase.Refresh();
        ImportSheet("Walk");
        ImportSheet("Idle");
        var controller = CreateAnimator();
        var material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        AssetDatabase.CreateAsset(material, "Assets/Projeto/Materials/Brasil/PixelArtUnlit.mat");
        var maps = new GameObject("Maps").transform;
        string[] names = { "Feira", "Amazonia", "PontoTuristico" };
        string[] textures = { "Feira.png", "Amazonas.png", "Ponto Turisitico.png" };
        Vector2[] spawnPixels = { new Vector2(960, 735), new Vector2(832, 777), new Vector2(925, 512) };
        var areas = new BrazilArea[3];
        for (int i = 0; i < areas.Length; i++)
        {
            string path = "Assets/Projeto/Sprites/Brasil/Mapa/" + textures[i];
            ConfigureTexture(path, SpriteImportMode.Single, 100);
            var area = new GameObject(names[i]).AddComponent<BrazilArea>();
            area.transform.SetParent(maps, false);
            var map = new GameObject("Mapa").AddComponent<SpriteRenderer>();
            map.transform.SetParent(area.transform, false);
            map.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            map.sharedMaterial = material;
            map.sortingOrder = -100;
            area.map = map;
            var spawns = new GameObject("SpawnPoints").transform;
            spawns.SetParent(area.transform, false);
            var spawn = new GameObject("EntradaSul").transform;
            spawn.SetParent(spawns, false);
            spawn.localPosition = new Vector3((spawnPixels[i].x - 836f) / 100f, (470.5f - spawnPixels[i].y) / 100f, 0);
            area.defaultSpawn = spawn;
            var collisions = new GameObject("Colisoes_Manuais").transform;
            collisions.SetParent(area.transform, false);
            var transitions = new GameObject("Transitions").transform;
            transitions.SetParent(area.transform, false);
            areas[i] = area;
        }
        var playerRoot = new GameObject("Player").transform;
        var quest = new GameObject("Quest");
        quest.transform.SetParent(playerRoot, false);
        var renderer = quest.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprites("Idle").First(s => s.name == "IdleDown_01");
        renderer.sharedMaterial = material;
        renderer.sortingOrder = 10;
        var animator = quest.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        var body = quest.AddComponent<Rigidbody2D>();
        body.gravityScale = 0;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        var collider = quest.AddComponent<CapsuleCollider2D>();
        collider.direction = CapsuleDirection2D.Horizontal;
        collider.size = new Vector2(0.22f, 0.10f);
        collider.offset = new Vector2(0f, 0.05f);
        var physics = new PhysicsMaterial2D("QuestFeet") { friction = 0f, bounciness = 0f };
        AssetDatabase.CreateAsset(physics, "Assets/Projeto/Materials/Brasil/QuestFeet.physicsMaterial2D");
        collider.sharedMaterial = physics;
        var player = quest.AddComponent<PlayerMovement>();
        PrefabUtility.SaveAsPrefabAssetAndConnect(quest, "Assets/Projeto/Prefabs/Brasil/Quest.prefab", InteractionMode.AutomatedAction);
        quest.transform.position = areas[0].defaultSpawn.position;
        player.areaBounds = areas[0].map;

        var camera = Camera.main;
        camera.orthographic = true;
        camera.orthographicSize = 2.7f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.allowHDR = false;
        camera.allowMSAA = false;
        var data = camera.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = false;
        data.antialiasing = AntialiasingMode.None;
        var pixel = camera.gameObject.AddComponent<PixelPerfectCamera>();
        pixel.assetsPPU = 100;
        pixel.refResolutionX = 960;
        pixel.refResolutionY = 540;
        pixel.gridSnapping = PixelPerfectCamera.GridSnapping.PixelSnapping;
        pixel.cropFrame = PixelPerfectCamera.CropFrame.Windowbox;
        var follow = camera.gameObject.AddComponent<CameraFollow>();
        follow.target = quest.transform;
        follow.mapBounds = areas[0].map;
        follow.SnapToTarget();

        var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        var panel = new GameObject("FadePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panel.transform.SetParent(canvasObject.transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = Color.black;
        panel.GetComponent<Image>().raycastTarget = false;
        var fade = panel.GetComponent<CanvasGroup>();
        fade.alpha = 0; fade.blocksRaycasts = false; fade.interactable = false;
        var world = new GameObject("GameManager").AddComponent<BrazilWorld>();
        world.player = player; world.followCamera = follow; world.areas = areas;
        world.initialArea = areas[0]; world.initialSpawn = areas[0].defaultSpawn; world.fadePanel = fade;
        // Exemplos desativados: nenhuma saida fica ativa antes de definir as posicoes.
        for (int i = 0; i < areas.Length; i++)
        {
            int dest = i == 2 ? 1 : i + 1;
            var exit = new GameObject("Saida_MODELO_DESATIVADA");
            exit.SetActive(false);
            exit.transform.SetParent(areas[i].transform.Find("Transitions"), false);
            var trigger = exit.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true; trigger.size = new Vector2(0.6f, 0.2f);
            var transition = exit.AddComponent<MapTransition>();
            transition.world = world; transition.destinationArea = areas[dest];
            transition.destinationSpawnPoint = areas[dest].defaultSpawn;
            areas[i].gameObject.SetActive(i == 0);
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = quest;
        Debug.Log("BRASIL_SETUP_OK: cena, 48 sprites, 8 animacoes, prefab e transicoes preparados.");
    }

    static void ConfigureTexture(string path, SpriteImportMode mode, float ppu)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = mode;
        importer.spritePixelsPerUnit = ppu;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 4096;
        importer.alphaIsTransparency = true;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteGenerateFallbackPhysicsShape = false;
        importer.SetTextureSettings(settings);
        foreach (string platform in new[] { "Standalone", "Android", "iPhone", "WebGL" })
            importer.ClearPlatformTextureSettings(platform);
        var defaults = importer.GetDefaultPlatformTextureSettings();
        defaults.maxTextureSize = 4096;
        defaults.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SetPlatformTextureSettings(defaults);
        importer.SaveAndReimport();
    }

    static void ImportSheet(string kind)
    {
        string path = Generated + "Quest" + kind + ".png";
        var sheet = JsonUtility.FromJson<Sheet>(File.ReadAllText(Generated + "Quest" + kind + ".json"));
        ConfigureTexture(path, SpriteImportMode.Multiple, sheet.pixelsPerUnit);
        var factories = new SpriteDataProviderFactories(); factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(AssetImporter.GetAtPath(path));
        provider.InitSpriteEditorDataProvider();
        var old = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
        var rects = sheet.frames.Select(f => new SpriteRect {
            name = f.name, rect = new Rect(f.x, f.y, f.width, f.height),
            pivot = new Vector2(f.pivotX, f.pivotY), alignment = SpriteAlignment.Custom,
            spriteID = old.ContainsKey(f.name) ? old[f.name] : GUID.Generate()
        }).ToArray();
        provider.SetSpriteRects(rects);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
        provider.Apply();
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    static Sprite[] Sprites(string kind)
    {
        return AssetDatabase.LoadAllAssetsAtPath(Generated + "Quest" + kind + ".png").OfType<Sprite>().OrderBy(s => s.name).ToArray();
    }

    static AnimatorController CreateAnimator()
    {
        var controller = AnimatorController.CreateAnimatorControllerAtPath(Animations + "Quest.controller");
        controller.AddParameter("Direction", AnimatorControllerParameterType.Int);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter(new AnimatorControllerParameter { name = "WalkRate", type = AnimatorControllerParameterType.Float, defaultFloat = 1 });
        controller.AddParameter(new AnimatorControllerParameter { name = "IdleRate", type = AnimatorControllerParameterType.Float, defaultFloat = 1 });
        var machine = controller.layers[0].stateMachine;
        string[] dirs = { "Down", "Left", "Right", "Up" };
        foreach (string kind in new[] { "Idle", "Walk" })
        {
            var sprites = Sprites(kind);
            for (int d = 0; d < 4; d++)
            {
                string name = kind + dirs[d];
                var frames = sprites.Where(s => s.name.StartsWith(name + "_")).ToArray();
                if (frames.Length != (kind == "Walk" ? 8 : 4)) throw new Exception("Frames ausentes: " + name);
                float fps = kind == "Walk" ? 10f : 4f;
                var clip = new AnimationClip { name = name, frameRate = fps };
                var keys = new ObjectReferenceKeyframe[frames.Length + 1];
                for (int i = 0; i < frames.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
                keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / fps, value = frames[0] };
                AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
                var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                AssetDatabase.CreateAsset(clip, Animations + name + ".anim");
                var state = machine.AddState(name, new Vector3(kind == "Idle" ? 280 : 550, d * 85));
                state.motion = clip; state.writeDefaultValues = false;
                state.speedParameterActive = true; state.speedParameter = kind == "Walk" ? "WalkRate" : "IdleRate";
                if (name == "IdleDown") machine.defaultState = state;
                var transition = machine.AddAnyStateTransition(state);
                transition.hasExitTime = false; transition.duration = 0f; transition.canTransitionToSelf = false;
                transition.AddCondition(AnimatorConditionMode.Equals, d, "Direction");
                transition.AddCondition(kind == "Walk" ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less, 0.001f, "Speed");
            }
        }
        return controller;
    }
}