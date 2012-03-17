using System;
using System.Collections.Generic;
using System.Text;

namespace CTL
{
    public struct Teste
    {
        public int folds;
        public int seed;
        public int profundidade;
        public string desc;
		public Boolean AaA;
		public Boolean OaA;
		public Boolean Voto;
        public Boolean Bounds;
	    public Boolean MBC;
		public Boolean MPD;
        public Boolean TODOS;

        public Teste(Teste nTeste)
        {
            folds = nTeste.folds;
            desc = nTeste.desc;
            profundidade = nTeste.profundidade;
            seed = nTeste.seed;
			
			AaA = nTeste.AaA;
			OaA = nTeste.OaA;
			
			Voto = nTeste.Voto;
			
            Bounds = nTeste.Bounds;
            MBC = nTeste.MBC;
            MPD = nTeste.MPD;
            TODOS = nTeste.TODOS;
        }

    }

    public class Resultado
    {
        public List<result> resultados;
        public Teste teste;
        public Resultado(Teste nTeste){
            resultados = new List<result>();
			teste = nTeste;
        }

        public double testeMedia
        {
            get
            {
                int i;
                double med = 0;
                for (i = 0; i < resultados.Count; i++)
                {
                    med += resultados[i].testeAcerto;
                }
                return med / resultados.Count;
            }
        }
        public double testeDP
        {
            get
            {
                int i;
                double dp = 0;
                for (i = 0; i < resultados.Count; i++)
                {
                    dp += Math.Pow(resultados[i].testeAcerto, 2);
                }
                return Math.Sqrt((dp) / resultados.Count - testeMedia);
            }
        }
        public double treinoMedia
        {
            get
            {
                int i;
                double med = 0;
                for (i = 0; i < resultados.Count; i++)
                {
                    med += resultados[i].treinoAcerto;
                }
                return med / resultados.Count;
            }
        }
        public double treinoDP
        {
            get
            {
                int i;
                double dp = 0;
                for (i = 0; i < resultados.Count; i++)
                {
                    dp += Math.Pow(resultados[i].treinoAcerto, 2);
                }
                return Math.Sqrt((dp) / resultados.Count - treinoMedia);
            }
        }

        public List<string> ImprimeTeste()
        {
            List<string> log = new List<string>();
            int j;
            char tab = '\u0009';
            log.Add("Caso:" + tab.ToString() + teste.desc);
            log.Add("ProfMax:" + tab.ToString() +teste.profundidade);
            log.Add("Fold:" + tab.ToString() + teste.folds);
            log.Add("Seed:" + tab.ToString() + teste.seed);
            log.Add("TxAcerto(treino):" + tab.ToString() + this.treinoMedia);
            log.Add("TxAcerto(teste):" + tab.ToString() + this.testeMedia);
            log.Add("..:::..");

            for (j = 0; j < resultados.Count; j++)
            {
                log.Add(resultados[j].ToString());
            }
            return log;
        }

    }

    public class result
    {
        public int testeCertos;
        public int testeErrados;
        public int treinoCertos;
        public int treinoErrados;
        public int profundidadeMaxima;
        public int planos;
        public int padroes;

        public double testeAcerto {
            get{
                return testeCertos / (double)(testeCertos + testeErrados);
            }
        }
        public double treinoAcerto
        {
            get
            {
                return treinoCertos / (double)(treinoCertos + treinoErrados);
            }
        }

        public override string ToString()
        {
            char tab = '\u0009';
            return "" + treinoCertos + tab + treinoErrados + tab + testeCertos + tab + testeErrados + tab + profundidadeMaxima + tab + planos + tab + padroes;
        }

    }
}
