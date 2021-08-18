namespace SolarCalc
{
    public class MathDegree
    {
        internal static double RadToDeg(double rad) { return rad * 180 / Math.PI; }
        internal static double DegToRad(double deg) { return deg * Math.PI / 180; }

        internal static double SinDegree(double degree)
        {
            return Math.Sin(DegToRad(degree));
        }

        internal static double AsinDegree(double value)
        {
            return RadToDeg(Math.Asin(value));
        }

        internal static double CosDegree(double degree)
        {
            return Math.Cos(DegToRad(degree));
        }

        internal static double AcosDegree(double value)
        {
            return RadToDeg(Math.Acos(value));
        }

        internal static double TanDegree(double degree)
        {
            return Math.Tan(DegToRad(degree));
        }

        internal static double AtanDegree(double value)
        {
            return RadToDeg(Math.Atan(value));
        }

    }
}