using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERS.Core.Algorithms
{
    public class Similarities
    {
        public static double CosSimilarity(Dictionary<int, double> vectorA, Dictionary<int, double> vectorB)
        {
            if (vectorA == null 
                || vectorB == null 
                || vectorA.Count == 0 
                || vectorB.Count == 0)
                return 0.0;

            double dotProduct = 0.0;
            double mA = 0.0;
            double mB = 0.0;

            var smallerDict = vectorA.Count < vectorB.Count ? vectorA : vectorB;
            var largerDict = vectorA.Count < vectorB.Count ? vectorB : vectorA;

            foreach (var kvp in smallerDict)
            {
                if (largerDict.TryGetValue(kvp.Key, out double valueInLarger))
                {
                    dotProduct += kvp.Value * valueInLarger;
                }
            }

            foreach (var val in vectorA.Values)
            {
                mA += Math.Pow(val, 2);
            }

            foreach (var val in vectorB.Values)
            {
                mB += Math.Pow(val, 2);
            }

            mA = Math.Sqrt(mA);
            mB = Math.Sqrt(mB);

            if (mA == 0 || mB == 0)
                return 0.0;

            return dotProduct / (mA * mB);
        }

        public static double EuclidDistance(Dictionary<string, double> userA, Dictionary<string, double> userB)
        {
            if (userA == null || userB == null)
                return double.MaxValue;

            double sum = 0.0;
            int coRatedCount = 0;

            foreach (var courseA in userA)
            {
                if (userB.TryGetValue(courseA.Key, out double ratingB))
                {
                    double ratingA = courseA.Value;
                    double diff = ratingA - ratingB;
                    sum += Math.Pow(diff, 2);
                    coRatedCount++;
                }
            }

            if (coRatedCount == 0)
                return double.MaxValue;

            return Math.Sqrt(sum);
        }
    }
}