using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.Manager;
using GameFramework.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GameFramework.UI
{
    public class UIManager : IManager, IManagerUpdatable
    {
        private readonly UISettings _settings;
        private readonly Dictionary<Type, UIForm> _opened = new Dictionary<Type, UIForm>();
        private readonly Dictionary<Type, GameObject> _prefabs = new Dictionary<Type, GameObject>();
        private readonly Dictionary<UILayer, Transform> _layers = new Dictionary<UILayer, Transform>();
        private readonly Dictionary<UILifetime, List<Type>> _formsByLifetime = new Dictionary<UILifetime, List<Type>>();
        private readonly List<IUIUpdatable> _updatables = new List<IUIUpdatable>();

        private ResourceManager _resourceManager;
        private GameObject _uiRoot;
        private Camera _uiCamera;
        private EventSystem _eventSystem;
        private InputSystemUIInputModule _inputModule;

        public Camera UICamera => _uiCamera;
        public EventSystem EventSystem => _eventSystem;
        public InputSystemUIInputModule InputModule => _inputModule;

        public UIManager(UISettings settings)
        {
            _settings = settings;
        }

        public UniTask InitAsync()
        {
            _resourceManager = GameFrameworkManager.Instance.GetManager<ResourceManager>();
            if (_settings == null)
            {
                Log.Error("[UIManager] 未配置 UISettings");
                return UniTask.CompletedTask;
            }

            CreateRoot();
            BuildFormCatalog();
            return UniTask.CompletedTask;
        }

        public void DoUpdate(float dt)
        {
            for (int i = 0; i < _updatables.Count; i++)
                _updatables[i].DoUpdate(dt);
        }

        public void Dispose()
        {
            CloseAll();
            _prefabs.Clear();
            if (_uiRoot != null)
                UnityEngine.Object.Destroy(_uiRoot);
        }

        public async UniTask<T> OpenAsync<T>() where T : UIForm
        {
            Type type = typeof(T);
            if (_opened.TryGetValue(type, out UIForm existing))
                return (T)Reveal(existing);

            T form = await CreateFormAsync<T>();
            RegisterOpened(form);
            form.NotifyCreate();
            form.NotifyOpen();
            return form;
        }

        public async UniTask<T> OpenAsync<T, TArgs>(TArgs args) where T : UIForm<TArgs>
        {
            Type type = typeof(T);
            if (_opened.TryGetValue(type, out UIForm existing))
                return (T)Reveal(existing);

            T form = await CreateFormAsync<T>();
            RegisterOpened(form);
            form.NotifyCreate();
            form.NotifyOpen(args);
            return form;
        }

        public void Close<T>() where T : UIForm
        {
            Close(typeof(T));
        }

        public void Close(UIForm form)
        {
            if (form == null)
                return;
            Close(form.GetType());
        }

        public void Hide<T>() where T : UIForm
        {
            if (!_opened.TryGetValue(typeof(T), out UIForm form))
                return;

            form.gameObject.SetActive(false);
            form.NotifyHide();
        }

        public void Show<T>() where T : UIForm
        {
            if (!_opened.TryGetValue(typeof(T), out UIForm form))
                return;

            form.gameObject.SetActive(true);
            form.NotifyShow();
        }

        public T Get<T>() where T : UIForm
        {
            return _opened.TryGetValue(typeof(T), out UIForm form) ? (T)form : null;
        }

        public bool IsOpen<T>() where T : UIForm
        {
            return _opened.ContainsKey(typeof(T));
        }

        public async UniTask PreloadAsync<T>() where T : UIForm
        {
            await LoadPrefabAsync(typeof(T));
        }

        public async UniTask PreloadAsync(UILifetime lifetime)
        {
            if (!_formsByLifetime.TryGetValue(lifetime, out List<Type> types))
                return;

            for (int i = 0; i < types.Count; i++)
                await LoadPrefabAsync(types[i]);
        }

        public void Close(UILifetime lifetime)
        {
            List<Type> types = new List<Type>(_opened.Keys);
            for (int i = 0; i < types.Count; i++)
            {
                UIFormAttribute attr = ResolveAttribute(types[i]);
                if (attr.Lifetime == lifetime)
                    Close(types[i]);
            }
        }

        public void CloseAll()
        {
            List<Type> types = new List<Type>(_opened.Keys);
            for (int i = 0; i < types.Count; i++)
                Close(types[i]);
        }

        private void Close(Type type)
        {
            if (!_opened.TryGetValue(type, out UIForm form))
                return;

            if (form is IUIUpdatable updatable)
                _updatables.Remove(updatable);

            form.NotifyClose();
            _opened.Remove(type);
            if (form != null && form.gameObject != null)
                UnityEngine.Object.Destroy(form.gameObject);
        }

        private static UIForm Reveal(UIForm form)
        {
            if (!form.gameObject.activeSelf)
            {
                form.gameObject.SetActive(true);
                form.NotifyShow();
            }

            return form;
        }

        private void RegisterOpened(UIForm form)
        {
            _opened.Add(form.GetType(), form);
            if (form is IUIUpdatable updatable)
                _updatables.Add(updatable);
        }

        private async UniTask<T> CreateFormAsync<T>() where T : UIForm
        {
            Type type = typeof(T);
            GameObject prefab = await LoadPrefabAsync(type);
            UIFormAttribute attr = ResolveAttribute(type);
            Transform parent = _layers[attr.Layer];

            GameObject instance = UnityEngine.Object.Instantiate(prefab, parent);
            instance.name = type.Name;

            T form = instance.GetComponent<T>();
            if (form == null)
            {
                Log.Error($"[UIManager] {type.Name} 预制体缺少 {type.Name} 组件");
                UnityEngine.Object.Destroy(instance);
                return null;
            }

            return form;
        }

        private async UniTask<GameObject> LoadPrefabAsync(Type type)
        {
            if (_prefabs.TryGetValue(type, out GameObject cached))
                return cached;

            UIFormAttribute attr = ResolveAttribute(type);
            string address = attr.Address;
            if (string.IsNullOrEmpty(address))
            {
                if (_settings == null || string.IsNullOrEmpty(_settings.PrefabRoot))
                {
                    Log.Error($"[UIManager] {type.Name} 未配置 Address，且 UISettings.PrefabRoot 为空");
                    return null;
                }

                address = $"{_settings.PrefabRoot}/{type.Name}.prefab";
            }

            GameObject prefab = await _resourceManager.LoadAssetAsync<GameObject>(address);
            if (prefab == null)
            {
                Log.Error($"[UIManager] 加载 {type.Name} 失败: {address}");
                return null;
            }

            _prefabs.Add(type, prefab);
            return prefab;
        }

        private void BuildFormCatalog()
        {
            _formsByLifetime.Clear();
            foreach (UILifetime life in Enum.GetValues(typeof(UILifetime)))
                _formsByLifetime[life] = new List<Type>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int a = 0; a < assemblies.Length; a++)
            {
                Type[] types;
                try
                {
                    types = assemblies[a].GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }

                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];
                    if (type.IsAbstract || !typeof(UIForm).IsAssignableFrom(type))
                        continue;

                    UIFormAttribute attr = ResolveAttribute(type);
                    _formsByLifetime[attr.Lifetime].Add(type);
                }
            }
        }

        private static UIFormAttribute ResolveAttribute(Type type)
        {
            UIFormAttribute attr = (UIFormAttribute)Attribute.GetCustomAttribute(type, typeof(UIFormAttribute));
            return attr ?? new UIFormAttribute(UILayer.Page);
        }

        private void CreateRoot()
        {
            int uiLayer = LayerMask.NameToLayer("UI");

            _uiRoot = new GameObject("UIRoot");
            _uiRoot.layer = uiLayer;
            UnityEngine.Object.DontDestroyOnLoad(_uiRoot);

            GameObject cameraObj = new GameObject("UICamera");
            cameraObj.transform.SetParent(_uiRoot.transform);
            cameraObj.transform.localPosition = new Vector3(0f, 0f, -_settings.CameraDistance);
            _uiCamera = cameraObj.AddComponent<Camera>();
            _uiCamera.orthographic = true;
            _uiCamera.cullingMask = LayerMask.GetMask("UI");
            _uiCamera.clearFlags = CameraClearFlags.Depth;
            cameraObj.AddComponent<AudioListener>();

            GameObject eventObj = new GameObject("EventSystem");
            eventObj.transform.SetParent(_uiRoot.transform);
            _eventSystem = eventObj.AddComponent<EventSystem>();
            _inputModule = eventObj.AddComponent<InputSystemUIInputModule>();
            _inputModule.pointerBehavior = UIPointerBehavior.AllPointersAsIs;

            Canvas canvas = _uiRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = _uiCamera;
            canvas.sortingOrder = _settings.SortingOrder(UILayer.Page);

            CanvasScaler scaler = _uiRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = _settings.ReferenceResolution;
            scaler.matchWidthOrHeight = _settings.MatchWidthOrHeight;

            _uiRoot.AddComponent<GraphicRaycaster>();

            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                GameObject layerObj = new GameObject(layer.ToString());
                layerObj.transform.SetParent(_uiRoot.transform, false);
                layerObj.layer = uiLayer;

                RectTransform rect = layerObj.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;

                Canvas layerCanvas = layerObj.AddComponent<Canvas>();
                layerCanvas.overrideSorting = true;
                layerCanvas.sortingOrder = _settings.SortingOrder(layer);
                layerObj.AddComponent<GraphicRaycaster>();

                _layers.Add(layer, layerObj.transform);
            }
        }
    }
}
