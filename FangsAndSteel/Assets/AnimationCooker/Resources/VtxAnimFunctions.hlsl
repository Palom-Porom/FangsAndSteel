// This is a collection of functions for decoding different bit depths stuffed into rgba buffers,
// and for accessing pixels in the position and normal textures.
//
//================================= UNWRAPPING PIXEL COORDS ======================================
//frameIndex = (begin(c) + f)
//index = ((frameIndex * vertexCount) + v) + width
//
//// slow way
//y = floor(index / width);
//x = index % width; // worst
//x = index - (width * y); // best
//
//// fast way (works because width is a power of two)
//y = index >> pow;
//x = (index & (width - 1)); // fast method 1
//x = index - (width * y); // fast method 2
//---------------------- EXAMPLE ------------------------------------------------------------------
//vertexCount 10
//clip0 - 2 frames, index 0..1
//clip1 - 3 frames, index 2..4
//frame count : 5
//width : 8
//pow : 3
//pix count : 8x8-- > 64
//total: 5 * 10 -- > 50
//
//X0             X1            X2             X3             X4             X5             X6             X7
//Y0 P00....H1....P01....H2....P02....C0....P03....C1....P04..........P05..........P06..........P07..........
//Y1 P08I00C0F0V00  P09I01C0F0V01  P10I02C0F0V02  P11I03C0F0V03  P12I04C0F0V04  P13I05C0F0V05  P14I06C0F0V06  P15I07C0F0V07
//Y2 P16I08C0F0V08  P17I09C0F0V09  P18I10C0F1V00  P19I11C0F1V01  P20I12C0F1V02  P21I13C0F1V03  P22I14C0F1V04  P23I15C0F1V05
//Y3 P24I16C0F1V06  P25I17C0F1V07  P26I18C0F1V08  P27I19C0F1V09  P28I20C1F0V00  P29I21C1F0V01  P30I22C1F0V02  P31I23C1F0V03
//Y4 P32I24C1F0V04  P33I25C1F0V05  P34I26C1F0V06  P35I27C1F0V07  P36I28C1F0V08  P37I29C1F0V09  P38I30C1F1V00  P39I31C1F1V01
//Y5 P40I32C1F1V02  P41I33C1F1V03  P42I34C1F1V04  P43I35C1F1V05  P44I36C1F1V06  P45I37C1F1V07  P46I38C1F1V08  P47I39C1F1V09
//Y6 P48I40C1F2V00  P49I41C1F2V01  P50I42C1F2V02  P51I43C1F2V03  P52I44C1F2V04  P53I45C1F2V05  P54I46C1F2V06  P55I47C1F2V07
//Y7 P56I48C1F2V08  P57I49C1F2V09  P58            P59            P60            P61            P62            P63
//
//Key
//P : Pixel - the cumulative pixel index(does not include header)
//I : Index - the cumulative vertex index(includes header)
//C : clip number
//F : frame index(global for all clips, starting at clip0, frame0)
//V : Vertex number
//P57I49C1F2V09-------------------- - C1, F2, V9 (1, 7)--------------------------------------------
//frameIndex = (2 + 2)-- > 4
//index = (4 * 10) + 9 -- > 49
//pixIndex = 49 + 8 -- > 57
//
//y = floor(57 / 8)-- > 7
//x = 57 % 8 -- > 1
//x = 57 - (8 * 7)-- > 1 // without modulus
//
//// fast way
//y = (57 >> 3)-- > 7
//x = (57 & (8 - 1))-- > 1
//P30I22C1F0V02----------------------C1, F0, V2 (6, 3)---------------------------------------------
//frameIndex = (2 + 0)-- > 2
//index = ((2 * 10) + 2) + 8 -- > 30
//
//// slow way
//y = floor(30 / 8)-- > 3
//x = 30 % 8 -- > 6
//x = 30 - (8 * 3)-- > 6
//
//// fast way
//y = (30 >> 3)-- > 3
//x = (30 & (8 - 1))-- > 6
//x = 30 - (8 * 3)-- > 6
//=================================================================================================

#pragma once

#define bitDec float4(1.0, 255, 65025, 16581375)

