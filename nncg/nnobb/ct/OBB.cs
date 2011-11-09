using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace CTL.CT
{
		
    public class OBB : BoundingVolume
    {

		public Matrix AutoVectores;
		public Matrix MatrixTransformacao;
		public Vector Limites;
		public Matrix VectoresFace;
		
        //Encontra uma caixa N-dimensional orientada ao eixo de maior significancia
        public OBB(List<Vector> nPontos, string nNome, int nNivel) : base (nPontos, nNome, nNivel)
        {
            if ((nPontos == null) || (nPontos.Count == 0))
            {
                return;
            }



            //Caso exista apenas um ponto não há necessidade de realizar todos os cálculos
            if (QntdDados == 1)
            {

                //o centro é o proprio ponto
                this.Centro = this.Pontos[0];
                //os autoVectores são os proprios e1, e2, en.
                this.AutoVectores = Matrix.Identity(QntdVariaveis, QntdVariaveis);
                //necessária para calculos
                this.MatrixTransformacao = this.AutoVectores;
                //não existe limite para 1 ponto
                this.Limites = new Vector(QntdVariaveis);
                //apanas para cálculos
                this.VectoresFace = Vector.VectorRepetitionLines(Pontos[0], QntdVariaveis);
            }
            else
            {
                //Pto médio é diferente de Pto central!!!!!!!!!!!!!!

                Vector PontoMedio = MédiaDosPontos(Pontos);
                Matrix CovMat = MatrixDeCovariancia(Pontos, PontoMedio);

                //EigenvalueDecomposition eig = CovMat.EigenvalueDecomposition;
                this.AutoVectores = CovMat.EigenvalueDecomposition.EigenVectors;
                //os autoVectores são as colunas
                //por isso mudamos para que sejam as linhas
                this.AutoVectores.TransposeInplace();

                //double[] AutoValores = new double[this.AutoVectores.RowCount];
                //for (int i = 0; i < this.AutoVectores.ColumnCount; i++)
                //{
                //    AutoValores[i] = CovMat.EigenvalueDecomposition.EigenValues[i].Real;
                //}

                //        %--------------------------------------------------------
                //        % Calculo dos eixos
                //        %--------------------------------------------------------
                //        %Procura o maior autovalor relativo ao maior autoVector: a direcao dominante da caixa

                //double V_Maior = double.NegativeInfinity;
                //int i_Maior = 0;

                //for (int i = 0; i < Qntd_Variaveis; i++)
                //{
                //    //double temp = AutoValores[i];
                //    if (AutoValores[i] > V_Maior)
                //    {
                //        //Salva a posição da maior direção e seu respectivo valor
                //        i_Maior = i;
                //        V_Maior = AutoValores[i];
                //    }
                //}


                // o maior autovetor é sempre o ultimo
                int i_Maior = AutoVectores.ColumnCount -1;

                //        %--------------------------------------------------------
                //        % Calculo dos limites, centro e Vectores <centro-face>
                //        %--------------------------------------------------------

                //        % A MatrixTransformacao faz a mudanca do sistema de coordenadas do
                //        % sistema global para o sistema local da caixa para a qual foi calculada,
                //        % transformando os Vectores e1, e2, ... , en em av1, av2, ..., avn.
                //        % Local * AutoVectores = Global
                //        % Local = Global * Inversa(AutoVectores*) *com os autoVectores nas linhas


                // para o caso da Matrix de autoVectores Transposta = Inversa.
                this.MatrixTransformacao = this.AutoVectores.Clone();
                this.MatrixTransformacao.TransposeInplace();

                //        % Encontra os limites superiores e inferiores em cada direcao da caixa

                Vector Limites_Superiores = new Vector(QntdVariaveis,double.NegativeInfinity);
                Vector Limites_Inferiores = new Vector(QntdVariaveis,double.PositiveInfinity);

                for (int i = 0; i < QntdDados; i++)
                {
                    //% Muda o sistema de coordenada dos pontos para o sistema local, com o ponto
                    //% médio como centro. Com base nisto determinamos os limites superiores e
                    //% inferiores em cada Vector. Obs: Lim_Sup sempre eh + e Lim_Inf sempre eh -.

                    Vector Valores_Projetado = this.Pontos[i].Subtract(PontoMedio);
                    Valores_Projetado.MatrixMultiplyInPlace(this.MatrixTransformacao);
					

                    Limites_Superiores = Valores_Projetado.Maximum(Limites_Superiores);
                    Limites_Inferiores = Valores_Projetado.Minimum(Limites_Inferiores);
                }

                this.Limites = (Limites_Superiores.Subtract(Limites_Inferiores));
                this.Limites.ScaleInplace(0.5);
                this.Centro = (Limites_Inferiores.Add(Limites_Superiores));
                this.Centro.ScaleInplace(0.5);
                this.Centro.MatrixMultiplyInPlace(this.AutoVectores);
                this.Centro.AddInplace(PontoMedio);

                //        %Os Vectores são as linhas da Matrix porque JÁ transpomos a Matrix no começo
                this.VectoresFace = this.AutoVectores.Clone();
                //        %Os autovalores (no mundo global) saem do centro e apontam para a
                //        %face da caixa mas são unitários. Desse modo multiplicamos estes
                //        %pelos limites, valores que indicam a distancia do centro a face e
                //        %somamos com o ponto central para obter o Vector no mundo global.
                for (int i = 0; i < QntdVariaveis; i++)
                {
                    Vector dummy = this.VectoresFace.GetRowVector(i);
                    dummy.ScaleInplace(this.Limites._data[i]);
                    dummy.AddInplace(this.Centro);
                    this.VectoresFace.SetRowVector(dummy, i);
                }

                //        %--------------------------------------------------------
                //        % Separacao dos pontos superiores e inferiores
                //        %--------------------------------------------------------
                //        % Os pontos só são separados caso a Qnt_Dados seja maior que 1


                List<int> pAcim = new List<int>(QntdDados);
                List<int> pAbai = new List<int>(QntdDados);

                // pega o autovetor principal para reduzir necessidade de acesso ao cache
                double[] maxAV = new double[AutoVectores.ColumnCount];
                for (int i = 0; i < AutoVectores.ColumnCount; i++)
                {
                    maxAV[i] = AutoVectores._data[i_Maior][i];
                }

                for (int i = 0; i < QntdDados; i++)
                {
                    double soma = 0;
                    for (int j = 0; j < Pontos[0].Length; j++)
                    {
                        soma += (Pontos[i]._data[j] - this.Centro._data[j]) * maxAV[j];
                    }

                    if (soma >= 0)
                    {
                        pAcim.Add(i);
                    }
                    else
                    {
                        pAbai.Add(i);
                    }
                }

                PontosAbaixo = new List<Vector>();
                for (int i = 0; i < pAbai.Count; i++)
                {
                    PontosAbaixo.Add(Pontos[pAbai[i]]);
                }
                PontosAcima = new List<Vector>();
                for (int i = 0; i < pAcim.Count; i++)
                {
                    PontosAcima.Add(Pontos[pAcim[i]]);
                }
            }
        }

        private Matrix MatrixDeCovariancia(List<Vector> Pontos, Vector VectorMedio)
        {
            int i, j, k;
            int ColumnCount = Pontos[0].Length;
            int RowCount = Pontos.Count;

            double[][] data = Matrix.CreateMatrixData(ColumnCount, ColumnCount);
            double soma;

            for (j = 0; j < ColumnCount; j++)
            {
                for (k = 0; k <= j; k++)
                {
                    soma = 0;
                    for (i = 0; i < RowCount; i++)
                    {
                        soma += (Pontos[i]._data[j] - VectorMedio._data[j]) *
                            (Pontos[i]._data[k] - VectorMedio._data[k]);
                    }
                    soma /= (RowCount - 1);
                    data[j][k] = soma;
                    data[k][j] = soma;
                }
            }
            return new Matrix(data);
        }

        private Vector MédiaDosPontos(List<Vector> Pontos)
        {
            int tam = Pontos[0].Length;

            double[] dummy = new double[tam];

            foreach (Vector v in Pontos)
            {
                for (int i = 0; i < tam; i++)
                {
                    dummy[i] += v._data[i];
                }
            }
            for (int i = 0; i < tam; i++)
            {
                dummy[i] /= Pontos.Count;
            }
            return new Vector(dummy);

        }
		
		public override int ProcuraPlano(ref List<Plano> Planos, BoundingVolume Caixa2)
        {
            int qntd = 0;
            qntd += this.Projeta(ref Planos, Caixa2);
            qntd += Caixa2.Projeta(ref Planos, this);

            return qntd;
        }
		
        public override int Projeta(ref List<Plano> Planos, BoundingVolume nCaixa)
        {
			OBB Caixa2 = ((OBB)nCaixa);
            int qntd = 0;
            //    %--------------------------------------------------------
            //    % Projetando a Caixa2 nesta Caixa
            //    %--------------------------------------------------------
            //pega os Vectores
            Matrix t = Caixa2.VectoresFace.Clone();
            //subtrai o centro para preparar para a rotação
            t.SubtractInplace(this.Centro);
            //rotaciona de acordo com a transformação estabelecida
            t.MultiplyInplace(this.MatrixTransformacao);

            //procurando distancias maximas e minimas a partir do centro projetado.

            //centro projetado no SCL da caixa 1
            //Vector Centro_Projetado = (Caixa2.Centro - Caixa1.Centro) * Caixa1.MatrixTransformação;
            Vector Centro_Projetado = Caixa2.Centro.Clone();
            Centro_Projetado.SubtractInplace(this.Centro);
            Centro_Projetado.MatrixMultiplyInPlace(this.MatrixTransformacao);

            //pega os VectoresFace da caixa 2 e subtrai o centro da caixa 2.
            //desse modo os VetF estarao situados na origem
            t.SubtractInplace(Vector.VectorRepetitionLines(Centro_Projetado, Caixa2.QntdVariaveis));
            //Soma o valor absoluto em cada eixo (x, y, ....) pois existe uma combinação linear
            //dos VetF que consegue atingir esse valor nesse eixo, configurando o máximo de extensão
            //positivo ou negativo com esse módulo nesta direção.
            t.Abs();
            //Vector nt = t.SomaColuna();
            Vector nt = Vector.Ones(t.ColumnCount);
            nt.MatrixMultiplyInPlace(t);

            Vector Maximo = Centro_Projetado.Add(nt);
            Vector Minimo = Centro_Projetado.Subtract(nt);
            //    % Verifica se o centro projetado da caixa 2 no Vector 'i' da caixa 1 é
            //    % positivo ou negativo
            for (int i = 0; i < Caixa2.QntdVariaveis; i++)
            {
                //        % Se positivo, o menor valor das projecoes de todos os vertices
                //        % no Vector 'i' tem que ser maior que a borda da caixa
                if (Centro_Projetado._data[i] > 0)
                {
                    if (Minimo._data[i] > this.Limites._data[i])
                    {
                        Plano p = new Plano();
                        p.Minimo = Vector.Zeros(this.QntdVariaveis);
                        p.Minimo._data[i] = this.Limites._data[i];
                        //                 % PontoMin é o ponto pelo qual passa o plano mais rente a caixa
                        //p.Minimo = (p.Minimo * Caixa1.AutoVectores) + Caixa1.Centro;
                        p.Minimo.MatrixMultiplyInPlace(this.AutoVectores);
                        p.Minimo.AddInplace(this.Centro);
                        //                 % PontoMax é o ponto pelo qual passa o plano mais longe da caixa
                        p.Maximo = new Vector(this.QntdVariaveis);
                        p.Maximo._data[i] = Minimo._data[i];

                        //p.Maximo = (p.Maximo * Caixa1.AutoVectores) + Caixa1.Centro;
                        p.Maximo.MatrixMultiplyInPlace(this.AutoVectores);
                        p.Maximo.AddInplace(this.Centro);

                        double dummy = Math.Abs(Minimo._data[i] - this.Limites._data[i]);
                        //                 % Direção é o Vector normal ao plano de separação
                        p.VectorNormal = this.AutoVectores.GetRowVector(i);
                        p.Média = (p.Maximo.Add(p.Minimo));
                        p.Média.ScaleInplace(0.5);

                        p.bias = -(p.Média.ScalarMultiply(p.VectorNormal));
                        p.d_2 = Math.Max(dummy / 2, double.MinValue);
                        //p.significancia = 0;// p.d_2;// Caixa1.Qntd_Dados + Caixa2.Qntd_Dados;
                        Planos.Add(p);
                        qntd++;
                    }
                }
                else if (Centro_Projetado._data[i] < 0)
                {
                    //            % Se negativo, o maior valor das projecoes de todos os vertices
                    //            % no Vector 'i' tem que ser menor que a borda da caixa
                    if (Maximo._data[i] < -this.Limites._data[i])
                    {
                        Plano p = new Plano();
                        p.Minimo = new Vector(this.QntdVariaveis);
                        p.Minimo._data[i] = -this.Limites._data[i];
                        //p.Minimo = (p.Minimo * Caixa1.AutoVectores) + Caixa1.Centro;
                        p.Minimo.MatrixMultiplyInPlace(this.AutoVectores);
                        p.Minimo.AddInplace(this.Centro);

                        //                 % PontoMin é o ponto pelo qual passa o plano mais rente a caixa
                        p.Maximo = new Vector(this.QntdVariaveis);
                        p.Maximo._data[i] = Maximo._data[i];
                        //p.Maximo = (p.Maximo * Caixa1.AutoVectores) + Caixa1.Centro;
                        p.Maximo.MatrixMultiplyInPlace(this.AutoVectores);
                        p.Maximo.AddInplace(this.Centro);

                        double dummy = Math.Abs(Maximo._data[i] - this.Limites._data[i]);
                        //                 % PontoMin é o ponto pelo qual passa o plano mais longe da caixa
                        p.VectorNormal = this.AutoVectores.GetRowVector(i);
                        p.Média = p.Maximo.Add( p.Minimo);
                        p.Média.ScaleInplace(0.5);
                        p.bias = -(p.Média.ScalarMultiply(p.VectorNormal));
                        p.d_2 = Math.Max(dummy / 2, double.MinValue);
                        //p.significancia = 0;//p.d_2;// Caixa1.Qntd_Dados + Caixa2.Qntd_Dados;
                        Planos.Add(p);
                        qntd++;
                    }
                }
            }
            return qntd;
        }
		
		public override void CriaFilhos()
        {
            //            % Na chamada de função abaixo a ordem é importante pois invertendo a
            //            % ordem da chamada anterior fazemos a divisao da arvore de uma
            //            % maneira mais uniforme, caso contrario o teste se daria como em
            //            % uma busca em profundidade.

            if ((this.volAcima == null) && (this.PontosAcima.Count > 0))
			{
                this.volAcima = new OBB(this.PontosAcima, this.Nome, this.nivel + 1);
            }
            if ((this.volAbaixo == null) && (this.PontosAbaixo.Count > 0))
            {
                this.volAbaixo = new OBB(this.PontosAbaixo, this.Nome, this.nivel + 1);
            }
        
		}
    }
}
