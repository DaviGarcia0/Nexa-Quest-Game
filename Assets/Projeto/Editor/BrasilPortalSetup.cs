using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Experimental.Rendering.Universal;
using NexaQuest.Brasil;

public static class BrasilPortalSetup
{
    const string Root = "Assets/Projeto/Sprites/Brasil/Portal/";
    const string Report = "Documentation/Brasil/EtapaPortal/";
    static T Ensure<T>(GameObject g) where T : Component { var c=g.GetComponent<T>(); return c != null ? c : g.AddComponent<T>(); }
    static Transform Child(Transform parent, string name)
    {
        var found = parent.Find(name);
        if (found != null) return found;
        var t = new GameObject(name).transform; t.SetParent(parent, false); return t;
    }
    static Transform Spawn(BrazilArea area, string name, Vector2 p)
    {
        var root = Child(area.transform, "SpawnPoints");
        var existing = root.Find(name); if (existing != null) return existing;
        var t = Child(root, name); t.localPosition = p; return t;
    }
    static Sprite Import(string path, float ppu)
    {
        var i = (TextureImporter)AssetImporter.GetAtPath(path);
        i.textureType = TextureImporterType.Sprite; i.spriteImportMode = SpriteImportMode.Single;
        i.spritePixelsPerUnit = ppu; i.filterMode = FilterMode.Point;
        i.mipmapEnabled = false; i.npotScale = TextureImporterNPOTScale.None;
        i.textureCompression = TextureImporterCompression.Uncompressed; i.maxTextureSize = 4096;
        var s = new TextureImporterSettings(); i.ReadTextureSettings(s);
        s.spriteMeshType = SpriteMeshType.FullRect; s.spriteGenerateFallbackPhysicsShape = false; i.SetTextureSettings(s);
        foreach (string platform in new[]{"Standalone", "Android", "iPhone", "WebGL"}) i.ClearPlatformTextureSettings(platform);
        var d = i.GetDefaultPlatformTextureSettings(); d.maxTextureSize = 4096;
        d.textureCompression = TextureImporterCompression.Uncompressed; i.SetPlatformTextureSettings(d);
        i.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    static GameObject UI(Transform parent, string name, Vector2 size, Vector2 pos)
    {
        var old = parent.Find(name); if (old != null) return old.gameObject;
        var g = new GameObject(name, typeof(RectTransform)); g.transform.SetParent(parent, false);
        var r = (RectTransform)g.transform; r.sizeDelta = size; r.anchoredPosition = pos; return g;
    }
    static string PathOf(Transform t) { return t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name; }
    public static string CollisionSnapshot()
    {
        var text = new StringBuilder();
        foreach (var a in UnityEngine.Object.FindObjectsOfType<BrazilArea>(true).OrderBy(a=>a.name))
        {
            if (a.name == "Portal") continue;
            var root = a.transform.Find("Colisoes_Manuais");
            foreach (var c in root.GetComponentsInChildren<Collider2D>(true).OrderBy(c=>PathOf(c.transform)))
            {
                // isTrigger is the only allowed collider change, and only on named exits.
                var data = EditorJsonUtility.ToJson(c);
                if (c.name.StartsWith("Troca")) data = data.Replace("\"m_IsTrigger\":true", "\"m_IsTrigger\":false");
                text.AppendLine(PathOf(c.transform)+"|"+EditorJsonUtility.ToJson(c.transform)+"|"+data);
            }
        }
        return text.ToString();
    }
    [MenuItem("Nexa Quest/Atualizar Portal e transicoes")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Projeto/Scenes/Brasil.unity") { Debug.LogError("Abra Brasil para atualizar."); return; }
        Directory.CreateDirectory(Report);
        string before = CollisionSnapshot(); File.WriteAllText(Report+"colisoes-antes.txt", before);
        var w = UnityEngine.Object.FindObjectOfType<BrazilWorld>();
        foreach (var a in w.areas) Import(AssetDatabase.GetAssetPath(a.map.sprite), a.map.sprite.pixelsPerUnit);
        var off = Import(Root+"Portal.png",100);
        var on = Import(Root+"Portal ligado.png",100);
        var empty = Import(Root+"Painel de chaves do portal vazia.png",100);
        var full = Import(Root+"Painel de chaves do portal cheia.png",100);
        var feira = w.areas.Single(a=>a.name=="Feira");
        var amazon = w.areas.Single(a=>a.name=="Amazonia");
        var turismo = w.areas.Single(a=>a.name=="PontoTuristico");
        var portal = w.areas.FirstOrDefault(a=>a.name=="Portal");
        if (portal == null)
        {
            portal = Child(feira.transform.parent,"Portal").gameObject.AddComponent<BrazilArea>();
            var map = Child(portal.transform,"Mapa").gameObject.AddComponent<SpriteRenderer>();
            map.sprite=off; map.sharedMaterial=feira.map.sharedMaterial; map.sortingOrder=feira.map.sortingOrder; portal.map=map;
            Child(portal.transform,"Colisoes_Manuais"); Child(portal.transform,"Transitions");
            portal.defaultSpawn = Spawn(portal,"EntradaSul",new Vector2(0,-3.9f));
            w.areas=w.areas.Concat(new[]{portal}).ToArray();
        }
        var fa=Spawn(feira,"EntradaAmazonia",new Vector2(-3.91f,3.3f));
        var fp=Spawn(feira,"EntradaPortal",new Vector2(6.3f,3.2f));
        var af=Spawn(amazon,"EntradaFeira",new Vector2(-0.04f,-3.8f));
        var at=Spawn(amazon,"EntradaTurismo",new Vector2(7.4f,3.0f));
        var ta=Spawn(turismo,"EntradaAmazonia",new Vector2(-7.4f,1.49f));
        var lines=new StringBuilder();
        foreach(var a in w.areas.Where(a=>a!=portal))
        foreach(var t in a.transform.Find("Colisoes_Manuais").GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Troca")))
        {
            BrazilArea dest; Transform spawn;
            if(t.name=="TrocaAmazonia") { dest=amazon; spawn=a==feira?af:at; }
            else if(t.name=="TrocaFeira") {dest=feira;spawn=fa;}
            else if(t.name=="TrocaPontoTurisco") {dest=turismo;spawn=ta;}
            else if(t.name=="TrocaPortal") {dest=portal;spawn=portal.defaultSpawn;}
            else throw new Exception("Destino desconhecido: "+t.name);
            var c=t.GetComponent<BoxCollider2D>(); if(c==null) throw new Exception("Saida sem BoxCollider2D");
            c.isTrigger=true;
            var exit=Ensure<MapTransition>(t.gameObject);
            exit.world=w;exit.destinationArea=dest;exit.destinationSpawnPoint=spawn;
            lines.AppendLine(PathOf(t)+" -> "+PathOf(spawn));
        }
        var back=Child(portal.transform.Find("Transitions"),"RetornoFeira");
        back.localPosition=new Vector3(0,-4.55f,0);
        var box=Ensure<BoxCollider2D>(back.gameObject);
        box.isTrigger=true;box.size=new Vector2(0.8f,0.2f);
        var ret=Ensure<MapTransition>(back.gameObject);
        ret.world=w;ret.destinationArea=feira;ret.destinationSpawnPoint=fp;
        var canvas=w.fadePanel.GetComponentInParent<Canvas>();
        var hint=UI(canvas.transform,"InteractionPrompt",new Vector2(440,60),new Vector2(0,-420));
        var text=Ensure<Text>(hint.gameObject);text.text="E - Interagir";
        text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=30;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;
        var outline=Ensure<Outline>(hint.gameObject);outline.effectColor=Color.black;outline.effectDistance=new Vector2(2,-2);
        var panel=UI(canvas.transform,"PortalKeysPanel",new Vector2(1920,1080),Vector2.zero);
        var shade=Ensure<Image>(panel.gameObject);shade.color=new Color(0,0,0,0.8f);shade.raycastTarget=false;
        var art=UI(panel.transform,"PainelVazio",new Vector2(1200,900),new Vector2(0,30));
        var img=Ensure<Image>(art.gameObject);img.sprite=empty;img.preserveAspect=true;img.raycastTarget=false;
        var close=UI(panel.transform,"Fechar",new Vector2(800,55),new Vector2(0,-475));
        var label=Ensure<Text>(close.gameObject);label.font=text.font;label.fontSize=28;label.alignment=TextAnchor.MiddleCenter;label.text="E ou Esc - Fechar";label.raycastTarget=false;
        var prox=Child(portal.transform,"InteracaoPortal");prox.localPosition=new Vector3(0,1.25f,0);
        var region=Ensure<BoxCollider2D>(prox.gameObject);region.isTrigger=true;region.size=new Vector2(1.8f,1.2f);
        var interaction=Ensure<PortalInteraction>(prox.gameObject);
        interaction.world=w;interaction.interactionPrompt=hint;interaction.keysPanel=panel;interaction.portalMap=portal.map;interaction.activatedPortal=on;interaction.filledKeysPanel=full;
        hint.SetActive(false);panel.SetActive(false);portal.gameObject.SetActive(false);
        w.fadePanel.transform.SetAsLastSibling();
        var pixel=w.followCamera.GetComponent<PixelPerfectCamera>();var pixelSettings=new SerializedObject(pixel);pixelSettings.FindProperty("m_FilterMode").enumValueIndex=1;pixelSettings.ApplyModifiedPropertiesWithoutUndo();
        string after=CollisionSnapshot();File.WriteAllText(Report+"colisoes-depois.txt",after);
        if(before!=after) throw new Exception("Colisoes manuais divergiram! Cena nao salva.");
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        File.WriteAllText(Report+"configuracao.txt",lines.ToString()+"Colisoes preservadas; apenas isTrigger das 5 saidas mudou.\n");
        Debug.Log("BRASIL_PORTAL_SETUP_OK");
    }
}