#define div11mul 2097151 // uint-max / 2048, where uint-max is 4294967295
#define div10mul 4194303 // uint-max / 1024, where uint-max is 4294967295
#define div11mulInv 0.00000047683738557690886350100684213965 // 1 / div11mul
#define div10mulInv 0.00000023841863594499491333840211353352 // 1 / div10mul

// this is a way to divide by 2048, but giving a floating point result
#define FastDivBy2048(num) ((num * div11mul) >> 11) * div11mulInv

// this is a way to divide by 1024, but giving a floating point result
#define FastDivBy1024(num) ((num * div10mul) >> 10) * div10mulInv

struct PixelInfo
{
    float3 position;
    float3 normal;
    float4 tangent;
};

// this is a replacement for the old 'UnityObjectToWorldNormal()'
#define ObjectToWorldNormal(normal) mul(GetObjectToWorldMatrix(), float4(normal, 1))

// this is a replacement for the old 'UnityObjectToClipPos()'
#define ObjectToClipPos(pos) mul(UNITY_MATRIX_VP, mul(UNITY_MATRIX_M, float4(pos.x, pos.y, pos.z, 1)))
//#define ObjectToClipPos(pos) mul(UNITY_MATRIX_VP, mul(UNITY_MATRIX_M, pos))

#define ScaleUnitIntervalToUnitRange(val) (val * 2) - 1
#define ScaleToUnitRange(val, oldMax) ((val / oldMax) * 2) - 1
#define Scale(val, oldMin, oldMax, newMin, newMax) (((val - oldMin) / (oldMax - oldMin)) * (newMax - newMin)) + newMin

uint UnpackRGBAToUint(float4 v)
{
    //return ((uint)(v.x * 255) << 24) | ((uint)(v.y * 255) << 16) | ((uint)(v.z * 255) << 8) | ((uint)(v.w * 255));
    return (((uint)round(v.x * 255)) << 24) | (((uint)round(v.y * 255)) << 16) | (((uint)round(v.z * 255)) << 8) | ((uint)round(v.w * 255));
}

// unpack RGBA vector (where values are normalized 0 to 1)
// returns the original float
float UnpackRGBAToOne32bitFloat(float4 v, float min, float max)
{
    float unscaled = dot(v, bitDec);
    return (unscaled * (max - min)) + min;
}

// unpack an rgba vector into two 16 bit floats
// the RGBA vector must have component values that are between 0 and 1
float2 UnpackRGBAToTwo16bitFloats(float4 val)
{
    uint input = UnpackRGBAToUint(val);
    return float2(f16tof32(input >> 16), f16tof32(input & 0xFFFF));
}

// given a packed value and the min and max values of val (before they were scaled)
// returns a de-scaled float3 with precision such that z and x are 11 bits, and y is 10 bits.
// 00 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31
// |--------------11 bits (x)-----| |-----------10 bits (y)-----| |----------11 bits (z) --------|
float3 UnpackRGBAToThree10BitFloats(float4 val, float min, float max)
{
    uint packed = UnpackRGBAToUint(val);
    uint x = (packed >> 21); // first 11 bits (0xFFE00000) [0000 0000 0001 1111 1111 1000 0000 0000]
    uint y = (packed & 0x1FF800) >> 11; // middle 10 bits --> 0x1FF800 is 1023(dec) << 11 --> [0000 0000 0001 1111 1111 1000 0000 0000]
    uint z = (packed & 2047); // last 11 bits, 2047 --> [0000 0000 0000 0000 0000 0111 1111 1111]

    float3 unpacked;
    // scaling equation: (((val - oldMin) / (oldMax - oldMin)) * (newMax - newMin)) + newMin;
    // since oldMin is zero, we can eliminate two subtractions
    // this gives: (((val - 0) / (oldMax - 0)) * (newMax - newMin)) + newMin;
    // which is the same as: ((val / oldMax) * (newMax - newMin)) + newMin;
    // we can optimize division since oldMax is 2048 for a 10 bit number and 1024 for a 10 bit number
    unpacked.x = (FastDivBy2048(x) * (max - min)) + min; // 11 bits
    unpacked.y = (FastDivBy1024(y) * (max - min)) + min; // 10 bits
    unpacked.z = (FastDivBy2048(z) * (max - min)) + min; // 11 bits
    return unpacked;
}

