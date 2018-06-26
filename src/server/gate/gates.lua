local skynet = require "skynet"
local gateserver = require "snax.gateserver"
local servicemsg = require "servicemsg"

local handler = {}
local agent = {}

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

function handler.message(fd, msg, sz)
    print("recv client msg:fd["..fd.."]msg["..msg.."]sz["..sz.."]")
    
    --消息解包：1byte（服务id）+ 2byte（消息id，Big-Endian 编码）+ 消息体（protobuf序列化的包）
    if sz >= 3 then

        --根据服务ID将消息分发给对应的服务处理
        local destservice = msg[1]


        if destservice ~= servicemsg.login then
            --                
        end


    else
        print("error msg, disconnect client")
        skynet.send(agent[fd], "lua", "logout")
        gateserver.closeclient(fd)
        agent[fd] = nil
    end
end

function bytes_to_int(str,endian,signed) -- use length of string to determine 8,16,32,64 bits
    local t={str:byte(1,-1)}
    if endian=="big" then --reverse bytes
        local tt={}
        for k=1,#t do
            tt[#t-k+1]=t[k]
        end
        t=tt
    end
    local n=0
    for k=1,#t do
        n=n+t[k]*2^((k-1)*8)
    end
    if signed then
        n = (n > 2^(#t-1) -1) and (n - 2^#t) or n -- if last bit set, negative.
    end
    return n
end

gateserver.start(handler)