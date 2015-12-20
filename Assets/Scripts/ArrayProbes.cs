using UnityEngine;
using System.Collections.Generic;

public class ArrayProbes : MonoBehaviour
{
    [SerializeField]
    ReflectionProbe centerProbe;
    [SerializeField]
    int dimension = 5;
    [SerializeField]
    Vector2 range = new Vector2(32, 32);
    [SerializeField]
    List<ReflectionProbe> newProbes = new List<ReflectionProbe>();

    [ContextMenu("Create Probes")]
    void CreateProbes()
    {
        CleanupProbes();
        GameObject clone = centerProbe.gameObject;
        Vector3 position;
        int diff = (dimension - 1) / 2;
        for (int x = -diff; x <= diff; ++x)
        {
            for (int y = -diff; y <= diff; ++y)
            {
                // Skip the origin
                if((x == 0) && (y == 0))
                {
                    continue;
                }

                // Create a reflection probe here
                clone = Instantiate<GameObject>(centerProbe.gameObject);
                clone.transform.SetParent(centerProbe.transform.parent);

                // Positon everything!
                position = centerProbe.transform.position;
                position.x += x * range.x;
                position.z += y * range.y;
                clone.transform.position = position;

                newProbes.Add(clone.GetComponent<ReflectionProbe>());
            }
        }
    }

    [ContextMenu("Cleanup Probes")]
    void CleanupProbes()
    {
        if(newProbes != null)
        {
            foreach(ReflectionProbe probe in newProbes)
            {
                DestroyImmediate(probe.gameObject);
            }
            newProbes.Clear();
        }
    }
}
