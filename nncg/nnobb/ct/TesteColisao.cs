using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace CTL.CT
{
    public static class TesteColisao
    {
        public static bool MPD = false;
		public static bool TODOS = false; //adicionar todos os planos encontrados?
        public static bool MBC = false;
        public static bool Bounds = false;
		public static int qtThreads = 1;
        public static List<System.Threading.Thread> threads;


        public static void RealizaTeste(ref List<BoundingVolume> boundingVolumes, out List<PreRedeNeural> preRede, int prof)
        {
            preRede = new List<PreRedeNeural>();
            List<Plano> dummyplanos = new List<Plano>();
            for (int i = 0; i < boundingVolumes.Count; i++)
            {
                BoundingVolume c1 = boundingVolumes[i];
                BoundingVolume c2;
                PreRedeNeural p = new PreRedeNeural();
                List<Vector> pontos = new List<Vector>();
                string nome2 = "";
                for (int j = 0; j < boundingVolumes.Count; j++)
                {
                    if (i != j)
                    {
                        pontos.AddRange(boundingVolumes[j].Pontos.ToArray());
                        nome2+= boundingVolumes[j].Nome;
                    }
                }
                c2 = new OBB(pontos, nome2, boundingVolumes[i].nivel);

                //encontra os planos
                testeColisao(ref p.planos, c1, c2, prof);

                //seleciona os planos
                dummyplanos = SelecionaPlanos(c1, c2, p.planos, out p.padDentro, out p.padFora);

                //atualiza os dados
                p.dentro = c1.Nome;
                p.fora = c2.Nome;
                p.planos = dummyplanos;
                preRede.Add(p);
            }
        }
		
        public static void RealizaTesteAaA(ref List<BoundingVolume> boundingVolumes, out List<PreRedeNeural> preRede, int prof)
        {
			preRede = new List<PreRedeNeural>();			
			List<Plano> dummyplanos = new List<Plano>();
            for (int i = 0; i < boundingVolumes.Count; i++)
            {
				BoundingVolume c1 = boundingVolumes[i];
                for (int j=i+1;j<boundingVolumes.Count  ;j++ ) 
				{
                    PreRedeNeural p = new PreRedeNeural();
					
					//encontra os planos
					
					BoundingVolume c2 = boundingVolumes[j];
	                testeColisao(ref p.planos,  c1, c2, prof);
					
					//seleciona os planos
	                dummyplanos = SelecionaPlanos(c1, c2, p.planos, out p.padDentro, out p.padFora);

					//atualiza os dados
					p.dentro = c1.Nome;
					p.fora = c2.Nome;
					p.planos = dummyplanos;
	                preRede.Add(p);
				}
            }
        }

        private static void testeColisao(ref List<Plano> Planos, BoundingVolume C1, BoundingVolume C2, int prof)
        {
            //    % Testa se duas caixas colidem, em caso afirmativo as divide até não haver
            //    % colisão ou não ser possivel mais dividir

            int qntd = C1.ProcuraPlano(ref Planos, C2);

            //    % Verifica se existe algum plano de separação, não encontrando nenhum plano
            //    % divide as caixas
            if (qntd == 0)
            {
                if ((C1.QntdDados > 1) && (C1.Profundidade() < prof))
                {
					C1.CriaFilhos();
                    //        % Caso ja existam os nós filhos
                    if (C1.volAcima != null)
                    {
                        testeColisao(ref Planos,  C2,  C1.volAcima, prof);
                    }
                    if (C1.volAbaixo != null)
                    {
                        testeColisao(ref Planos,  C2, C1.volAbaixo, prof);
                    }
                }
                else if ((C2.QntdDados > 1) && (C2.Profundidade() < prof))
                {
					C2.CriaFilhos();
                    if (C2.volAcima != null)
                    {
                        /* qntd +=*/
                        testeColisao(ref Planos,  C1, C2.volAcima, prof);
                    }
                    if (C2.volAbaixo != null)
                    {
                        /* qntd +=*/
                        testeColisao(ref Planos, C1,  C2.volAbaixo, prof);
                    }
                }
            }
            //return qntd;
        }

        private static List<Plano> SelecionaPlanos(BoundingVolume Caixa1, BoundingVolume Caixa2, List<Plano> planos, out Matrix p1, out Matrix p2)
        {
			
			if (!TODOS)
			{
	            ElencaPlanos(Caixa1, Caixa2, ref planos);
	
	   	
				List<Vector> pad1 = new List<Vector>();
				List<Vector> pad2 = new List<Vector>();
	
				// Guarda o numero dos planos escolhidos
	            List<int> PlanSelec = new List<int>();
	
	            // Guarda o numero dos planos NÃO escolhidos
	            List<int> PlanNSelec = new List<int>(planos.Count);
				
	            for (int i = 0; i < planos.Count; i++)
	            {
	                PlanNSelec.Add(i);
	            }
	
				
	
	            int ini1 = 0;
	            int ini2 = 0;
	            bool separado = false;
	
				
	            while (!separado)
	            {
                    //retorna o número do plano, que é o índice dele na variavel "planos"
	                int Indice = ProcuraIndiceSepara(ref ini1, ref  ini2, planos, Caixa1, Caixa2, PlanNSelec);
	                //se nao encontrou incrementa ini2, pula esse ponto
	                if (Indice == -1)
	                {
	                    ini2++;
	                }
	                else
	                {
						
	                    PlanSelec.Add(Indice);
	                    PlanNSelec.Remove(Indice);
	
	                    Vector vdummy1 = new Vector(Caixa1.QntdDados);
	                    for (int i = 0; i < Caixa1.QntdDados; i++)
	                    {
	                        vdummy1._data[i] = VerificaPonto(Caixa1.Pontos[i], planos[Indice]);
	                    }
						
						pad1.Add(vdummy1);
	
	
	                    Vector vdummy2 = new Vector(Caixa2.QntdDados);
	                    for (int i = 0; i < Caixa2.QntdDados; i++)
	                    {
	                        vdummy2._data[i] = VerificaPonto(Caixa2.Pontos[i], planos[Indice]);
	                    }
						pad2.Add(vdummy2);
	
	                }
	                if (pad1.Count != 0)
	                {
	                    separado = VerificaSeparacao(pad1, pad2, ref ini1, ref ini2);
	                }
	
	            }
	
	
	
	            List<Plano> novosPlanos = new List<Plano>();
	            foreach (int i in PlanSelec)
	            {
	                novosPlanos.Add(planos[i]);
	            }
		
	            if (Bounds)
	            {
	                AjustaDistancias(ref pad1, ref pad2, ref novosPlanos);
	            }
						
				//para deixar binário, -1 ou 1;
	            AjustaPadrões(ref pad1, ref pad2);
	
	            // pega os padrões de verdade
				p1 = FindUnique(pad1);
				p2 = FindUnique(pad2);
	
	
	            return novosPlanos;
			}
			else
			{
	   	
				List<Vector> pad1 = new List<Vector>();
				List<Vector> pad2 = new List<Vector>();
	
				// Guarda o numero dos planos escolhidos
	            List<int> PlanSelec = new List<int>();
				Vector vdummy1 = new Vector(Caixa1.QntdDados);
				Vector vdummy2 = new Vector(Caixa2.QntdDados);
			
	            for (int Indice = 0; Indice < planos.Count; Indice++)
	            {
	                PlanSelec.Add(Indice);
					for (int i = 0; i < Caixa1.QntdDados; i++)
	                {
	                	vdummy1._data[i] = VerificaPonto(Caixa1.Pontos[i], planos[Indice]);
	                }

					pad1.Add(vdummy1.Clone());
	                for (int i = 0; i < Caixa2.QntdDados; i++)
	                {
	                	vdummy2._data[i] = VerificaPonto(Caixa2.Pontos[i], planos[Indice]);
	                }
					pad2.Add(vdummy2.Clone());
                }
				
				List<Plano> novosPlanos = new List<Plano>();
				foreach (int i in PlanSelec)
	            {
	               	novosPlanos.Add(planos[i]);
	            }
		
	            if (Bounds)
	            {
	                AjustaDistancias(ref pad1, ref pad2, ref novosPlanos);
	            }
						
				//para deixar binário, -1 ou 1;
	            AjustaPadrões(ref pad1, ref pad2);
	
	            // pega os padrões de verdade
				p1 = FindUnique(pad1);
				p2 = FindUnique(pad2);
				
	            return novosPlanos;
			}
        }
		
		private static Matrix FindUnique(List<Vector> lv)
		{
			int _rowCount = lv[0].Length;
			int _columnCount =  lv.Count;
			
            Matrix dummy = new Matrix(_rowCount, _columnCount);
			
            int target = 0;

            for (int orgLine = 0; orgLine < _rowCount; orgLine++)
            {
                bool findEqualLine = false;
                for (int resLine = 0; resLine < target; resLine++)
                {
                    for (int col = 0; col < _columnCount; col++)
                    {
                        if (lv[ col]._data[orgLine] != dummy._data[resLine][col])
                        {
                            break;
                        }
						else if (col == _columnCount -1)
						{
							findEqualLine = true;
						}
                    }
					if (findEqualLine)
					{
						break;
					}
                }
                if (!findEqualLine)
                {
                    for (int i = 0; i < _columnCount; i++)
                    {
                        dummy._data[target][i] = lv[i]._data[orgLine];
                    }
                    target++;
                }
            }

            Matrix resut = new Matrix(target,_columnCount);
            for (int i = 0; i < target; i++)
            {
                for (int j = 0; j < _columnCount ; j++)
                {
                    resut._data[i][j] = dummy._data[i][j];
                }
            }
            return resut;
        }	

        private static void AjustaPadrões(ref  List<Vector>  p1, ref  List<Vector>  p2)
        {
            for (int c = 0; c < p1.Count; c++)
            {
                for (int l1 = 0; l1 < p1[0].Length; l1++)
                {
                    if (p1[c]._data[l1] >= 0)
                    {
                        p1[c]._data[l1] = 1;
                    }
                    else
                    {
                        p1[c]._data[l1] = -1;
                    }
                }
                for (int l2 = 0; l2 < p2[0].Length; l2++)
                {
                    if (p2[c]._data[l2] >= 0)
                    {
                        p2[c]._data[l2] = 1;
                    }
                    else
                    {
                        p2[c]._data[l2] = -1;
                    }
                }
            }
        }

        private static void AjustaDistancias(ref List<Vector> p1, ref  List<Vector>  p2, ref List<Plano> planos)
        {
            double dmin = double.MaxValue;
            for (int c = 0; c < p1.Count; c++)
            {
                for (int l1 = 0; l1 < p1[0].Length; l1++)
                {
                    dmin = Math.Min(dmin, Math.Abs(p1[c]._data[l1]));
                }
                for (int l2 = 0; l2 < p2[0].Length; l2++)
                {
                    dmin = Math.Min(dmin, Math.Abs(p2[c]._data[l2]));
                }
                if (dmin > 0)
                {
                    planos[c].d_2 *= dmin;
                }
                else
                {
                    planos[c].d_2 = double.Epsilon;
                }

            }
        }

        private static void ElencaPlanos(BoundingVolume Caixa1, BoundingVolume Caixa2, ref List<Plano> planos)
        {

            if (MBC)
            {
                for (int np = 0; np < planos.Count; np++)
                {
                    double Qntd1P = 0;
                    double Qntd1N = 0;
                    double Qntd2P = 0;
                    double Qntd2N = 0;

                    for (int i = 0; i < Caixa1.QntdDados; i++)
                    {
                        if (VerificaPonto(Caixa1.Pontos[i], planos[np]) > 0)
                        {
                            Qntd1P++;
                        }
                        else
                        {
                            Qntd1N++;
                        }
                    }
                    for (int i = 0; i < Caixa2.QntdDados; i++)
                    {
                        if (VerificaPonto(Caixa2.Pontos[i], planos[np]) > 0)
                        {
                            Qntd2P++;
                        }
                        else
                        {
                            Qntd2N++;
                        }
                    }

                    Qntd1P /= Caixa1.QntdDados;
                    Qntd1N /= Caixa1.QntdDados;
                    double s1;
                    if (Qntd1P > Qntd1N)
                        s1 = Qntd1P;
                    else
                        s1 = -Qntd1N;

                    Qntd2P /= Caixa2.QntdDados;
                    Qntd2N /= Caixa2.QntdDados;
                    double s2;
                    if (Qntd2P > Qntd2N)
                        s2 = Qntd2P;
                    else
                        s2 = -Qntd2N;

                    //if (Math.Sign(s1) == Math.Sign(s2))
                    //    planos[np].significancia = -s1 * s2;
                    //else
                    //    planos[np].significancia = -s1 * s2;

                    if (MPD)
                    {
                        planos[np].significancia = -s1 * s2 * planos[np].d_2;

                    }
                    else
                    {
                        planos[np].significancia = -s1 * s2;
                    }
                }

                //int total = Caixa1.Qntd_Dados + Caixa2.Qntd_Dados;
                //for (int np = 0; np < planos.Count; np++)
                //{
                //    planos[np].significancia /= total;
                //}
                planos.Sort();
            }
            else
            {
                if (MPD)
                {
                    for (int np = 0; np < planos.Count; np++)
                    {
                        planos[np].significancia = planos[np].d_2;
                    }
                    planos.Sort();
                }
            }
        }

        private static int ProcuraIndiceSepara(ref int l1, ref int l2, List<Plano> planos, BoundingVolume Caixa1, BoundingVolume Caixa2, List<int> PlanNSelec)
        {

            //adjusting the pointers
            if (l2 >= Caixa2.QntdDados)
            {
                l2 = 0;
                l1++;
            }

            int best = -1;
            double bestDist = 0;

            //for (int c = 0; c < planos.Count; c++)
            foreach (int c in PlanNSelec)
            {
                double p1 = VerificaPonto(Caixa1.Pontos[l1], planos[c]);

                double p2 = VerificaPonto(Caixa2.Pontos[l2], planos[c]);

                //if the plane divide correctly we use it.
                if ((p1 == -p2) && (Math.Abs(p1) == 1))
                {
                    return c;
                }

                //otherwise we keep the best result
                if (Bounds)
                {
                    if (Math.Sign(p1) == -Math.Sign(p2))
                    {
                        if (Math.Min(Math.Abs(p1), Math.Abs(p2)) > bestDist)
                        {
                            best = c;
                            bestDist = Math.Min(Math.Abs(p1), Math.Abs(p2));
                        }
                    }
                }
            }
            //if (Bounds)
            //    planos[best].d_2 *= bestDist;
            return best;
        }

        private static bool VerificaSeparacao(List<Vector> Padrao1, List<Vector> Padrao2, ref int ini1, ref int ini2)
        {
            int l1 = ini1;
            int l2 = ini2;
			//pontos do padrao 1
            for (l1 = ini1; l1 < Padrao1[0].Length; l1++)
            {
				//pontos do padrao 2
                for (l2 = ini2; l2 < Padrao2[0].Length; l2++)
                {
                    bool igual = true;
					//colunas do padrao 1 ou 2 (deve ser igual)
					//é a mesma coisa que a quantidade de planos encontrada até agora
                    for (int c = 0; c < Padrao1.Count; c++)
                    {
                        double p1 = Padrao1[c]._data[l1];

                        double p2 = Padrao2[c]._data[l2];

                        if ((Math.Sign(p1) == -Math.Sign(p2)) && (p1 != 0))
                        {
                            igual = false;
                            break;
                        }
                    }
                    if (igual)
                    {
                        ini1 = l1;
                        ini2 = l2;
                        return false;
                    }
                }

                //depois da primeira passada pelo FOR de dentro,
                //temos que recomeçar do zero.
                ini2 = 0;
            }
            ini1 = l1;
            ini2 = l2;
            return true;
        }

        private static double VerificaPonto(Vector Ponto, Plano Plano)
        {
            //    % Verifica em qual plano o ponto esta

            //    % O plano divide o espaço em dois semi-espaços, um, que denominaremos
            //    % positivo, compreende o espaço acima do plano, ou seja, o que está
            //    % na direção apontada pelo Vector normal.
            //    % O segundo espaço é o negativo, por estar "abaixo" do plano seguindo
            //    % orientação oposta a do Vector normal.

            //    % Retorna +1 se esta acima do upper bound
            //    % Retorna -1 se esta abaixo do lower bound
            //    % A transicao e linear

            //    % A escolha de trabalhar com o intervalo de [-1,1] foi feita pois
            //    % existe na segunda camada de neuronios um peso denominado relevancia,
            //    % que indica quao "bom" e aquele plano. Dessa forma se ele classifica o
            //    % ponto como negativo ou positivo a influencia dessa multiplicacao deve
            //    % ser igual para ambas as classificacoes.

            //if (UsaPtoMedio)
            //{
            //    double d = (Plano.VectorNormal * (Ponto - (Plano.Maximo + Plano.Minimo) / 2).Transposto())[0, 0];
            //    if (d >= 0)
            //    {
            //        return 1;
            //    }
            //    else
            //    {
            //        return -1;
            //    }
            //}
            //else
            //{


            //    double dmax = (Plano.VectorNormal * (Ponto - Plano.Maximo).Transposto())[0, 0];

            //    if (dmax >= 0)
            //    {
            //        return 1;
            //    }

            //    double dmin = (Plano.VectorNormal * (Ponto - Plano.Minimo).Transposto())[0, 0];
            //    if (dmin <= 0)
            //    {
            //        return -1;
            //    }
            //    else
            //    {
            //        return ((dmin / (dmax - dmin) * 2) + 1);
            //    }
            //}


            //######################################################
            //#############Ultra fast calc##########################
            //######################################################


            double soma = 0;
            for (int i = 0; i < Ponto.Length; i++)
            {
                soma += Ponto._data[i] * Plano.VectorNormal._data[i];
            }

            //double soma = Ponto * Plano.VectorNormal;

            soma += Plano.bias;

            if (!Bounds)
            {
                if (soma >= 0)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                soma /= Plano.d_2;

                if (soma >= 1)
                {
                    return 1;
                }
                if (soma <= -1)
                {
                    return -1;
                }
                return soma;
            }
        }
    }
}
