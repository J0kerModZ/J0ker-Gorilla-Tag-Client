using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using GorillaTagScripts;
using TMPro;
using J0kersClient.NotificationLib;

namespace J0kersClient.Client
{
    [HarmonyPatch(typeof(GorillaLocomotion.Player), "LateUpdate", MethodType.Normal)]
    public class Client : MonoBehaviour
    {
        #region Instance
        public static Client _instance;
        public static Material boardmat;
        public static bool SendOneTime;
        public static bool AllowNoClip = false;
        public static bool AntiNoClip = false;
        private static int pageSize = 4;
        public static int pageNumber = 0;
        public static bool gripDown;
        public static GameObject menu = null;
        public static GameObject canvasObj = null;
        public static string MenuPageActive = "1";
        public static GameObject MenuSelectorSphere = null;
        public static int framePressCooldown = 0;
        public static GameObject pointer = null;
        public static int btnCooldown = 0;
        public static Color coloresp;
        public static GradientColorKey[] ColorStrobe = new GradientColorKey[4];
        static Client OnAwake;
        public static bool DOAntiReport = false;
        public static bool DOAntiReportJR = false;
        public static bool AntiOculusReport = false;
        static bool OnceTrigger;
        public static bool changingPage;
        static bool OnceGrip;
        public static Photon.Realtime.Player PlayerIDHelp;

        public static Client Instance
        {
            get
            {
                return _instance;
            }
        }

        public static bool GrabState(float grabValue, float grabThreshold)
        {
            return grabValue >= grabThreshold;
        }

        internal static Client client
        {
            get
            {
                return OnAwake;
            }
        }
        #endregion

