using UnityEngine;
namespace NorthernLab {
[CreateAssetMenu(menuName="NorthernLab/Station balance")]
public class StationConfig : ScriptableObject {
 public float shiftSeconds=225, fuelCapacity=110, fuelPerSecond=.13f, coldPerSecond=.24f, warmthPerSecond=.45f;
 public int generatorPower=5, boostPower=8;
 public int[] demand={2,1,3,2};
 public float sosSeconds=85, repairSeconds=3, warningSeconds=9;
 public Shift[] shifts;
 [System.Serializable] public class Shift { public string title, briefing, task, record; public int parts; public float heatingMultiplier=1; }
}
[System.Serializable] public class SaveData {
 public int shift=1, parts, radio, records, upgrades, flares=3; public float fuel=70, temperature=65, sos; public bool fuse, generator, watered;
}
public enum DeviceKind { Fuel, Generator, Board, Heater, Tools, Fuse, Radio, Parts, Record, Shutter, Lift, Plant, Flare }
}
