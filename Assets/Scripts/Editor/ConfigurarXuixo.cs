using UnityEditor;
using UnityEngine;

/// <summary>
/// Eina d'editor: posa l'script Xuixo (classe mare) al prefab "pickup"
/// i marca el seu collider com a trigger perquè la nau el pugui agafar.
/// Menú de Unity: AstroXuixos > Configurar prefab xuixo (pickup)
/// </summary>
public static class ConfigurarXuixo
{
    const string RutaPrefab = "Assets/Prefabs/pickup.prefab";

    [MenuItem("AstroXuixos/Configurar prefab xuixo (pickup)")]
    public static void Configurar()
    {
        GameObject arrel = PrefabUtility.LoadPrefabContents(RutaPrefab);
        if (arrel == null)
        {
            Debug.LogError("[Xuixo] No trobo el prefab " + RutaPrefab);
            return;
        }

        try
        {
            if (arrel.GetComponent<Xuixo>() == null)
                arrel.AddComponent<Xuixo>();

            foreach (Collider2D c in arrel.GetComponentsInChildren<Collider2D>())
                c.isTrigger = true;

            arrel.tag = "Pickup";

            PrefabUtility.SaveAsPrefabAsset(arrel, RutaPrefab);
            Debug.Log("[Xuixo] Prefab pickup configurat: té l'script Xuixo i el collider és trigger.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(arrel);
        }
    }
}
