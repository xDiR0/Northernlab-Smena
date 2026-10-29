using UnityEngine;
namespace NorthernLab {
public class StationDevice : MonoBehaviour {
 public DeviceKind kind; public string title; public int floor, unlockShift=1, room; public bool consumed; public Vector2 position { get {return transform.position;} }
}
}
