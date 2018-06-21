local skynet = require "skynet"


skynet.start(function()
	print("launch server...")

	local login = skynet.newservice("logins")
	local gate = skynet.newservice("gates")

	skynet.send(login, "lua", "start")
	skynet.send(gate, "lua", "start")

end)
