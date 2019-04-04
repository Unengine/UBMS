# UBMS (Unity-BMS)

Unity BMS player. Supports modern bmses.


## What is BMS?
A format for rhythm games similiar to [Beatmania IIDX](https://en.wikipedia.org/wiki/Beatmania_IIDX). (Acutally is almost the same!)

But the difference is, it uses user's songs (or datas)! Some musicians compose songs for bms and enter their songs in [BOF](http://www.bmsoffighters.net/) (Bms of Fighters, a contest for bms)

## Supported File Extensions

BMS Data File : .bms, .bme, .bml

BGA (Background Animation) : MP4, MPEG (partially not supported), bmp, png

Audio Files : .ogg, .wav, .mp3 (not supported in Windows)

## System Requirement

Minimum : Windows 7 SP1, DirectX 10, SSE2 supporting CPU

## BMS Supports

Landmines (does not explode), Loading/Saving Records, Chaning Scroll Speed, Autoplay, Auto-Scratch, Directory System

### Supported Commands

HEADERS


#TOTAL #STAGEFILE (but not used), #BANNER, #BACKBMP, #PLAYLEVEL, #TITLE, #SUBTITLE, #ARTIST, #GENRE, #WAVxx, #STOPxx, #LNTYPE 1, #LNOBJ, #BMPxx

CONTROL FLOWS

#RANDOM, #IF, #ENDIF, #ENDRANDOM

CHANNELS

01, 02, 03, 04, 08, 09, 11-19, 51-59, D1-D9, E1-E9 