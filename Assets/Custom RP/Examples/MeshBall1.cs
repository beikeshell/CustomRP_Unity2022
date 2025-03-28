using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeshBall1 : MonoBehaviour
{
    
    [SerializeField]
    Material material = default;
    
    private void Awake()
    {
        for (var i = 0; i < 100; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.GetComponent<Renderer>().material = material;
            go.AddComponent<PerObjectMaterialProperties>();
        }
    }
}