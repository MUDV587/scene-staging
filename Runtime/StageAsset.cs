﻿using UnityEngine;

namespace Popcron.SceneStaging
{
    [CreateAssetMenu(menuName = "Stage")]
    public class StageAsset : ScriptableObject
    {
        [SerializeField]
        private Stage stage;

        /// <summary>
        /// A copy of the stage in this asset.
        /// </summary>
        public Stage Stage
        {
            get => stage.Clone();
            set => stage = value.Clone();
        }

        /// <summary>
        /// The pretty display name of this stage.
        /// </summary>
        public string DisplayName => stage.DisplayName;

        /// <summary>
        /// Unique ID of this stage.
        /// </summary>
        public string ID => stage.ID;

        /// <summary>
        /// Returns a JSON string that represents this stage.
        /// </summary>
        public string ToJson(bool prettyPrint = true) => stage.ToJson(prettyPrint);

        /// <summary>
        /// Creates a new stage asset with this stage object.
        /// </summary>
        public static StageAsset Create(Stage stage)
        {
            StageAsset asset = CreateInstance<StageAsset>();
            asset.stage = stage;
            asset.name = stage.DisplayName;
            return asset;
        }

        /// <summary>
        /// Creates a new stage asset with no stage assigned.
        /// </summary>
        public static StageAsset Create(string name)
        {
            StageAsset asset = CreateInstance<StageAsset>();
            asset.stage = null;
            asset.name = name;
            return asset;
        }
    }
}