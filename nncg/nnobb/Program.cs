using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using System.Threading;
using CTL.NN;
using CTL.CT;


namespace CTL
{
    static class Program
    {
		
		public static bool arquivar = false;
			
		
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			
			System.Diagnostics.Stopwatch tempo = new System.Diagnostics.Stopwatch();
			tempo.Start();
			
            Teste t = new Teste();
            t.seed = 0;
            t.folds = 10;
            t.profundidade = 20;
            t.desc = "generico";
			t.Bounds = true;
			t.MBC = true;
			t.MPD  = false;
			t.TODOS  = false;
			
			List<Dados> data;
			
            //imprimeMelhorResultado(BateriaDeTestes(Flor, t, 1));
			data  = Vinho ();
			//data  = Segmentacao();
			
			
            imprimeMelhorResultado(BateriaDeTestes(data, t, 10));
            //imprimeMelhorResultado(BateriaDeTestes(Vinho, t, 1));
            //imprimeMelhorResultado(BateriaDeTestes(Vogal, t, 1));
            //imprimeMelhorResultado(BateriaDeTestes(Carro, t, 1));
            //imprimeMelhorResultado(BateriaDeTestes(Segmentacao, t, 1));
			Console.WriteLine(tempo.ElapsedMilliseconds.ToString ());
            //System.Console.ReadLine();
        }
		
		
        public static List<Resultado> BateriaDeTestes(List<Dados> nDados, Teste t, int repetições)
        {
            List<Resultado> resultFinal = new List<Resultado>();
            
			//for (int rep = t.seed; rep < (t.seed + repetições); rep++)
			while(repetições > 0)
			{
				foreach(Dados d in nDados){
					d.ReSeedDados(t.seed, t.folds);	
				}
	
				
				//realiza o teste para cada 'fold'

                resultFinal.Add(ExecutaTeste(nDados,t));
					
				imprimeResultado(resultFinal[resultFinal.Count -1]);
				
				t.seed++;
				repetições--;
            }
            return resultFinal;
        }
		

		
		
//        public void TestaPadrao()
//        {
//            TesteFuncão(Padrao, nbFolds, nbProf, nbSeed);
//        }		

		
        public static Resultado ExecutaTeste(List<Dados> grupos, Teste nTeste)
        {
			Resultado resultado = new Resultado(nTeste);

			for (int fold = 0; fold < nTeste.folds; fold++)
			{
	            List<List<Vector>> dadosTreino = new List<List<Vector>>();
	            List<List<Vector>> dadosTeste = new List<List<Vector>>();
	
	
	            for (int i = 0; i < grupos.Count; i++)
	            {
	                dadosTreino.Add(grupos[i].GetKFoldTreino(nTeste.folds, fold));
	                dadosTeste.Add(grupos[i].GetKFoldTeste(nTeste.folds, fold));
	            }
	
	            List<BoundingVolume> caixas = new List<BoundingVolume>();
				
				//Aqui Seleciona o tipo de bounding volume
	            for (int i = 0; i < grupos.Count; i++)
	            {
	                caixas.Add(new OBB(dadosTreino[i], "-" + i.ToString() + "-",0));
	            }
	
	    
				List<PreRedeNeural> preRede = new List<PreRedeNeural>();
				
				TesteColisao teste = new TesteColisao(nTeste);
	
	            preRede = teste.RealizaTeste(caixas);
				
				result result = new result();
	            foreach (BoundingVolume c in caixas)
	            {
	                result.profundidadeMaxima = Math.Max(result.profundidadeMaxima, c.MaxProfundidade());
	            }
	            foreach (PreRedeNeural p in preRede)
	            {
	                result.planos += p.planos.Count;
	            }
	            foreach (PreRedeNeural p in preRede)
	            {
	                result.padroes += p.padDentro.RowCount;
					result.padroes += p.padFora.RowCount;
	            }
				
	            List<Rede> redes = GeraRedesNeurais(preRede, nTeste);
				
	            result dummy = TestaPontos(caixas, redes);
	            result.treinoCertos = dummy.treinoCertos;
	            result.treinoErrados = dummy.treinoErrados;
				
	            if (dadosTeste.Count == dadosTreino.Count)
	            {
	                caixas.Clear();
	                for (int i = 0; i < grupos.Count; i++)
	                {
	                    caixas.Add(new BoundingVolume(dadosTeste[i], "-"+i.ToString()+"-",0));
	                }
	                dummy = TestaPontos(caixas, redes);
	                result.testeCertos = dummy.treinoCertos;
	                result.testeErrados = dummy.treinoErrados;
	            }
				resultado.resultados.Add (result);
			}
			return resultado;
        }		
		
