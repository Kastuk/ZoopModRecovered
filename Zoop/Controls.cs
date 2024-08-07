using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using HarmonyLib;
using UnityEngine;
using Assets.Scripts.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using InputSystem;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using InputSystem;
using UnityEngine;


namespace Zoop
{
    class Controls
    {
        /* Custom shortcut key binding injection is done after KeyManager.SetupKeyBindings() method is 
     * called; this way, we can get our custom new bindings saved/load by the game during the 
     * controls initialisation without needing any extra file access.
     */
        [HarmonyPatch(typeof(KeyManager), "SetupKeyBindings")]
        class ShortcutInjectBindingGroup
        {
            static void Postfix()
            {
                // We need to add a custom control group for the keys to be attached to, and create 
                // the Lookout reference.
                UnityEngine.Debug.Log("Adding custom Shortcuts group");
                ControlsGroup controlsGroup1 = new ControlsGroup("Zoop");
                KeyManager.AddGroupLookup(controlsGroup1);

                // We will add the custom keys with default values to the KeyItem list using our new
                // created control group, however this method -due to accesibility of the class method-
                // will change the current ControlGroup name.

                KeyManager.AddKey("Zoop hold", KeyCode.Z, controlsGroup1, false);
                KeyManager.AddKey("Zoop switch", KeyCode.Mouse2, controlsGroup1, false);

                //ShortcutInjectBindingGroup.AddKey("Zoop hold", KeyCode.Z, controlsGroup1, false);
                //ShortcutInjectBindingGroup.AddKey("Zoop switch", KeyCode.Mouse2, controlsGroup1, false);


                // TODO ADD other tools
                //ShortcutInjectBindingGroup.AddKey("Weapon", KeyCode.None, controlsGroup1, false);
                //ShortcutInjectBindingGroup.AddKey("Water", KeyCode.None, controlsGroup1, false);


                // We need to restore the name of the control group back to its correct string
                //controlsGroup1.Name = "Zoop";
                //if (KeyManager.OnControlsChanged != null)
                //{
                //  KeyManager.OnControlsChanged();
                //}
                ControlsAssignment.RefreshState();
            }

            /* Custom method to add keys to a ControlGroup. We 'hijack' the control group lookup function 
             * that will also save the name of they key in the list for us
             */
            private static void AddKey(string assignmentName,
                KeyCode keyCode,
                ControlsGroup controlsGroup,
                bool hidden = false
            )
            {
                //KeyManager._controlsGroupLookup[assignmentName] = controlsGroup;
                //KeyItem keyItem = new KeyItem(assignmentName, keyCode, hidden);
                //KeyManager.KeyItemLookup[assignmentName] = keyItem;
                //if (!KeyManager.AllKeys.Contains(keyItem))
                //{
                //    KeyManager.AllKeys.Add(keyItem);
                //}


                // This is just because of the accessibility to change the assigned name, we use 
                // the control group for that.
                //controlsGroup.Name = assignmentName;
                KeyManager.AddGroupLookup(controlsGroup);

                // Now Create the key, add its looup string name, and save it in the allkeys list, to ensure
                // is being saved/load by the game config initialisation function.
                KeyItem keyItem = new KeyItem(assignmentName, keyCode, hidden);
                KeyManager.KeyItemLookup.Add(assignmentName, keyItem);
                KeyManager.AllKeys.Add(keyItem);
            }
        }
    }
}
