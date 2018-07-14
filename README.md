# Facemoji

**A voice chatbot that can imitate your expression.**

This is a **Unity** project (just for Android now), which has two modules.

* One is **FaceTracking**, which using [OpenCV](https://enoxsoftware.com/opencvforunity/) and [Dlib](https://enoxsoftware.com/dlibfacelandmarkdetector/) to detects facial expressions, converts them into [Live2D](http://sites.cybernoids.jp/cubism-sdk2_e/unity_2-1) model, and [record](https://github.com/Chman/Moments) gif. 
* The other is **AI** ~~(chatbot)~~, which uses [Turing Robot](https://github.com/huihut/TuringRobot), [Iflytek IAT](http://www.xfyun.cn/services/voicedictation) and [Iflytek TTS](http://www.xfyun.cn/services/online_tts) to make a voice chat.

English | [简体中文](README_zh_CN.md)

## Releases

* [Google Play . Facemoji 废萌](https://play.google.com/store/apps/details?id=com.huihut.facemoji)
* [酷安 . Facemoji 废萌](https://www.coolapk.com/apk/192260)
* [Github . Facemoji/releases](https://github.com/huihut/Facemoji/releases)
* [Drive.Google . Facemoji/Platform](https://drive.google.com/open?id=1ofJMFIdzXCdYYO3qO5hvrTQPJUumgSY-)
* [Pan.Baidu . Facemoji/Platform](https://pan.baidu.com/s/1U08B_wPY67Zh1RTwFhrihA)

## Setup

* Download `shape_predictor_68_face_landmarks.dat`(Facial Landmark Detector) and `Facemoji_Plugins_Assets_1.5.0.unitypackage`(Streamlined OpenCV, Dlib, Live2D and Iflytek Assets Library) from [Drive.Google](https://drive.google.com/open?id=1ofJMFIdzXCdYYO3qO5hvrTQPJUumgSY-) or [Pan.Baidu](https://pan.baidu.com/s/1U08B_wPY67Zh1RTwFhrihA)
* `git clone https://github.com/huihut/Facemoji.git`
* Create new Unity project (called `FacemojiDemo`)
* Copy `Facemoji/Assets` and `Facemoji/ProjectSettings` to your unity project (`FacemojiDemo/`)
* Copy `shape_predictor_68_face_landmarks.dat` to your `FacemojiDemo/Assets/StreamingAssets/`
* Import `Facemoji_Plugins_Assets_1.5.0.unitypackage`
* Select Android platform
* Build & Run

## Usage

### FaceTracking

Using **OpenCV** and **Dlib** to detects facial expressions, converts them into **Live2D** model.

She can move with your face and you can try shaking your head.

### Record gif

The middle of the above is the record button, you can record 3 seconds **gif**.

Recorder State : **Recording**(Ready to record) -> **PreProcessing**(Is recording) -> **Paused**(Compressing gif) -> **Recording**(Ready to record)

Save the gif in `Application.dataPath`

(Android in `/storage/emulated/0/Android/data/com.huihut.facemoji/files/`)

### Voice and text chat (~~chatbot?~~ She said she is AI, not Robot! hhhh...)

Using **Turing Robot**, **Iflytek IAT** and **Iflytek TTS**.

You can chat with her by voice or text.

She's a great AI ~~(robot)~~, and she can:

* Chat
* Encyclopedia
* Calculate
* Tell a story
* Tell a joke
* Idiom Solitaire
* Horoscope
* Weather forecast
* ...

But because she is a Chinese robot(**Turing robot only supports Chinese**), she can **only chat in Chinese**.

~~She will chat in English later.~~

## Preview

![](Images/Capture_Facemoji.png)

## Gif

* Come On !
    
    ![](Images/GifCapture-ComeOn.gif)

* No~ No~
    
    ![](Images/GifCapture-NoNo.gif)

* Wink !
    
    ![](Images/GifCapture-Spark.gif)

## License

[GPL v3.0](https://github.com/huihut/Facemoji/blob/master/LICENSE)