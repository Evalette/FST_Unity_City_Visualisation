using System.Linq;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace CityAR
{
    public class IMixedRealityTouchHandler : MonoBehaviour, Microsoft.MixedReality.Toolkit.Input.IMixedRealityTouchHandler
    {

        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            Entry entry = gameObject.transform.GetComponent<VisualizationCreator.EntryData>().entry;
            var message = "name: " + gameObject.transform.name +
                          "\n #linesOfCode: " + entry.numberOfLines +
                          "\n #Interfaces: " + entry.numberOfInterfaces +
                          "\n #Methods: " + entry.numberOfMethods +
                          "\n #AbstractClasses: " + entry.numberOfAbstractClasses;
                          
            
            TooltipManager._instance.SetAndShowToolTip(message);
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            TooltipManager._instance.HideToolTip();
        }

        public void OnTouchUpdated(HandTrackingInputEventData eventData)
        {
        }
    }
}
