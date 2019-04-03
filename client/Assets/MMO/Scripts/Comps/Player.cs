using UnityEngine;
using ghbc;
using System;
using System.Collections;

 
    public delegate void Act(Vector3 target_dir, Vector3 target_pos, float move_speed, msg.State state, bool force_send);
public class Player:MonoBehaviour
{
    [SerializeField]
	public bool isMine = false;
	[SerializeField]
	public PlayerData data;
	/*
	[SerializeField]
	public vThirdPersonInput inputCtrl;
	[SerializeField]
	public vThirdPersonController controller;
	*/
	//=====

	//	public float validTouchDistance = 200f;
	//200
	//	public string layerName = "Ground";

	RoleController roleCtr;

	//	TurnCharactorByAnim animCtr;
	protected virtual void Awake ()
	{

		/*inputCtrl = this.GetComponent<vThirdPersonInput> ();
		controller = this.GetComponent<vThirdPersonController> ();
		
		controller.data = this.data;*/

		roleCtr = this.GetComponent<RoleController> ();
		 
	}

	/// <summary>
	/// 心跳中强制拉回别人的偏差
	/// </summary>
	public Action<PlayerData> callbackSyncPos;
	/// <summary>
	/// 处理udp返回同步
	/// </summary>
	public Action<PlayerData> callbackStatus;
	/// <summary>
	/// 发送udp同步
	/// </summary>
	public Act sendStatus;

	 

	protected virtual void Start ()
	{
		
//		Facade_Base.instance.AddEvent ("status", dealWithStatusUpdate, true);
		// callbackSyncPos += forceDragBackPos;
		callbackStatus += dealWithStatusUpdate;
		sendStatus += sendStatusUpdate;
		//		animCtr = new TurnCharactorByAnim ();
		//		animCtr.initData (this.transform, () => {
		//			print ("clickend");
		//		}, true);

		////turn.SetTarget(target.position,1f);


//		if (data.isMine) {
////			inputCtrl.isMine = true;
////			inputCtrl.data = this.data;
//		} else {
////			inputCtrl.isMine = false;
////			inputCtrl.data = null;
//		}
	}

	protected virtual void OnEnable ()
	{
		/*if (controller == null)
			return;*/
		if (data == null) {
			Debug.LogError ("data is null");
			return;
		}
        
		this.isMine = data.isMine;
		if (this.isMine) {
//			inputCtrl.isMine = true;
//			inputCtrl.data = this.data;
			/*controller.isMine = true;
			inputCtrl.enabled = true;
			inputCtrl.CharacterInit ();*/

			if (roleCtr != null) {
				roleCtr.isMine = true;
			}
			this.GetComponent<CapsuleCollider> ().enabled = true;

		} else {
			if (roleCtr != null) {
				roleCtr.isMine = false;
			}
			this.GetComponent<CapsuleCollider> ().enabled = true;
            //			Destroy (this.GetComponent<Rigidbody> ());

             

			//			inputCtrl.isMine = false;
			//			inputCtrl.data = null;
			/*controller.isMine = false;
			inputCtrl.enabled = true;
			inputCtrl.CharacterInit ();*/
		}
	}

	protected virtual void OnDestroy ()
	{
//		Facade_Base.instance.RemoveEvent ("status", dealWithStatusUpdate, true);
		// callbackSyncPos -= forceDragBackPos;
		callbackStatus -= dealWithStatusUpdate;
		sendStatus -= sendStatusUpdate;
		if (GameProxy.ClientAllPlayerDic.ContainsKey(this.data.userid)){

			GameProxy.ClientAllPlayerDic.Remove(this.data.userid);

		}
	}

    //	void OnTriggerEnter (Collider other)
    //	{
    //		DebugTool.LogYellow ("enter trigger1");
    //		if (other.CompareTag ("Item")) {
    //			roleCtr.canMove = false;
    //			DebugTool.LogYellow ("enter trigger2");
    //		}
    //	}

