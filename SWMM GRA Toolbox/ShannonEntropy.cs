using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SWMM_GRA_Toolbox
{
    public class ShannonEntropy
    {
        int m, n;
        double k;
        Matrix<double> _matrix;
        Matrix<double> _normalizedMatrix;
        double[] E;
        double[] d;
        double[] w;

        public Matrix<double> Matrix
        {
            get { return _matrix; }
            set { _matrix = value; }
        }

        public double[] W
        {
            get { return w; }
            set { w = value; }
        }

        /// <summary>
		/// Creates the decision matrix.
		/// </summary>
		/// <param name="optionsCount">The number of options.</param>
		/// <param name="criterionsCount">The number of criterions.</param>
        public ShannonEntropy(int optionsCount, int criterionsCount)
        {
            m = optionsCount;
            n = criterionsCount;
            _matrix = DenseMatrix.OfArray(new double[m, n]);
            k = 1 / Math.Log(m);
            E = new double[n];
            d = new double[n];
            w = new double[n];
        }

        /// <summary>
		/// Normalizes the decision matrix.
		/// </summary>
        public void Normalize()
        {
            _normalizedMatrix = _matrix.NormalizeColumns(1);

            Parallel.ForEach(_normalizedMatrix.EnumerateRows().ToList(), (rowValues, pls, row_index) =>
            {
                _normalizedMatrix.SetRow((int)row_index, rowValues.Select(x => x = x > 0 ? x : Double.Epsilon).ToArray());
            });
        }

        public void CalculateEntropy()
        {
            Parallel.ForEach(_normalizedMatrix.EnumerateColumns().ToList(), (column, pls, col_index) =>
            {
                column.ToList().ForEach(p => { E[col_index] += -k * p * Math.Log(p); });
            });

            //for (int i = 0; i < _normalizedMatrix.EnumerateColumns().ToList().Count(); i++)
            //{
            //    for (int j = 0; j < _normalizedMatrix.EnumerateColumns().ToList().ElementAt(i).Count; j++)
            //    {
            //        double p = _normalizedMatrix.EnumerateColumns().ToList().ElementAt(i)[j];
            //        E[i] += -k * p * Math.Log(p);
            //    }
            //}
        }

        public void CalculateDistances()
        {
            Parallel.ForEach(E, (eValue, pls, e_index) =>
            {
                d[e_index] = 1 - eValue;
            });
        }

        public void CalculateWeights()
        {
            Parallel.ForEach(d, (dValue, pls, d_index) =>
            {
                w[d_index] = dValue / d.Sum();
            });
        }
    }

}
