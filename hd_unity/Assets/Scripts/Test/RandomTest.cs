using System;
using UnityEngine;

namespace Test
{
    public class RandomTest : MonoBehaviour
    {
        private void Awake()
        {
            for (int i = 0; i < 1000; i++)
            {
                int random=  UnityEngine.Random.Range(0, 2);
                if(random==0)
                    Debug.Log("========"+random);
            }
         
        }
    }
}