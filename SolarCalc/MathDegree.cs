namespace SolarCalc
{
    public class MathDegree
    {
        internal static double RadToDeg(double rad) => rad * 180 / Math.PI; 
        internal static double DegToRad(double deg) => deg * Math.PI / 180; 

        internal static double SinDegree(double degree) => Math.Sin(DegToRad(degree));
        internal static double AsinDegree(double value) => RadToDeg(Math.Asin(value));
        internal static double CosDegree(double degree) => Math.Cos(DegToRad(degree));
        internal static double AcosDegree(double value) => RadToDeg(Math.Acos(value));
        internal static double TanDegree(double degree) => Math.Tan(DegToRad(degree));
        internal static double AtanDegree(double value) => RadToDeg(Math.Atan(value));
        internal static double Atan2Degree(double valueY, double valueX) => RadToDeg(Math.Atan2(valueY,valueX));
    }
}