        [Obsolete]
        private static void Prefix()
        {
            try
            {
                #region On Start
                #region Text B4 menu
                if (AHH == "1")
                {
                    DrawTextB4Menu();
                }
                CanvasHolder.transform.localPosition = new Vector3(0f, 0.091f, -0.1f);
                CanvasHolder.transform.localScale = new Vector3(0.0025f, 0.0025f, 0.0025f);
                CanvasHolder.transform.localRotation = Quaternion.Euler(75f, 0f, 0f);
                #endregion
                UpdateBoards();
                NewCam();
                AntiReport();
                ARRejoin();
                PartyStuff();
                AntiModChecker();
                Mods.AntiReportSafty();
                #endregion

                GorillaLocomotion.Player __instance = GorillaLocomotion.Player.Instance;
                List<UnityEngine.XR.InputDevice> list = new List<UnityEngine.XR.InputDevice>();
                if (ControllerInputPoller.instance.controllerType == GorillaControllerType.OCULUS_DEFAULT)
                {
                    if (GrabState(ControllerInputPoller.instance.leftControllerIndexFloat, 0.1f) && menu == null)
                    {
                        Destroy(Loltext);
                        if (MenuPageActive == "1")
                        {
                            Draw();
                        }
                        if (MenuSelectorSphere == null)
                        {
                            MenuSelectorSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            MenuSelectorSphere.name = "GunSphere";
                            MenuSelectorSphere.transform.parent = __instance.rightControllerTransform;
                            MenuSelectorSphere.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                            MenuSelectorSphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                            ColorStrobe[0].color = Color.black;
                            ColorStrobe[0].time = 0f;
                            ColorStrobe[1].color = Color.white;
                            ColorStrobe[1].time = 0.3f;
                            ColorStrobe[2].color = Color.black;
                            ColorStrobe[2].time = 0.6f;


                            ColorChanger colorChanger = MenuSelectorSphere.AddComponent<ColorChanger>();
                            colorChanger.colors = new Gradient
                            {
                                colorKeys = ColorStrobe
                            };
                            colorChanger.Start();
                        }
                    }
                    else if (!GrabState(ControllerInputPoller.instance.leftControllerIndexFloat, 0.1f) && menu != null)
                    {
                        Destroy(MenuSelectorSphere);
                        menu.AddComponent<Rigidbody>();
                        menu.GetComponent<Rigidbody>().useGravity = false;
                        menu.GetComponent<Rigidbody>().velocity = new Vector3(0f, 15f, 0f);
                        UnityEngine.Object.Destroy(menu, 1f);
                        menu = null;
                        MenuSelectorSphere = null;
                    }
                    if (GrabState(ControllerInputPoller.instance.leftControllerIndexFloat, 0.1f) && menu != null)
                    {
                        menu.transform.position = __instance.leftControllerTransform.position;
                        menu.transform.rotation = __instance.leftControllerTransform.rotation;
                    }
                }
                if (ControllerInputPoller.instance.controllerType == GorillaControllerType.INDEX)
                {
                    if (GrabState(ControllerInputPoller.instance.leftControllerIndexFloat, 0.1f) && menu == null)
                    {
                        Destroy(Loltext);
                        if (MenuPageActive == "1")
                        {
                            Draw();
                        }
                        if (MenuSelectorSphere == null)
                        {
                            MenuSelectorSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            MenuSelectorSphere.name = "GunSphere";
                            MenuSelectorSphere.transform.parent = __instance.rightControllerTransform;
                            MenuSelectorSphere.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                            MenuSelectorSphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                            ColorStrobe[0].color = Color.black;
                            ColorStrobe[0].time = 0f;
                            ColorStrobe[1].color = Color.white;
                            ColorStrobe[1].time = 0.3f;
                            ColorStrobe[2].color = Color.black;
                            ColorStrobe[2].time = 0.6f;


                            ColorChanger colorChanger = MenuSelectorSphere.AddComponent<ColorChanger>();
                            colorChanger.colors = new Gradient
                            {
                                colorKeys = ColorStrobe
                            };
                            colorChanger.Start();
                        }
                    }
                    else if (!GrabState(ControllerInputPoller.instance.leftControllerIndexFloat, 0.1f) && menu != null)
                    {
                        Destroy(MenuSelectorSphere);
                        menu.AddComponent<Rigidbody>();
                        menu.GetComponent<Rigidbody>().useGravity = false;
                        menu.GetComponent<Rigidbody>().velocity = new Vector3(0f, 15f, 0f);
                        UnityEngine.Object.Destroy(menu, 1f);
                        menu = null;
                        MenuSelectorSphere = null;
                    }
                    if (GrabState(ControllerInputPoller.instance.leftControllerIndexFloat, 0.1f) && menu != null)
                    {
                        menu.transform.position = __instance.leftControllerTransform.position;
                        menu.transform.rotation = __instance.leftControllerTransform.rotation;
                    }
                }

                #region Buttons
                if (buttonsActive[0] == true)
                {
                    Mods.FlightWithNoClip();
                }

                if (buttonsActive[1] == true)
                {
                    Mods.Rocket();
                }

                if (buttonsActive[2] == true)
                {
                    Mods.FlyAtPlayer();
                }

                if (buttonsActive[3] == true)
                {
                    Mods.CopyMovement();
                }

                if (buttonsActive[4] == true)
                {
                    Mods.RigGun();
                }

                if (buttonsActive[5] == true)
                {
                    Mods.Bees();
                }

                if (buttonsActive[6] == true)
                {
                    Mods.AutoFunnyRun();
                }

                if (buttonsActive[7] == true)
                {
                    Mods.Speed();
                }
                else
                {
                    Mods.SpeedFix();
                }

                if (buttonsActive[8] == true)
                {
                    Mods.NoClip();
                }

                if (buttonsActive[9] == true)
                {
                    Mods.Platforms();
                }

                if (buttonsActive[10] == true)
                {
                    Mods.NoGrav();
                }
                else
                {
                    Mods.NoGravOff();
                }

                if (buttonsActive[11] == true)
                {
                    Mods.Ghost();
                }

                if (buttonsActive[12] == true)
                {
                    Mods.Invis();
                }

                if (buttonsActive[13] == true)
                {
                    Mods.GhostTPose();
                }

                if (buttonsActive[14] == true)
                {
                    GorillaLocomotion.Player.Instance.disableMovement = false;
                }

                if (buttonsActive[15] == true)
                {
                    Mods.NoFinger();
                }

                if (buttonsActive[16] == true)
                {
                    Mods.SpawnMineGhost();
                    buttonsActive[16] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[17] == true)
                {
                    Mods.OpenGates();
                    buttonsActive[17] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[18] == true)
                {
                    Mods.BoneESP();
                }

                if (buttonsActive[19] == true)
                {
                    Mods.RandomGhostCode();
                    buttonsActive[19] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[20] == true)
                {
                    Mods.WaterVomitGun();
                }

                if (buttonsActive[21] == true)
                {
                    Mods.WaterSplashHands();
                }

                if (buttonsActive[22] == true)
                {
                    Mods.WaterSplashBody();
                }

                if (buttonsActive[23] == true)
                {
                    Mods.GrabBeachBall();
                }

                if (buttonsActive[24] == true)
                {
                    Mods.BeachBallScoreSpam();
                }

                if (buttonsActive[25] == true)
                {
                    Mods.WalkOnWater();
                    NotifiLib.SendNotification("Added Default Layer To Water");
                    buttonsActive[25] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[26] == true)
                {
                    Mods.NoWater();
                    NotifiLib.SendNotification("Added TransparentFX Layer To Water");
                    buttonsActive[26] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[27] == true)
                {
                    Mods.AddWater();
                    NotifiLib.SendNotification("Added Water Layer To Water");
                    buttonsActive[27] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[28] == true)
                {
                    Mods.SpeedDiving();
                    NotifiLib.SendNotification("Diving Speed Is Now 50f");
                    buttonsActive[28] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[29] == true)
                {
                    Mods.SpamHand();
                }
                else
                {
                    Mods.NoSpamHand();
                }

                if (buttonsActive[30] == true)
                {
                    Mods.TagAll();
                }

                if (buttonsActive[31] == true)
                {
                    Mods.BraceletSpam();
                }

                if (buttonsActive[32] == true)
                {
                    Mods.DoorSpam();
                }

                if (buttonsActive[33] == true)
                {
                    Mods.WaterBalloon();
                    buttonsActive[33] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[34] == true)
                {
                    Mods.SnowBall();
                    buttonsActive[34] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[35] == true)
                {
                    Mods.LavaRock();
                    buttonsActive[35] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[36] == true)
                {
                    Mods.Mento();
                    buttonsActive[36] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[37] == true)
                {
                    Mods.Present();
                    buttonsActive[37] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[38] == true)
                {
                    Mods.FishFood();
                    buttonsActive[38] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[39] == true)
                {
                    Mods.RGBSnowBalls();
                }

                if (buttonsActive[40] == true)
                {
                    Mods.GrabBug();
                }

                if (buttonsActive[41] == true)
                {
                    Mods.RideBug();
                }

                if (buttonsActive[42] == true)
                {
                    Mods.DisableBug();
                }
                else
                {
                    Mods.EnableBug();
                }

                if (buttonsActive[43] == true)
                {
                    Mods.BugESP();
                    buttonsActive[43] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[44] == true)
                {
                    Mods.GrabBat();
                }

                if (buttonsActive[45] == true)
                {
                    Mods.RideBat();
                }

                if (buttonsActive[46] == true)
                {
                    Mods.DisableBat();
                }
                else
                {
                    Mods.EnableBat();
                }

                if (buttonsActive[47] == true)
                {
                    Mods.BatESP();
                    buttonsActive[47] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[48] == true)
                {
                    Mods.SantaBat();
                    buttonsActive[48] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[49] == true)
                {
                    Mods.GrabShark();
                }

                if (buttonsActive[50] == true)
                {
                    Mods.RideShark();
                }

                if (buttonsActive[51] == true)
                {
                    Mods.SharkGun();
                }

                if (buttonsActive[52] == true)
                {
                    Mods.SharkESP();
                    buttonsActive[52] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[53] == true)
                {
                    Mods.KickAllParty();
                    buttonsActive[53] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[54] == true)
                {
                    Mods.LagAll();
                }

                if (buttonsActive[55] == true)
                {
                    Mods.LagGun();
                }

                if (buttonsActive[56] == true)
                {
                    Mods.RopeGun();
                }

                if (buttonsActive[57] == true)
                {
                    Mods.RopesForward();
                }

                if (buttonsActive[58] == true)
                {
                    Mods.RopesBackwards();
                }

                if (buttonsActive[59] == true)
                {
                    Mods.RopesBounce();
                }

                if (buttonsActive[60] == true)
                {
                    Mods.SpazRopes();
                }

                if (buttonsActive[61] == true)
                {
                    Mods.GrabGliders();
                }

                if (buttonsActive[62] == true)
                {
                    Mods.GliderGun();
                }

                if (buttonsActive[63] == true)
                {
                    Mods.GliderSpaz();
                }

                if (buttonsActive[64] == true)
                {
                    Mods.GliderBlindGun();
                }

                if (buttonsActive[65] == true)
                {
                    Mods.GrabAllBalloons();
                }

                if (buttonsActive[66] == true)
                {
                    Mods.BalloonGun();
                }

                if (buttonsActive[67] == true)
                {
                    Mods.SpazAllBalloons();
                }

                if (buttonsActive[68] == true)
                {
                    Mods.InvisTagAll();
                }

                if (buttonsActive[69] == true)
                {
                    Mods.InvisSelf();
                }

                if (buttonsActive[70] == true)
                {
                    Mods.AmbushParticalSpam();
                }

                if (buttonsActive[71] == true)
                {
                    Mods.AmbushESP();
                }

                if (buttonsActive[72] == true)
                {
                    Mods.RGB();
                }

                if (buttonsActive[73] == true)
                {
                    Mods.FireWork();
                    buttonsActive[73] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[74] == true)
                {
                    Mods.FireWorkVolley();
                    buttonsActive[74] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[75] == true)
                {
                    Mods.TargetHit();
                    buttonsActive[75] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[76] == true)
                {
                    for (int i = 0; i <= 86; i++)
                    {
                        buttonsActive[i] = false;
                    }
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[77] == true)
                {
                    GameObject.Find("LeaveButton").SetActive(false);
                    GameObject.Find("LeaveButtonLine").SetActive(false);
                    GameObject.Find("LeaveButtonText").SetActive(false);

                    GameObject.Find("HomeButton").SetActive(false);
                    GameObject.Find("HomeButtonLine").SetActive(false);
                    GameObject.Find("HomeButtonText").SetActive(false);
                }
                else
                {
                    GameObject.Find("LeaveButton").SetActive(true);
                    GameObject.Find("LeaveButtonLine").SetActive(true);
                    GameObject.Find("LeaveButtonText").SetActive(true);

                    GameObject.Find("HomeButton").SetActive(true);
                    GameObject.Find("HomeButtonLine").SetActive(true);
                    GameObject.Find("HomeButtonText").SetActive(true);
                }

                if (buttonsActive[78] == true)
                {
                    DoFov = false;
                }
                else
                {
                    DoFov = true;
                }

                if (buttonsActive[79] == true)
                {
                    Mods.GetInfo();
                    buttonsActive[79] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[80] == true)
                {
                    NotifiLib.IsEnabled = false;
                    buttonsActive[80] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[81] == true)
                {
                    NotifiLib.IsEnabled = true;
                    buttonsActive[81] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[82] == true)
                {
                    DoRisky = true;
                    NotifiLib.SendNotification("[ <color=red>RISKY MODS ENABLED</color> ] J0KER IS NOT RESPONSABLE FOR ANY BANS!");
                    buttonsActive[82] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[83] == true)
                {
                    DoRisky = false;
                    NotifiLib.SendNotification("[ <color=green>RISKY MODS DISABLED</color> ] ALL RISKY MODS HAVE BEEN DISABLED!");
                    buttonsActive[54] = false;
                    buttonsActive[55] = false;
                    buttonsActive[83] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[84] == true)
                {
                    Mods.SpoofName();
                    buttonsActive[84] = false;
                    Destroy(menu);
                    menu = null;
                    Draw();
                }

                if (buttonsActive[85] == true)
                {
                    DOAntiReport = true;
                }
                else
                {
                    DOAntiReport = false;
                }

                if (buttonsActive[85] == true)
                {
                    DOAntiReportJR = true;
                }
                else
                {
                    DOAntiReportJR = false;
                }


                #endregion
            }
            catch (Exception ex)
            {
                Debug.LogError($"[J0ker Menu Error Log] : {ex}");
            }
        }

