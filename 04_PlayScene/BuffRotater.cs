using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffRotater : MonoBehaviour
{
    private float m_rotationSpeed = 140f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, m_rotationSpeed * Time.deltaTime);
    }
}
