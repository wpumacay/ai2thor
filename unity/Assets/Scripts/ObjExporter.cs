using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ObjExporter
{
    private int m_StartIndex = 0;

    public void Start()
    {
        m_StartIndex = 0;
    }

    public void End()
    {
        m_StartIndex = 0;
    }

    public string MeshToString(MeshFilter mf, Transform tf)
    {
        Vector3 scale = tf.localScale;
        Vector3 pos = tf.localPosition;
        Quaternion rot = tf.localRotation;

        int numVertices = 0;
        Mesh mesh = mf.sharedMesh;
        if (!mesh)
        {
            return "####Error####";
        }

        var strBuilder = new StringBuilder();
        foreach (var vv in mesh.vertices)
        {
            var v = tf.TransformPoint(vv);
            numVertices++;
            strBuilder.AppendLine($"v {v.x} {v.y} {v.z}");
        }
        strBuilder.Append("\n");
        foreach (var nn in mesh.normals)
        {
            var v = rot * nn;
            strBuilder.AppendLine($"vn {v.x} {v.y} {v.z}");
        }
        strBuilder.Append("\n");
        foreach (var uv in mesh.uv)
        {
            strBuilder.AppendLine($"vt {uv.x} {uv.y}");
        }
        for (var idx = 0; idx < mesh.subMeshCount; idx++)
        {
            int[] triangles = mesh.GetTriangles(idx);
            for (var i = 0; i < triangles.Length; i += 3)
            {
                strBuilder.AppendLine(
                    string.Format(
                        "f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                        triangles[i] + 1 + m_StartIndex,
                        triangles[i + 1] + 1 + m_StartIndex,
                        triangles[i + 2] + 1 + m_StartIndex
                    )
                );
            }
        }

        m_StartIndex += numVertices;
        return strBuilder.ToString();
    }

    public string ProcessTransform(Transform tf, bool makeSubmeshes)
    {
        var strBuilder = new StringBuilder();

        if (tf.TryGetComponent<MeshFilter>(out var meshFilter))
        {
            strBuilder.AppendLine($"# {tf.name}");
            strBuilder.AppendLine($"#---------------");

            if (makeSubmeshes)
            {
                strBuilder.AppendLine($"g {tf.name}");
            }

            strBuilder.Append(MeshToString(meshFilter, tf));
        }

        for (var i = 0; i < tf.childCount; i++)
        {
            strBuilder.Append(ProcessTransform(tf.GetChild(i), makeSubmeshes));
        }

        return strBuilder.ToString();
    }

    public string Export(GameObject go, string fileName, bool makeSubmeshes)
    {
        Start();

        var strBuilder = new StringBuilder();

        strBuilder.AppendLine("----------------------------------------------");
        strBuilder.AppendLine($"# {fileName}.obj");
        strBuilder.AppendLine($"# Exported using iThor OBJ exporter");
        strBuilder.AppendLine($"# {System.DateTime.Now.ToLongDateString()}");
        strBuilder.AppendLine($"# {System.DateTime.Now.ToLongTimeString()}");
        strBuilder.AppendLine("----------------------------------------------");

        var tf = go.transform;
        var originalPosition = tf.position;
        tf.position = Vector3.zero;

        if (!makeSubmeshes)
        {
            strBuilder.AppendLine($"g {tf.name}");
        }

        strBuilder.Append(ProcessTransform(tf, makeSubmeshes));

        tf.position = originalPosition;

        End();

        return strBuilder.ToString();
    }
}

public class ObjExporterTools : MonoBehaviour
{
    [MenuItem("Tools/Export Selection to OBJ")]
    static void ExportSelectionWithSubmeshes()
    {
        ExportSelectionToObj(true);
    }

    [MenuItem("Tools/Export Selection to OBJ (No Submeshes)")]
    static void ExportSelectionWithoutSubmeshes()
    {
        ExportSelectionToObj(false);
    }

    static void ExportSelectionToObj(bool makeSubmeshes)
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("No GameObject selected to export");
            return;
        }

        string meshName = Selection.activeGameObject.name;
        string fileName = EditorUtility.SaveFilePanel("Export .obj file", "", meshName, "obj");

        var objExporter = new ObjExporter();
        var objStr = objExporter.Export(Selection.activeGameObject, fileName, makeSubmeshes);

        WriteToFile(fileName, objStr);
        Debug.Log($"Exported Mesh: {fileName}");
    }

    static void WriteToFile(string fileName, string content)
    {
        using (var sw = new StreamWriter(fileName))
        {
            sw.Write(content);
        }
    }
}
