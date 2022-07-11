using UnityEngine.UI;

public static class Extensions
{
    public static void SetAlpha<T>(this T g, float alpha) where T : Graphic
    {
        var color = g.color;
        color.a = alpha;
        g.color = color;
    }
}