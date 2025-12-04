using UnityEngine;
using Unity.Cinemachine;
using UnityEditor;
using System.Collections;

public class MonitorTarget : MonoBehaviour
{
    public CinemachineCamera monitorCamera;
    private GameObject spawnedInstance;
    private LocalEventManager events;
    public MonoScript screenCore;
    public RenderTexture screenTexture;
    private GameObject prefab;
    
    private void Start()
    {
        MonitorsManager manager = MonitorsManager.Instance;
        manager.monitorsLength++;

        prefab = Resources.Load<GameObject>("Monitors/MonitorPrefab");
        System.Type type = screenCore.GetClass();

        GameObject instance = Instantiate(prefab);
        instance.AddComponent(type);

        MonitorElements elements = instance.GetComponent<MonitorElements>();
        Camera camera = elements.UICamera;
        camera.targetTexture = screenTexture;

        spawnedInstance = instance;

        events = GetComponent<LocalEventManager>();

        if (events != null)
        {
            events.Subscribe("OnFocus", OnFocus);
            events.Subscribe("OnBlur", OnBlur);
        }

        Transform transform = instance.GetComponent<Transform>();
        transform.position = new Vector3(manager.monitorsLength * 1000, 0, 0);
    }

    private void OnFocus()
    {
        LocalEventManager events = spawnedInstance.GetComponent<LocalEventManager>();
        events?.Invoke("OnFocus");
    }

    private void OnBlur()
    {
        LocalEventManager events = spawnedInstance.GetComponent<LocalEventManager>();
        events?.Invoke("OnBlur");
    }

}
