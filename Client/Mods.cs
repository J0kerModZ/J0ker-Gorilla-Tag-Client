using GorillaLocomotion.Gameplay;
using GorillaNetworking;
using GorillaTag;
using GorillaTagScripts;
using HarmonyLib;
using J0kersClient.NotificationLib;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace J0kersClient.Client
{
    internal class Mods : MonoBehaviour
    {
        // Out bc its in like all the regions
        static VRRig VrRigPlayers = null;
        static bool CopyPlayer;
        static GameObject GunSphere;
        static int[] bones = { 4, 3, 5, 4, 19, 18, 20, 19, 3, 18, 21, 20, 22, 21, 25, 21, 29, 21, 31, 29, 27, 25, 24, 22, 6, 5, 7, 6, 10, 6, 14, 6, 16, 14, 12, 10, 9, 7 };

        #region Get Stuff

        public static VRRig GetVRRigFromPlayer(Player p)
        {
            return GorillaGameManager.instance.FindPlayerVRRig(p);
        }

        public static Photon.Realtime.Player GetPlayerFromVRRig(VRRig p)
        {
            return p.Creator;
        }

        public static VRRig RandomVRRig(bool includeSelf)
        {
            Photon.Realtime.Player randomPlayer;
            if (includeSelf)
            {
                randomPlayer = PhotonNetwork.PlayerList[UnityEngine.Random.Range(0, PhotonNetwork.PlayerList.Length - 1)];
            }
            else
            {
                randomPlayer = PhotonNetwork.PlayerListOthers[UnityEngine.Random.Range(0, PhotonNetwork.PlayerListOthers.Length - 1)];
            }
            return GetVRRigFromPlayer(randomPlayer);
        }


        public static PhotonView PhotonViewVRRig(VRRig p)
        {
            return (PhotonView)Traverse.Create(p).Field("photonView").GetValue();
        }

        public static VRRig FindVRRigForPlayer(Photon.Realtime.Player player)
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isOfflineVRRig && vrrig.GetComponent<PhotonView>().Owner == player)
                {
                    return vrrig;
                }
            }
            return null;
        }

        public static VRRig GetClosestVRRig()
        {
            float num = float.MaxValue;
            VRRig outRig = null;
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, vrrig.transform.position) < num && vrrig != GorillaTagger.Instance.offlineVRRig)
                {
                    num = Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, vrrig.transform.position);
                    outRig = vrrig;
                }
            }
            return outRig;
        }

        public static void TakeOwnership(PhotonView view)
        {
            if (!view.AmOwner)
            {
                try
                {
                    view.OwnershipTransfer = OwnershipOption.Request;
                    view.OwnerActorNr = PhotonNetwork.LocalPlayer.ActorNumber;
                    view.ControllerActorNr = PhotonNetwork.LocalPlayer.ActorNumber;
                    view.RequestOwnership();
                    view.TransferOwnership(PhotonNetwork.LocalPlayer);

                    RequestableOwnershipGuard OwnerGard = view.GetComponent<RequestableOwnershipGuard>();
                    if (OwnerGard != null)
                    {
                        view.GetComponent<RequestableOwnershipGuard>().actualOwner = PhotonNetwork.LocalPlayer;
                        view.GetComponent<RequestableOwnershipGuard>().currentOwner = PhotonNetwork.LocalPlayer;
                        view.GetComponent<RequestableOwnershipGuard>().RequestTheCurrentOwnerFromAuthority();
                        view.GetComponent<RequestableOwnershipGuard>().TransferOwnership(PhotonNetwork.LocalPlayer);
                        view.GetComponent<RequestableOwnershipGuard>().TransferOwnershipFromToRPC(PhotonNetwork.LocalPlayer, view.GetComponent<RequestableOwnershipGuard>().ownershipRequestNonce, default(PhotonMessageInfo));
                    }
                    RpcCleanUp();
                }
                catch { }
            }
            else
            {
                view.OwnershipTransfer = OwnershipOption.Fixed;
            }
        }

        #endregion

        #region Gorilla Tagger Mods

        static float beesDelay;

        static void BallsOnHands()
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(gameObject.GetComponent<Rigidbody>());
            Object.Destroy(gameObject.GetComponent<SphereCollider>());
            gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            gameObject.transform.position = GorillaTagger.Instance.leftHandTransform.position;
            GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(gameObject2.GetComponent<Rigidbody>());
            Object.Destroy(gameObject2.GetComponent<SphereCollider>());
            gameObject2.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            gameObject2.transform.position = GorillaTagger.Instance.rightHandTransform.position;
            gameObject.GetComponent<Renderer>().material.color = UnityEngine.Color.black;
            gameObject2.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
            Object.Destroy(gameObject, Time.deltaTime);
            Object.Destroy(gameObject2, Time.deltaTime);
        }

        static void LineToRig()
        {
            GameObject gameObject = new GameObject("Line");
            LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startColor = UnityEngine.Color.white;
            lineRenderer.endColor = UnityEngine.Color.black;
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.SetPosition(0, GorillaLocomotion.Player.Instance.rightControllerTransform.position);
            lineRenderer.SetPosition(1, GorillaTagger.Instance.offlineVRRig.transform.position);
            lineRenderer.material.shader = Shader.Find("GUI/Text Shader");
            UnityEngine.Object.Destroy(lineRenderer, Time.deltaTime);
            UnityEngine.Object.Destroy(gameObject, Time.deltaTime);
        }

        public static void LongArms()
        {
            GorillaTagger.Instance.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }

        public static void LongArmsFix()
        {
            GorillaTagger.Instance.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        public static void FixHead()
        {
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.x = 0f;
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.y = 0f;
            GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.z = 0f;
        }

        public static void FlyAtPlayer()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up, out var Ray);
                GameObject NewPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                NewPointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                NewPointer.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
                NewPointer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                NewPointer.transform.position = CopyPlayer ? VrRigPlayers.transform.position : Ray.point;
                UnityEngine.Object.Destroy(NewPointer.GetComponent<BoxCollider>());
                UnityEngine.Object.Destroy(NewPointer.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(NewPointer.GetComponent<Collider>());
                UnityEngine.Object.Destroy(NewPointer, Time.deltaTime);

                GameObject line = new GameObject("Line");
                LineRenderer liner = line.AddComponent<LineRenderer>();
                liner.material.shader = Shader.Find("GUI/Text Shader");
                liner.startColor = UnityEngine.Color.white;
                liner.endColor = UnityEngine.Color.black;
                liner.startWidth = 0.025f;
                liner.endWidth = 0.025f;
                liner.positionCount = 2;
                liner.useWorldSpace = true;
                liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                liner.SetPosition(1, CopyPlayer ? VrRigPlayers.transform.position : Ray.point);
                UnityEngine.Object.Destroy(line, Time.deltaTime);

                if (CopyPlayer && VrRigPlayers != null)
                {
                    BallsOnHands();
                    GorillaTagger.Instance.offlineVRRig.enabled = false;

                    Vector3 look = VrRigPlayers.transform.position - GorillaTagger.Instance.offlineVRRig.transform.position;
                    look.Normalize();

                    Vector3 position = GorillaTagger.Instance.offlineVRRig.transform.position + (look * (30f * Time.deltaTime));

                    GorillaTagger.Instance.offlineVRRig.transform.position = position;
                    try
                    {
                        GorillaTagger.Instance.myVRRig.transform.position = position;
                    }
                    catch { }

                    GorillaTagger.Instance.offlineVRRig.transform.LookAt(VrRigPlayers.transform.position);
                    try
                    {
                        GorillaTagger.Instance.myVRRig.transform.LookAt(VrRigPlayers.transform.position);
                    }
                    catch { }

                    GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.rotation = GorillaTagger.Instance.offlineVRRig.transform.rotation;
                    GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.position = GorillaTagger.Instance.offlineVRRig.transform.position + (GorillaTagger.Instance.offlineVRRig.transform.right * -1f);
                    GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position = GorillaTagger.Instance.offlineVRRig.transform.position + (GorillaTagger.Instance.offlineVRRig.transform.right * 1f);

                    GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.rotation = GorillaTagger.Instance.offlineVRRig.transform.rotation;
                    GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.rotation = GorillaTagger.Instance.offlineVRRig.transform.rotation;
                }
                if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
                {
                    VRRig possibly = Ray.collider.GetComponentInParent<VRRig>();
                    if (possibly && possibly != GorillaTagger.Instance.offlineVRRig)
                    {
                        CopyPlayer = true;
                        VrRigPlayers = possibly;
                    }
                }
            }
            else
            {
                if (CopyPlayer)
                {
                    CopyPlayer = false;
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }
        }

        public static void CopyMovement()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up, out var Ray);
                GameObject NewPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                NewPointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                NewPointer.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
                NewPointer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                NewPointer.transform.position = CopyPlayer ? VrRigPlayers.transform.position : Ray.point;
                UnityEngine.Object.Destroy(NewPointer.GetComponent<BoxCollider>());
                UnityEngine.Object.Destroy(NewPointer.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(NewPointer.GetComponent<Collider>());
                UnityEngine.Object.Destroy(NewPointer, Time.deltaTime);

                GameObject line = new GameObject("Line");
                LineRenderer liner = line.AddComponent<LineRenderer>();
                liner.material.shader = Shader.Find("GUI/Text Shader");
                liner.startColor = UnityEngine.Color.white;
                liner.endColor = UnityEngine.Color.black;
                liner.startWidth = 0.025f;
                liner.endWidth = 0.025f;
                liner.positionCount = 2;
                liner.useWorldSpace = true;
                liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                liner.SetPosition(1, CopyPlayer ? VrRigPlayers.transform.position : Ray.point);
                UnityEngine.Object.Destroy(line, Time.deltaTime);

                if (CopyPlayer && VrRigPlayers != null)
                {
                    BallsOnHands();
                    GorillaTagger.Instance.offlineVRRig.enabled = false;

                    GorillaTagger.Instance.offlineVRRig.transform.position = VrRigPlayers.transform.position;
                    GorillaTagger.Instance.offlineVRRig.transform.rotation = VrRigPlayers.transform.rotation;

                    GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.position = VrRigPlayers.head.rigTarget.transform.position;
                    GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.rotation = VrRigPlayers.head.rigTarget.transform.rotation;

                    GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.position = VrRigPlayers.leftHandTransform.transform.position;
                    GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position = VrRigPlayers.rightHandTransform.transform.position;

                    GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.rotation = VrRigPlayers.leftHandTransform.transform.rotation;
                    GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.rotation = VrRigPlayers.rightHandTransform.transform.rotation;
                }
                if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
                {
                    VRRig possibly = Ray.collider.GetComponentInParent<VRRig>();
                    if (possibly && possibly != GorillaTagger.Instance.offlineVRRig)
                    {
                        CopyPlayer = true;
                        VrRigPlayers = possibly;
                    }
                }
            }
            else
            {
                if (CopyPlayer)
                {
                    CopyPlayer = false;
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }
        }

        public static void RigGun()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out var hitinfo);
                GunSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GunSphere.transform.position = hitinfo.point;
                GunSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                GunSphere.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                GunSphere.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
                GameObject.Destroy(GunSphere.GetComponent<BoxCollider>());
                GameObject.Destroy(GunSphere.GetComponent<Rigidbody>());
                GameObject.Destroy(GunSphere.GetComponent<Collider>());

                GameObject line = new GameObject("Line");
                LineRenderer liner = line.AddComponent<LineRenderer>();
                liner.material.shader = Shader.Find("GUI/Text Shader");
                liner.startColor = UnityEngine.Color.white;
                liner.endColor = UnityEngine.Color.black;
                liner.startWidth = 0.025f;
                liner.endWidth = 0.025f;
                liner.positionCount = 2;
                liner.useWorldSpace = true;
                liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                liner.SetPosition(1, GunSphere.transform.position);
                UnityEngine.Object.Destroy(line, Time.deltaTime);

                if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
                {
                    GameObject.Destroy(GunSphere, Time.deltaTime);
                    GunSphere.GetComponent<Renderer>().material.color = UnityEngine.Color.black;
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.position = GorillaTagger.Instance.offlineVRRig.transform.position + (GorillaTagger.Instance.offlineVRRig.transform.right * -1f);
                    GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position = GorillaTagger.Instance.offlineVRRig.transform.position + (GorillaTagger.Instance.offlineVRRig.transform.right * 1f);
                    GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.rotation = GorillaTagger.Instance.offlineVRRig.transform.rotation;
                    // Fix For The hands rotation
                    Quaternion handRotation = GorillaTagger.Instance.offlineVRRig.transform.rotation;

                    Quaternion leftHandRotation = handRotation * Quaternion.Euler(0f, -1f, 0f) * Quaternion.Euler(0f, 0f, 80f); // 80f bc 90f makes FindPiece go down a bit
                    GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.rotation = leftHandRotation;

                    Quaternion rightHandRotation = handRotation * Quaternion.Euler(0f, 1f, 0f) * Quaternion.Euler(0f, 0f, -80f); // ^^ Same here but negative ^^
                    GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.rotation = rightHandRotation;
                    GorillaTagger.Instance.offlineVRRig.transform.position = GunSphere.transform.position + new Vector3(0f, 1f, 0f);
                }

            }
            if (GunSphere != null)
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
                GameObject.Destroy(GunSphere, Time.deltaTime);
            }
        }

        public static void Bees()
        {
            if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
            {
                BallsOnHands();
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                LineToRig();

                if (Time.time > Mods.beesDelay)
                {
                    VRRig vrrig4 = GorillaParent.instance.vrrigs[Random.Range(0, GorillaParent.instance.vrrigs.Count - 1)];
                    GorillaTagger.Instance.offlineVRRig.transform.position = vrrig4.transform.position + new Vector3(0f, 1f, 0f);
                    GorillaTagger.Instance.myVRRig.transform.position = vrrig4.transform.position + new Vector3(0f, 1f, 0f);
                    GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.position = vrrig4.transform.position;
                    GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position = vrrig4.transform.position;
                    Mods.beesDelay = Time.time + 0.777f;
                }
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }

        public static void Ghost()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                BallsOnHands();
                LineToRig();
                GorillaTagger.Instance.offlineVRRig.enabled = false;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }

        public static void GhostTPose()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                BallsOnHands();
                LineToRig();
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.position = GorillaTagger.Instance.offlineVRRig.transform.position + (GorillaTagger.Instance.offlineVRRig.transform.right * -1f);
                GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position = GorillaTagger.Instance.offlineVRRig.transform.position + (GorillaTagger.Instance.offlineVRRig.transform.right * 1f);
                GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.rotation = GorillaTagger.Instance.offlineVRRig.transform.rotation;

                // Fix For The hands rotation
                Quaternion handRotation = GorillaTagger.Instance.offlineVRRig.transform.rotation;

                Quaternion leftHandRotation = handRotation * Quaternion.Euler(0f, -1f, 0f) * Quaternion.Euler(0f, 0f, 80f); // 80f bc 90f makes FindPiece go down a bit
                GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.rotation = leftHandRotation;

                Quaternion rightHandRotation = handRotation * Quaternion.Euler(0f, 1f, 0f) * Quaternion.Euler(0f, 0f, -80f); // ^^ Same here but negative ^^
                GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.rotation = rightHandRotation;
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }

        static void TposeSpin()
        {
            BallsOnHands();
            LineToRig();
            GorillaTagger.Instance.offlineVRRig.enabled = false;
            Quaternion rotation = Quaternion.Euler(0f, 40 * Time.deltaTime, 0f);
            GorillaTagger.Instance.offlineVRRig.transform.rotation *= rotation;
            GorillaTagger.Instance.offlineVRRig.transform.position = new Vector3(-66.058f, 16.3358f, -82.357f);
            GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.position = GorillaTagger.Instance.offlineVRRig.transform.position + (GorillaTagger.Instance.offlineVRRig.transform.right * -1f);
            GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position = GorillaTagger.Instance.offlineVRRig.transform.position + (GorillaTagger.Instance.offlineVRRig.transform.right * 1f);
            GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.rotation = GorillaTagger.Instance.offlineVRRig.transform.rotation;
            Quaternion handRotation = GorillaTagger.Instance.offlineVRRig.transform.rotation;
            Quaternion leftHandRotation = handRotation * Quaternion.Euler(0f, -1f, 0f) * Quaternion.Euler(0f, 0f, 80f);
            GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.rotation = leftHandRotation;
            Quaternion rightHandRotation = handRotation * Quaternion.Euler(0f, 1f, 0f) * Quaternion.Euler(0f, 0f, -80f);
            GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.rotation = rightHandRotation;
        }

        public static void Invis()
        {
            if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
            {
                BallsOnHands();
                GorillaTagger.Instance.offlineVRRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.transform.position = new Vector3(100000f, 100000f, 100000f);
            }
            else
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
            }
        }

        #endregion

        #region Room Mods

        public static void GetInfo()
        {
            string text = "=======================Player IDs!=========================";

            foreach (Player player in PhotonNetwork.PlayerListOthers)
            {
                if (PhotonNetwork.InRoom || PhotonNetwork.InLobby)
                {
                    string playerName = player.NickName;
                    string playerId = player.UserId;
                    string roomId = PhotonNetwork.CurrentRoom.Name;
                    string isMaster = player.IsMasterClient.ToString();

                    text += $"\nPlayer Name: {playerName}, Player ID: {playerId}, Room ID: {roomId}, Master: {isMaster}";
                }
            }

            text += "\n==========================================================\n";
            File.AppendAllText("PLAYER INFO.txt", text);
            GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/UI/motd/motdtext").GetComponent<Text>().text = "PLAYFAB ID: " + PlayFabSettings.TitleId.ToString() + "\nPhoton Real Time: " + PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime.ToString() + "\nPhoton Voice: " + PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice.ToString() + "\nRegion: " + PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion + "\nSDK Version: " + PlayFabSettings.VersionString.ToString() + "\nAUTH URL: " + PlayFabAuthenticator.Playfab_Auth_API + "\nGAME VERSION: " + GorillaComputer.instance.version + "\nMASTER: " + PhotonNetwork.MasterClient;
            NotifiLib.SendNotification("<color=green>INFO PULLED</color> <color=gray>CHECK GTAG FILE AND MOTD</color>");
        }
        public static float notiDelay = 0f;

        public static void RandomGhostCode()
        {
            string[] roomNames =
            {
                "666",
                "DAISY09",
                "PBBV",
                "SREN17",
                "SREN18",
                "AI",
                "GHOST",
                "J3VU",
                "RUN",
                "BOT",
                "TIPTOE",
                "SPIDER",
                "STATUE",
                "BANSHEE",
                "RABBIT",
                "ERROR",
                "ISEEYOUBAN"
            };
            int num = new System.Random().Next(roomNames.Length);
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomNames[num], JoinType.Solo);
        }

        /* 
         Sorry Got Rid Of The Other Ones Just Taking Up Space 
         But If You Want The Code Here It Is: 

         PhotonNetworkController.Instance.AttemptToJoinSpecificRoom("Room Name Here", JoinType.Solo);
        */
        #endregion

        #region Plats

        private static GameObject PlatFL;
        private static GameObject PlatFR;
        private static bool PlatLSpawn;
        private static bool PlatRpawn;

        public static void PlatL()
        {
            // Plat L Game OBJS
            PlatFL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(PlatFL.GetComponent<Rigidbody>());
            Object.Destroy(PlatFL.GetComponent<BoxCollider>());
            Object.Destroy(PlatFL.GetComponent<Renderer>());
            PlatFL.transform.localScale = new Vector3(0.25f, 0.3f, 0.25f);

            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.transform.parent = PlatFL.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.1f, 1f, 1f);
            gameObject.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
            gameObject.GetComponent<Renderer>().material.color = UnityEngine.Color.black;
            gameObject.transform.position = new Vector3(0.02f, 0f, 0f);
        }

        public static void PlatR()
        {
            // Plat R Game OBJS
            PlatFR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(PlatFR.GetComponent<Rigidbody>());
            Object.Destroy(PlatFR.GetComponent<BoxCollider>());
            Object.Destroy(PlatFR.GetComponent<Renderer>());

            PlatFR.transform.localScale = new Vector3(0.25f, 0.3f, 0.25f);
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.transform.parent = PlatFR.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.1f, 1f, 1f);
            gameObject.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
            gameObject.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
            gameObject.transform.position = new Vector3(-0.02f, 0f, 0f);
        }

        public static void Platforms()
        {
            List<UnityEngine.XR.InputDevice> list = new List<UnityEngine.XR.InputDevice>();

            #region Quest
            if (ControllerInputPoller.instance.controllerType == GorillaControllerType.OCULUS_DEFAULT)
            {
                if (ControllerInputPoller.instance.leftControllerSecondaryButton && PlatFL == null)
                {
                    PlatL();
                }
                if (ControllerInputPoller.instance.rightControllerSecondaryButton && PlatFR == null)
                {
                    PlatR();
                }

                // Tansform Game Obj To Hands
                if (ControllerInputPoller.instance.leftControllerSecondaryButton && PlatFL != null && !PlatLSpawn)
                {
                    PlatFL.transform.position = GorillaLocomotion.Player.Instance.leftControllerTransform.position;
                    PlatFL.transform.rotation = GorillaLocomotion.Player.Instance.leftControllerTransform.rotation;
                    PlatLSpawn = true;
                }
                if (ControllerInputPoller.instance.rightControllerSecondaryButton && PlatFR != null && !PlatRpawn)
                {
                    PlatFR.transform.position = GorillaLocomotion.Player.Instance.rightControllerTransform.position;
                    PlatFR.transform.rotation = GorillaLocomotion.Player.Instance.rightControllerTransform.rotation;
                    PlatRpawn = true;
                }

                // Apply Rig Id Body for falling and destroy on null
                if (!ControllerInputPoller.instance.leftControllerSecondaryButton && PlatFL != null)
                {
                    GameObject.Destroy(PlatFL);
                    PlatFL = null;
                    PlatLSpawn = false;
                }
                if (!ControllerInputPoller.instance.rightControllerSecondaryButton && PlatFR != null)
                {
                    GameObject.Destroy(PlatFR);
                    PlatFR = null;
                    PlatRpawn = false;
                }
            }
            #endregion

            #region Index

            if (ControllerInputPoller.instance.controllerType == GorillaControllerType.INDEX)
            {
                if (ControllerInputPoller.instance.leftControllerSecondaryButton && PlatFL == null)
                {
                    PlatL();
                }
                if (ControllerInputPoller.instance.rightControllerSecondaryButton && PlatFR == null)
                {
                    PlatR();
                }

                // Tansform Game Obj To Hands
                if (ControllerInputPoller.instance.leftControllerSecondaryButton && PlatFL != null && !PlatLSpawn)
                {
                    PlatFL.transform.position = GorillaLocomotion.Player.Instance.leftControllerTransform.position;
                    PlatFL.transform.rotation = GorillaLocomotion.Player.Instance.leftControllerTransform.rotation;
                    PlatLSpawn = true;
                }
                if (ControllerInputPoller.instance.rightControllerSecondaryButton && PlatFR != null && !PlatRpawn)
                {
                    PlatFR.transform.position = GorillaLocomotion.Player.Instance.rightControllerTransform.position;
                    PlatFR.transform.rotation = GorillaLocomotion.Player.Instance.rightControllerTransform.rotation;
                    PlatRpawn = true;
                }

                // Apply Rig Id Body for falling and destroy on null
                if (!ControllerInputPoller.instance.leftControllerSecondaryButton && PlatFL != null)
                {
                    GameObject.Destroy(PlatFL);
                    PlatFL = null;
                    PlatLSpawn = false;
                }
                if (!ControllerInputPoller.instance.rightControllerSecondaryButton && PlatFR != null)
                {
                    GameObject.Destroy(PlatFR);
                    PlatFR = null;
                    PlatRpawn = false;
                }
            }

            #endregion
        }


        #endregion

        #region Player

        static bool bothHands;
        static bool spam = true;
        public static void BraceletSpam()
        {
            if (!PhotonNetwork.IsConnected)
            {
                NotifiLib.SendNotification("[ <color=red>ERROR</color> ] Not In Room");
            }
            else
            {
                if (ControllerInputPoller.instance.rightGrab)
                {
                    spam = !spam;
                    GorillaGameManager.instance.FindVRRigForPlayer(PhotonNetwork.LocalPlayer).RPC("EnableNonCosmeticHandItemRPC", RpcTarget.All, new object[]
                    {
                    spam,
                    false
                    });
                    RpcCleanUp();
                }
            }
        }

        public static void SpamHand()
        {
            GorillaTagger.Instance.tapCoolDown = float.MinValue;
            RpcCleanUp();
        }

        public static void NoSpamHand()
        {
            GorillaTagger.Instance.tapCoolDown = 0.15f;
        }

        public static void FlightWithNoClip()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                GorillaLocomotion.Player.Instance.transform.position += GorillaLocomotion.Player.Instance.headCollider.transform.forward * Time.deltaTime * 15;
                GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                #region No Clip
                foreach (MeshCollider collider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                {
                    collider.enabled = false;
                }
            }
            else
            {
                foreach (MeshCollider collider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                {
                    collider.enabled = true;
                }
                #endregion
            }
        }

        public static void Rocket()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                GorillaLocomotion.Player.Instance.transform.position += GorillaLocomotion.Player.Instance.transform.up * Time.deltaTime * 25;
                //GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero; Add this if you want no velocity
            }
        }

        public static void NoGrav()
        {
            GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity = false;
        }

        public static void NoGravOff()
        {
            GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity = true;
        }

        public static void AutoFunnyRun()
        {
            if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
            {
                if (bothHands)
                {
                    float time = Time.frameCount;
                    GorillaTagger.Instance.rightHandTransform.position = GorillaTagger.Instance.headCollider.transform.position + (GorillaTagger.Instance.headCollider.transform.forward * Mathf.Cos(time) / 10) + new Vector3(0, -0.5f - (Mathf.Sin(time) / 7), 0) + (GorillaTagger.Instance.headCollider.transform.right * -0.05f);
                    GorillaTagger.Instance.leftHandTransform.position = GorillaTagger.Instance.headCollider.transform.position + (GorillaTagger.Instance.headCollider.transform.forward * Mathf.Cos(time + 180) / 10) + new Vector3(0, -0.5f - (Mathf.Sin(time + 180) / 7), 0) + (GorillaTagger.Instance.headCollider.transform.right * 0.05f);
                }
                else
                {
                    float time = Time.frameCount;
                    GorillaTagger.Instance.rightHandTransform.position = GorillaTagger.Instance.headCollider.transform.position + (GorillaTagger.Instance.headCollider.transform.forward * Mathf.Cos(time) / 10) + new Vector3(0, -0.5f - (Mathf.Sin(time) / 7), 0);
                    GorillaTagger.Instance.leftHandTransform.position = GorillaTagger.Instance.headCollider.transform.position + (GorillaTagger.Instance.headCollider.transform.forward * Mathf.Cos(time + 180) / 10) + new Vector3(0, -0.5f - (Mathf.Sin(time + 180) / 7), 0) + (GorillaTagger.Instance.headCollider.transform.right * 0.05f);
                }
            }
        }

        public static void NoClip()
        {
            if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
            {
                foreach (MeshCollider meshCollider in Object.FindObjectsOfType(typeof(MeshCollider)))
                {
                    meshCollider.enabled = false;
                }
            }
            else
            {
                foreach (MeshCollider meshCollider in Object.FindObjectsOfType(typeof(MeshCollider)))
                {
                    meshCollider.enabled = true;
                }
            }
        }

        public static void RGB()
        {
            float red = Random.Range(0f, 1f);
            float green = Random.Range(0f, 1f);
            float blue = Random.Range(0f, 1f);

            if (!PhotonNetwork.IsConnected)
            {
                NotifiLib.SendNotification("[ <color=red>ERROR</color> ] Not In Room");
            }
            else
            {
                GorillaTagger.Instance.myVRRig.RPC("InitializeNoobMaterial", RpcTarget.All, new object[]
                {
                    red,
                    green,
                    blue
                });
                GorillaTagger.Instance.UpdateColor(red, green, blue);
                RpcCleanUp();
            }
        }
        public static void Speed()
        {
            GorillaLocomotion.Player.Instance.maxJumpSpeed = 9.8f; // Fix To Your Settings
        }

        public static void SpeedFix()
        {
            GorillaLocomotion.Player.Instance.maxJumpSpeed = 6.5f; // Resets Speed
        }

        #endregion

        #region Safty
        public static bool Frame = false;
        private static float SplashCoolDown;
        public static bool JoiningRoom = false;
        public static float RoomJoinDelay = 0f;
        private static float threshold = 0.35f;

        public static void NoFinger()
        {
            ControllerInputPoller.instance.leftControllerGripFloat = 0f;
            ControllerInputPoller.instance.rightControllerGripFloat = 0f;
            ControllerInputPoller.instance.leftControllerIndexFloat = 0f;
            ControllerInputPoller.instance.rightControllerIndexFloat = 0f;
            ControllerInputPoller.instance.leftControllerPrimaryButton = false;
            ControllerInputPoller.instance.leftControllerSecondaryButton = false;
            ControllerInputPoller.instance.rightControllerPrimaryButton = false;
            ControllerInputPoller.instance.rightControllerSecondaryButton = false;
        }

        public static void SpoofName()
        {
            string[] SpoofNames =
            {
              "ALICEVR",
              "BOB",
              "JMAN",
              "DAVIDVR",
              "EVE",
              "FRANK",
              "VMT",
              "HANNAHVR",
              "ISAAC",
              "JACKVR",
              "KATHY",
              "LIAM",
              "MONAVR",
              "RARENAME",
              "NATHANVR",
              "OLIVIA",
              "DAISY09",
              "QUINCY",
              "RACHELVR",
              "UHH",
              "ILOVEYOU"
            };
            int num = new System.Random().Next(SpoofNames.Length);
            PhotonNetwork.LocalPlayer.NickName = SpoofNames[num];
        }

        public static void AntiReportSafty()
        {
            foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer == NetworkSystem.Instance.LocalPlayer)
                {
                    if (line.reportInProgress || line.reportedCheating || line.reportedHateSpeech || line.reportedToxicity)
                    {
                        if (Client.DOAntiReport == true)
                        {
                            PhotonNetwork.Disconnect();
                            RpcCleanUp();
                        }

                        if (Client.DOAntiReportJR == true)
                        {
                            PhotonNetwork.Disconnect();
                            JoiningRoom = true;
                            RpcCleanUp();
                        }
                    }
                }
            }
        }

        public static void AntiReportDisconnect()
        {
            try
            {
                foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                {
                    if (line.linePlayer == NetworkSystem.Instance.LocalPlayer)
                    {
                        Transform report = line.reportButton.gameObject.transform;
                        foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                        {
                            if (vrrig != GorillaTagger.Instance.offlineVRRig)
                            {
                                float DistanceRight = Vector3.Distance(vrrig.rightHandTransform.position, report.position);
                                float DistanceLeft = Vector3.Distance(vrrig.leftHandTransform.position, report.position);

                                if (DistanceRight < threshold || DistanceLeft < threshold)
                                {
                                    PhotonNetwork.Disconnect();
                                    NotifiLib.SendNotification("Someone attempted to report you, you have been disconnected.");
                                    RpcCleanUp();
                                }
                            }
                        }
                    }
                }
            }
            catch { } // Not connected
        }

        public static void AntiReportJR()
        {
            try
            {
                foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                {
                    if (line.linePlayer == NetworkSystem.Instance.LocalPlayer)
                    {
                        Transform report = line.reportButton.gameObject.transform;
                        foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                        {
                            if (vrrig != GorillaTagger.Instance.offlineVRRig)
                            {
                                float DistanceRight = Vector3.Distance(vrrig.rightHandTransform.position, report.position);
                                float DistanceLeft = Vector3.Distance(vrrig.leftHandTransform.position, report.position);

                                if (DistanceRight < threshold || DistanceLeft < threshold)
                                {
                                    PhotonNetwork.Disconnect();
                                    NotifiLib.SendNotification("Someone attempted to report you, you are now joining a new lobby!");
                                    JoiningRoom = true;
                                    RpcCleanUp();
                                }
                            }
                        }
                    }
                }
            }
            catch { } // Not connected
        }

        public static void JoinRandom()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.Disconnect();
                JoiningRoom = true;
            }
            else
            {
                JoinRandomRoom();
            }
        }

        public static void JoinRandomRoom()
        {
            string gamemode = PhotonNetworkController.Instance.currentJoinTrigger.networkZone;
            #region GameModes
            if (gamemode == "forest")
            {
                GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Forest, Tree Exit").GetComponent<GorillaNetworkJoinTrigger>().OnBoxTriggered();
            }
            if (gamemode == "city")
            {
                GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - City Front").GetComponent<GorillaNetworkJoinTrigger>().OnBoxTriggered();
            }
            if (gamemode == "canyons")
            {
                GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Canyon").GetComponent<GorillaNetworkJoinTrigger>().OnBoxTriggered();
            }
            if (gamemode == "mountains")
            {
                GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Mountain For Computer").GetComponent<GorillaNetworkJoinTrigger>().OnBoxTriggered();
            }
            if (gamemode == "beach")
            {
                GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Beach from Forest").GetComponent<GorillaNetworkJoinTrigger>().OnBoxTriggered();
            }
            if (gamemode == "sky")
            {
                GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Clouds").GetComponent<GorillaNetworkJoinTrigger>().OnBoxTriggered();
            }
            if (gamemode == "basement")
            {
                GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Basement For Computer").GetComponent<GorillaNetworkJoinTrigger>().OnBoxTriggered();
            }
            if (gamemode == "caves")
            {
                GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Cave").GetComponent<GorillaNetworkJoinTrigger>().OnBoxTriggered();
            }
            #endregion
        }

        public static void RpcCleanUp()
        {
            float RemoveTimer;
            PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
            PhotonNetwork.RemoveBufferedRPCs();
            GorillaGameManager.instance.OnPlayerLeftRoom(PhotonNetwork.LocalPlayer);
            GorillaGameManager.instance.OnPlayerLeftRoom(PhotonNetwork.LocalPlayer);
            GorillaGameManager.instance.OnPlayerLeftRoom(PhotonNetwork.LocalPlayer);
            GorillaGameManager.instance.OnMasterClientSwitched(PhotonNetwork.LocalPlayer);
            ScienceExperimentManager.instance.OnMasterClientSwitched(PhotonNetwork.LocalPlayer);
            GorillaGameManager.instance.OnMasterClientSwitched(PhotonNetwork.LocalPlayer);
            GorillaGameManager.instance.OnMasterClientSwitched(PhotonNetwork.LocalPlayer);

            if (GorillaTagger.Instance.myVRRig != null)
            {
                PhotonNetwork.OpCleanRpcBuffer(GorillaTagger.Instance.myVRRig);
            }
            RemoveTimer = Time.time;
            if (!Frame)
            {
                Frame = true;
                GorillaNot.instance.rpcErrorMax = int.MaxValue;
                GorillaNot.instance.rpcCallLimit = int.MaxValue;
                GorillaNot.instance.logErrorMax = int.MaxValue;
                PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
                PhotonNetwork.OpCleanRpcBuffer(GorillaTagger.Instance.myVRRig);
                PhotonNetwork.RemoveBufferedRPCs(GorillaTagger.Instance.myVRRig.ViewID, null, null);
                PhotonNetwork.RemoveRPCsInGroup(int.MaxValue);
                PhotonNetwork.SendAllOutgoingCommands();
                GorillaNot.instance.OnPlayerLeftRoom(PhotonNetwork.LocalPlayer);
            }
        }
        #endregion

        #region Cave Mods
        public static void OpenGates()
        {
            if (!PhotonNetwork.IsConnected)
            {
                NotifiLib.SendNotification("[ <color=red>ERROR</color> ] Not In Room");
            }
            else
            {
                NotifiLib.SendNotification("[ <color=green>Success</color> ] Gates Are Opening");
                GameObject.Find("Environment Objects/05Maze_PersistentObjects/GhostLab").GetComponent<GhostLabReliableState>().photonView.ControllerActorNr = PhotonNetwork.LocalPlayer.ActorNumber;
                GameObject.Find("Environment Objects/05Maze_PersistentObjects/GhostLab").GetComponent<GhostLabReliableState>().photonView.OwnerActorNr = PhotonNetwork.LocalPlayer.ActorNumber;

                GameObject.Find("Environment Objects/05Maze_PersistentObjects/GhostLab").GetComponent<GhostLabReliableState>().doorState = GhostLab.EntranceDoorsState.OuterDoorOpen;
                GameObject.Find("Environment Objects/05Maze_PersistentObjects/GhostLab").GetComponent<GhostLabReliableState>().singleDoorOpen[8] = true;
            }
        }

        public static void SpawnMineGhost()
        {
            if (!PhotonNetwork.IsConnected)
            {
                NotifiLib.SendNotification("[ <color=red>ERROR</color> ] Not In Room");
            }
            else
            {
                GameObject.Find("Environment Objects/05Maze_PersistentObjects/MinesSecondLookSkeleton").GetComponent<SecondLookSkeletonSynchValues>().photonView.ControllerActorNr = PhotonNetwork.LocalPlayer.ActorNumber;
                GameObject.Find("Environment Objects/05Maze_PersistentObjects/MinesSecondLookSkeleton").GetComponent<SecondLookSkeletonSynchValues>().photonView.OwnerActorNr = PhotonNetwork.LocalPlayer.ActorNumber;
                GameObject.Find("Environment Objects/05Maze_PersistentObjects/MinesSecondLookSkeleton").GetComponent<SecondLookSkeleton>().currentState = SecondLookSkeleton.GhostState.Activated;

                if (GameObject.Find("Environment Objects/05Maze_PersistentObjects/MinesSecondLookSkeleton").GetComponent<SecondLookSkeleton>().currentState == SecondLookSkeleton.GhostState.Activated)// Checks if the ghost is active
                {
                    NotifiLib.SendNotification("[ <color=green>SUCCESS</color> ] Ghost Has Been Spawn");
                }
                else
                {
                    NotifiLib.SendNotification("<[ <color=red>ERROR</color> ] Ghost Has Not Been Spawn");
                }
            }
        }
        #endregion

        #region Water/Beach Mods

        public static void WalkOnWater()
        {
            GameObject gameObject = GameObject.Find("Beach/B_WaterVolumes");
            Transform transform = gameObject.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject gameObject2 = transform.GetChild(i).gameObject;
                gameObject2.layer = LayerMask.NameToLayer("Default");
            }
        }

        public static void NoWater()
        {
            GameObject gameObject = GameObject.Find("Beach/B_WaterVolumes");
            Transform transform = gameObject.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject gameObject2 = transform.GetChild(i).gameObject;
                gameObject2.layer = LayerMask.NameToLayer("TransparentFX");
            }
        }

        public static void AddWater()
        {
            GameObject gameObject = GameObject.Find("Beach/B_WaterVolumes");
            Transform transform = gameObject.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject gameObject2 = transform.GetChild(i).gameObject;
                gameObject2.layer = LayerMask.NameToLayer("Water");
            }
        }

        public static void SpeedDiving()
        {
            GameObject.Find("Beach/Beach_Gameplay_V6/B_Divingboard_1/DivingBoard_board").GetComponent<GorillaSurfaceOverride>().extraVelMaxMultiplier = 50f;
            GameObject.Find("Beach/Beach_Gameplay_V6/B_Divingboard_1/DivingBoard_board").GetComponent<GorillaSurfaceOverride>().extraVelMultiplier = 50f;
        }

        public static void BeachBallScoreSpam()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                GameObject.Find("BeachBall").transform.localPosition = new Vector3(40.6455f, 3.1775f, 67.3042f);
                GameObject.Find("Ball").transform.localPosition = new Vector3(-25.9165f, 9.0868f, 10.3558f);
            }

            if (ControllerInputPoller.instance.leftControllerSecondaryButton)
            {
                GameObject.Find("BeachBall").transform.localPosition = new Vector3(19.6511f, 3.1466f, 79.8349f);
                GameObject.Find("Ball").transform.localPosition = new Vector3(-4.0491f, 9.0868f, -1.4955f);
            }

            if (ControllerInputPoller.instance.rightGrab)
            {
                GameObject.Find("BeachBall").transform.localPosition = new Vector3(42.2247f, 3.219f, 75.8183f);
                GameObject.Find("Ball").transform.localPosition = new Vector3(-2.8921f, 9.0858f, 6.1698f);
            }
        }

        public static void GrabBeachBall()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                GameObject.Find("BeachBall").transform.position = GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position;
                GameObject.Find("Ball").transform.position = GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position;
            }
        }

        public static void WaterVomitGun()
        {
            if (!PhotonNetwork.IsConnected)
            {
                NotifiLib.SendNotification("[ <color=red>ERROR</color> ] Not In Room");
            }
            else
            {
                if (ControllerInputPoller.instance.rightGrab)
                {
                    Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up, out var Ray);
                    GameObject NewPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    NewPointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    NewPointer.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
                    NewPointer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    NewPointer.transform.position = CopyPlayer ? VrRigPlayers.transform.position : Ray.point;
                    UnityEngine.Object.Destroy(NewPointer.GetComponent<BoxCollider>());
                    UnityEngine.Object.Destroy(NewPointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(NewPointer.GetComponent<Collider>());
                    UnityEngine.Object.Destroy(NewPointer, Time.deltaTime);

                    GameObject line = new GameObject("Line");
                    LineRenderer liner = line.AddComponent<LineRenderer>();
                    liner.material.shader = Shader.Find("GUI/Text Shader");
                    liner.startColor = UnityEngine.Color.white;
                    liner.endColor = UnityEngine.Color.black;
                    liner.startWidth = 0.025f;
                    liner.endWidth = 0.025f;
                    liner.positionCount = 2;
                    liner.useWorldSpace = true;
                    liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                    liner.SetPosition(1, CopyPlayer ? VrRigPlayers.transform.position : Ray.point);
                    UnityEngine.Object.Destroy(line, Time.deltaTime);

                    if (CopyPlayer && VrRigPlayers != null && Time.time > SplashCoolDown)
                    {
                        BallsOnHands();
                        GorillaTagger.Instance.offlineVRRig.enabled = false;

                        GorillaTagger.Instance.offlineVRRig.transform.position = VrRigPlayers.transform.position;
                        GorillaTagger.Instance.offlineVRRig.transform.rotation = VrRigPlayers.transform.rotation;

                        GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.position = VrRigPlayers.head.rigTarget.transform.position;
                        GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.rotation = VrRigPlayers.head.rigTarget.transform.rotation;
                        GorillaTagger.Instance.offlineVRRig.leftHand.rigTarget.transform.position = GorillaTagger.Instance.offlineVRRig.transform.position + (GorillaTagger.Instance.offlineVRRig.transform.right * 0f);
                        GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position = GorillaTagger.Instance.offlineVRRig.transform.position + (GorillaTagger.Instance.offlineVRRig.transform.right * 0f);
                        GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", RpcTarget.All, new object[]
                        {
                    GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.position,
                    GorillaTagger.Instance.offlineVRRig.head.rigTarget.transform.rotation,
                    15f,
                    600f,
                    true,
                    false
                        });
                        RpcCleanUp();
                        SplashCoolDown = Time.time + 0.1f;
                    }
                    if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
                    {
                        VRRig possibly = Ray.collider.GetComponentInParent<VRRig>();
                        if (possibly && possibly != GorillaTagger.Instance.offlineVRRig)
                        {
                            CopyPlayer = true;
                            VrRigPlayers = possibly;
                        }
                    }
                }
                else
                {
                    if (CopyPlayer)
                    {
                        CopyPlayer = false;
                        GorillaTagger.Instance.offlineVRRig.enabled = true;
                    }
                }
            }
        }

        public static void WaterSplashHands()
        {
            if (!PhotonNetwork.IsConnected)
            {
                NotifiLib.SendNotification("[ <color=red>ERROR</color> ] Not In Room");
            }
            else
            {
                if (ControllerInputPoller.instance.rightControllerSecondaryButton && Time.time > SplashCoolDown)
                {
                    GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", RpcTarget.All, new object[]
                    {
                    GorillaTagger.Instance.rightHandTransform.position,
                    GorillaTagger.Instance.rightHandTransform.rotation,
                    15f,
                    600f,
                    true,
                    false
                    });
                    RpcCleanUp();
                    SplashCoolDown = Time.time + 0.1f;
                }

                if (ControllerInputPoller.instance.leftControllerSecondaryButton && Time.time > SplashCoolDown)
                {
                    GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", RpcTarget.All, new object[]
                    {
                    GorillaTagger.Instance.leftHandTransform.position,
                    GorillaTagger.Instance.leftHandTransform.rotation,
                    15f,
                    600f,
                    true,
                    false
                    });
                    RpcCleanUp();
                    SplashCoolDown = Time.time + 0.1f;
                }
            }
        }

        public static void WaterSplashBody()
        {
            if (!PhotonNetwork.IsConnected)
            {
                NotifiLib.SendNotification("[ <color=red>ERROR</color> ] Not In Room");
            }
            else
            {
                if (ControllerInputPoller.instance.rightControllerSecondaryButton && Time.time > SplashCoolDown)
                {
                    GorillaTagger.Instance.myVRRig.RPC("PlaySplashEffect", RpcTarget.All, new object[]
                    {
                    GorillaTagger.Instance.bodyCollider.transform.position,
                    GorillaTagger.Instance.bodyCollider.transform.rotation,
                    15f,
                    600f,
                    true,
                    false
                    });
                    RpcCleanUp();
                    SplashCoolDown = Time.time + 0.1f;
                }
            }
        }

        #endregion

        #region Other Players
        public static void BoneESP()
        {
            Material material = new Material(Shader.Find("GUI/Text Shader"));
            material.color = UnityEngine.Color.Lerp(UnityEngine.Color.white, UnityEngine.Color.black, Mathf.PingPong(Time.time, 1));

            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                {
                    if (!vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>())
                    {
                        vrrig.head.rigTarget.gameObject.AddComponent<LineRenderer>();
                    }
                    vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().endWidth = 0.025f;
                    vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().startWidth = 0.025f;
                    vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().material = material;
                    vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().SetPosition(0, vrrig.head.rigTarget.transform.position + new Vector3(0, 0.160f, 0));
                    vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().SetPosition(1, vrrig.head.rigTarget.transform.position - new Vector3(0, 0.4f, 0));

                    for (int i = 0; i < bones.Count(); i += 2)
                    {
                        if (!vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>())
                        {
                            vrrig.mainSkin.bones[bones[i]].gameObject.AddComponent<LineRenderer>();
                        }
                        vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().endWidth = 0.025f;
                        vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().startWidth = 0.025f;
                        vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().material = material;
                        vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().SetPosition(0, vrrig.mainSkin.bones[bones[i]].position);
                        vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().SetPosition(1, vrrig.mainSkin.bones[bones[i + 1]].position);
                    }
                }
                else
                {
                    foreach (VRRig vrrigs in GorillaParent.instance.vrrigs)
                    {
                        if (!vrrigs.isOfflineVRRig && !vrrigs.isMyPlayer)
                        {
                            for (int i = 0; i < bones.Count(); i += 2)
                            {
                                if (vrrigs.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>())
                                {
                                    UnityEngine.Object.Destroy(vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>());
                                }
                                if (vrrigs.head.rigTarget.gameObject.GetComponent<LineRenderer>())
                                {
                                    UnityEngine.Object.Destroy(vrrigs.head.rigTarget.gameObject.GetComponent<LineRenderer>());
                                }
                            }
                        }
                    }
                }
            }
        }


        public static void TagAll() // VERY BAD WAY TO TAG EVERYONE BUT ITS UD AND IT WORKS
        {
            if (!PhotonNetwork.IsConnected)
            {
                NotifiLib.SendNotification("[ <color=red>ERROR</color> ] Not In Room");
            }
            else
            {
                if (ControllerInputPoller.instance.rightControllerSecondaryButton)
                {
                    BallsOnHands();
                    LineToRig();
                    foreach (VRRig rigs in GorillaParent.instance.vrrigs)
                    {
                        if (!rigs.mainSkin.material.name.Contains("fected"))
                        {
                            GorillaTagger.Instance.offlineVRRig.enabled = false;

                            GorillaLocomotion.Player.Instance.rightControllerTransform.position = rigs.transform.position;
                            GorillaLocomotion.Player.Instance.leftControllerTransform.position = rigs.transform.position;

                            GorillaTagger.Instance.myVRRig.transform.position = rigs.transform.position;
                            GorillaTagger.Instance.offlineVRRig.transform.position = rigs.transform.position;
                        }
                    }
                }
                else
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }
        }


        public static void KickAllParty()
        {
            string[] KickRooms = new string[]
            {
                "KICKEDBYJ0KERMODZ",
                "J0KERMODZRUNSYOU",
                "kicked",
                "DISCORD.GG/J0KERMODZ",
                "byebye",
                "lmao_you_got_kicked",
                "j0kermodz_runs_you"
            };

            if (FriendshipGroupDetection.Instance.IsInParty)
            {
                Client.oldpartycode = PhotonNetwork.CurrentRoom.Name;
                Client.PlayerJoin = true;
                int num = new System.Random().Next(KickRooms.Length);
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(KickRooms[num], JoinType.ForceJoinWithParty);
                Client.TimeForParty = Time.time + 0.25f;
                Client.Doing = false;
                Client.PeoplePartying = ((List<string>)typeof(FriendshipGroupDetection).GetField("myPartyMemberIDs", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(FriendshipGroupDetection.Instance)).Count - 1;
            }

            if (!PhotonNetwork.IsConnected)
            {
                NotifiLib.SendNotification("<color=red>NOT CONNECTED</color>");
            }

            if (!FriendshipGroupDetection.Instance.IsInParty)
            {
                NotifiLib.SendNotification("<color=red>NOT IN PARTY</color>");
            }
        }

        #endregion

        #region Risky
        public static void LagAll()
        {
            if (!Client.DoRisky)
            {
                NotifiLib.SendNotification("<color=red>RISKY MODS ARE NOT ENABLED</color>");
            }
            else
            {
                PhotonView PlayerPhotonView = PhotonViewVRRig(RandomVRRig(false));
                TakeOwnership(PlayerPhotonView);
                if (PlayerPhotonView.AmOwner)
                {
                    PhotonNetwork.Destroy(PlayerPhotonView);
                }
                RpcCleanUp();
            }
        }

        public static void LagGun()
        {
            if (!Client.DoRisky)
            {
                NotifiLib.SendNotification("<color=red>RISKY MODS ARE NOT ENABLED</color>");
            }
            else
            {
                if (ControllerInputPoller.instance.rightGrab)
                {
                    Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up, out var Ray);
                    GameObject NewPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    NewPointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    NewPointer.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
                    NewPointer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    NewPointer.transform.position = CopyPlayer ? VrRigPlayers.transform.position : Ray.point;
                    UnityEngine.Object.Destroy(NewPointer.GetComponent<BoxCollider>());
                    UnityEngine.Object.Destroy(NewPointer.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(NewPointer.GetComponent<Collider>());
                    UnityEngine.Object.Destroy(NewPointer, Time.deltaTime);

                    GameObject line = new GameObject("Line");
                    LineRenderer liner = line.AddComponent<LineRenderer>();
                    liner.material.shader = Shader.Find("GUI/Text Shader");
                    liner.startColor = UnityEngine.Color.white;
                    liner.endColor = UnityEngine.Color.black;
                    liner.startWidth = 0.025f;
                    liner.endWidth = 0.025f;
                    liner.positionCount = 2;
                    liner.useWorldSpace = true;
                    liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                    liner.SetPosition(1, CopyPlayer ? VrRigPlayers.transform.position : Ray.point);
                    UnityEngine.Object.Destroy(line, Time.deltaTime);

                    if (CopyPlayer && VrRigPlayers != null)
                    {
                        PhotonView PlayerPhotonView = PhotonViewVRRig(VrRigPlayers);
                        TakeOwnership(PlayerPhotonView);
                        if (PlayerPhotonView.AmOwner)
                        {
                            PhotonNetwork.Destroy(PlayerPhotonView);
                        }
                        RpcCleanUp();
                    }
                    if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
                    {
                        VRRig possibly = Ray.collider.GetComponentInParent<VRRig>();
                        if (possibly && possibly != GorillaTagger.Instance.offlineVRRig)
                        {
                            CopyPlayer = true;
                            VrRigPlayers = possibly;
                        }
                    }
                }
                else
                {
                    if (CopyPlayer)
                    {
                        CopyPlayer = false;
                        GorillaTagger.Instance.offlineVRRig.enabled = true;
                    }
                }
            }
        }

        #endregion

        #region World

        public static void WaterBalloon()
        {
            foreach (GorillaSurfaceOverride gorillaSurface in Resources.FindObjectsOfTypeAll<GorillaSurfaceOverride>())
            {
                gorillaSurface.overrideIndex = 204;
            }
            NotifiLib.SendNotification("Grab Any Surface To Pick Up The Object");
        }

        public static void SnowBall()
        {
            foreach (GorillaSurfaceOverride gorillaSurface in Resources.FindObjectsOfTypeAll<GorillaSurfaceOverride>())
            {
                gorillaSurface.overrideIndex = 32;
            }
            NotifiLib.SendNotification("Grab Any Surface To Pick Up The Object");
        }

        public static void Present()
        {
            foreach (GorillaSurfaceOverride gorillaSurface in Resources.FindObjectsOfTypeAll<GorillaSurfaceOverride>())
            {
                gorillaSurface.overrideIndex = 240;
            }
            NotifiLib.SendNotification("Grab Any Surface To Pick Up The Object");
        }

        public static void LavaRock()
        {
            foreach (GorillaSurfaceOverride gorillaSurface in Resources.FindObjectsOfTypeAll<GorillaSurfaceOverride>())
            {
                gorillaSurface.overrideIndex = 231;
            }
            NotifiLib.SendNotification("Grab Any Surface To Pick Up The Object");
        }

        public static void Mento()
        {
            foreach (GorillaSurfaceOverride gorillaSurface in Resources.FindObjectsOfTypeAll<GorillaSurfaceOverride>())
            {
                gorillaSurface.overrideIndex = 249;
            }
            NotifiLib.SendNotification("Grab Any Surface To Pick Up The Object");
        }

        public static void FishFood()
        {
            foreach (GorillaSurfaceOverride gorillaSurface in Resources.FindObjectsOfTypeAll<GorillaSurfaceOverride>())
            {
                gorillaSurface.overrideIndex = 252;
            }
            NotifiLib.SendNotification("Grab Any Surface To Pick Up The Object");
        }

        public static void DoorSpam()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                foreach (GTDoor gTDoor in Object.FindObjectsOfType(typeof(GTDoor)))
                {
                    gTDoor.photonView.RPC("ChangeDoorState", RpcTarget.AllViaServer, new object[]
                    {
                        GTDoor.DoorState.Opening
                    });
                }
                RpcCleanUp();
            }
        }

        public static void RGBSnowBalls()
        {
            foreach (SnowballThrowable snowball in Object.FindObjectsOfType(typeof(SnowballThrowable)))
            {
                snowball.randomizeColor = true;
            }
        }

        public static void FireWork()
        {
            EnableFireWorks();
            foreach (FireworksController fireworksController in Object.FindObjectsOfType<FireworksController>())
            {
                foreach (Firework firework in Object.FindObjectsOfType<Firework>())
                {
                    fireworksController.Launch(firework);
                }
            }
        }

        public static void FireWorkVolley()
        {
            EnableFireWorks();
            foreach (FireworksController fireworksController in Object.FindObjectsOfType<FireworksController>())
            {
                fireworksController.LaunchVolley();
            }
        }

        static void EnableFireWorks()
        {
            if (!GameObject.Find("Beach/23SummerBeachGeo").transform.Find("SummerFireworksControllerPrefab (1)").gameObject.activeSelf)
            {
                GameObject.Find("Beach/23SummerBeachGeo/SummerFireworksControllerPrefab (1)").SetActive(true);
                GameObject.Find("Beach/23SummerBeachGeo/SummerFireworksControllerPrefab (2)").SetActive(true);
                GameObject.Find("Beach/23SummerBeachGeo/SummerFireworksControllerPrefab (3)").SetActive(true);
            }
        }

        public static void TargetHit()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                NotifiLib.SendNotification("<color=red>NOT MASTER!</color>");
            }

            foreach (HitTargetNetworkState hit in Object.FindObjectsOfType(typeof(HitTargetNetworkState)))
            {
                hit.TargetHit(new Vector3(0f, 999f, 0f), Vector3.forward);
            }
        }

        public static void TutorialColliders()
        {
            GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Terrain/pitgeo/pit tutorialcave").SetActive(true);
        }

        #region Bug
        public static void GrabBug()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                GameObject.Find("Floating Bug Holdable").GetComponent<ThrowableBug>().transform.position = GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position;
            }
        }

        public static void RideBug()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                NoGrav();
                GorillaTagger.Instance.transform.position = GameObject.Find("Floating Bug Holdable").GetComponent<ThrowableBug>().transform.position;
                #region No Clip
                foreach (MeshCollider collider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                {
                    collider.enabled = false;
                }
            }
            else
            {
                foreach (MeshCollider collider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                {
                    collider.enabled = true;
                }
                NoGravOff();
                #endregion
            }
        }

        public static void DisableBug()
        {
            GameObject.Find("Floating Bug Holdable").GetComponent<ThrowableBug>().disableStealing = true;
        }

        public static void EnableBug()
        {
            GameObject.Find("Floating Bug Holdable").GetComponent<ThrowableBug>().disableStealing = false;
        }

        public static void BugESP()
        {
            GameObject.Find("Floating Bug Holdable/model/PlumpBeetle").GetComponent<SkinnedMeshRenderer>().material.shader = Shader.Find("GUI/Text Shader");
            GameObject.Find("Floating Bug Holdable/model/PlumpBeetle").GetComponent<SkinnedMeshRenderer>().material.color = UnityEngine.Color.white;
        }
        #endregion

        #region Bat
        public static void GrabBat()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                GameObject.Find("Cave Bat Holdable").GetComponent<ThrowableBug>().transform.position = GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position;
            }
        }

        public static void RideBat()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                NoGrav();
                GorillaTagger.Instance.transform.position = GameObject.Find("Cave Bat Holdable").GetComponent<ThrowableBug>().transform.position;
                #region No Clip
                foreach (MeshCollider collider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                {
                    collider.enabled = false;
                }
            }
            else
            {
                foreach (MeshCollider collider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                {
                    collider.enabled = true;
                }
                NoGravOff();
                #endregion
            }
        }

        public static void DisableBat()
        {
            GameObject.Find("Cave Bat Holdable").GetComponent<ThrowableBug>().disableStealing = true;
        }
        public static void EnableBat()
        {
            GameObject.Find("Cave Bat Holdable").GetComponent<ThrowableBug>().disableStealing = false;
        }

        public static void BatESP()
        {
            GameObject.Find("Cave Bat Holdable/CaveBat_Prefab/Bat").GetComponent<SkinnedMeshRenderer>().material.shader = Shader.Find("GUI/Text Shader");
            GameObject.Find("Cave Bat Holdable/CaveBat_Prefab/Bat").GetComponent<SkinnedMeshRenderer>().material.color = UnityEngine.Color.white;
        }

        public static void SantaBat()
        {
            GameObject.Find("Cave Bat Holdable/CaveBat_Prefab/Bone-BatBody/Bone-Head/SantaHat_Wardrobe").SetActive(true);
        }
        #endregion

        #region Shark
        public static void GrabShark()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                GameObject.Find("Swimming Shark prefab").transform.position = GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position;
            }
        }

        public static void SharkGun()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out var hitinfo);
                GunSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GunSphere.transform.position = hitinfo.point;
                GunSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                GunSphere.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                GunSphere.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
                GameObject.Destroy(GunSphere.GetComponent<BoxCollider>());
                GameObject.Destroy(GunSphere.GetComponent<Rigidbody>());
                GameObject.Destroy(GunSphere.GetComponent<Collider>());

                GameObject line = new GameObject("Line");
                LineRenderer liner = line.AddComponent<LineRenderer>();
                liner.material.shader = Shader.Find("GUI/Text Shader");
                liner.startColor = UnityEngine.Color.white;
                liner.endColor = UnityEngine.Color.black;
                liner.startWidth = 0.025f;
                liner.endWidth = 0.025f;
                liner.positionCount = 2;
                liner.useWorldSpace = true;
                liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                liner.SetPosition(1, GunSphere.transform.position);
                UnityEngine.Object.Destroy(line, Time.deltaTime);

                if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
                {
                    GameObject.Destroy(GunSphere, Time.deltaTime);
                    GunSphere.GetComponent<Renderer>().material.color = UnityEngine.Color.black;
                    GameObject.Find("Swimming Shark prefab").transform.position = GunSphere.transform.position;
                }

            }
            if (GunSphere != null)
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
                GameObject.Destroy(GunSphere, Time.deltaTime);
            }
        }


        public static void RideShark()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                NoGrav();
                GorillaTagger.Instance.transform.position = GameObject.Find("Swimming Shark prefab").transform.position;
                #region No Clip
                foreach (MeshCollider collider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                {
                    collider.enabled = false;
                }
            }
            else
            {
                foreach (MeshCollider collider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                {
                    collider.enabled = true;
                }
                NoGravOff();
                #endregion
            }
        }

        public static void SharkESP()
        {
            GameObject.Find("Swimming Shark prefab").GetComponent<MeshRenderer>().material.shader = Shader.Find("GUI/Text Shader");
            GameObject.Find("Swimming Shark prefab").GetComponent<MeshRenderer>().material.color = UnityEngine.Color.white;
        }
        #endregion

        #region Ropes 

        static int[] CanyonRopeIds =
        {
            1131859731, -1781828257, 1263972346, 1630809674, 1163461840,
            1730840134, -133291319, 572393470, -130316683, 1147538199,
            -943752376, -2056681474, 599078497, 1611611971, 28674045,
            -1897003994, 1709973560, 291380855, 1675931389, -479563788,
            -2103784167, 720115, 455941587, 1706239093, 637599653,
            1159654396, 1962374141, 2130686977
        };

        public static void RopeGun()
        {
            if (!Client.DoRisky)
            {
                NotifiLib.SendNotification("<color=red>RISKY MODS ARE NOT ENABLED</color>");
            }
            else
            {
                if (ControllerInputPoller.instance.rightGrab)
                {
                    Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out var hitinfo);
                    GunSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    GunSphere.transform.position = hitinfo.point;
                    GunSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    GunSphere.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                    GunSphere.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
                    GameObject.Destroy(GunSphere.GetComponent<BoxCollider>());
                    GameObject.Destroy(GunSphere.GetComponent<Rigidbody>());
                    GameObject.Destroy(GunSphere.GetComponent<Collider>());

                    GameObject line = new GameObject("Line");
                    LineRenderer liner = line.AddComponent<LineRenderer>();
                    liner.material.shader = Shader.Find("GUI/Text Shader");
                    liner.startColor = UnityEngine.Color.white;
                    liner.endColor = UnityEngine.Color.black;
                    liner.startWidth = 0.025f;
                    liner.endWidth = 0.025f;
                    liner.positionCount = 2;
                    liner.useWorldSpace = true;
                    liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                    liner.SetPosition(1, GunSphere.transform.position);
                    UnityEngine.Object.Destroy(line, Time.deltaTime);
                    GorillaRopeSwing rope = hitinfo.collider.GetComponentInParent<GorillaRopeSwing>();
                    if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
                    {
                        GameObject.Destroy(GunSphere, Time.deltaTime);
                        GunSphere.GetComponent<Renderer>().material.color = UnityEngine.Color.black;
                        PhotonMessageInfo info = new PhotonMessageInfo();
                        RopeSwingManager.instance.SetVelocity(rope.ropeId, 1, new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)), true, info);
                        RpcCleanUp();
                    }
                }
                if (GunSphere != null)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                    GameObject.Destroy(GunSphere, Time.deltaTime);
                }
            }
        } 

        public static void RopesForward()
        {
            if (!Client.DoRisky)
            {
                NotifiLib.SendNotification("<color=red>RISKY MODS ARE NOT ENABLED</color>");
            }
            else
            {
                if (ControllerInputPoller.instance.rightGrab)
                {
                    PhotonMessageInfo info = new PhotonMessageInfo();
                    int Index = Random.Range(0, CanyonRopeIds.Length);
                    int RopeId = CanyonRopeIds[Index];
                    RopeSwingManager.instance.SetVelocity(RopeId, 1, Vector3.forward * 99f, true, info);
                    RpcCleanUp();
                }
            }
        }


        public static void RopesBackwards()
        {
            if (!Client.DoRisky)
            {
                NotifiLib.SendNotification("<color=red>RISKY MODS ARE NOT ENABLED</color>");
            }
            else
            {
                if (ControllerInputPoller.instance.rightGrab)
                {
                    PhotonMessageInfo info = new PhotonMessageInfo();
                    int Index = Random.Range(0, CanyonRopeIds.Length);
                    int RopeId = CanyonRopeIds[Index];
                    RopeSwingManager.instance.SetVelocity(RopeId, 1, Vector3.back * 99f, true, info);
                    RpcCleanUp();
                }
            }
        }



        public static void SpazRopes()
        {
            if (!Client.DoRisky)
            {
                NotifiLib.SendNotification("<color=red>RISKY MODS ARE NOT ENABLED</color>");
            }
            else
            {
                if (ControllerInputPoller.instance.rightGrab)
                {
                    PhotonMessageInfo info = new PhotonMessageInfo();
                    int Index = Random.Range(0, CanyonRopeIds.Length);
                    int RopeId = CanyonRopeIds[Index];
                    RopeSwingManager.instance.SetVelocity(RopeId, 1, new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)), true, info);
                    RpcCleanUp();
                }
            }
        }


        public static void RopesBounce()
        {
            if (!Client.DoRisky)
            {
                NotifiLib.SendNotification("<color=red>RISKY MODS ARE NOT ENABLED</color>");
            }
            else
            {
                if (ControllerInputPoller.instance.rightGrab)
                {
                    PhotonMessageInfo info = new PhotonMessageInfo();
                    int Index = Random.Range(0, CanyonRopeIds.Length);
                    int RopeId = CanyonRopeIds[Index];
                    RopeSwingManager.instance.SetVelocity(RopeId, 1, Vector3.down * 60f, true, info);
                    RpcCleanUp();
                }
            }
        }
        
        #endregion

        #region Gliders

        public static void GrabGliders()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                foreach (GliderHoldable glider in Object.FindObjectsOfType(typeof(GliderHoldable)))
                {
                    glider.gameObject.transform.position = GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position;
                }
            }
        }


        public static void GliderGun()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out var hitinfo);
                GunSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GunSphere.transform.position = hitinfo.point;
                GunSphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                GunSphere.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                GunSphere.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
                GameObject.Destroy(GunSphere.GetComponent<BoxCollider>());
                GameObject.Destroy(GunSphere.GetComponent<Rigidbody>());
                GameObject.Destroy(GunSphere.GetComponent<Collider>());

                GameObject line = new GameObject("Line");
                LineRenderer liner = line.AddComponent<LineRenderer>();
                liner.material.shader = Shader.Find("GUI/Text Shader");
                liner.startColor = UnityEngine.Color.white;
                liner.endColor = UnityEngine.Color.black;
                liner.startWidth = 0.025f;
                liner.endWidth = 0.025f;
                liner.positionCount = 2;
                liner.useWorldSpace = true;
                liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                liner.SetPosition(1, GunSphere.transform.position);
                UnityEngine.Object.Destroy(line, Time.deltaTime);
                GorillaRopeSwing rope = hitinfo.collider.GetComponentInParent<GorillaRopeSwing>();
                if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
                {
                    GameObject.Destroy(GunSphere, Time.deltaTime);
                    GunSphere.GetComponent<Renderer>().material.color = UnityEngine.Color.black;
                    foreach (GliderHoldable glider in Object.FindObjectsOfType(typeof(GliderHoldable)))
                    {
                        glider.gameObject.transform.position = GunSphere.transform.position;
                    }
                }

            }
            if (GunSphere != null)
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
                GameObject.Destroy(GunSphere, Time.deltaTime);
            }
        }


        public static void GliderBlindGun()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up, out var Ray);
                GameObject NewPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                NewPointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                NewPointer.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
                NewPointer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                NewPointer.transform.position = CopyPlayer ? VrRigPlayers.transform.position : Ray.point;
                UnityEngine.Object.Destroy(NewPointer.GetComponent<BoxCollider>());
                UnityEngine.Object.Destroy(NewPointer.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(NewPointer.GetComponent<Collider>());
                UnityEngine.Object.Destroy(NewPointer, Time.deltaTime);

                GameObject line = new GameObject("Line");
                LineRenderer liner = line.AddComponent<LineRenderer>();
                liner.material.shader = Shader.Find("GUI/Text Shader");
                liner.startColor = UnityEngine.Color.white;
                liner.endColor = UnityEngine.Color.black;
                liner.startWidth = 0.025f;
                liner.endWidth = 0.025f;
                liner.positionCount = 2;
                liner.useWorldSpace = true;
                liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                liner.SetPosition(1, CopyPlayer ? VrRigPlayers.transform.position : Ray.point);
                UnityEngine.Object.Destroy(line, Time.deltaTime);

                if (CopyPlayer && VrRigPlayers != null && Time.time > SplashCoolDown)
                {
                    foreach (GliderHoldable glider in Object.FindObjectsOfType(typeof(GliderHoldable)))
                    {
                        glider.gameObject.transform.position = VrRigPlayers.head.rigTarget.transform.position;
                        glider.gameObject.transform.rotation = Quaternion.Euler(new Vector3(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)));
                    }
                }
                if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
                {
                    VRRig possibly = Ray.collider.GetComponentInParent<VRRig>();
                    if (possibly && possibly != GorillaTagger.Instance.offlineVRRig)
                    {
                        CopyPlayer = true;
                        VrRigPlayers = possibly;
                    }
                }
            }
            else
            {
                if (CopyPlayer)
                {
                    CopyPlayer = false;
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }
        }

        public static void GliderSpaz()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                foreach (GliderHoldable glider in Object.FindObjectsOfType(typeof(GliderHoldable)))
                {
                    glider.gameObject.transform.rotation = Quaternion.Euler(new Vector3(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)));
                }
            }
        }
        #endregion

        #region Balloons

        public static void GrabAllBalloons()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                foreach (BalloonHoldable balloonHoldable in Object.FindObjectsOfType<BalloonHoldable>())
                {
                    balloonHoldable.gameObject.transform.position = GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.transform.position;
                }
            }
        }

        public static void BalloonGun()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position, -GorillaLocomotion.Player.Instance.rightControllerTransform.up, out var hitinfo);
                GunSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GunSphere.transform.position = hitinfo.point;
                GunSphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                GunSphere.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                GunSphere.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
                GameObject.Destroy(GunSphere.GetComponent<BoxCollider>());
                GameObject.Destroy(GunSphere.GetComponent<Rigidbody>());
                GameObject.Destroy(GunSphere.GetComponent<Collider>());

                GameObject line = new GameObject("Line");
                LineRenderer liner = line.AddComponent<LineRenderer>();
                liner.material.shader = Shader.Find("GUI/Text Shader");
                liner.startColor = UnityEngine.Color.white;
                liner.endColor = UnityEngine.Color.black;
                liner.startWidth = 0.025f;
                liner.endWidth = 0.025f;
                liner.positionCount = 2;
                liner.useWorldSpace = true;
                liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                liner.SetPosition(1, GunSphere.transform.position);
                UnityEngine.Object.Destroy(line, Time.deltaTime);
                GorillaRopeSwing rope = hitinfo.collider.GetComponentInParent<GorillaRopeSwing>();
                if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
                {
                    GameObject.Destroy(GunSphere, Time.deltaTime);
                    GunSphere.GetComponent<Renderer>().material.color = UnityEngine.Color.black;
                    foreach (BalloonHoldable balloonHoldable in Object.FindObjectsOfType<BalloonHoldable>())
                    {
                        balloonHoldable.gameObject.transform.position = GunSphere.transform.position;
                    }
                }

            }
            if (GunSphere != null)
            {
                GorillaTagger.Instance.offlineVRRig.enabled = true;
                GameObject.Destroy(GunSphere, Time.deltaTime);
            }
        }

        public static void BalloonBlindGun()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up, out var Ray);
                GameObject NewPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                NewPointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                NewPointer.GetComponent<Renderer>().material.color = UnityEngine.Color.white;
                NewPointer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                NewPointer.transform.position = CopyPlayer ? VrRigPlayers.transform.position : Ray.point;
                UnityEngine.Object.Destroy(NewPointer.GetComponent<BoxCollider>());
                UnityEngine.Object.Destroy(NewPointer.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(NewPointer.GetComponent<Collider>());
                UnityEngine.Object.Destroy(NewPointer, Time.deltaTime);

                GameObject line = new GameObject("Line");
                LineRenderer liner = line.AddComponent<LineRenderer>();
                liner.material.shader = Shader.Find("GUI/Text Shader");
                liner.startColor = UnityEngine.Color.white;
                liner.endColor = UnityEngine.Color.black;
                liner.startWidth = 0.025f;
                liner.endWidth = 0.025f;
                liner.positionCount = 2;
                liner.useWorldSpace = true;
                liner.SetPosition(0, GorillaTagger.Instance.rightHandTransform.position);
                liner.SetPosition(1, CopyPlayer ? VrRigPlayers.transform.position : Ray.point);
                UnityEngine.Object.Destroy(line, Time.deltaTime);

                if (CopyPlayer && VrRigPlayers != null && Time.time > SplashCoolDown)
                {
                    foreach (BalloonHoldable balloonHoldable in Object.FindObjectsOfType<BalloonHoldable>())
                    {
                        balloonHoldable.transform.position = VrRigPlayers.head.rigTarget.transform.position;
                        balloonHoldable.gameObject.transform.rotation = Quaternion.Euler(new Vector3(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)));
                    }
                }
                if (Client.GrabState(ControllerInputPoller.instance.rightControllerIndexFloat, 0.1f))
                {
                    VRRig possibly = Ray.collider.GetComponentInParent<VRRig>();
                    if (possibly && possibly != GorillaTagger.Instance.offlineVRRig)
                    {
                        CopyPlayer = true;
                        VrRigPlayers = possibly;
                    }
                }
            }
            else
            {
                if (CopyPlayer)
                {
                    CopyPlayer = false;
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }
        }

        public static void SpazAllBalloons()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                foreach (BalloonHoldable balloonHoldable in Object.FindObjectsOfType<BalloonHoldable>())
                {
                    balloonHoldable.gameObject.transform.rotation = Quaternion.Euler(new Vector3(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)));
                }
            }
        }

        public static void ShootBallons()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                foreach (BalloonHoldable balloonHoldable in Object.FindObjectsOfType<BalloonHoldable>())
                {
                    balloonHoldable.gameObject.transform.position = GorillaTagger.Instance.rightHandTransform.forward * 10f;
                }
            }
        }

        #endregion

        #endregion

        #region Ambush GameMode
        static float HandTapRPCCoolDown = 0f;

        public static void InvisTagAll()
        {
            if (!PhotonNetwork.IsConnected)
            {
                NotifiLib.SendNotification("<color=red>NOT CONNECTED</color>");
            }
            else
            {
                if (ControllerInputPoller.instance.rightControllerSecondaryButton)
                {
                    BallsOnHands();
                    LineToRig();
                    foreach (VRRig rigs in GorillaParent.instance.vrrigs)
                    {
                        if (!rigs.mainSkin.material.name.Contains("stealth"))
                        {
                            GorillaTagger.Instance.offlineVRRig.enabled = false;

                            GorillaLocomotion.Player.Instance.rightControllerTransform.position = rigs.transform.position;
                            GorillaLocomotion.Player.Instance.leftControllerTransform.position = rigs.transform.position;

                            GorillaTagger.Instance.myVRRig.transform.position = rigs.transform.position;
                            GorillaTagger.Instance.offlineVRRig.transform.position = rigs.transform.position;
                        }
                    }
                }
                else
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }
        }

        public static void InvisSelf()
        {
            if (!PhotonNetwork.IsConnected)
            {
                NotifiLib.SendNotification("<color=red>NOT CONNECTED</color>");
            }
            else
            {
                if (ControllerInputPoller.instance.rightControllerSecondaryButton)
                {
                    BallsOnHands();
                    LineToRig();
                    foreach (VRRig rigs in GorillaParent.instance.vrrigs)
                    {
                        if (rigs.mainSkin.enabled)
                        {
                            GorillaTagger.Instance.myVRRig.transform.position = rigs.transform.position;
                            GorillaTagger.Instance.offlineVRRig.transform.position = rigs.transform.position;
                        }
                    }
                }
            }
        }

        public static void AmbushParticalSpam()
        {
            if (!PhotonNetwork.IsConnected)
            {
                NotifiLib.SendNotification("<color=red>NOT CONNECTED</color>");
            }
            else
            {
                if (ControllerInputPoller.instance.rightControllerSecondaryButton && Time.time > HandTapRPCCoolDown) // Spams On Right Hand
                {
                    GorillaTagger.Instance.myVRRig.RPC("OnHandTapRPC", RpcTarget.All, new object[]
                    {
                    18,
                    false, // Sets To Right
                    10000f,
                    Utils.PackVector3ToLong(new Vector3(10000f, 10000f, 10000f))
                    });
                    HandTapRPCCoolDown = Time.time + 0.1f;
                }

                if (ControllerInputPoller.instance.leftControllerSecondaryButton && Time.time > HandTapRPCCoolDown) // Spams On Left Hand
                {
                    GorillaTagger.Instance.myVRRig.RPC("OnHandTapRPC", RpcTarget.All, new object[]
                    {
                    18,
                    true, // Sets to left
                    10000f,
                    Utils.PackVector3ToLong(new Vector3(10000f, 10000f, 10000f))
                    });
                    HandTapRPCCoolDown = Time.time + 0.1f;
                }
                RpcCleanUp();
            }
        }

        public static void AmbushESP()
        {
            Material material = new Material(Shader.Find("GUI/Text Shader"));
            material.color = UnityEngine.Color.red;
            if (!PhotonNetwork.IsConnected)
            {
                NotifiLib.SendNotification("<color=red>NOT CONNECTED</color>");
            }
            else
            {
                foreach (VRRig vrrig in GorillaParent.instance.vrrigs) // Same code as bone esp but it checks if the mainSkin is enabled (for the invis players) and makes them red
                {
                    if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer && !vrrig.mainSkin.enabled)
                    {
                        if (!vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>())
                        {
                            vrrig.head.rigTarget.gameObject.AddComponent<LineRenderer>();
                        }
                        vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().endWidth = 0.025f;
                        vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().startWidth = 0.025f;
                        vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().material = material;
                        vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().SetPosition(0, vrrig.head.rigTarget.transform.position + new Vector3(0, 0.160f, 0));
                        vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>().SetPosition(1, vrrig.head.rigTarget.transform.position - new Vector3(0, 0.4f, 0));

                        for (int i = 0; i < bones.Count(); i += 2)
                        {
                            if (!vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>())
                            {
                                vrrig.mainSkin.bones[bones[i]].gameObject.AddComponent<LineRenderer>();
                            }
                            vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().endWidth = 0.025f;
                            vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().startWidth = 0.025f;
                            vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().material = material;
                            vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().SetPosition(0, vrrig.mainSkin.bones[bones[i]].position);
                            vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>().SetPosition(1, vrrig.mainSkin.bones[bones[i + 1]].position);
                        }
                    }
                    else
                    {
                        foreach (VRRig vrrigs in GorillaParent.instance.vrrigs)
                        {
                            if (!vrrigs.isOfflineVRRig && !vrrigs.isMyPlayer)
                            {
                                for (int i = 0; i < bones.Count(); i += 2)
                                {
                                    if (vrrigs.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>())
                                    {
                                        UnityEngine.Object.Destroy(vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>());
                                    }
                                    if (vrrigs.head.rigTarget.gameObject.GetComponent<LineRenderer>())
                                    {
                                        UnityEngine.Object.Destroy(vrrigs.head.rigTarget.gameObject.GetComponent<LineRenderer>());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
