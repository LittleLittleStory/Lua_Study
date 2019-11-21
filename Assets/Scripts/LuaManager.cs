using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LuaInterface;
using System;

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
    public void Init()
    {
        luaState = new LuaState();
        luaState.Start();
        Bind();
        InitLuaFile();
    }
    protected virtual void Bind()
    {
        LuaBinder.Bind(luaState);
        DelegateFactory.Init();
        LuaCoroutine.Register(luaState, this);
    }

    private void InitLuaFile()
    {
        TextAsset textAsset = null;
        textAsset = Resources.Load<TextAsset>("game_lua/init");
        if (textAsset != null)
            LoadLuaInit(textAsset.text);
        luaState.AddSearchPath(Application.dataPath + "/Resources/game_lua");

        foreach (var item in luaFileList)
        {
            luaState.Require(item);
            Debug.Log("初始化脚本:" + item);
        }
        CallLuaFunction("Main.Start");
    }

    //读取lua的初始化配置
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
    //Lua方法执行
    public void CallLuaFunction(string name)
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
        try
        {

        }
        catch (Exception ex)
        {
            Debug.LogError("执行LUA方法失败!" + name + "原因" + ex);
        }
    }
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
    public void CallLuaFunction(string name, Array array)
    {
        try
        {
            LuaFunction function = luaState.GetFunction(name);
            if (function != null)
            {
                function.Push(array);
                function.Call();
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
