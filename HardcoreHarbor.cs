using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Oxide.Plugins
{
    [Info("HardcoreHarbor", "Wujaszkun", "1.2.1")]
    [Description("Adds military to harbor")]
    internal class HardcoreHarbor : RustPlugin
    {
        private static HardcoreHarbor instance;
        public BasePlayer player;
        public HarborData harborData;
        public DynamicConfigFile entityDataFile;
        public List<MonumentInfo> harborList = new List<MonumentInfo>();
        private ConfigData configData;
        private string containerName;

        private void OnServerInitialized()
        {
            try { entityDataFile = Interface.Oxide.DataFileSystem.GetFile("HardcoreHarbor/entity_data"); } catch (Exception e) { instance.Puts("GetDataFile error " + e.Message); }
            try { LoadConfigData(); } catch (Exception e) { instance.Puts("LoadConfigData error " + e.Message); }
            try { LoadEntityData(); } catch (Exception e) { instance.Puts("LoadEntityData spawn error " + e.Message); }
            Puts("Loaded done!");

            instance = this;

            try
            {
                harborList = GetHarborList();
                InitializeComponents();
                Puts("Component Initialized!");
            }
            catch (Exception e)
            {
                Puts("Harbor autoinit failed: " + e.Message);
            }
        }

        private void Unload()
        {
            try { SaveEntityData(); } catch { }
            foreach (MonumentInfo harbor in GetHarborList())
            {
                try
                {
                    HardcoreHarborComponent harborBuff = harbor.GetComponent<HardcoreHarborComponent>();
                    if (harborBuff != null)
                    {
                        harborBuff.ShowTimer = false;
                        harborBuff.DespawnAllEntities();
                        harborBuff.DestroyGUI();
                        GameManager.Destroy(harborBuff);
                    }
                }
                catch (Exception e)
                {
                    Puts("Unload failed: " + e.Message);
                }
            }
        }

        private void OnServerShutdown()
        {
            try { SaveEntityData(); } catch { }
            foreach (MonumentInfo harbor in GetHarborList())
            {
                try
                {
                    HardcoreHarborComponent harborBuff = harbor.GetComponent<HardcoreHarborComponent>();
                    if (harborBuff != null)
                    {
                        harborBuff.ShowTimer = false;
                        harborBuff.DespawnAllEntities();
                        harborBuff.DestroyGUI();
                        GameManager.Destroy(harborBuff);
                    }
                }
                catch (Exception e)
                {
                    Puts("Unload failed: " + e.Message);
                }
            }
        }

        #region Chat commands
        [ChatCommand("nogui")]
        private void DNoGUICommand(BasePlayer player, string command, string[] args)
        {
            foreach (MonumentInfo harbor in GetHarborList())
            {
                try
                {
                    HardcoreHarborComponent harborBuff = harbor.GetComponent<HardcoreHarborComponent>();
                    if (harborBuff != null)
                    {
                        harborBuff.ShowTimer = false;
                    }
                }
                catch (Exception e)
                {
                    Puts("nogui failed: " + e.Message);
                }
            }
        }
        //[ChatCommand("showcontrol")]
        //private void ShowControlPanel(BasePlayer player, string command, string[] args)
        //{
        //    ShowControlGUI(player);
        //}
        //[ChatCommand("hidecontrol")]
        //private void HideControlPanel(BasePlayer player, string command, string[] args)
        //{
        //    HideControlGUI(player);
        //}
        [ChatCommand("d_despawn")]
        private void DespawnCommand(BasePlayer player, string command, string[] args)
        {
            if (player.IsAdmin)
            {
                foreach (MonumentInfo harbor in harborList)
                {
                    HardcoreHarborComponent harborBuff = harbor.GetComponent<HardcoreHarborComponent>();
                    harborBuff.DespawnAllEntities();
                }
            }
        }
        [ChatCommand("d_spawn")]
        private void SpawnCommand(BasePlayer player, string command, string[] args)
        {
            if (player.IsAdmin)
            {
                foreach (MonumentInfo harbor in GetHarborList())
                {
                    if (harbor.GetComponent<HardcoreHarborComponent>() != null)
                    {
                        harbor.GetComponent<HardcoreHarborComponent>().SpawnAllEntities();
                    }
                }
            }
        }
        [ChatCommand("d_reset")]
        private void ResetCommand(BasePlayer player, string command, string[] args)
        {
            if (player.IsAdmin)
            {
                foreach (MonumentInfo harbor in GetHarborList())
                {
                    if (harbor.GetComponent<HardcoreHarborComponent>() != null)
                    {
                        harbor.GetComponent<HardcoreHarborComponent>().ResetHarborOnDemand();
                    }
                }
            }
        }
        [ChatCommand("d_point")]
        private void PointCommand(BasePlayer player, string command, string[] args)
        {
            if (player.IsAdmin)
            {
                foreach (MonumentInfo harbor in GetHarborList())
                {
                    if (harbor.GetComponent<HardcoreHarborComponent>() != null)
                    {
                        PrintToChat(player,player.transform.InverseTransformPoint(harbor.transform.position).ToString());
                        PrintToChat(player, harbor.transform.InverseTransformPoint(player.transform.position).ToString());
                    }
                }
            }
        }
        #endregion
        private List<MonumentInfo> GetHarborList()
        {
            try
            {
                List<MonumentInfo> tempList = new List<MonumentInfo>();
                List<MonumentInfo> resultList = new List<MonumentInfo>();
                foreach (MonumentInfo monInfo in GameObject.FindObjectsOfType<MonumentInfo>())
                {
                    if (monInfo.name.Contains("harbor_1"))
                    {
                        tempList.Add(monInfo);
                    }
                }
                if (tempList.Count > 1)
                {
                    resultList.Add(tempList[1]);
                }
                else if (tempList.Count == 1)
                {
                    resultList.Add(tempList[0]);
                }
                return resultList;
            }
            catch (Exception e)
            {
                Puts("GetHarborList failed!" + e.Message);
                return null;
            }
        }
        private void InitializeComponents()
        {
            harborList = GetHarborList();
            try
            {
                if (harborList.Count == 0)
                {
                    Puts("Initialize components failed: No harbors found!");
                    return;
                }
                else
                {
                    foreach (MonumentInfo harbor in GetHarborList())
                    {
                        if (harbor.GetComponent<HardcoreHarborComponent>() == null)
                        {
                            harbor.gameObject.AddComponent<HardcoreHarborComponent>();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Puts("Initialize components failed: " + e.Message);
            }
        }
        #region ControlGUI

        public void ShowControlGUI(BasePlayer player)
        {
            if (!player.IsAdmin) return;


            CuiElementContainer mainContainer = new CuiElementContainer();

            CuiPanel mainPanel = new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.6" },
                RectTransform = { AnchorMin = "0.2 0.2", AnchorMax = "0.8 0.8" },
                CursorEnabled = true
            };

            CuiPanel listPanel = new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.8" },
                RectTransform = { AnchorMin = "0.6 0.25", AnchorMax = "0.75 0.75" }
            };
            CuiLabel mainLabel = new CuiLabel
            {
                RectTransform = { AnchorMin = "0.6 0.76", AnchorMax = "0.75 0.79" },
                Text = { Color = "1 1 1 1", Text = "Upcoming events list", Align = TextAnchor.MiddleCenter}
            };

            CuiButton closeWindowButton = new CuiButton
            {
                Button =
                {
                    Command = "hidecontrol",
                },
                Text = { Color = "0 0 0 1", Text = "Close Window", Align = TextAnchor.MiddleCenter },
                RectTransform = { AnchorMin = "0.25 0.25", AnchorMax = "0.35 0.30" }
            };

            mainContainer.Add(mainPanel, "Hud", $"mainPanel_{player.net.ID.ToString()}");
            mainContainer.Add(mainLabel, "Hud", $"mainLabel_{player.net.ID.ToString()}");
            mainContainer.Add(listPanel, "Hud", $"listPanel_{player.net.ID.ToString()}");
            mainContainer.Add(closeWindowButton, "Hud", $"closeButton_{player.net.ID.ToString()}");

            CuiHelper.AddUi(player, mainContainer);
        }

        public void HideControlGUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, containerName);
            CuiHelper.DestroyUi(player, $"mainPanel_{player.net.ID.ToString()}");
            CuiHelper.DestroyUi(player, $"mainLabel_{player.net.ID.ToString()}");
            CuiHelper.DestroyUi(player, $"listPanel_{player.net.ID.ToString()}");
            CuiHelper.DestroyUi(player, $"closeButton_{player.net.ID.ToString()}");
        }

        #endregion


        #region ConfigData
        public void SaveConfig(ConfigData config)
        {
            Config.WriteObject(config, true);
        }

        protected override void LoadDefaultConfig()
        {
            ConfigData config = new ConfigData
            {
                SpawnCargoShip = true,
                SpawnBradley = true,
                SpawnStrongPoints = true,
                SpawnMilitaryPersonnel = true,
                SpawnAdvancedCrates = true,
                SpawnSAMSites = true,
                SpawnAmmoCrates = true,
                SpawnHackableCrates = true,
                RespawnTime = 21600f,
                ShowTimer = true
            };
            SaveConfig(config);
        }
        private void LoadConfigData()
        {
            try
            {
                LoadConfigFromFile();
                Puts("Configuration has been loaded succesfully!");
            }
            catch
            {
                LoadDefaultConfig();
                Puts("Config load failed. Loaded default configuration!");
            }
        }
        private void LoadConfigFromFile()
        {
            configData = Config.ReadObject<ConfigData>();
        }

        public class ConfigData
        {
            [JsonProperty(PropertyName = "Spawn Carbo Ship")]
            public bool SpawnCargoShip { get; set; }

            [JsonProperty(PropertyName = "Spawn Bradley")]
            public bool SpawnBradley { get; set; }

            [JsonProperty(PropertyName = "Spawn Strong Points")]
            public bool SpawnStrongPoints { get; set; }

            [JsonProperty(PropertyName = "Spawn Military Personnel")]
            public bool SpawnMilitaryPersonnel { get; set; }

            [JsonProperty(PropertyName = "Spawn Advanced Crates")]
            public bool SpawnAdvancedCrates { get; set; }

            [JsonProperty(PropertyName = "Spawn SAM Sites")]
            public bool SpawnSAMSites { get; set; }

            [JsonProperty(PropertyName = "Spawn Ammo Crates")]
            public bool SpawnAmmoCrates { get; set; }

            [JsonProperty(PropertyName = "Spawn Hackable Crates")]
            public bool SpawnHackableCrates { get; set; }

            [JsonProperty(PropertyName = "Respawn Time")]
            public float RespawnTime { get; set; }

            [JsonProperty(PropertyName = "Show Timer")]
            public bool ShowTimer { get; set; }
        }
        #endregion

        #region PluginData
        public void SaveEntityData()
        {
            entityDataFile.WriteObject(harborData.harborListData);
        }
        private HarborData LoadEntityData()
        {
            try
            {
                harborData = entityDataFile.ReadObject<HarborData>();
                Puts("Entity Data has been loaded succesfully!");
            }
            catch
            {
                Puts("No entity data found! Creating new data file!");
                harborData = new HarborData();
            }
            return harborData;
        }
        private HarborData UpdateHarborData()
        {
            HarborData tempHarborData = new HarborData();

            foreach (MonumentInfo harbor in harborList)
            {
                int hKey = harbor.GetInstanceID();
                Dictionary<uint, string> hValue = harbor.GetComponent<HardcoreHarborComponent>().spawnedEntityList;
                tempHarborData.harborListData.Add(hKey, hValue);
            }

            harborData = tempHarborData;

            SaveEntityData();

            return tempHarborData;
        }
        public class HarborData
        {
            public Dictionary<int, Dictionary<uint, string>> harborListData = new Dictionary<int, Dictionary<uint, string>>();
        }
        #endregion

        #region OxideHooks
        private bool CanBradleyApcTarget(BradleyAPC apc, BaseEntity entity)
        {
            if (entity is HTNPlayer)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            foreach (MonumentInfo harbor in harborList)
            {
                try
                {
                    HardcoreHarborComponent harborBuff = harbor.GetComponent<HardcoreHarborComponent>();
                    if (harborBuff != null)
                    {
                        harborBuff.DestroyGUI(player);
                    }
                }
                catch (Exception e)
                {
                    Puts("OnPlayerDisconnected failed: " + e.Message);
                }
            }
        }
        #endregion

        #region BuffTimer class
        private class BuffTimer : MonoBehaviour
        {
            private ConfigData configData;

            public float TimeLeft { get; private set; }
            private bool timerIsActive;

            private void Awake()
            {
                configData = instance.configData;
                TimeLeft = configData.RespawnTime;
                timerIsActive = false;
            }

            private void FixedUpdate()
            {
                try
                {
                    if (TimeLeft > 0 && TimerIsActive())
                    {
                        TimeLeft -= Time.deltaTime;
                    }
                    else if (TimeLeft == 0)
                    {
                        Deactivate();
                    }
                }
                catch (Exception e)
                {
                    instance.Puts("Timer fixedUpdate()" + e.Message);
                }
            }

            public void Activate()
            {
                timerIsActive = true;
            }
            public void Activate(float time)
            {
                TimeLeft = time;
                timerIsActive = true;
            }
            public void Deactivate()
            {
                timerIsActive = false;
            }
            public bool TimerIsActive()
            {
                return timerIsActive;
            }
            public float GetCurrentTimer()
            {
                return TimeLeft;
            }
            public float ResetTimer(float time)
            {
                TimeLeft = time;
                return TimeLeft;
            }
            public float ResetTimerAndDeactivate(float time)
            {
                Deactivate();
                TimeLeft = time;
                return TimeLeft;
            }
            public float SetTimer(float time)
            {
                TimeLeft = time;
                return TimeLeft;
            }
        }
        #endregion

        #region HarborBuff class
        private class HardcoreHarborComponent : MonoBehaviour
        {
            public BradleyAPC bradley;
            public BuffTimer respawnTimer;

            public Vector3 centerPoint;
            public Vector3 harborCenter;

            private ConfigData configData;
            private MonumentInfo harborInfo;
            private CargoShip cargoShip;

            public Dictionary<uint, string> spawnedEntityList = new Dictionary<uint, string>();
            public Dictionary<uint, BaseEntity> spawnedBaseEntityList = new Dictionary<uint, BaseEntity>();
            public Dictionary<Vector3, Vector3> soldiersPositionRotation = new Dictionary<Vector3, Vector3>();

            private readonly List<Vector3> path;
            private List<Vector3> spawnPoints = new List<Vector3>();
            private readonly List<BaseEntity> projectileList = new List<BaseEntity>();
            private readonly List<BaseEntity> flareList = new List<BaseEntity>();
            private List<BaseEntity> militaryPersonnelList;
            private List<AutoTurret> shipTurrets = new List<AutoTurret>();
            private CuiElementContainer container;
            private readonly string samPrefab = "assets/prefabs/npc/sam_site_turret/sam_site_turret_deployed.prefab";
            private readonly string flarePrefab = "assets/prefabs/tools/flareold/flare.deployed.prefab";
            private readonly string mobileSoldierPrefab = "assets/prefabs/npc/scientist/htn/scientist_full_any.prefab";
            private readonly string advancedCratePrefab = "assets/bundled/prefabs/radtown/dmloot/dm tier3 lootbox.prefab";
            private readonly string ammoCratePrefab = "assets/bundled/prefabs/radtown/dmloot/dm ammo.prefab";
            private readonly string hackableCratePrefab = "assets/prefabs/deployable/chinooklockedcrate/codelockedhackablecrate.prefab";
            private readonly string watchTowerPrefab = "assets/prefabs/building/watchtower.wood/watchtower.wood.prefab";
            private readonly string barricadePrefab = "assets/prefabs/deployable/barricades/barricade.concrete.prefab";
            private readonly string metalWirePrefab = "assets/prefabs/deployable/barricades/barricade.metal.prefab";
            private readonly string outpostScientist = "assets/prefabs/npc/scientist/htn/scientist_full_any.prefab";

            public bool ShowTimer { get; set; }

            private readonly TOD_Sky Sky = TOD_Sky.Instance;
            private float nextActionTime;
            private float period;

            #region Hooks
            private void Awake()
            {
                instance.Puts("Component awake!");
                try
                {
                    nextActionTime = 0.0f;
                    period = 120f;

                    ShowTimer = true;
                    respawnTimer = gameObject.AddComponent<BuffTimer>();
                    harborInfo = gameObject.GetComponent<MonumentInfo>();

                    configData = instance.configData;

                    militaryPersonnelList = new List<BaseEntity>();
                    spawnPoints = new List<Vector3>();

                    respawnTimer.Activate();

                    SpawnAllEntities();
                }
                catch (Exception e)
                {
                    instance.Puts("Awake failed" + e.Message);
                }
            }
            public Vector3 GetRandomPoint(Vector3 center, float maxDistance)
            {
                NavMeshHit hit;

                Vector3 randomPos = UnityEngine.Random.insideUnitSphere * maxDistance + center;

                NavMesh.SamplePosition(randomPos, out hit, maxDistance, NavMesh.AllAreas);

                return hit.position;
            }

            public List<Vector3> GetRandomPointsList(int numberOfPoints, int range)
            {
                List<Vector3> tempList = new List<Vector3>();
                for (int i = 0; i < numberOfPoints; i++)
                {
                    try
                    {
                        Vector3 pos = GetRandomPoint(harborInfo.transform.position, UnityEngine.Random.Range(10, range)) + new Vector3(0, 3f, 0);

                        if (!Physics.CheckSphere(pos, 2f, LayerMask.GetMask("Terrain", "World")))
                        {
                            tempList.Add(pos);
                        }
                        else
                        {
                            i--;
                        }
                    }
                    catch (Exception e)
                    {
                        instance.Puts("Get point failed: " + e.Message);
                    }
                }
                return tempList;
            }
            public void ResetHarborOnDemand()
            {
                respawnTimer.ResetTimer(configData.RespawnTime);
                DespawnAllEntities();
                instance.Puts("================================================================================");
                instance.Puts("Harbor has been reset");
                instance.Puts($"Entities spawned: { spawnedBaseEntityList.Count.ToString()}");
                instance.Puts("================================================================================");
                spawnedEntityList.Clear();
                spawnedBaseEntityList.Clear();
                soldiersPositionRotation.Clear();
                SpawnAllEntities();
                instance.UpdateHarborData();
            }
            public void ResetHarbor()
            {
                if (respawnTimer != null && respawnTimer.GetCurrentTimer() == 0)
                {
                    ResetHarborOnDemand();
                }
            }

            private void FixedUpdate()
            {
                try
                {
                    if (shipTurrets.Count > 0)
                    {
                        foreach (AutoTurret shipTurret in shipTurrets)
                        {
                            if (shipTurret.GetAttachedWeapon().primaryMagazine.contents < 1 )
                            {
                                shipTurret.GetAttachedWeapon().primaryMagazine.contents = shipTurret.GetAttachedWeapon().primaryMagazine.capacity;
                                //shipTurret.Reload();
                                //shipTurret.UpdateAttachedWeapon();
                            }
                        }
                    }
                }
                catch { }

                try
                {
                    DestroyGUIInactive();
                    if (respawnTimer != null && ShowTimer == true) UpdateGUI();
                }
                catch (Exception e)
                {
                    instance.Puts("Update GUI failed! " + e.Message);
                }
                ResetHarbor();
                try
                {
                    if (cargoShip != null)
                    {
                        cargoShip.targetNodeIndex = -1;
                    }
                    else
                    {

                    }
                }
                catch (Exception e)
                {
                    instance.Puts("CargoShip path fixedUpdate() " + e.Message);
                }
                try
                {
                    if (bradley != null && path != null)
                    {
                        if (bradley.PathComplete())
                        {
                            path.Reverse();
                            bradley.currentPath = path;
                        }
                    }
                }
                catch (Exception e)
                {
                    instance.Puts("Bradley path fixedUpdate() " + e.Message);
                }
                try
                {
                    if (projectileList.Count > 0)
                    {
                        foreach (BaseEntity proj in projectileList)
                        {
                            if (proj != null && proj.GetComponent<ServerProjectile>()._currentVelocity.y < -0.01f)
                            {
                                flareList.Add(CreateFlare(proj.transform.position));
                                proj.GetComponent<TimedExplosive>().Explode();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    instance.Puts("Flare projectile launch failed: " + e.Message);
                }
                try
                {
                    if (flareList.Count > 0)
                    {
                        foreach (BaseEntity flare in flareList)
                        {
                            if (flare != null && flare.GetComponent<Rigidbody>().velocity.y < -0.5f)
                            {
                                Vector3 velocity = new Vector3(UnityEngine.Random.Range(-2f, 2f), -0.01f, UnityEngine.Random.Range(-2f, 2f));
                                flare.GetComponent<Rigidbody>().velocity = velocity;
                            }

                        }
                    }
                }
                catch (Exception e)
                {
                    instance.Puts("Flare ignition failed: " + e.Message);
                }
                try
                {
                    if (Time.time > nextActionTime)
                    {
                        nextActionTime = Time.time + period;
                        if (Sky.IsNight)
                        {
                            instance.Puts("Flares deployed");
                            instance.Puts("Current time: " + Time.time);
                            instance.Puts("Is night: " + Sky.IsNight);
                            DeployFlare();
                        }
                    }

                }
                catch (Exception e)
                {
                    instance.Puts("Flare ignition failed: " + e.Message);
                }
            }
            #endregion

            #region DespawnEntities
            public void DespawnAllEntities()
            {
                respawnTimer.ResetTimer(configData.RespawnTime);
                foreach (KeyValuePair<uint, BaseEntity> entity in spawnedBaseEntityList)
                {
                    try
                    {
                        entity.Value.Kill();
                        spawnedEntityList.Remove(entity.Key);
                    }
                    catch (Exception e)
                    {
                        instance.Puts("Couldn't delete entity " + entity.Key + " " + e.ToString());
                    }
                }
                instance.UpdateHarborData();
            }
            #endregion

            #region Spawn Entities
            public void SpawnAllEntities()
            {
                if (harborInfo != null)
                {
                    instance.Puts("Spawning entities");

                    try { spawnedBaseEntityList.Clear(); instance.Puts("spawnedBaseEntityList cleared "); }

                    catch (Exception e) { instance.Puts("spawnedBaseEntityList clear or null" + e.Message); }

                    if (configData.SpawnCargoShip)
                    {
                        try { SpawnShip(); } catch (Exception e) { instance.Puts("Ship spawn error " + e.Message); }
                    }

                    if (configData.SpawnBradley)
                    {
                        try { SpawnBradley(); } catch (Exception e) { instance.Puts("Bradley spawn error " + e.Message); }
                    }

                    if (configData.SpawnStrongPoints)
                    {
                        try { SpawnOutposts(); } catch (Exception e) { instance.Puts("Outpost spawn error " + e.Message); }
                    }

                    if (configData.SpawnMilitaryPersonnel)
                    {
                        try { SpawnMilitaryPersonnel(); } catch (Exception e) { instance.Puts("Soldiers spawn error " + e.Message); }
                    }

                    if (configData.SpawnSAMSites)
                    {
                        try { SpawnSAMSites(); } catch (Exception e) { instance.Puts("SAMSites spawn error " + e.Message); }
                    }

                    if (configData.SpawnAdvancedCrates)
                    {
                        try { SpawnAdvancedCrates(); } catch (Exception e) { instance.Puts("AdvCrates spawn error " + e.Message); }
                    }

                    if (configData.SpawnAmmoCrates)
                    {
                        try { SpawnAmmoCrates(); } catch (Exception e) { instance.Puts("AmmoCrates spawn error " + e.Message); }
                    }

                    if (configData.SpawnHackableCrates)
                    {
                        try { SpawnHackableCrates(); } catch (Exception e) { instance.Puts("HackCrates spawn error " + e.Message); }
                    }
                }
                else
                {
                    instance.Puts("No monument !!!");
                }
                instance.SaveEntityData();
            }
            private void RotateObject(BaseEntity entity, bool rotateInner)
            {
                Quaternion monumentRotation = harborInfo.transform.rotation;
                Vector3 centerPoint = harborInfo.transform.position;

                entity.transform.RotateAround(centerPoint, Vector3.up, monumentRotation.eulerAngles.y + 62f);
                if (rotateInner)
                {
                    entity.transform.rotation = monumentRotation;
                }
            }
            private void SpawnBradley()
            {
                instance.Puts("Spawning Bradley");

                if (harborInfo != null)
                {
                    List<Vector3> path = new List<Vector3>
                    {
                        harborInfo.transform.TransformPoint(new Vector3(107f, 5.3f, -21f)),
                        harborInfo.transform.TransformPoint(new Vector3(33.6f, 5.2f, -21f)),
                        harborInfo.transform.TransformPoint(new Vector3(15f, 5.2f, -36.6f)),
                        harborInfo.transform.TransformPoint(new Vector3(-115f, 5.2f, -36f)),
                        harborInfo.transform.TransformPoint(new Vector3(15f, 5.2f, -36.6f)),
                        harborInfo.transform.TransformPoint(new Vector3(33.6f, 5.2f, -21f))
                    };

                    BaseEntity entity = GameManager.server.CreateEntity("assets/prefabs/npc/m2bradley/bradleyapc.prefab", harborInfo.transform.TransformPoint(new Vector3(107f, 5.3f, -21f)), Quaternion.identity, true);
                    bradley = entity.GetComponent<BradleyAPC>();
                    bradley.currentPath = path;
                    bradley.UpdateMovement_Patrol();
                    bradley.Spawn();

                    AddEntityToData(entity);
                }
                else
                {
                    instance.Puts("Monument is null");
                }
            }
            private void SpawnOutposts()
            {
                instance.Puts("Spawning Strongpoints");
                List<Vector3> outpostPositions = new List<Vector3>
                {
                    new Vector3(-30f, 5.1f, -18f),
                    new Vector3(-30f, 5.1f, -92f),
                    new Vector3(32f, 5.1f, -60f),
                    new Vector3(116f, 5.1f, -45f),
                    new Vector3(-123f, 5.1f, -53f)
                };
                foreach (Vector3 position in outpostPositions)
                {
                    SpawnOutpostEntity(position, harborInfo, 7.5f, true, false, true);
                }
                instance.UpdateHarborData();
            }
            private void SpawnMilitaryPersonnel()
            {
                instance.Puts("Spawning Military Personnel");
                int counter = 0;
                foreach (Vector3 point in GetRandomPointsList(40, 150))
                {
                    BaseEntity soldier = SpawnEntityMilitary(point, mobileSoldierPrefab, false);
                    counter++;
                }

                instance.Puts($"Spawned {counter} military personnel units!");
                instance.UpdateHarborData();
            }

            private void SpawnSAMSites()
            {
                instance.Puts("Spawning SAM sites");
                List<Vector3> SAMSitesPositions = new List<Vector3>
                {
                    new Vector3(2.5f, 29.3f, -18.7f)
                };

                foreach (Vector3 position in SAMSitesPositions)
                {
                    BaseEntity samSite = SpawnEntity(harborInfo.transform.TransformPoint(position), new Vector3(0, 0, 0), samPrefab, false);
                    samSite.GetComponent<SamSite>().isLootable = true;
                    samSite.GetComponent<SamSite>().UpdateHasPower(100, 1);
                    samSite.GetComponent<SamSite>().inventory.AddItem(ItemManager.FindItemDefinition("ammo.rocket.sam"), 200);
                }

                SAMSitesPositions.Clear();

                instance.UpdateHarborData();
            }

            private void SpawnAdvancedCrates()
            {
                //instance.Puts("Spawning advanced crates");

                //foreach (Vector3 point in GetRandomPointsList(3, 100))
                //{
                //    BaseEntity loot = SpawnEntity(point + new Vector3(0, -3f, 0), Vector3.zero, advancedCratePrefab, false);
                //    loot.GetComponent<LootContainer>().SpawnType = LootContainer.spawnType.CRASHSITE;
                //    loot.GetComponent<LootContainer>().maxDefinitionsToSpawn = 10;
                //    loot.GetComponent<LootContainer>().minSecondsBetweenRefresh = 8000;
                //}

                //instance.UpdateHarborData();
            }
            private void SpawnAmmoCrates()
            {
                //instance.Puts("Spawning ammo crates");

                //foreach (Vector3 point in GetRandomPointsList(3, 100))
                //{
                //    BaseEntity ammoCrate = SpawnEntity(point + new Vector3(0, -3f, 0), Vector3.zero, ammoCratePrefab, false);
                //    ammoCrate.GetComponent<LootContainer>().minSecondsBetweenRefresh = 8000;
                //}
                //instance.UpdateHarborData();
            }
            private void SpawnHackableCrates()
            {
                instance.Puts("Spawning hackable crates");

                foreach (Vector3 point in GetRandomPointsList(3, 100))
                {
                    BaseEntity crateEntity = SpawnEntity(point, Vector3.zero, hackableCratePrefab, false);
                    crateEntity.GetComponent<HackableLockedCrate>().CreateMapMarker(15f);
                }

                instance.UpdateHarborData();
            }
            private void SpawnShip()
            {
                var shipPosition = harborInfo.transform.TransformPoint(new Vector3(35f, 0f, 168f));
                var shipRotation = Quaternion.Euler(harborInfo.transform.rotation.eulerAngles + new Vector3(0f, 180f, 0f));

                BaseEntity entity = GameManager.server.CreateEntity("assets/content/vehicles/boats/cargoship/cargoshiptest.prefab", shipPosition, shipRotation, true);
                entity.Spawn();
                cargoShip = entity.GetComponent<CargoShip>();
                cargoShip.SpawnSubEntities();

                instance.UpdateHarborData();

                AddEntityToData(entity);
                SpawnShipTurrets(entity);
            }
            private void SpawnShipTurrets(BaseEntity shipEntity)
            {
                shipTurrets = new List<AutoTurret>();

                //right side
                List<Vector3> shipTurretsList = new List<Vector3>
                {
                    new Vector3(12f, 5f, -10f),
                    new Vector3(12f, 5f, 0f),
                    new Vector3(12f, 5f, 10f),
                    new Vector3(12f, 5f, 20f),
                    new Vector3(12f, 5f, 30f)
                };
                string turretPrefab = "assets/prefabs/npc/autoturret/autoturret_deployed.prefab";
                foreach (Vector3 point in shipTurretsList)
                {
                    //spawn turrents 
                    BaseEntity turret = GameManager.server.CreateEntity(turretPrefab, shipEntity.transform.TransformPoint(point), Quaternion.Euler(shipEntity.transform.rotation.eulerAngles + new Vector3(0f, 0, -90f)), true);
                    turret?.Spawn();

                    AutoTurret autoTurret = turret.GetComponent<AutoTurret>();

                    autoTurret.inventory.Clear();
                    ItemManager.CreateByName("lmg.m249", 1).MoveToContainer(autoTurret.inventory, 0);
                    ItemManager.CreateByName("ammo.rifle", 10000).MoveToContainer(autoTurret.inventory, 1);
                    autoTurret.UpdateAttachedWeapon();
                    autoTurret.Reload();
                    autoTurret.isLootable = false;

                    try { autoTurret.UpdateFromInput(100, 0); } catch { }
                    AddEntityToData(turret);
                    shipTurrets.Add(autoTurret);
                }

                shipTurretsList = new List<Vector3>
                {
                    new Vector3(-12f, 5f, -10f),
                    new Vector3(-12f, 5f, 0f),
                    new Vector3(-12f, 5f, 10f),
                    new Vector3(-12f, 5f, 20f),
                    new Vector3(-12f, 5f, 30f)
                };

                foreach (Vector3 point in shipTurretsList)
                {
                    Quaternion.Euler(new Vector3(0f, 0f, 90f));
                    BaseEntity turret = GameManager.server.CreateEntity(turretPrefab, shipEntity.transform.TransformPoint(point), Quaternion.Euler(shipEntity.transform.rotation.eulerAngles + new Vector3(0f, 0, 90f)), true);
                    turret?.Spawn();

                    AutoTurret autoTurret = turret.GetComponent<AutoTurret>();

                    autoTurret.inventory.Clear();
                    ItemManager.CreateByName("lmg.m249", 1).MoveToContainer(autoTurret.inventory, 0);
                    ItemManager.CreateByName("ammo.rifle", 10000).MoveToContainer(autoTurret.inventory, 1);
                    autoTurret.UpdateAttachedWeapon();
                    autoTurret.Reload();
                    autoTurret.isLootable = false;

                    try { autoTurret.UpdateFromInput(100, 0); } catch { }
                    AddEntityToData(turret);
                    shipTurrets.Add(autoTurret);
                }

                instance.UpdateHarborData();
            }

            private void SpawnOutpostEntity(Vector3 centerPoint, MonumentInfo monumentInfo, float size, bool spawnAmmoCrates, bool spawnAdvancedCrates, bool spawnSamSites)
            {
                BaseEntity towerEntity = GameManager.server.CreateEntity(watchTowerPrefab, monumentInfo.transform.TransformPoint(centerPoint), monumentInfo.transform.rotation, true);
                towerEntity.Spawn();
                AddEntityToData(towerEntity);

                float x = 3;
                float y = 0;
                float z = 3;

                // parent version
                List<Vector3> sandBagsPositions = new List<Vector3>
                {
                    new Vector3(x * 1, y, size),
                    new Vector3(x * 2, y, size),
                    new Vector3(x * -1, y, size),
                    new Vector3(x * -2, y, size),

                    new Vector3(x * 1, y, -size),
                    new Vector3(x * 2, y, -size),
                    new Vector3(x * -1, y, -size),
                    new Vector3(x * -2, y, -size)
                };

                List<Vector3> metalWirePositions = new List<Vector3>
                {
                    new Vector3(x * 1, y, size + 1.5f),
                    new Vector3(x * 2, y, size + 1.5f),
                    new Vector3(x * -1, y, size + 1.5f),
                    new Vector3(x * -2, y, size + 1.5f),

                    new Vector3(x * 1, y, -size - 1.5f),
                    new Vector3(x * 2, y, -size - 1.5f),
                    new Vector3(x * -1, y, -size - 1.5f),
                    new Vector3(x * -2, y, -size - 1.5f)
                };

                SpawnEntityFromList(sandBagsPositions, new Vector3(0, 0, 0), barricadePrefab, towerEntity, true);
                SpawnEntityFromList(metalWirePositions, new Vector3(0, 0, 0), metalWirePrefab, towerEntity, true);

                sandBagsPositions = new List<Vector3>
                {
                    new Vector3(size, y, z * 1),
                    new Vector3(size, y, z * 2),
                    new Vector3(size, y, z * -1),
                    new Vector3(size, y, z * -2),

                    new Vector3(-size, y, z * 1),
                    new Vector3(-size, y, z * 2),
                    new Vector3(-size, y, z * -1),
                    new Vector3(-size, y, z * -2)
                };

                metalWirePositions = new List<Vector3>
                {
                    new Vector3(size + 1.5f, y, z * 1),
                    new Vector3(size + 1.5f, y, z * 2),
                    new Vector3(size + 1.5f, y, z * -1),
                    new Vector3(size + 1.5f, y, z * -2),

                    new Vector3(-size - 1.5f, y, z * 1),
                    new Vector3(-size - 1.5f, y, z * 2),
                    new Vector3(-size - 1.5f, y, z * -1),
                    new Vector3(-size - 1.5f, y, z * -2)
                };

                SpawnEntityFromList(sandBagsPositions, new Vector3(0f, 90f, 0f), barricadePrefab, towerEntity, true);
                SpawnEntityFromList(metalWirePositions, new Vector3(0f, 90f, 0f), metalWirePrefab, towerEntity, true);
                float addY = 1.25f;
                y += addY;

                sandBagsPositions = new List<Vector3>
                {
                    new Vector3(x * 1, y, size),
                    new Vector3(x * -1, y, size),

                    new Vector3(x * 1, y, -size),
                    new Vector3(x * -1, y, -size)
                };
                SpawnEntityFromList(sandBagsPositions, new Vector3(0f, 0f, 0f), barricadePrefab, towerEntity, true);

                sandBagsPositions = new List<Vector3>
                {
                    new Vector3(size, y, z * 1f),
                    new Vector3(size, y, z * -1f),

                    new Vector3(-size, y, z * 1f),
                    new Vector3(-size, y, z * -1f)
                };

                SpawnEntityFromList(sandBagsPositions, new Vector3(0f, 90f, 0f), barricadePrefab, towerEntity, true);
                y -= addY;
                if (spawnAmmoCrates)
                {
                    //List<Vector3> cratesLocations = new List<Vector3>
                    //{
                    //    new Vector3(size - 1, y, -size + 4),
                    //    new Vector3(size - 1, y, -size + 2),
                    //    new Vector3(size - 1, y, -size + 3)
                    //};

                    //SpawnEntityFromList(cratesLocations, new Vector3(0, 0, 0), ammoCratePrefab, towerEntity, true);
                }
                if (spawnSamSites)
                {
                    List<Vector3> samPositions = new List<Vector3>
                    {
                        new Vector3(-5f, y, -5f),
                        new Vector3(5f, y, -5f)
                    };

                    SpawnEntityFromList(samPositions, Vector3.zero, samPrefab, towerEntity, true);
                }

                float offset = 4f;

                List<Vector3> gunnerPositions = new List<Vector3>
                {
                    new Vector3(0f, 7.3f, 0f),
                    new Vector3(offset, y, offset),
                    new Vector3(-offset, y, offset),
                    new Vector3(offset, y, -offset),
                    new Vector3(-offset, y, -offset)
                };

                SpawnEntityFromList(gunnerPositions, Vector3.zero, outpostScientist, towerEntity, true);


            }
            private void SpawnEntityFromList(List<Vector3> list, Vector3 rot, string prefab, BaseEntity parentEntity, bool setAsActive)
            {
                foreach (Vector3 point in list)
                {

                    Vector3 pos = parentEntity.transform.TransformPoint(point);
                    Quaternion rotation = new Quaternion
                    {
                        eulerAngles = parentEntity.transform.rotation.eulerAngles + rot
                    };

                    BaseEntity entity = GameManager.server.CreateEntity(prefab, pos, rotation, setAsActive);
                    if (entity.GetComponent<Barricade>() != null)
                    {
                        entity.GetComponent<Barricade>().canNpcSmash = false;
                        entity.GetComponent<Barricade>().reflectDamage = 0f;
                        entity.GetComponent<Barricade>().Spawn();
                        AddEntityToData(entity.GetComponent<Barricade>());
                    }
                    else
                    {
                        entity.Spawn();
                    }

                    AddEntityToData(entity);
                }
            }
            private BaseEntity SpawnEntity(Vector3 pos, Vector3 rot, string prefab, bool rotateInner)
            {
                Quaternion rotNew = new Quaternion
                {
                    eulerAngles = rot
                };

                BaseEntity entity = GameManager.server.CreateEntity(prefab, pos, rotNew, true);

                RotateObject(entity, rotateInner);
                entity.Spawn();
                AddEntityToData(entity);
                return entity;
            }
            private BaseEntity SpawnEntityMilitary(Vector3 pos, string prefab, bool rotateInner)
            {
                BaseEntity entity = GameManager.server.CreateEntity(prefab, pos, Quaternion.identity, true);

                RotateObject(entity, rotateInner);
                entity.Spawn();

                militaryPersonnelList.Add(entity);
                AddEntityToData(entity);
                return entity;
            }
            #endregion

            #region Helpers
            private void AddEntityToData(BaseEntity entity)
            {
                if (!spawnedEntityList.ContainsKey(entity.net.ID))
                {
                    spawnedEntityList.Add(entity.net.ID, entity.ShortPrefabName);
                }
                if (!spawnedBaseEntityList.ContainsKey(entity.net.ID))
                {
                    spawnedBaseEntityList.Add(entity.net.ID, entity);
                }
            }
            #endregion
            public void DeployFlare()
            {
                string projectile = "assets/prefabs/ammo/40mmgrenade/40mm_grenade_he.prefab";
                foreach (Vector3 position in FlareSpawnPoints())
                {
                    BaseEntity projectilet = GameManager.server.CreateEntity(projectile, position);
                    if (projectilet != null)
                    {
                        projectilet.SendMessage("InitializeVelocity", Vector3.up * 30f);
                        projectilet.Spawn();
                        projectileList.Add(projectilet);
                    }
                }
            }

            private List<Vector3> FlareSpawnPoints()
            {
                if (spawnPoints.Count == 0)
                {
                    foreach (KeyValuePair<uint, BaseEntity> entity in spawnedBaseEntityList)
                    {
                        if (entity.Value.ShortPrefabName.Contains("scientist"))
                        {
                            Vector3 updatedPoint = new Vector3(entity.Value.transform.position.x, entity.Value.transform.position.y + 0.25f, entity.Value.transform.position.z);
                            spawnPoints.Add(updatedPoint);

                        }
                    }
                }
                return spawnPoints;
            }

            private BaseEntity CreateFlare(Vector3 position)
            {
                BaseEntity ent = GameManager.server.CreateEntity(flarePrefab, position, new Quaternion(), true);
                ent?.Spawn();

                LightEx projectileLight = ent.gameObject.AddComponent<LightEx>();
                projectileLight.colorA = Color.green;
                projectileLight.colorB = Color.green;
                projectileLight.alterColor = false;
                projectileLight.randomOffset = true;
                projectileLight.transform.parent = ent.transform;
                return ent;
            }
            #region GUI
            public void ShowGUI()
            {
                foreach (BasePlayer player in BasePlayer.activePlayerList)
                {
                    container = new CuiElementContainer();
                    if (respawnTimer != null)
                    {
                        TimeSpan timeSpan = TimeSpan.FromSeconds(respawnTimer.GetCurrentTimer());
                        int hr = timeSpan.Hours;
                        int mn = timeSpan.Minutes;
                        int sec = timeSpan.Seconds;

                        string anchorMaxDynamic = GetCuiSizeMax(0.199f, 0.048f, respawnTimer.GetCurrentTimer());

                        CuiLabel text = new CuiLabel
                        {
                            RectTransform = { AnchorMin = "0.02 0.02", AnchorMax = "0.2 0.05" },
                            Text = { Text = " Harbor respawn in: " + hr + ":" + mn + ":" + sec, FontSize = 14, Color = "1 1 1 1", Align = TextAnchor.MiddleLeft }
                        };

                        container.Add(text, "Hud", "text_" + player.net.ID.ToString());
                    }

                    CuiHelper.AddUi(player, container);
                }
            }
            private string GetCuiSizeMax(float maxx, float maxy, float time)
            {
                float startTime = configData.RespawnTime;
                float newMaxx = (time / startTime) * maxx;
                string anchorMax = newMaxx.ToString() + " " + maxy.ToString();

                return anchorMax;
            }
            public void DestroyGUI()
            {
                foreach (BasePlayer player in BasePlayer.activePlayerList)
                {
                    CuiHelper.DestroyUi(player, "background_" + player.net.ID.ToString());
                    CuiHelper.DestroyUi(player, "progressBar_" + player.net.ID.ToString());
                    CuiHelper.DestroyUi(player, "text_" + player.net.ID.ToString());
                }
            }

            public void DestroyGUI(BasePlayer player)
            {
                if (player != null)
                {
                    CuiHelper.DestroyUi(player, "background_" + player.net.ID.ToString());
                    CuiHelper.DestroyUi(player, "progressBar_" + player.net.ID.ToString());
                    CuiHelper.DestroyUi(player, "text_" + player.net.ID.ToString());
                }
                else
                {

                }
            }
            public void DestroyGUIInactive()
            {
                foreach (BasePlayer player in BasePlayer.sleepingPlayerList)
                {
                    CuiHelper.DestroyUi(player, "background_" + player.net.ID.ToString());
                    CuiHelper.DestroyUi(player, "progressBar_" + player.net.ID.ToString());
                    CuiHelper.DestroyUi(player, "text_" + player.net.ID.ToString());
                }
            }
            public void UpdateGUI()
            {
                DestroyGUI();
                if (ShowTimer == true)
                {
                    ShowGUI();
                }
            }
            #endregion
        }
        #endregion
    }
}