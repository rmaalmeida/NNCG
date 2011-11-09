using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace CTL.CT
{
	public class BoundingVolume
	{
		public string Nome;
		public List<Vector> Pontos;
		
		public List<Vector> PontosAcima;
		public List<Vector> PontosAbaixo;
		
		public BoundingVolume volAcima;
		public BoundingVolume volAbaixo;
		
		public Vector Centro;

        public int nivel;
		
        public int QntdDados{
            get {
                return Pontos.Count;
            }
        }
		
		public override string ToString ()
		{
			return string.Format ("[Dad={0}, Niv={1} - {2}]", QntdDados, Profundidade(), Nome);
		}
		
        public int QntdVariaveis{
            get{
                return Pontos[0].Length;
            }
        }
		public BoundingVolume(List<Vector> nPontos, string nNome, int nNivel)
        {
			
            this.Pontos = nPontos;
            this.Nome = nNome;
            this.nivel = nNivel;
		}
		
		public virtual int Projeta(ref List<Plano> Planos, BoundingVolume Caixa2)
        {
			throw new Exception("Implementar Projeta() com override na classe filho");
		}
		
		public virtual int ProcuraPlano(ref List<Plano> Planos, BoundingVolume Caixa2)
        {
			throw new Exception("Implementar ProcuraPlano() com override na classe filho");
		}
		
		public virtual void CriaFilhos()
        {
			throw new Exception("Implementar CriaFilhos() com override na classe filho");
		}
		
		
		public int Profundidade()
        {
            return nivel;
        }



        public int MaxProfundidade()
        {
            int max = 0;
            if (volAbaixo != null)
                max = volAbaixo.MaxProfundidade() + 1;
            if (volAcima != null)
                max = Math.Max(volAcima.MaxProfundidade() + 1, max);
            return max;
        }
		
	}
}

