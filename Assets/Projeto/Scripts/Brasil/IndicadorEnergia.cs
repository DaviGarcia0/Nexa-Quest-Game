using TMPro;
using UnityEngine;
namespace NexaQuest.Brasil
{
    public class IndicadorEnergia : MonoBehaviour
    {
        [SerializeField] TMP_Text texto;
        ProgressionManager progresso;
        void Start() { progresso=ProgressionManager.Instance; if(progresso!=null){progresso.Changed+=Atualizar;Atualizar();} }
        void Atualizar() { if(texto!=null)texto.text=progresso.Energia.ToString(); }
        void OnDestroy(){if(progresso!=null)progresso.Changed-=Atualizar;}
    }
}
