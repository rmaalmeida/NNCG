using System;
using System.Collections.Generic;
using System.Text;

namespace CTL.NN
{
    public class Neuronio
    {
        public double[] Pesos;
        public Rede.FAtivação F;
        public double PesoOffset;
        public double[] Entradas;
        public double Saída;
        public string desc;
        public int nEntradas
        {
            get
            {
                return Pesos.Length;
            }
        }
        public double this[int i]
        {
            get
            {
                return this.Pesos[i];
            }
            set
            {
                this.Pesos[i] = value;
            }
        }
        public Neuronio(int nEntradas, Rede.FAtivação nF)
        {
            Pesos = new double[nEntradas];
            F = nF;
        }

        public double Processamento(double[] nEntradas)
        {
            Entradas = nEntradas;
            if (nEntradas.Length == Pesos.Length)
            {
                Saída = 0;
                for(int i=0; i<nEntradas.Length;i++)
                {
                    Saída += nEntradas[i] * Pesos[i];
                }
                Saída += PesoOffset;
                Saída = F(Saída);
				return Saída;
			}
            else
            {
                throw new Exception("Entradas não compatíveis");
            }
        }

        public override string ToString()
        {
            string sNeuronio = "";
            for (int i = 0; i < nEntradas; i++)
            {
                sNeuronio = sNeuronio+" w"+i.ToString()+":"+Math.Round( Pesos[i],3);
            }   
            sNeuronio = sNeuronio+" woff:"+Math.Round(PesoOffset,3);
            return sNeuronio;
        }
    }
}