        #region ButtonsActive
        static string R = " [<color=red>R</color>]";
        public static string[] buttons = new string[]
        {
            "Fly + Noclip [B]",
            "Rocket [B]",
            "Fly At Player Gun",
            "Copy Player Gun",
            "Rig Gun",
            "Bees [RT]",
            "Auto Run [RT]",
            "Speed Boost",
            "No Clip [T]",
            "Platforms [Secondary]",
            "No Grav",
            "Ghost Monkey [B]",
            "Invisable Rig [RT]",
            "T-Pose [B]",
            "No Tag Freeze",
            "No Finger Movement",
            "Spawn Skeleton [SS] [CAVES]",
            "Open Gates [SS] [Caves]",
            "Bone ESP",
            "Random Ghost Code",
            "Water Vomit Others Gun",
            "Water Splash Hands [L/R] [B]",
            "Water Splash Body [B]",
            "Grab Beach Ball [Beach]",
            "Goal Spam [B] [Beach]",
            "Walk On Water [Beach]",
            "No Water [Beach]",
            "Fix Water [Beach]",
            "Fast Diving [Beach]",
            "Spam Hand Taps",
            "Tag All [B]",
            "Bracelet Spam [G]",
            "Basement Door Spam [B]",
            "Grab WaterBalloon",
            "Grab SnowBall",
            "Grab LavaRocks",
            "Grab Mentos",
            "Grab Present",
            "Grab FishFood",
            "RGB SnowBalls [SS]",
            "Grab Bug [G]",
            "Ride Bug [G]",
            "Disable Bug [SS]",
            "Bug ESP",
            "Grab Bat [G]",
            "Ride Bat [G]",
            "Disable Bat [SS]",
            "Bat ESP",
            "Santa Bat [CS]",
            "Grab Shark [G] [CS]",
            "Ride Shark [G]",
            "Shark Gun [CS]",
            "Shark ESP",
            "Kick All [Party]",
            "Lag All" + R,
            "Lag Gun" + R,
            "Rope Gun" + R,
            "Ropes Forward [G]" + R,
            "Ropes Backward [G]" + R,
            "Ropes Bounce [G]" + R,
            "Ropes Spaz [G]" + R,
            "Grab All Gliders [G] [Clouds]",
            "Glider Gun [Clouds]",
            "Spaz Gliders [B] [Clouds]",
            "Glider Blind Gun [Clouds]",
            "Grab All Balloons [G]",
            "Balloon Gun",
            "Spaz All Balloons [B]",
            "Tag All [Ambush] [B]",
            "Tag Self [Ambush] [B]",
            "Particle Spam [Ambush] [L/R] [B]",
            "ESP [Ambush]",
            "Flash [RGB]",
            "Launch FireWorks [CS] [Beach]",
            "Launch FireWorks Volly [CS] [Beach]",
            "Hit Targets [M]",
            "Disable All Mods",
            "Disable Top Buttons",
            "Disable FOV",
            "Grab All Info",
            "Notifications [Off]",
            "Notifications [On]",
            "<color=red>Risky Mods [On]</color>",
            "<color=green>Risky Mods [Off]</color>",
            "Spoof Name",
            "Anti Report [Disconnect]",
            "Anti Report [Join Random]",
        };

