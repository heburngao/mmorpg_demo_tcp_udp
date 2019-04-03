using UnityEngine;
using System.Collections;
using System;

namespace ghbc
{


	/**

        public class Follower : MonoBehaviour {
private TurnCharactorByAnim turn;
public Transform target;
public float speed;
public float minStopDis = 1f;
// Use this for initialization
void Start()
{
        turn = new TurnCharactorByAnim();
        turn.minStopDis = 0.3f;
        turn.initData(this.transform, speed, true);
        //turn.SetTarget(target.position,1f);
        turn.SetSpeed(1f);

}

// Update is called once per frame
void Update()
{
        if (turn != null)
        {
            turn.toUpdate();
            if (target != null)
            {
                turn.SetTarget(target.position, speed);
            }
            else
            {
                if (turn.m_useClickTarget == false)
                {
                    speed = 0;
                    turn.SetSpeed(speed);
                }
            }
            turn.SetMinDis(this.minStopDis);
        }
}
}
    **/

	public enum TowardDirection
	{
		North,
		South,
		East,
		West,
		None,
	}

	public class TurnCharactorByAnim
	{
		private Vector3 targetPos;
		private float Forward = 0f;
		private CharacterController role;
		//GameObject ball;
		//GameObject ball2;
		private float dis = 1f;
		private float minStopDis = .1f;
		private Animator anim;
		private Vector3 _targetPoint;
		private Action _endCallback;
		private bool m_useClickTarget = false;
		public bool walkMode = false;
		private float dir = 0f;
		private float edgs = .7f;


		private bool _isMovingTarget;
		private bool forceStop = false;
		private bool arrived = false;


		private TowardDirection Direction = TowardDirection.East;
		private Vector3 __direction = Vector3.back;

		private bool _moveAfterTurn = true;
		private float ss = 1f;
		private bool toAutoToward = true;


		//private GameObject ball3;

		// Use this for initialization
		public void initData (Transform hostTarget, Action endCallback = null, bool useClickTarget = false)//float speed = 0f,bool useClickTarget = false, Action endCallback = null)
		{
			role = hostTarget.GetComponent<CharacterController> ();
			anim = hostTarget.GetComponent<Animator> ();
			//        this.Forward = speed;
			this._endCallback = endCallback;
			this.m_useClickTarget = useClickTarget;
			//ball = GameObject.Find("Sphere");
			//ball2 = GameObject.Find("Sphere2");
			//ball3 = GameObject.Find("Sphere3");

		}

		public void SetMinDis (float minStopdis)
		{
			this.minStopDis = minStopdis;
		}

		private void SetSpeed (float speed)
		{
			this.Forward = speed;
		}

		public void SetUseClickTarget (bool useclick)
		{
			this.m_useClickTarget = useclick;
		}

		/// <summary>
		/// 设置目标
		/// </summary>
		/// <param name="targetPoint"> 目标点</param>
		/// <param name="_direction">自由方向</param>
		/// <param name="callback">目的地到达回调</param>
		/// <param name="moveAfterturn">true: 先转向再移动向目标,false: 转向与移动同时发生,易小转弯时转圈</param>
		public void SetTarget (Vector3 targetPoint, Vector3 _direction, Action callback, bool moveAfterturn = false)//float speed = 0f, Action callback = null)
		{
			if (targetPoint == Vector3.zero)
				return;
			if (_targetPoint != targetPoint) {
				forceStop = false;
				lockOnece = true;
				arrived = false;
				_isMovingTarget = !moveAfterturn;
				this._targetPoint = targetPoint;
				toAutoToward = true;
				this._endCallback = callback;
				this.Direction = TowardDirection.None;
				__direction = _direction;
				TimerMgr.UnRegister ("delayTurnAnim");
			}


			//        this.Forward = speed;
		}

