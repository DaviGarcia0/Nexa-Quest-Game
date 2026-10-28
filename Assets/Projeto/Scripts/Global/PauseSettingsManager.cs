using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NexaQuest.Brasil;
namespace NexaQuest {
 [DefaultExecutionOrder(-15000)] public class PauseSettingsManager:MonoBehaviour {
  public static PauseSettingsManager Instance {get;private set;}
  public static bool IsOpen=>Instance!=null&&Instance.aberto;
  static int consumedFrame=-1;
  public static bool GameplayBlocked=>IsOpen||consumedFrame==Time.frameCount;
  public GameObject panel;public Slider volume;
  bool aberto,oldAudioPause,oldPlayerLock,navigating;float oldTimeScale;
  BrazilWorld world;QuizFeira qf;QuizAmazonia qa;MinijogoFeira mf;MinijogoAmazoniaController ma;PainelDesafiosFeira pf;PainelDesafiosAmazonia pa;DialogueController dialog;PortalInteraction[] portals;
  GameObject oldSelection;
  void Awake(){if(Instance!=null&&Instance!=this){Destroy(this);return;}Instance=this;panel.SetActive(false);SceneManager.sceneLoaded+=OnSceneLoaded;}
  void OnDestroy(){if(Instance!=this)return;Close();SceneManager.sceneLoaded-=OnSceneLoaded;Instance=null;}
  void Start()=>OnSceneLoaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);
  void OnSceneLoaded(Scene scene,LoadSceneMode mode){if(mode==LoadSceneMode.Additive)return;Close();navigating=false;
   world=FindObjectOfType<BrazilWorld>();qf=FindObjectOfType<QuizFeira>(true);qa=FindObjectOfType<QuizAmazonia>(true);mf=FindObjectOfType<MinijogoFeira>(true);ma=FindObjectOfType<MinijogoAmazoniaController>(true);pf=FindObjectOfType<PainelDesafiosFeira>(true);pa=FindObjectOfType<PainelDesafiosAmazonia>(true);dialog=FindObjectOfType<DialogueController>();portals=FindObjectsOfType<PortalInteraction>(true);
   if(FindObjectOfType<EventSystem>()==null)new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
  }
  public bool ActivityConsumesEsc()=>NexaQuest.MinhaQuest.MinhaQuestController.ConsumesEscape||world!=null&&(world.IsTransitioning||world.player.ControlsLocked)||qf!=null&&qf.Estado!=EstadoQuizFeira.Fechado||qa!=null&&qa.Estado!=EstadoQuizAmazonia.Fechado||mf!=null&&mf.Estado!=EstadoMinijogoFeira.Fechado||ma!=null&&ma.Estado!=EstadoMergeAmazonia.Fechado||pf!=null&&pf.EstaAberto||pa!=null&&pa.EstaAberto||dialog!=null&&dialog.IsOpen;
  void Update()=>ProcessInput(Input.GetKeyDown(KeyCode.P),Input.GetKeyDown(KeyCode.Escape));
  public void ProcessInput(bool p,bool esc){if(p){if(aberto)Close();else Open();}else if(esc){if(aberto)Close();else if(!ActivityConsumesEsc())Open();}}
  public void Open(){if(aberto||navigating||world!=null&&world.IsTransitioning)return;consumedFrame=Time.frameCount;oldTimeScale=Time.timeScale;oldAudioPause=AudioListener.pause;oldPlayerLock=world!=null&&world.player.ControlsLocked;
   oldSelection=EventSystem.current!=null?EventSystem.current.currentSelectedGameObject:null;aberto=true;Time.timeScale=0;AudioListener.pause=true;
   if(world!=null)world.player.SetControlsLocked(true);if(NexaAudioManager.Instance!=null){NexaAudioManager.Instance.sfxSource.Stop();volume.SetValueWithoutNotify(NexaAudioManager.Instance.MasterVolume);}panel.SetActive(true);if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(null);
  }
  public void Close(){if(!aberto)return;consumedFrame=Time.frameCount;aberto=false;panel.SetActive(false);Time.timeScale=oldTimeScale;AudioListener.pause=oldAudioPause;if(world!=null)world.player.SetControlsLocked(oldPlayerLock);if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(oldSelection);}
  public void MinhaQuest(){if(navigating)return;Close();navigating=true;NexaQuest.MinhaQuest.MinhaQuestNavigation.RegisterCurrent();SceneManager.LoadSceneAsync("MinhaQuest");}
  public void Sair(){Close();
#if UNITY_EDITOR
   Debug.Log("Sair solicitado: Application.Quit no Standalone.");
#else
   Application.Quit();
#endif
  }
 }
}
