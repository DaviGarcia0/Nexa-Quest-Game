using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
namespace NexaQuest {
 public enum AudioCue { Normal, Iniciar, QuizCorreto, QuizErrado, ErroFeira, Fusao }
 [DefaultExecutionOrder(-20000)] public class NexaAudioManager:MonoBehaviour {
  public static NexaAudioManager Instance {get;private set;}
  public const string VolumeKey="NexaQuest.Audio.MasterVolume";
  public AudioMixer mixer;
  public AudioSource musicSource,uiSource,sfxSource;
  public AudioClip menuMusic,feiraMusic,amazoniaMusic,pontoMusic,portalMusic;
  public AudioClip normalClick,iniciar,quizCorreto,quizErrado,erroFeira,fusao;
  public float MasterVolume {get;private set;}=1;
  public event Action<AudioCue> Played;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetStatics(){Instance=null;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void Bootstrap(){if(Instance==null){var prefab=Resources.Load<GameObject>("NexaGlobalSystems");if(prefab!=null)Instantiate(prefab);}}
  void Awake(){if(Instance!=null&&Instance!=this){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);musicSource.ignoreListenerPause=true;uiSource.ignoreListenerPause=true;sfxSource.ignoreListenerPause=false;MasterVolume=Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey,1));SceneManager.sceneLoaded+=SceneLoaded;}
  void Start(){ApplyVolume();SceneLoaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);}
  void OnDestroy(){if(Instance==this){SceneManager.sceneLoaded-=SceneLoaded;Instance=null;}}
  void SceneLoaded(Scene scene,LoadSceneMode mode){if(mode==LoadSceneMode.Additive)return;if(scene.name=="Brasil")PlayMusic(feiraMusic);else if(scene.name=="Menu"||scene.name=="MapaMundi"||scene.name=="MinhaQuest")PlayMusic(menuMusic);}
  public void SetMasterVolume(float value){MasterVolume=Mathf.Clamp01(value);ApplyVolume();PlayerPrefs.SetFloat(VolumeKey,MasterVolume);PlayerPrefs.Save();}
  void ApplyVolume(){float db=MasterVolume<=0?-80:Mathf.Max(-80,20*Mathf.Log10(MasterVolume));if(mixer!=null&&mixer.SetFloat("MasterVolume",db))AudioListener.volume=1;else AudioListener.volume=MasterVolume;}
  public void ReloadVolume(){MasterVolume=Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey,1));ApplyVolume();}
  public void PlayMusic(AudioClip clip){if(clip==null||musicSource.clip==clip&&musicSource.isPlaying)return;musicSource.Stop();musicSource.clip=clip;musicSource.loop=true;musicSource.Play();}
  public static void PlayArea(string area){if(Instance==null)return;var a=Instance;AudioClip clip=area=="Amazonia"?a.amazoniaMusic:area=="PontoTuristico"?a.pontoMusic:area=="Portal"?a.portalMusic:a.feiraMusic;a.PlayMusic(clip);}
  public static void Play(AudioCue cue){if(Instance!=null)Instance.PlayCue(cue);}
  public void PlayCue(AudioCue cue){bool ui=cue==AudioCue.Normal||cue==AudioCue.Iniciar;if(!ui&&PauseSettingsManager.IsOpen)return;
   AudioClip clip=cue==AudioCue.Normal?normalClick:cue==AudioCue.Iniciar?iniciar:cue==AudioCue.QuizCorreto?quizCorreto:cue==AudioCue.QuizErrado?quizErrado:cue==AudioCue.ErroFeira?erroFeira:fusao;
   if(clip==null)return;(ui?uiSource:sfxSource).PlayOneShot(clip);Played?.Invoke(cue);
  }
  public static void PlayQuizCorrect()=>Play(AudioCue.QuizCorreto);
  public static void PlayQuizWrong()=>Play(AudioCue.QuizErrado);
  public static void PlayFeiraError()=>Play(AudioCue.ErroFeira);
  public static void PlayMerge()=>Play(AudioCue.Fusao);
 }
}
