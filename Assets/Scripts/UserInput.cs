 using UnityEngine;
using System.Collections;

public class UserInput : InputInterface 
{	
	
	void Update ()
	{
		if (Input.GetJoystickNames ().Length != 0) 
		{ //XboxController
			userThrottle = Input.GetAxis ("RT");
			userBrake = Input.GetAxis ("LT");

			userLeftStickHorizontal = Input.GetAxis ("Horizontal");
			userLeftStickVertical = Input.GetAxis ("Vertical");

			userRightStickHorizontal = Input.GetAxis ("Horizontal2");
			userRightStickVertical = Input.GetAxis ("Vertical2");

			//			if (Input.GetButton ("RB") && !Input.GetButton ("LB")) {
			//				userRoll = -1f;
			//			} else {
			//				if (Input.GetButton ("LB") && !Input.GetButton ("RB")) {
			//					userRoll = 1f;
			//				} else {
			//					userRoll = 0f;
			//				}
			//			}	
		} else 
		{ //Keyboard
			userThrottle = 0;
			userBrake = 0;
            if (Input.GetKey(KeyCode.W))
            {
                userThrottle = 1f;
            }
			if(Input.GetKey (KeyCode.S)) userBrake = 1f;	

			if (Input.GetKey (KeyCode.D) && !Input.GetKey (KeyCode.A)) {
				userLeftStickHorizontal = 1f;
			} else {
				if (!Input.GetKey (KeyCode.D) && Input.GetKey (KeyCode.A)) {
					userLeftStickHorizontal = -1f;
				} else {
					userLeftStickHorizontal = 0f;
				}
			}	

			//			if (Input.GetKey (KeyCode.W) && !Input.GetKey (KeyCode.S)) {
			//				userLeftStickVertical = 1f;
			//			} else {
			//				if (!Input.GetKey (KeyCode.W) && Input.GetKey (KeyCode.S)) {
			//					userLeftStickVertical = -1f;
			//				} else {
			//					userLeftStickVertical = 0f;
			//				}
			//			}	


			if (Input.GetKey (KeyCode.UpArrow) && !Input.GetKey (KeyCode.DownArrow)) {
				userRightStickVertical = 1f;
			} else {
				if (!Input.GetKey (KeyCode.UpArrow) && Input.GetKey (KeyCode.DownArrow)) {
					userRightStickVertical = -1f;
				} else {
					userRightStickVertical = 0f;
				}
			}	
			if (Input.GetKey (KeyCode.RightArrow) && !Input.GetKey (KeyCode.LeftArrow)) {
				userRightStickHorizontal = 1f;
			} else {
				if (!Input.GetKey (KeyCode.RightArrow) && Input.GetKey (KeyCode.LeftArrow)) {
					userRightStickHorizontal = -1f;
				} else {
					userRightStickHorizontal = 0f;
				}
			}	

			//			if (Input.GetKey (KeyCode.Q) && !Input.GetKey (KeyCode.E)) {
			//				userRoll = 1f;
			//			} else {
			//				if (!Input.GetKey (KeyCode.Q) && Input.GetKey (KeyCode.E)) {
			//					userRoll = -1f;
			//				} else {
			//					userRoll = 0f;
			//				}
			//			}	
		}


	}

}
