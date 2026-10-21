using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
namespace NexaQuest {
 [RequireComponent(typeof(VideoPlayer))] public class MenuVideoBackground:MonoBehaviour {
  public RawImage display;public Image fallback;VideoPlayer player;bool failed;
  void Awake(){player=GetComponent<VideoPlayer>();display.enabled=false;player.audioOutputMode=VideoAudioOutputMode.None;player.sendFrameReadyEvents=true;player.frameReady+=FrameReady;player.errorReceived+=Failed;}
  IEnumerator Start(){player.Stop();player.timeUpdateMode=VideoTimeUpdateMode.UnscaledGameTime;player.Prepare();while(!player.isPrepared&&!failed)yield return null;if(!failed)player.Play();}
  void FrameReady(VideoPlayer p,long frame){if(!p.isPlaying)return;if(!display.enabled){display.enabled=true;fallback.enabled=false;}p.sendFrameReadyEvents=false;}
  void Failed(VideoPlayer p,string message){failed=true;display.enabled=false;fallback.enabled=true;Debug.LogError("Vídeo do menu: "+message);}
  void OnDestroy(){if(player==null)return;player.frameReady-=FrameReady;player.errorReceived-=Failed;}
 }
}