    #region xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    // long _FrameIndex = 0L;
	//每隔66毫秒上报一次
	float timer = 0f;
	private void sendStatusUpdate (Vector3 dir_target ,Vector3 targetPos, float MoveSpeed, msg.State state, bool forceSnd)
	{
        //======
        if (forceSnd)
        {
            timer = 0f;
        }
		timer -= Time.deltaTime;  
		if (0f < timer) {
			return;
		}
		//timer = .066f;
		timer = 0.1f;
		//======

		#region 发送
		msg.StatusInfo info = new msg.StatusInfo ();
		 
		// info.TargetPos = new msg.Vect3 () {
		// 	X = (int)(1000000 * targetPos.x),
		// 	Y = (int)(1000000 * targetPos.y),
		// 	Z = (int)(1000000 * targetPos.z),
		// };
		info.MoveSpeed = (int)(10000 * MoveSpeed); 
	 
		info.Status = state;

		if (0 < info.MoveSpeed) {
			info.Status = msg.State.WALK;
		} else {
			info.Status = msg.State.IDLE;
		}
		info.Userid = data.userid;
		 
		// var xx = (int)(this.transform.position.x * 10000); //xxxxx
		// var yy = (int)(this.transform.position.y * 10000);
		// var zz = (int)(this.transform.position.z * 10000); //xxxxx
		var xx = (int)(targetPos.x * 10000); //xxxxx
		var yy = (int)(targetPos.y * 10000);
		var zz = (int)(targetPos.z * 10000); //xxxxx
		// info.SpawnPos = new msg.Vect2 () {
		// 	X = (float)xx,//this.transform.position.x,
		// 	Y = (float)zz,//this.transform.position.z,
		// };
		info.Pos = new msg.Vect3(){
			X = xx,
			Y = yy,
			Z = zz,
		};
        var x_dir = (int)(dir_target.x * 10000);  
        var y_dir = (int)(dir_target.y * 10000);
        var z_dir = (int)(dir_target.z * 10000);
        info.Dir = new msg.Vect3(){
            X = x_dir,
            Y = y_dir,
            Z = z_dir,
        };
        // info.FrameIndex = this._FrameIndex;
        // this._FrameIndex++;
        DebugTool.LogYellow (data.userid + "|||上报坐标: " + xx +"," + yy+ "," + zz);
        Debug.Log(data.userid + "|||上报坐标: " + xx + "," + yy + "," + zz);
        Debug.Log(" snd dir :::::::::::::: " + info.Dir.X + "|" + info.Dir.Y + "|" + info.Dir.Z);
        roleCtr.SetTarget(dir_target);
        Netmanager.sendUpdateStatus_UDP (info);
		#endregion
	}

	 
    //每66毫秒被服务端调用一次
	private void dealWithStatusUpdate (PlayerData args)
	{
		var receiveData = args;//(PlayerData)args [0];
		 
		if (this.isMine)//只同步别人的
		{
			Debug.Log("本人自身 status 数据 不处理" + receiveData.Pos.X + "," + receiveData.Pos.Y + "," + receiveData.Pos.Z + "|||" + receiveData.Dir.X + "," + receiveData.Dir.Y + "," + receiveData.Dir.Z);
			return;
		}
			Debug.Log("他人 status 数据" + receiveData.Pos.X + "," + receiveData.Pos.Y + "," + receiveData.Pos.Z);
		if (this.data.userid != receiveData.userid) {//只处理对应人的status update
			Debug.LogError ("数据交差" + this.data.userid + " / " + receiveData.userid);
			return;
		}
//		this.data.isMine
		//			ExitGameInput2 (data.status);
		//				if (!controller.lockMovement) {
		//			MoveCharacter2 (data.pos);
		//			SprintInput2 (data.status);
		//			StrafeInput2 (data.status);
		//			JumpInput2 (data.status);
		//				}
		 
		if (receiveData.status == msg.State.WALK ){
			var target_pos = new Vector3 ((float)(receiveData.Pos.X * .0001m), (float)(receiveData.Pos.Y * .0001m), (float)(receiveData.Pos.Z * .0001m));
            // var dis = Vector3.Distance(this.transform.position , target_pos );
            // print(".........: " + dis);
            var target_dir = new Vector3((float)(receiveData.Dir.X * .0001m), (float)(receiveData.Dir.Y * .0001m), (float)(receiveData.Dir.Z * .0001m));
            roleCtr.SyncOthersSpeed2 (target_dir, target_pos,(float)(receiveData.MoveSpeed * .0001m));
			 
		}else if (receiveData.status == msg.State.IDLE) {
			//var idle_pos = new Vector3 ((float)(receiveData.Pos.X * .0001m), (float)(receiveData.Pos.Y * .0001m), (float)(receiveData.Pos.Z * .0001m));
            print("rcv .........站立: ");
            var target_dir = new Vector3((float)(receiveData.Dir.X * .0001m), (float)(receiveData.Dir.Y * .0001m), (float)(receiveData.Dir.Z * .0001m));
            roleCtr.SyncIdle(target_dir);

        }
        else if (receiveData.status == msg.State.JUMP)
        {
            roleCtr.SyncOtherRotate(new Vector3((float)(receiveData.Pos.X * .0001m), (float)(receiveData.Pos.Y * .0001m), (float)(receiveData.Pos.Z * .0001m)));
            roleCtr.SyncOthersSpeed((float)(receiveData.MoveSpeed * .0001m));
            roleCtr.JumpUp(false);
        }

		//				ControlSpeed2 (data.pos);
		//				FreeMovement2 (data.rotate);
//xxx			this.controller.input = receiveData.inputForce;//读取远程摇杆值
//			if (this.controller.isMine == false) {
//				controller.input = new Vector2 (receiveData.force.X * .000001f, receiveData.force.Z * .000001f);
//				DebugTool.LogError ("########## 同步别人的状态 inputForce xz:: " + controller.inputForce.X + "," + controller.inputForce.Z + " || " + receiveData.force.X + "," + receiveData.force.Z);
//			} else {
//				DebugTool.LogError ("<<########## 同步自己的状态 inputForce xy:: " + controller.inputForce.X + "," + controller.inputForce.Z + " || " + receiveData.Force.X + "," + receiveData.Force.Z);
//				DebugTool.LogError ("<<########## 不同步自己的状态 receive xz:: " + receiveData.force.X + "," + receiveData.force.Z);
//			}
 
//			this.controller.FreeMovement2 (receiveData.rotate); //TODO
//			this.controller.ControlSpeed2 (receiveData.force);
//		} else if (receiveData.status == msg.State.ROTATE) {
//			this.controller.ControlSpeed2 (receiveData.pos);
//		}
	}

