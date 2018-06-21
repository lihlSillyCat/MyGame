local skynet = require "skynet"
local socket = require "skynet.socketdriver"

local CMD = {}

function CMD.shakehands(name)
	print("logins server recv shakehands["..name.."]")
end

function CMD.start( ... )
	print("logins server recv start")
	
end


skynet.start(function()
	print("hello, this logins!")

	skynet.dispatch("lua", function(session, source, cmd, ...)	
		print(skynet.self()..":recv server msg==>>session["..session.."]source["..source.."]cmd["..cmd.."]")
		local f = assert(CMD[cmd])
		f(...)
	end)

	local id = socket.listen("127.0.0.1", 12345)
	socket.start(id, function(id, addr)
		print("connect from "..addr.." "..id)
		skynet.start(id)
		newclient(id, addr)
	end)

end)


function newclient(id, addr)
	while true do
		local str = socket.read(id)
		if str then
			print(str)
			socket.write(id, "give you "..str)

			if str == "bye" then
				socket.close(id)
				return
			end
		else
			socket.close(id)
			return
		end
	end
end
