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
    [BepInPlugin("Elmotrix.jixxed.Kastuk.ZoopMod", "Zoop Mod", "2024.06.08")]
    // [BepInProcess("rocketstation.exe")]
    //[BepInDependency("CreativeFreedom", BepInDependency.DependencyFlags.SoftDependency)]
    //that dependency not works with StationeersMods
    public class ZoopPatch : BaseUnityPlugin
    {
        public static KeyCode ZoopHold;// = KeyCode.LeftShift;
        public static KeyCode ZoopSwitch;// = KeyCode.Z;

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
                var harmony = new Harmony("Elmotrix.jixxed.Kastuk.Zoop");
                harmony.PatchAll();
                this.Log("Patch succeeded");
                KeyManager.OnControlsChanged += new KeyManager.Event(ControlsChangedEvent);
                //ZoopConfig.Bind(this);
                //BindValidate.ParseKey();

                //KeyManager.OnControlsChanged += new KeyManager.Event(ControlsChangedEvent);
            }

            catch (Exception e)
            {
                this.Log("Patch Failed");
                this.Log(e.ToString());
            }
        }
        public static ZoopPatch Instance;
        public static bool CFree = false;

        //private void ControlsChangedEvent()
        //{
        //    UnityEngine.Debug.Log("Keybinding Controls changed");

        //    // Backpack keybindings
        //    ZoopHold = KeyManager.GetKey("Zoop hold");
        //    ZoopSwitch = KeyManager.GetKey("Zoop switch");
        //}


        /* Track current player keybinding selection, event trigger after any 
             * keybinding change.
             */
        private void ControlsChangedEvent()
        {
            UnityEngine.Debug.Log("Keybinding Controls changed");

            ZoopHold = KeyManager.GetKey("Zoop Hold");
            ZoopSwitch = KeyManager.GetKey("Zoop Switch");

        }
    }


    //public static class ZoopConfig //Copy from Beef's Game Fixes
    //{
    //    public static string HoldZoop = "LeftShift";
    //    public static string SwitchZoop = "Z";

    //    public static void Bind(ZoopPatch zm)
    //    {
    //        HoldZoop = zm.Config.Bind("Keys", "Hold zoop key", "LeftShift", "In construction mode, hold this key then click primary action to start zooping. Possible key names is like LeftControl or V.").Value;
    //        SwitchZoop = zm.Config.Bind("Keys", "Switch zoop key", "Z", "In construction mode. press this key once to start zooping.").Value;
    //    }
    //}




    //public static class BindValidate
    //{
    //    //public static KeyCode hold = KeyCode.LeftShift;
    //    //public static KeyCode switcher = KeyCode.Z;

    //    public static void ParseKey()
    //    {
    //        KeyCode key1 = ZoopPatch.ZoopHold;
    //        KeyCode key2 = ZoopPatch.ZoopSwitch;
    //        try
    //        {
    //            key1 = (KeyCode)Enum.Parse(typeof(KeyCode), ZoopConfig.HoldZoop);
    //        }
    //        catch (Exception)
    //        {
    //            Debug.Log("Wrong KeyCode name in Zoop config: " + ZoopConfig.HoldZoop + ". Return to default LeftShift key");
    //            ZoopConfig.HoldZoop = "LeftShift";
    //        }
    //        ZoopPatch.ZoopHold = key1;
    //        try
    //        {
    //            key2 = (KeyCode)Enum.Parse(typeof(KeyCode), ZoopConfig.SwitchZoop);
    //        }
    //        catch (Exception)
    //        {
    //            Debug.Log("Wrong KeyCode name in Zoop config: " + ZoopConfig.SwitchZoop + ". Return to default Z key");
    //            ZoopConfig.SwitchZoop = "Z";
    //        }
    //        ZoopPatch.ZoopSwitch = key2;
    //    }
}

