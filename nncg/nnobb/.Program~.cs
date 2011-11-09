using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using System.Threading;
using NNOBB.NN;
using NNOBB.OBB;


namespace NNOBB
{
    static class Program
    {
		
		public static bool log = false;
		public static bool thread = false;
		
		
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			NNOBB.OBB.OBB.Bounds = false;
			NNOBB.OBB.OBB.MBC = true;
			NNOBB.OBB.OBB.MPD  = false;
			NNOBB.OBB.OBB.TODOS  = false;
			TesteFuncão(Flor, 10, 10, 0, 100);
			
			//Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }
		
		
		
		
//        public void TestaPadrao()
//        {
//            TesteFuncão(Padrao, nbFolds, nbProf, nbSeed);
//        }
		
        public delegate Resultado FuncaoFold(int K, int P, int prof, int seed);

        public static void TesteFuncão(FuncaoFold f, int folds, int prof, int seed_ini, int repetições)
        {
            char tab = '\u0009';

            double mediaTreino = 0;
            double dpTreino = 0;
            double maximoTreino = double.NegativeInfinity;
            double minimoTreino = double.PositiveInfinity;

            double mediaTeste = 0;
            double dpTeste = 0;
            double maximoTeste = double.NegativeInfinity;
            double minimoTeste = double.PositiveInfinity;

            double MmediaTreino = 0;
            double MdpTreino = 0;
            double MmaximoTreino = double.NegativeInfinity;
            double MminimoTreino = double.PositiveInfinity;

            double MmediaTeste = 0;
            double MdpTeste = 0;
            double MmaximoTeste = double.NegativeInfinity;
            double MminimoTeste = double.PositiveInfinity;
			
			double tmedTot = 0;
			int indic = 0;


            List<string> bestResult = new List<string>();
            List<string> valores = new List<string>();

            double tPadrao = TempoPadrão();

            System.Diagnostics.Stopwatch t = new System.Diagnostics.Stopwatch();

            int megaseed = seed_ini;
            for (int rep = 0; rep < repetições; rep++) {
				
                bestResult.Clear();

                mediaTeste = 0;
                mediaTreino = 0;
                minimoTeste = 1;

                t = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < folds; i++)
                {
                    Resultado dummy = f.Invoke(folds, i, prof, seed_ini);
                    double val = (dummy.treinoCertos / (double)(dummy.treinoCertos + dummy.treinoErrados));
                    mediaTreino += val;
                    dpTreino += Math.Pow(val, 2);
                    maximoTreino = Math.Max(val, maximoTreino);
                    minimoTreino = Math.Min(val, minimoTreino);

                    val = (dummy.testeCertos / (double)(dummy.testeCertos + dummy.testeErrados));
                    mediaTeste += val;
                    dpTeste += Math.Pow(val, 2);
                    maximoTeste = Math.Max(val, maximoTeste);
                    minimoTeste = Math.Min(val, minimoTeste);
                    bestResult.Add(dummy.ToString());
                }
				
                t.Stop();
               

                mediaTreino = mediaTreino / folds;
                dpTreino = Math.Sqrt((dpTreino / folds) - Math.Pow(mediaTreino, 2));

                mediaTeste = mediaTeste / folds;
                dpTeste = Math.Sqrt((dpTeste / folds) - Math.Pow(mediaTeste, 2));

                if (mediaTreino >= MmediaTreino)
                {
				  if (log)
                    {
                        string arquivo = Application.StartupPath + "/OBBNN - " + f.Method.Name + " - " + folds + "fold, prof " + prof;
                        if (OBB.OBB.Bounds)
                            arquivo += " Bounds ";
                        if (OBB.OBB.MBC)
                            arquivo += " MBC ";
                        if (OBB.OBB.MPD)
                            arquivo += " MPD ";

                        arquivo += " log.txt";
                        System.IO.StreamWriter sw = new System.IO.StreamWriter(arquivo, true);
                        sw.WriteLine("Seed " + seed_ini.ToString());
                        sw.Close();
                    }
                    if ((mediaTeste > MmediaTeste) || ((mediaTeste == MmediaTeste) && (minimoTeste > MminimoTeste)))
                    {
						
                        MmediaTreino = mediaTreino;
                        MdpTreino = dpTreino;
                        MmaximoTreino = maximoTreino;
                        MminimoTreino = minimoTreino;

                        MmediaTeste = mediaTeste;
                        MdpTeste = dpTeste;
                        MmaximoTeste = maximoTeste;
                        MminimoTeste = minimoTeste;
						
						if (log)
                        {
                            string arquivo = Application.StartupPath + "/OBBNN - " + f.Method.Name + " - " + folds + "fold, prof " + prof;
                            if (OBB.OBB.Bounds)
                                arquivo += " Bounds ";
                            if (OBB.OBB.MBC)
                                arquivo += " MBC ";
                            if (OBB.OBB.MPD)
                                arquivo += " MPD ";

                            arquivo += " log.txt";
                            System.IO.StreamWriter sw = new System.IO.StreamWriter(arquivo, true);

                            for (int i = 0; i < bestResult.Count; i++)
                            {
                                sw.WriteLine(bestResult[i]);
                            }
                            sw.WriteLine("-> Pontos usados para treinamento");
                            sw.WriteLine("Maximo:" + tab.ToString() + MmaximoTreino.ToString());
                            sw.WriteLine("Media:" + tab.ToString() + MmediaTreino.ToString());
                            sw.WriteLine("Minimo:" + tab.ToString() + MminimoTreino.ToString());
                            sw.WriteLine("-> Pontos usados para testes");
                            sw.WriteLine("Maximo:" + tab.ToString() + MmaximoTeste.ToString());
                            sw.WriteLine("Media:" + tab.ToString() + MmediaTeste.ToString());
                            sw.WriteLine("Minimo:" + tab.ToString() + MminimoTeste.ToString());
                            sw.WriteLine(" ");
            

                            sw.Close();
                        }
                        megaseed = seed_ini;
                        valores.Clear();
                        valores.AddRange(bestResult.ToArray());


                      
                    }
					
					
                }
					Console.Clear();

				tmedTot +=mediaTeste;
				indic++;
				//Console.WriteLine(tmedTot/indic);
				Console.WriteLine("Tempo:" + tab.ToString() + t.ElapsedMilliseconds);
                Console.WriteLine("------------------------------------");
                Console.WriteLine("Seed Atual:" + tab.ToString() + seed_ini.ToString());
                Console.WriteLine("Media teste:" + tab.ToString() + mediaTeste.ToString());
                Console.WriteLine("Min teste:" + tab.ToString() + minimoTeste.ToString());
                Console.WriteLine("------------------------------------");
                Console.WriteLine("Melhor Seed:" + tab.ToString() + megaseed.ToString());
                Console.WriteLine("Media teste:" + tab.ToString() + MmediaTeste.ToString());
                Console.WriteLine("Min teste:" + tab.ToString() + MminimoTeste.ToString());

                seed_ini++;
            }while (1!=1);
			
