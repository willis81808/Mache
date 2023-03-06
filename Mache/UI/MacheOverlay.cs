using BepInEx.Configuration;
using Mache.Networking;
using Mache.Utils;
using Sons.Gui;
using Sons.Input;
using Sons.Items.Core;
using Sons.Save;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheForest.Modding.Bridge;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace Mache.UI
{
    public sealed class MacheOverlay : UniverseLib.UI.Panels.PanelBase
    {
        public override string Name => "Mod Settings";
        public override int MinWidth => 1000;
        public override int MinHeight => 500;
        public override Vector2 DefaultAnchorMin => new Vector2(0f, 0f);
        public override Vector2 DefaultAnchorMax => new Vector2(0f, 0f);
        public override bool CanDragAndResize => true;

        internal static GameObject ModCategoryView { get; private set; }
        internal static GameObject ModDetailsView { get; private set; }

        internal bool IsActive { get; private set; }

        private static Dictionary<string, ModDetails> registeredModDetails = new Dictionary<string, ModDetails>();

        private Vector2Config MenuPosition = null;

        public MacheOverlay(UIBase owner) : base(owner)
        {
            MenuPosition = MachePlugin.Instance.Config.Vector2Config(MachePlugin.ModId, "MacheOverlayPosition", "Stored position of Mache overlay window", new Vector2(0 - (MinWidth / 2f), MinHeight / 2f));
        }

        internal void RegisterMod(ModDetails details)
        {
            if (registeredModDetails.ContainsKey(details.Id))
            {
                MachePlugin.Instance.Log.LogWarning($"Mod menu already registered with ID: {details.Id}");
                return;
            }

            registeredModDetails.Add(details.Id, details);

            var categoryButton = UIFactory.CreateButton(ModCategoryView, $"{details.Name}_details_button", details.Name);
            UIFactory.SetLayoutElement(categoryButton.Component.gameObject, flexibleWidth: 9999, minHeight: 30, flexibleHeight: 0);

            var detailsParent = UIFactory.CreateVerticalGroup(ModDetailsView, $"{details.Name}_details_parent", true, false, true, true);
            UIFactory.SetLayoutElement(detailsParent, flexibleHeight: 9999);
            details.DetailsView = detailsParent;

            var detailsList = UIFactory.CreateScrollView(detailsParent, "details_scroll", out var detailsScrollContent, out _, color: new Color(0.17f, 0.17f, 0.17f));
            UIFactory.SetLayoutElement(detailsList, flexibleHeight: 9999);
            var detailsContent = UIFactory.CreateVerticalGroup(detailsScrollContent, $"{details.Name}_details", true, false, true, true, spacing: 25, padding: new Vector4(25, 25, 25, 25), childAlignment: TextAnchor.UpperCenter);

            var detailsTitle = UIFactory.CreateLabel(detailsContent, $"{details.Name}_title", $"{details.Name}{(details.Version == null ? "" : $" {details.Version}")}", TextAnchor.LowerCenter, fontSize: 24);
            UIFactory.SetLayoutElement(detailsTitle.gameObject, minHeight: 30, flexibleHeight: 0);

            var detailsDescription = UIFactory.CreateLabel(detailsContent, $"{details.Name}_description", details.Description, TextAnchor.UpperCenter);
            UIFactory.SetLayoutElement(detailsDescription.gameObject, flexibleHeight: 9999);

            if (details.OnMenuShow != null)
            {
                var settingsButton = UIFactory.CreateButton(detailsParent, $"show_{details.Name}_options_button", "Open Settings");
                UIFactory.SetLayoutElement(settingsButton.Component.gameObject, minHeight: 50, flexibleHeight: 0);
                settingsButton.OnClick = details.OnMenuShow;
            }
            
            categoryButton.OnClick = () => EnableDetailsFor(details.Id);

            details.OnFinishedCreating?.Invoke(detailsContent);

            details.DetailsView.SetActive(false);
        }

        internal void EnableDetailsFor(string modId)
        {
            foreach (var d in registeredModDetails.Values)
            {
                d.DetailsView.SetActive(d.Id == modId);
            }
        }

        protected override void ConstructPanelContent()
        {
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(ContentRoot, true, false, true, true);

            var viewport = UIFactory.CreateHorizontalGroup(ContentRoot, "mods_viewport", true, true, true, true, 2, new Vector4(3, 3, 3, 3), new Color(0.1f, 0.1f, 0.1f));
            
            // create mod list view
            var modsList = UIFactory.CreateScrollView(viewport, "mods_list", out var modsListContent, out _, new Color(0.1f, 0.1f, 0.1f));
            UIFactory.SetLayoutElement(modsList, minWidth: 300, flexibleWidth: 0);
            ModCategoryView = modsListContent;
            UIFactory.CreateVerticalGroup(modsListContent, "mods_list_content", true, false, true, true, spacing: 10, bgColor: new Color(0.05f, 0.05f, 0.05f));

            // create mod details view
            var modDetails = UIFactory.CreateVerticalGroup(viewport, "mod_details", true, true, true, true, 4, default, new Color(0.17f, 0.17f, 0.17f));
            UIFactory.SetLayoutElement(modDetails, flexibleWidth: 9999);
            ModDetailsView = modDetails;

            SetActive(false);
        }

        public override void OnFinishDrag()
        {
            base.OnFinishDrag();
            MenuPosition.Value = UIRoot.transform.localPosition;
        }

        protected override void LateConstructUI()
        {
            base.LateConstructUI();

            UIRoot.transform.localPosition = MenuPosition.Value;
            EnsureValidPosition();
        }

        public override void SetActive(bool active)
        {
            base.SetActive(active);

            if (active == IsActive) return;
            IsActive = active;

            if (LocalPlayer.IsInWorld && !PauseMenu.IsActive && PauseMenu._instance.CanBeOpened() && IsActive)
            {
                PauseMenu._instance.Open();
            }
        }
    }
}
