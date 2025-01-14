using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAthletictrack : MonoBehaviour
{

    public GameObject Movingfloor;
    public GameObject Movingperson;
    public float Movespeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Move the object forward along its z axis 1 unit/second.
        Movingfloor.transform.Translate(Vector3.forward * Time.deltaTime);
        Movingperson.transform.Translate(Vector3.back * Time.deltaTime/Movespeed);

    }
}