            bestResult.Clear();
            bestResult.Add("Data:" + tab.ToString() + DateTime.Now.ToUniversalTime());
            bestResult.Add("Caso:" + tab.ToString() + f.Method.Name);
            bestResult.Add("ProfMax:" + tab.ToString() + prof.ToString());
            bestResult.Add("Fold:" + tab.ToString() + folds.ToString());
            bestResult.Add("Seed:" + tab.ToString() + megaseed.ToString());
            bestResult.Add("Bounds:" + tab.ToString() + OBB.OBB.Bounds.ToString());
            bestResult.Add("MPD:" + tab.ToString() + OBB.OBB.MPD .ToString());
            bestResult.Add("MBC:" + tab.ToString() + OBB.OBB.MBC.ToString());
            bestResult.Add("Trein_C" + tab.ToString() + "Trein_E" + tab.ToString() + "Teste_C" + tab.ToString() + "Teste_E" + tab.ToString() + "Profund" + tab.ToString() + "Planos" + tab.ToString() + "Padroes");
            bestResult.AddRange(valores.ToArray());

            bestResult.Add("-> Pontos usados para treinamento");
            bestResult.Add("Maximo:" + tab.ToString() + MmaximoTreino.ToString());
            bestResult.Add("Media:" + tab.ToString() + MmediaTreino.ToString());
            bestResult.Add("DP:" + tab.ToString() + MdpTreino.ToString());
            bestResult.Add("Minimo:" + tab.ToString() + MminimoTreino.ToString());
            bestResult.Add("-> Pontos usados para testes");
            bestResult.Add("Maximo:" + tab.ToString() + MmaximoTeste.ToString());
            bestResult.Add("Media:" + tab.ToString() + MmediaTeste.ToString());
            bestResult.Add("DP:" + tab.ToString() + MdpTeste.ToString());
            bestResult.Add("Minimo:" + tab.ToString() + MminimoTeste.ToString());
            bestResult.Add(" ");
            bestResult.Add("Tempo Execução(mS):" + tab.ToString() + t.ElapsedMilliseconds.ToString());
            bestResult.Add("Tempo Execução(PU):" + tab.ToString() + (t.ElapsedMilliseconds / tPadrao).ToString());

