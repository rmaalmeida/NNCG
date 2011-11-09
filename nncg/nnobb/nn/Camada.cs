using System;
using System.Collections.Generic;
using System.Text;

namespace CTL.NN
{
    public class Camada
    {
        public Neuronio[] Neuronios;
        public int nEntradas
        {
            get
            {
                if (Neuronios.Length != 0)
                {
                    return Neuronios[0].nEntradas;
                }
                else
                {
                    return 0;
                }
            }
        }
		
        public double[] Saídas;
//        {
//            get
//            {
//                double[] nSaídas = new double[Neurônios.Length];
//
//                for (int i = 0; i < Neurônios.Length; i++)
//                {
//                    nSaídas[i] = Neurônios[i].Saída;
//                }
//                return nSaídas;
//            }
//        }
		
        public Neuronio this[int i]
        {
            get
            {
                return this.Neuronios[i];
            }
            set
            {
                this.Neuronios[i] = value;
            }
        }
		
        public Camada(int nNeuronios, int nEntradas, Rede.FAtivação F)
        {
            Neuronios = new Neuronio[nNeuronios];
			Saídas = new double[nNeuronios];
            for (int i = 0; i < nNeuronios; i++)
            {
                Neuronios[i] = new Neuronio(nEntradas,F);
            }
        }
		
        public void Processamento(double[] Entradas)
        {
                for (int i = 0; i < Neuronios.Length; i++)
                {
					Saídas[i] = Neuronios[i].Processamento(Entradas);
                }
        }

    }
}