        public static bool?[] buttonsActive = new bool?[]
        {
            false,false,false,false,false, false,false,false,false,false,false, false,false,false,false,false,false, false,false,false,false,false,false, false,false,false,false,false,false, false,false,false,false,false,false, false,false,false,false,false,false, false,false,false,false,false, false,false,false,false,false, false,false,false,false,false, false,false,false,false,false, false,false,false,false,false, false,false,false,false,false, false,false,false,false,false, false,false,false,false,false, false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,
            false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,
        };
        #endregion

        #region Buttons
        private static void AddButton(float offset, string text)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.09f, 0.8f, 0.08f);
            gameObject.transform.localPosition = new Vector3(0.56f, 0f, 0.53f - offset);
            gameObject.AddComponent<BtnCollider>().relatedText = text;
            gameObject.name = text;

            int num = -1;

            for (int i = 0; i < buttons.Length; i++)
            {
                if (text == buttons[i])
                {
                    num = i;
                    break;
                }
            }

            if (buttonsActive[num] == false)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.black;
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = Color.white;
            }

            GameObject gameObject2 = new GameObject();
            gameObject2.transform.parent = canvasObj.transform;

            Text text2 = gameObject2.AddComponent<Text>();
            text2.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text2.text = text;
            text2.fontSize = 1;
            text2.fontStyle = FontStyle.Italic;
            text2.alignment = TextAnchor.MiddleCenter;
            text2.resizeTextForBestFit = true;
            text2.resizeTextMinSize = 0;

            if (buttonsActive[num] == false)
            {
                text2.color = Color.white;
            }

            if (buttonsActive[num] == true)
            {
                text2.color = Color.black;
            }

            RectTransform component = text2.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.2f, 0.03f);
            component.localPosition = new Vector3(0.064f, 0f, 0.211f - offset / 2.55f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        private static void AddPageButtons()
        {
            int num = (buttons.Length + pageSize - 1) / pageSize;
            int num2 = pageNumber + 1;
            int num3 = pageNumber - 1;
            if (num2 > num - 1)
            {
                num2 = 0;
            }
            if (num3 < 0)
            {
                num3 = num - 1;
            }
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.09f, 0.15f, 0.58f);
            gameObject.transform.localPosition = new Vector3(0.56f, 0.5833f, 0.07f);
            gameObject.AddComponent<BtnCollider>().relatedText = "PreviousPage";
            gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            gameObject.name = "back";
            gameObject.GetComponent<Renderer>().material.color = Color.black;
            GameObject gameObject2 = new GameObject();
            gameObject2.transform.parent = canvasObj.transform;
            gameObject2.name = "back";
            Text text = gameObject2.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.text = "<";
            text.fontSize = 1;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.2f, 0.03f);
            component.localPosition = new Vector3(0.064f, 0.175f, 0.04f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            component.localScale = new Vector3(1.3f, 1.3f, 1.3f);


            #region Outlineback

            GameObject OutlineBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(OutlineBack.GetComponent<Rigidbody>());
            OutlineBack.GetComponent<BoxCollider>().isTrigger = true;
            OutlineBack.transform.parent = menu.transform;
            OutlineBack.transform.rotation = Quaternion.identity;
            OutlineBack.transform.localScale = new Vector3(0.08f, 0.16f, 0.59f);
            OutlineBack.transform.localPosition = new Vector3(0.56f, 0.5833f, 0.07f);
            OutlineBack.name = "BackLine";
            OutlineBack.GetComponent<Renderer>().material.shader = Shader.Find("UI/Default");


            #endregion

            GameObject gameObject3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject3.GetComponent<Renderer>().material.color = Color.black;
            Destroy(gameObject3.GetComponent<Rigidbody>());
            gameObject3.GetComponent<BoxCollider>().isTrigger = true;
            gameObject3.transform.parent = menu.transform;
            gameObject3.transform.rotation = Quaternion.identity;
            gameObject3.name = "Next";
            gameObject3.transform.localScale = new Vector3(0.09f, 0.15f, 0.58f);
            gameObject3.transform.localPosition = new Vector3(0.56f, -0.5833f, 0.07f);
            gameObject3.AddComponent<BtnCollider>().relatedText = "NextPage";
            gameObject3.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            GameObject gameObject4 = new GameObject();
            gameObject4.transform.parent = canvasObj.transform;
            gameObject4.name = "Next";
            Text text2 = gameObject4.AddComponent<Text>();
            text2.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text2.text = ">";
            text2.fontSize = 1;
            text2.alignment = TextAnchor.MiddleCenter;
            text2.resizeTextForBestFit = true;
            text2.resizeTextMinSize = 0;
            RectTransform component2 = text2.GetComponent<RectTransform>();
            component2.localPosition = Vector3.zero;
            component2.sizeDelta = new Vector2(0.2f, 0.03f);
            component2.localPosition = new Vector3(0.064f, -0.175f, 0.04f);
            component2.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            component2.localScale = new Vector3(1.3f, 1.3f, 1.3f);

            #region Outline Next

            GameObject OutlineNext = GameObject.CreatePrimitive(PrimitiveType.Cube);
            OutlineNext.GetComponent<Renderer>().material.color = Color.black;
            Destroy(OutlineNext.GetComponent<Rigidbody>());
            OutlineNext.GetComponent<BoxCollider>().isTrigger = true;
            OutlineNext.transform.parent = menu.transform;
            OutlineNext.transform.rotation = Quaternion.identity;
            OutlineNext.name = "Nextline";
            OutlineNext.transform.localScale = new Vector3(0.08f, 0.16f, 0.59f);
            OutlineNext.transform.localPosition = new Vector3(0.56f, -0.5833f, 0.07f);
            OutlineNext.GetComponent<Renderer>().material.shader = Shader.Find("UI/Default");

            #endregion

            #region Leave
            GameObject gameObject5 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject5.GetComponent<Renderer>().material.color = Color.black;
            Destroy(gameObject5.GetComponent<Rigidbody>());
            gameObject5.GetComponent<BoxCollider>().isTrigger = true;
            gameObject5.transform.parent = menu.transform;
            gameObject5.transform.rotation = Quaternion.identity;
            gameObject5.name = "LeaveButton";
            gameObject5.transform.localScale = new Vector3(0.09f, 0.2682f, 0.075f);
            gameObject5.transform.localPosition = new Vector3(0.56f, 0.1924f, 0.5755f);
            gameObject5.AddComponent<BtnCollider>().relatedText = "Leave";
            gameObject5.GetComponent<Renderer>().material.SetColor("_Color", Color.black);


            #region Outline 

            GameObject LeaveOutline = GameObject.CreatePrimitive(PrimitiveType.Cube);
            LeaveOutline.GetComponent<Renderer>().material.shader = Shader.Find("UI/Default");
            Destroy(LeaveOutline.GetComponent<Rigidbody>());
            LeaveOutline.transform.parent = menu.transform;
            LeaveOutline.transform.rotation = Quaternion.identity;
            LeaveOutline.name = "LeaveButtonLine";
            LeaveOutline.transform.localScale = new Vector3(0.08f, 0.2783f, 0.079f);
            LeaveOutline.transform.localPosition = new Vector3(0.56f, 0.1924f, 0.5755f);

            #endregion

            GameObject gameObject6 = new GameObject();
            gameObject6.transform.parent = canvasObj.transform;
            gameObject6.name = "LeaveButtonText";
            Text text3 = gameObject6.AddComponent<Text>();
            text3.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text3.text = "Leave";
            text3.fontSize = 1;
            text3.alignment = TextAnchor.MiddleCenter;
            text3.resizeTextForBestFit = true;
            text3.resizeTextMinSize = 0;
            RectTransform component3 = text3.GetComponent<RectTransform>();
            component3.localPosition = Vector3.zero;
            component3.sizeDelta = new Vector2(0.2f, 0.03f);
            component3.localPosition = new Vector3(0.064f, 0.06f, 0.23f);
            component3.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            component3.localScale = new Vector3(1f, 1f, 1f);
            #endregion

            #region Home
            GameObject gameObject8 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject8.GetComponent<Renderer>().material.color = Color.black;
            Destroy(gameObject8.GetComponent<Rigidbody>());
            gameObject8.GetComponent<BoxCollider>().isTrigger = true;
            gameObject8.transform.parent = menu.transform;
            gameObject8.transform.rotation = Quaternion.identity;
            gameObject8.name = "HomeButton";
            gameObject8.transform.localScale = new Vector3(0.09f, 0.2682f, 0.075f);
            gameObject8.transform.localPosition = new Vector3(0.56f, -0.2076f, 0.5755f);
            gameObject8.AddComponent<BtnCollider>().relatedText = "Home";
            gameObject8.GetComponent<Renderer>().material.SetColor("_Color", Color.black);


            #region Outline 

            GameObject HomeOutline = GameObject.CreatePrimitive(PrimitiveType.Cube);
            HomeOutline.GetComponent<Renderer>().material.shader = Shader.Find("UI/Default");
            Destroy(HomeOutline.GetComponent<Rigidbody>());
            HomeOutline.transform.parent = menu.transform;
            HomeOutline.transform.rotation = Quaternion.identity;
            HomeOutline.name = "HomeButtonLine";
            HomeOutline.transform.localScale = new Vector3(0.08f, 0.2783f, 0.079f);
            HomeOutline.transform.localPosition = new Vector3(0.56f, -0.2076f, 0.5755f);

            #endregion

            GameObject gameObject69 = new GameObject();
            gameObject69.transform.parent = canvasObj.transform;
            gameObject69.name = "HomeButtonText";
            Text text44 = gameObject69.AddComponent<Text>();
            text44.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text44.text = "Home";
            text44.fontSize = 1;
            text44.alignment = TextAnchor.MiddleCenter;
            text44.resizeTextForBestFit = true;
            text44.resizeTextMinSize = 0;
            RectTransform component33 = text44.GetComponent<RectTransform>();
            component33.localPosition = Vector3.zero;
            component33.sizeDelta = new Vector2(0.2f, 0.03f);
            component33.localPosition = new Vector3(0.064f, -0.06f, 0.23f);
            component33.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            component33.localScale = new Vector3(1f, 1f, 1f);
            #endregion
        }

        public static void Toggle(string relatedText)
        {
            int num = (buttons.Length + pageSize - 1) / pageSize;
            if (relatedText == "Leave")
            {
                PhotonNetwork.Disconnect();
                Destroy(menu);
                menu = null;
                Draw();
                return;
            }

            if (relatedText == "Home")
            {
                pageNumber = 0;
                Destroy(menu);
                menu = null;
                Draw();
                return;
            }

            if (relatedText == "NextPage")
            {
                if (pageNumber < num - 1)
                {
                    pageNumber++;
                }
                else
                {
                    pageNumber = 0;
                }
                Destroy(menu);
                menu = null;
                Draw();
                return;
            }
            if (relatedText == "PreviousPage")
            {
                if (pageNumber > 0)
                {
                    pageNumber--;
                }
                else
                {
                    pageNumber = num - 1;
                }
                Destroy(menu);
                menu = null;
                Draw();
                return;
            }
            int num2 = -1;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (relatedText == buttons[i])
                {
                    num2 = i;
                    break;
                }
            }
            if (buttonsActive[num2].HasValue)
            {
                buttonsActive[num2] = !buttonsActive[num2];
                Destroy(menu);
                menu = null;
                Draw();
            }
        }

        #endregion

        #region Draw
        public static void Draw()
        {
            MenuPageActive = "1";

            menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(menu.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(menu.GetComponent<BoxCollider>());
            UnityEngine.Object.Destroy(menu.GetComponent<Renderer>());
            menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f);
            menu.name = "Menu";

            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(gameObject.GetComponent<BoxCollider>());
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.1f, 0.86f, 0.6f);
            gameObject.name = "Menucolor";
            gameObject.transform.position = new Vector3(0.05f, 0f, 0.03f);

            GameObject Outline = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(Outline.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(Outline.GetComponent<BoxCollider>());
            Outline.transform.parent = menu.transform;
            Outline.GetComponent<Renderer>().material.shader = Shader.Find("UI/Default");
            Outline.transform.rotation = Quaternion.identity;
            Outline.transform.localScale = new Vector3(0.09f, 0.87f, -0.61f);
            Outline.name = "MenuOutline";
            Outline.transform.position = new Vector3(0.05f, 0f, 0.03f);

            canvasObj = new GameObject();
            canvasObj.transform.parent = menu.transform;
            canvasObj.name = "j0kercanvas";
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            CanvasScaler canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScaler.dynamicPixelsPerUnit = 1000f;

            GameObject gameObject2 = new GameObject();
            gameObject2.transform.parent = canvasObj.transform;
            gameObject2.name = "Title";
            MenuTitle = gameObject2.AddComponent<Text>();
            MenuTitle.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            if (DoRisky == false)
            {
                MenuTitle.text = "J0ker Client - Page [ <color=cyan>" + pageNumber.ToString() + "</color> ]";
            }
            else if (DoRisky == true)
            {
                MenuTitle.text = "J0ker Client - <color=red>RISKY</color>\n[R] =<color=red> RISKY!!!</color>";
            }
            MenuTitle.name = "MainTitle";
            MenuTitle.fontSize = 1;
            MenuTitle.color = Color.white;
            MenuTitle.fontStyle = FontStyle.Italic;
            MenuTitle.alignment = TextAnchor.MiddleCenter;
            MenuTitle.resizeTextForBestFit = true;
            MenuTitle.resizeTextMinSize = 0;
            RectTransform component = MenuTitle.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.28f, 0.05f);
            component.position = new Vector3(0.06f, 0f, 0.175f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            AddPageButtons();
            string[] array2 = buttons.Skip(pageNumber * pageSize).Take(pageSize).ToArray();
            for (int i = 0; i < array2.Length; i++)
            {
                AddButton((float)i * 0.13f + 0.26f, array2[i]);
            }
        }

        static Text MenuTitle;

        [Obsolete]
        static void DrawTextB4Menu()
        {
            AHH = "2";
            CanvasHolder = new GameObject("CanvasHolder");

            // changed thingys
            CanvasHolder.transform.SetParent(GorillaLocomotion.Player.Instance.leftControllerTransform);

            // adds text
            CanvasHolder.AddComponent<Canvas>();

            //change name of canvas
            CanvasHolder.GetComponent<Canvas>().name = "GorillaUI";

            // load text
            GameObject TextObject = new GameObject("Text");
            TextObject.transform.SetParent(CanvasHolder.transform, false);
            TextObject.AddComponent<CanvasRenderer>();

            //changed text and font
            Loltext = TextObject.AddComponent<Text>();
            Loltext.font = GameObject.Find("motd").GetComponent<Text>().font;
            Loltext.name = "Loltext";
            Loltext.fontSize = 5;
            Loltext.fontStyle = FontStyle.Italic;
            Loltext.alignment = TextAnchor.MiddleCenter;
            Loltext.resizeTextForBestFit = true;
            Loltext.color = Color.white;
            Loltext.resizeTextMinSize = 0;
            Loltext.name = "LolText";
            Loltext.transform.localPosition = new Vector3(0f, 25f, 0f);
            Loltext.transform.rotation = Quaternion.EulerRotation(70f, 0f, 0f);
            Loltext.text = "J0KER CLIENT\n--------------------\n<color=red>OPEN MENU WITH TRIGGER\nMORE AT:\nj0kermodz.lol & discord.gg/j0kermodz</color>\n7/21/2024<color=green> FULLY UD!</color>";
            obj = GameObject.Find("GorillaUI/Text");
            obj.name = "GorillaUI Text";
            obj = GameObject.Find("GorillaUI/GorillaUI Text");

            //Changes text colour
            obj.GetComponent<Text>().color = Color.white;
        }

        static void DrawWatchMenu()
        {
            WatchEnable.SetActive(true); // Enable Watch
            WatchColor.GetComponent<SkinnedMeshRenderer>().material.color = Color.black; // Watch Color
            WatchGlassColor.GetComponent<MeshRenderer>().material.color = Color.white; // Watch Glass Color

            // If your gonna skid this i feel bad for you 💀💀💀
            // This is not done please dont take my idea! The full watch will be here when its done then u can have source!
        }

        static GameObject WatchEnable = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L/LMAEO.LEFT.");
        static GameObject WatchColor = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L/LMAEO.LEFT./ScubaWatch");
        static GameObject WatchGlassColor = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L/LMAEO.LEFT./ScubaWatchBone/ScubaWatchGlass");
        private static GameObject CanvasHolder;
        private static GameObject obj;
        static Text Loltext;

        static void UpdateBoards()
        {
            Client.boardmat = new Material(Shader.Find("GorillaTag/UberShader"));
            Client.boardmat.color = Color.black;
            if (GameObject.Find("Environment Objects/LocalObjects_Prefab").transform.Find("TreeRoom").gameObject.activeSelf)
            {
                int index = 0;
                for (int i = 0; i < GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom").transform.childCount; i++)
                {
                    GameObject AtlasBG = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom").transform.GetChild(i).gameObject;
                    if (AtlasBG.name.Contains("forestatlas"))
                    {
                        index++;
                        if (index == 2)
                        {
                            AtlasBG.GetComponent<Renderer>().material = boardmat;
                        }
                        if (index == 1)
                        {
                            AtlasBG.GetComponent<Renderer>().material = boardmat;
                        }
                    }
                }
                
                if (GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/GorillaComputerObject/ComputerUI/monitor/monitorScreen/Data").GetComponent<Text>().text.Contains("GORILLA OS") || GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/GorillaComputerObject/ComputerUI/monitor/monitorScreen/Data").GetComponent<Text>().text.Contains("POLAR OS"))
                {
                    GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/GorillaComputerObject/ComputerUI/monitor/monitorScreen/Data").GetComponent<Text>().text = "J0KER CLIENT\n\n<color=cyan>discord.gg/j0kermodz\nj0kermodz.lol</color>\n\nFIND J0KER ON YT AND TIKTOK\n\nYT: J0kerModZ\n\nTIKTOK: j0kermodz_real";
                }
                GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/CodeOfConduct_Group/CodeOfConduct").GetComponent<TextMeshPro>().text = "< J0ker Client >";
                GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/motd").GetComponent<Text>().text = "< J0ker Client >";
                GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/CodeOfConduct_Group/CodeOfConduct/COC Text").GetComponent<TextMeshPro>().richText = true;
                GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/CodeOfConduct_Group/CodeOfConduct/COC Text").GetComponent<TextMeshPro>().text = "THAKNS FOR USING J0KER CLIENT!\nTHIS MENU IS FREE; IF YOU BOUGHT IT, YOU GOT SCAMMED!THIS MENU IS OPEN SOURCE!\n\nSITES:\n<color=#00FFFF>j0kermodz.lol & discord.gg/j0kermodz</color>\n\nCREDS:\nLARS: Notification Lib\n\nPLAYER NAME: " + PhotonNetwork.LocalPlayer.NickName + "\nFPS: " + ((int)(1f / Time.smoothDeltaTime)).ToString() + "\nPing: " + PhotonNetwork.GetPing().ToString();
                GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomBoundaryStones/BoundaryStoneSet_Forest/wallmonitorforestbg").GetComponent<Renderer>().material = boardmat;
                GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/GorillaComputerObject/ComputerUI/monitor/monitorScreen").GetComponent<MeshRenderer>().material = boardmat;
                GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/GorillaComputerObject/ComputerUI/monitor").GetComponent<MeshRenderer>().material = boardmat;
                GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/GorillaComputerObject/ComputerUI/keyboard (1)").GetComponent<MeshRenderer>().material = boardmat;
            }
        }


        static void NewCam()
        {
            GameObject cam;
            Camera fovcam;

            if (DoFov == true)
            {
                cam = GameObject.Find("Shoulder Camera");
                fovcam = cam.GetComponent<Camera>();
                if (fovcam.fieldOfView != 135f)
                {
                    fovcam.fieldOfView = 120f;
                    cam.transform.SetParent(Camera.main.transform);
                    cam.transform.localPosition = new Vector3(0f, 0f, 0f);
                    cam.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                }
            }
            else if (DoFov == false)
            {
                cam = GameObject.Find("Shoulder Camera");
                fovcam = cam.GetComponent<Camera>();
                if (fovcam.fieldOfView != 60f)
                {
                    fovcam.fieldOfView = 60f;
                    cam.transform.SetParent(Camera.main.transform);
                }
            }
        }

        public void Awake()
        {
            OnAwake = this;
        }

        public static bool DoRisky = false;
        public static bool DoFov = true;
        public static string AHH = "1";


        #endregion

        #region Room

        public static string oldpartycode = null;
        public static float TimeForParty = 0f;
        public static bool Doing2 = false;
        public static bool Doing = false;
        public static bool PlayerJoin = false;
        public static int PeoplePartying = 0;
        public static float Spppppppppd = 3f;


        static void ARRejoin()
        {
            if (PhotonNetwork.InRoom)
            {
                if (Mods.JoiningRoom != false)
                {
                    Mods.JoiningRoom = false;
                }
            }
            else
            {
                if (Mods.JoiningRoom && Time.time > Mods.RoomJoinDelay)
                {
                    Mods.JoinRandomRoom();
                }
            }
        }

        static void AntiReport()
        {
            if (DOAntiReport == true)
            {
                Mods.AntiReportDisconnect();
            }

            if (DOAntiReportJR == true)
            {
                Mods.AntiReportJR();
            }
        }

        static void PartyStuff()
        {
            // Kick Stuff
            if (PhotonNetwork.InRoom)
            {
                if (Doing)
                {
                    oldpartycode = null;
                    Doing = false;
                    NotifiLib.ClearAllNotifications();
                    NotifiLib.SendNotification("<color=green>SUCCESS</color> Kicked " + PeoplePartying.ToString() + " Party Members");
                    FriendshipGroupDetection.Instance.LeaveParty();
                }
                else
                {
                    if (oldpartycode != null && Time.time > TimeForParty && (PlayerJoin ? PhotonNetwork.PlayerListOthers.Length > 0 : true))
                    {
                        PhotonNetwork.Disconnect();
                        Doing = true;
                    }
                }
            }
            else
            {
                if (Doing)
                {
                    if (oldpartycode != null && Time.time > TimeForParty && (PlayerJoin ? PhotonNetwork.PlayerListOthers.Length > 0 : true))
                    {
                        PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(oldpartycode, JoinType.Solo);
                        TimeForParty = Time.time + (float)Spppppppppd;
                    }
                }
            }
        }

        public static void AntiModChecker()
        {
            ExitGames.Client.Photon.Hashtable propertiesToSet = new ExitGames.Client.Photon.Hashtable
            {
                {
                    "mods",
                    null
                }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToSet, null, null);
        }
    
    

        #endregion

        #region Event Received 
        public static void EventReceived(EventData data)
        {
            try
            {
                if (AntiOculusReport && data.Code == 50)
                {
                    object[] args = (object[])data.CustomData;
                    if ((string)args[0] == PhotonNetwork.LocalPlayer.UserId)
                    {
                        PhotonNetwork.Disconnect();
                        //NotifiLib.SendNotification("Someone attempted to report you using the Oculus menu, you have been disconnected.");
                        Mods.RpcCleanUp();
                    }
                }
            }
            catch { }
        }
    }

    #endregion

    #region TimedBehaviour
    public class TimedBehaviour : MonoBehaviour
    {
        public virtual void Start()
        {
            startTime = Time.time;
        }
        public virtual void Update()
        {
            if (!complete)
            {
                progress = Mathf.Clamp((Time.time - startTime) / duration, 0f, 1.5f);
                if ((double)Time.time - startTime > duration)
                {
                    if (loop)
                    {
                        OnLoop();
                    }
                    else
                    {
                        complete = true;
                    }
                }
            }
        }
    
        public virtual void OnLoop()
        {
            startTime = Time.time;
        }
        public bool complete = false;
        public bool loop = true;
        public float progress = 0f;
        protected bool paused = false;
        protected float startTime;
        protected float duration = 2f;
    }
    public class ColorChanger : TimedBehaviour
    {
        public override void Start()
        {
            base.Start();
            gameObjectRenderer = GetComponent<Renderer>();
        }
        public override void Update()
        {
            base.Update();
            if (colors != null)
            {
                if (timeBased)
                {
                    color = colors.Evaluate(progress);
                }
                gameObjectRenderer.material.color = color;
                gameObjectRenderer.material.SetColor("_Color", color);
                gameObjectRenderer.material.SetColor("_EmissionColor", color);
            }
        }
        public Renderer gameObjectRenderer;
        public Gradient colors = null;
        public Color color;
        public bool timeBased = true;
    }
    #endregion

    #region BtnCollider
    internal class BtnCollider : MonoBehaviour
    {
        private void OnTriggerEnter(Collider collider)
        {
            if (Time.frameCount >= Client.framePressCooldown + 10)
            {
                GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(66, false, 0.1f);
                GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tagHapticStrength / 2, GorillaTagger.Instance.tagHapticDuration / 2);
                if (Client.MenuPageActive == "1")
                {
                    Client.Toggle(relatedText);
                }
                Client.framePressCooldown = Time.frameCount;
            }
        }
        public string relatedText;
    }
    #endregion
}
