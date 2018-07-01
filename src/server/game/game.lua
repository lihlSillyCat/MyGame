local skynet = require "skynet"
local servicemsg = require "servicemsg"

local gate = tonumber(...)

local net_cmd = {}

--玩家进入游戏
local function user_enter(uid, name)
	-- body
end

--玩家退出游戏
local function user_out(uid)
	-- body
end


skynet.start(function()
    
    skynet.send(gate, "lua", "register", servicemsg.game)
    
end)
