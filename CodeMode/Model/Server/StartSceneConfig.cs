using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace ET
{
    public partial class StartSceneConfigCategory
    {
        private readonly MultiMap<int, StartSceneConfig> processScenes = new();

        private readonly Dictionary<long, Dictionary<string, StartSceneConfig>> scenesByName = new();

        private readonly Dictionary<long, MultiMap<int, StartSceneConfig>> sceneByType = new();
        
        public List<StartSceneConfig> GetByProcess(int process)
        {
            return this.processScenes[process];
        }
        
        public StartSceneConfig GetBySceneName(int zone, string name)
        {
            return this.scenesByName[zone][name];
        }
        
        public List<StartSceneConfig> GetBySceneType(int zone, int type)
        {
            return this.sceneByType[zone][type];
        }
        
        public StartSceneConfig GetOneBySceneType(int zone, int type)
        {
            return this.sceneByType[zone][type][0];
        }

        public override void EndInit()
        {
            foreach (StartSceneConfig startSceneConfig in this.GetAll().Values)
            {
                this.processScenes.Add(startSceneConfig.Process, startSceneConfig);
                
                if (!this.scenesByName.ContainsKey(startSceneConfig.Zone))
                {
                    this.scenesByName.Add(startSceneConfig.Zone, new Dictionary<string, StartSceneConfig>());
                }
                this.scenesByName[startSceneConfig.Zone].Add(startSceneConfig.Name, startSceneConfig);
                
                if (!this.sceneByType.ContainsKey(startSceneConfig.Zone))
                {
                    this.sceneByType.Add(startSceneConfig.Zone, new MultiMap<int, StartSceneConfig>());
                }
                this.sceneByType[startSceneConfig.Zone].Add(startSceneConfig.Type, startSceneConfig);
            }
        }
    }
    
    public partial class StartSceneConfig
    {
        public ActorId ActorId;
        
        public int Type;

        public StartProcessConfig StartProcessConfig
        {
            get
            {
                return StartProcessConfigCategory.Instance.Get(this.Process);
            }
        }
        
        public StartZoneConfig StartZoneConfig
        {
            get
            {
                return StartZoneConfigCategory.Instance.Get(this.Zone);
            }
        }

        // 内网地址外网端口，通过防火墙映射端口过来
        private IPEndPoint innerIPPort;

        public IPEndPoint InnerIPPort
        {
            get
            {
                if (innerIPPort == null)
                {
                    this.innerIPPort = NetworkHelper.ToIPEndPoint($"{this.StartProcessConfig.InnerIP}:{this.Port}");
                }

                return this.innerIPPort;
            }
        }

        private IPEndPoint outerIPPort;

        // 外网地址外网端口
        public IPEndPoint OuterIPPort
        {
            get
            {
                return this.outerIPPort ??= NetworkHelper.ToIPEndPoint($"{this.StartProcessConfig.OuterIP}:{this.Port}");
            }
        }

        public override void EndInit()
        {
            this.ActorId = new ActorId(this.Process, this.Id, 1);
            this.Type = SceneTypeSingleton.Instance.GetSceneType(this.SceneType);
        }
    }
}