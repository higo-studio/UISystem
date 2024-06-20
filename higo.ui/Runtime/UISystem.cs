using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Higo.UI
{
    public class UISystem : MonoBehaviour
    {
        protected struct DelayCall
        {
            public bool isOpen;
            public string path;
            public bool isExclusive;
            public UIUUID uuid;
            public int layerIndex;
        }

        protected List<UILayerData> m_Layers = new();
        protected Queue<DelayCall> m_DelayProcessingUIPanelDatas = new();
        protected Dictionary<UIUUID, GameObject> m_Instances = new();
        protected Dictionary<string, GameObject> m_Loaded = new();
        protected HashSet<string> m_Loading = new();
        protected int m_UUIDGenerator = 1;

        protected int m_Depth = 0;

        protected static UISystem m_Instance;
        public static UISystem Instance => m_Instance;
        public string PathPrefix = "";

        private void OnEnable()
        {
            register();
        }

        private void OnDisable()
        {
            unregister();
        }

        private void register()
        {
            if (m_Instance == null)
            {
                m_Instance = this;
                DontDestroyOnLoad(m_Instance.gameObject);

                var childCount = transform.childCount;
                for (var i = 0; i < childCount; i++)
                {
                    var child = transform.GetChild(i);
                    var data = new UILayerData()
                    {
                        Root = child
                    };
                    m_Layers.Add(data);
                    if (child.TryGetComponent<IUILayer>(out var layerC))
                    {
                        layerC.Init(i);
                    }
                }
            }
        }

        private void unregister()
        {
            if (m_Instance == this)
            {
                m_Layers.Clear();
                m_Instances.Clear();
                m_Loaded.Clear();
                m_Loading.Clear();
                m_UUIDGenerator = 0;
                m_Depth = 0;
                m_DelayProcessingUIPanelDatas.Clear();

                var scene = SceneManager.GetActiveScene();
                if (scene.isLoaded)
                {
                    SceneManager.MoveGameObjectToScene(m_Instance.gameObject, scene);
                }
                m_Instance = null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void EnsureCreation()
        {
            if (m_Instance != null)
            {
                return;
            }
            var settings = Resources.Load<UISystemSettings>(nameof(UISystemSettings));
            if (settings != null && settings.Prefab != null)
            {
                m_Instance = Instantiate(settings.Prefab);
                m_Instance.name = "UI System";
            }
            else
            {
                var go = new GameObject("UI System");
                m_Instance = go.AddComponent<UISystem>();
            }
            m_Instance.register();
        }

        public bool TryFindUUID(int layerIndex, string path, out UIUUID uuid)
        {
            if (layerIndex < 0 || layerIndex >= m_Layers.Count)
            {
                uuid = default;
                return false;
            }
            var layer = m_Layers[layerIndex];
            if (path == null)
            {
                if (layer.Panels.Count > 0)
                {
                    uuid = layer.Panels[layer.Panels.Count - 1].UUID;
                    return true;
                }
                else
                {
                    uuid = default;
                    return false;
                }
            }
            var index = layer.Panels.FindIndex(x => x.Path == path);
            if (index < 0)
            {
                uuid = default;
                return false;
            }
            uuid = layer.Panels[index].UUID;
            return true;
        }

        public bool TryGetUI(UIUUID uuid, out GameObject go) => m_Instances.TryGetValue(uuid, out go);

        public UIUUID OpenUI(int layerIndex, string path, bool isExclusive = true)
        {
            Assert.IsTrue(m_Depth >= 0);
            Assert.IsTrue(layerIndex >= 0 && layerIndex < m_Layers.Count);
            var layer = m_Layers[layerIndex];
            var uuid = new UIUUID(layerIndex, layer.UUIDGenerator++);
            if (m_Depth > 0)
            {
                m_DelayProcessingUIPanelDatas.Enqueue(new DelayCall()
                {
                    isOpen = true,
                    path = path,
                    isExclusive = isExclusive,
                    uuid = uuid,
                    layerIndex = layerIndex,
                });
                return uuid;
            }
            m_Depth++;
            InternalOpenUI(layerIndex, uuid, path, isExclusive);
            m_Depth--;
            if (m_Depth == 0)
                HandleDelayCalls();

            return uuid;
        }

        protected void InternalOpenUI(int layerIndex, UIUUID uuid, string path, bool isExclusive)
        {
            var layer = m_Layers[layerIndex];
            var layerController = layer.Root.GetComponent<IUILayer>();
            if (isExclusive)
            {
                var pauseList = ListPool<PanelInfo>.Get();
                var i = layer.Panels.Count - 1;
                while (i >= 0)
                {
                    var d = layer.Panels[i];
                    Assert.AreEqual(d.State, UIPanelData.States.Shown);
                    d.State = UIPanelData.States.Freezed;
                    layer.Panels[i] = d;
                    i--;

                    if (m_Instances.TryGetValue(d.UUID, out var instance))
                    {
                        var ctx = new UIPauseContext();
                        foreach (var comp in instance.GetComponents<IUIPanelPause>())
                        {
                            comp.OnPause(ref ctx);
                        }
                    }
                    pauseList.Add(new PanelInfo()
                    {
                        Uuid = d.UUID,
                        Path = d.Path
                    });
                    if (d.IsExclusive) break;
                }
                if (layerController != null)
                    layerController.OnPanelPause(pauseList);
                ListPool<PanelInfo>.Release(pauseList);
            }

            var showList = ListPool<PanelInfo>.Get();
            var data = new UIPanelData()
            {
                UUID = uuid,
                Path = path,
                IsExclusive = isExclusive,
                State = UIPanelData.States.Shown,
            };
            layer.Panels.Add(data);
            showList.Add(new PanelInfo()
            {
                Uuid = uuid,
                Path = path
            });

            LoadAsset(path);
            if (m_Loaded.TryGetValue(path, out var asset))
            {
                doInstantiateWithOnShow(layerIndex, data, asset);
            }

            if (layerController != null)
                layerController.OnPanelShow(showList);
            ListPool<PanelInfo>.Release(showList);
        }

        private void LoadAsset(string path)
        {
            if (m_Loading.Contains(path) || m_Loaded.ContainsKey(path)) return;
            // var request = Resources.LoadAsync(path);
            // request.completed += oper => CheckLoadingQueue(oper, path);
            // m_Loading.Add(path);
            m_Loading.Remove(path);
            m_Loaded[path] = (GameObject)Resources.Load($"{PathPrefix}/{path}");
        }

        private void CheckLoadingQueue(AsyncOperation oper, string path)
        {
            m_Loading.Remove(path);
            if (oper is ResourceRequest req && req.asset is not null)
            {
                var asset = (GameObject)req.asset;
                m_Loaded[path] = asset;
                for (int i = 0; i < m_Layers.Count; i++)
                {
                    var layer = m_Layers[i];
                    foreach (var data in layer.Panels)
                    {
                        if (data.Path != path || data.State != UIPanelData.States.Shown) continue;
                        doInstantiateWithOnShow(i, data, asset);
                    }
                }
            }
        }

        private GameObject doInstantiate(int layerIndex, UIPanelData data, GameObject asset)
        {
            if (asset == null) return null;

            var uuid = data.UUID;

            var instance = Instantiate(asset, m_Layers[layerIndex].Root);
            m_Instances[uuid] = instance;
            var panelInfo = new PanelInfo()
            {
                Uuid = uuid,
                Path = data.Path
            };
            foreach (var initComp in instance.GetComponents<IUIPanelInit>())
            {
                initComp.OnInit(panelInfo);
            }

            return instance;
        }

        private GameObject doInstantiateWithOnShow(int layerIndex, UIPanelData data, GameObject asset)
        {
            if (asset == null) return null;
            var instance = doInstantiate(layerIndex, data, asset);

            foreach (var showComp in instance.GetComponents<IUIPanelShow>())
            {
                showComp.OnShow();
            }
            return instance;
        }

        public bool CloseUI(UIUUID uuid)
        {
            Assert.IsTrue(m_Depth >= 0);
            var layer = m_Layers[uuid.LayerIndex];
            var index = layer.Panels.FindIndex(x => x.UUID == uuid);
            if (index < 0) return false;

            if (m_Depth > 0)
            {
                m_DelayProcessingUIPanelDatas.Enqueue(new DelayCall()
                {
                    isOpen = false,
                    uuid = uuid,
                });
                return true;
            }
            m_Depth++;
            InternalCloseUI(uuid.LayerIndex, index);
            m_Depth--;
            if (m_Depth == 0)
                HandleDelayCalls();
            return true;
        }

        public bool CloseUI(int layerIndex, string path = null)
            => TryFindUUID(layerIndex, path, out var uuid) && CloseUI(uuid);

        protected void InternalCloseUI(int layerIndex, int index)
        {
            if (index < 0) return;
            var layer = m_Layers[layerIndex];
            var layerController = layer.Root.GetComponent<IUILayer>();
            var data = layer.Panels[index];
            data.State = UIPanelData.States.Hided;
            layer.Panels.RemoveAt(index);

            var hideList = ListPool<PanelInfo>.Get();
            if (m_Instances.TryGetValue(data.UUID, out var removed))
            {
                var ctx = new UIHideContext();
                foreach (var comp in removed.GetComponents<IUIPanelHide>())
                {
                    comp.OnHide(ref ctx);
                }

                if (!ctx.DontDestroy)
                {
                    Destroy(removed);
                }
                hideList.Add(new PanelInfo()
                {
                    Uuid = data.UUID,
                    Path = data.Path,
                });
            }
            if (layerController != null)
                layerController.OnPanelHide(hideList);
            ListPool<PanelInfo>.Release(hideList);

            var resumeList = ListPool<PanelInfo>.Get();
            if (data.IsExclusive && index > 0)
            {
                var start = index - 1;
                while (start > 0 && !layer.Panels[start].IsExclusive)
                {
                    start--;
                }

                for (int i = start; i < index; i++)
                {
                    var d = layer.Panels[i];
                    var previous1 = d.State;
                    Assert.AreEqual(previous1, UIPanelData.States.Freezed);
                    d.State = UIPanelData.States.Shown;
                    layer.Panels[i] = d;
                    Debug.Log($"OnResume: {d.UUID}, {d.Path}, {d.IsExclusive}");

                    if (!m_Instances.TryGetValue(d.UUID, out var resumed) && m_Loaded.TryGetValue(d.Path, out var asset))
                    {
                        doInstantiate(i, d, asset);
                    }
                    if (resumed != null)
                    {
                        foreach (var comp1 in resumed.GetComponents<IUIPanelResume>())
                        {
                            comp1.OnResume();
                        }

                    }
                    resumeList.Add(new PanelInfo()
                    {
                        Uuid = d.UUID,
                        Path = d.Path
                    });
                }
            }
            if (layerController != null)
                layerController.OnPanelResume(resumeList);
            ListPool<PanelInfo>.Release(resumeList);
        }

        protected void HandleDelayCalls()
        {
            if (m_DelayProcessingUIPanelDatas == null || m_DelayProcessingUIPanelDatas.Count == 0) return;
            while (m_DelayProcessingUIPanelDatas.TryDequeue(out var data))
            {
                if (data.isOpen)
                {
                    OpenUI(data.layerIndex, data.path, data.isExclusive);
                }
                else
                {
                    CloseUI(data.uuid);
                }
            }
        }

        protected virtual void onPostUIChanged() { }

        public void DestroyUI(UIUUID uuid)
        {
            if (TryGetUI(uuid, out var go))
            {
                Destroy(go);
            }
        }

        public IReadOnlyUILayerData GetLayer(int layerIndex)
        {
            if (layerIndex >= m_Layers.Count) return null;
            return m_Layers[layerIndex];
        }
    }
}