	#region 拉回

	//	GameObject cube;
	Vector3 forceSyncPos;
	//	bool forceSyncFinish;
	float timeDragBack = 0f;

// 	/// <summary>
// 	/// tcp心跳中强制拉回UDP产生的误差
// 	/// </summary>
// 	/// <param name="args">Arguments.</param>
// 	private void forceDragBackPos (PlayerData args)
// 	{
// 		if (this.isMine)//只同步别人的
// 			return;
// 		PlayerData receiveData = args;
// 		if (this.data.userid != receiveData.userid) {//只处理对应人的status update
// 			Debug.LogError ("数据交差" + this.data.userid + " / " + receiveData.userid);
// 			return;
// 		}
// 		Debug.LogError ("################## 强制拉回 ##############");
// //		forceSyncPos = new Vector3 (receiveData.spawnPos.X * .000001f, receiveData.spawnPos.Y * .000001f, receiveData.spawnPos.Z * .000001f);
// 		//TODO 注意bug ， 不能写错 运算符
// 		forceSyncPos = new Vector3 ((float)((int)(receiveData.Pos.X) * 0.0001m), this.transform.position.y, (float)((int)((receiveData.Pos.Y)) * 0.0001m));//xxxxx
// //		forceSyncFinish = false;
// 		//=====================
// 		float dis = Vector3.Distance (this.transform.position, forceSyncPos);
// 		Debug.LogError ("dis: " + dis);
// 		if (.2f < dis) { //当UDP帧同步产生的误差大于.3f时，采用TCP心跳包中的拉回
// 			//==
// //			if (cube == null) {
// //				cube = GameObject.CreatePrimitive (PrimitiveType.Sphere);
// //				cube.transform.localScale = .3f * Vector3.one;
// //				cube.GetComponent<SphereCollider> ().enabled = false;
// //				cube.transform.SetParent (this.transform.parent);
// //			}
// //			cube.transform.position = forceSyncPos;
// 			//==
// 			print ("<color=red>" + this.transform.position + "</color>");
// //			this.transform.position = forceSyncPos;
// 			//换成下面的补间
// //			StartCoroutine (syncAnimate (forceSyncPos));//, dis));
// 			timeDragBack = .3f;
// 			roleCtr.applyRootMotion = roleCtr.ApplyRootMotion;
// 			print ("<color=cyan>" + forceSyncPos + "</color> ||| " + receiveData.Pos.X + "," + receiveData.Pos.Y);
// //			roleCtr.SyncOthersSpeed (0f);
// 		} else {
// 			//到达目标，停止速度
// 			// roleCtr.SyncOthersSpeed (0f);xxx
// 			Debug.LogError ("################## 不强制拉回 ##############");
// //			forceSyncFinish = true;
// 		}
// 	}

	#endregion

