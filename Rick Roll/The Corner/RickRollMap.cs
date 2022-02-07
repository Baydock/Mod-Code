using Assets.Scripts.Data.MapSets;
using Assets.Scripts.Models.Map;
using Assets.Scripts.Models.Map.Gizmos;
using Assets.Scripts.Models.Map.Spawners;
using Assets.Scripts.Models.Map.Triggers;
using Assets.Scripts.Simulation.SMath;
using Assets.Scripts.Unity.Map;
using Assets.Scripts.Utils;
using Il2CppSystem.Collections.Generic;

namespace RickRoll {
    static class RickRollMap {
        public static string Name = "Test Map";

        private static MapDetails details = null;
        private static MapModel model = null;

        public static MapDetails Details {
            get {
                if (details == null) {
                    details = new MapDetails() {
                        id = Name,
                        difficulty = MapDifficulty.Expert,
                        coopMapDivisionType = CoopDivision.FREE_FOR_ALL,
                        unlockDifficulty = MapDifficulty.Advanced,
                        mapMusic = "",
                        mapSprite = new SpriteReference(Name)
                    };
                }
                return details;
            }
        }

        public static MapModel Model {
            get {
                if (model == null) {
                    model = new MapModel(Name,
                        GetAreaModels(),
                        GetBlockerModels(),
                        GetCoopAreaLayoutModels(),
                        GetPathModels(),
                        GetRemoveableModels(),
                        GetMapGizmoModels(),
                        (int)MapDifficulty.Expert,
                        GetPathSpawnerModel(),
                        GetMapEventModels(),
                        GetBloonWideSpeed());
                }
                return model;
            }
        }

        #region Areas

        private static AreaModel[] GetAreaModels() => new AreaModel[] {
            GetAreaWhole(),
            GetAreaTrack(),
            GetAreaWater()
        };

        private static AreaModel GetAreaWhole() {
            List<Vector2> initArea = new List<Vector2>();
            initArea.Add(new Vector2(-330, -330));
            initArea.Add(new Vector2(-330, 330));
            initArea.Add(new Vector2(330, 330));
            initArea.Add(new Vector2(330, -330));
            return new AreaModel("Whole", new Polygon(initArea), 0, AreaType.land);
        }

        private static AreaModel GetAreaTrack() {
            List<Vector2> initArea = new List<Vector2>();
            initArea.Add(new Vector2(100, 330));
            initArea.Add(new Vector2(100, 72.5f));
            initArea.Add(new Vector2(330, 72.5f));
            initArea.Add(new Vector2(330, 67.5f));
            initArea.Add(new Vector2(105, 67.5f));
            initArea.Add(new Vector2(105, 330));
            return new AreaModel("Track", new Polygon(initArea), 0, AreaType.track);
        }

        private static AreaModel GetAreaWater() {
            List<Vector2> initArea = new List<Vector2>();
            initArea.Add(new Vector2(105, 330));
            initArea.Add(new Vector2(105, 72.5f));
            initArea.Add(new Vector2(330, 72.5f));
            initArea.Add(new Vector2(330, 330));
            return new AreaModel("Water", new Polygon(initArea), 0, AreaType.water);
        }

        #endregion

        #region Blockers

        private static BlockerModel[] GetBlockerModels() => new BlockerModel[0];

        #endregion

        #region Coop Areas

        private static CoopAreaLayoutModel[] GetCoopAreaLayoutModels() => new CoopAreaLayoutModel[] {
            GetCoopAreaLayoutFFA()
        };

        private static CoopAreaLayoutModel GetCoopAreaLayoutFFA() => new CoopAreaLayoutModel(new CoopAreaModel[] {
            GetCoopAreaFFA()
        }, AreaLayoutType.FREE_FOR_ALL);

        private static CoopAreaModel GetCoopAreaFFA() {
            List<Vector2> initArea = new List<Vector2>();
            initArea.Add(new Vector2(-330, -300));
            initArea.Add(new Vector2(-330, 330));
            initArea.Add(new Vector2(330, 330));
            initArea.Add(new Vector2(330, -330));
            return new CoopAreaModel(0, new Polygon(initArea), new Vector2(0, 0));
        }

        #endregion

        #region Paths

        private static PathModel[] GetPathModels() => new PathModel[] { GetPath() };

        private static PathModel GetPath() {
            List<PointInfo> initPath = new List<PointInfo>();
            initPath.Add(new PointInfo { point = new Vector3(329, 330), bloonScale = 1, moabScale = 1 });
            initPath.Add(new PointInfo { point = new Vector3(329, 329), bloonScale = 1, moabScale = 1 });
            initPath.Add(new PointInfo { point = new Vector3(330, 329), bloonScale = 1, moabScale = 1 });
            return new PathModel("Path", (PointInfo[])initPath.ToArray(), true, false,
                new Vector3(-1000, -1000, -1000), new Vector3(-1000, -1000, -1000), null, null);
        }

        #endregion

        #region Removeables

        private static RemoveableModel[] GetRemoveableModels() => new RemoveableModel[0];

        #endregion

        #region Gizmos

        private static MapGizmoModel[] GetMapGizmoModels() => new MapGizmoModel[0];

        #endregion

        #region Path Spawners

        private static PathSpawnerModel GetPathSpawnerModel() => new PathSpawnerModel(""
            , GetForwardSplitterModel(), GetReverseSplitterModel());

        private static SplitterModel GetForwardSplitterModel() => new SplitterModel("", new string[] { "Path" });

        private static SplitterModel GetReverseSplitterModel() => new SplitterModel("", new string[] { "Path" });

        #endregion

        #region Events

        private static MapEventModel[] GetMapEventModels() => new MapEventModel[0];

        #endregion

        private static float GetBloonWideSpeed() => 1;
    }
}
