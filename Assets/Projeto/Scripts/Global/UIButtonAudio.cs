using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace NexaQuest {
 public class UIButtonAudio:MonoBehaviour,IPointerDownHandler,IPointerUpHandler {
  public AudioCue cue=AudioCue.Normal;
  int lastFrame=-1,suppressClickFrame=-1;bool pointerPlayed;
  void Emit(){if(lastFrame==Time.frameCount)return;lastFrame=Time.frameCount;NexaAudioManager.Play(cue);}
  public void OnPointerDown(PointerEventData data){
   if(data.button!=PointerEventData.InputButton.Left)return;
   var button=GetComponent<Button>();if(button==null||!button.IsActive()||!button.IsInteractable())return;
   pointerPlayed=true;Emit();
  }
  public void OnPointerUp(PointerEventData data){if(data.button==PointerEventData.InputButton.Left&&pointerPlayed){suppressClickFrame=Time.frameCount;pointerPlayed=false;}}
  // O listener de onClick continua atendendo teclado/Submit; o mouse já soou no pressionar.
  public void Play(){if(suppressClickFrame!=Time.frameCount)Emit();}
  void OnDisable(){pointerPlayed=false;}
 }
}
