using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine;

public class ARManager : MonoBehaviour
{
    //ARPlaneManager planeTracker;
    public GameObject battleGame;
    public ARRaycastManager arRaycastManager;
    private List<ARRaycastHit> arRaycastHits = new List<ARRaycastHit>();
    void Awake() {
        //planeTracker.planesChanged += onPlaneDetection;
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {            
            var touch = Input.GetTouch(0);            
            if (touch.phase == TouchPhase.Ended)
            {  
                if (Input.touchCount == 1)
                {
                    if(arRaycastManager.Raycast(touch.position, arRaycastHits)) 
                    {
                        var pose = arRaycastHits[0].pose;   
                        //battleGame.SetActive(true);                     
                        //battleGame.transform.position = pose.position;
                        return;
                    }
                }
            }
        }
    }

    void onPlaneDetection(ARPlanesChangedEventArgs list) {
        // Get the list of any kinds of change from the last call
        List<ARPlane> newPlane = list.added;
        List<ARPlane> lostPlane = list.removed;
        List<ARPlane> updatePlane = list.updated;
        foreach(ARPlane p in list.added) {
            p.boundaryChanged += onSinglePlaneUpdate;
        }
    }
 
    void onSinglePlaneUpdate(ARPlaneBoundaryChangedEventArgs args) {
        ARPlane updatedPlane = args.plane;
    }
}
