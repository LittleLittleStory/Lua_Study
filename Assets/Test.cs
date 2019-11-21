using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Awake()
    {
        LuaManager.GetInstance().Init();
    }
    void Start()
    {
        LuaManager.GetInstance().CallLuaFunction("Main.Start");
    }

}
