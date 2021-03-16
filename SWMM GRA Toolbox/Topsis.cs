using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SWMM_GRA_Toolbox
{
    public class Topsis
    {
        int m, n;
        Matrix<double> _matrix;
        Matrix<double> _normalizedMatrix;
        Matrix<double> _weighedNormalizedMatrix;
        bool[] _positiveEffects;
        double[] _positiveIdeals, _negativeIdeals;
        double[] _positiveDistances, _negativeDistances;
        double[] _similarity;

        public Matrix<double> Matrix
        {
            get { return _matrix; }
            set { _matrix = value; }
        }

        public double[] Similarity
        {
            get { return _similarity; }
            set { _similarity = value; }
        }

        /// <summary>
		/// Creates the decision matrix.
		/// </summary>
		/// <param name="optionsCount">The number of options.</param>
		/// <param name="criterionsCount">The number of criterions.</param>
        public Topsis(int optionsCount, int criterionsCount, bool[] positiveEffects)
        {
            m = optionsCount;
            n = criterionsCount;
            _matrix = DenseMatrix.OfArray(new double[m, n]);
            _weighedNormalizedMatrix = DenseMatrix.OfArray(new double[m, n]);
            _positiveEffects = positiveEffects;
            _positiveIdeals = new double[n];
            _negativeIdeals = new double[n];
            _positiveDistances = new double[m];
            _negativeDistances = new double[m];
            _similarity = new double[m];
        }

        /// <summary>
		/// Normalizes the decision matrix.
		/// </summary>
        public void Normalize()
        {
            _normalizedMatrix = _matrix.NormalizeColumns(2);

            Parallel.ForEach(_normalizedMatrix.EnumerateRows().ToList(), (rowValues, pls, row_index) =>
            {
                _normalizedMatrix.SetRow((int)row_index, rowValues.Select(x => x = x > 0 ? x : Double.Epsilon).ToArray());
            });
        }

        public void WeighingNormalizedMatrix(double[] w)
        {
            Parallel.ForEach(w.ToList(), (wValue, pls, col_index) =>
            {
                _weighedNormalizedMatrix.SetColumn((int)col_index, _normalizedMatrix.Column((int)col_index).Multiply(wValue));
            });
        }

        public void FindingIdealValues()
        {
            Parallel.ForEach(_positiveEffects.ToList(), (effectValue, pls, col_index) =>
            {
                Vector<double> columnValues = _weighedNormalizedMatrix.Column((int)col_index);

                _positiveIdeals[col_index] = effectValue ? columnValues.Max() : columnValues.Min();
                _negativeIdeals[col_index] = effectValue ? columnValues.Min() : columnValues.Max();
            });
        }

        public void CalculateDistances()
        {
            Parallel.ForEach(_weighedNormalizedMatrix.EnumerateRows().ToList(), (rowValues, pls, row_index) =>
            {
                double positiveDistance = 0;
                double negativeDistance = 0;
                for (int j = 0; j < n; j++)
                {
                    positiveDistance += Math.Pow(rowValues[j] - _positiveIdeals[j], 2);
                    negativeDistance += Math.Pow(rowValues[j] - _negativeIdeals[j], 2);
                }

                _positiveDistances[row_index] = Math.Pow(positiveDistance, 0.5);
                _negativeDistances[row_index] = Math.Pow(negativeDistance, 0.5);
            });
        }

        public void CalculateSimilarity()
        {
            Parallel.ForEach(_negativeDistances.ToList(), (negativeValue, pls, row_index) =>
            {
                _similarity[row_index] = _negativeDistances[row_index] / (_negativeDistances[row_index] + _positiveDistances[row_index]);
            });
        }

    }
}
    

