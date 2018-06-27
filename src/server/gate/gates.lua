local skynet = require "skynet"
local gateserver = require "snax.gateserver"
local servicemsg = require "servicemsg"

local handler = {}
local agent = {} --玩家代理
local service = {} --所有注册的服务

function handler.connect(fd, ipaddr)
    print("client:fd["..fd.."]ip["..ipaddr.."] has connected")

    --给每个用户启动一个代理服务
    agent[fd] = skynet.newservice("agents")
    if agent[fd] == nil then
        skynet.error("newservice agents error")
        gateserver.closeclient(fd)
        return
    end

    local info = {
        gate        = skynet.self(),
        fd          = fd,
        ipaddr      = ipaddr,
        uid         = fd
    }
    skynet.call(agent[fd], "lua", "start", info)

    --开始接收网络消息
    gateserver.openclient(fd)
end

function handler.disconnect(fd)
    print("client:fd["..fd.."] has disconnected")  
    skynet.send(agent[fd], "lua", "logout")
    agent[fd] = nil
end

function handler.error(fd, msg)
    print("client:fd["..fd.."] error["..msg.."]")
    skynet.send(agent[fd], "lua", "logout")
    agent[fd] = nil
end

--消息分发
function handler.message(fd, msg, sz)
    print("recv client msg:fd["..fd.."]msg["..msg.."]sz["..sz.."]")
    
    --消息解包：1byte（服务id）+ 2byte（消息id，Big-Endian 编码）+ 消息体（protobuf序列化的包）
    if sz >= 3 then
        --根据服务ID将消息分发给对应的服务处理
        local dest, msghead, msgsub = string.byte(msg, 1, 3)
        local msgid = msghead * 256 + msgsub
        if service[msgid] then
            skynet.send(service[msgid], "client", msgid, string.sub(msg, 4, -1), sz - 3)  
        else
            skynet.error("unkown msgid["..msgid.."]")
        end
    else
        print("error msg, disconnect client")
        skynet.send(agent[fd], "lua", "logout")
        gateserver.closeclient(fd)
        agent[fd] = nil
    end
end

gateserver.start(handler)
