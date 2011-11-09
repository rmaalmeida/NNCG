using System;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace CTL.CT
{
	public class PreRedeNeural
	{
	 	public List<Plano>	planos;
        public Matrix padDentro;
		public Matrix padFora;
		public string dentro;
		public string fora;
		
		public PreRedeNeural()
		{
			planos = new List<Plano>();
			dentro = "";
			fora = "";
		}
		public override string ToString ()
		{
			return dentro + " " + fora;
		}

			
	}	
	
}

