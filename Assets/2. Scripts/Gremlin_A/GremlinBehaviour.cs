using System;
using UnityEngine;

[Serializable]
public abstract class GremlinBehaviour : MonoBehaviour
{
    public Action OnTick { get; set; }
    public abstract void Tick();
}
