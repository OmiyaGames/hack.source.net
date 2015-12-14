using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TimerText : MonoBehaviour
{
    [SerializeField]
    string format = "0.00";
    Text label;

	// Use this for initialization
	void Start () {
        label = GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
        label.text = Network.time.ToString(format);
	}
}
