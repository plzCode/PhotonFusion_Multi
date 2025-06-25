using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShapesFX
{
    public class SC_EffectControl : MonoBehaviour
    {
        private Renderer rend;
        public Material mat;
        [Header("Influencer1")]

        public GameObject Target;
        private Vector3 TargetPosition;

        [Header("Influencer2")]
        public GameObject Target2;
        private Vector3 TargetPosition2;

        [Header("Influencer3")]
        public GameObject Target3;
        private Vector3 TargetPosition3;

        [Header("Influencer4")]
        public GameObject Target4;
        private Vector3 TargetPosition4;


        void Start()
        {
            rend = GetComponent<Renderer>();
            mat = rend.material;

        }

        void Update()
        {
            if (Target)
            {
                mat.SetFloat("_Activate_Target", 1);
                TargetPosition = Target.transform.position;
                mat.SetVector("_target", TargetPosition);
            }


            if (Target2)
            {
                mat.SetFloat("_Activate_Target_2", 1);
                TargetPosition2 = Target2.transform.position;
                mat.SetVector("_target2", TargetPosition2);
            }


            if (Target3)
            {
                mat.SetFloat("_Activate_Target_3", 1);
                TargetPosition3 = Target3.transform.position;
                mat.SetVector("_target3", TargetPosition3);
            }


            if (Target4)
            {
                mat.SetFloat("_Activate_Target_4", 1);
                TargetPosition4 = Target4.transform.position;
                mat.SetVector("_target4", TargetPosition4);
            }

        }
    }
}