        public static result TestaPontos(List<BoundingVolume> caixas, List<Rede> redes)
        {
            int correto = 0;
            int errado = 0;

            //teste para a caixa "ncaixa"
            for (int nCaixa = 0; nCaixa < caixas.Count; nCaixa++)
            {
                //teste para o ponto "i"
                for (int i = 0; i < caixas[nCaixa].QntdDados; i++)
                {

                    double[] ponto = caixas[nCaixa].Pontos[i].CopyToArray();

                    double max = double.NegativeInfinity;
                    int maxNeuronio = -1;
                    int maxRede = -1;
                    string voto = "";


                    //Acha valor maximo entre os neurônios de saída
                    for (int nRede = 0; nRede < redes.Count; nRede++)
                    {
                        double[] respostas = redes[nRede].CalculaSaída(ponto);
                        //achar o neuronio com maior valor para cada rede
                        for (int nResp = 0; nResp < respostas.Length; nResp++)
                        {
                            if (max < respostas[nResp])
                            {
                                max = respostas[nResp];
                                maxNeuronio = nResp;
                                maxRede = nRede;
                            }
                        }
                    }

                    //decide o voto
                    voto = redes[maxRede].Camadas[1][maxNeuronio].desc;

                    if (voto == caixas[nCaixa].Nome)
                    {
                        correto++;
                    }
                    else
                    {
                        errado++;
                    }
                }
            }

            result resultado = new result();
            resultado.treinoCertos = correto;
            resultado.treinoErrados = errado;
            return resultado;
        }

        public static result TestaPontosVoto(List<BoundingVolume> caixas, List<Rede> redes/*, List<PreRedeNeural> preRede*/)
        {
            int correto = 0;
            int errado = 0;
			
			//teste para a caixa "ncaixa"
            for (int nCaixa = 0; nCaixa < caixas.Count; nCaixa++)
            {
				//teste para o ponto "i"
                for (int i = 0; i < caixas[nCaixa].QntdDados; i++)
                {

                    double[] ponto = caixas[nCaixa].Pontos[i].CopyToArray();
					
					//inicializa o vetor de votos cheio de zeros
					double[] votos = new double[caixas.Count];
					
					//Acha valor maximo entre os neurônios de saída
                    for (int nRede = 0; nRede < redes.Count; nRede++)
                    {
						double max = double.NegativeInfinity;
						int maxIndex =  -1;
						string voto = "";
                        double[] respostas = redes[nRede].CalculaSaída(ponto);
						
						//achar o neuronio com maior valor para cada rede
                        for (int nResp = 0; nResp < respostas.Length; nResp++)
                        {
							if (max <respostas[nResp])
							{
								max = respostas[nResp];
								maxIndex = nResp;
                                
							}
                        }
                        
						//decide o voto
                        voto = redes[nRede].Camadas[1][maxIndex].desc;

						//contabiliza
						for (int dc = 0; dc < caixas.Count; dc++)
        				{
							if (voto == caixas[dc].Nome)
							{
							  	//votos[dc] += max;
								//soma 1 voto, mesmo que pequeno
								votos[dc]++;
								break;
							}
						}
                    }
					
					//correr vetor de votos e achar maior,
					double Nmax = double.NegativeInfinity;
					int NmaxIndex =  -1;
					for (int v = 0; v<caixas.Count ;v++ )
					{
						if (Nmax < votos[v])
						{
							Nmax = votos[v];
							NmaxIndex = v;
						}
					}
                    if (NmaxIndex == nCaixa)
					{
                        correto++;
					}
                    else
					{
                        errado++;
					}
                }
            }

            result resultado = new result();
            resultado.treinoCertos = correto;
            resultado.treinoErrados = errado;
            return resultado;
        }
		

