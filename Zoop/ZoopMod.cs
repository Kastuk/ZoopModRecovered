using System;
using System.Collections;
using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.UI;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
//using System.Threading.Tasks;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Networking;
using Assets.Scripts.Objects.Electrical;
using static Assets.Scripts.Inventory.InventoryManager; //INTERESTING NEW THING
using Assets.Scripts.Serialization;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Util;
//using Assets.Scripts.Voxel;
using Cysharp.Threading.Tasks;
using Trading;
using UnityEngine.Networking;
using Object = System.Object; //SOMETHING NEW

using CreativeFreedom;


//TODO make it work in authoring mode
// let it ignore collisions if CreativeFreedom mod is enabled.

namespace ZoopMod
{
    [HarmonyPatch(typeof(InventoryManager), "SetMultiConstructorItemPlacement")]
    public class InventoryManagerSetMultiContstruct
    {
        [UsedImplicitly]
        public static void Prefix(InventoryManager __instance, MultiConstructor multiConstructorItem)
        {
            if (ZoopUtility.isZooping)
            {
                //ConsoleWindow.Print("detected: " + multiConstructorItem.PrefabHash);
                ZoopUtility.StartZoop(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(InventoryManager), "SetConstructorItemPlacement")]
    public class InventoryManagerSetContstruct
    {
        [UsedImplicitly]
        public static void Prefix(InventoryManager __instance, Constructor constructorItem)
        {
            if (ZoopUtility.isZooping)
            {
                //ConsoleWindow.Print("detected: " + constructorItem.PrefabHash);
                ZoopUtility.StartZoop(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(InventoryManager), "CancelPlacement")]
    public class InventoryManagerCancelPlacement
    {
        [UsedImplicitly]
        public static void Prefix(InventoryManager __instance)
        {
            if (ZoopUtility.isZooping)
            {
                //Debug.Log("zoop canceled at CancelPlacement");
                ZoopUtility.CancelZoop();
                ZoopUtility.isZoopKeyPressed = false;
            }
        }
    }

    [HarmonyPatch(typeof(InventoryManager), "UpdatePlacement", new Type[] {typeof(Constructor)})]
    public class InventoryManagerUpdatePlacementConstructor
    {
        [UsedImplicitly]
        public static bool Prefix(InventoryManager __instance)
        {
            return !ZoopUtility.isZooping; //false prevents placing down item //NICE CHECK
        }
    }

    [HarmonyPatch(typeof(InventoryManager), "UpdatePlacement", new Type[] {typeof(Structure)})]
    public class InventoryManagerUpdatePlacementStructure
    {
        [UsedImplicitly]
        public static bool Prefix(InventoryManager __instance)
        {
            return !ZoopUtility.isZooping; //false prevents placing down item
        }
    }

    [HarmonyPatch(typeof(InventoryManager), "WaitUntilDone",
        new Type[] {typeof(InventoryManager.DelegateEvent), typeof(float), typeof(Structure)})]
    public class InventoryManagerWaitUntilDone0
    {
        [UsedImplicitly]
        public static void Prefix(InventoryManager __instance, InventoryManager.DelegateEvent onFinished,
            ref float timeToWait,
            Structure structure)
        {
            if (!InventoryManager.IsAuthoringMode)
            {
                if (ZoopUtility.structures.Count <= 0)
                {
                    timeToWait = Math.Min(timeToWait * 1, timeToWait * 10);
                }
                else
                {
                    timeToWait = Math.Min(timeToWait * ZoopUtility.structures.Count, timeToWait * 10); //PROBLEM same time of placement for single pieces after zooping
                }
            }
            else timeToWait = 0f; //try to make it instant for creative tool
        }
    }

    // [HarmonyPatch(typeof(InventoryManager), "WaitUntilDone", new Type[] {typeof(Interactable), typeof(Assets.Scripts.Objects.Thing.DelayedActionInstance), typeof(Interaction)})]
    // public class InventoryManagerWaitUntilDone
    // {
    //     [UsedImplicitly]
    //     public static void Prefix(InventoryManager __instance,  Interactable interactable,
    //         Assets.Scripts.Objects.Thing.DelayedActionInstance interactionInstance,
    //         Interaction interaction)
    //     {
    //         Debug.Log("InventoryManagerWaitUntilDone1");
    //         // // if (ZoopUtility.isZooping)
    //         // // {
    //         // var interactionInstanceDuration = ZoopUtility.structures.Count * InventoryManager.ConstructionCursor.BuildStates[0].Tool.ExitTime;
    //         // Debug.Log("InventoryManagerWaitUntilDone" +interactionInstanceDuration);
    //         // interactionInstance.Duration =  interactionInstanceDuration;
    //         // // }
    //         //
    //         // return false; //false prevents placing down item
    //     }
    // }
    //
    // [HarmonyPatch(typeof(InventoryManager), "WaitUntilDone", new Type[] {typeof(InventoryManager.DelegateEvent), typeof(Assets.Scripts.Objects.Thing.DelayedActionInstance), typeof(int), typeof(int)})]
    // public class InventoryManagerWaitUntilDone2
    // {
    //     [UsedImplicitly]
    //     public static void Prefix(InventoryManager __instance,   InventoryManager.DelegateEvent onFinished,
    //         Assets.Scripts.Objects.Thing.DelayedActionInstance attack,
    //         int actionSoundHash,
    //         int actionCompleteSoundHash)
    //     {
    //         Debug.Log("InventoryManagerWaitUntilDone2");
    //     }
    // }
    // [HarmonyPatch(typeof(InventoryManager), "WaitUntilDone", new Type[] {typeof(InventoryManager.DelegateEvent), typeof(float), typeof(string), typeof(Assets.Scripts.Objects.Item), typeof(int), typeof(int), typeof(string), typeof(bool)})]
    // public class InventoryManagerWaitUntilDone3
    // {
    //     [UsedImplicitly]
    //     public static void Prefix(InventoryManager __instance,  
    //         InventoryManager.DelegateEvent onFinished,
    //         float timeToWait,
    //         string actionName,
    //         Assets.Scripts.Objects.Item item,
    //         int actionSoundHash,
    //         int actionCompleteSoundHash,
    //         string axis = "Secondary",
    //         bool keepAlive = false)
    //     {
    //         Debug.Log("InventoryManagerWaitUntilDone3");
    //     }
    // }
    //
    // [HarmonyPatch(typeof(InventoryManager), "WaitUntilDone", new Type[] {typeof(InventoryManager.DelegateEvent), typeof(float), typeof(string), typeof(Vector3), typeof(Minables)})]
    // public class InventoryManagerWaitUntilDone4
    // {
    //     [UsedImplicitly]
    //     public static void Prefix(InventoryManager __instance, InventoryManager.DelegateEvent onFinished,
    //         float timeToWait,
    //         string actionName,
    //         Vector3 voxelLocalPosition,
    //         Minables minable)
    //     {
    //         // if (ZoopUtility.isZooping)
    //         // {
    //         Debug.Log("InventoryManagerWaitUntilDone4");
    //         // }
    //
    //         // return false; //false prevents placing down item
    //     }
    // }

    [HarmonyPatch(typeof(InventoryManager), "PlacementMode")]
    public class InventoryManagerPlacementMode
    {
        //public static bool CFree = false;
        [UsedImplicitly]
        public static bool Prefix(InventoryManager __instance)
        {
            Type type = Type.GetType("CreativeFreedom.CreativeFreedom, CreativeFreedom");
            if (type != null)
            {
                ZoopPatch.CFree = true;
            }

            bool scrollUp = __instance.newScrollData > 0f;
            bool scrollDown = __instance.newScrollData < 0f;
            ZoopUtility.isZoopKeyPressed = KeyManager.GetButton(ZoopUtility.ZoopHoldKey);
            bool secondary = KeyManager.GetMouseDown("Secondary");
            bool primary = KeyManager.GetMouseDown("Primary");
            bool spec = KeyManager.GetButtonDown(ZoopUtility.ZoopSwitchKey);
            //bool place = KeyManager.GetButton(KeyMap.PrecisionPlace);

            if (ZoopUtility.isZoopKeyPressed && primary || spec)
            {
               // Debug.Log("zoop must start now");
                ZoopUtility.StartZoop(__instance);
            }

            //if (InventoryManager.ConstructionCursor && drop)
            //{
            //    __instance.CancelPlacement();
            //}


            if (primary && ZoopUtility.isZooping && !ZoopUtility.isZoopKeyPressed)
            {
                //Debug.Log("PlacementMode build has error:" + ZoopUtility.HasError);
                if (!ZoopUtility.HasError)
                {
                    //NotAuthoringMode.Completion = true; //try not let original InventoryManager.UsePrimaryComplete override completion for Authoring Tool

                    //CHANGE tried to evade authoring mode check, as zero placement time is it
                    if (!InventoryManager.IsAuthoringMode && (double)InventoryManager.ConstructionCursor.BuildPlacementTime > 0.0)
                    {
                        float num1 = 1f;
                        if ((UnityEngine.Object) InventoryManager.ParentHuman.Suit == (UnityEngine.Object) null)
                            num1 += 0.2f; //whyyy make it longer in suit there...
                        float num2 = Mathf.Clamp(num1, 0.2f, 5f);

                        Type InventoryManagerType = typeof(InventoryManager);
                        var method = InventoryManagerType.GetMethod("WaitUntilDone",
                            BindingFlags.NonPublic | BindingFlags.Instance, null,
                            new Type[] {typeof(InventoryManager.DelegateEvent), typeof(float), typeof(Structure)},
                            null);
                        ZoopUtility.ActionCoroutine = __instance.StartCoroutine((IEnumerator) method.Invoke(__instance,
                            new Object[]
                            {
                                new InventoryManager.DelegateEvent(() => ZoopUtility.BuildZoop(__instance)),
                                InventoryManager.ConstructionCursor.BuildPlacementTime / num2,
                                InventoryManager.ConstructionCursor
                            })
                        );
                    }
                    else
                        ZoopUtility.BuildZoop(__instance);
                }

                return !ZoopUtility.isZooping;
            }

            if (secondary)// || drop)
            {
                //Debug.Log("zoop canceled by rmb");
                ZoopUtility.CancelZoop();
            }

            return !ZoopUtility.isZoopKeyPressed;
        }
    }

    [HarmonyPatch(typeof(ConstructionPanel), "SelectUp")]
    public class ConstructionPanelSelectUp
    {
        [UsedImplicitly]
        public static bool Prefix()
        {
            return !(ZoopUtility.isZoopKeyPressed);
        }
    }

    [HarmonyPatch(typeof(ConstructionPanel), "SelectDown")]
    public class ConstructionPanelSelectDown
    {
        [UsedImplicitly]
        public static bool Prefix()
        {
            return !(ZoopUtility.isZoopKeyPressed);
        }
    }

    [HarmonyPatch(typeof(CursorManager), "SetSelectionColor")]
    public class CursorManagerSetSelectionColor
    {
        [UsedImplicitly]
        public static void Postfix()
        {
            if (ZoopUtility.isZooping)
            {
                CursorManager.CursorSelectionRenderer.material.color =
                    ZoopUtility.color.SetAlpha(InventoryManager.Instance.CursorAlphaInteractable);
            }
        }
    }

    //[HarmonyPatch(typeof(InventoryManager), "IsAuthoringMode", MethodType.Getter)]
    //public class NotAuthoringMode
    //{
    //    public static bool Completion;
    //    [UsedImplicitly]
    //    public static void Postfix(ref bool __result)
    //    {
    //        if (Completion)
    //        {
    //            __result = false;
    //        }
    //    }
    //}

    //[HarmonyPatch(typeof(OnServer), "UseItemPrimaryAuthoring")]
    //public class OnServerUsePrimaryAuthoring
    //{
    //    public static bool Completion;
    //    [UsedImplicitly]
    //    public static bool Prefix()
    //    {
    //        if (Completion)
    //        {
    //            return false;
    //        }
    //        else return true;
    //    }
    //}

    public class ZoopUtility
    {
        public static KeyCode ZoopHoldKey = BindValidate.hold;//KeyCode.LeftShift;
        public static KeyCode ZoopSwitchKey = BindValidate.switcher; //Z (previously QUantityModifier which is C, which overlap smartrotation and bring problems

        public static List<Structure> structures = new List<Structure>();
        public static List<Structure> structuresCacheStraight = new List<Structure>();
        public static List<Structure> structuresCacheCorner = new List<Structure>();

        public static bool isZoopKeyPressed;
        public static bool isZooping;
        private static bool increasingX;
        private static bool increasingY;
        private static bool increasingZ;
        public static int spacing = 1;
        public static int countX;
        public static int countY;
        public static int countZ;
        public static Vector3? startPos;
        public static Vector3? previousCurrentPos;
        public static Color color = Color.green;
        private static CancellationTokenSource _cancellationToken;

        //preferred zoop order is built up by every first detection of a direction
        private static List<ZoopDirection> preferredZoopOrder = new List<ZoopDirection>();
        public static bool HasError;
        public static Coroutine ActionCoroutine { get; set; }


        //PROBLEM in new beta: After start of zooping, construct cursor structure disappear, only empty green cube of smallgrid remain

        public static void StartZoop(InventoryManager inventoryManager)
        {
            //Debug.Log("Trying to start zooping with " + InventoryManager.ConstructionCursor.DisplayName);

            if (IsAllowed(InventoryManager.ConstructionCursor))
            {
               // Debug.Log(InventoryManager.ConstructionCursor.DisplayName + " is allowed for zooping.");
                isZooping = true;
                if (_cancellationToken == null)
                {
                    preferredZoopOrder.Clear();
                    if (InventoryManager.ConstructionCursor != null)
                    {
                        InventoryManager.UpdatePlacement(inventoryManager.ConstructionPanel.Parent.Constructables[0]);
                        startPos = getCurrentMouseGridPosition();
                        //Debug.Log("Construct cursor is not null");
                    }

                    if (startPos != null)
                    {
                        _cancellationToken = new CancellationTokenSource();
                        UniTask.Run(() => ZoopAsync(_cancellationToken.Token, inventoryManager));
                       // Debug.Log("Start pos is not null");
                    }
                }
                else
                {
                   // Debug.Log("zoop canceled at startzoop");
                    CancelZoop();
                }
            }
           // else Debug.Log(InventoryManager.ConstructionCursor.DisplayName + " is not allowed for zooping!");//I want to knwo why it not work in Authoring mode
        }

        private static bool IsAllowed(Structure constructionCursor)
        {
            return constructionCursor is Pipe || constructionCursor is Cable || constructionCursor is Chute;
        }

        public static async UniTask ZoopAsync(CancellationToken cancellationToken,
            InventoryManager inventoryManager)
        {
            //Debug.Log("Async started");
            List<ZoopDirection> zoops = new List<ZoopDirection>();
            if (InventoryManager.ConstructionCursor != null)
                InventoryManager.ConstructionCursor.gameObject.SetActive(false); //it's disabling default hovering piece
            while (cancellationToken != null && !cancellationToken.IsCancellationRequested)
            {
               // Debug.Log("While cycle");
                try
                {
                    //
                    // if (InventoryManager.ConstructionCursor != null &&
                    //     InventoryManager.ConstructionCursor.Renderers != null)
                    //     InventoryManager.ConstructionCursor.Renderers.ForEach(renderer => renderer.SetColor(color));

                    if (startPos.HasValue)
                    {
                        var startPosition = startPos.Value;
                        zoops.Clear();
                        var currentPos = getCurrentMouseGridPosition();
                        if ((!previousCurrentPos.HasValue && currentPos.HasValue) ||
                            (currentPos.HasValue && previousCurrentPos.HasValue &&
                             !currentPos.Equals(previousCurrentPos.Value))) //only if mouse has changed gridposition
                        {
                            var startX = startPosition.x;
                            var startY = startPosition.y;
                            var startZ = startPosition.z;
                            var currentX = (float) Math.Round(currentPos.Value.x * 2f) / 2f;
                            var currentY = (float) Math.Round(currentPos.Value.y * 2f) / 2f;
                            var currentZ = (float) Math.Round(currentPos.Value.z * 2f) / 2f;
                            bool singleItem = startX == currentX && startY == currentY && startZ == currentZ;

                            if (Math.Abs(currentX - startX) > float.Epsilon)
                            {
                                countX = 1 + (int) (Math.Abs(startX - currentX) * 2);
                                increasingX = startX < currentX;
                                updateZoopOrder(ZoopDirection.x);
                                zoops.Add(ZoopDirection.x);
                            }

                            if (Math.Abs(currentY - startY) > float.Epsilon)
                            {
                                countY = 1 + (int) (Math.Abs(startY - currentY) * 2);
                                increasingY = startY < currentY;
                                updateZoopOrder(ZoopDirection.y);
                                zoops.Add(ZoopDirection.y);
                            }

                            if (Math.Abs(currentZ - startZ) > float.Epsilon)
                            {
                                countZ = 1 + (int) (Math.Abs(startZ - currentZ) * 2);
                                increasingZ = startZ < currentZ;
                                updateZoopOrder(ZoopDirection.z);
                                zoops.Add(ZoopDirection.z);
                            }

                            if (singleItem)
                            {
                                countX = 1 + (int) (Math.Abs(startX - currentX) * 2);
                                increasingX = startX < currentX;
                                zoops.Add(ZoopDirection.x);//unused for single item
                            }

                            zoops.Sort(delegate(ZoopDirection a, ZoopDirection b)
                            {
                                return preferredZoopOrder.IndexOf(a) - preferredZoopOrder.IndexOf(b);
                            });
                            BuildStructureList(inventoryManager, zoops);
                            SetConstructorRotation(zoops);

                            float xOffset = 0;
                            float yOffset = 0;
                            float zOffset = 0;
                            int structureCounter = 0;
                            HasError = false;
                            if (structures.Count > 0)
                            {
                                int previousZoopIndex = 0;
                                for (int i = 0; i < zoops.Count; i++)
                                {
                                    if (structureCounter == structures.Count)
                                    {
                                        break;
                                    }

                                    ZoopDirection zoopDirection = zoops[i];
                                    bool increasing = getIncreasingForDirection(zoopDirection);
                                    var zoopCounter = getCountForDirection(zoopDirection);
                                    if (i < zoops.Count - 1)
                                    {
                                        zoopCounter--;
                                    }

                                    for (int zi = 0; zi < zoopCounter; zi++)
                                    {
                                        if (structureCounter == structures.Count)
                                        {
                                            break;
                                        }

                                        spacing = Mathf.Max(spacing, 1);
                                        float minValue = InventoryManager.ConstructionCursor is SmallGrid ? 0.5f : 2f;
                                        float value = increasing ? minValue * spacing : -(minValue * spacing);
                                        switch (zoopDirection)
                                        {
                                            case ZoopDirection.x:
                                                xOffset = zi * value;
                                                break;
                                            case ZoopDirection.y:
                                                yOffset = zi * value;
                                                break;
                                            case ZoopDirection.z:
                                                zOffset = zi * value;
                                                break;
                                        }

                                        if (i > 0 && zi == 0)
                                        {
                                            SetCornerRotation(structures[structureCounter],
                                                zoops[previousZoopIndex],
                                                getIncreasingForDirection(zoops[previousZoopIndex]),
                                                zoopDirection,
                                                increasing);
                                        }
                                        else
                                        {
                                            if (!singleItem)
                                            {
                                                SetStraightRotation(structures[structureCounter], zoopDirection);
                                            }
                                        }

                                        var offset = new Vector3(xOffset, yOffset, zOffset);
                                        structures[structureCounter].GameObject.SetActive(true);
                                        structures[structureCounter].ThingTransformPosition = (startPosition + offset);
                                        structures[structureCounter].Position = (startPosition + offset);
                                        //structures[structureCounter].GridController = GridController.GetController(structures[structureCounter].ThingTransformPosition);
                                        //GridController gcontrol = GridController.GetController(structures[structureCounter].ThingTransformPosition);
                                        //gcontrol.OffsetPosition;
                                        if (!ZoopPatch.CFree)
                                        {
                                            HasError = HasError || !CanConstruct(inventoryManager, structures[structureCounter]);
                                        }
                                        structureCounter++;
                                        if (zi == zoopCounter - 1)
                                        {
                                            switch (zoopDirection)
                                            {
                                                case ZoopDirection.x:
                                                    xOffset = (zi + 1) * value;
                                                    break;
                                                case ZoopDirection.y:
                                                    yOffset = (zi + 1) * value;
                                                    break;
                                                case ZoopDirection.z:
                                                    zOffset = (zi + 1) * value;
                                                    break;
                                            }
                                        }
                                    }

                                    previousZoopIndex = i;
                                }
                            }

                            foreach (Structure structure in structures)
                            {
                                SetColor(inventoryManager, structure, HasError);
                            }
                        }

                        previousCurrentPos = currentPos;
                    }

                    await UniTask.Delay(100, DelayType.Realtime); //run 10x per second
                    //await Task.Delay(100);//Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    // catch exceptions and print to the log
                    Debug.Log(e.Message);
                    Debug.LogException(e);
                }
            }
        }


        private static void SetConstructorRotation(List<ZoopDirection> zoops)
        {
            if (zoops.Count > 0)
            {
                for (int i = zoops.Count - 1; i >= 0; i--)
                {
                    switch (zoops[i])
                    {
                        case ZoopDirection.x:
                            ConstructionCursor.ThingTransformRotation = SmartRotate.RotY.Rotation;
                            break;
                        case ZoopDirection.y:
                            ConstructionCursor.ThingTransformRotation = SmartRotate.RotX.Rotation;
                            break;
                        case ZoopDirection.z:
                            ConstructionCursor.ThingTransformRotation = SmartRotate.RotZ.Rotation;
                            break;
                    }
                    //Debug.Log("SetConstructorRotation for cycle out of switch");
                    //break; //may it help to fix ureachable end of cycle i--
                }
            }
        }

        private static void BuildStructureList(InventoryManager inventoryManager, List<ZoopDirection> zoops)
        {
            structures.Clear();
            structuresCacheStraight.ForEach(structure => structure.GameObject.SetActive(false));
            structuresCacheCorner.ForEach(structure => structure.GameObject.SetActive(false));

            var straight = 0;
            var corners = 0;
            for (int i = 0; i < zoops.Count; i++)
            {
                var zoopDirection = zoops[i];
                var zoopCounter = getCountForDirection(zoopDirection);
                if (i < zoops.Count - 1)
                {
                    zoopCounter--;
                }

                for (int j = 0; j < zoopCounter; j++)
                {
                    if (structures.Count > 0 && j == 0 &&
                        inventoryManager.ConstructionPanel.Parent.Constructables.Count > 1)
                    {
                        AddStructure(inventoryManager.ConstructionPanel.Parent
                                .Constructables, true, corners,
                            inventoryManager); // start with corner on secondary and tertiary zoop directions
                        corners++;
                    }
                    else
                    {
                        AddStructure(inventoryManager.ConstructionPanel.Parent
                            .Constructables, false, straight, inventoryManager);
                        straight++;
                    }
                }
            }
        }

        private static void SetStraightRotation(Structure structure, ZoopDirection zoopDirection)
        {
            switch (zoopDirection)
            {
                case ZoopDirection.x:
                    if (structure is Chute)
                    {
                        structure.ThingTransformRotation =
                            SmartRotate.RotX.Rotation;
                    }
                    else
                    {
                        structure.ThingTransformRotation =
                            SmartRotate.RotY.Rotation;
                    }

                    break;
                case ZoopDirection.y:
                    if (structure is Chute)
                    {
                        structure.ThingTransformRotation =
                            SmartRotate.RotZ.Rotation;
                    }
                    else
                    {
                        structure.ThingTransformRotation =
                            SmartRotate.RotX.Rotation;
                    }

                    break;
                case ZoopDirection.z:
                    if (structure is Chute)
                    {
                        structure.ThingTransformRotation =
                            SmartRotate.RotY.Rotation;
                    }
                    else
                    {
                        structure.ThingTransformRotation =
                            SmartRotate.RotZ.Rotation;
                    }

                    break;
            }
        }

        private static void SetCornerRotation(Structure structure, ZoopDirection zoopDirectionFrom,
            bool increasingFrom, ZoopDirection zoopDirectionTo, bool increasingTo)
        {
            var xOffset = 0.0f;
            var yOffset = 0.0f;
            var zOffset = 0.0f;
            if (structure.GetPrefabName().Equals("StructureCableCorner"))
            {
                xOffset = 180.0f;
            }

            if (structure.GetPrefabName().Equals("StructureChuteCorner"))
            {
                xOffset = -90.0f;
                if (zoopDirectionTo == ZoopDirection.z && zoopDirectionFrom == ZoopDirection.x)
                    yOffset = increasingTo ? -90.0f : 90f;
                else if (zoopDirectionTo == ZoopDirection.x && zoopDirectionFrom == ZoopDirection.z) //good
                    yOffset = increasingFrom ? 90.0f : -90f;
                else
                    yOffset = 180.0f;
            }

            structure.ThingTransformRotation = ZoopUtils.getCornerRotation(zoopDirectionFrom,
                increasingFrom, zoopDirectionTo, increasingTo, xOffset, yOffset, zOffset);
        }


        private static bool getIncreasingForDirection(ZoopDirection zoopDirection)
        {
            switch (zoopDirection)
            {
                case ZoopDirection.x: return increasingX;
                case ZoopDirection.y: return increasingY;
                case ZoopDirection.z: return increasingZ;
                default: return false;
            }
        }

        private static int getCountForDirection(ZoopDirection zoopDirection)
        {
            switch (zoopDirection)
            {
                case ZoopDirection.x: return countX;
                case ZoopDirection.y: return countY;
                case ZoopDirection.z: return countZ;
                default: return 0;
            }
        }

        private static void updateZoopOrder(ZoopDirection direction)
        {
            // add if this direction is not yet in the list
            if (!preferredZoopOrder.Contains(direction))
            {
                preferredZoopOrder.Add(direction);
            }
        }

        private static Vector3? getCurrentMouseGridPosition()
        {
            if (InventoryManager.ConstructionCursor != null)
            {
                var cursorHitPoint = InventoryManager.ConstructionCursor.GetLocalGrid().ToVector3();//InventoryManager.ConstructionCursor.GetLocalGrid(InventoryManager.ConstructionCursor.ThingTransformPosition).ToVector3();
                //var cursorHitPoint = new Vector3(cursorGrid.x, cursorGrid.y, cursorGrid.z); //ADDED
                return cursorHitPoint;
            }

            return null;
        }

        public static void CancelZoop()
        {
           // NotAuthoringMode.Completion = false;
            isZooping = false;
            if (_cancellationToken != null)
            {
                _cancellationToken.Cancel();
                _cancellationToken = null;
                ClearStructureCache();
                startPos = null;
                previousCurrentPos = null;
                //ZoopUtility.structures = new List<Structure>(); 
                ZoopUtility.structures.Clear(); //try to reset a list of structures for single piece placing
                //Structure structure = new Structure();
                //ZoopUtility.structures.Add(structure); //just add single to return vanilla time of cable\pipe\chute placement
            }

            if (InventoryManager.ConstructionCursor != null)
                InventoryManager.ConstructionCursor.gameObject.SetActive(true);
        }


        public static void BuildZoop(InventoryManager IM)
        {
            
            foreach (Structure item in structures)
            {
                if (InventoryManager.ActiveHandSlot.Occupant == null)
                {
                    break;
                }
                //if (ZoopPatch.CFree)
                //{
                //    Grid3 loc = item.GetLocalGrid();
                //    SmallCell cell = item.GridController.GetSmallCell(loc);
                //    if (cell != null)
                //    {
                //        Cable cab = item as Cable;
                //        if (cab && cell.Cable != null && cell.Cable.CustomColor != cab.CustomColor)
                //        {
                //            //cab.WillMergeWhenPlaced = false;
                //            //CUMBERSOME!
                //        }
                //    }
                //}

                UsePrimaryComplete(IM, item);
            }
            
            //Debug.Log("zoop canceled at BuildZoop");
            CancelZoop();
        }

        private static void UsePrimaryComplete(InventoryManager IM, Structure item)
        {
           // DynamicThing occupant0 = item.BuildStates[0].Tool.ToolEntry; //try to evade taking authoring tool as occupant

            int optionIndex = IM.ConstructionPanel.Parent.Constructables.FindIndex(structure =>
            {
                return structure.PrefabName == item.PrefabName;
            });
            //Debug.Log(item.PrefabName + ":" + optionIndex);
            if (GameManager.RunSimulation)
            {
                if (IM.ConstructionPanel.IsVisible)
                    OnServer.UseMultiConstructor(InventoryManager.Parent, IM.ActiveHand.SlotId, IM.InactiveHand.SlotId,
                        item.transform.position, item.transform.rotation, optionIndex,
                        InventoryManager.IsAuthoringMode, InventoryManager.ParentBrain.ClientId,
                        optionIndex); //InventoryManager.IsAuthoringMode
                else
                    OnServer.UseItemPrimary((Assets.Scripts.Objects.Thing) InventoryManager.Parent,
                        IM.ActiveHand.SlotId, item.transform.position, item.transform.rotation,
                        InventoryManager.ParentBrain.ClientId, optionIndex);
            }
            else
            {
                CreateStructureMessage structureMessage = new CreateStructureMessage();
                DynamicThing occupant1 = IM.ActiveHand.Slot.Occupant;//InventoryManager.IsAuthoringMode ? occupant0 : IM.ActiveHand.Slot.Occupant; //IM.ActiveHand.Slot.Occupant
                // ISSUE: explicit non-virtual call
                structureMessage.ConstructorId = occupant1 != null ? (occupant1.ReferenceId) : 0L;
                DynamicThing occupant2 = IM.InactiveHand.Slot.Occupant;// InventoryManager.IsAuthoringMode ? occupant0 : IM.InactiveHand.Slot.Occupant;
                // ISSUE: explicit non-virtual call
                structureMessage.OffhandOccupantReferenceId = occupant2 != null ? (occupant2.ReferenceId) : 0L;
                structureMessage.LocalPosition = item.transform.position.ToGrid();
                structureMessage.Rotation = item.transform.rotation;
                structureMessage.CreatorSteamId = (ulong) InventoryManager.ParentBrain.ReferenceId;
                structureMessage.OptionIndex = optionIndex;
                DynamicThing occupant3 = IM.ActiveHand.Slot.Occupant;// InventoryManager.IsAuthoringMode ? occupant0 : IM.ActiveHand.Slot.Occupant;
                structureMessage.PrefabHash = occupant3 != null ? occupant3.PrefabHash : 0;
                structureMessage.AuthoringMode = InventoryManager.IsAuthoringMode;  //false;//InventoryManager.IsAuthoringMode;
                NetworkClient.SendToServer<CreateStructureMessage>(
                    (MessageBase<CreateStructureMessage>) structureMessage, NetworkChannel.GeneralTraffic);
            }
            
        }
        //NEED TO ADD CHECK FOR CREATIVE FREEDOM MOD, so zooping will be without construction checks
        //InventoryManager.IsAuthoringMode ?
        public static bool CanConstruct(InventoryManager IM, Structure structure) 
        {
            var smallCell = structure.GridController.GetSmallCell(structure.ThingTransformLocalPosition);
            bool invalidStructureExistsOnGrid = smallCell != null &&
                                                ((smallCell.Device != (Object) null &&
                                                  !((structure is Piping pipe && pipe == pipe.IsStraight &&
                                                     smallCell.Device is DevicePipeMounted device &&
                                                     device.contentType == pipe.PipeContentType) ||
                                                    (structure is Cable cable && cable == cable.IsStraight &&
                                                     smallCell.Device is DeviceCableMounted))) ||
                                                 smallCell.Other != (Object) null);

            bool differentEndsCollision = false;
            Type structureType = null;
            if (structure is Piping)
            {
                structureType = typeof(Piping);
            }
            else if (structure is Cable)
            {
                structureType = typeof(Cable);
            }
            else if (structure is Chute)
            {
                structureType = typeof(Chute);
            }

            if (structureType != null)
            {
                MethodInfo method =
                    structureType.GetMethod("_IsCollision", BindingFlags.Instance | BindingFlags.NonPublic);

                differentEndsCollision = smallCell != null && (smallCell.Cable != null) &&
                                         (bool) method.Invoke(structure, new object[] {smallCell.Cable});
                differentEndsCollision |= smallCell != null && smallCell.Pipe != null &&
                                          (bool) method.Invoke(structure, new object[] {smallCell.Pipe});
                differentEndsCollision |= smallCell != null && smallCell.Chute != null &&
                                          (bool) method.Invoke(structure, new object[] {smallCell.Chute});
            }


            bool canConstruct = (!invalidStructureExistsOnGrid && !differentEndsCollision);// || ZoopPatch.CFree;

            if (smallCell != null && smallCell.IsValid() &&
                structure is Piping && smallCell.Pipe is Piping piping)
            {
                int optionIndex = IM.ConstructionPanel.Parent.Constructables.FindIndex(item =>
                {
                    return structure.PrefabName == item.PrefabName;
                });
                MultiConstructor activeHandOccupant = IM.ActiveHand.Slot.Occupant as MultiConstructor;
                Item inactiveHandOccupant = InventoryManager.Parent.Slots[IM.InactiveHand.SlotId].Occupant as Item;
                var canReplace = piping.CanReplace(IM.ConstructionPanel.Parent, inactiveHandOccupant);
                canConstruct &= canReplace.CanConstruct;
            }
            else if (smallCell != null && smallCell.IsValid() && structure is Cable && smallCell.Cable is Cable cable2)
            {
                int optionIndex = IM.ConstructionPanel.Parent.Constructables.FindIndex(item =>
                {
                    return structure.PrefabName == item.PrefabName;
                });
                MultiConstructor activeHandOccupant = IM.ActiveHand.Slot.Occupant as MultiConstructor;
                Item inactiveHandOccupant = InventoryManager.Parent.Slots[IM.InactiveHand.SlotId].Occupant as Item;
                var canReplace = cable2.CanReplace(IM.ConstructionPanel.Parent, inactiveHandOccupant);
                canConstruct &= canReplace.CanConstruct;
            }
            else if (smallCell != null && smallCell.IsValid() && structure is Chute && smallCell.Chute is Chute)
            {
                canConstruct &= false;
            }


            return canConstruct;
        }


        private static void SetColor(InventoryManager IM, Structure structure, bool hasError)
        {
            bool canConstruct = !hasError;
            color = canConstruct ? Color.green : Color.red;
            if (structure is SmallGrid smallGrid)
            {
                List<Connection> list = smallGrid.WillJoinNetwork();
                foreach (Connection openEnd in smallGrid.OpenEnds)
                {
                    if (canConstruct)
                    {
                        Color colorToSet = (list.Contains(openEnd)
                            ? Color.yellow.SetAlpha(IM.CursorAlphaConstructionHelper)
                            : Color.green.SetAlpha(IM.CursorAlphaConstructionHelper));
                        foreach (ThingRenderer renderer in structure.Renderers)
                        {
                            if (renderer.HasRenderer())
                            {
                                renderer.SetColor(colorToSet);
                            }
                        }

                        foreach (Connection end in ((SmallGrid) structure).OpenEnds)
                        {
                            end.HelperRenderer.material.color = colorToSet;
                        }
                    }
                    else
                    {
                        foreach (ThingRenderer renderer in structure.Renderers)
                        {
                            if (renderer.HasRenderer())
                                renderer.SetColor(Color.red.SetAlpha(IM.CursorAlphaConstructionHelper));
                        }

                        foreach (Connection end in ((SmallGrid) structure).OpenEnds)
                        {
                            end.HelperRenderer.material.color = Color.red.SetAlpha(IM.CursorAlphaConstructionHelper);
                        }
                    }
                }

                color = ((canConstruct && list.Count > 0) ? Color.yellow : color);
            }

            color.a = IM.CursorAlphaConstructionMesh;
            structure.Wireframe.BlueprintRenderer.material.color = color;
            //may it affect end structure color at collided pieces and merge same colored cables?
        }

        public static void ClearStructureCache()
        {
            foreach (Structure structure in structuresCacheStraight)
            {
                structure.gameObject.SetActive(false);
                MonoBehaviour.Destroy(structure);
            }

            structuresCacheStraight.Clear();

            foreach (Structure structure in structuresCacheCorner)
            {
                structure.gameObject.SetActive(false);
                MonoBehaviour.Destroy(structure);
            }

            structuresCacheCorner.Clear();
        }

        public static void AddStructure(List<Structure> Constructables, bool corner, int index, InventoryManager IM)
        {
            if (InventoryManager.ActiveHandSlot.Occupant is Stackable Constructor)
            {
                if (Constructor.Quantity > structures.Count)
                {
                    MakeItem(Constructables, corner, index, IM);
                }
            }
            else if (InventoryManager.ActiveHandSlot.Occupant is AuthoringTool)
            {
                MakeItem(Constructables, corner, index, IM);
            }
        }

        private static void MakeItem(List<Structure> Constructables, bool corner, int index, InventoryManager IM)
        {
            if (!corner && structuresCacheStraight.Count > index)
            {
                structures.Add(structuresCacheStraight[index]);
            }
            else if (corner && structuresCacheCorner.Count > index)
            {
                structures.Add(structuresCacheCorner[index]);
            }
            else
            {
                var structure = corner ? Constructables[1] : Constructables[0];
                if (structure == null)
                {
                    return;
                }

                Structure structureNew =
                    MonoBehaviour.Instantiate(InventoryManager.GetStructureCursor(structure.PrefabName));
                if (structureNew != null)
                {
                    structureNew.gameObject.SetActive(true);
                    structures.Add(structureNew);
                    if (corner)
                    {
                        structuresCacheCorner.Add(structureNew);
                    }
                    else
                    {
                        structuresCacheStraight.Add(structureNew);
                    }
                }
            }
        }
    }
}