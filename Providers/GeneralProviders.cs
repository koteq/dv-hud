using QuantitiesNet;
using QuantitiesNet.Dimensions;
using static QuantitiesNet.Units;
using UnityEngine;
using System.Linq;

namespace DvMod.HeadsUpDisplay
{
    public static class GeneralProviders
    {
        // U+2007 FIGURE SPACE
        // U+002B PLUS SIGN
        // U+2212 MINUS SIGN
        private const string GradeFormat = "\u002b0.0' %';\u22120.0' %'";

        public static void Register()
        {
            Registry.Register(new FloatQueryDataProvider(
                "Altitude",
                car => car.transform.position.y - 110f,
                f => $"{f:F1} m"));

            Registry.Register(new FloatQueryDataProvider(
                "Speed",
                car => Mathf.Abs(car.GetForwardSpeed()) * 3.6f,
                f => $"{f:F1} km/h"));

            Registry.Register(new FloatQueryDataProvider(
                "Grade",
                car => {
                    var inclination = car.transform.localEulerAngles.x;
                    inclination = inclination > 180 ? 360f - inclination : -inclination;
                    return Mathf.Tan(inclination * Mathf.PI / 180) * 100;
                },
                f => f.ToString(GradeFormat)));

            Registry.Register(new FloatQueryDataProvider(
                "Brake pipe",
                car => car.brakeSystem?.brakePipePressure,
                f => $"{f:F2} bar"));

            Registry.Register(new QuantityQueryDataProvider<Velocity>(
                "SpeedQ",
                car => new QuantitiesNet.Quantities.Velocity(car.GetForwardSpeed(), Meter / Second)));
            
            if (UnitRegistry.Default.TryGetUnits<QuantitiesNet.Dimensions.Velocity>(out var velocityUnits))
            {
                var powerUnit = velocityUnits.First();
                Main.DebugLog($"Setting default unit {powerUnit}");
                UnitRegistry.Default.SetPreferredUnit(velocityUnits.First());
            }
        }
    }
}