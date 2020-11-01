using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace LAB_3_SOLAE_TCP_CLIENT_CS
{
    public class Solae
    {
        public Matrix<double> System { get; }
        public Matrix<double> Coeffs { get; }

        public Solae(Matrix<double> system, Matrix<double> coeffs)
        {
            System = system;
            Coeffs = coeffs;
        }

        public Matrix<double> Solve()
        {
            if (System == null || 
                Coeffs == null)
            {
                throw new System.Exception("SOLAE has been initialized incorrectly!");
            }

            return System.Inverse() * Coeffs;
        }
    }
}
