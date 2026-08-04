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
}
