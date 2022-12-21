using UnityEngine;

namespace CityAR
{
    public class Tooltip : MonoBehaviour
    {

        private void OnMouseEnter()
        {
            Entry entry = GameObject.Find(gameObject.transform.name).GetComponent<Entry>();
            var message = "name: " + gameObject.transform.name + "\n #linesOfCode: " + entry.numberOfLines;
            
            TooltipManager._instance.SetAndShowToolTip(message);
        }

        private void OnMouseExit()
        {
            TooltipManager._instance.HideToolTip();
        }
    }
}