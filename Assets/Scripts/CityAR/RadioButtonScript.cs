using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CityAR
{
    public class RadioButtonScript : MonoBehaviour
    {
        private ToggleGroup _toggleGroup;

        private void Start()
        {
            _toggleGroup = GetComponent<ToggleGroup>();
        }

        public void OnSubmit()
        {
            Toggle toggle = _toggleGroup.ActiveToggles().FirstOrDefault();
            Debug.Log(toggle.name + "_" + toggle.GetComponentInChildren<Text>().text);
            VisualizationCreator.mode = toggle.GetComponentInChildren<Text>().text;
            GameObject.Find("Platform").GetComponent<VisualizationCreator>().BuildCity();
        }
    }
}
