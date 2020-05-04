using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class CubeMovement : MonoBehaviour
    {
        private GameObject cube;
        
        public float scaleRange = -0.01f;
        public float positionRange = -0.005f;
        
        private Vector3 scaleChange, positionChange;

        void Awake()
        {
            Time.timeScale = 3f;
            cube = gameObject;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            scaleRange = Random.Range(-0.01f, -0.02f);
            positionRange = -scaleRange * 100 * -0.005f;
            scaleChange = new Vector3(0, scaleRange, 0);
            positionChange = new Vector3(0, positionRange, 0.0f);
        }

        void Update()
        {
            cube.transform.localScale += scaleChange;
            cube.transform.position += positionChange;

            // Move upwards when the sphere hits the floor or downwards
            // when the sphere scale extends 1.0f.
            if (cube.transform.localScale.y < 0.1f || cube.transform.localScale.y > 1.0f)
            {
                scaleChange = -scaleChange;
                positionChange = -positionChange;
            }
        }
        
        IEnumerator CubeTransition()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
            }
        }
    }
}