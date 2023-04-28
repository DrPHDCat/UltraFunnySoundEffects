using BepInEx;
using UnityEngine;
using System.IO;
using HarmonyLib;

namespace VineBoomAndMetalPipe
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public AssetBundle assetBundle;
        public static Plugin Instance;
        public AudioClip vineBoom;
        public AudioClip metalPipe;
        private void Awake()
        {
            Instance = this;
            // Plugin startup logic
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            assetBundle = AssetBundle.LoadFromFile(Path.Combine(Paths.PluginPath, "VineBoomAndMetalPipe\\Assets\\soundbundle"));
            vineBoom = assetBundle.LoadAsset<AudioClip>("VineBoom");
            metalPipe = assetBundle.LoadAsset<AudioClip>("MetalPipe");
            Harmony har = new Harmony(MyPluginInfo.PLUGIN_GUID);
            har.PatchAll();
            if (metalPipe != null)
            {
                Logger.LogInfo("amogus");
            }
        }
    }
    [HarmonyPatch(typeof(Punch), "Start")]
    static public class AssignSound
    {
        [HarmonyPostfix]
        static public void Postfix(Punch __instance)
        {
            __instance.heavyHit.clip = VineBoomAndMetalPipe.Plugin.Instance.vineBoom;
            GameObject soundPlayer = new GameObject();
            GameObject soundPlayerInst = Object.Instantiate(soundPlayer, __instance.transform.position, Quaternion.identity, __instance.transform);
            soundPlayerInst.AddComponent(typeof(AudioSource));
            soundPlayerInst.GetComponent<AudioSource>().playOnAwake = false;
            soundPlayerInst.name = "SoundPlayer";
        }
    }
    [HarmonyPatch(typeof(Punch), "Parry")]
    static public class MetalPipePlay
    {
        [HarmonyPostfix]
        static public void Postfix(Punch __instance)
        {
            for (int i = 0; i < __instance.transform.childCount; i++)
            {
                if (__instance.transform.GetChild(i).name == "SoundPlayer")
                {
                    __instance.transform.GetChild(i).GetComponent<AudioSource>().clip = VineBoomAndMetalPipe.Plugin.Instance.metalPipe;
                    __instance.transform.GetChild(i).GetComponent<AudioSource>().volume = 0.5f;
                    __instance.transform.GetChild(i).GetComponent<AudioSource>().Play();
                }
            }
        }
    }
    [HarmonyPatch(typeof(Punch), "BlastCheck")]
    static public class VineBoomPlay
    {
        [HarmonyPrefix]
        static public bool Prefix(Punch __instance, ref GameObject ___camObj, ref bool ___holdingInput, ref GameObject ___blastWave)
        {
            if (MonoSingleton<InputManager>.Instance.InputSource.Punch.IsPressed)
            {
                ___holdingInput = false;
                __instance.anim.SetTrigger("PunchBlast");
                Vector3 position = MonoSingleton<CameraController>.Instance.GetDefaultPos() + MonoSingleton<CameraController>.Instance.transform.forward * 2f;
                if (Physics.Raycast(MonoSingleton<CameraController>.Instance.GetDefaultPos(), MonoSingleton<CameraController>.Instance.transform.forward, out var hitInfo, 2f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies)))
                {
                    position = hitInfo.point - ___camObj.transform.forward * 0.1f;
                }

                Object.Instantiate(___blastWave, position, MonoSingleton<CameraController>.Instance.transform.rotation);
                __instance.heavyHit.Play();
            }
            return false;
        }
    }
}
