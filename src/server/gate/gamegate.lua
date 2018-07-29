local skynet = require "skynet"
local gateserver = require "snax.gateserver"
local servicemsg = require "servicemsg"
local netpack = require "skynet.netpack"

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
    print("recv client msg:fd["..fd.."]sz["..sz.."]")
    
    if sz >= 3 then
        --根据服务ID将消息分发给对应的服务处理
        local msgunpack = netpack.tostring(msg, sz)
        local dest, msgsub, msghead = string.byte(msgunpack, 1, 3)
        local msgid = msghead * 256 + msgsub

        print("dstid[" .. dest .. "]msgid[" .. msgid .. "]msg[" .. string.sub(msgunpack, 4, -1) .. "]") 

        if service[dest] then
            skynet.send(service[dest], "lua", msgid, string.sub(msg, 4, -1), sz - 3)  
        else
            skynet.error("unkown msgid["..msgid.."]")
        end
        
    else
        print("error msg, disconnect client")
        gateserver.closeclient(fd)
    end


    --[[
    --消息解包：1byte（服务id）+ 2byte（消息id，Big-Endian 编码）+ 消息体（protobuf序列化的包）
    if sz >= 3 then
        local sid = skynet.unpack("B", msg, 1)
        local msgid = skynet.unpack("<H", msg, 2)
        local protobuf = string.sub(msg, 4, -1)

        --根据服务ID将消息分发给对应的服务处理
        if service[msgid] then
            skynet.send(service[msgid], "client", msgid, protobuf, sz - 3)  
        else
            skynet.error("unkown msgid["..msgid.."]")
        end

        --测试
        send(fd, sid + 1, msgid + 1, "hello, i am server")

    else
        print("error msg, disconnect client")
        gateserver.closeclient(fd)
    end
    ]]

end

local CMD = {}
function CMD.register(source, msgid)
    if service[msgid] == nil then
        service[msgid] = source
        print("服务["..source.."]msgid["..msgid.."]注册成功")
    else
        skynet.error("重复注册服务["..source.."]msgid["..msgid.."]")
    end
end

function handler.command(cmd, source, ...)
    local f = assert(CMD[cmd])
    return f(source, ...)   
end

gateserver.start(handler)
