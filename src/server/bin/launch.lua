local skynet = require "skynet"


skynet.start(function()
	print("launch server...")
	
	local gate = skynet.newservice("gates")

	skynet.call(gate, "lua", "open", {
	    address = "192.168.2.170", -- 监听地址 127.0.0.1
		port = 2048,    -- 监听端口 8888
		maxclient = 1024,   -- 最多允许 1024 个外部连接同时建立
		nodelay = true,     -- 给外部连接设置  TCP_NODELAY 属性
	})

    local game = skynet.newservice("game", gate)
    --local test = skynet.newservice("test")

end)
