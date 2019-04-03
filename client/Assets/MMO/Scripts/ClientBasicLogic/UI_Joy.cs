using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_Joy : MonoBehaviour
{
	public Image bg;
	public Image panel;
	public Vector3 pos;

	public Vector3 bgPos;


	public Rect bgrect;
	public Rect rect;


	public Vector3 originalPos;
	[SerializeField]
	private Image img;

	private Vector2 min;
	private Vector2 max;
	// more big value more moveable district
	private float edge = 0.5f;
	public Vector2 transfer;
	public Vector2 transferPercent;
	public static UI_Joy instance;

	void Awake ()
	{
		instance = this;
		
	}
	// Use this for initialization
	void Start ()
	{
		EventTriggerListener.Get (gameObject).onEndDrag = mouseEndDrag;
		EventTriggerListener.Get (gameObject).onDrag = mouseDrag;
		EventTriggerListener.Get (gameObject).onDown = mouseDown;
		img = this.GetComponent<Image> ();
		originalPos = img.rectTransform.position;
		pos = originalPos;


		 

		bgPos = bg.transform.position;

		bgrect = bg.rectTransform.rect;
		rect = img.rectTransform.rect;

		this.transform.SetParent (bg.transform);

		getEdge ();

		transform.localPosition = Vector3.zero; 
	}
 	// void setUI(){
         
    //     var uijoyRect = GameObject.Find ("InGame").transform.Find ("Safari").GetComponent<RectTransform> ();
    //     //this.gameObject.transform.parent.parent.GetComponent<RectTransform> ();
    //     uijoyRect.anchorMin = new Vector2 (0, 0);
    //     uijoyRect.anchorMax = new Vector2 (1, 1);
    //     uijoyRect.pivot = new Vector2 (.5f, .5f);
    //     //uijoyRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left,
    //     uijoyRect.offsetMin = Vector2.zero;//new Vector2(100f,72f);  // left, bottom
    //     uijoyRect.offsetMax = Vector2.zero;//new Vector2(-140f,0f); // right, top

    //     uijoyRect.localScale = Vector3.one;
    //     print ("uijoyRect>>>>>"+ uijoyRect);
        
    // }
    float radius = 0f;
	private void getEdge ()
	{
		var w = bgrect.width - rect.width;
        var h = bgrect.height - rect.height;
        radius = w * 2f;//.4f;

        var xMin = originalPos.x - w * edge;
        var yMin = originalPos.y - h * edge;
        min = new Vector2 (xMin, yMin);

        var xMax = originalPos.x + w * edge;
        var yMax = originalPos.y + h * edge;
        max = new Vector2 (xMax, yMax);
	}

	// public void resetJoyPos ()
	// {
	// 	#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
	// 	if (Input.GetMouseButtonDown (0)) {
	// 		#elif UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
	// 	if (Input.GetTouch (0).phase == TouchPhase.Began && Input.touchCount.Equals (1)) {
	// 		#endif
	// 		//   var panelTran = panel.rectTransform;
	// 		//   
	// 		//   var minX = panelTran.position.x;
	// 		//   var minY = panelTran.position.y;
	// 		//   var maxX = minX + panelTran.rect.width * panelTran.localScale.x;
	// 		//   var maxY = minY + panelTran.rect.height * panelTran.localScale.y;
	// 		//   var district = new Rect (minX, minY, maxX, maxY);
	// 		//   print (" district : " + district);
	// 		////   print ("rect: " + panel.rectTransform.rect.xMin + "   :  " + panel.rectTransform.rect.xMax);
	// 		//   print ("rect: " + panel.rectTransform.rect + " mouse : " + Input.mousePosition);
	// 		//   print (" panel xy: " + panel.rectTransform.position);
	// 		//   print ("panel scale : " + panel.rectTransform.localScale);
	// 		//   print ("panel offset min : " + panel.rectTransform.offsetMin);
	// 		//   print ("panel offset max : " + panel.rectTransform.offsetMax);
	// 		transform.localPosition = Vector3.zero;//Input.mousePosition;
	// 		bg.transform.position = Input.mousePosition;
	// 		bgPos = bg.transform.position;
	// 		originalPos = img.rectTransform.position;
	// 		getEdge ();
	// 		//==
	// 		pos = originalPos;
	// 	}
	// }

	void syncPos ()
	{
		#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        Vector3 tmp = Input.mousePosition;
        #elif UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
        var tmp = Input.GetTouch(0).position;
        #endif


        #region 约束 摇杆活动范围::=================
        //method one::================= 摇杆在一个四方形中移动，不够自然
        //tmp = new Vector3 (Mathf.Clamp (tmp.x, min.x, max.x), Mathf.Clamp (tmp.y, min.y, max.y), 0f);
        //img.rectTransform.position = tmp;
        //pos = tmp;


        //method two::======================== 摇杆在指定半径的圆中移动
        float dis = Vector2.Distance (tmp, originalPos);
        if (dis < radius) {
            print ("未超出" + dis + "<" + radius);
            img.rectTransform.position = tmp;
            pos = tmp;
        } else {
            float targetHuDu = getRadianByPoint(tmp,originalPos) ;//坐标求得弧度
            Vector3 targetPoint = getPointByRadian(targetHuDu,radius,originalPos);//将弧度与指定半径，求得目标点
            /*if (img2 == null) {
                img2 = new GameObject("joy");
                img2.AddComponent<Image> ();
                img2.transform.SetParent (img.gameObject.transform.parent);
            }
            img2.GetComponent<RectTransform>().position = targetPoint;
            */
            img.rectTransform.position = targetPoint;
            pos = targetPoint;
            print ("超出"+ dis + ">" + radius);
        }
        #endregion =============
	}
#region 
	//工具 ======================
    //得半径
    // public float getRadiusByPoint2(){
    //     #if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
    //     Vector3 tmp = Input.mousePosition;
    //     #elif UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
    //     var tmp = Input.GetTouch(0).position;
    //     #endif
    //     return getRadiusByPoint (tmp, this.originalPos);
    // }
    /// <summary>
    /// Gets the radius by point.
    /// </summary>
    /// <returns>The radius by point. 相对原点，在指定点，得到半径长度</returns>
    /// <param name="targetPoint">Target point.</param>
    /// <param name="originalPos">Original position.</param>
    // private float getRadiusByPoint(Vector2 targetPoint, Vector3 originalPos){
    //     Vector2 joy = new Vector2 (targetPoint.x - originalPos.x, targetPoint.y - originalPos.y);//transferPercent;
    //     float longline = Mathf.Sqrt (joy.x * joy.x + joy.y * joy.y);
    //     //float joyHuDu = Mathf.Atan2 (joy.y, joy.x);//坐标求得弧度
    //     return longline;
    // }
    //得弧度
    // public float getRadianByPoint2(){
    //     #if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
    //     Vector3 tmp = Input.mousePosition;
    //     #elif UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
    //     var tmp = Input.GetTouch(0).position;
    //     #endif
    //     return getRadianByPoint(tmp,this.originalPos);
    // }
    /// <summary>
    /// Gets the radian by point.
    /// </summary>
    /// <returns>The radian by point. 相对于原点，在指定坐标点，得到相对弧度</returns>
    /// <param name="targetPoint">Target point. 某个坐标</param>
     /// <param name="originalPos">Original position. 原点</param>
    private float getRadianByPoint(Vector2 targetPoint , Vector3 originalPos){
        Vector2 joy = new Vector2 (targetPoint.x - originalPos.x, targetPoint.y - originalPos.y);//transferPercent;
        float joyHuDu = Mathf.Atan2 (joy.y, joy.x);//坐标求得弧度
        return joyHuDu;
    }
    /// <summary>
    /// Gets the point byt radio.
    /// </summary>
    /// <returns>The point byt radio. 相对于原点，在指定的弧度和指定半径，得到一个坐标点</returns>
    /// <param name="targetHuDu">Target hu du. 某一弧度</param>
    /// <param name="disRange">Dis range. 距离原点的半径</param>
    /// <param name="originalPos">Original position. 原点</param>
    public Vector3 getPointByRadian(float targetHuDu,float disRange , Vector3 originalPos){
        Vector3 targetPoint = originalPos + new Vector3 (disRange * Mathf.Cos (targetHuDu), disRange * Mathf.Sin (targetHuDu) ,0f); //弧度求得目标坐标
        return targetPoint;
    }

    /// <summary>
    /// Gets the point byt radio.
    /// </summary>
    /// <returns>The point byt radio. 相对于原点，在指定的弧度和指定半径，得到一个坐标点</returns>
    /// <param name="targetHuDu">Target hu du. 某一弧度</param>
    /// <param name="disRange">Dis range. 距离原点的半径</param>
    /// <param name="originalPos">Original position. 原点</param>
    public Vector3 getPointByRadian3D(float targetHuDu,float disRange , Vector3 originalPos){
        Vector3 targetPoint = originalPos + new Vector3 (disRange * Mathf.Cos (targetHuDu), originalPos.y ,disRange * Mathf.Sin (targetHuDu) ); //弧度求得目标坐标
        return targetPoint;
    }
    #endregion =====================
	// Update is called once per frame
	void Update ()
	{
		transfer = new Vector2 (pos.x - originalPos.x, pos.y - originalPos.y);
	}

	public Vector2 getPercentTransfer ()
	{
		var dis = (max.x - min.x) * .5f;
		return new Vector2 (transfer.x / dis, transfer.y / dis);
	}

	public void mouseEndDrag (GameObject obj)
	{
		//   print ("mouse end drag");

		img.rectTransform.position = originalPos;
		pos = originalPos;
	}

	public void mouseDrag (GameObject obj)
	{
		//   print ("mouse drag");

		syncPos ();
		transferPercent = getPercentTransfer ();
	}

	public void mouseDown (GameObject obj)
	{
		//   resetJoyPos ();

		//   print ("mouse down");
		//   img.GetComponent<RectTransform> ().pivot.Set (0f, 0f);
		//   transform.position = Input.mousePosition;

	}

	// public void mouseMove (GameObject obj)
	// {
	// 	//   print ("mouse move");
	// }
}
