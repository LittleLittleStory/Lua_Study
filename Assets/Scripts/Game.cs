using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

/// <summary>
/// 项目基础框架，游戏工具类接口
/// </summary>

public class Game : MonoBehaviour
{
    static Game game;
    public string strChangeRgn = "";
    private string Lua_InvakeCall;

    public static Game GetGame()
    {
        if (game == null)
        {
            
            GameObject gameobj = new GameObject();
            gameobj.name = "Game";
            game = gameobj.AddComponent<Game>();
            GameObject.DontDestroyOnLoad(gameobj);
        }
        return game;
    }
    void Start()
    {
        Application.targetFrameRate = 60;
        //UnityEngine.SceneManager.GetActiveScene().name
    }

    /*void Update()
    {
         LuaManager.GetInstance().CallLuaFunction("game.Update");
    }*/

    public void ChangeRgn(string strSceneName)
    {
        SceneManager.LoadSceneAsync(strSceneName);
    }

    /// <summary>
    /// 场景切换后的回调
    /// </summary>
    void OnLevelWasLoaded(int level)
    {
        Debug.Log("切换场景完成回调!!!!!");
        //LuaManager.GetInstance().CallLuaFunction("game.OnLevelWasLoaded",level);
    }

    /// <summary>
    /// 读取场景
    /// </summary>
    /// <param name="strSceneName">场景名</param>
    public void LoadScene(string strSceneName)
    {
        strChangeRgn = strSceneName;
        if (ResourceManager.GetInstance().bLoadFromStream)
            StartCoroutine(ResourceManager.GetInstance().LoadScene(strSceneName));
        else
            SceneManager.LoadScene("loading");
    }

    /// <summary>
    /// 增加场景
    /// </summary>
    /// <param name="sceneName"></param>
    public void AddScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    /// <summary>
    /// 删除场景
    /// </summary>
    /// <param name="sceneName"></param>
    public void DelScene(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }

    /// <summary>
    /// 得到时间
    /// </summary>
    /// <returns></returns>
    public float GetTime()
    {
        return Time.time;
    }

    /// <summary>
    /// 得到渲染时间
    /// </summary>
    /// <returns></returns>
    public float GetDeltaTime()
    {
        return Time.deltaTime;
    }

    /// <summary>
    /// 射线获取指定层对象
    /// </summary>
    /// <param name="orc"></param>
    /// <param name="dir"></param>
    /// <param name="distance"></param>
    /// <param name="layermask"></param>
    /// <returns></returns>
    public bool GetRay(Vector2 orc,Vector2 dir,float distance,int layermask)
    {
        int Mask = 1 << layermask;
        RaycastHit2D hit2d = Physics2D.Raycast(orc, dir, distance, Mask);
        if (hit2d.collider != null)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 开启延时调用
    /// </summary>
    /// <param name="call"></param>
    /// <param name="time"></param>
    public void OnCreateInvoke(string call,float time)
    {
        Lua_InvakeCall = call;
        Invoke("OnInvokeBack", time);
    }

    /// <summary>
    /// Lua中延时调用后回调
    /// </summary>
    void OnInvokeBack()
    {
        LuaManager.GetInstance().CallLuaFunction(Lua_InvakeCall); 
    }

    /*
    /// <summary>
    /// Lua线程开启
    /// </summary>
    /// <param name="LuaFunc"></param>
    public void OnLuaThread(string LuaFunc)
    {
        LuaManager.GetInstance().OnLuaThread(LuaFunc);
    }*/

    /// <summary>
    /// 创建一个Guid
    /// </summary>
    /// <returns></returns>
    public string GetNewGuid()
    {
        Guid guid = Guid.NewGuid();
        return guid.ToString();
    }

    /// <summary>
    /// 得到当前触发事件的UI对象
    /// </summary>
    /// <returns></returns>
    public bool GetEventSystemOverGameObject()
    {
        if (EventSystem.current == null)
            return false;
        else
            return EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// 得到当前活跃场景的名字
    /// </summary>
    /// <returns></returns>
    public string GetCurActiveScene()
    {
        return SceneManager.GetActiveScene().name;
    }

    /// <summary>
    ///世界坐标转成UI中父节点的坐标, 并设置子节点的位置
    /// </summary>
    /// <returns></returns>
    public Vector3 World2UI(Vector3 wpos)
    {
        Vector3 screemPos = Camera.main.WorldToScreenPoint(wpos);
        GameObject UICanvas = GameObject.Find("Canvas");
        Vector3 UIPos = Vector3.zero;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(UICanvas.transform as RectTransform, screemPos, null, out UIPos);
        UIPos.z = 0;

        return UIPos;
    }
}
