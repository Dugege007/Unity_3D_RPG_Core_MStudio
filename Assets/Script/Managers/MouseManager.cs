using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Events;
using System;


//[System.Serializable]//将未继承自MonoBehaviour的类序列化，以在Inspector面板中显示参数
//public class EventVector3 : UnityEvent<Vector3> { }
////若使用代码控制事件关联的对象，则不需要在Inspector中显示，以上代码可以删掉

public class MouseManager : Singleton<MouseManager>
{
    //单例模式
    //public static MouseManager Instance;
    //已有泛型单例，上方代码不需要了

    //记录射线膨胀到的物体的相关信息
    private RaycastHit hitInfo;

    //public EventVector3 OnMouseClicked;
    public event Action<Vector3> OnMouseClicked;
    public event Action<GameObject> OnEnemyClicked;

    public Texture2D point, doorway, attack, target, arrow;

    //private void Awake()
    //{
    //    if (Instance!=null)
    //        Destroy(gameObject);
    //    Instance = this;
    //}
    //已有泛型单例，上方代码不需要了

    protected override void Awake()
    {
        base.Awake();   //包含继承的Awake()中的代码

        //在切换场景时不销毁
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    private void SetCursorTexture()
    {
        //从摄像机向鼠标处发射一条射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray,out hitInfo))
        {
            //切换鼠标贴图
            switch (hitInfo.collider.gameObject.tag)
            {
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(0, 0), CursorMode.Auto);
                    break;
                case "Portal":
                    Cursor.SetCursor(doorway, new Vector2(16, 16), CursorMode.Auto);
                    break;

                default:
                    Cursor.SetCursor(arrow, new Vector2(0, 0), CursorMode.Auto);
                    break;
            }
        }
    }

    void MouseControl()
    {
        //如果鼠标左键点击，并且射线碰撞到的不为空
        if (Input.GetMouseButtonDown(1)&&hitInfo.collider!=null)
        {
            //判断射线碰撞到地面点的位置，传回角色的Agent
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
                //点击鼠标左键触发事件
                OnMouseClicked?.Invoke(hitInfo.point);
            //触发后，事件中订阅的函数将会全部执行

            //Debug.Log("目标点：" + hitInfo.point);

            //判断射线碰撞到的敌人，传回角色的Agent
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            if (hitInfo.collider.gameObject.CompareTag("Attackable"))
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            if (hitInfo.collider.gameObject.CompareTag("Portal"))
                OnMouseClicked?.Invoke(hitInfo.point);
        }
    }
}
