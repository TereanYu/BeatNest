using UnityEngine;
using System.Collections;

namespace SCPE
{
    public class CharacterMovement : MonoBehaviour
    {

        public float walkSpeed = 6.0F;
        public float runSpeed = 8.0F;
        public float gravity = 20.0F;

        private float speed = 5.0F;

        private Vector3 moveDirection = Vector3.zero;
        private Rigidbody rbody;

        void Awake()
        {
            rbody = GetComponent<Rigidbody>();
            rbody.freezeRotation = true;
        }

        void Update()
        {

            //vector3 Input horizontal are the buttons A and D and vertical is W and S
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            //TransformDirection to get the rotation and move in the way its rotated
            moveDirection = transform.TransformDirection(moveDirection);

            //Reset gravity
            moveDirection.y = 0;
            //Get input sprint(Shift) 
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed = runSpeed;
            }
            //If you are not pressing shift the speed is walkspeed
            else
            {
                speed = walkSpeed;
            }

            //moveDirection is either a value between -1 and 1 so you multiply it by a float so you will be moving faster
            moveDirection *= speed;

            rbody.velocity = moveDirection;
        }

    }
}