// given a packed value and the min/max values of val (before they were scaled)
// returns a de-scaled float4 with 10 bit precisions for x,y,z and w will be -1 or 1
float4 UnpackRGBAToTanget(float4 val, float min, float max)
{
    uint packed = UnpackRGBAToUint(val);
    uint x = (packed >> 22); // first 10 bits
    uint y = (packed & 0x1FFC00) >> 10; // next 10 bits - 0x1FFC00 is (1023 dec << 10)
    uint z = (packed & 0x1FFC00) >> 2; // next 10 bits - 0x1FFC00 is (1023 dec << 10)
    uint w = packed & 4; // last two bits

    float4 unpacked;
    unpacked.x = (FastDivBy1024(x) * (max - min)) + min; // 10 bits
    unpacked.y = (FastDivBy1024(y) * (max - min)) + min; // 10 bits
    unpacked.z = (FastDivBy1024(z) * (max - min)) + min; // 10 bits
    // w is 0 or 1. convert it back to -1 or 1.
    unpacked.w = ScaleUnitIntervalToUnitRange((float)w);
    return unpacked; // the tangent
}

// This macro makes it easier to lookup the specified value of a pixel
// xx is the x coordinate, yy is the y coordinate, and ts is the texel size (such as _PosMap_TexelSize)
// I had to use xx and yy instead of x and y because the macro compiler gets confused with the x and y values in ts.x and ts.y
// for the float passed into tex2Dlod(): x and y are the coordinates, z is the LOD, and w is the offset.
// The return value of LookupPixel is a float4
#define LookupPixel(tex, xx, yy, ts) (tex2Dlod(tex, float4((xx + 0.5) * ts.x, (yy + 0.5) * ts.y, 0, 0)))

// fetches the value at x, y. returns a float4, where each element is a number between 0 and 255
#define Lookup4Bytes(tex, x, y, ts) (LookupPixel(tex, x, y, ts) * 255.0)

#define Lookup8BitFloats(tex, x, y, ts) LookupPixel(tex, x, y, ts)

// fetches the value at x, y. returns a float2 where each element is a 16 bit float
#define Lookup16BitFloats(tex, x, y, ts) UnpackRGBAToTwo16bitFloats(LookupPixel(tex, x, y, ts))

// fetches the value at x, y. returns a single float.
// you need to specify a min and max value that was used to pack it.
#define Lookup32BitFloat(tex, x, y, ts, minVal, maxVal) UnpackRGBAToOne32bitFloat(LookupPixel(tex, x, y, ts), minVal, maxVal)

// fetches the value at x, y and returns a float3.
// x and z will have a precision of 11 bits for x, while y's precision will be 10 bits.
// you must specify a min and max value that was used to pack it
#define Lookup10BitFloats(tex, x, y, ts, min, max) UnpackRGBAToThree10BitFloats(LookupPixel(tex, x, y, ts), min, max)

// fetches teh value at x,y and returns a float4 where x,y,z are 10 bit tangent values and w is -1 or 1 for sign
#define Lookup10BitTangentFloats(tex, x, y, ts, min, max) UnpackRGBAToTanget(LookupPixel(tex, x, y, ts), min, max)

// fetches the value at x,y and returns a uint
#define Lookup32BitUint(tex, x, y, ts) UnpackRGBAToUint(LookupPixel(tex, x, y, ts))

// scales the specified value from [0..1] to [-1..1]
// transformedVal = (((val - oldMin) / (oldMax - oldMin)) * (newMax - newMin)) + newMin;
// transformedVal = (((val - 0) / (1 - 0)) * (1 - -1)) + -1
// transformedVal = (val * 2) - 1
#define ScaleFromUnitIntervalToUnitRange(val) (val * 2.0) - 1.0