		/// <summary>
		/// 设置目标
		/// </summary>
		/// <param name="targetPoint"> 目标点</param>
		/// <param name="callback">目的地到达回调</param>
		/// <param name="moveAfterturn">true: 先转向再移动向目标,false: 转向与移动同时发生,易小转弯时转圈</param>
		/// <param name="toAutoToward">true: 到达目的地后,自动转向 预设方向</param>
		/// <param name="_direction">预设方向</param>
		public void SetTarget (Vector3 targetPoint, Action callback, bool moveAfterturn = false, bool toAutoToward = false, TowardDirection _direction = TowardDirection.South)//float speed = 0f, Action callback = null)
		{
			if (targetPoint == Vector3.zero)
				return;
			if (_targetPoint != targetPoint) {
				forceStop = false;
				lockOnece = true;
				arrived = false;
				_isMovingTarget = !moveAfterturn;
				this._targetPoint = targetPoint;
				this.toAutoToward = toAutoToward;
				this._endCallback = callback;
				this.Direction = _direction;
				TimerMgr.UnRegister ("delayTurnAnim");
			}


			//        this.Forward = speed;
		}

		public void Stop ()
		{
			Forward = 0f;
			dir = 0f;
			_endCallback = null;
			forceStop = true;
			anim.SetFloat ("Forward", Forward);
			anim.SetFloat ("Turn", dir);
		}

		public void toUpdate ()
		{

			if (Input.GetMouseButtonDown (0)) {
				if (m_useClickTarget == false)
					return;
				/*
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, 100f))
                {
                    //_isMovingTarget = false;
                    //this._targetPoint = hit.point;
                    //toTurn = false;
                    //使用实例 as below -------------------
                    //				SetTarget(hit.point,Vector3.right,()=>{	DebugTool.LogRed("arrived"); },true);
                    //				SetTarget(hit.point,Vector3.right,()=>{	DebugTool.LogRed("arrived"); },false);

                    //				SetTarget(hit.point, () => { DebugTool.LogRed("arrived"); }, false,true, TowardDirection.East);
                    //				var rot = Quaternion.LookRotation(hit.point -this.role.transform.position);
                    //				DebugTool.LogRed ("目标偏角: "+rot.eulerAngles.y);
                    //				SetTarget(hit.point, () => { DebugTool.LogRed("arrived"); }, true,true, TowardDirection.South); TODO

                    //                				SetTarget(hit.point, () => { DebugTool.LogRed("arrived"); }, false,false, TowardDirection.None);
                    //                				SetTarget(hit.point, () => { DebugTool.LogRed("arrived"); }, true,false, TowardDirection.None);
                    //                				SetTarget(hit.point, () => { DebugTool.LogRed("arrived"); }, true,true, TowardDirection.None);


                    //				Debug.DrawLine(this.role.transform.position, _targetPoint, Color.yellow, 3f);
                }*/
			}
			if (forceStop == false)
				Updated ();
		}
		//float tt = 1f;
		private bool lockOnece = false;

