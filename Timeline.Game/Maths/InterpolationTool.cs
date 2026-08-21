using System.Numerics;

namespace Timeline.Game.Maths;

public static class InterpolationTool
{
    public static Vector2 Linear(Vector2 start, Vector2 end, float progress) =>
        start + (end - start) * progress;

    public static Vector2 Linear(Vector2[] Points, float progress)
    {
        if (Points.Length == 0)
            return Vector2.Zero;
        if (Points.Length == 1)
            return Points[0];
        float Total = 0;
        float[] ds = new float[Points.Length - 1];
        for (int i = 0; i + 1 < Points.Length; i++)
        {
            Total += Vector2.Distance(Points[i], Points[i + 1]);
            ds[i] = Total;
        }
        float tgd = Total * progress;
        for (int i = 0; i < ds.Length; i++)
        {
            if (ds[i] > tgd)
            {
                // 目标落在线段 [i, i+1] 内（注意 ds[i] 是 Points[i+1] 的累积距离）
                float prevDist = (i == 0) ? 0f : ds[i - 1]; // 线段起点距离
                float segLength = ds[i] - prevDist; // 当前线段长度
                float t = (tgd - prevDist) / segLength; // 正确进度
                return Vector2.Lerp(Points[i], Points[i + 1], t);
            }
            else if (ds[i] == tgd) // 恰好落在线段终点
            {
                return Points[i + 1];
            }
        }

        // 若 tgd 等于总长度或略大（由于浮点误差），返回最后一个点
        return Points[^1];
    }

    //二阶
    public static Vector2 QuadraticEaseIn(Vector2 start, Vector2 end, float progress) =>
        Linear(start, end, progress * progress);

    public static Vector2 QuadraticEaseOut(Vector2 start, Vector2 end, float progress) =>
        Linear(start, end, 1f - (1f - progress) * (1f - progress));

    public static Vector2 QuadraticEaseInOut(Vector2 start, Vector2 end, float progress) =>
        progress < 0.5f
            ? QuadraticEaseIn(start, end * 0.5f + start * 0.5f, 2f * progress)
            : QuadraticEaseOut(end * 0.5f + start * 0.5f, end, 2f * progress - 1f);

    //三阶
    public static Vector2 CubicEaseIn(Vector2 start, Vector2 end, float progress) =>
        Linear(start, end, progress * progress * progress);

    public static Vector2 CubicEaseOut(Vector2 start, Vector2 end, float progress) =>
        Linear(start, end, 1f - (1f - progress) * (1f - progress) * (1f - progress));

    public static Vector2 CubicEaseInOut(Vector2 start, Vector2 end, float progress) =>
        progress < 0.5f
            ? CubicEaseIn(start, end * 0.5f + start * 0.5f, 2f * progress)
            : CubicEaseOut(end * 0.5f + start * 0.5f, end, 2f * progress - 1f);

    public static Vector2 Bezier(Vector2[] Points, float progress)
    {
        //空值或无意义值处理
        if (Points.Length == 0)
            return Vector2.Zero;
        if (Points.Length == 1)
            return Points[0];
        if (progress <= 0)
            return Points[0];
        if (progress >= 1)
            return Points.Last();
        Vector2[] p = (Vector2[])Points.Clone();
        int n = p.Length;

        while (n > 1)
        {
            for (int i = 0; i < n - 1; i++)
            {
                //算线性
                p[i] = Vector2.Lerp(p[i], p[i + 1], progress);
            }
            n--; //减少范围
        }
        return p[0];
    }
    public static Vector2 GetPointOnPerfCircle(Vector2 start, Vector2 mid, Vector2 end, float progress)
    {
        // 1. 计算圆心
        if (!TryGetCircleCenter(start, mid, end, out Vector2 center))
            return Linear([start, mid, end], progress);

        // 2. 半径
        float radius = Vector2.Distance(start, center);

        // 3. 各点极角
        float angleStart = (float)Math.Atan2(start.Y - center.Y, start.X - center.X);
        float angleMid = (float)Math.Atan2(mid.Y - center.Y, mid.X - center.X);
        float angleEnd = (float)Math.Atan2(end.Y - center.Y, end.X - center.X);

        // 4. 计算从 start 到 end 经过 mid 的总角度（带符号）
        float totalAngle = GetSignedAngle(angleStart, angleMid, angleEnd);

        // 5. 插值角度
        float angle = angleStart + totalAngle * Math.Clamp(progress, 0f, 1f);

        // 6. 返回点
        return center + new Vector2(radius * (float)Math.Cos(angle), radius * (float)Math.Sin(angle));
    }

    private static bool TryGetCircleCenter(Vector2 a, Vector2 b, Vector2 c, out Vector2 center)
    {
        float ax = a.X, ay = a.Y;
        float bx = b.X, by = b.Y;
        float cx = c.X, cy = c.Y;

        float d = 2 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
        if (Math.Abs(d) < 1e-6f)
        {
            center = Vector2.Zero;
            return false;
        }

        float ux = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
        float uy = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;
        center = new Vector2(ux, uy);
        return true;
    }

    private static float GetSignedAngle(float start, float mid, float end)
    {
        // 归一化差值到 [-PI, PI]
        float Normalize(float a)
        {
            while (a > Math.PI) a -= 2 * (float)Math.PI;
            while (a < -Math.PI) a += 2 * (float)Math.PI;
            return a;
        }

        float deltaMid = Normalize(mid - start);
        float deltaEnd = Normalize(end - start);

        // 如果 deltaMid 和 deltaEnd 同号，且 deltaEnd 包含 deltaMid（绝对值更大），直接返回 deltaEnd
        if (Math.Sign(deltaMid) == Math.Sign(deltaEnd) && Math.Abs(deltaMid) <= Math.Abs(deltaEnd))
            return deltaEnd;

        // 否则调整 deltaEnd 加或减 2PI，使 deltaMid 落在 0 和调整后的值之间
        float candidate1 = deltaEnd + (deltaEnd >= 0 ? -2 * (float)Math.PI : 2 * (float)Math.PI);
        float candidate2 = deltaEnd + (deltaEnd >= 0 ? 2 * (float)Math.PI : -2 * (float)Math.PI);

        bool Between(float angle, float a, float b)
        {
            if (a <= b) return angle >= a && angle <= b;
            else return angle >= b && angle <= a;
        }

        if (Between(deltaMid, 0, candidate1)) return candidate1;
        if (Between(deltaMid, 0, candidate2)) return candidate2;
        return deltaEnd; // 回退
    }

    public static float GetAngle(Vector2 Delta)
    {
        if (Equals(Delta, default)) return 0;
        return (MathF.Atan2(Delta.Y, Delta.X) * (180f / MathF.PI) + 270) % 360;
    }

    public static Vector2 Rotate(Vector2 Delta, float angle)
    {
        float t = angle * MathF.PI / 180f;
        float cos = MathF.Cos(t);
        float sin = MathF.Sin(t);
        return new(Delta.X * cos - Delta.Y * sin, Delta.Y * cos + Delta.X * sin);
    }
}
