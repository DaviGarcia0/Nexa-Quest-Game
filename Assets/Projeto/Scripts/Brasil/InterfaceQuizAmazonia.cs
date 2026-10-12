using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NexaQuest.Brasil
{
    public class InterfaceQuizAmazonia : MonoBehaviour
    {
        public GameObject painelInicial, painelPrincipal, painelFinal;
        public Button botaoVoltar, botaoContinuar;
        public TMP_Text recompensas;
        public CanvasGroup feedbackErro, feedbackAcerto;
        public TMP_Text progressoPerguntas, tempo, enunciado, status;
        public TMP_Text[] alternativas;
        public Button[] botoesRespostas;
        public Image retrato;
        public Sprite questPensando, questTriste, questFeliz;
        public Transform camadaHUD;
        public Transform[] hudsCompartilhadas;
        public TMP_Text avisoEnergia;
        Transform[] pais; int[] indices;
        Transform paiAviso; int indiceAviso; Vector2 posicaoAviso;
        bool emprestados;

        public void Preparar()
        {
            pais = new Transform[hudsCompartilhadas.Length]; indices = new int[pais.Length];
            for (int i = 0; i < pais.Length; i++)
            {
                pais[i] = hudsCompartilhadas[i].parent;
                indices[i] = hudsCompartilhadas[i].GetSiblingIndex();
            }
            paiAviso = avisoEnergia.transform.parent;
            indiceAviso = avisoEnergia.transform.GetSiblingIndex();
            posicaoAviso = avisoEnergia.rectTransform.anchoredPosition;
            for (int i = 0; i < pais.Length; i++) hudsCompartilhadas[i].SetParent(camadaHUD, false);
            avisoEnergia.transform.SetParent(transform, false);
            avisoEnergia.transform.SetAsLastSibling();
            avisoEnergia.rectTransform.anchoredPosition = new Vector2(0, -510);
            emprestados = true;
            OcultarAviso(); OcultarFeedback();
        }

        public void Restaurar()
        {
            OcultarFeedback();
            if (!emprestados) return;
            for (int i = 0; i < pais.Length; i++) if (pais[i] != null)
            {
                hudsCompartilhadas[i].SetParent(pais[i], false);
                hudsCompartilhadas[i].SetSiblingIndex(indices[i]);
            }
            avisoEnergia.transform.SetParent(paiAviso, false);
            avisoEnergia.transform.SetSiblingIndex(indiceAviso);
            avisoEnergia.rectTransform.anchoredPosition = posicaoAviso;
            OcultarAviso(); emprestados = false;
        }

        public void MostrarEstado(EstadoQuizAmazonia estado)
        {
            painelInicial.SetActive(estado == EstadoQuizAmazonia.Inicial);
            painelPrincipal.SetActive(estado == EstadoQuizAmazonia.Pergunta || estado == EstadoQuizAmazonia.Erro || estado == EstadoQuizAmazonia.Acerto);
            painelFinal.SetActive(estado == EstadoQuizAmazonia.Concluido);
            foreach (var botao in botoesRespostas)
            {
                botao.gameObject.SetActive(estado != EstadoQuizAmazonia.Concluido);
                botao.interactable = estado == EstadoQuizAmazonia.Pergunta;
            }
            if (estado == EstadoQuizAmazonia.Pergunta)
            {
                retrato.sprite = questPensando;
                status.text = "Escolha uma resposta.  |  Esc: voltar aos desafios";
            }
        }

        public void Carregar(PerguntaQuizAmazonia pergunta, int indice)
        {
            progressoPerguntas.text = (indice + 1) + "/3";
            enunciado.enableWordWrapping = true;
            enunciado.text = QuebrarEnunciado(pergunta.enunciado);
            for (int i = 0; i < 4; i++) alternativas[i].text = ((char)('A' + i)) + ") " + pergunta.alternativas[i];
            OcultarFeedback();
        }

        static string QuebrarEnunciado(string texto)
        {
            var palavras = (texto ?? string.Empty).Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
            var resultado = new System.Text.StringBuilder();
            for (int i = 0; i < palavras.Length; i++)
            {
                if (i > 0) resultado.Append(i % 7 == 0 ? '\n' : ' ');
                resultado.Append(palavras[i]);
            }
            return resultado.ToString();
        }
        public void MostrarTempo(float restante) => tempo.text = Mathf.CeilToInt(restante) + "s";

        public void MostrarFeedback(bool acerto, bool esgotou)
        {
            OcultarFeedback();
            retrato.sprite = acerto ? questFeliz : questTriste;
            status.text = esgotou ? "Tempo esgotado!" :
                acerto ? "+10 NexaCoins!" : "Tente outra alternativa.";
            var grupo = acerto ? feedbackAcerto : feedbackErro;
            grupo.alpha = 1; grupo.blocksRaycasts = true; grupo.gameObject.SetActive(true);
        }

        public void DefinirAlphaFeedback(float alpha)
        {
            if (feedbackErro.gameObject.activeSelf) feedbackErro.alpha = alpha;
            if (feedbackAcerto.gameObject.activeSelf) feedbackAcerto.alpha = alpha;
        }

        public void OcultarFeedback()
        {
            foreach (var grupo in new[] { feedbackErro, feedbackAcerto }) if (grupo != null)
            {
                grupo.gameObject.SetActive(false); grupo.alpha = 1; grupo.blocksRaycasts = false;
            }
        }

        public void MostrarRecompensas(int moedas, bool chaveNova)
        {
            recompensas.text = "Recompensas desta rodada: +" + moedas + " NexaCoins  |  +50 Conhecimento  |  " +
                (chaveNova ? "Chave da Amazônia conquistada" : "Chave da Amazônia já obtida");
        }

        public void AvisarEnergia()
        {
            avisoEnergia.text = "Energia insuficiente. Você precisa de 20 para jogar.";
            avisoEnergia.gameObject.SetActive(true);
        }
        public void OcultarAviso() => avisoEnergia.gameObject.SetActive(false);
        void OnDisable() { Restaurar(); }
    }
}
