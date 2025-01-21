using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Library.Utils
{
    public static class VectorCalculation
    {
        public static double DotProduct(List<double> vector1, List<double> vector2)
        {
            if (vector1.Count != vector2.Count)
            {
                throw new ArgumentException("Vectors must have the same length");
            }
            double result = 0;
            for (int i = 0; i < vector1.Count; i++)
            {
                result += vector1[i] * vector2[i];
            }
            return result;
        }

        public static double Magnitude(List<double> vector)
        {
            double ssum = 0;
            foreach (double d in vector)
            {
                ssum += d * d;
            }
            return Math.Sqrt(ssum);
        }

        public static double AngleBetween(List<double> vector1, List<double> vector2)
        {

            double dotProduct = DotProduct(vector1, vector2);
            double magnitudeVector1 = Magnitude(vector1);
            double magnitudeVector2 = Magnitude(vector2);

            if (magnitudeVector1 == 0 || magnitudeVector2 == 0)
                throw new ArgumentException("Die Länge eines der Vektoren ist 0.");

            // Kosinus des Winkels berechnen
            double cosTheta = dotProduct / (magnitudeVector1 * magnitudeVector2);

            // Numerische Stabilität sicherstellen
            cosTheta = MathUtils.Clamp(cosTheta, -1.0, 1.0);

            // Winkel in Radiant berechnen und in Grad konvertieren
            return Math.Acos(cosTheta) * (180.0 / Math.PI);
        }
    }
}
