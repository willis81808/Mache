using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Mache.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace Mache.UI
{
    public class MenuBuilder
    {
        private List<MenuComponent> components = new List<MenuComponent>();

        internal MenuBuilder() { }

        public MenuBuilder AddComponent(MenuComponent component)
        {
            components.Add(component);
            return this;
        }

        public MenuPanel BuildStandalone(string name, string menuId, Action updateMethod = null)
        {
            var uiBase = UniversalUI.RegisterUI(menuId, updateMethod == null ? () => { } : updateMethod);
            return new MenuPanel(uiBase, name, components);
        }

        public GameObject BuildToTarget(GameObject parent)
        {
            var container = UIFactory.CreateVerticalGroup(parent, "dynamic_menu_group", true, false, true, true, spacing: 30);
            foreach (var component in components)
            {
                component.Construct(container);
            }
            return container;
        }
    }

    public class MenuPanel : UniverseLib.UI.Panels.PanelBase
    {
        private List<MenuComponent> components;
        private string _name = "Dynamic Menu Panel";

        public override string Name => "";
        public override int MinWidth => 600;
        public override int MinHeight => 300;
        public override Vector2 DefaultAnchorMin => new Vector2(0f, 0f);
        public override Vector2 DefaultAnchorMax => new Vector2(0f, 0f);

        private Vector2Config MenuPosition = null;

        internal MenuPanel(UIBase owner, string name, List<MenuComponent> components) : base(owner)
        {
            this.components = components;
            this._name = name;

            MenuPosition = MachePlugin.Instance.Config.Vector2Config(MachePlugin.ModId, UIRoot.transform.parent.parent.name + "MenuPosition", "Stored position of built menu", new Vector2(0 - (MinWidth / 2f), MinHeight / 2f));
        }

        internal MenuPanel(UIBase owner) : base(owner) { }

        public override void OnFinishDrag()
        {
            base.OnFinishDrag();
            MenuPosition.Value = UIRoot.transform.localPosition;
        }

        protected override void LateConstructUI()
        {
            base.LateConstructUI();

            TitleBar.transform.GetChild(0).GetComponent<Text>().text = _name;
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(ContentRoot, true, false, true, true, spacing: 30, padTop: 3, padBottom: 3, padLeft: 3, padRight: 3);

            foreach (var component in components)
            {
                component.Construct(ContentRoot);
            }

            UIRoot.transform.localPosition = MenuPosition.Value;
            EnsureValidPosition();
        }

        protected override void ConstructPanelContent() { }

        public static MenuBuilder Builder()
        {
            return new MenuBuilder();
        }
    }

    public abstract class MenuComponent
    {
        public abstract GameObject Construct(GameObject root);
    }

    public class LabelComponent : MenuComponent
    {
        public string Text { private get; set; } = "Label Text";
        public int FontSize { private get; set; } = 14;
        public TextAnchor Alignment { private get; set; } = TextAnchor.MiddleLeft;

        public Text TextObject { get; private set; }

        public override GameObject Construct(GameObject root)
        {
            TextObject = UIFactory.CreateLabel(root, "label", Text, alignment: Alignment, fontSize: FontSize);
            UIFactory.SetLayoutElement(TextObject.gameObject, minHeight: 30);

            return TextObject.gameObject;
        }
    }

    public class SliderComponent : MenuComponent
    {
        public string Name { private get; set; } = null;
        public int FontSize { private get; set; } = 14;

        public float StartValue { private get; set; } = 0f;
        public  float MinValue { private get; set; } = 0f;
        public float MaxValue { private get; set; } = 1f;
        public bool WholeNumbers { private get; set; } = false;
        public Action<SliderComponent, float> OnValueChanged { get; set; }

        public Slider SliderObject { get; private set; }
        public Text LabelObject { get; private set; }

        public override GameObject Construct(GameObject root)
        {
            var sliderHolder = UIFactory.CreateVerticalGroup(root, "slider_holder", true, false, true, true);
            UIFactory.SetLayoutElement(sliderHolder, minHeight: Name != null ? 60 : 30);

            if (Name != null)
            {
                LabelObject = UIFactory.CreateLabel(sliderHolder, "slider_label", $"{Name} ({GetClampedValue(StartValue)})", fontSize: FontSize);
                UIFactory.SetLayoutElement(LabelObject.gameObject, minHeight: 30);
            }

            var sliderObj = UIFactory.CreateSlider(sliderHolder, "slider", out var sliderObject);
            UIFactory.SetLayoutElement(sliderObj, minHeight: 30);
            SliderObject = sliderObject;
            SliderObject.maxValue = MaxValue;
            SliderObject.minValue = MinValue;
            SliderObject.value = GetClampedValue(StartValue);
            SliderObject.onValueChanged.AddListener((val) => OnValueChanged?.Invoke(this, val));
            SliderObject.wholeNumbers = WholeNumbers;

            if (Name != null)
            {
                SliderObject.onValueChanged.AddListener(UpdateLabel);
            }

            return sliderHolder;
        }

        private float GetClampedValue(float value)
        {
            return Mathf.Min(MaxValue, Mathf.Max(MinValue, value));
        }

        private void UpdateLabel(float value)
        {
            LabelObject.text = $"{Name} ({value})";
        }
    }

    public class ButtonComponent : MenuComponent
    {
        public string Text { private get; set; } = "Button";
        public int MinHeight { private get; set; } = 50;
        public int MinWidth { private get; set; } = 100;
        public Action<ButtonComponent> OnClick { private get; set; } = null;

        public ButtonRef ButtonObject { get; private set; }

        public override GameObject Construct(GameObject root)
        {
            ButtonObject = UIFactory.CreateButton(root, "button", Text);
            UIFactory.SetLayoutElement(ButtonObject.GameObject, minHeight: MinHeight, minWidth: MinWidth);
            ButtonObject.OnClick = () => OnClick?.Invoke(this);

            return ButtonObject.GameObject;
        }
    }

    public class InputComponent : MenuComponent
    {
        public string Title { private get; set; } = null;
        public string Placeholder { private get; set; } = "";
        public Action<InputComponent, string> OnInputChanged { private get; set; } = null;

        public Text LabelObject { get; private set; }
        public InputFieldRef InputObject { get; private set; }

        public override GameObject Construct(GameObject root)
        {
            var inputHolder = UIFactory.CreateVerticalGroup(root, "input_holder", true, false, true, true);
            UIFactory.SetLayoutElement(inputHolder, minHeight: Title != null ? 60 : 30);

            if (Title != null)
            {
                LabelObject = UIFactory.CreateLabel(inputHolder, "input_label", Title);
                UIFactory.SetLayoutElement(LabelObject.gameObject, minHeight: 30);
            }

            InputObject = UIFactory.CreateInputField(inputHolder, "input", Placeholder);
            UIFactory.SetLayoutElement(InputObject.GameObject, minHeight: 30);
            InputObject.OnValueChanged += (val) => OnInputChanged?.Invoke(this, val);

            return inputHolder;
        }
    }

    public class ToggleComponent : MenuComponent
    {
        public string Title { private get; set; } = "Toggle";
        public bool DefaultValue { private get; set; } = false;
        public Action<ToggleComponent, bool> OnValueChanged { private get; set; }

        public Text LabelObject { get; private set; }
        public Toggle ToggleObject { get; private set; }

        public override GameObject Construct(GameObject root)
        {
            var toggleHolder = UIFactory.CreateVerticalGroup(root, "toggle_holder", true, false, true, true);
            UIFactory.SetLayoutElement(toggleHolder, minHeight: Title != null ? 60 : 30);

            if (Title != null)
            {
                LabelObject = UIFactory.CreateLabel(toggleHolder, "toggle_label", Title);
                UIFactory.SetLayoutElement(LabelObject.gameObject, minHeight: 30);
            }

            var toggleObj = UIFactory.CreateToggle(toggleHolder, "toggle", out var toggleObject, out _);
            UIFactory.SetLayoutElement(toggleObj, minHeight: 30);
            ToggleObject = toggleObject;
            ToggleObject.onValueChanged.AddListener((val) => OnValueChanged?.Invoke(this, val));

            return toggleHolder;
        }
    }
}