        public static List<Rede> GeraRedesNeurais(List<PreRedeNeural> preRede,Teste nTeste)
        {
            Rede.FAtivação[] fs;
			if (nTeste.Bounds)
			{
			 	fs = new Rede.FAtivação[2] { F1Bounds, F2 };
			}
			else
			{
				fs = new Rede.FAtivação[2] { F1, F2 };	
			}
			List<Rede> r = new List<Rede>();
			
			foreach (PreRedeNeural prn in preRede)
			{
				//juntar padDentro com padFora
				//os primeiros neuronios representam pad dentro
				
				//qntd de entradas = qntd de variaveis
				//qntd de neuronios na primeira camada = qnd de planos
				//qntd de neuronios na segunda camada = qnd de padroes dentr+fora
	            int[] camadas = new int[3] { prn.planos[0].VectorNormal.Length, prn.planos.Count, prn.padDentro.RowCount + prn.padFora.RowCount};
	            Rede dummy = new Rede(camadas, fs);
				
				//preencher a primeira camada
	            for (int neuronio = 0; neuronio < prn.planos.Count; neuronio++)
	            {
	                dummy[0][neuronio].Pesos = (prn.planos[neuronio].VectorNormal.Scale(1/prn.planos[neuronio].d_2)).CopyToArray();
	                dummy[0][neuronio].PesoOffset = prn.planos[neuronio].bias / prn.planos[neuronio].d_2;
	            }
				
				//preenchendo a segunda camada  paddentro
	            double nentradasD = prn.padDentro.ColumnCount;
				
				
	            for (int neuronio = 0; neuronio < prn.padDentro.RowCount; neuronio++)
	            {
	                dummy[1][neuronio].Pesos = (prn.padDentro.GetRowVector(neuronio).Scale( 1/nentradasD)).CopyToArray();
	                dummy[1][neuronio].PesoOffset = 0;
                    dummy[1][neuronio].desc = prn.dentro;
	            }
				
				//padfora
				double nentradasF = prn.padFora.ColumnCount;
				for (int neuronio = 0; neuronio < prn.padFora.RowCount; neuronio++)
	            {
	                dummy[1][neuronio + prn.padDentro.RowCount].Pesos = (prn.padFora.GetRowVector(neuronio).Scale( 1/nentradasF)).CopyToArray();
	                dummy[1][neuronio + prn.padDentro.RowCount].PesoOffset = 0;
                    dummy[1][neuronio + prn.padDentro.RowCount].desc = prn.fora;
	            }
				
				r.Add(dummy);
			}   
            return r;
        }
		
		
		public static double F1Bounds(double v)
        {
                if (v > +1)
                    return +1;
                if (v < -1)
                    return -1;
                return v;
        }
		
        public static double F1(double v)
        {
                if (v >= 0)
                    return +1;
                else
                    return -1;
        }
		
        public static double F2(double v)
        {
            return v;
        }

   
        public static long TempoPadrão()
        {

            //System.Diagnostics.Stopwatch t = new System.Diagnostics.Stopwatch();
            //t.Reset();
            //t.Start();
            //int size = 300;
            ////cria uma Matrix de senos
            //double[,] mat = new double[size, size];
            //for (int i = 0; i < size; i++)
            //{
            //    for (int j = 0; j < size; j++)
            //    {
            //        mat[i, j] = Math.Sin(i * j);
            //    }
            //}

            ////concatena ela com ela mesma
            //double[,] nMat = new double[size, size * 2];
            //for (int i = 0; i < size; i++)
            //{
            //    for (int j = 0; j < size * 2; j++)
            //    {
            //        if (j < size)
            //        {
            //            nMat[i, j] = mat[i, j];
            //        }
            //        else
            //        {
            //            nMat[i, j] = mat[i, j - size];
            //        }
            //    }
            //}

            ////cria um Vector de cosenos
            //double[] vet = new double[size];
            //for (int i = 0; i < size; i++)
            //{
            //    vet[i] = Math.Cos(i);
            //}

            ////redimensiona a Matrix inicial para o dobro do tamanho
            //mat = new double[size, size * 2];

            ////multiplica Vector de cosenos com a Matrix concatenada e salva na antiga.
            //for (int i = 0; i < size; i++)
            //{
            //    for (int j = 0; j < size * 2; j++)
            //    {
            //        double soma = 0;
            //        for (int e = 0; e < size; e++)
            //        {
            //            soma += vet[e] * nMat[e, j];
            //        }
            //        mat[i, j] = soma;
            //    }
            //}
            //t.Stop();
            //return t.ElapsedMilliseconds;
            return 1;
        }
		
		
		#region impressao
		
        public static void imprimeResultado(Resultado result){
         
            List<string> log = new List<string>();

            
            log.AddRange(result.ImprimeTeste().ToArray());
            for (int i = 0; i < log.Count; i++)
			{
            	Console.WriteLine(log[i]);
            }
		}
		
