using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LuaInterface;
using System;

public class LuaTest : MonoBehaviour
{
    string luaFile = "LuaStudy.lua";
    LuaState state;
    LuaFunction luaFunction;
    void Start()
    {
        state = new LuaState();//建立Lua虚拟机
        state.Start();//启动虚拟机
        LuaBinder.Bind(state);
        state.AddSearchPath(Application.dataPath + "/Resources/game_lua");
        state.DoFile(luaFile);
        CallFunc("Main.Start");
    }
    private void Update()
    {
        CallFunc("Main.Start");
    }
    void CallFunc(string func)
    {
        luaFunction = state.GetFunction(func);
        luaFunction.Call();
    }
}


