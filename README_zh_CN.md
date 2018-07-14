# 废萌（Facemoji）

**一个可以模仿你的表情的语音聊天机器人。**

这是一个 **Unity** 项目（暂时只适配 Android），有两个模块：

* 模块一是**实时人脸卡通化（FaceTracking）**，使用 [OpenCV](https://enoxsoftware.com/opencvforunity/)和 [Dlib](https://enoxsoftware.com/dlibfacelandmarkdetector/) 检测面部表情，并实时转化为 [Live2D](http://sites.cybernoids.jp/cubism-sdk2_e/unity_2-1) 模型，然后可 [录制 gif 图](https://github.com/Chman/Moments)；
* 模块二是 **AI 人工智能** ~~（聊天机器人）~~ 使用 [图灵机器人](https://github.com/huihut/TuringRobot) 、[讯飞 IAT 语音听写](http://www.xfyun.cn/services/voicedictation)、 [讯飞 TTS 语音合成](http://www.xfyun.cn/services/online_tts) 进行语音聊天。

[English](README.md) | 简体中文

## 下载

* [酷安 . Facemoji 废萌](https://www.coolapk.com/apk/192260)
* [Google Play . Facemoji 废萌](https://play.google.com/store/apps/details?id=com.huihut.facemoji)
* [Github . Facemoji/releases](https://github.com/huihut/Facemoji/releases)
* [Drive.Google . Facemoji/Platform](https://drive.google.com/open?id=1ofJMFIdzXCdYYO3qO5hvrTQPJUumgSY-)
* [Pan.Baidu . Facemoji/Platform](https://pan.baidu.com/s/1U08B_wPY67Zh1RTwFhrihA)

## 制作

1. 从 [Drive.Google](https://drive.google.com/open?id=1ofJMFIdzXCdYYO3qO5hvrTQPJUumgSY-) 或者 [Pan.Baidu](https://pan.baidu.com/s/1U08B_wPY67Zh1RTwFhrihA) 下载 `shape_predictor_68_face_landmarks.dat`（人脸特征点检测器）和 `Facemoji_Plugins_Assets_1.5.0.unitypackage` （精简的 OpenCV, Dlib, Live2D 和 Iflytek 库） 
2. `git clone https://github.com/huihut/Facemoji.git`
2. 创建一个新的 Unity 项目，命名为 `FacemojiDemo`
3. 复制 `Facemoji/Assets` 和 `Facemoji/ProjectSettings` 到你的项目 (`FacemojiDemo/`)
4. 把 `shape_predictor_68_face_landmarks.dat` 复制到 `FacemojiDemo/Assets/StreamingAssets/`
5. 导入 `Facemoji_Plugins_Assets_1.5.0.unitypackage`
6. 转换平台到 Android（其他平台未适配）
7. Build & Run

## 使用

### 实时人脸卡通化（FaceTracking）

使用 **OpenCV** 和 **Dlib** 检测面部表情，并实时转化为 **Live2D** 模型；

她可以跟着你的头部表情动，试着摇头看看吧。

### 录制 gif 图

点击顶部中间的录制键可以录制 3 秒的 gif；

录制状态为：Recording（准备录制）、PreProcessing（正在录制）、Paused（正在压缩成 gif 图）

生成的 gif 存储在 `Application.dataPath`，Android 平台的话在 `/storage/emulated/0/Android/data/com.huihut.facemoji/files/`

### 语音和文字聊天（~~聊天机器人？~~ 她说她是AI，不是机器人！ hhhh...）

使用 **图灵机器人**、**讯飞 IAT 语音听写**、**讯飞 TTS 语音合成**

她很智能（~~zhizhang~~），可以：

* 聊天对话
* 生活百科
* 数学计算
* 故事大全
* 笑话大全
* 成语接龙
* 星座运势
* 天气查询
* ...

但是由于她是个中国 AI（~~机器人~~），图灵机器人只支持中文，所以她只能进行中文聊天，和她讲英文她只会翻译。

不过她以后会学习英文的（~~换个会讲英文的~~）。

## 预览

![](Images/Capture_Facemoji.png)

## Gif

* 加油！
    
    ![](Images/GifCapture-ComeOn.gif)

* 不要~ 不要~
    
    ![](Images/GifCapture-NoNo.gif)

* 放电！
    
    ![](Images/GifCapture-Spark.gif)

## License

[GPL v3.0](https://github.com/huihut/Facemoji/blob/master/LICENSE)