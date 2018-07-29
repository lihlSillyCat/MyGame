local sprotoparser = require "sprotoparser"

--login msg
local loginmsg = {}

--msg client to server
loginmsg.c2s = sprotoparser.parse [[
.login {
	type        0 : integer    # 登陆方式
	account     1 : string	   # 玩家账户
	passwd      2 : string     # 玩家密码
	cversion    3 : string     # 客户端版本				
}
]]

--msg server to client
loginmsg.s2c = sprotoparser.parse [[
.login_ok {
	uid        0 : integer     # 玩家ID
	token      1 : string      # 访问令牌
}

.login_error {
	code       0 : integer     # 错误码	
	des        1 : string      # 描述	
}
]]

return loginmsg



