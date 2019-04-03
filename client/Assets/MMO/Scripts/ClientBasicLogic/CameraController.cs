using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField]
	Transform
		target;
	[SerializeField]
	float
		distance = 4.0f;
	[SerializeField]
	float
		xSpeed = 250.0f;
	[SerializeField]
	float
		ySpeed = 120.0f;
	[SerializeField]
	float
		xAxisAngleMinLimit = 0f;
	[SerializeField]
	float
		xAxisAngleMaxLimit = 60f;
	[SerializeField]
	float
		angleYAxis = 0.0f;
	//jiaodu
	[SerializeField]
	float
		angleXAxis = 0.0f;
	//jiaodu
	[SerializeField]
	float minDis = 1.5f;
	[SerializeField]
	float maxDis = 6f;
	//	[SerializeField]
	//	bool Rotate = false;

	private Vector2 oldPosition1;
	private Vector2 oldPosition2;

	float dist;

	//	Vector3 originalAngle;
	public static CameraController instance;
	[SerializeField]
	#region 处理相机遮挡相关的 射线节点 ####################
	GameObject dir;
	Ray cameraRay = new Ray ();
	//new Ray ();
	RaycastHit camerHit;
	bool hit = false;
	float lastDis = 0f;
	//记录非遮挡前的距离，用以恢复
	LineRenderer line;

	#endregion 处理相机遮挡相关的 射线节点 ####################

	void Start ()
	{
		
		instance = this;
		Camera.SetupCurrent (Camera.main);
		var origiAngle = transform.eulerAngles;
		angleXAxis = origiAngle.x;
		angleYAxis = origiAngle.y; 
		 
//		originalAngle = transform.eulerAngles;
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody> ())
			GetComponent<Rigidbody> ().freezeRotation = true;

		//   target = RoleMgrScript.instance.curRole.transform;
	}
	// jiao du pian yi zhi
	public float getAngleOffset ()
	{
		return   transform.eulerAngles.y;// - originalAngle.y;
	}
	//hu du pian yi zhi
	public float getRadianOffset ()
	{
		return (transform.eulerAngles.y /*- originalAngle.y*/) / 180f * Mathf.PI;
	}
	#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
	int fingerId = -1;
	#endif


	void Update ()
	{
		if (target == null) {
			if (GameObject.FindWithTag ("Me") != null) {
				target = GameObject.FindWithTag ("Me").transform;
				#region 处理相机遮挡相关的 射线节点
				if (dir == null) {
					dir = target.Find ("rayForCameraCover").gameObject;
				}
				#endregion
			} else {
				return;
			}
		}  
		#region 处理相机遮挡相关的 射线节点 ####################
//		if (line == null) {
//			line = this.gameObject.AddComponent<LineRenderer> ();
//			line.enabled = true;
//			line.startWidth = .1f;
//			line.endWidth = .1f;
//		}
		//人物身上设个节点，照射一条射线到相机，以供处理遮挡
		dir.transform.LookAt (this.transform);
		cameraRay.origin = dir.transform.position + dir.transform.forward;// * 1f;
		cameraRay.direction = dir.transform.forward;
//		cameraRay = Camera.main.ViewportPointToRay (target.position);
		if (Physics.Raycast (cameraRay, out camerHit, (maxDis - 1f), LayerMask.GetMask ("Item"))) {
			
			Debug.DrawLine (target.position, camerHit.point, Color.red);
//			if (0f == lastDis) {
//				lastDis = distance;
//			}
			if (lastDis == 0f) {
				lastDis = dist;
			}
//			if (1f > dist) {
//				dist = 1f;	
//			} else {
			dist = Vector3.Distance (target.position, camerHit.point);//- 2f;
//			}
//			print ("hit:" + dist);
			hit = true;
//			line.SetPosition (0, target.position);
//			line.SetPosition (1, camerHit.point);

		} else {

			hit = false;
//			Debug.DrawLine (Camera.main.transform.position, cameraRay.direction, Color.blue);
//			Debug.DrawLine (target.position, transform.position, Color.blue);
			Debug.DrawLine (cameraRay.origin, cameraRay.origin + cameraRay.direction * (maxDis - 1f), Color.black);
//			line.SetPosition (0, cameraRay.origin);
//			line.SetPosition (1, cameraRay.origin + cameraRay.direction * 30f);
			if (lastDis != 0f) {
				dist = lastDis;
				lastDis = 0f;
			}
		}

		if (hit) {
			distance = Mathf.Lerp (distance, dist, Time.deltaTime * 10f);	

		} else {
			if (0f == dist) {//|| 0f != lastDis) {
				dist = Vector3.Distance (transform.position, target.position);
			 
			}
			
			
			distance = Mathf.Lerp (distance, dist, Time.deltaTime);
			//			lastDis = 0f;
		}

		#endregion 处理相机遮挡相关的 射线节点 ####################

		#if UNITY_EDITOR || UNITY_STANDALONE_WIN  || UNITY_STANDALONE_OSX 
		 
		//==
		 

		float wheel = Input.GetAxis ("Mouse ScrollWheel");
		dist -= wheel * 6.5f;   
		if (!hit) {
			dist = Mathf.Clamp (dist, minDis, maxDis);
		}
			 
		//==

		if (Input.GetMouseButton (1)) {


			angleXAxis -= Input.GetAxis ("Mouse Y") * ySpeed * 0.02f;
			angleYAxis += Input.GetAxis ("Mouse X") * xSpeed * 0.02f;
			//   angleYAxis %= 360;   
		}

		//===================
		#elif !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
		//===================

		if (Input.touchCount == 1 && MouseTools.rayCheckOverUI ().Equals (false)) {
			if (!fingerId.Equals (-1)) {
				if (fingerId.Equals (Input.GetTouch (0).fingerId) && Input.GetTouch (0).phase == TouchPhase.Moved) {
					fingerId = Input.GetTouch (0).fingerId;
					angleYAxis += Input.GetAxis ("Mouse X") * xSpeed * 0.02f;
					angleXAxis -= Input.GetAxis ("Mouse Y") * ySpeed * 0.02f;

				}
		 
			} else {
				if (Input.GetTouch (0).phase == TouchPhase.Moved) {
					fingerId = Input.GetTouch (0).fingerId;
					//   angleYAxis += Input.GetAxis ("Mouse X") * xSpeed * 0.02f;
					//   angleXAxis -= Input.GetAxis ("Mouse Y") * ySpeed * 0.02f;

				}
			}

		}


		if (Input.touchCount > 1 && (MouseTools.rayCheckOverUI (0).Equals (false) || MouseTools.rayCheckOverUI (1).Equals (false))) {
			if (Input.GetTouch (0).phase == TouchPhase.Stationary || Input.GetTouch (1).phase == TouchPhase.Stationary)
				return;
			if (Input.GetTouch (0).phase == TouchPhase.Moved || Input.GetTouch (1).phase == TouchPhase.Moved) {

				Vector2 tempPosition1 = Input.GetTouch (0).position;
				Vector2 tempPosition2 = Input.GetTouch (1).position;
				if (isEnlarge (oldPosition1, oldPosition2, tempPosition1, tempPosition2)) {
//					if (distance > 1.5f) {
//						distance -= 0.5f;   
//					}
						if(dist > 1.5f){
						dist -= .5f;
						}
				} else {
//					if (distance < 10f) {
//						distance += 0.5f;
//					}
					if (dist < 10f) {
						dist += 0.5f;
					}
				}
				oldPosition1 = tempPosition1;
				oldPosition2 = tempPosition2;
			}
		}

		//   if (Input.touchCount == 0) {
		//   if (Input.GetMouseButton (1)) {
		//   
		//   angleYAxis += Input.GetAxis ("Mouse X") * xSpeed * 0.02f;
		//   angleXAxis -= Input.GetAxis ("Mouse Y") * ySpeed * 0.02f;
		//   
		//   }
		//   }
		//===================
		#endif


	}
	//no use
	public void slerpCamerRotation (float targetRotation)
	{
		angleYAxis = Mathf.Lerp (angleYAxis, targetRotation, Time.deltaTime * 10f);
	}
	//no use
	public void slerpCamerRotation (Quaternion targetrotation)
	{
		this.targetRotation = Quaternion.Slerp (transform.rotation, targetrotation, Time.deltaTime * 10f);
		angleYAxis = targetrotation.eulerAngles.y;
		angleXAxis = targetrotation.eulerAngles.x;
	}

	/// <summary>
	/// two fingure 's distance  between new and old
	/// </summary>
	/// <returns><c>true</c>, if enlarge was ised, <c>false</c> otherwise.</returns>
	/// <param name="oP1">O p1.</param>
	/// <param name="oP2">O p2.</param>
	/// <param name="nP1">N p1.</param>
	/// <param name="nP2">N p2.</param>
	bool isEnlarge (Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
	{

		//   Vector2.Distance (oP1, oP2);
		float leng1 = Mathf.Sqrt ((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
		//   Vector2.Distance (nP1, nP2);
		float leng2 = Mathf.Sqrt ((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));
		if (leng1 < leng2) {
			return true; 
		} else {
			return false; 
		}
	}

	Quaternion targetRotation;
	public float upOffset = 1.5f;

	void LateUpdate ()
	{
		if (target != null) {
			if (camState.Equals (CameraState.FreedomCamera)) {   
				//   print (" xxx  " + angleYAxis);
				angleXAxis = ClampAngle (angleXAxis, xAxisAngleMinLimit, xAxisAngleMaxLimit); 
				Quaternion rotation = Quaternion.Euler (angleXAxis, angleYAxis, 0f);
				transform.rotation = rotation;

				Vector3 position = rotation * (Vector3.back * distance) + target.position + Vector3.up * upOffset;
				transform.position = position;
			} else if (camState.Equals (CameraState.FollowCamera)) {
				transform.rotation = targetRotation;

				Vector3 position = targetRotation * (Vector3.back * distance) + target.position + Vector3.up * upOffset;
				transform.position = position;
				 
			}
		}
	}


	public static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}

	public CameraState camState = CameraState.FreedomCamera;
}

public enum CameraState
{
	FollowCamera,
	FreedomCamera
}
