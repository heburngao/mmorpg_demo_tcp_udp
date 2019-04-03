 

using UnityEngine;
using System.Collections;
using System;

public class RoleController : MonoBehaviour
{
	[HideInInspector]
	private float moveSpeed = 0f;
	[SerializeField]
	private Vector3 targetPoint;
	public float MaxTrasferSpeed = 1;
	public float rotateSpeed = 1f;

	void Start ()
	{
		// Time.fixedDeltaTime = 0.066f;
		this._rigidbody = this.GetComponent<Rigidbody> ();


		if (anim == null) {
			anim = this.GetComponent<Animator> ();
			anim.applyRootMotion = false;
		}
		if (anim.applyRootMotion) {
			anim.SetFloat ("InputVertical", moveSpeed);
			//TODO send to server by UDP  :: MoveSpeed

		} else {
			anim.SetFloat ("InputVertical", moveSpeed);
			transform.Translate (Vector3.forward * Time.deltaTime * moveSpeed); // move forward
		}
        //standCast();
       
    }

	float autotRadian;


	#region ########### 射线相关 #################

	Ray shootRay = new Ray ();
	RaycastHit shootHit;
	float range = 1f;
	int shootableMask = -1;
	LineRenderer gunLine;
	//=====
	Ray enemyRay = new Ray ();
	int enemyMask = -1;

	#endregion############################

	private Animator anim;
	private Player player;

	public bool isMine = false;

	private bool lock_Idle_snd = false;

	private float delayTimeOfLock = .1f;

	/// <summary>
	/// 66毫秒
	/// </summary>
	
	//每隔66毫秒上报一次
	// void FixedUpdate ()
	void Update()
	{
		if(_moveToNetTarget != null){
			_moveToNetTarget();
		}


		if (player == null) {
			if (this.GetComponent<Player> () != null) {
				player = this.GetComponent<Player> ();
				gunLine = GetComponent <LineRenderer> ();
				gunLine.enabled = false;
			}
		}
		

		if (isMine) {


			autotRadian += 1f * Time.deltaTime;//自由旋转的射线
			var end = transform.position + new Vector3 (10f * Mathf.Cos (autotRadian), transform.position.y, 10f * Mathf.Sin (autotRadian));
			Debug.DrawLine (transform.position, end, Color.green);
			//==============================================================================================================

			Vector2 joy = UI_Joy.instance.getPercentTransfer ();
//			jiaodu = CameraController.instance.getAngleOffset ();
			var hudu = CameraController.instance.getRadianOffset ();


			var longline = Mathf.Sqrt (joy.x * joy.x + joy.y * joy.y);//joy盘的输出偏移值用以控制速度  magetitude 
			var joyHuDu = Mathf.Atan2 (joy.y, joy.x);//joy盘的动态相对偏角
			var disRange = 10f;//一条长度为10的线
			if (joyHuDu != 0f) {
				joyHuDu -= Mathf.PI / 2;
			}  
		 
			moveSpeed = longline;//移动的速度 * joy 输出量
			moveSpeed = moveSpeed > MaxTrasferSpeed ? MaxTrasferSpeed : moveSpeed;//移动的最大速度
			////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			UpdateCheckObstacle ();
			UpdateCheckEnemy ();
			////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			#region 旋转
			if (0f != joyHuDu) {
				float targetHuDu = -1f * (hudu - joyHuDu) + Mathf.PI / 2;//因为相机会自由视角旋转，所以要把相机偏角量-joy的偏移角 = 目标相对弧度
				targetPoint = transform.position + new Vector3 (disRange * Mathf.Cos (targetHuDu), 0f, disRange * Mathf.Sin (targetHuDu));
				//TODO send to sever by UDP  :: targetPoint
				//if (player.sendStatus != null) {
				//	DebugTool.LogOrange("上报 status , idle");
				//	player.sendStatus (this.transform.localEulerAngles.y,targetPoint, moveSpeed, msg.State.IDLE);	
				//}
				Quaternion targetRotate = Quaternion.LookRotation (new Vector3 (targetPoint.x, 0f, targetPoint.z) - new Vector3 (transform.position.x, 0f, transform.position.z));
			 
				//   transform.rotation = Quaternion.Slerp (transform.rotation, targetRotate, Time.deltaTime * rotateSpeed);//turn direction
				transform.localRotation = targetRotate;
				 
  
			}
			#endregion 旋转
			#region 行走
			if (0f < moveSpeed) {
				if (anim == null) {
					anim = this.GetComponent<Animator> ();
				}
				if (anim.applyRootMotion) {
					anim.SetFloat ("InputVertical", moveSpeed);
					//TODO send to server by UDP  :: MoveSpeed
//					print (joyRandian + "/" + MoveSpeed + "/" + longline);
				} else {
					anim.SetFloat ("InputVertical", moveSpeed);
					transform.Translate (Vector3.forward * Time.deltaTime *  moveSpeed * 2f); // move forward
				}
				if (player.sendStatus != null) {
					DebugTool.LogOrange("上报 status , WALK " );
					//player.sendStatus (targetPoint, moveSpeed, msg.State.WALK);	
					
					player.sendStatus (targetPoint, transform.position, moveSpeed, msg.State.WALK ,false);	
				}
				lock_Idle_snd = false;///有走过，就可以解开上报停止的数据
			} else {// speed is 0
               
                moveSpeed = 0f;
                if (lock_Idle_snd == false)
                {
                    if (anim == null)
                    {
                        anim = this.GetComponent<Animator>();
                    }
                    if (anim.applyRootMotion)
                    {
                        anim.SetFloat("InputVertical", moveSpeed);
                        //TODO send to server by UDP  :: MoveSpeed

                    }
                    else
                    {
                        anim.SetFloat("InputVertical", moveSpeed);
                        transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed); // move forward
                    }


                    if (player.sendStatus != null)
                    {
                        print("snd .........站立: ");
                        DebugTool.LogOrange("上报 status , idle 2");
                        // player.sendStatus (targetPoint, moveSpeed, msg.State.IDLE);	
                        player.sendStatus(targetPoint, this.transform.position, moveSpeed, msg.State.IDLE , true);
                    }
                    // delayTimeOfLock -= Time.fixedDeltaTime;
                    delayTimeOfLock -= Time.deltaTime;
                    if (0f > delayTimeOfLock)
                    {//延时处理锁定，用意在于，多次上报speed=0的指令，防止UDP的不可靠性造成丢包
                        lock_Idle_snd = true; //锁定，在下一次走动之前不许再上报 idle 数据
                        delayTimeOfLock = .1f;
                    }
                }

            }
			#endregion 行走


		} else {
			//同步别人的信息
//			print ("others : speed" + MoveSpeed);
			//others born state  stand
 
		} 

