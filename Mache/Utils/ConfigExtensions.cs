using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mache.Utils
{
    public static class ConfigExtensions
    {
        public static Vector2Config Vector2Config(this ConfigFile config, string modId, string configName, string configDescription, Vector2 defaultValue)
        {
            return new Vector2Config(config, modId, configName, configDescription, defaultValue);
        }
        public static Vector3Config Vector3Config(this ConfigFile config, string modId, string configName, string configDescription, Vector3 defaultValue)
        {
            return new Vector3Config(config, modId, configName, configDescription, defaultValue);
        }
        public static RectConfig RectConfig(this ConfigFile config, string modId, string configName, string configDescription, Rect defaultValue)
        {
            return new RectConfig(config, modId, configName, configDescription, defaultValue);
        }
    }

    public class Vector2Config
    {
        private ConfigEntry<float> _positionX;
        private ConfigEntry<float> _positionY;
        public Vector2 Value
        {
            get
            {
                return new Vector2(_positionX.Value, _positionY.Value);
            }
            set
            {
                _positionX.Value = value.x;
                _positionY.Value = value.y;
            }
        }

        public Vector2Config(ConfigFile config, string modId, string configName, string configDescription, Vector2 defaultValue)
        {
            _positionX = config.Bind(modId, configName + "_X", defaultValue.x, configDescription);
            _positionY = config.Bind(modId, configName + "_Y", defaultValue.y, configDescription);
        }
    }

    public class Vector3Config
    {
        private ConfigEntry<float> _positionX;
        private ConfigEntry<float> _positionY;
        private ConfigEntry<float> _positionZ;
        public Vector3 Value
        {
            get
            {
                return new Vector3(_positionX.Value, _positionY.Value, _positionZ.Value);
            }
            set
            {
                _positionX.Value = value.x;
                _positionY.Value = value.y;
                _positionZ.Value = value.z;
            }
        }

        public Vector3Config(ConfigFile config, string modId, string configName, string configDescription, Vector3 defaultValue)
        {
            _positionX = config.Bind(modId, configName + "_X", defaultValue.x, configDescription);
            _positionY = config.Bind(modId, configName + "_Y", defaultValue.y, configDescription);
            _positionZ = config.Bind(modId, configName + "_Z", defaultValue.z, configDescription);
        }
    }

    public class RectConfig
    {
        private ConfigEntry<float> _positionX;
        private ConfigEntry<float> _positionY;
        private ConfigEntry<float> _width;
        private ConfigEntry<float> _height;
        public Rect Value
        {
            get
            {
                return new Rect(_positionX.Value, _positionY.Value, _width.Value, _height.Value);
            }
            set
            {
                _positionX.Value = value.x;
                _positionY.Value = value.y;
                _width.Value = value.width;
                _height.Value = value.height;
            }
        }

        public RectConfig(ConfigFile config, string modId, string configName, string configDescription, Rect defaultValue)
        {
            _positionX = config.Bind(modId, configName + "_X", defaultValue.x, configDescription);
            _positionY = config.Bind(modId, configName + "_Y", defaultValue.y, configDescription);
            _width = config.Bind(modId, configName + "_Width", defaultValue.width, configDescription);
            _height = config.Bind(modId, configName + "_Height", defaultValue.width, configDescription);
        }
    }
}
