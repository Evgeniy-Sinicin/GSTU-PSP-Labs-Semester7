using System;
using MathNet.Numerics.LinearAlgebra;

namespace LAB_3_SOLAE_TCP_CLIENT_CS
{
    [Serializable]
    public class Message
    {
        public double[,] System { get; }
        public double[,] Coeffs { get; }
        public double[,] Decision { get; set; }
        public Message(Matrix<double> system, Matrix<double> coeffs)
        {
            System = system.ToArray();
            Coeffs = coeffs.ToArray();
        }
    }
}
