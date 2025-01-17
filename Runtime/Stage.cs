﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Popcron.SceneStaging
{
    [Serializable]
    public class Stage : IEquatable<Stage>
    {
        public const ushort CurrentVersion = 2;

        public delegate void OnPropAddedDelegate(Stage stage, Prop prop);

        /// <summary>
        /// Occurs when a prop has been added to a stage.
        /// </summary>
        public static OnPropAddedDelegate OnPropAdded { get; set; }

        [SerializeField]
        private ushort version = CurrentVersion;

        [SerializeField]
        private string id;

        [SerializeField]
        private string displayName;

        [SerializeField]
        private List<Prop> props = new List<Prop>();

        private Dictionary<int, Prop> idToProp = null;
        private ReadOnlyCollection<Prop> propsReadOnly = null;

        /// <summary>
        /// The pretty display name of this stage.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Unique ID of this stage.
        /// </summary>
        public string ID => id;

        /// <summary>
        /// Collection of all props in this stage.
        /// </summary>
        public ReadOnlyCollection<Prop> Props
        {
            get
            {
                if (propsReadOnly is null || propsReadOnly.Count != props.Count)
                {
                    propsReadOnly = props.AsReadOnly();
                }

                return propsReadOnly;
            }
        }

        /// <summary>
        /// The version in which this stage was created in.
        /// </summary>
        public ushort Version => version;

        public Stage(string displayName, string id)
        {
            this.id = id;
            this.displayName = displayName;
        }

        public Stage(string displayName)
        {
            id = StageUtils.GetID();
            this.displayName = displayName;
        }

        /// <summary>
        /// Returns true if this string is meant to point to this stage.
        /// </summary>
        public bool IsMatch(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                return text.Equals(displayName) || text.Equals(id, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// Returns a JSON string that represents this stage.
        /// </summary>
        public string ToJson(bool prettyPrint = true) => Conversion.ToJson(this, prettyPrint);

        /// <summary>
        /// Clones this stage into a new one.
        /// </summary>
        public Stage Clone()
        {
            string unityJson = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<Stage>(unityJson);
        }

        /// <summary>
        /// Returns a prop that represents this game object.
        /// </summary>
        public Prop GetProp(GameObject gameObject)
        {
            int propsCount = props.Count;
            for (int i = propsCount - 1; i >= 0; i--)
            {
                Prop prop = props[i];
                if (prop.GameObject == gameObject)
                {
                    return prop;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a prop that represents this transform.
        /// </summary>
        public Prop GetProp(Transform transform)
        {
            if (transform)
            {
                return GetProp(transform.gameObject);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a prop with this ID.
        /// </summary>
        public Prop GetProp(int id)
        {
            if (idToProp == null)
            {
                idToProp = new Dictionary<int, Prop>();
                int propsCount = props.Count;
                for (int p = propsCount - 1; p >= 0; p--)
                {
                    Prop propHere = props[p];
                    idToProp[propHere.ID] = propHere;
                }
            }

            if (idToProp.TryGetValue(id, out Prop prop))
            {
                return prop;
            }

            return null;
        }

        /// <summary>
        /// Returns the first prop with this exact name.
        /// </summary>
        public Prop GetProp(string name)
        {
            int propsCount = props.Count;
            for (int p = propsCount - 1; p >= 0; p--)
            {
                Prop prop = props[p];
                if (prop.Name == name)
                {
                    return prop;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns all props with this component type.
        /// </summary>
        public List<Prop> GetPropWithComponent<T>() where T : UnityEngine.Component => GetPropWithComponent(typeof(T));

        /// <summary>
        /// Returns all props with this component type.
        /// </summary>
        public List<Prop> GetPropWithComponent(Type type)
        {
            List<Prop> result = new List<Prop>();
            int propsCount = props.Count;
            for (int p = propsCount - 1; p >= 0; p--)
            {
                Prop prop = props[p];
                int componentCount = prop.Count;
                for (int c = componentCount - 1; c >= 0; c--)
                {
                    Component component = prop[c];
                    if (type.IsAssignableFrom(component.Type))
                    {
                        result.Add(prop);
                        break;
                    }
                }
            }

            return result;
        }

        public void AddProp(Prop prop)
        {
            props.Add(prop);
            propsReadOnly = null;

            if (idToProp == null)
            {
                idToProp = new Dictionary<int, Prop>();
            }

            idToProp[prop.ID] = prop;

            //invoke event
            OnPropAdded?.Invoke(this, prop);
        }

        public Prop AddProp(GameObject gameObject, int parent = -1)
        {
            if (gameObject)
            {
                int id = props.Count;
                Prop prop = new Prop(gameObject, id, parent);
                AddProp(prop);
                return prop;
            }
            else
            {
                return null;
            }
        }

        public Prop AddProp(int id, int parent = -1)
        {
            Prop prop = new Prop(id, parent);
            AddProp(prop);
            return prop;
        }

        public Prop AddProp(GameObject gameObject, int id, int parent = -1)
        {
            if (gameObject)
            {
                Prop prop = new Prop(gameObject, id, parent);
                AddProp(prop);
                return prop;
            }
            else
            {
                return null;
            }
        }

        public override bool Equals(object obj) => Equals(obj as Stage);
        public bool Equals(Stage other) => other?.GetHashCode() == GetHashCode();

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 578067217;
                hashCode = hashCode * -1521134295 + id?.GetHashCode() ?? -1;
                hashCode = hashCode * -1521134295 + displayName?.GetHashCode() ?? -1;

                if (props != null)
                {
                    int propsCount = props.Count;
                    for (int i = propsCount - 1; i >= 0; i--)
                    {
                        Prop prop = props[i];
                        hashCode = hashCode * 31 + prop.GetHashCode();
                    }
                }

                return hashCode;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Stage left, Stage right) => left?.GetHashCode() == right?.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Stage left, Stage right) => !(left == right);

        /// <summary>
        /// Creates a new map from json data.
        /// </summary>
        public static Stage FromJson(string json) => Conversion.FromJson(json).GetAwaiter().GetResult();

        /// <summary>
        /// Creates a new map from json data.
        /// </summary>
        public static async Task<Stage> FromJsonTaskAsync(string json) => await Conversion.FromJson(json);
    }
}