			if (log)
            {
                string arquivo = Application.StartupPath + "/OBBNN - " + f.Method.Name + " - " + folds + "fold, prof " + prof;
                if (OBB.OBB.Bounds)
                    arquivo += " Bounds ";
                if (OBB.OBB.MBC)
                    arquivo += " MBC ";
                if (OBB.OBB.MPD)
                    arquivo += " MPD ";

                arquivo += ".txt";
                System.IO.StreamWriter sw = new System.IO.StreamWriter(arquivo, false);
                for (int i = 0; i < bestResult.Count; i++)
                {
                    sw.WriteLine(bestResult[i]);
                }
                sw.Close();
            }
			else
			{	
				Console.Clear();
				for (int i = 0; i < bestResult.Count; i++)
				{
                    Console.WriteLine(bestResult[i]);
                }
			}
		}
				
		
//        public Resultado Padrao(int K, int p, int prof, int seed)
//        {
//            List<Dados> grupos = new List<Dados>();
//
//            grupos.Add(DadosPadraoSeno(seed));
//            grupos.Add(DadosPadraoCaixa(seed));
//
//            return ExecutaOBBNN(grupos, K, p, prof);
//        }
        public static Resultado Vinho(int K, int p, int prof, int seed)
        {
            List<Dados> grupos = new List<Dados>();

            grupos.Add(Numeros.DadosVinho1(seed));
            grupos.Add(Numeros.DadosVinho2(seed));
            grupos.Add(Numeros.DadosVinho3(seed));

            return ExecutaOBBNN(grupos, K, p, prof);
        }
        public static Resultado Flor(int K, int p, int prof, int seed)
        {
            List<Dados> grupos = new List<Dados>();

            grupos.Add(Numeros.DadosIrisSetosa(seed));
            grupos.Add(Numeros.DadosIrisVersicolor(seed));
            grupos.Add(Numeros.DadosIrisVirginica(seed));

            return ExecutaOBBNN(grupos, K, p, prof);
        }
        public static Resultado Vidro(int K, int p, int prof, int seed)
        {
            List<Dados> grupos = new List<Dados>();
            grupos.Add(Numeros.DadosVidro1(seed));
            grupos.Add(Numeros.DadosVidro2(seed));
            grupos.Add(Numeros.DadosVidro3(seed));
            grupos.Add(Numeros.DadosVidro5(seed));
            grupos.Add(Numeros.DadosVidro6(seed));
            grupos.Add(Numeros.DadosVidro7(seed));

            return ExecutaOBBNN(grupos, K, p, prof);
        }
        public static Resultado Vogal(int K, int p, int prof, int seed)
        {
            List<Dados> grupos = new List<Dados>();
            grupos.Add(Numeros.DadosVogal(0, seed));
            grupos.Add(Numeros.DadosVogal(1, seed));
            grupos.Add(Numeros.DadosVogal(2, seed));
            grupos.Add(Numeros.DadosVogal(3, seed));
            grupos.Add(Numeros.DadosVogal(4, seed));
            grupos.Add(Numeros.DadosVogal(5, seed));
            grupos.Add(Numeros.DadosVogal(6, seed));
            grupos.Add(Numeros.DadosVogal(7, seed));
            grupos.Add(Numeros.DadosVogal(8, seed));
            grupos.Add(Numeros.DadosVogal(9, seed));


            return ExecutaOBBNN(grupos, K, p, prof);
        }
        public static Resultado Segmentacao(int K, int p, int prof, int seed)
        {
            List<Dados> grupos = new List<Dados>();
            grupos.Add(Numeros.DadosSeg1(seed));
            grupos.Add(Numeros.DadosSeg2(seed));
            grupos.Add(Numeros.DadosSeg3(seed));
            grupos.Add(Numeros.DadosSeg4(seed));
            grupos.Add(Numeros.DadosSeg5(seed));
            grupos.Add(Numeros.DadosSeg6(seed));
            grupos.Add(Numeros.DadosSeg7(seed));


            return ExecutaOBBNN(grupos, K, p, prof);
        }
        public static Resultado Carro(int K, int p, int prof, int seed)
        {
            List<Dados> grupos = new List<Dados>();
            grupos.Add(Numeros.DadosCarro1(seed));
            grupos.Add(Numeros.DadosCarro2(seed));
            grupos.Add(Numeros.DadosCarro3(seed));
            grupos.Add(Numeros.DadosCarro4(seed));


            return ExecutaOBBNN(grupos, K, p, prof);
        }
        public static Resultado Diabetes(int K, int p, int prof, int seed)
        {
            List<Dados> grupos = new List<Dados>();
            grupos.Add(Numeros.DadosDiabetes0(seed));
            grupos.Add(Numeros.DadosDiabetes1(seed));

            return ExecutaOBBNN(grupos, K, p, prof);
        }


