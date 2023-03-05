using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime;
using UniverseLib;
using Sons.Items.Core;
using Sons.Inventory;
using Sons.Gameplay;
using TheForest.Items.Inventory;
using TheForest.Modding.Bridge;
using UniverseLib.UI;
using Mache.UI;
using Mache.Utils;
using System;
using System.IO;
using System.Text;
using Sons.Input;
using Mache.Networking;
using TheForest.Utils;

namespace Mache
{
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("SonsOfTheForest.exe")]
    internal class MachePlugin : BasePlugin
    {
        public const string ModId = "com.willis.sotf.mache";
        public const string ModName = "Mache";
        public const string Version = "0.0.6";

        internal static MachePlugin Instance { get; private set; }

        public override void Load()
        {
            Instance = this;
            AddComponent<Mache>();
            AddComponent<MacheGlobalEventListener>();
        }
    }

    public class Mache : MonoBehaviour
    {
        private static MacheOverlay Overlay { get; set; }

        private static List<Func<ModDetails>> pendingModRegistrations = new List<Func<ModDetails>>();
        private static bool overlayReady = false;


        public static void RegisterMod(Func<ModDetails> detailsProvider)
        {
            if (detailsProvider == null) return;
            if (overlayReady)
            {
                Overlay.RegisterMod(detailsProvider.Invoke());
                return;
            }
            pendingModRegistrations.Add(detailsProvider);
        }

        public void Awake()
        {
            // initialize UniverseLib
            UniverseLib.Config.UniverseLibConfig config = new UniverseLib.Config.UniverseLibConfig()
            {
                Disable_EventSystem_Override = false,
                Force_Unlock_Mouse = false,
                Unhollowed_Modules_Folder = Path.Combine(Paths.BepInExRootPath, "interop")
            };
            Universe.Init(3f, UniverseInitialized, Log, config);
        }

        public void Start()
        {
            RegisterMenu();
            RegisterEvents();
        }

        private void RegisterMenu()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Mache is a modding framework designed for Sons of the Forest. As a utility it provides support for modders, allowing simple creation of new game content and functions to tweak existing elements of the game. Mache provides access to a unified set of tools, menus, and actions that make the process of creating and implementing mods easier and more streamlined.");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("<b><size=20>Contact</size></b>");
            sb.AppendLine();
            sb.AppendLine("If you encounter issues or have suggestions, feel free to contact me on Discord at <i>Willis#8400</i>");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("<i><color=orange>This project is a work-in-progress, and undergoing active development</color></i>");
            var description = sb.ToString();
            Mache.RegisterMod(() => new ModDetails
            {
                Name = "Mache",
                Id = MachePlugin.ModId,
                Version = MachePlugin.Version,
                Description = description,
                //OnMenuShow = () => { MachePlugin.Instance.Log.LogInfo("Opening Mache menu!"); }
            });
        }

        private void RegisterEvents()
        {
            //EventDispatcher.RegisterEvent(TestEvent.Id, TestEvent.Serialize, TestEvent.Deserialize, TestEvent.OnReceived);
            //EventDispatcher.RegisterEvent<SimpleEventTest>(SimpleEventTest.Id);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Overlay.Toggle();
            }
        }
        private void UniverseInitialized()
        {
            var uiBase = UniversalUI.RegisterUI(MachePlugin.ModId, MacheMenuUpdate);
            Overlay = new MacheOverlay(uiBase);

            overlayReady = true;

            foreach (var detailsProvider in pendingModRegistrations)
            {
                Overlay.RegisterMod(detailsProvider.Invoke());
            }
            pendingModRegistrations.Clear();
        }

        private void MacheMenuUpdate() { }

        private static void Log(object message, LogType logType)
        {
            string text = ((message != null) ? message.ToString() : null) ?? "";
            switch (logType)
            {
                case LogType.Error:
                case LogType.Exception:
                    MachePlugin.Instance.Log.LogError(message);
                    break;
                case LogType.Assert:
                case LogType.Log:
                    MachePlugin.Instance.Log.LogInfo(text);
                    break;
                case LogType.Warning:
                    MachePlugin.Instance.Log.LogWarning(text);
                    break;
            }
        }

        public static IEnumerable<T> FindObjectsOfType<T>() where T : Component
        {
            return GameObject.FindObjectsOfType(Il2CppType.Of<T>()).Select(i => i.Cast<T>());
        }
        public static T FindObjectOfType<T>() where T : Component
        {
            return GameObject.FindObjectOfType(Il2CppType.Of<T>()).Cast<T>();
        }
    }
}
