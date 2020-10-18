using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectZoneController : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Detect zone catched ? "+other.gameObject.tag.ToString());
        if (this.gameObject.GetComponentInParent<DefendScript>() != null)
            this.gameObject.GetComponentInParent<DefendScript>().PullTriggerFromDetectZone(other);
    }
}
