# GameFrame 游戏开发入门指南 (小白专用版)

## 这是什么？

- **GameFrame**: 这是一个游戏开发框架，就像游戏的"发动机"
- **游戏项目**: 使用这个"发动机"开发的具体游戏

## 为什么要用这个框架？

想象一下：
- 你有10辆不同的车，但它们用的都是同一种发动机
- 如果你升级了发动机，所有车都能变得更好
- 你不需要为每辆车单独制造一个发动机

## 开始前准备

你需要：
- 一台电脑 (Windows/Mac/Linux都可以)
- 安装好Git (不知道怎么装？[点击这里学习安装Git](https://git-scm.com/book/zh/v2/起步-安装-Git))
- 安装好Unity (我们的游戏开发软件)

## 小白三步走

### 第一步：获取代码

1. 在你的电脑上创建一个名为`workspace`的文件夹
   - Windows用户：在C盘根目录创建 (C:\workspace)
   - Mac/Linux用户：在你的用户目录创建 (~/workspace)

2. 下载项目代码 (项目地址)
   - GameFrame框架: http://182.254.155.12:5080/game/GameFrame.git
   - 游戏项目（抓大鹅）: http://182.254.155.12:5080/game/GooseClient.git

### 第二步：连接框架和游戏

这一步很重要！我们要让游戏能使用框架，通过创建一个"快捷方式"。连接脚本就在GameFrame框架目录中。

#### Windows用户：

1. 进入你下载的GameFrame框架文件夹
2. 在此文件夹中找到`setup-symlink.ps1`文件
3. 右键点击此文件，选择"以管理员身份运行PowerShell脚本"
   (如果没有这个选项，可以右键点击开始菜单，选择"Windows PowerShell (管理员)"，然后拖放脚本文件到PowerShell窗口中)
4. 按照脚本提示输入游戏项目目录路径（不需要精确到Assets文件夹）

#### Mac/Linux用户：

1. 打开你下载的GameFrame框架文件夹
2. 找到`setup-symlink.sh`文件
3. 右键点击此文件，选择"获取信息"
4. 在权限选项卡中，勾选"允许作为程序执行"
5. 打开终端，拖放脚本到终端窗口中运行
6. 按照脚本提示输入游戏项目目录路径（不需要精确到Assets文件夹）

> **智能升级**: 现在脚本更加智能了！只需要输入游戏项目的根目录，脚本会自动查找Assets文件夹并创建软链接。

#### 目录智能检测

脚本会自动在以下位置查找Assets文件夹：
- 项目根目录下的`Assets`文件夹
- 项目根目录下的`client/Assets`文件夹
- 项目根目录下任何一级子目录中的`Assets`文件夹

例如，只需输入游戏项目根目录路径：
```
# Windows示例:
.\setup-symlink.ps1 -ClientPath ..\goosecatcher

# Mac/Linux示例:
./setup-symlink.sh -c ../goosecatcher
```

> 重要提示：框架路径会自动设置，你只需提供游戏项目的根目录，脚本会自动找到Assets文件夹。

### 第三步：在Unity中打开游戏

1. 打开Unity Hub
2. 点击"添加"按钮
3. 找到并选择你的游戏项目文件夹：
   - Windows: `C:\workspace\goosecatcher\GooseClient\client`
   - Mac/Linux: `~/workspace/goosecatcher/GooseClient/client`
4. 点击"打开"
5. 使用推荐的Unity版本打开项目

## 常见问题

### 问：我在Windows上无法运行脚本
**答**：右键点击开始菜单，选择"Windows PowerShell (管理员)"，然后将脚本文件拖入窗口中运行。

### 问：我在Mac上无法运行脚本
**答**：
1. 打开"终端"应用
2. 输入 `chmod +x `（注意空格）
3. 然后将脚本文件拖入终端窗口
4. 按回车键
5. 然后将脚本文件再次拖入终端窗口运行

### 问：我不知道要输入什么路径
**答**：
如果你按照上面的步骤创建了workspace文件夹并下载了项目，那么可以使用上面提供的示例命令。脚本会自动处理相对路径。

### 问：Unity无法找到GameFramework的资源
**答**：
1. 关闭Unity
2. 删除游戏项目中的`Library`文件夹
3. 重新打开项目

### 问：我修改了框架代码，但游戏中没有更新
**答**：
1. 确保连接脚本成功运行了
2. 在Unity中，按Ctrl+R (Windows) 或 Cmd+R (Mac) 刷新资源

## 恭喜你！

你已经成功设置了GameFrame框架和游戏项目。现在你可以：
1. 修改框架代码，所有使用框架的游戏都会自动更新
2. 开发新的游戏功能
3. 愉快地玩耍和开发！

记住：框架是工具，游戏是作品。祝你创作顺利！ 