using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquidGame {

    public class CameraTarget : MonoBehaviour {

        private void Update() {
            Vector3 moveDir = new Vector3(0, 0);

            /*
            if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
            if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
            if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
            if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;
            */
            if (Input.GetKey(KeyCode.W)) moveDir += transform.forward * +1f;
            if (Input.GetKey(KeyCode.S)) moveDir += transform.forward * -1f;
            if (Input.GetKey(KeyCode.A)) moveDir += transform.right * -1f;
            if (Input.GetKey(KeyCode.D)) moveDir += transform.right * +1f;

            float moveSpeed = 20f;
            transform.position += moveDir * moveSpeed * Time.deltaTime;



            float rotationSpeed = 90f;
            if (Input.GetKey(KeyCode.Q)) {
                transform.eulerAngles += new Vector3(0, +1, 0) * rotationSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.E)) {
                transform.eulerAngles += new Vector3(0, -1, 0) * rotationSpeed * Time.deltaTime;
            }
        }

    }

}