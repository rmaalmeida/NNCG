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
            t.seed = 1;
            t.folds = 10;
            t.profundidade = 20;
            t.desc = "generico";
			
			t.Bounds = true;
			t.MBC = true;
			t.MPD  = false;
			t.TODOS  = false;
			
			t.AaA = true;
			t.OaA = !t.AaA;
			t.Voto = true;
			
			t.BV = 0;
			
			List<Dados> data;
			
			data  = Flor ();
			
			
            imprimeMelhorResultado(BateriaDeTestes(data, t, 10));
			Console.WriteLine(tempo.ElapsedMilliseconds.ToString ());
        }
		
		
        public static List<Resultado> BateriaDeTestes(List<Dados> nDados, Teste t, int repetições)
        {
            List<Resultado> resultFinal = new List<Resultado>();
            
			while(repetições > 0)
			{
				foreach(Dados d in nDados){
					d.ReSeedDados(t.seed, t.folds);	
				}
	
				
				//realiza o teste para cada 'fold'
                resultFinal.Add(ExecutaTeste(nDados,t));

				t.seed++;
				repetições--;
            }
            return resultFinal;
        }
		
					
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
					if (nTeste.BV == 2){
	                	caixas.Add(new OBB(dadosTreino[i], "-" + i.ToString() + "-",0));
					}
					if (nTeste.BV == 1){
	                	//caixas.Add(new AABB(dadosTreino[i], "-" + i.ToString() + "-",0));
					}
					if (nTeste.BV == 0){
	                	caixas.Add(new Sphere(dadosTreino[i], "-" + i.ToString() + "-",0));
					}
					
	            }
	
	    				
				TesteColisao teste = new TesteColisao(nTeste);
	
	            teste.RealizaTeste(caixas);
				
				result result = new result();
	            foreach (BoundingVolume c in caixas)
	            {
	                result.profundidadeMaxima = Math.Max(result.profundidadeMaxima, c.MaxProfundidade());
	            }
	            
				foreach (PreRedeNeural p in teste.PRN)
	            {
	                result.planos += p.planos.Count;
	                result.padroes += p.padDentro.RowCount;
					result.padroes += p.padFora.RowCount;
	            }
				
	            List<Rede> redes = GeraRedesNeurais(teste.PRN, nTeste);
				
	            TestaPontos(caixas, redes, nTeste, out result.treinoCertos, out result.treinoErrados);
	            
				
	            if (dadosTeste.Count == dadosTreino.Count)
	            {
	                caixas.Clear();
	                for (int i = 0; i < grupos.Count; i++)
	                {
	                    caixas.Add(new BoundingVolume(dadosTeste[i], "-"+i.ToString()+"-",0));
	                }
	                TestaPontos(caixas, redes,nTeste, out result.testeCertos, out result.testeErrados);
	            }
				resultado.resultados.Add (result);
			}
			return resultado;
        }		
		
        public static void TestaPontos(List<BoundingVolume> caixas, List<Rede> redes, Teste tipo, out int correto, out int errado)
        {
			correto=0;
			errado=0;
			if (!tipo.Voto)
			{
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
			}
			else
			{
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
							//as vezes nao acha o index por problemas de contas nos neurônios
	                        if (maxIndex == -1)
							{
								voto = "";
							}
							else
							{
	                        	voto = redes[nRede].Camadas[1][maxIndex].desc;
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
						
						//correr vetor de votos e achar maior
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
			}
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
				r.Add(prn.GerarRedeNeural(fs));
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
			log.Add("AaA:" + tab.ToString() + result[0].teste.AaA.ToString());
			log.Add("OaA:" + tab.ToString() + result[0].teste.OaA.ToString());
			log.Add("BV:" + tab.ToString() + result[0].teste.BV.ToString());
			log.Add("Folds:" + tab.ToString() + result[0].teste.folds.ToString());
			log.Add("Voto:" + tab.ToString() + result[0].teste.Voto.ToString());
            log.Add("---------");

            int melhor = 0;
            for (int i = 0; i < result.Count; i++)
            {
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