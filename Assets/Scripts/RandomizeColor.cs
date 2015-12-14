using UnityEngine;

public class RandomizeColor : MonoBehaviour
{
    [SerializeField]
    Renderer[] allRenderers;
    [SerializeField]
    Color minRange;
    [SerializeField]
    Color maxRange;

    // Use this for initialization
    void Start ()
    {
        HSBColor min = HSBColor.FromColor(minRange);
        HSBColor max = HSBColor.FromColor(maxRange);
        HSBColor newColor = new HSBColor();
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
	}
	
	// Update is called once per frame
    [ContextMenu("Get All Renderers")]
	void GetRenderers()
    {
        allRenderers = GetComponentsInChildren<Renderer>();
	}
}
