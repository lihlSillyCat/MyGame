local skynet = require "skynet"

--[[
skynet.register_protocol {
	name = "client",
	id = skynet.PTYPE_CLIENT,
	--unpack = skynet.tostring,
}
]]

local gate
local uid
local socket_fd
local CMD = {}

function CMD.start(info)
	gate = info.gate
    socket_fd = info.fd
    uid = info.uid

    skynet.error("agent start, uid["..uid.."]")
end

function CMD.logout()
	skynet.error(string.format("%s is logout", uid))
	
	skynet.exit()
end

skynet.start(function()
	skynet.dispatch("lua", function(session, source, command, ...)
		local f = assert(CMD[command])
		skynet.ret(skynet.pack(f(...)))
	end)

end)
