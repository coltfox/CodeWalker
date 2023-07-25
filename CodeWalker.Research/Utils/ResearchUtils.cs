using SharpDX;

namespace CodeWalker.Research.Utils
{
    public class ResearchUtils
    {
        public static bool matsNearEqual(Matrix a, Matrix b, double tolerance)
        {
            float E = (float)tolerance;
            if (MathUtil.WithinEpsilon(a.M11, b.M11, E) && MathUtil.WithinEpsilon(a.M12, b.M12, E) && MathUtil.WithinEpsilon(a.M13, b.M13, E) && MathUtil.WithinEpsilon(a.M14, b.M14, E) && MathUtil.WithinEpsilon(a.M21, b.M21, E) && MathUtil.WithinEpsilon(a.M22, b.M22, E) && MathUtil.WithinEpsilon(a.M23, b.M23, E) && MathUtil.WithinEpsilon(a.M24, b.M24, E) && MathUtil.WithinEpsilon(a.M31, b.M31, E) && MathUtil.WithinEpsilon(a.M32, b.M32, E) && MathUtil.WithinEpsilon(a.M33, b.M33, E) && MathUtil.WithinEpsilon(a.M34, b.M34, E) && MathUtil.WithinEpsilon(a.M41, b.M41, E) && MathUtil.WithinEpsilon(a.M42, b.M42, E) && MathUtil.WithinEpsilon(a.M43, b.M43, E))
            {
                return MathUtil.WithinEpsilon(a.M44, b.M44, E);
            }

            return false;
        }
    }
}
