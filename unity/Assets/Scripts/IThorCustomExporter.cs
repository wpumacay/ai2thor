using UnityEditor;
using UnityEngine;

public class IThorCustomExporter
{
    private ObjExporter m_ObjExporter = null;

    IThorCustomExporter()
    {
        m_ObjExporter = new ObjExporter();
    }

    public void ExportToJSON(GameObject go)
    {
        ///
    }

    public void ExportToMJCF(GameObject go)
    {
        ///
    }
}

public class IThorCustomExporterTools : MonoBehaviour
{
    [MenuItem("Tools/IThor - Export GameObject")]
    static void ExportGameObject()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("No GameObject selected to export");
            return;
        }
    }
}
