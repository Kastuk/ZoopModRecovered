using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.UI;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace ZoopMod
{

    //Original creators: Elmotrix & jixxed
    [BepInPlugin("Elmotrix.jixxed.Kastuk.ZoopMod", "Zoop Mod", "2024.04.01")]
    // [BepInProcess("rocketstation.exe")]
    //[BepInDependency("CreativeFreedom", BepInDependency.DependencyFlags.SoftDependency)]
    //that dependency not works with StationeersMods
    public class ZoopPatch : BaseUnityPlugin
    {
        public void Log(string line)
        {
            Debug.Log("[Zoop Mod]: " + line);
        }

        //Originial creators: Elmotrix & jixxed
        private void Awake()
        {
            //foreach (var plugin in Chainloader.PluginInfos)
            //{
            //    var metadata = plugin.Value.Metadata;
            //    if (metadata.GUID.Equals("CreativeFreedom"))
            //    {
            //        // found it
            //        this.Log("Creative Freedom is detected. No more collision checks for zooping.");
            //        CFree = true;
            //        break;
            //    }
            //}

            ZoopPatch.Instance = this;
            this.Log("Hello World");
            try
            {
                var harmony = new Harmony("net.jixxed.stationeers.Zoop");
                harmony.PatchAll();
                this.Log("Patch succeeded");
                ZoopConfig.Bind(this);
                BindValidate.ParseKey();
            }

            catch (Exception e)
            {
                this.Log("Patch Failed");
                this.Log(e.ToString());
            }
        }
        public static ZoopPatch Instance;
        public static bool CFree = false;
    }

    public static class ZoopConfig //Copy from Beef's Game Fixes
    {
        public static string HoldZoop = "LeftShift";
        public static string SwitchZoop = "Z";

        public static void Bind(ZoopPatch zm)
        {
            HoldZoop = zm.Config.Bind("Keys", "Hold zoop key", "LeftShift", "In construction mode, hold this key then click primary action to start zooping. Possible key names is like LeftControl or V.").Value;
            SwitchZoop = zm.Config.Bind("Keys", "Switch zoop key", "Z", "In construction mode. press this key once to start zooping.").Value;
        }
    }

    public static class BindValidate
    {
        public static KeyCode hold = KeyCode.LeftShift;
        public static KeyCode switcher = KeyCode.Z;

        public static void ParseKey ()
        {
            KeyCode key1 = hold;
            KeyCode key2 = switcher;
            try
            {
                key1 = (KeyCode)Enum.Parse(typeof(KeyCode), ZoopConfig.HoldZoop);
            }
            catch (Exception)
            {
                Debug.Log("Wrong KeyCode name in Zoop config: " + ZoopConfig.HoldZoop + ". Return to default LeftShift key");
                ZoopConfig.HoldZoop = "LeftShift";
            }
            hold = key1;
            try
            {
                key2 = (KeyCode)Enum.Parse(typeof(KeyCode), ZoopConfig.SwitchZoop);
            }
            catch (Exception)
            {
                Debug.Log("Wrong KeyCode name in Zoop config: " + ZoopConfig.SwitchZoop + ". Return to default Z key");
                ZoopConfig.SwitchZoop = "Z";
            }
            switcher = key2;
        }
    }
}
