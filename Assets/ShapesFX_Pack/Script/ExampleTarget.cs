using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShapesFX
{
    public class ExampleTarget : MonoBehaviour
    {
        public float speed;
        public Vector3 direction = new Vector3(1, 0, 0);

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //transform.Translate(direction * Mathf.Sin(Time.fixedTime * speed));
            transform.position = (direction * Mathf.Sin(Time.fixedTime * speed));
        }
    }

}