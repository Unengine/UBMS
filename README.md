# UBMS (Unity-BMS)

Unity BMS player. Supports modern bmses.


[Download currently executable binary](https://github.com/Unengine/UBMS/releases/tag/v1.3.2-alpha)

[Quicktime](https://support.apple.com/kb/DL837?locale=ko_KR) should be installed to play video bga.

Thanks to sound piercer for providing [Main BGM](https://www.youtube.com/watch?v=EmcTqpdJ1gs)!

선곡창 BGM 제공해주신 sound piercer님 정말 감사합니다!

## What is BMS?
A format for rhythm games similiar to [Beatmania IIDX](https://en.wikipedia.org/wiki/Beatmania_IIDX). (Acutally is almost the same!)

But the difference is, it uses user's songs (or datas)! Some musicians compose songs for bms and enter their songs in [BOF](http://www.bmsoffighters.net/) (Bms of Fighters, a contest for bms)

## Supported File Extensions

BMS Data File : .bms, .bme, .bml

BGA (Background Animation) : MP4, MPEG (partially not supported), bmp, png

Audio Files : .ogg, .wav, .mp3 (not supported on Windows)

## System Requirement

Minimum : Windows 7 SP1, DirectX 10, SSE2 supporting CPU

[Quicktime](https://support.apple.com/kb/DL837?locale=ko_KR) should be installed to play video bga.

## BMS Supports

Only SP available now (1P Side)

Landmines (does not explode), Loading/Saving Records, Chaning Scroll Speed, Autoplay, Auto-Scratch, Directory System

### Supported Commands

HEADERS


#TOTAL #STAGEFILE (but not used), #BANNER, #BACKBMP, #PLAYLEVEL, #TITLE, #SUBTITLE, #ARTIST, #GENRE, #WAVxx, #STOPxx, #LNTYPE 1, #LNOBJ, #BMPxx

CONTROL FLOWS

#RANDOM, #IF, #ENDIF, #ENDRANDOM

CHANNELS

01, 02, 03, 04, 08, 09, 11-19, 51-59, D1-D9, E1-E9 

## TODO Priority

· Judgeline adjusting

· 2P/DP Play

· Gauges (Groove, Hard Survival, etc ..)