#define Lerp10BitPos(hdr, vid, tex, ts) lerp(Lookup10BitVal(tex, vid, hdr.currentClipFrame, ts, hdr.vertexCount, hdr.power, hdr.minPos, hdr.maxPos), Lookup10BitVal(tex, vid, hdr.nextClipFrame, ts, hdr.vertexCount, hdr.power, hdr.minPos, hdr.maxPos), hdr.lerpAmount)
#define Lerp10BitNml(hdr, vid, tex, ts) lerp(Lookup10BitVal(tex, vid, hdr.currentClipFrame, ts, hdr.vertexCount, hdr.power, hdr.minNml, hdr.maxNml), Lookup10BitVal(tex, vid, hdr.nextClipFrame, ts, hdr.vertexCount, hdr.power, hdr.minNml, hdr.maxNml), hdr.lerpAmount)
//#define Lerp10BitUnit(hdr, vid, tex, ts) lerp(Lookup10BitVal(tex, vid, hdr.currentClipFrame, ts, hdr.vertexCount, hdr.power, -1, 1), Lookup10BitVal(tex, vid, hdr.nextClipFrame, ts, hdr.vertexCount, hdr.power, -1, 1), hdr.lerpAmount)
#define Lerp10BitTangent(hdr, vid, tex, ts) lerp(Lookup10BitTangentVal(tex, vid, hdr.currentClipFrame, ts, hdr.vertexCount, hdr.power, hdr.minTan, hdr.maxTan), Lookup10BitTangentVal(tex, vid, hdr.nextClipFrame, ts, hdr.vertexCount, hdr.power, hdr.minTan, hdr.maxTan), hdr.lerpAmount)
//#define Lerp8Bit(hdr, vid, tex, ts) lerp(LookupAndScale8BitVal(tex, vid, hdr.currentClipFrame, ts, hdr.vertexCount, hdr.power), LookupAndScale8BitVal(tex, vid, hdr.nextClipFrame, ts, hdr.vertexCount, hdr.power), hdr.lerpAmount)

// use this to look up a position.
// tex --> the texture to look in
// v --> the vertex index
// f --> the frame number
// ts --> texel-size (_PosMap_TexelSize, _NmlTex_TexelSize, etc)
// vtx --> the vertex count
// minPos, maxPos --> the min/max values used to for packing. these are positions for the bounding box. (second pixel in the header).
float3 Lookup10BitVal(sampler2D tex, uint vtxIdx, int frmIdx, float4 ts, uint vtxCount, uint pow, float minPos, float maxPos)
{
    // ts.z is texture width

    // first fetch the index for this texture (accounts for skipping header)
    // this is basically --> index = ((frameIndex * vertexCount) + v) + width
    uint idx = ((frmIdx * vtxCount) + vtxIdx) + ts.z;

    // next fetch the y coordinate.
    // this is basically --> float y = floor(idx / ts.z);
    // however, to avoid doing a division, we can use a bitshift instead because width is a power of 2 (256, 512, etc).
    float y = idx >> pow;

    // now getting the x coordinate uses the equation --> x = index - (width * y)
    float x = idx - (ts.z * y);

    // with the x and y pixel coordinates, we can fetch the values from the texture
    return Lookup10BitFloats(tex, x, y, ts, minPos, maxPos);
}

float4 Lookup10BitTangentVal(sampler2D tex, uint vtxIdx, int frmIdx, float4 ts, uint vtxCount, uint pow, float min, float max)
{
    uint idx = ((frmIdx * vtxCount) + vtxIdx) + ts.z;
    float y = idx >> pow;
    float x = idx - (ts.z * y);
    return Lookup10BitTangentFloats(tex, x, y, ts, min, max);
}

//float4 Lookup8BitVal(sampler2D tex, uint vtxIdx, int frmIdx, float4 ts, uint vtxCount, uint pow)
//{
//    uint idx = ((frmIdx * vtxCount) + vtxIdx) + ts.z;
//    float y = idx >> pow;
//    float x = idx - (ts.z * y);
//    return Lookup8BitFloats(tex, x, y, ts);
//}

struct HeaderInfo
{
    float minPos;
    float maxPos;
    float minNml;
    float maxNml;
    float minTan;
    float maxTan;
    float beginFrame; // todo - change to uint
    float endFrame; // todo - change to uint
    float vertexCount; // todo - change to uint
    uint power;
    float frameRate; // todo - change to uint
    uint version;
    int currentClipFrame;
    int nextClipFrame;
    float lerpAmount;
};

