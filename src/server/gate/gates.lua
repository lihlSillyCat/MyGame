local skynet = require "skynet"

local CMD = {}

function CMD.shakehands(name)
	print("gates server recv shakehands["..name.."]")
end

function CMD.start( ... )
	print("gates server recv start")
	
end


skynet.start(function()
	print("hello, this gates!")

	skynet.dispatch("lua", function(session, source, cmd, ...)	
		print(skynet.self()..":recv server msg==>>session["..session.."]source["..source.."]cmd["..cmd.."]")
		local f = assert(CMD[cmd])
		f(...)
	end)


end)
