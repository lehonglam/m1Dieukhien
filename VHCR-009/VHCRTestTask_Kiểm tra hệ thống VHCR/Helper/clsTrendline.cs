using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHCRTestTask
{
    class clsTrendline
    {
        /// <summary>
        /// Get's the line's best fit slope of the line
        /// </summary>
        public double Slope { get; private set; }

        /// <summary>
        /// Get's the Mia
        /// </summary>
        public double Offset { get; private set; }

        public double[] ValuesX { get; private set; }

        public double[] ValuesY { get; private set; }

        private double sumY;

        private double sumX;

        private double sumXY;

        private double sumX2;

        private double n;

        public void Trendline(double[] x, double[] y)
        {
            this.ValuesX = x;
            this.ValuesY = y;

            this.sumXY = this.calculateSumXsYsProduct(this.ValuesX, this.ValuesY);
            this.sumX = this.calculateSumXs(this.ValuesX);
            this.sumY = this.calculateSumYs(this.ValuesY);
            this.sumX2 = this.calculateSumXsSquare(this.ValuesX);
            this.n = this.ValuesX.Length;

            this.calculateSlope();
            this.calculateOffset();
        }

        public void Trendline(double[] y)
        {
            //Assinging Y Values
            this.ValuesY = y;
            int length = y.Length;
            //Assinging X Values
            this.ValuesX = new double[length];
            for (int i = 0; i < length; i++)
            {
                this.ValuesX[i] = i;
            }


            this.sumXY = this.calculateSumXsYsProduct(this.ValuesX, this.ValuesY);
            this.sumX = this.calculateSumXs(this.ValuesX);
            this.sumY = this.calculateSumYs(this.ValuesY);
            this.sumX2 = this.calculateSumXsSquare(this.ValuesX);
            this.n = this.ValuesX.Length;
        }

        public void Trendline(double[][] xy)
        {
            double[] xs = new double[xy.Length];
            double[] ys = new double[xy.Length];
            for (int i = 0; i < xy.Length; i++)
            {
                xs[i] = xy[i][0];
                ys[i] = xy[i][1];
            }
            this.ValuesX = xs;
            this.ValuesY = ys;
        }

        private double calculateSumXsYsProduct(double[] xs, double[] ys)
        {
            double sum = 0;
            for (int i = 0; i < xs.Length; i++)
            {
                sum += xs[i] * ys[i];
            }
            return sum;
        }

        private double calculateSumXs(double[] xs)
        {
            double sum = 0;
            foreach (double x in xs)
            {
                sum += x;
            }
            return sum;
        }

        private double calculateSumYs(double[] ys)
        {
            double sum = 0;
            foreach (double y in ys)
            {
                sum += y;
            }
            return sum;
        }

        private double calculateSumXsSquare(double[] xs)
        {
            double sum = 0;
            foreach (double x in xs)
            {
                sum += System.Math.Pow(x, 2);
            }
            return sum;
        }

        private void calculateSlope()
        {
            try
            {
                Slope = (n * sumXY - sumX * sumY) / (n * sumX2 - System.Math.Pow(sumX, 2));
            }
            catch (DivideByZeroException)
            {
                Slope = 0;
            }
        }

        private void calculateOffset()
        {
            try
            {
                Slope = (sumY - Slope * sumX) / n;
            }
            catch (DivideByZeroException) { }
        }
    }
}