HeaderInfo ReadHeader(float4 posTexSize, sampler2D posTex, int clipIdx, float curTime, float speedInst, float speedMat)
{
    HeaderInfo hdr;

    //// the very first pixel in the header-line contains some info about the format
    float4 firstPixInfo = Lookup4Bytes(posTex, 0, 0, posTexSize);
    hdr.version = firstPixInfo.x;
    hdr.frameRate = firstPixInfo.y * speedInst * speedMat;
    hdr.power = firstPixInfo.w;

    // the second pixel in the header-line contains min and max position values (bounding box for positions)
    float2 range = Lookup16BitFloats(posTex, 1, 0, posTexSize);
    hdr.minPos = range.x;
    hdr.maxPos = range.y;

    // the third pixel in the header-line contains min and max normal values (bounding box for normals)
    range = Lookup16BitFloats(posTex, 2, 0, posTexSize);
    hdr.minNml = range.x;
    hdr.maxNml = range.y;

    // the fourth pixel in the header-line contains min and max tangent values (bounding box for tangents)
    range = Lookup16BitFloats(posTex, 3, 0, posTexSize);
    hdr.minTan = range.x;
    hdr.maxTan = range.y;

    // the fifth pixel in the header-line contains the vertex count, which is needed for finding the index of each pixel
    hdr.vertexCount = Lookup32BitUint(posTex, 4, 0, posTexSize);

    // The remaining pixels in the header-line each correspond to an animation clip,
    // and they tell us the begin and end frame for each clip.
    // In this case, we're interested in the clip specified by _ClipIdx.
    // We add 3 in order to skip over the first three pixels.
    float2 frameInfo = Lookup16BitFloats(posTex, clipIdx + 5, 0, posTexSize);
    hdr.beginFrame = frameInfo.x;
    hdr.endFrame = frameInfo.y;

    // _CurTime should be a number between 0 and clip.length
    // to convert from time to frame, multiply time (seconds) by frame-rate (frames-per-second)
    // here we convert that to a frame, which will range beginFrame to endFrame
    hdr.currentClipFrame = ((curTime * hdr.frameRate) + hdr.beginFrame);
    // now fetch the next frame (loop to beginning if there isn't a next one)
    // todo - we could use fmod to remove the branch
    hdr.nextClipFrame = (hdr.currentClipFrame + 1) > hdr.endFrame ? hdr.beginFrame : hdr.currentClipFrame + 1;

    // lerpAmount needs to be a number between 0 and 1 (linear interpolation between frames)
    // we're going to set it based on where _CurTime falls between the current frame and the next frame.
    // currently, _CurTime is incrementing based on an unknown high frame rate... like 50fps, or even hundreds of fps
    // so we need to convert _CurTime to framerate and then scale that to between 0 and 1
    hdr.lerpAmount = (curTime - ((hdr.currentClipFrame - hdr.beginFrame) / hdr.frameRate)) * hdr.frameRate;

    return hdr;
}

// todo - can i switch this to the non-verbose way?
float3 DerivePosition(float4 posTexSize, sampler2D posTex, float speedInst, float speedMat, int clipIdx, float curTime, uint vtxId)
{
    HeaderInfo hdr = ReadHeader(posTexSize, posTex, clipIdx, curTime, speedInst, speedMat);

    // non-verbose way
    return Lerp10BitPos(hdr, vtxId, posTex, posTexSize);
}

PixelInfo DerivePosAndNml(float4 posTexSize, sampler2D posTex, float4 nmlTexSize, sampler2D nmlTex, float speedInst, float speedMat, int clipIdx, float curTime, uint vtxId)
{
    HeaderInfo hdr = ReadHeader(posTexSize, posTex, clipIdx, curTime, speedInst, speedMat);
    PixelInfo retVal;
    retVal.position = Lerp10BitPos(hdr, vtxId, posTex, posTexSize);
    retVal.normal = Lerp10BitNml(hdr, vtxId, nmlTex, nmlTexSize).xyz;
    retVal.tangent = 0;
    return retVal;
}

PixelInfo DerivePixelInfo(float4 posTexSize, sampler2D posTex, float4 nmlTexSize, sampler2D nmlTex, float4 tanTexSize, sampler2D tanTex, float speedInst, float speedMat, int clipIdx, float curTime, uint vtxId)
{
    HeaderInfo hdr = ReadHeader(posTexSize, posTex, clipIdx, curTime, speedInst, speedMat);
    PixelInfo retVal;
    retVal.position = Lerp10BitPos(hdr, vtxId, posTex, posTexSize);
    retVal.normal = Lerp10BitNml(hdr, vtxId, nmlTex, nmlTexSize).xyz;
    retVal.tangent = Lerp10BitTangent(hdr, vtxId, tanTex, tanTexSize);
    return retVal;
}