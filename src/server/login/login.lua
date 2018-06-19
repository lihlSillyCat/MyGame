local skynet = require "skynet"
local gateserver = require "snax.gateserver"

local handler = {}

--连接池
local connection = {}

--新的客户端连接
function handler.connect(fd, ipaddr)	
	print("a new client connected. fd["..fd.."], ip["..ipaddr.."]")

	local client_info = {fd, ipaddr}
	connection[fd] = client_info

	gateserver.openclient(fd)
end

function handler.disconnect(fd)
	print("a client disconnected. fd["..fd.."], ip["..connection[fd].ipaddr.."]")
	
	connection[fd] = nil
end

function handler.error(fd, msg)
	print("a client error then disconnected. fd["..fd.."], msg["..msg.."]")
	
	connection[fd] = nil
end

function handler.command(cmd, source, ...)
	print("recv cmd:"..cmd.."|source:"..source)

end

-- 网络消息处理
function handler.message(fd, msg, sz)
	print("recv net msg, fd["..fd.."]msg["..msg"]sz["..sz.."]")

end

local function userlogin(fd, name, password)
	print("userlogin name["..name.."]password["..password.."]")

	--登陆成功，让玩家去登陆网关，并断开连接
	local token = "a token use in gate"
	

	gateserver.closeclient(fd)
end 