        public static Resultado ExecutaOBBNN(List<Dados> grupos, int K, int n, int prof)
        {

            List<List<Vector>> dadosTreino = new List<List<Vector>>();
            List<List<Vector>> dadosTeste = new List<List<Vector>>();


            for (int i = 0; i < grupos.Count; i++)
            {
                dadosTreino.Add(grupos[i].GetKFoldTreino(K, n));
                dadosTeste.Add(grupos[i].GetKFoldTeste(K, n));
            }


            List<Caixa> caixas = new List<Caixa>();

            for (int i = 0; i < grupos.Count; i++)
            {
                caixas.Add(new Caixa(dadosTreino[i], "-" + i.ToString() + "-"));
            }

            Resultado result = new Resultado();
			List<PreRedeNeural> preRede;

            OBB.OBB.DetectaColisão(ref caixas, out preRede, prof);
			
            foreach (Caixa c in caixas)
            {
                result.profundidadeMaxima = Math.Max(result.profundidadeMaxima, c.Profundidade());
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
			
            List<Rede> redes = GeraRedesNeurais(preRede);
			
            Resultado dummy = TestaPontos(caixas, redes, preRede);
            result.treinoCertos = dummy.treinoCertos;
            result.treinoErrados = dummy.treinoErrados;
			
            if (dadosTeste.Count == dadosTreino.Count)
            {
                caixas.Clear();
                for (int i = 0; i < grupos.Count; i++)
                {
                    caixas.Add(new Caixa(dadosTeste[i], "-"+i.ToString()+"-"));
                }
                dummy = TestaPontos(caixas, redes, preRede);
                result.testeCertos = dummy.treinoCertos;
                result.testeErrados = dummy.treinoErrados;
            }


            return result;
        }

        public static Resultado TestaPontos(List<Caixa> caixas, List<Rede> redes, List<PreRedeNeural> preRede)
        {
            int correto = 0;
            int errado = 0;
			
			//teste para a caixa "ncaixa"
            for (int nCaixa = 0; nCaixa < caixas.Count; nCaixa++)
            {
				//teste para o ponto "i"
                for (int i = 0; i < caixas[nCaixa].Qntd_Dados; i++)
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
						if (maxIndex < preRede[nRede].padDentro.RowCount)
						{
							voto = preRede[nRede].dentro;
						}
						else
						{
							voto = preRede[nRede].fora;
						}
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

            Resultado resultado = new Resultado();
            resultado.treinoCertos = correto;
            resultado.treinoErrados = errado;
            return resultado;
        }
		

        public static List<Rede> GeraRedesNeurais(List<PreRedeNeural> preRede)
        {
            Rede.FAtivação[] fs = new Rede.FAtivação[2] { F1, F2 };
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
	            }
				
				//padfora
				double nentradasF = prn.padFora.ColumnCount;
				for (int neuronio = 0; neuronio < prn.padFora.RowCount; neuronio++)
	            {
	                dummy[1][neuronio + prn.padDentro.RowCount].Pesos = (prn.padFora.GetRowVector(neuronio).Scale( 1/nentradasF)).CopyToArray();
	                dummy[1][neuronio + prn.padDentro.RowCount].PesoOffset = 0;
	            }
				
				r.Add(dummy);
			}   
            return r;
        }
		
        public static double F1(double v)
        {
            if (OBB.OBB.Bounds)
            {
                if (v > +1)
                    return +1;
                if (v < -1)
                    return -1;
                return v;
            }
            else
            {
                if (v >= 0)
                    return +1;
                else
                    return -1;
            }
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

    }

}