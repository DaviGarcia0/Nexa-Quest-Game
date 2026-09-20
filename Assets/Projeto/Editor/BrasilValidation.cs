using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NexaQuest.Brasil;

[InitializeOnLoad]
public static class BrasilValidation
{
    const string Key = "NexaQuest.Brasil.Validation";
    static BrasilValidation() { EditorApplication.playModeStateChanged += Changed; }
    [MenuItem("Nexa Quest/Testar Brasil")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Projeto/Scenes/Brasil.unity");
        var gameType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        EditorWindow.GetWindow(gameType).Show();
        SessionState.SetBool(Key, true);
        EditorApplication.isPlaying = true;
    }
    static void Changed(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false))
        {
            SessionState.SetBool(Key, false);
            new GameObject("Validacao_Temporaria_PlayMode").AddComponent<BrasilPlayValidation>();
        }
    }
}

// Existe apenas no Editor e durante o teste; nao e salvo na cena ou incluido no jogo.
public class BrasilPlayValidation : MonoBehaviour
{
    readonly List<string> results = new List<string>();
    readonly List<string> issues = new List<string>();
    int failed;
    void Check(bool condition, string message)
    {
        results.Add((condition ? "PASS: " : "FAIL: ") + message);
        if (!condition) failed++;
    }
    void Log(string message, string trace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Warning || type == LogType.Assert)
            issues.Add(type + ": " + message);
    }
    IEnumerator Start()
    {
        Application.logMessageReceived += Log;
        yield return null;
        var world = FindObjectOfType<BrazilWorld>();
        Check(world != null, "GameManager presente");
        if (world == null) { Finish(); yield break; }
        var player = world.player;
        var animator = player.GetComponent<Animator>();
        var body = player.GetComponent<Rigidbody2D>();
        Check(world.CurrentArea == world.initialArea, "Feira como area inicial");
        Check(Vector3.Distance(player.transform.position, world.initialSpawn.position) < 0.01f, "Spawn inicial aplicado");
        Check(world.areas.Count(a => a.gameObject.activeSelf) == 1, "Somente uma area ativa");
        Check(world.areas.All(a => a.map.GetComponent<Collider2D>() == null), "Nenhum collider nos mapas");
        Check(world.areas.All(a => a.transform.Find("Colisoes_Manuais").GetComponentsInChildren<Collider2D>(true).Length == 0), "Pastas de colisoes manuais vazias");
        Check(world.areas.SelectMany(a => a.GetComponentsInChildren<MapTransition>(true)).All(t => !t.gameObject.activeInHierarchy), "Saidas modelo desativadas");
        Check(FindObjectsOfType<Transform>(true).All(t => t.GetComponents<Component>().All(c => c != null)), "Sem scripts ausentes na cena");
        Check(body.gravityScale == 0 && (body.constraints & RigidbodyConstraints2D.FreezeRotation) != 0, "Fisica top-down sem gravidade e sem rotacao");
        Check(world.initialArea.map.bounds.size.y > Camera.main.orthographicSize * 2, "Mapa maior que o enquadramento");
        foreach (string kind in new[] { "Walk", "Idle" })
        {
            string path = "Assets/Projeto/Sprites/Quest/Organizados/Quest" + kind + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Check(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && importer.textureCompression == TextureImporterCompression.Uncompressed, kind + ": Point, sem mipmap e sem compressao");
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            Check(sprites.Length == (kind == "Walk" ? 32 : 16), kind + ": quantidade de sprites");
        }
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip).Single();
            var frames = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            int n = clip.name.StartsWith("Walk") ? 8 : 4;
            Check(frames.Length == n + 1 && frames.Take(n).Select((k,i) => k.value != null && k.value.name == clip.name + "_" + (i+1).ToString("D2")).All(v=>v) && frames[n].value == frames[0].value, clip.name + ": ordem e loop dos frames");
        }
        yield return new WaitForEndOfFrame();
        Capture("Feira-inicial.png");
        // Desliga apenas a coleta do teclado; chama o FixedUpdate de producao a cada tick.
        player.enabled = false;
        Vector2[] directions = { Vector2.down, Vector2.left, Vector2.right, Vector2.up, new Vector2(1,1), new Vector2(-1,1), new Vector2(1,-1), new Vector2(-1,-1) };
        int[] facing = { 0, 1, 2, 3, 2, 1, 2, 1 };
        string[] names = { "Down", "Left", "Right", "Up" };
        for (int d = 0; d < directions.Length; d++)
        {
            player.Teleport(new Vector3(0, 0, 0));
            yield return new WaitForFixedUpdate();
            Vector2 start = body.position;
            player.SetMovementInput(directions[d]);
            for (int step = 0; step < 20; step++)
            {
                player.SendMessage("FixedUpdate");
                yield return new WaitForFixedUpdate();
            }
            yield return null;
            float distance = Vector2.Distance(start, body.position);
            float expected = player.moveSpeed * Time.fixedDeltaTime * 20;
            Check(Mathf.Abs(distance - expected) < 0.025f, "Movimento " + directions[d] + ": distancia " + distance.ToString("F3") + ", esperada " + expected.ToString("F3"));
            Check(player.FacingDirection == facing[d] && animator.GetCurrentAnimatorStateInfo(0).IsName("Walk" + names[facing[d]]), "Walk correto para " + directions[d]);
            player.SetMovementInput(Vector2.zero);
            yield return new WaitForSeconds(0.08f);
            Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle" + names[facing[d]]), "Idle conserva direcao " + names[facing[d]]);
            if (d < 4) { yield return new WaitForEndOfFrame(); Capture("Idle" + names[d] + ".png"); }
        }
        player.Teleport(Vector3.zero);
        world.followCamera.SnapToTarget();
        Vector3 oldCamera = world.followCamera.transform.position;
        player.Teleport(Vector3.right * 1.5f);
        yield return new WaitForSeconds(0.5f);
        Check(world.followCamera.transform.position.x > oldCamera.x + 1f, "Camera acompanha suavemente");
        var viewport = Camera.main.WorldToViewportPoint(player.transform.position);
        Check(viewport.x > 0 && viewport.x < 1 && viewport.y > 0 && viewport.y < 1, "Quest visivel pela camera");
        // Verifica o limite retangular sem adicionar colliders ao mapa.
        var bounds = world.CurrentArea.map.bounds;
        foreach (var corner in new[] { new Vector2(1,1), new Vector2(-1,1), new Vector2(1,-1), new Vector2(-1,-1) })
        {
            player.Teleport(new Vector3(corner.x > 0 ? bounds.max.x - 0.25f : bounds.min.x + 0.25f, corner.y > 0 ? bounds.max.y - 0.55f : bounds.min.y + 0.05f, 0));
            player.SetMovementInput(corner);
            for (int i = 0; i < 4; i++) { player.SendMessage("FixedUpdate"); yield return new WaitForFixedUpdate(); }
            Check(bounds.Contains(new Vector3(body.position.x, body.position.y, bounds.center.z)), "Limite de area no canto " + corner);
        }
        player.SetMovementInput(Vector2.zero);
        player.Teleport(Vector3.zero);
        world.followCamera.SnapToTarget();
        // Trigger criado so durante Play: exercita o callback real e a transicao completa.
        var temporaryExit = new GameObject("Trigger_Temporario_Teste");
        temporaryExit.transform.position = player.transform.position;
        var box = temporaryExit.AddComponent<BoxCollider2D>(); box.isTrigger = true;
        var transition = temporaryExit.AddComponent<MapTransition>();
        transition.world = world; transition.destinationArea = world.areas[1]; transition.destinationSpawnPoint = world.areas[1].defaultSpawn;
        Physics2D.SyncTransforms();
        yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
        Check(world.IsTransitioning && player.ControlsLocked, "Trigger inicia fade e bloqueia controle");
        Check(!world.TravelTo(world.areas[2], world.areas[2].defaultSpawn), "Transicao concorrente rejeitada");
        player.SetMovementInput(Vector2.one);
        Check(player.MoveInput == Vector2.zero, "Input bloqueado durante fade");
        yield return new WaitForSecondsRealtime(0.85f);
        Check(world.CurrentArea == world.areas[1] && !world.IsTransitioning && !player.ControlsLocked && world.fadePanel.alpha == 0, "Floresta: troca, fade e controle restaurado");
        Check(Vector3.Distance(player.transform.position, world.areas[1].defaultSpawn.position) < 0.01f, "Spawn da Floresta aplicado");
        temporaryExit.SetActive(false);
        yield return new WaitForEndOfFrame(); Capture("Amazonia-play.png");
        Check(world.TravelTo(world.areas[2], world.areas[2].defaultSpawn), "Destino PontoTuristico aceito");
        yield return new WaitForSecondsRealtime(0.85f);
        Check(world.CurrentArea == world.areas[2] && !world.IsTransitioning && world.fadePanel.alpha == 0, "Ponto Turistico: transicao completa");
        yield return new WaitForEndOfFrame(); Capture("PontoTuristico-play.png");
        Check(world.TravelTo(world.areas[0], world.initialSpawn), "Retorno a Feira aceito");
        yield return new WaitForSecondsRealtime(0.85f);
        Check(world.CurrentArea == world.initialArea, "Retorno a Feira concluido");
        player.enabled = true;
        Check(issues.Count == 0, "Nenhum erro ou aviso durante o teste");
        Finish();
    }
    void Capture(string name)
    {
        var texture = ScreenCapture.CaptureScreenshotAsTexture();
        if (texture != null)
        {
            Directory.CreateDirectory("Documentation/Brasil/Validacao");
            File.WriteAllBytes("Documentation/Brasil/Validacao/" + name, texture.EncodeToPNG());
            Destroy(texture);
        }
    }
    void Finish()
    {
        Application.logMessageReceived -= Log;
        Directory.CreateDirectory("Documentation/Brasil/Validacao");
        File.WriteAllLines("Documentation/Brasil/Validacao/playmode.txt", results.Concat(issues).Concat(new[] { "FALHAS: " + failed, "Avisos/erros capturados: " + issues.Count }));
        var logsType = typeof(Editor).Assembly.GetType("UnityEditor.LogEntries");
        var countMethod = logsType.GetMethod("GetCountsByType", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        object[] counts = { 0, 0, 0 };
        countMethod.Invoke(null, counts);
        File.WriteAllText("Documentation/Brasil/Validacao/console.txt", "Erros: " + counts[0] + "\nAvisos: " + counts[1] + "\nMensagens: " + counts[2] + "\nData: " + DateTime.Now.ToString("s"));
        Debug.Log("BRASIL_VALIDATION_DONE: " + results.Count + " verificacoes; falhas=" + failed + "; avisos/erros=" + issues.Count);
        EditorApplication.isPlaying = false;
    }
}