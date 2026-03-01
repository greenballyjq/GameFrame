# TinyServer 协议与客户端接入指南

本文档面向客户端开发同学，目标是让你可以按步骤完成从“拉服”到“全业务通信”的接入。

## 1. 协议文件总览

`proto` 目录包含以下协议：

- `auth.proto`：认证域（登录、发验证码、选账号）。
- `user.proto`：用户域（资料、心跳、绑定手机、资产、登出）。
- `social.proto`：社交域（签到与签到信息）。
- `payment.proto`：支付域（商城、下单、订单列表）。
- `game.proto`：游戏域（基础数据、自定义数据、进度、结算、配置）。
- `opcode.proto`：统一错误码。

> 服务列表返回体使用 `etcd.ServerList`，通常由独立 serverlist 服务提供。

## 2. 通信规范（必须遵守）

### 2.1 传输协议

- 使用 HTTPS + Protobuf（二进制）短连接请求。
- 请求方法：`POST`。

### 2.2 根消息约定

每个业务域都使用根消息：

- 认证域：`C2SAuth` / `S2CAuth`
- 用户域：`C2SUser` / `S2CUser`
- 社交域：`C2SSocial` / `S2CSocial`
- 支付域：`C2SPayment` / `S2CPayment`
- 游戏域：`C2SGame` / `S2CGame`

根消息中统一包含：

- `session`：客户端请求流水号（建议从 1 开始单调递增）。
- `oneof msg`：本次请求的具体子消息。

服务端会在响应中回传同一个 `session`，用于请求/响应对齐。

### 2.3 认证头

- 登录成功后，从 `S2CAuth.login.token` 获取会话令牌。
- 后续请求（`/user` `/social` `/payment` `/game`）都需带请求头：

```text
Authorization: <token>
```

### 2.4 错误码

- 每个根响应都有 `code` 字段，类型为 `S2COpcode`。
- `OP_SUCCESS` 表示成功。
- 非成功时，通常仅处理 `code` 即可，不要依赖对应子消息一定存在。

## 3. Step-by-step 接入流程

以下流程按真实接入顺序给出，建议严格照此落地。

### Step 1：拉取服务器列表

1. 请求 serverlist 地址（例如：`https://dev.sanqianpan.com:9999/serverlist`）。
2. 解析 `etcd.ServerList`。
3. 选定一个目标服务器，读取：
	- `login_address`：用于 `/auth`
	- `address`：用于 `/user` `/social` `/payment` `/game`

### Step 2：发送短信验证码（短信登录场景）

1. 向 `/auth` 发送 `C2SAuth`，`msg=send_sms`。
2. 子消息使用 `C2SSendSMS`：
	- `phone`：手机号
	- `usage`：验证码用途（推荐 `login`）
3. 收到 `S2CAuth.send_sms` 后检查：
	- 根 `code == OP_SUCCESS`
	- `send_sms.success == true`

### Step 3：登录并拿 token

1. 向 `/auth` 发送 `C2SAuth`，`msg=login`。
2. 子消息使用 `C2SLogin`：
	- 在 `platform` oneof 中按实际平台填充（如 `sms`）
	- `game_id` 填目标游戏 ID（会写入服务端会话）
3. 收到 `S2CAuth.login` 后保存：
	- `token`
	- `uuid`

### Step 4：进入业务域通信

使用 `address` 作为 host，并带 `Authorization` 请求头。

#### 4.1 用户域 `/user`

常用顺序：

1. `get_profile`
2. `get_assets`
3. `heartbeat`（建议定时）

#### 4.2 游戏域 `/game`

建议顺序：

1. `base_data`
2. `custom_data`
3. `progress_data`
4. `wave_result`（上报本次 `point_gain/threekp_gain/win`）
5. 再次 `progress_data`（校验累计结果）
6. `battle_history`（查询最近 10 场战绩）
7. `gold_gain`（上报金币获得，携带 `gold_gain/source_type/source_id`）
8. `configuration`（按配置名拉取）

> `wave_result` 现在多了一个 `win` 字段（bool），客户端需上报战斗胜负。每次结算成功后服务端自动按 `uuid+game_id` 写入一条战绩，超过 10 条自动裁剪。`battle_history` 为空请求，响应 `S2CBattleHistory.records` 按时间倒序返回最近 10 条。

#### 4.3 支付域 `/payment`

建议顺序：

1. `mall`：拉商品
2. `order_list`：拉历史订单
3. `buy`：下单（`item_id/count/phone/address`）
4. 再次 `order_list`：确认订单变化

> 注意：商城可见商品与可购买性由登录会话中的 `game_id` 决定，客户端无需在 `mall` 请求里传游戏类型。

#### 4.4 社交域 `/social`

建议顺序：

1. `get_sign_in_info`
2. `sign_in`
3. 再次 `get_sign_in_info`

### Step 5：登出

向 `/user` 发送 `logout`，成功后清理本地 token 与会话态。

## 4. 客户端实现建议

### 4.1 SDK 封装建议

建议封装一个统一发送函数：

1. 组装根消息（填 `session` + 子消息）。
2. Protobuf 编码。
3. 发 HTTP POST（可选加 `Authorization`）。
4. 解码根响应。
5. 先判断 `code`，再处理子消息。

### 4.2 重试策略

- 网络错误可做有限次重试（建议指数退避）。
- `OP_SESSION_EXPIRED` / `OP_SESSION_TOKEN_INVALID`：触发重新登录。
- 业务错误码（如资产不足）不应盲目重试。

### 4.3 日志建议

联调阶段建议打印：

- 请求域名与路径
- 请求 `session`
- 根响应 `code`
- 子消息关键字段（如 token、uuid、order_id）

## 5. 联调验收清单（建议）

- [ ] 能成功拉到 serverlist 并解析出 `login_address/address`
- [ ] `/auth send_sms` 成功
- [ ] `/auth login` 成功并拿到 token
- [ ] `/user get_assets` 成功返回 threekp
- [ ] `/game wave_result` 后 `progress_data.point` 正常累加
- [ ] `/game battle_history` 返回最近战绩且不超过 10 条
- [ ] `/payment buy` 成功并可在 `order_list` 看到新订单
- [ ] `/social sign_in` 行为符合预期（已签到当天会失败）
- [ ] `/user logout` 成功

## 6. 版本维护约定

- 修改字段语义时，同步更新对应 `.proto` 注释与本文档。
- 新增业务消息时，补充：
  - 请求/响应字段说明
  - 推荐调用时序
  - 失败码与处理建议
