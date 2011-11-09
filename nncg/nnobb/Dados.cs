using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace CTL
{
    public class Dados
    {
        List<Vector> dados;
        List<List<Vector>> dadosDivididos;
        static Random r;
        int divisoes;
        int qtdDados;
        int qtdCaracteristicas;

        public Dados(double[,] nDados, int nSeed, int nDivisoes)
        {
            qtdDados = nDados.GetLength(0);
            qtdCaracteristicas = nDados.GetLength(1);
            dados = new List<Vector>(qtdDados);
            for (int i = 0; i < qtdDados; i++)
            {
                double[] t = new double[qtdCaracteristicas];
                for (int j = 0; j < qtdCaracteristicas; j++)
                {
                    t[j] = nDados[i,j];
                }
                dados.Add(new Vector(t));
            }
            
            r = new Random(nSeed);
            divisoes = nDivisoes;
            kFold(divisoes);
            kFold(divisoes);
        }


        public List<Vector> GetKFoldTeste(int nDivisoes, int Escolhido)
        {
            if (dadosDivididos == null)
            {
                kFold(nDivisoes);
                divisoes = nDivisoes;
            }
            if (divisoes != nDivisoes)
            {
                kFold(nDivisoes);
                divisoes = nDivisoes;
            }

            //+1 por caua de arredondamento
            List<Vector> teste = new List<Vector>(qtdDados/divisoes +1);

            for (int i = 0; i < dadosDivididos[Escolhido].Count; i++)
            {
                teste.Add(dadosDivididos[Escolhido][i]);
            }

            return teste;
        }

        public List<Vector> GetKFoldTreino(int nDivisoes, int escolhido)
        {
            if (dadosDivididos == null)
            {
                kFold(nDivisoes);
                divisoes = nDivisoes;
            }
            if (divisoes != nDivisoes)
            {
                kFold(nDivisoes);
                divisoes = nDivisoes;
            }

            //qntdade de treinamento é uma "unidade" a menos
            // nove décimos(9/10), sete oitavos (7/8) etc
            //+1 por caua de arredondamento
            List<Vector> treinoo = new List<Vector>((qtdDados * (divisoes-1)) / divisoes + 1);

            for (int grupo = 0; grupo < nDivisoes; grupo++)
            {
                if (grupo != escolhido)
                {
                    for (int i = 0; i < dadosDivididos[grupo].Count; i++)
                    {
                        treinoo.Add(dadosDivididos[grupo][i]);
                    }
                }
            }

            return treinoo;
        }

        private void kFold(int divisoes)
        {

            int qndPorDivisao;
            List<int> lista = new List<int>();

            for (int i = 0; i < qtdDados; i++)
            {
                lista.Add(i);
            }

            qndPorDivisao = (qtdDados/ divisoes);

            if (qndPorDivisao <= 0)
            {
                qndPorDivisao = 1;
            }

            //cria NDivisões para inserir os dados
            dadosDivididos = new List<List<Vector>>(divisoes);
            for (int i = 0; i < divisoes; i++)
			{
                dadosDivididos.Add(new List<Vector>());
			}

            while (lista.Count >0)
            //for (int i = 0; i < qnd; i++)
            {
                for (int nDivisoes = 0; nDivisoes < divisoes; nDivisoes++)
                {
                    if (lista.Count<=0)
                    {
                        break;
                    }

                    //pega próximo ponto ainda não dividido
                    int indice = r.Next(0, lista.Count);

                    dadosDivididos[nDivisoes].Add(dados[lista[indice]]);

                    lista.RemoveAt(indice);
                }
            }
        }

        internal void NewSeed()
        {
            r = new Random();
        }
    }
}
