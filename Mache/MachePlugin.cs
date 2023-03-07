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
using BepInEx.Configuration;
using Il2CppInterop.Runtime.Injection;
using Endnight.Types;

namespace Mache
{
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("SonsOfTheForest.exe")]
    internal class MachePlugin : BasePlugin
    {
        public const string ModId = "com.willis.sotf.mache";
        public const string ModName = "Mache";
        public const string Version = "0.1.3";

        internal static MachePlugin Instance { get; private set; }

        public override void Load()
        {
            var harmony = new HarmonyLib.Harmony(ModId);
            harmony.PatchAll(typeof(MachePlugin).Assembly);

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

        internal ConfigEntry<KeyCode> openMenuKeybind;

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
            openMenuKeybind = MachePlugin.Instance.Config.Bind(MachePlugin.ModId, "MacheOpenMenuKeybind", KeyCode.F1, "Keybind for opening the Mache Mod Menu");

            // initialize UniverseLib
            UniverseLib.Config.UniverseLibConfig config = new UniverseLib.Config.UniverseLibConfig()
            {
                Allow_UI_Selection_Outside_UIBase = true,
                Disable_EventSystem_Override = true,
                Force_Unlock_Mouse = false,
                Unhollowed_Modules_Folder = Path.Combine(Paths.BepInExRootPath, "interop"),
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
                OnFinishedCreating = CreateMenuSettings
            });
        }

        private void CreateMenuSettings(GameObject parent)
        {
            MenuPanel.Builder()
                .AddComponent(new DropdownComponent
                {
                    Title = "Open Menu Keybind",
                    DefaultValue = openMenuKeybind.Value.ToString(),
                    DefaultOptions = Enum.GetNames(typeof(KeyCode)),
                    DropdownHeight = 300,
                    OnValueChanged = OnKeybindDropdownChanged
                })
                .BuildToTarget(parent);
        }

        private void OnKeybindDropdownChanged(DropdownComponent self, int val)
        {
            var keycode = ((KeyCode[])Enum.GetValues(typeof(KeyCode)))[val];
            openMenuKeybind.Value = keycode;
        }

        private void RegisterEvents()
        {
            //EventDispatcher.RegisterEvent(TestEvent.Id, TestEvent.Serialize, TestEvent.Deserialize, TestEvent.OnReceived);
            //EventDispatcher.RegisterEvent<HudNoticeEvent>();
        }

        public void Update()
        {
            if (Input.GetKeyDown(openMenuKeybind.Value))
            {
                Overlay.Toggle();
            }
            else if (Overlay != null && Overlay.IsActive && Input.GetKeyDown(KeyCode.Escape))
            {
                Overlay.SetActive(false);
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
