using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
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

        private void Start()
        {
            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                m_Layers.Add(new()
                {
                    Root = child
                });
            }
        }

        private void OnEnable()
        {
            if (m_Instance == null)
            {
                m_Instance = this;
                DontDestroyOnLoad(m_Instance.gameObject);
            }
        }

        private void OnDisable()
        {
            if (m_Instance == this)
            {
                var scene = SceneManager.GetActiveScene();
                if (scene.isLoaded)
                {
                    SceneManager.MoveGameObjectToScene(m_Instance.gameObject, scene);
                }
                m_Instance = null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
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
            }
            else
            {
                var go = new GameObject("UI System");
                m_Instance = go.AddComponent<UISystem>();
            }
        }

        public bool TryFindUUID(int layerIndex, string path, out UIUUID uuid)
        {
            if (layerIndex < 0 || layerIndex >= m_Layers.Count)
            {
                uuid = default;
                return false;
            }
            var layer = m_Layers[layerIndex];
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
            if (isExclusive)
            {
                var i = layer.Panels.Count - 1;
                while (i >= 0)
                {
                    var d = layer.Panels[i];
                    Assert.AreEqual(d.State, UIPanelData.States.Shown);
                    d.State = UIPanelData.States.Freezed;
                    layer.Panels[i] = d;
                    i--;

                    Debug.Log($"OnPause: {d.UUID}, {d.Path}, {d.IsExclusive}");
                    if (m_Instances.TryGetValue(d.UUID, out var instance))
                    {
                        var comp = instance.GetComponent<IUIPanelPause>();
                        if (comp != null)
                        {
                            comp.OnPause();
                        }
                    }
                    if (d.IsExclusive) break;
                }
            }

            var data = new UIPanelData()
            {
                UUID = uuid,
                Path = path,
                IsExclusive = isExclusive,
                State = UIPanelData.States.Shown,
            };
            layer.Panels.Add(data);

            LoadAsset(path);
            if (m_Loaded.TryGetValue(path, out var asset))
            {
                doInstantiateWithOnShow(layerIndex, uuid, asset);
            }

            Debug.Log($"OnShow: {uuid}, {path}, {isExclusive}");
            onPostUIChanged();
        }

        private void LoadAsset(string path)
        {
            if (m_Loading.Contains(path) || m_Loaded.ContainsKey(path)) return;
            // var request = Resources.LoadAsync(path);
            // request.completed += oper => CheckLoadingQueue(oper, path);
            // m_Loading.Add(path);
            m_Loading.Remove(path);
            m_Loaded[path] = (GameObject)Resources.Load(path);
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
                        doInstantiateWithOnShow(i, data.UUID, asset);
                    }
                }
            }
        }

        private GameObject doInstantiate(int layerIndex, UIUUID uuid, GameObject asset)
        {
            var instance = Instantiate(asset, m_Layers[layerIndex].Root);
            m_Instances[uuid] = instance;
            var initComp = instance.GetComponent<IUIPanelInit>();
            if (initComp != null)
            {
                initComp.OnInit(uuid);
            }

            return instance;
        }

        private GameObject doInstantiateWithOnShow(int layerIndex, UIUUID uuid, GameObject asset)
        {
            var instance = doInstantiate(layerIndex, uuid, asset);

            var showComp = instance.GetComponent<IUIPanelShow>();
            if (showComp != null)
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

        public bool CloseUI(int layerIndex, string path)
            => TryFindUUID(layerIndex, path, out var uuid) && CloseUI(uuid);

        protected void InternalCloseUI(int layerIndex, int index)
        {
            var layer = m_Layers[layerIndex];
            if (index < 0) return;
            var data = layer.Panels[index];
            var previous = data.State;
            data.State = UIPanelData.States.Hided;
            layer.Panels.RemoveAt(index);

            if (m_Instances.TryGetValue(data.UUID, out var removed))
            {
                var comp = removed.GetComponent<IUIPanelHide>();
                if (comp != null)
                {
                    comp.OnHide();
                }
                Destroy(removed);
            }

            Debug.Log($"OnHide: {data.UUID}, {data.Path}, {data.IsExclusive}");

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
                        doInstantiate(i, d.UUID, asset);
                    }
                    if (resumed != null)
                    {
                        var comp1 = resumed.GetComponent<IUIPanelResume>();
                        if (comp1 != null)
                        {
                            comp1.OnResume();
                        }
                    }
                }
            }

            onPostUIChanged();
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
    }
}