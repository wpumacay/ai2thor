using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class IThorHouseExporter : MonoBehaviour
{
    [System.Serializable]
    class IThorExportNode
    {
        public string objectType = string.Empty;
        public string assetId = string.Empty;
        public Vector3 position;
        public Vector3 rotation;
        public bool kinematic;
    }

    [System.Serializable]
    class IThorExportNodeList
    {
        public List<IThorExportNode> objects;
    }

    void Start()
    {
        Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        GameObject[] objects = scene.GetRootGameObjects();

        var nodes = new List<IThorExportNode>();

        var stack = new Stack<(GameObject, GameObject)>();
        foreach (var obj in objects)
            stack.Push((obj, null));

        while (stack.Count > 0)
        {
            var (obj, parent) = stack.Pop();
            if (obj.GetComponent<SimObjPhysics>())
            {
                var simObjPhysics = obj.GetComponent<SimObjPhysics>();
                var exportNode = new IThorExportNode
                {
                    objectType = Enum.GetName(typeof(SimObjType), simObjPhysics.Type),
                    assetId = simObjPhysics.assetID,
                    position = obj.transform.position,
                    rotation = obj.transform.rotation.eulerAngles,
                    kinematic = simObjPhysics.isStatic,
                };

                nodes.Add(exportNode);
            }

            foreach (Transform childTf in obj.transform)
                stack.Push((childTf.gameObject, obj));
        }

        // Use the wrapper for JsonUtility to serialize the list
        var nodesList = new IThorExportNodeList{objects = nodes};

        var json = JsonUtility.ToJson(nodesList, true);
        File.WriteAllText(Path.Combine(Application.dataPath, "IThorHouseExport.json"), json);
    }
}
