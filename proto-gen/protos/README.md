# 小游戏网络通信说明文档

## 主要协议文件

### etcd.proto
> 服务器列表协议, 用于获取可登录的游戏服务器信息。

### message.proto
> 定义了客户端和服务器上行/下行的协议结构
* C2S_Message 表示上行, 即客户端到服务器的协议。分为请求数据和执行操作。
* S2C_Message 表示下行, 即服务器到客户端的协议。分为下发数据和反馈结果。
* session 字段是一个透传参数, 即服务器不会改变它，原样返回。但客户端需保证每个请求都必须不一样。
* 游戏内的任何协议都不能直接发送，必须嵌入 msg 字段中才可以通信。

### login.proto
> 登陆协议, 是客户端和游戏服务器通信的第一条协议。

### opcode.proto
> 错误码协议，用于反馈客户端操作结果。

### ping.proto
> 心跳包协议，正常登录戏后，客户端应定时发送心跳包给务器,服务器超过一定时间未收到客户端的心跳包，则认为客户端离线，后续通信需重新登录。

### base.proto
> 玩家基础信息协议

### item.proto
> 物品协议, 一般会把金币，体力等资源类也定义为具有固定id的物品。

### custom.proto
> 客户端自定义数据协议, 一般服务器不关心，仅仅做存储，所以上传数据设定为序列化后的binary数据。

### record.proto
> 服务器使用，通信过程不涉及。

## 通信方式
> 小游戏由于交互少的特点，我们采取短连接的方式来作为游戏主要通信方式，即使用 http 通信协议。

## 客户端和服务器完整交互过程

### 1. 获取服务器列表
开发服接口: https://dev.sanqianpan.com:9999/serverlist
返回 etcd.ServerList 消息结构，解析获得想登录的游戏服务器地址信息。

### 2. 登陆游戏
登陆地址: etcd.Server 的 login_address 字段, 步骤1获得。
发送登陆协议到登陆地址，成功会收到一个 token 用于后续通信，失败则会收到相应的 opcode 反馈。

### 3. 正常游戏
游戏地址: etcd.Server 的 address 字段, 步骤1获得。
通信令牌: S2C_Login 的 token 字段，步骤2获得。
将通信令牌放在 header 的 "Authorization" 中, 后续每个通信都要携带令牌。

## 游戏内已实现的协议（持续更新）
- [x] etcd.proto
- [x] message.proto
- [x] opcode.proto
- [x] base.proto
- [x] ping.proto
- [x] custom.proto
- [x] login.proto
- [x] currency.proto
- [x] wave.proto
- [ ] item.proto(需道具表)
- [x] mall.proto

## 注意
> 上面的协议和通信过程仅限于目前开发阶段，后续根据业务需求可能会相应调整。
