using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("SAR/SARProjector")]
public class SARProjectorBoss : Singleton<SARProjectorBoss> {

    [Header("The location to find SARProjector matrices")]
    [TextArea]
    public string matrixDirectory;
}
