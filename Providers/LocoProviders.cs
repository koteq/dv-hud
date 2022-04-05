using DV;
using HarmonyLib;
using QuantitiesNet.Quantities;
using static QuantitiesNet.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityModManagerNet;

namespace DvMod.HeadsUpDisplay
{
    internal static class LocoProviders
    {
        public static QuantityPushProvider<QuantitiesNet.Dimensions.Force> tractiveEffortProvider =
            new QuantityPushProvider<QuantitiesNet.Dimensions.Force>("Tractive effort");

        public static QuantityPushProvider<QuantitiesNet.Dimensions.Force> adhesionProvider =
            new QuantityPushProvider<QuantitiesNet.Dimensions.Force>("Adhesion");

        public static QuantityPushProvider<QuantitiesNet.Dimensions.Power> indicatedPowerProvider =
            new QuantityPushProvider<QuantitiesNet.Dimensions.Power>("Power");

        public static void Register()
        {
            Registry.Register(tractiveEffortProvider);
            Registry.Register(adhesionProvider);
            Registry.Register(indicatedPowerProvider);
            Registry.Register(new FloatQueryDataProvider(
                "Slip",
                car => car.GetComponent<DrivingForce>()?.wheelslip,
                f => $"{f:P1}"));

            SteamLocoProviders.Register();
        }

        [HarmonyPatch]
        public static class GetTractionForcePatch
        {
            public static void Postfix(LocoControllerBase __instance, float __result)
            {
                if (!AppUtil.IsPaused)
                {
                    tractiveEffortProvider.SetValue(__instance.train, new Force(__result, Newton));
                    indicatedPowerProvider.SetValue(__instance.train, new Force(__result, Newton) * new Velocity(__instance.GetSpeedKmH(), Kilometer / Hour));
                }
            }

            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(LocoControllerDiesel), nameof(LocoControllerBase.GetTractionForce));
                yield return AccessTools.Method(typeof(LocoControllerShunter), nameof(LocoControllerBase.GetTractionForce));
                yield return AccessTools.Method(typeof(LocoControllerSteam), nameof(LocoControllerBase.GetTractionForce));
                if (UnityModManager.FindMod("DVCustomCarLoader")?.Assembly is Assembly assembly && assembly != null)
                {
                    var typeNames = new string[]
                    {
                        "DieselElectric.CustomLocoControllerDiesel",
                        "Steam.CustomLocoControllerSteam",
                    };
                    var methods = typeNames
                        .Select(n => assembly.GetType($"DVCustomCarLoader.LocoComponents.{n}"))
                        .OfType<Type>()
                        .Where(typeof(LocoControllerBase).IsAssignableFrom)
                        .Select(t => t.GetMethod("GetTractionForce"))
                        .OfType<MethodBase>();
                    foreach (var method in methods)
                        yield return method;
                }
            }
        }

        [HarmonyPatch(typeof(DrivingForce), "UpdateWheelslip")]
        public static class UpdateWheelslipPatch
        {
            public static void Postfix(DrivingForce __instance, Bogie bogie)
            {
                var car = bogie.Car;
                adhesionProvider.SetValue(car, new Force(__instance.tractionForceWheelslipLimit * car.Bogies.Length));
            }
        }
    }

    internal static class SteamLocoProviders
    {
        public static FloatPushProvider cutoffProvider = new FloatPushProvider("Cutoff", f => $"{f:P0}");

        public static void Register()
        {
            Registry.Register(cutoffProvider);
        }

        [HarmonyPatch(typeof(SteamLocoSimulation), "SimulateTick")]
        public static class SimulateTickPatch
        {
            public static bool Prefix(SteamLocoSimulation __instance)
            {
                cutoffProvider.SetValue(TrainCar.Resolve(__instance.gameObject), __instance.cutoff.value);
                return true;
            }
        }
    }
}