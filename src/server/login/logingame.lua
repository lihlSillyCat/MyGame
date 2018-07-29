local skynet = require "skynet"
local servicemsg = require "servicemsg"
local loginmsg = require "loginmsg"

local gate = tonumber(...)

local msghandler = {}

local function user_login(login)
    
    print("logintype:" .. login.logintype)
    print("account:" .. login.account)
    print("passwd:" .. login.passwd)
    print("cversion:" .. login.cversion)
    
end 

msghandler[loginmsg.id.login] = user_login


local function handler(msgid, buffer, sz)  
    local msg = loginmsg.deconde(msgid, buffer, sz)
    if msg then
        msghandler[msgid](msg)
    else
        print("unkown msgid:" .. msgid)
    end
end 

skynet.start(function()
    
    skynet.send(gate, "lua", "register", servicemsg.login)
    
    skynet.dispatch("lua", function(session, source, id, ...)
        handler(msgid, ...)
    end)

end)