		private void Updated ()
		{
			if (minStopDis < role.stepOffset) {
				minStopDis = role.stepOffset;
			}

			//tt -= Time.deltaTime;

			if (this.role == null || this.anim == null) {
				return;
			}

			//============================================================================

			if (_targetPoint != Vector3.zero) {
				targetPos = _targetPoint;
			}
			//======= debug print
			//if (tt < 0f)
			//{
			//var boo = dis > minStopDis;
			//if (boo) Debug.Log("can move :: " + boo + " - > dis:: " + dis + " stopDis:: " + minStopDis + " / speed::" + curSpeed + " / turn::" + curTurn);

			//tt = 5f;
			//}
			Debug.DrawLine (this.role.transform.position, this.role.transform.position + this.role.gameObject.transform.forward, Color.grey);
			//        Debug.DrawLine(this.role.transform.position, this.role.transform.position + Vector3.back, Color.green);
			//        Debug.DrawLine(this.role.transform.position, this.role.transform.position + Vector3.forward, Color.green);
			//        Debug.DrawLine(this.role.transform.position, this.role.transform.position + Vector3.left, Color.green);
			//        Debug.DrawLine(this.role.transform.position, this.role.transform.position + Vector3.right, Color.green);
			//        Debug.DrawLine(this.role.transform.position, targetPos + this.role.transform.forward * 1.2f, Color.yellow);
			//        Debug.DrawLine(this.role.transform.position, targetPos, Color.blue);
			//=======


			//        if (!_isMovingTarget)
			//        {
			//            vv = .4f;
			//        }
			//=================
			//        Debug.Log("->> " + dis +"|"+ minStopDis);
			//		if (setDis > minStopDis)
			//		{
			//			arrived = false;
			//			//			step = 0;
			//		}
			//		else
			//		{
			//			arrived = true;
			//			//			step = 1;
			//		}

			//=================
			dis = ((Vector3.Distance (this.role.transform.position, targetPos)));// + this.role.transform.forward * .2f)));

			var target_offset = targetPos + this.role.transform.forward * 1.2f;

			//		Debug.DrawLine(targetPos + this.role.transform.forward * 1f ,targetPos, Color.red);
			//	Debug.DrawLine(targetPos , target_offset, Color.red);
			//		dir = 0f;
			//		_dir = Mathf.Clamp(_dir, -.4f, .4f);
			if (arrived == false) {
				dir = Vector3.Dot (this.role.transform.right, target_offset - this.role.transform.position);
				//						Debug.Log (" 未到达目的地 ");
				switch (walkMode) {
				case true:
					{//走
						//Debug.Log("walk");
						if (dis <= edgs) {// / 2f)//近距
							if (Forward > .4f * ss)
								Forward -= .001f * ss;
                                //							Forward = 1f;
                                else
								Forward = .3f;
						} else if (dis > edgs) {//远距
							Forward = .6f * ss;
						}
						//					Forward = 1f * ss;
					}
					break;

				case false:
					{//跑
						//Debug.Log("run");
						if (dis <= edgs) {// / 2f)//近距
							if (Forward > .4f * ss)
								Forward -= .001f * ss;
                                //							Forward = 1f;
                                else
								Forward = .3f;

						} else if (dis > edgs) {//远距
							Forward = 1f * ss;
						}
						//					Forward = 1f * ss;
					}
					break;

				}

				if (!lockOnece) {
					Forward = 0f;
				}
				//if (angle < 45f && Forward > .9f * ss)
				//{
				//    var rt = Quaternion.LookRotation(new Vector3(targetPos.x, 0f, targetPos.z) - new Vector3(role.transform.position.x, 0f, role.transform.position.z));
				//    //Debug.Log("ooo" + rt.eulerAngles);
				//    this.role.transform.rotation = Quaternion.Lerp(role.transform.rotation, rt, Time.deltaTime * 10f);

				//}


				//dir = (int)(dir * 10f) / 10f;
				//_dir *= 200f;

				//dir = Mathf.Lerp(dir, _dir, Time.deltaTime * 5f);



				if (_isMovingTarget) {
					//				edgs = .7f;
					//				 
					anim.SetFloat ("Forward", Forward);
				} else {
					//				edgs = .7f;

					//				Debug.Log ("angle : " + angle);
					if (_moveAfterTurn) {
						var angle = Vector3.Angle (this.role.transform.forward, target_offset - role.transform.position);
						if (angle < 20f) {
							anim.SetFloat ("Forward", Forward);
						} else {
							anim.SetFloat ("Forward", 0f);
						}
					} else {
						anim.SetFloat ("Forward", Forward);

					}

				}
				if (anim != null) {
					anim.SetFloat ("Turn", dir);
				}

			} else {
				dir = 0f;
				Forward = 0f;
				if (anim != null) {
					anim.SetFloat ("Forward", Forward);
					anim.SetFloat ("Turn", dir);
				}
			}

			if (!lockOnece) {
				Forward = 0f;
			}
			//ball.transform.position = this.dog.position + Vector3.right;
			//ball2.transform.position = this.dog.position + Vector3.forward;
			//ball3.transform.position = this.dog.position;
			//float angle = Mathf.Acos(Vector3.Dot(targetPos, this.transform.position)) * Mathf.Rad2Deg;
			//        }

			//=================
			var vv = 0f;//1.2f;
			var setDis = dis + vv;
			//		Debug.Log("dis:"+dis + " | minStopDis: " + minStopDis);
			if (setDis <= minStopDis) {
				// 到达目的地


				//			if (toAutoToward && angle > 1f && !_isMovingTarget)
				//			{
				//				//						Debug.DrawLine (this.role.transform.position, targetPos, Color.red, 3f);
				//				Debug.Log("目标自动朝向s :");
				//				//				anim.SetFloat("Turn", dir);
				//				toTurn = true;
				//			}
				//			else
				//			{

				if (lockOnece) {
					lockOnece = false;
					//Debug.Log("到了");

					//-----------------------------
					if (toAutoToward) {

						Forward = 0f;
						anim.SetFloat ("Forward", Forward);
						//__direction = Vector3.back;
						//                    __direction = Vector3.zero;
						switch (Direction) {
						case TowardDirection.South:
							__direction = Vector3.back;
							break;
						case TowardDirection.North:
							__direction = Vector3.forward;
							break;
						case TowardDirection.East:
							__direction = Vector3.left;
							break;
						case TowardDirection.West:
							__direction = Vector3.right;
							break;
						default:
							__direction = Vector3.zero;
							break;
						}
						//DebugTool.LogRed("Direction: " + Direction);

						var tt = 3f;//* Mathf.Abs(dir) / 360f;
						//					DebugTool.LogYellow (tt + " | "+ dir);


						//                    DebugTool.LogCyan("目标已抵达1 : delayTime : " + tt);
						//DebugTool.LogCyan("目标已抵达1 :");
						if (this._endCallback != null) {
							this._endCallback ();
							this._endCallback = null;

						}
						//					var vxv = targetPos - (this.role.gameObject.transform.forward);//this.role.transform.position 
						//					var ang = Quaternion.LookRotation(vxv);
						//
						//					DebugTool.LogRed(" : dir : " + Vector3.Angle(targetPos, this.role.gameObject.transform.forward) + "  ang : " + ang.eulerAngles + " : " + angle );
						//					Debug.DrawLine(this.role.transform.position + this.role.gameObject.transform.forward, targetPos, Color.yellow);

						TimerMgr.Register ("delayTurnAnim", tt, () => {
							arrived = true;
							//						DebugTool.LogCyan ("delayTurnAnim stop");
							dir = 0f;
							Forward = 0f;
							if (anim != null) {
								anim.SetFloat ("Forward", Forward);
								//							anim.SetFloat ("Turn", dir);
							}
							//DebugTool.LogCyan("目标已抵达2 :");
							//				if (this._endCallback != null) {
							//					this._endCallback ();
							//					this._endCallback = null;
							//
							//				}

						}, () => {
							//DebugTool.LogCyan("目标已抵达 :turn :: " + dir);
							//						DebugTool.LogCyan ("delayTurnAnim" + _dir);
							//dir = (int)(dir * 10f) / 10f;
							//_dir *= 200f;
							targetPos = this.role.transform.position + __direction * .5f;
							//						target = this.role.transform.position + __direction * .51f;
							var _dir = Vector3.Dot (this.role.transform.right, targetPos - this.role.transform.position);


							this.dir = _dir;
							//						dir = Mathf.Lerp (dir, _dir, Time.deltaTime * 5f);
							if (anim != null) {
								anim.SetFloat ("Turn", dir);
								Forward = 0f;
								anim.SetFloat ("Forward", Forward);
							}
							//							if (dir < 1f) {
							//								//Debug.Log("目标已抵达 : after turn" + Direction);
							//								this._endCallback ();
							//								this._endCallback = null;
							//							}
						});
					} else {

						//					TimerMgr.Register ("delayStopAnim", 1.5f, () => {
						//						DebugTool.LogCyan ("delayStopAnim");
						arrived = true;
						Forward = 0f;
						if (anim != null) {
							anim.SetFloat ("Forward", Forward);
							//							anim.SetFloat ("Turn", dir);
						}
						//DebugTool.LogCyan("目标已抵达 :");
						if (this._endCallback != null) {
							this._endCallback ();
							this._endCallback = null;
							//							arrived = true;
						}
						//-----------------------------
						//					});
					}
				}
				//				this.role.transform.position += (targetPos - this.role.transform.position) * Time.deltaTime;
				//			}
				_targetPoint = Vector3.zero;
			}
		}

		TimerLogic delayTurnLogic;
		//public float maxRunSpeed = 1f;
		//public float maxWalkSpeed = .6f;

		public float curSpeed {
			get {
				return this.Forward;
			}
		}

		public float curTurn {
			get {
				return this.dir;
			}
		}

		public float curDis {
			get {
				return this.dis;
			}

		}

	}
}
