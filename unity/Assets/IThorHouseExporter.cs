using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IThorHouseExporter : MonoBehaviour
{
    [System.Serializable]
    class IThorExportNode
    {
        public string objectName = string.Empty;
        public string objectType = string.Empty;
        public string parentName = string.Empty; // Structural, Lighting, etc.
        public string assetId = string.Empty;
        public string customId = string.Empty;
        public Vector3 position;
        public Vector3 rotation;
        public bool kinematic;
    }

    [System.Serializable]
    class IThorExportLightNode
    {
        public string name = string.Empty;
        public string type = string.Empty;
        public string parentName = string.Empty; // Structural, Lighting, etc.
        public Vector3 position;
        public Vector3 rotation;
        public Color color;
        public float intensity;
        public bool castShadows;
    }

    [System.Serializable]
    class IThorExportNodeList
    {
        public List<IThorExportNode> objects;
        public List<IThorExportLightNode> lights;
    }

    [MenuItem("Tools/IThor - Export Current Scene")]
    static void ExportCurrentScene()
    {
        Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        GameObject[] objects = scene.GetRootGameObjects();

        var fileNameJSON = EditorUtility.SaveFilePanel("Export .json file", "", scene.name, "json");
        var folderPath = Path.GetDirectoryName(fileNameJSON);

        var objExporter = new ObjExporter();

        var nodes = new List<IThorExportNode>();
        var lightNodes = new List<IThorExportLightNode>();

        var stack = new Stack<(GameObject, GameObject)>();
        foreach (var obj in objects)
            stack.Push((obj, null));

        while (stack.Count > 0)
        {
            var (obj, parent) = stack.Pop();
            if (obj.GetComponent<SimObjPhysics>())
            {
                var simObjPhysics = obj.GetComponent<SimObjPhysics>();
                var node = new IThorExportNode
                {
                    objectName = obj.name,
                    objectType = Enum.GetName(typeof(SimObjType), simObjPhysics.Type),
                    parentName = parent != null ? parent.name : string.Empty,
                    assetId = simObjPhysics.assetID,
                    position = obj.transform.position,
                    rotation = obj.transform.rotation.eulerAngles,
                    kinematic = simObjPhysics.isStatic,
                    customId = string.Empty,
                };

                // Export custom geometry
                if (simObjPhysics.assetID == "")
                {
                    var fileNameObj = Path.Combine(folderPath, "models", $"{obj.name}.obj");
                    var fileNameMtl = Path.Combine(folderPath, "models", $"{obj.name}.mtl");

                    var fileNameNoExt = Path.GetFileNameWithoutExtension(fileNameObj);
                    var fileDirectory = Path.GetDirectoryName(fileNameObj);

                    node.customId = fileNameNoExt;

                    var (objMeshStr, objMaterialStr) = objExporter.Export(
                        obj,
                        fileNameNoExt,
                        fileDirectory,
                        true
                    );

                    using (var writer = new StreamWriter(fileNameObj))
                        writer.Write(objMeshStr);
                    using (var writer = new StreamWriter(fileNameMtl))
                        writer.Write(objMaterialStr);
                }

                nodes.Add(node);
            }
            else if (obj.GetComponent<Light>())
            {
                var light = obj.GetComponent<Light>();
                var lightNode = new IThorExportLightNode
                {
                    name = light.name,
                    type = Enum.GetName(typeof(LightType), light.type).ToLower(),
                    parentName = parent != null ? parent.name : string.Empty,
                    position = obj.transform.position,
                    rotation = obj.transform.rotation.eulerAngles,
                    color = light.color,
                    intensity = light.intensity,
                    castShadows = light.shadows != LightShadows.None,
                };

                lightNodes.Add(lightNode);
            }

            foreach (Transform childTf in obj.transform)
                stack.Push((childTf.gameObject, obj));
        }

        // Use the wrapper for JsonUtility to serialize the list
        var nodesList = new IThorExportNodeList { objects = nodes, lights = lightNodes };

        var json = JsonUtility.ToJson(nodesList, true);
        File.WriteAllText(fileNameJSON, json);
    }
}
