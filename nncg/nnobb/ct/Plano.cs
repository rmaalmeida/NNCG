using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace CTL.CT
{
    public class Plano : IComparable<Plano>
    {
        public Vector VectorNormal;
        public Vector Maximo;
        public Vector Média;
        public Vector Minimo;
        public double bias;
        public double d_2;
        public double significancia;

        public override string ToString()
        {
            return "N: " + VectorNormal.ToString() + ". S(" + significancia.ToString() + "): Pto_Med: " + Média.ToString();
        }

        public int CompareTo(Plano other)
        {
            //o sinal é para ordenar descendentemente
            return -this.significancia.CompareTo(other.significancia);
        }

    }
}
