using System;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using CTL.NN;

namespace CTL.CT
{
	public class PreRedeNeural
	{
	 	public List<Plano>	planos;
        public Matrix padDentro;
		public Matrix padFora;
		public int dentro;
		public int fora;
		
		public PreRedeNeural()
		{
			planos = new List<Plano>();
			dentro = 0;
			fora = 0;
		}
		
		public Rede GerarRedeNeural(Rede.FAtivação[] fs){
			//juntar padDentro com padFora
			//os primeiros neuronios representam pad dentro
			
			//qntd de entradas = qntd de variaveis
			//qntd de neuronios na primeira camada = qnd de planos
			//qntd de neuronios na segunda camada = qnd de padroes dentr+fora
            int[] camadas = new int[3] { planos[0].VectorNormal.Length, planos.Count, padDentro.RowCount + padFora.RowCount};
            Rede dummy = new Rede(camadas, fs);
			
			//preencher a primeira camada
            for (int neuronio = 0; neuronio < planos.Count; neuronio++)
            {
                dummy[0][neuronio].Pesos = (planos[neuronio].VectorNormal.Scale(1/planos[neuronio].d_2)).CopyToArray();
                dummy[0][neuronio].PesoOffset = planos[neuronio].bias / planos[neuronio].d_2;
            }
			
			//preenchendo a segunda camada  paddentro
            double nentradasD = padDentro.ColumnCount;
			
			
            for (int neuronio = 0; neuronio < padDentro.RowCount; neuronio++)
            {
                dummy[1][neuronio].Pesos = (padDentro.GetRowVector(neuronio).Scale( 1/nentradasD)).CopyToArray();
                dummy[1][neuronio].PesoOffset = 0;
                dummy[1][neuronio].desc = dentro;
            }
			
			//padfora
			double nentradasF = padFora.ColumnCount;
			for (int neuronio = 0; neuronio < padFora.RowCount; neuronio++)
            {
                dummy[1][neuronio + padDentro.RowCount].Pesos = (padFora.GetRowVector(neuronio).Scale( 1/nentradasF)).CopyToArray();
                dummy[1][neuronio + padDentro.RowCount].PesoOffset = 0;
                dummy[1][neuronio + padDentro.RowCount].desc = fora;
            }
			
			return dummy;
			
		}
		
		
		public override string ToString ()
		{
			return dentro + " " + fora;
		}

			
	}	
	
}

