using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace CTL.CT
{
		
    public class Sphere : BoundingVolume
    {
  		private double raio = 0;
        public override string ToString()
        {
            return "Nome: " + Nome + ". Pts: " + Pontos;// +". Centro: " + Centro + ". Limites: " + Limites;
        }

        //Encontra uma esfera N-dimensional
        public Sphere(List<Vector> nPontos, string nNome, int nNivel) : base (nPontos,nNome, nNivel)
        {


            //Caso exista apenas um ponto não há necessidade de realizar todos os cálculos
            if (QntdDados == 1)
            {

                //o centro é o proprio ponto
                this.Centro = this.Pontos[0];
                this.raio = 0;
            }
            else
            {
                //Pto médio é diferente de Pto central!!!!!!!!!!!!!!

                Vector minimo = new Vector (this.QntdVariaveis,double.MaxValue);
				Vector maximo = new Vector (this.QntdVariaveis);
				
				
				
                for (int i = 0; i < QntdDados; i++)
                {
					minimo.MinimumInPlace(this.Pontos[i]);
					maximo.MaximumInPlace(this.Pontos[i]);
                }
				//maior + menor / 2
				this.Centro = minimo.Add(maximo);
				this.Centro.ScaleInplace(0.5);

				Vector tempRaio = maximo.Subtract(minimo);
				tempRaio.ScaleInplace(0.5);
				
				this.raio = 0;
				for (int i = 0; i < QntdVariaveis; i++)
                {
					this.raio = Math.Max (this.raio,tempRaio._data[i]);
                }
				

                //        %--------------------------------------------------------
                //        % Separacao dos pontos superiores e inferiores (usa msma divisao da OBB
                //        %--------------------------------------------------------
                //        % Os pontos só são separados caso a Qnt_Dados seja maior que 1

				OBB tmp = new OBB(nPontos,nNome,0);
				this.PontosAcima = tmp.PontosAcima;
				this.PontosAbaixo = tmp.PontosAbaixo;

            }
        }

		public override List<Plano> ProcuraPlano( BoundingVolume Caixa2)
        {
			List<Plano> Planos = new List<Plano>();
			
            Vector vetDist = this.Centro.Subtract(Caixa2.Centro);
			double dist = vetDist.Norm();
			if (dist > (this.raio + ((Sphere)Caixa2).raio))
			{
				Plano p = new Plano();
				p.VectorNormal = vetDist.Normalize();
				//p.Média = this.Centro.Add(vetDist.Scale(0.5));
				p.Maximo = this.Centro.Subtract(       p.VectorNormal.Scale(            this.raio));
				p.Minimo = Caixa2.Centro.Add(p.VectorNormal.Scale(((Sphere)Caixa2).raio));
				p.Média = p.Maximo.Add(p.Minimo).Scale(0.5);
				p.d_2 = Math.Max(dist, double.MinValue);
				//bias do neurônio
				p.bias = -(p.Média.ScalarMultiply(p.VectorNormal));
				Planos.Add(p);
			}
            return Planos;
        }
		
		
		public override void CriaFilhos()
        {
            //            % Na chamada de função abaixo a ordem é importante pois invertendo a
            //            % ordem da chamada anterior fazemos a divisao da arvore de uma
            //            % maneira mais uniforme, caso contrario o teste se daria como em
            //            % uma busca em profundidade.

            if ((this.volAcima == null) && (this.PontosAcima.Count > 0))
			{
                this.volAcima = new Sphere(this.PontosAcima, this.Nome, this.nivel+1);
            }
            if ((this.volAbaixo == null) && (this.PontosAbaixo.Count > 0))
            {
                this.volAbaixo = new Sphere(this.PontosAbaixo, this.Nome, this.nivel + 1);
            }
        
		}
    }
}