	//	IEnumerator syncAnimate (Vector3  forceSyncPos)//, float dds)
	//	{
	////		float dis = dds;
	//		while (true) {
	////			roleCtr.SyncOthersSpeed (.2f);
	////			roleCtr.SyncOtherRotate (forceSyncPos);
	////			yield return new WaitForSeconds (0.066f);
	//			this.transform.position = Vector3.Lerp (this.transform.position, forceSyncPos, .3f);
	//			var dis = Vector3.Distance (this.transform.position, forceSyncPos);
	//			print ("bug dis: " + dis + " / " + this.transform.position + " / " + forceSyncPos);
	//			if (0f == dis)
	//				break;
	//			yield return new WaitForEndOfFrame ();
	//		}
	//
	//	}

	//	private bool dealWithStatusUpdate (object[] args)
	//	{
	//		var data = (PlayerData)args [0];
	//		if (this.data.userid != data.userid)//只处理对应人的status update
	//			return true; //TODO
	//
	//		//			ExitGameInput2 (data.status);
	//		//				if (!controller.lockMovement) {
	//		//			MoveCharacter2 (data.pos);
	//		//			SprintInput2 (data.status);
	//		//			StrafeInput2 (data.status);
	//		//			JumpInput2 (data.status);
	//		//				}
	//
	//		if (data.status == msg.State.WALK) {
	//			//				ControlSpeed2 (data.pos);
	//			//				FreeMovement2 (data.rotate);
	//			this.controller.FreeMovement2 (data.rotate);
	//		} else if (data.status == msg.State.ROTATE) {
	//			this.controller.ControlSpeed2 (data.pos);
	//		}
	//		return true;
	//	}

	#endregion

	protected virtual void Update ()
	{
		if (roleCtr == null)
			return;
		
		#region 偏差拉回处理
		timeDragBack -= Time.deltaTime;
		if (0f < timeDragBack) {
//			this.transform.position = Vector3.Lerp (this.transform.position, forceSyncPos, .3f);
			roleCtr.SyncDrageBack (forceSyncPos);
		} else {
			if (roleCtr.applyRootMotion != null) {
				roleCtr.applyRootMotion ();
				roleCtr.applyRootMotion = null;
			}
		}
		#endregion
		//		animCtr.toUpdate ();
		//点击地板，同步简单的坐标
		//		if (Input.GetMouseButtonDown (0)) {
		//			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);  //摄像机需要设置MainCamera的Tag这里才能找到
		//			RaycastHit hitInfo;
		//			if (Physics.Raycast (ray, out hitInfo, validTouchDistance, LayerMask.GetMask (layerName))) {
		//				GameObject gameObj = hitInfo.collider.gameObject;
		//				Vector3 hitPoint = hitInfo.point;
		////				animCtr.SetTarget (hitPoint, null);
		//				if (this != null && this.data != null && this.data.isMine) {
		//
		//					this.transform.position = hitPoint;
		//					data.pos = this.transform.position;
		//					var info = new PlayerInfo ();
		//					info.Level = this.data.level;
		//					info.Nickname = data.nickname;
		//					info.Pos = new Vect3 () {
		//						X = data.pos.x,
		//						Y = data.pos.y,
		//						Z = data.pos.z
		//					};
		//					info.Status = State.WALK;
		//					info.Userid = data.userid;
		//					Netmanager.sendUpdateStatus (info);
		//				}
		//				Debug.Log ("click object name is " + gameObj.name + " , hit point " + hitPoint.ToString ());
		//
		//			}
	}






	//		if (this.isMine)//只同步别人的
	//			return;
	//		if (forceSyncFinish == false) {
	//			var dis = Vector3.Distance (this.transform.position, forceSyncPos);
	//			Debug.LogError ("dis: " + dis);
	//			if (.8f < dis) { //当UDP帧同步产生的误差大于.3f时，采用TCP心跳包中的拉回
	//				if (cube == null) {
	//					cube = GameObject.CreatePrimitive (PrimitiveType.Sphere);
	//					cube.transform.localScale = .3f * Vector3.one;
	//					cube.transform.SetParent (this.transform.parent);
	//				}
	//				cube.transform.position = forceSyncPos;
	////				roleCtr.SyncOtherRotate (forceSyncPos);
	//
	//				this.transform.position = forceSyncPos;
	//				roleCtr.SyncOthersSpeed (0f);
	//			} else {
	//				//到达目标，停止速度
	//				roleCtr.SyncOthersSpeed (0f);
	//				forceSyncFinish = true;
	//			}
	//		}

	//	}
}