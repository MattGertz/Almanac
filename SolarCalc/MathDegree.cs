using System;

namespace SolarCalc
{
    public class MathDegree
    {
        internal static double RadToDeg(double rad) => rad * 180.0 / Math.PI;
        internal static double DegToRad(double deg) => deg * Math.PI / 180.0;
        internal static double RadToGrad(double rad) => rad * 200.0 / Math.PI;
        internal static double GradToRad(double grad) => grad * Math.PI / 200.0;
        internal static double GradToDeg(double grad) => grad * 9.0/10.0;
        internal static double DegToGrad(double rad) => rad * 10.0/9.0;


        internal static double SinDegree(double degree) => Math.Sin(DegToRad(degree));
        internal static double AsinDegree(double value) => RadToDeg(Math.Asin(value));
        internal static double CosDegree(double degree) => Math.Cos(DegToRad(degree));
        internal static double AcosDegree(double value) => RadToDeg(Math.Acos(value));
        internal static double TanDegree(double degree) => Math.Tan(DegToRad(degree));
        internal static double AtanDegree(double value) => RadToDeg(Math.Atan(value));
        internal static double Atan2Degree(double valueY, double valueX) => RadToDeg(Math.Atan2(valueY,valueX));

        internal static double SinGradian(double gradian) => Math.Sin(GradToRad(gradian));
        internal static double AsinGradian(double value) => RadToGrad(Math.Asin(value));
        internal static double CosGradian(double gradian) => Math.Cos(GradToRad(gradian));
        internal static double AcosGradian(double value) => RadToGrad(Math.Acos(value));
        internal static double TanGradian(double gradian) => Math.Tan(GradToRad(gradian));
        internal static double AtanGradian(double value) => RadToGrad(Math.Atan(value));
        internal static double Atan2Gradian(double valueY, double valueX) => RadToGrad(Math.Atan2(valueY, valueX));

    }
}