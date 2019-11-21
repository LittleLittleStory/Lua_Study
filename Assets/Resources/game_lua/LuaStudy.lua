Main = {}

--local GameObject = UnityEngine.GameObject

function Main.Start()
    --go = GameObject.Instantiate(Resources.Load("Cube"))
    --go.name = "123"
    --local button = UnityEngine.GameObject.Find("Button")
    --EventTriggerListener.Get(button.gameObject).OnClickCall = "Main.Fire"
    --itemmaneger.Test()
end

function Main.Fire(gameobject)
    print("fire")
    return gameobject
end
function fib(n)
    local a, b = 0, 1
    while n > 0 do
        a, b = b, a + b
        n = n - 1
    end

    return a
end

--携程
function CoFunc()
    print("Coroutine started")
    for i = 0, 10, 1 do
        print(fib(i))
        coroutine.wait(5)
    end
    print("current frameCount: " .. Time.frameCount)
    coroutine.step()
    print("yield frameCount: " .. Time.frameCount)

    local www = UnityEngine.WWW("http://www.baidu.com")
    coroutine.www(www)
    local s = tolua.tolstring(www.bytes)
    print(s:sub(1, 128))
    print("Coroutine ended")
end

function TestCortinue()
    coroutine.start(CoFunc)
end

local coDelay = nil

function Delay()
    local c = 1

    while true do
        coroutine.wait(5)
        print("Count: " .. c)
        c = c + 1
    end
end

function StartDelay()
    coDelay = coroutine.start(Delay)
end

function StopDelay()
    coroutine.stop(coDelay)
end
--数组
--声明，这里声明了类名还有属性，并且给出了属性的初始值。
item = {}

--这句是重定义元表的索引，就是说有了这句，这个才是一个类。
item.__index = item

--构造体，构造体的名字是随便起的，习惯性改为New()
function item:New(id)
    local self = {} --初始化self，如果没有这句，那么类所建立的对象改变，其他对象都会改变
    setmetatable(self, item) --将self的元表设定为Class
    self.id = id
    return self --返回自身
end

itemmaneger = {}

itemmaneger.__index = itemmaneger

function itemmaneger:New()
    local self = {} --初始化self，如果没有这句，那么类所建立的对象改变，其他对象都会改变
    setmetatable(self, itemmaneger) --将self的元表设定为Class
    self.itemlist = {}
    return self --返回自身
end

function itemmaneger:DiraddItem(name,id)
    local _item = item:New(id)
    print("字典")
    self.itemlist[name]= _item
end

function itemmaneger:ListaddItem(id)
    local _item = item:New(id)
    print("数组")
    table.insert(self.itemlist, _item)
end

function itemmaneger.Test()
    local itemmaneger = itemmaneger:New()
    itemmaneger:DiraddItem("abc","1")
    itemmaneger:DiraddItem("xpf","2")
    itemmaneger:DiraddItem("kfc","3")
    --print(itemmaneger.itemlist[2].id)
    for key, value in pairs(itemmaneger.itemlist) do
        print(key, value.id)
    end
end
