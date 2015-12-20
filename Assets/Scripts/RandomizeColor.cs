using UnityEngine;

public class RandomizeColor : MonoBehaviour
{
    public const int ConsistentSeed = 10101;

    [SerializeField]
    Renderer[] allRenderers;
    [SerializeField]
    Color minRange;
    [SerializeField]
    Color maxRange;
    [SerializeField]
    bool makeConsistent = false;

    [Header("Randomizer")]
    [SerializeField]
    Vector2 scaleRange = new Vector2(0.5f, 1.5f);

    // Use this for initialization
    [ContextMenu("Randomize Color")]
    void Start ()
    {
        HSBColor min = HSBColor.FromColor(minRange);
        HSBColor max = HSBColor.FromColor(maxRange);
        HSBColor newColor = new HSBColor();

        int seed = Random.seed;
        if(makeConsistent == true)
        {
            Random.seed = ConsistentSeed;
        }
        foreach (Renderer renderer in allRenderers)
        {
            foreach(Material material in renderer.materials)
            {
                newColor.Hue = Random.value;
                newColor.Saturation = Random.Range(min.Saturation, max.Saturation);
                newColor.Brightness = Random.Range(min.Brightness, max.Brightness);
                material.color = newColor.ToColor();
            }
        }
        if (makeConsistent == true)
        {
            Random.seed = seed;
        }
    }

    // Update is called once per frame
    [ContextMenu("Get All Renderers")]
    void GetRenderers()
    {
        allRenderers = GetComponentsInChildren<Renderer>();
    }

    // Update is called once per frame
    [ContextMenu("Random Scale")]
    void RandomizeScale()
    {
        Vector3 angles;
        foreach (Renderer renderer in allRenderers)
        {
            renderer.transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);
            angles = renderer.transform.eulerAngles;
            angles.y = Random.Range(0f, 360f);
            renderer.transform.rotation = Quaternion.Euler(angles);
        }
    }
}
