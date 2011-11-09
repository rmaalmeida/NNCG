using System;
using System.Collections.Generic;
using System.Text;

namespace CTL.NN
{
    public class Rede
    {
        public delegate double FAtivação(double e);
        public  Camada[] Camadas;
        public Camada this[int i]
        {
            get
            {
                return this.Camadas[i];
            }
            set
            {
                this.Camadas[i] = value;
            }
        }
		
        public Rede(int[] nNeurônios, FAtivação[] F)
        {
            //menos 1 pois a primeira camada não conta
            Camadas = new Camada[nNeurônios.Length -1];
            for (int i = 0; i < nNeurônios.Length-1; i++)
            {
                //o numero de entradas de uma camada é o numero de saidas(neurônios), da anterior
                Camadas[i] = new Camada(nNeurônios[i+1], nNeurônios[i],F[i]);
            }
        }
		
        public double[] CalculaSaída(double[] entradas)
        {
            Camadas[0].Processamento(entradas);

            for (int i = 1; i < Camadas.Length; i++)
            {
                Camadas[i].Processamento(Camadas[i-1].Saídas);
            }
            return Camadas[Camadas.Length - 1].Saídas;
        }

    }
}
