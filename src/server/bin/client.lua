package.cpath = "luaclib/?.so"

local socket = require "client.socket"

if _VERSION ~= "Lua 5.3" then
	error "Use lua 5.3"
end

local fd = assert(socket.connect("127.0.0.1", 12345))

socket.send(fd, "hello")

while true do
	local str = socket.recv(fd)
	if str == "give you bye" then
		socket.close(fd)
		break
	end

	print(str)

	local readstr = socket.readstdin()
	socket.send(fd, readstr)
end