        public static void imprimeMelhorResultado(List<Resultado> result){

            char tab = '\u0009';
            
            List<string> log = new List<string>();

            log.Add("Data:" + tab.ToString() + DateTime.Now.ToUniversalTime());
            log.Add("Bounds:" + tab.ToString() + result[0].teste.Bounds.ToString());
            log.Add("MPD:" + tab.ToString() + result[0].teste.MPD.ToString());
            log.Add("MBC:" + tab.ToString() + result[0].teste.MBC.ToString());
            log.Add("---------");

            int melhor = 0;
            for (int i = 0; i < result.Count; i++)
            {
                //log.AddRange(result[i].ImprimeTeste().ToArray());
                if (result[melhor].testeMedia < result[i].testeMedia)
                {
                    melhor = i;
                }
            }
            log.Add("Melhor Teste");
            log.AddRange(result[melhor].ImprimeTeste().ToArray());



			if (arquivar)
            {
                string arquivo = Application.StartupPath + "/NNCG - ";
                if (result[0].teste.Bounds)
                    arquivo += " Bounds ";
                if (result[0].teste.MBC)
                    arquivo += " MBC ";
                if (result[0].teste.MPD)
                    arquivo += " MPD ";

                arquivo += ".txt";
                System.IO.StreamWriter sw = new System.IO.StreamWriter(arquivo, false);
                for (int i = 0; i < log.Count; i++)
                {
                    sw.WriteLine(log[i]);
                }
                sw.Close();
            }
			else
			{	
				for (int i = 0; i < log.Count; i++)
				{
                    Console.WriteLine(log[i]);
                }
			}
		}
		
		#endregion
		
		#region Data
		
        public static List<Dados> Padrao( )
        {
            List<Dados> grupos = new List<Dados>();

            grupos.Add(new Dados(Numeros.DadosPadraoSeno()));
            grupos.Add(new Dados(Numeros.DadosPadraoCaixa()));

			return grupos;
        }
        public static List<Dados> Vinho( )
        {
            List<Dados> grupos = new List<Dados>();

            grupos.Add(new Dados(Numeros.DadosVinho1()));
            grupos.Add(new Dados(Numeros.DadosVinho2()));
            grupos.Add(new Dados(Numeros.DadosVinho3()));

            return grupos;
        }
        public static List<Dados> Flor( )
        {
            List<Dados> grupos = new List<Dados>();

            grupos.Add(new Dados(Numeros.DadosIrisSetosa()));
            grupos.Add(new Dados(Numeros.DadosIrisVersicolor()));
            grupos.Add(new Dados(Numeros.DadosIrisVirginica()));

            return grupos;
        }
        public static List<Dados> Vidro( )
        {
            List<Dados> grupos = new List<Dados>();
            grupos.Add(new Dados(Numeros.DadosVidro1()));
            grupos.Add(new Dados(Numeros.DadosVidro2()));
            grupos.Add(new Dados(Numeros.DadosVidro3()));
            grupos.Add(new Dados(Numeros.DadosVidro5()));
            grupos.Add(new Dados(Numeros.DadosVidro6()));
            grupos.Add(new Dados(Numeros.DadosVidro7()));

            return grupos;
        }
        public static List<Dados> Vogal()
        {
            List<Dados> grupos = new List<Dados>();
            grupos.Add(new Dados(Numeros.DadosVogal(0)));
            grupos.Add(new Dados(Numeros.DadosVogal(1)));
            grupos.Add(new Dados(Numeros.DadosVogal(2)));
            grupos.Add(new Dados(Numeros.DadosVogal(3)));
            grupos.Add(new Dados(Numeros.DadosVogal(4)));
            grupos.Add(new Dados(Numeros.DadosVogal(5)));
            grupos.Add(new Dados(Numeros.DadosVogal(6)));
            grupos.Add(new Dados(Numeros.DadosVogal(7)));
            grupos.Add(new Dados(Numeros.DadosVogal(8)));
            grupos.Add(new Dados(Numeros.DadosVogal(9)));


           return grupos;
        }
        public static List<Dados> Segmentacao()
        {
            List<Dados> grupos = new List<Dados>();
            grupos.Add(new Dados(Numeros.DadosSeg1()));
            grupos.Add(new Dados(Numeros.DadosSeg2()));
            grupos.Add(new Dados(Numeros.DadosSeg3()));
            grupos.Add(new Dados(Numeros.DadosSeg4()));
            grupos.Add(new Dados(Numeros.DadosSeg5()));
            grupos.Add(new Dados(Numeros.DadosSeg6()));
            grupos.Add(new Dados(Numeros.DadosSeg7()));


            return grupos;
        }
        public static List<Dados> Carro( )
        {
            List<Dados> grupos = new List<Dados>();
            grupos.Add(new Dados(Numeros.DadosCarro1()));
            grupos.Add(new Dados(Numeros.DadosCarro2()));
            grupos.Add(new Dados(Numeros.DadosCarro3()));
            grupos.Add(new Dados(Numeros.DadosCarro4()));


            return grupos;
        }
        public static List<Dados> Diabetes( )
        {
            List<Dados> grupos = new List<Dados>();
            grupos.Add(new Dados(Numeros.DadosDiabetes0()));
            grupos.Add(new Dados(Numeros.DadosDiabetes1()));

            return grupos;
        }
		
		#endregion


    }

}