using UnityEngine;

// Single place that defines the weight unit.
// Item weights are stored as integers: 1 point = 0.01 kg (so 100 points = 1 kg).
public static class WeightFormat
{
    public const float UnitsPerKg = 100f;

    public static string ToKg(int weight)
    {
        return (weight / UnitsPerKg).ToString("0.##") + " kg";
    }
    public static string Tolb(int weight)
    {
        return (weight / UnitsPerKg).ToString("0.##") + " lbs";
    }
}
