namespace MMOR.Utils.Statistics
{
    /// <summary>
    ///     <strong>Online Covariance</strong>
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Data Struct to caluclate statistics.
    ///     <br /> - Operates on Online/Running algortihms,
    ///     <br /> - i.e. can update the results for each new element without keeping track of the array.
    ///     <br /> -+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///     <br /> - Based on
    ///     <seealso href="https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/">Math.NET.Numerics</seealso>.
    ///     <br /> - Expanded for compability with our frequency mapping based data.
    /// </summary>
    public class RunningCovariance
    {
        public double Count;
        private double covariance;
        private double MeanA;
        private double MeanB;
        public double Covariance => Count > 1 ? covariance / (Count - 1) : double.NaN;

        public void Push(double a, double b)
        {
            Count += 1;
            double deltaA = a - MeanA;
            MeanA += deltaA / Count;
            MeanB += (b - MeanB) / Count;

            covariance += deltaA * (b - MeanB);
        }
    }
}