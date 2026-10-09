using System;
using UnityEngine;

namespace NexaQuest.Brasil
{
    [Serializable]
    public class PerguntaQuizFeira
    {
        [TextArea(2, 4)] public string enunciado;
        public string[] alternativas = new string[4];
        [Range(0, 3)] public int respostaCorreta;
        public string fonte;
    }

    [CreateAssetMenu(menuName = "Nexa Quest/Quiz da Feira", fileName = "PerguntasQuizFeira")]
    public class DadosQuizFeira : ScriptableObject
    {
        public PerguntaQuizFeira[] perguntas = new PerguntaQuizFeira[3];

        public bool EstaValido()
        {
            if (perguntas == null || perguntas.Length != 3) return false;
            foreach (var pergunta in perguntas)
            {
                if (pergunta == null || string.IsNullOrWhiteSpace(pergunta.enunciado) ||
                    pergunta.alternativas == null || pergunta.alternativas.Length != 4 ||
                    pergunta.respostaCorreta < 0 || pergunta.respostaCorreta > 3) return false;
                foreach (var alternativa in pergunta.alternativas)
                    if (string.IsNullOrWhiteSpace(alternativa)) return false;
            }
            return true;
        }
    }
}