		UpdateCheckGround ();
		UpdateJumpDown ();

//		if (isJumping == false) {//记下玩家非跳跃时的平地坐标，用以较准
//			localPositionRecord = this.transform.position;
//		}
	}

	//	Vector3 localPositionRecord;

	 

	#region ##################### 测定前向障碍 ######

	void UpdateCheckObstacle ()
	{

		#region ############# 前向射线相关 ###############
		//			if (!this.isMine)//只同步自己的
		//				return;



		if (-1 == shootableMask) {
			shootableMask = LayerMask.GetMask ("Default");
		}
		// Set the shootRay so that it starts at the end of the gun and points forward from the barrel.
		shootRay.origin = transform.position + Vector3.up * .5f;
		shootRay.direction = transform.forward;
		//////////////////////////////
//		gunLine.enabled = true; xxxxx
		//			gunLine.SetPosition (0, transform.position + Vector3.up * .5f);
//		gunLine.SetPosition (0, shootRay.origin); xxxxx
		//////////////////////////////
		// Perform the raycast against gameobjects on the shootable layer and if it hits something...
		if (Physics.Raycast (shootRay, out shootHit, range, shootableMask)) {
			moveSpeed = 0f;
			//				print ("hit sthing ");
//			gunLine.SetPosition (1, shootHit.point); xxxxx
		} else {
//			gunLine.SetPosition (1, shootRay.origin + shootRay.direction * range); xxxxx
			//				gunLine.SetPosition (1, transform.position + Vector3.up + shootRay.direction * range);
		}
		#endregion ############# 前向射线相关 ###############

	}

	#endregion ##################### 测定前向障碍 ######

	#region ##### 敌人检测 #######

	float dir = -90f;

	void UpdateCheckEnemy ()
	{

		#region ############# 前向射线相关 ###############
		//			if (!this.isMine)//只同步自己的
		//				return;



		if (-1 == enemyMask) {
			enemyMask = LayerMask.GetMask ("Player");
		}
		// Set the shootRay so that it starts at the end of the gun and points forward from the barrel.
		enemyRay.origin = transform.position + Vector3.up * .5f;

		dir += 1f;
		if (dir > 0f) {
			dir = -180f;
		}
		var range = 10f;
		enemyRay.direction = transform.forward + new Vector3 (range * Mathf.Cos (dir * Mathf.PI / 180f), 0f, range * Mathf.Sin (dir * Mathf.PI / 180f));
		//////////////////////////////
		gunLine.enabled = false;//TODO
		gunLine.SetPosition (0, enemyRay.origin);
		//////////////////////////////
		// Perform the raycast against gameobjects on the shootable layer and if it hits something...
		if (Physics.Raycast (enemyRay, out shootHit, range, enemyMask)) {
			//				print ("hit sthing ");
			gunLine.SetPosition (1, shootHit.point);
		} else {
			gunLine.SetPosition (1, enemyRay.origin + enemyRay.direction * range);
		}
		#endregion ############# 前向射线相关 ###############

	}

	#endregion ########


	#region  ###################### fall down & ground check #########################

	[HideInInspector]
	public bool
		isGrounded,
		isStrafing,
		isSprinting,
		isSliding;
	protected float groundDistance;
	protected float verticalVelocity;
	float DownRange = 1f;
	Ray rayGroundHit = new Ray ();
	public RaycastHit groundHit;

	public virtual void UpdateCheckGround () //use in Update
	{
		if (anim == null || !anim.enabled)
			return;


		#region ############# 底向射线相关 ###############
		//			if (!this.isMine)//只同步自己的
		//				return;



		if (-1 == shootableMask) {
			shootableMask = LayerMask.GetMask ("Default");
		}
		// Set the shootRay so that it starts at the end of the gun and points forward from the barrel.
		rayGroundHit.origin = transform.position + Vector3.up * .5f;
		rayGroundHit.direction = transform.up * -1;
		//////////////////////////////
//		gunLine.enabled = true;
//		gunLine.SetPosition (0, rayGroundHit.origin);
		//////////////////////////////
		// Perform the raycast against gameobjects on the shootable layer and if it hits something...
		if (Physics.Raycast (rayGroundHit, out groundHit, DownRange, shootableMask)) {
			 
//			gunLine.SetPosition (1, groundHit.point);
			groundDistance = Vector3.Distance (transform.position + Vector3.up * 0f, groundHit.point);
//			print ("hit sthing :" + grounddis);
			if (.2f > groundDistance) {
				isGrounded = true;
//				print ("is ground");
			} else {
				isGrounded = false;
//				print ("is not ground 1");
			}

		} else {
//			isGrounded = false;
//			print ("is not ground 2");
//			gunLine.SetPosition (1, rayGroundHit.origin + rayGroundHit.direction * DownRange);
		}
//		print (anim.applyRootMotion + " | " + MoveSpeed);
		#endregion ############# 底向射线相关 ###############

		anim.SetBool ("IsStrafing", isStrafing);//true 作战形态动画
		anim.SetBool ("IsLanded", isGrounded);
		anim.SetFloat ("GroundDis", groundDistance);

//		if (!isGrounded)
//			anim.SetFloat ("VerticalVelocity", verticalVelocity);

//		if (isStrafing) {
//			// strafe movement get the input 1 or -1
//			anim.SetFloat ("InputHorizontal", direction, 0.1f, Time.deltaTime);
//		}

//		// fre movement get the input 0 to 1
//		anim.SetFloat ("InputVertical", speed, 0.1f, Time.deltaTime);
	}

	#endregion ###################### fall down & ground check #########################

	 
	#region jump ######################

	[HideInInspector]
	public bool isJumping;
	Rigidbody _rigidbody;
	[HideInInspector]
	public float jumpCounter;
	//	[Tooltip ("How much time the character will be jumping")]
	//	public float jumpTimer = 0.3f;
	[Tooltip ("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
	public const float jumpHeight = 4f;

	public virtual void JumpUp (bool isMine)
	{
//		print ("Jump");
		if (_rigidbody == null) {
			_rigidbody = this.GetComponent<Rigidbody> ();
		}
		// conditions to do this action
		bool jumpConditions = isGrounded && !isJumping; //如果在地面上，且不在跳跃中，则可跳
		// return if jumpCondigions is false
		if (!jumpConditions)
			return;
		// trigger jump behaviour
		jumpCounter = .3f;//jumpTimer;    跳跃持续时长        
		isJumping = true;
		// trigger jump animations            
		if (this._rigidbody.velocity.magnitude < 1)//重力小于1时，说明往下掉? 切换到跳跃
			anim.CrossFadeInFixedTime ("Jump", 0.1f);
		else
			anim.CrossFadeInFixedTime ("JumpMove", 0.2f);

		#region send to server 
		if (player.sendStatus != null && isMine) {
			DebugTool.LogOrange("上报 status , jump");
			player.sendStatus (targetPoint, this.transform.position, moveSpeed, msg.State.JUMP,true);
		}
		#endregion

	}

	#region 跳起后的 回落

	private void UpdateJumpDown ()
	{
		if (!isJumping || _rigidbody == null)
			return;

		jumpCounter -= Time.deltaTime;
		if (jumpCounter <= 0) {
			jumpCounter = 0;
			isJumping = false;
//			this.transform.position = localPositionRecord;
		}
		// apply extra force to the jump height
		var vel = this._rigidbody.velocity;
		vel.y = jumpHeight;
		vel.z = moveSpeed;
//		print ("jump:" + vel);
		this._rigidbody.velocity = vel;
	}

	#endregion 跳起后的 回落

	#endregion jump ######################







	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	#region ###################### Remote Call API  处理远程调用

	public void SyncOtherRotate (Vector3 targetPoint_dir)
	{
        var dir = new Vector3(targetPoint_dir.x, 0f, targetPoint_dir.z) - new Vector3(transform.position.x, 0f, transform.position.z);
        //if (dir.magnitude != 0f)
        //{
            Debug.Log(" rcv dir :::::::::::::: " + targetPoint_dir.x + "|" + targetPoint_dir.y + "|" + targetPoint_dir.z);
            Quaternion targetRotate = Quaternion.LookRotation(dir, Vector3.up);
            transform.localRotation = targetRotate;

        //}
        //targetPoint.y = 0f;
        //transform.LookAt(targetPoint);
    }

	public void SyncOthersSpeed (float speed)
	{
		if (anim == null) {
			anim = this.GetComponent<Animator> ();
		}
		Debug.Log("他人的速度: " + speed);
		if (anim.applyRootMotion) {
			anim.SetFloat ("InputVertical", speed);
		} else {
			anim.SetFloat ("InputVertical", moveSpeed);
			transform.Translate (Vector3.forward * Time.deltaTime * speed* 2f); // move forward
		}
	}
    private Vector3 _netTarget_dir;
    private Vector3 _netTarget;
	private float _netSpeed;
	private Action _moveToNetTarget ;
	private void moveToNetTarget(){
		// print("move to target ");
		if (anim == null) {
			anim = this.GetComponent<Animator> ();
		}
        //Quaternion targetRotate = Quaternion.LookRotation (new Vector3 (_netTarget.x, 0f, _netTarget.z) - new Vector3 (transform.position.x, 0f, transform.position.z));
        //transform.rotation = targetRotate;
        this.SyncOtherRotate(_netTarget_dir);

        // var duration = Vector3.Distance(_netTarget, transform.position) * .1f;
        anim.SetFloat ("InputVertical", _netSpeed);
        SetTarget(_netTarget_dir);
        transform.position += (_netTarget - transform.position) * .1f ; // move forward
        //this.transform.position = Vector3.Lerp(this.transform.position, _netTarget, .2f);
        //transform.position = _netTarget;
    }
	public void SyncOthersSpeed2 (Vector3 dir , Vector3 targetPos , float speed)
	{
		_netTarget = targetPos;
        _netTarget_dir = dir;
        _netSpeed = speed;
		if(_moveToNetTarget == null){
			_moveToNetTarget = moveToNetTarget; 
		}
        if (anim == null)
        {
            anim = this.GetComponent<Animator>();
        }
        //this.SyncOtherRotate(dir);
        //anim.SetFloat("InputVertical", 1f);
        //SetTarget(_netTarget_dir);
        //transform.position = _netTarget;
        // print("SyncOthersSpeed2");

    }
	public void SyncIdle(Vector3 dir){
			 
			// this.SyncOthersSpeed (0f);
            this.SyncOtherRotate(dir);
            Debug.Log("rcv 站立!!!!!!");
			_netSpeed = 0f;
			anim.SetFloat ("InputVertical", _netSpeed);
            _moveToNetTarget = null;
	}
	#region drag back 拉回

	public void SyncDrageBack (Vector3 targetPoint)
	{
		if (anim == null) {
			anim = this.GetComponent<Animator> ();
		}
		anim.applyRootMotion = false;
		anim.SetFloat ("InputVertical", .3f);
		this.transform.position = Vector3.Lerp (this.transform.position, targetPoint, .2f);
		Quaternion targetRotate = Quaternion.LookRotation (new Vector3 (targetPoint.x, 0f, targetPoint.z) - new Vector3 (transform.position.x, 0f, transform.position.z));
		transform.rotation = targetRotate;
	}

	public void ApplyRootMotion ()
	{
		if (anim == null) {
			anim = this.GetComponent<Animator> ();
		}  
		anim.SetFloat ("InputVertical", 0f);
		anim.applyRootMotion = false;//true;
	}

	public Action applyRootMotion;

    #endregion drag back 拉回

    #endregion ###################### end Remote Call API


    private Transform _cube;
    public void SetTarget(Vector3 pos)
    {
        if (null == _cube)
        {
            _cube = this.transform.Find("Cube");

        }
            Debug.Log(":::::::::::::::::::: cube pos: " + _cube.position);
            _cube.position = pos;
    }
}