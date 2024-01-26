using BTD_Mod_Helper;
using MelonLoader;
using HarmonyLib;
using Il2CppAssets.Scripts.Unity.UI_New.GameOver;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Utils;
using Il2CppAssets.Scripts.Unity;
using BTD_Mod_Helper.Api.ModOptions;

[assembly: MelonInfo(typeof(CCTM.Main), "Continues Cost Too Much", "0.0.37", "Tetricidal")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace CCTM
{
    public class Main : BloonsTD6Mod
    {
        public static int continues;
        public static double[] priceMap = new double[3];

        public static readonly ModSettingDouble FirstContinueCost = 20.0;
        public static readonly ModSettingDouble SecondContinueCost = 30.0;
        public static readonly ModSettingDouble ThirdContinueCost = 50.0;

        // redefine cost of continues
        [HarmonyPatch(typeof(InGame), "GetContinueCost")]
        public class PatchContinueCost
        {
            [HarmonyPostfix]
            public static KonFuze Postfix(KonFuze original)
            {
                if (continues >= 0 && continues < 3)
                {
                    Game.instance.model.continueEnabled = true;
                    original.Write(priceMap[continues]);
                }
                else
                {
                    Game.instance.model.continueEnabled = false;
                    original.Write(9999999);
                }
                return original;
            }
        }

        // display correct continue price
        [HarmonyPatch(typeof(DefeatScreen), "Open")]
        public class PatchDefeatScreen
        {
            [HarmonyPostfix]
            public static void Postfix(DefeatScreen __instance)
            {
                __instance.continueCostTxt.SetText("$" + priceMap[continues]);
                __instance.continuePrice.Write(priceMap[continues]);
            }
        }

        // increment continues counter upon use
        [HarmonyPatch(typeof(InGame), "Continue")]
        public class PatchContinue
        {
            [HarmonyPostfix]
            public static void Postfix() { continues++; }
        }

        // reset counter upon entering a new map
        public override void OnMatchStart()
        {
            base.OnMatchStart();
            continues = 0;
        }

        // initialize mod
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            HarmonyInstance.PatchAll();

            ModSettingDouble[] arr = { FirstContinueCost, SecondContinueCost, ThirdContinueCost };
            for (int i = 0; i < 3; i++)
            {
                arr[i].min = 0;
                arr[i].max = 100000;
                arr[i].onSave = newPrice => priceMap[i] = newPrice;
                priceMap[i] = arr[i];
            };
        }
    }
}
