using UnityEngine;
namespace NexaQuest {
 public class UIButtonAudio:MonoBehaviour {
  public AudioCue cue=AudioCue.Normal;int lastFrame=-1;
  public void Play(){if(lastFrame==Time.frameCount)return;lastFrame=Time.frameCount;NexaAudioManager.Play(cue);}
 }
}
