using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LuaInterface;
using System;

/// <summary>
/// Lua管理类
/// </summary>
public class LuaManager : MonoBehaviour
{
    static LuaManager instance;
    public LuaState luaState = null;
    List<string> luaFileList = new List<string>();
    Text LogText = null;

    public static LuaManager GetInstance()
    {
        if (instance == null)
        {
            GameObject obj = new GameObject();
            obj.name = "LuaManager";
            instance = obj.AddComponent<LuaManager>();
            GameObject.DontDestroyOnLoad(obj);
        }
        return instance;
    }
    /// <summary>
    /// Lua初始化操作
    /// </summary>
    public void Init()
    {
        luaState = new LuaState();
        luaState.AddSearchPath(Application.streamingAssetsPath + "/ToLua/Lua");
        luaState.Start();
        Bind();
        InitLuaFile();
    }
    /// <summary>
    /// Lua虚拟机注册
    /// </summary>
    protected virtual void Bind()
    {
        LuaBinder.Bind(luaState);
        DelegateFactory.Init();
        LuaCoroutine.Register(luaState, this);
    }

    /// <summary>
    /// 初始化Lua相关文件与脚本
    /// </summary>
    private void InitLuaFile()
    {
        TextAsset textAsset = null;
        textAsset = ResourceManager.GetInstance().LoadtextAsset("game_lua/init");
        if (textAsset != null)
            LoadLuaInit(textAsset.text);

        if (!ResourceManager.GetInstance().bLoadFromStream)
            luaState.AddSearchPath(Application.dataPath + "/Resources/game_lua");

        foreach (var item in luaFileList)
        {
            if (ResourceManager.GetInstance().bLoadFromStream)
            {
                AssetBundle bundle = ResourceManager.GetInstance().LoadAssetBundle("game_lua/" + item + ".lua");
                TextAsset text = bundle.LoadAsset(item + ".lua") as TextAsset;
                luaState.DoString(text.text,"LuaManager.cs");
                Debug.Log("AB:初始化脚本:" + item);
            }
            else
            {
                luaState.DoString(strGetLuaFileText(item), "LuaManager.cs");
                Debug.Log("Resources:初始化脚本:" + item);
            }
        }
        Debug.Log("开始游戏逻辑");
        CallLuaFunction("Main.Start");
    }

    /// <summary>
    ///读取lua的初始化配置
    /// </summary>
    private void LoadLuaInit(string filetext)
    {
        luaFileList.Clear();
        if (filetext != null)
        {
            string[] Lines = filetext.Split('\n');
            for (int i = 0; i < Lines.Length; i++)
            {
                string strFileName = Lines[i].Replace(" ", "");
                strFileName = strFileName.Replace("\r", "");
                luaFileList.Add(strFileName);
            }
        }
    }

    /// <summary>
    /// 读取Lua脚本
    /// </summary>
    /// <param name="strFileName"></param>
    /// <returns></returns>
    public string strGetLuaFileText(string strFileName)
    {
        strFileName = "game_lua/" + strFileName + ".lua";
        TextAsset fileText = ResourceManager.GetInstance().LoadtextAsset(strFileName);
        if (fileText != null)
            return fileText.text;
        else
        {
            Debug.Log(strFileName + " is null!!!!");
            return "";
        }
    }
    /// <summary>
    ///Lua方法执行
    /// </summary>
    /// <param name="name"></param>

    public void CallLuaFunction(string name)
    {
        try
        {
            LuaFunction function = luaState.GetFunction(name);
            if (function != null)
            {
                function.Call();
            }
            else
            {
                Debug.LogError("没有找到lua方法!" + name);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("执行LUA方法失败!" + name + "原因" + ex);
        }
    }

    /// <summary>
    /// Lua执行方法泛型类
    /// </summary>
    /// <param name="name"></param>
    /// <param name="args"></param>
    public void CallLuaFunction(string name, params object[] args)
    {
        try
        {
            LuaFunction function = luaState.GetFunction(name);
            if (function != null)
            {
                object[] res = function.Invoke<object[], object[]>(args);
            }
            else
            {
                Debug.LogError("没有找到lua方法!" + name);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("执行LUA方法失败!" + name);
        }
    }
}
