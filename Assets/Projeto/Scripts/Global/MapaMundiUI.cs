using System.Collections;
using TMPro;
using UnityEngine;
namespace NexaQuest {
 public class MapaMundiUI:MonoBehaviour {
  public GameObject popup;public TMP_Text message;Coroutine rotina;
  public void Indisponivel(){if(rotina!=null)StopCoroutine(rotina);popup.SetActive(true);message.text="Mapa indisponível por enquanto.";rotina=StartCoroutine(FecharDepois());}
  IEnumerator FecharDepois(){yield return new WaitForSecondsRealtime(2.5f);popup.SetActive(false);rotina=null;}
 }
}
