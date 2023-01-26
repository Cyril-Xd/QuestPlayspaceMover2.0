#region
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

namespace QuestPlayspaceMover
{
    public static class ModInfo
    {
        public const string Name = "QuestPlayspaceMover";
        public const string Description = "PlayspaceMover to oculus Quest2";
        public const string Author = "Rafa (original for Desktop)/Solexid(fixes for quest)";
        public const string Company = "";
        public const string Version = "1.0.0";
        public const string DownloadLink = "";
    }

    public class Main : MelonMod
    {
        #region Settings


        #endregion

        public override void OnApplicationStart()
        {


            MelonCoroutines.Start(WaitInitialization());
            MelonCoroutines.Start(waitforui());
        }




        private OVRCameraRig Camera;
        private OVRInput.Controller LastPressed;
        private Vector3 startingOffset;
        private Vector3 StartPosition;
        public static GameObject UserInterfaceObj = null;
        public int leftspeed = 5;
        public int rightspeed = 5;

        private IEnumerator WaitInitialization()
        {
            // Wait for the VRCUiManager
            while (VRCUiManager.prop_VRCUiManager_0 == null)
            {
                yield return new WaitForFixedUpdate();
                MelonLogger.Warning("-------------------------------------------------------------------------------------------------------");
            }


            var objects = UnityEngine.Object.FindObjectsOfType(UnhollowerRuntimeLib.Il2CppType.Of<OVRCameraRig>());

            MelonLogger.Warning(objects.Count);
            if (objects != null && objects.Length > 0)
            {

                Camera = objects[0].TryCast<OVRCameraRig>();
                StartPosition = Camera.trackingSpace.localPosition;
                yield break;
            }
            OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.High;
            OVRManager.useDynamicFixedFoveatedRendering = true;
            startingOffset = new Vector3(0, 0, 0);
            MelonLogger.Error("OVRCameraRig not found, this mod only work on Oculus! If u are using SteamVR, use the OVR Advanced Settings!");
        }
        protected IEnumerator waitforui()
        {
            while (UserInterfaceObj == null)
            {
                GameObject[] Objects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                foreach (GameObject obj in Objects)
                {
                    if (obj.name.Contains("UserInterface"))
                    {
                        UserInterfaceObj = obj;
                        yield return null;
                    }
                }
                yield return new WaitForSeconds(2);
            }
            while (UserInterfaceObj.transform.Find("Canvas_QuickMenu(Clone)").gameObject.activeSelf == false) yield return new WaitForEndOfFrame();
            new WaitForSeconds(0.7f); // Waits to Prevent Breakage
            MenuStart();
            yield break; // Breaks Statement
        }

        public override void OnUpdate()
        {
            if (Camera == null)
            {

                return;
            }

            if ((HasDoubleClicked(OVRInput.Button.PrimaryThumbstick, 0.25f) || HasDoubleClicked(OVRInput.Button.SecondaryThumbstick, 0.25f)))
            {

                Camera.trackingSpace.localPosition = StartPosition;
                return;
            }

            bool isLeftPressed = IsKeyJustPressed(OVRInput.Button.PrimaryThumbstick);
            bool isRightPressed = IsKeyJustPressed(OVRInput.Button.SecondaryThumbstick);
            if (isLeftPressed || isRightPressed)
            {



                if (isLeftPressed)
                {
                    startingOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);

                    LastPressed = OVRInput.Controller.LTouch;
                }
                else if (isRightPressed)
                {
                    startingOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);

                    LastPressed = OVRInput.Controller.RTouch;
                }
            }



            bool leftTrigger = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch);
            bool rightTrigger = OVRInput.Get(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);

            if (leftTrigger && LastPressed == OVRInput.Controller.LTouch)
            {
                Vector3 currentOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                Vector3 calculatedOffset = (startingOffset * 1) - (currentOffset * leftspeed);

                startingOffset = currentOffset;
                Camera.trackingSpace.localPosition += calculatedOffset;

            }

            if (rightTrigger && LastPressed == OVRInput.Controller.RTouch)
            {
                Vector3 currentOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                Vector3 calculatedOffset = (startingOffset * 5) - (currentOffset * rightspeed); ;
                startingOffset = currentOffset;
                Camera.trackingSpace.localPosition += calculatedOffset;

            }



        }

        public void MenuStart()
        {
            new Wings("Playspace +", "Wing_Left", () =>
            {
               leftspeed = leftspeed + 1;
            });
            new Wings("Playspace -", "Wing_Left", () =>
            {
               leftspeed = leftspeed - 1;
            });
            new Wings("Playspace +", "Wing_Right", () =>
            {
               rightspeed = rightspeed + 1;
            });
            new Wings("Playspace +", "Wing_Right", () =>
            {
               rightspeed = rightspeed - 1;
            });
        }
      

        private static readonly Dictionary<OVRInput.Button, bool> PreviousStates = new Dictionary<OVRInput.Button, bool>
        {
            { OVRInput.Button.PrimaryThumbstick, false }, { OVRInput.Button.SecondaryThumbstick, false }
        };

        private static bool IsKeyJustPressed(OVRInput.Button key)
        {
       
            if (!PreviousStates.ContainsKey(key))
            {
                PreviousStates.Add(key, false);
            }

            return PreviousStates[key] = OVRInput.Get(key, OVRInput.Controller.Touch) && !PreviousStates[key];
        }

        private static readonly Dictionary<OVRInput.Button, float> lastTime = new Dictionary<OVRInput.Button, float>();

        // Thanks to Psychloor!
        // https://github.com/Psychloor/DoubleTapRunner/blob/master/DoubleTapSpeed/Utilities.cs#L30
        public static bool HasDoubleClicked(OVRInput.Button keyCode, float threshold)
        {
            if (!OVRInput.GetDown(keyCode, OVRInput.Controller.Touch))
            {
                return false;
            }

            if (!lastTime.ContainsKey(keyCode))
            {
                lastTime.Add(keyCode, Time.time);
            }

            if (Time.time - lastTime[keyCode] <= threshold)
            {
                lastTime[keyCode] = threshold * 2;
                return true;
            }

            lastTime[keyCode] = Time.time;
            return false;
        }
    }
    public class Wings

    {
        public Wings(string name, string side, Action onClick)
        {
            var toinst = Main.UserInterfaceObj.transform.Find("Canvas_QuickMenu(Clone)/CanvasGroup/Container/Window/" + side + "/Container/InnerContainer/WingMenu/ScrollRect/Viewport/VerticalLayoutGroup/Button_Emotes");
            var inst = GameObject.Instantiate(toinst, toinst.parent).gameObject;
            var txt = inst.transform.Find("Container/Text_QM_H3").GetComponent<TMPro.TextMeshProUGUI>();
            txt.richText = true;
            txt.text = name;
            GameObject.DestroyImmediate(inst.transform.Find("Container/Icon").gameObject);
            var btn = inst.GetComponent<UnityEngine.UI.Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(new Action(onClick));
        }
    }
}
