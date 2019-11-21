using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using LuaInterface;
public class EventTriggerListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler,
IPointerExitHandler, IPointerUpHandler, ISelectHandler, IUpdateSelectedHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public delegate void VoidDelegate(GameObject go);
    public VoidDelegate  onClick;
    public VoidDelegate onDown;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VoidDelegate onUp;
    public VoidDelegate onSelect;
    public VoidDelegate onUpdateSelect;
    public VoidDelegate onBeginDrag;
    public VoidDelegate onDrag;

    public string OnClickCall;

    private LuaTable mSelfTable = null;

    static public EventTriggerListener Get(GameObject go)
    {
        EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
        if (listener == null) listener = go.AddComponent<EventTriggerListener>();
        return listener;
    }
    //方法执行
    private void OnCallLua(string lua_Call)
    {
        LuaFunction StartFunc = mSelfTable.GetLuaFunction(lua_Call);
        if (StartFunc != null)
        {
            StartFunc.Call(mSelfTable, gameObject);
        }
    }
    private void CallFunction(string functionName)
    {
        if (!string.IsNullOrEmpty(functionName))
        {
            if (mSelfTable != null)
                OnCallLua(functionName);
            else
                LuaManager.GetInstance().CallLuaFunction(functionName, gameObject);
        }
    }
    //事件接口
    public void OnClick(PointerEventData data)
    {
        CallFunction(OnClickCall);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        CallFunction(OnClickCall);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null) onDown(gameObject);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {

        if (onEnter != null) onEnter(gameObject);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(gameObject);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null) onUp(gameObject);
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null) onSelect(gameObject);
    }
    public void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelect != null) onUpdateSelect(gameObject);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (onBeginDrag != null) onBeginDrag(gameObject);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData data)
    {
        if (onDrag != null) onDrag(gameObject);
    }

}
