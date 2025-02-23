using DV.Wheels;
using HarmonyLib;
using LocoSim.Implementations;
using QuantitiesNet;
using static QuantitiesNet.Quantities;
using static QuantitiesNet.Units;

namespace DvMod.HeadsUpDisplay
{
    internal static class LocoProviders
    {
        public static QuantityPushProvider<Dimensions.Force> adhesionProvider =
            new QuantityPushProvider<Dimensions.Force>("Adhesion");

        public static void Register()
        {
            Registry.Register(new QuantityQueryDataProvider<Dimensions.Force>(
                "Tractive effort",
                car => new Force(car.SimController.drivingForce.generatedForce, Newton)));
            Registry.Register(adhesionProvider);
            Registry.Register(new QuantityQueryDataProvider<Dimensions.Power>(
                "Power",
                car => new Force(car.SimController.drivingForce.generatedForce, Newton) * new Velocity(car.GetForwardSpeed(), KilometersPerHour)));
            Registry.Register(new FloatQueryDataProvider(
                "Slip",
                car => car.SimController.wheelslipController.wheelslip,
                f => $"{f:P1}"));

            // SteamLocoProviders.Register();
        }

        //     public static IEnumerable<MethodBase> TargetMethods()
        //     {
        //         yield return AccessTools.Method(typeof(LocoControllerDiesel), nameof(LocoControllerBase.GetTractionForce));
        //         yield return AccessTools.Method(typeof(LocoControllerShunter), nameof(LocoControllerBase.GetTractionForce));
        //         yield return AccessTools.Method(typeof(LocoControllerSteam), nameof(LocoControllerBase.GetTractionForce));
        //         if (UnityModManager.FindMod("DVCustomCarLoader")?.Assembly is Assembly assembly)
        //         {
        //             var typeNames = new string[]
        //             {
        //                 "DieselElectric.CustomLocoControllerDiesel",
        //                 "Steam.CustomLocoControllerSteam",
        //             };
        //             var methods = typeNames
        //                 .Select(n => assembly.GetType($"DVCustomCarLoader.LocoComponents.{n}"))
        //                 .OfType<Type>()
        //                 .Where(typeof(LocoControllerBase).IsAssignableFrom)
        //                 .Select(t => t.GetMethod("GetTractionForce"))
        //                 .OfType<MethodBase>();
        //             foreach (var method in methods)
        //                 yield return method;
        //         }
        //     }
        // }

        [HarmonyPatch(typeof(WheelslipController), "UpdateWheelslip")]
        public static class UpdateWheelslipPatch
        {
            public static void Prefix(TrainCar ___car, Port ___numberOfPoweredAxlesPort, float adhesionForceLimitPerAxle, bool wheelsliding, float deltaTime)
            {
                adhesionProvider.SetValue(___car, new Force(adhesionForceLimitPerAxle * ___numberOfPoweredAxlesPort?.Value ?? 2));
            }
        }
    }

    // internal static class SteamLocoProviders
    // {
    // public static FloatPushProvider cutoffProvider = new FloatPushProvider("Cutoff", f => $"{f:P0}");

    // public static void Register()
    // {
    // Registry.Register(new QuantityQueryDataProvider<Dimensions.Pressure>("Boiler pressure", car =>
    //     {
    //         var boiler = car.SimController.GetComponentInChildren<LocoSim.Implementations.Boiler>();
    //         if (boiler == null)
    //             return null;
    //         var pressure = boiler.GetSaveStateData().GetDouble("pressure");
    //         if (!pressure.HasValue)
    //             return null;
    //         return new Pressure(pressure.Value, Bar);
    //     }));
    // Registry.Register(cutoffProvider);
    // Registry.Register(new FloatQueryDataProvider(
    //     "Cutoff",
    //     car => (car.SimController.controlsOverrider?.Reverser?.Value - ReverserControl.NEUTRAL_VALUE) * 2,
    //     f => $"{f:P1}"));
    // }

    // [HarmonyPatch(typeof(LocoSim.Implementations.ReciprocatingSteamEngine), "Tick")]
    // public static class TickPatch
    // {
    //     public static bool Postfix(float ___cutoff)
    //     {
    //         cutoffProvider.SetValue(TrainCar.Resolve(__instance.gameObject), ___cutoff);
    //         return true;
    //     }
    // }
    // }
}
