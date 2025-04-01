using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;

public class IThorHouseExporter : MonoBehaviour
{
    [System.Serializable]
    class IThorExportNode
    {
        public string objectType = string.Empty;
        public string assetId = string.Empty;
        public Vector3 position;
        public Quaternion rotation;
        public bool kinematic;
    }



    void Start()
    {
        Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        GameObject[] objects = scene.GetRootGameObjects();

        // var nodesCache = new Dictionary<string, IThorExportNode>();
        var nodesCache = new List<IThorExportNode>();

        var stack = new Stack<(GameObject, GameObject)>();
        foreach (var obj in objects)
            stack.Push((obj, null));

        while (stack.Count > 0)
        {
            var (obj, parent) = stack.Pop();
            if (obj.GetComponent<SimObjPhysics>()) {
                var simObj = obj.GetComponent<SimObjPhysics>();
                var exportNode = new IThorExportNode
                {
                    objectType = obj.name,
                    assetId = simObj.assetID,
                    position = obj.transform.position,
                    rotation = obj.transform.rotation,
                    kinematic = true,
                };

                nodesCache.Add(exportNode);
                // nodesCache.Add(obj.name, exportNode);
            }

            foreach (Transform childTf in obj.transform)
            {
                stack.Push((childTf.gameObject, obj));
            }
        }

        var jsonStorage = new Dictionary<string, List<IThorExportNode>> {
            { "objects", nodesCache }
        };

        var json = JsonUtility.ToJson(nodesCache, true);
        File.WriteAllText(Application.dataPath + "/IThorHouseExport.json", json);
    }
}
