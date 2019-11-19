Main = {}

local GameObject = UnityEngine.GameObject

function Main.Start()
    --go = GameObject.Instantiate(Resources.Load("Cube"))
    --go.name = "123"
    local button = GameObject.Find("Button")
    EventTriggerListener.Get(button.gameObject).OnClickCall = "Fire"
end

function Main.Fire(gameobject)
    print(fire)
    return gameobject
end
