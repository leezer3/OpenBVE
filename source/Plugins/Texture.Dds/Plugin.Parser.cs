﻿/*
 * Copyright (C) 1999, 2000 NVIDIA Corporation
 * This file is provided without support, instruction, or implied warranty of any
 * kind.  NVIDIA makes no guarantee of its fitness for a particular purpose and is
 * not liable under any circumstances for any damages or loss whatsoever arising
 * from the use or inability to use this file or items derived from it.
 *
 * Converted to C#, assorted changes to make compatible with openBVE texture loading
 * Also some minor enum conversion & cleanup
 */

// ReSharper disable UnusedMember.Local
// ReSharper disable NotAccessedField.Local

using System;
using System.IO;
using OpenBveApi.Colors;

namespace Texture.Dds
{
    public class DDSImage
    {
	    internal OpenBveApi.Textures.Texture myTexture;
        public DDSImage(byte[] ddsImage)
        {
            if (ddsImage == null) return;
            if (ddsImage.Length == 0) return;

            using (MemoryStream stream = new MemoryStream(ddsImage.Length))
            {
                stream.Write(ddsImage, 0, ddsImage.Length);
                stream.Seek(0, SeekOrigin.Begin);

                using (BinaryReader reader = new BinaryReader(stream))
                {
                    this.Parse(reader);
                }
            }
        }

        private void Parse(BinaryReader reader)
        {
            DdsHeader header = new DdsHeader(reader);
            if (header.depth == 0) header.depth = 1;

            int blocksize;
            PixelFormat pixelFormat = this.GetFormat(header, out blocksize);
            byte[] data = this.ReadData(reader, header);
            if (data != null)
            {
	            byte[] rawData = this.DecompressData(header, data, pixelFormat);
	            CreateTexture(header.width, header.height, rawData);
            }
            else
            {
	            throw new InvalidDataException("No data read from DDS file.");
            }
        }

        private byte[] ReadData(BinaryReader reader, DdsHeader header)
        {
            byte[] data;

            if ((header.flags & DDSD_LINEARSIZE) > 1)
            {
                data = reader.ReadBytes((int)header.sizeorpitch);
                return data;
            }
            data = reader.ReadBytes((header.pixelFormat.rgbbitcount / 8) * header.width * header.height);
            return data;

        }

        private void CreateTexture(int width, int height, byte[] rawData)
        {
            
            int size = width * height * 4;
			byte[] textureData = new byte[size];
	        for (int i = 0; i < size; i += 4)
	        {
		        textureData[i] = rawData[i]; // red
		        textureData[i + 1] = rawData[i + 1]; // green
		        textureData[i + 2] = rawData[i + 2]; // blue
		        textureData[i + 3] = rawData[i + 3]; // alpha
	        }
	        myTexture = new OpenBveApi.Textures.Texture(width, height, OpenBveApi.Textures.PixelFormat.RGBAlpha, textureData, null);
        }

        private PixelFormat GetFormat(DdsHeader header, out int blocksize)
        {
            PixelFormat format;
            if ((header.pixelFormat.flags & DDPF_FOURCC) == DDPF_FOURCC)
            {
                blocksize = ((header.width + 3) / 4) * ((header.height + 3) / 4) * header.depth;
				switch (header.pixelFormat.fourcc)
                {
                    case FourCC.DXT1:
                        format = PixelFormat.DXT1;
                        blocksize *= 8;
                        break;
                    case FourCC.DXT2:
                        format = PixelFormat.DXT2;
                        blocksize *= 16;
                        break;
                    case FourCC.DXT3:
                        format = PixelFormat.DXT3;
                        blocksize *= 16;
                        break;
                    case FourCC.DXT4:
                        format = PixelFormat.DXT4;
                        blocksize *= 16;
                        break;
                    case FourCC.DXT5:
                        format = PixelFormat.DXT5;
                        blocksize *= 16;
                        break;
                    case FourCC.ATI1:
                        format = PixelFormat.ATI1N;
                        blocksize *= 8;
                        break;
                    case FourCC.ATI2:
                        format = PixelFormat.THREEDC;
                        blocksize *= 16;
                        break;
                    case FourCC.RXGB:
                        format = PixelFormat.RXGB;
                        blocksize *= 16;
                        break;
                    case FourCC.DOLLARNULL:
                        format = PixelFormat.A16B16G16R16;
                        blocksize = header.width * header.height * header.depth * 8;
                        break;
                    case FourCC.oNULL:
                        format = PixelFormat.R16F;
                        blocksize = header.width * header.height * header.depth * 2;
                        break;
                    case FourCC.pNULL:
                        format = PixelFormat.G16R16F;
                        blocksize = header.width * header.height * header.depth * 4;
                        break;
                    case FourCC.qNULL:
                        format = PixelFormat.A16B16G16R16F;
                        blocksize = header.width * header.height * header.depth * 8;
                        break;
                    case FourCC.rNULL:
                        format = PixelFormat.R32F;
                        blocksize = header.width * header.height * header.depth * 4;
                        break;
                    case FourCC.sNULL:
                        format = PixelFormat.G32R32F;
                        blocksize = header.width * header.height * header.depth * 8;
                        break;
                    case FourCC.tNULL:
                        format = PixelFormat.A32B32G32R32F;
                        blocksize = header.width * header.height * header.depth * 16;
                        break;
                    default:
                        throw new InvalidDataException("Compressed DDS PixelFormat value invalid");
                }
            }
            else
            {
                if ((header.pixelFormat.flags & DDPF_LUMINANCE) == DDPF_LUMINANCE)
                {
	                format = (header.pixelFormat.flags & DDPF_ALPHAPIXELS) == DDPF_ALPHAPIXELS ? PixelFormat.LUMINANCE_ALPHA : PixelFormat.LUMINANCE;
                }
                else
                {
	                format = (header.pixelFormat.flags & DDPF_ALPHAPIXELS) == DDPF_ALPHAPIXELS ? PixelFormat.RGBA : PixelFormat.RGB;
                }
                blocksize = (header.width * header.height * header.depth * (header.pixelFormat.rgbbitcount >> 3));
            }

            return format;
        }

        private int PixelFormatToBpp(PixelFormat pf, int rgbbitcount)
        {
            switch (pf)
            {
                case PixelFormat.LUMINANCE:
                case PixelFormat.LUMINANCE_ALPHA:
                case PixelFormat.RGBA:
                case PixelFormat.RGB:
                    return rgbbitcount / 8;

                case PixelFormat.THREEDC:
                case PixelFormat.RXGB:
                    return 3;

                case PixelFormat.ATI1N:
                    return 1;

                case PixelFormat.R16F:
                    return 2;

                case PixelFormat.A16B16G16R16:
                case PixelFormat.A16B16G16R16F:
                case PixelFormat.G32R32F:
                    return 8;

                case PixelFormat.A32B32G32R32F:
                    return 16;

                default:
                    return 4;
            }
        }

        private int PixelFormatToBpc(PixelFormat pf)
        {
            switch (pf)
            {
                case PixelFormat.R16F:
                case PixelFormat.G16R16F:
                case PixelFormat.A16B16G16R16F:
                    return 4;

                case PixelFormat.R32F:
                case PixelFormat.G32R32F:
                case PixelFormat.A32B32G32R32F:
                    return 4;

                case PixelFormat.A16B16G16R16:
                    return 2;

                default:
                    return 1;
            }
        }

        private void CorrectPremult(uint pixnum, ref byte[] buffer)
        {
            for (uint i = 0; i < pixnum; i++)
            {
                byte alpha = buffer[i + 3];
                if (alpha == 0) continue;
                int red = (buffer[i] << 8) / alpha;
                int green = (buffer[i + 1] << 8) / alpha;
                int blue = (buffer[i + 2] << 8) / alpha;

                buffer[i] = (byte)red;
                buffer[i + 1] = (byte)green;
                buffer[i + 2] = (byte)blue;
            }
        }

        private void ComputeMaskParams(uint mask, out int shift1, out int mul, out int shift2)
        {
            shift1 = 0; mul = 1; shift2 = 0;
            while ((mask & 1) == 0)
            {
                mask >>= 1;
                shift1++;
            }
            uint bc = 0;
            while ((mask & (1 << (int)bc)) != 0) bc++;
            while ((mask * mul) < 255)
                mul = (mul << (int)bc) + 1;
            mask *= (uint)mul;

            while ((mask & ~0xff) != 0)
            {
                mask >>= 1;
                shift2++;
            }
        }

        private unsafe void DxtcReadColors(byte* data, ref Color32[] op)
        {
            byte b0 = (byte)(data[0] & 0x1F);
            byte g0 = (byte)(((data[0] & 0xE0) >> 5) | ((data[1] & 0x7) << 3));
            byte r0 = (byte)((data[1] & 0xF8) >> 3);

            byte b1 = (byte)(data[2] & 0x1F);
            byte g1 = (byte)(((data[2] & 0xE0) >> 5) | ((data[3] & 0x7) << 3));
            byte r1 = (byte)((data[3] & 0xF8) >> 3);

            op[0].R = (byte)(r0 << 3 | r0 >> 2);
            op[0].G = (byte)(g0 << 2 | g0 >> 3);
            op[0].B = (byte)(b0 << 3 | b0 >> 2);

            op[1].R = (byte)(r1 << 3 | r1 >> 2);
            op[1].G = (byte)(g1 << 2 | g1 >> 3);
            op[1].B = (byte)(b1 << 3 | b1 >> 2);
        }

        private void DxtcReadColor(ushort data, ref Color32 op)
        {
            byte b = (byte)(data & 0x1f);
            byte g = (byte)((data & 0x7E0) >> 5);
            byte r = (byte)((data & 0xF800) >> 11);

            op.R = (byte)(r << 3 | r >> 2);
            op.G = (byte)(g << 2 | g >> 3);
            op.B = (byte)(b << 3 | b >> 2);
        }

        private unsafe void DxtcReadColors(byte* data, ref Color32 color_0, ref Color32 color_1)
        {
            color_0.B = (byte)(data[0] & 0x1F);
            color_0.G = (byte)(((data[0] & 0xE0) >> 5) | ((data[1] & 0x7) << 3));
            color_0.R = (byte)((data[1] & 0xF8) >> 3);

            color_1.B = (byte)(data[2] & 0x1F);
            color_1.G = (byte)(((data[2] & 0xE0) >> 5) | ((data[3] & 0x7) << 3));
            color_1.R = (byte)((data[3] & 0xF8) >> 3);
        }

        private void GetBitsFromMask(uint mask, out uint shiftLeft, out uint shiftRight)
        {
            uint i;

            if (mask == 0)
            {
                shiftLeft = shiftRight = 0;
                return;
            }

            uint temp = mask;
            for (i = 0; i < 32; i++, temp >>= 1)
            {
                if ((temp & 1) != 0)
                    break;
            }
            shiftRight = i;

            // Temp is preserved, so use it again:
            for (i = 0; i < 8; i++, temp >>= 1)
            {
                if ((temp & 1) == 0)
                    break;
            }
            shiftLeft = 8 - i;
        }

        // This function simply counts how many contiguous bits are in the mask.
        private uint CountBitsFromMask(uint mask)
        {
            uint i, testBit = 0x01, count = 0;
            bool foundBit = false;

            for (i = 0; i < 32; i++, testBit <<= 1)
            {
                if ((mask & testBit) != 0)
                {
                    if (!foundBit)
                        foundBit = true;
                    count++;
                }
                else if (foundBit)
                    return count;
            }

            return count;
        }

        private uint HalfToFloat(ushort y)
        {
            int s = (y >> 15) & 0x00000001;
            int e = (y >> 10) & 0x0000001f;
            int m = y & 0x000003ff;

            if (e == 0)
            {
	            if (m == 0)
                {
                    //
                    // Plus or minus zero
                    //
                    return (uint)(s << 31);
                }

	            //
	            // Denormalized number -- renormalize it
	            //
	            while ((m & 0x00000400) == 0)
	            {
		            m <<= 1;
		            e -= 1;
	            }

	            e += 1;
	            m &= ~0x00000400;
            }
            else if (e == 31)
            {
	            if (m == 0)
                {
                    //
                    // Positive or negative infinity
                    //
                    return (uint)((s << 31) | 0x7f800000);
                }

	            //
	            // Nan -- preserve sign and significand bits
	            //
	            return (uint)((s << 31) | 0x7f800000 | (m << 13));
            }

            //
            // Normalized number
            //
            e += (127 - 15);
            m <<= 13;

            //
            // Assemble s, e and m.
            //
            return (uint)((s << 31) | (e << 23) | m);
        }

        private unsafe void ConvFloat16ToFloat32(uint* dest, ushort* src, uint size)
        {
            uint i;
            for (i = 0; i < size; ++i, ++dest, ++src)
            {
                //float: 1 sign bit, 8 exponent bits, 23 mantissa bits
                //half: 1 sign bit, 5 exponent bits, 10 mantissa bits
                *dest = HalfToFloat(*src);
            }
        }

        private unsafe void ConvG16R16ToFloat32(uint* dest, ushort* src, uint size)
        {
            uint i;
            for (i = 0; i < size; i += 3)
            {
                //float: 1 sign bit, 8 exponent bits, 23 mantissa bits
                //half: 1 sign bit, 5 exponent bits, 10 mantissa bits
                *dest++ = HalfToFloat(*src++);
                *dest++ = HalfToFloat(*src++);
                *((float*)dest++) = 1.0f;
            }
        }

        private unsafe void ConvR16ToFloat32(uint* dest, ushort* src, uint size)
        {
            uint i;
            for (i = 0; i < size; i += 3)
            {
                //float: 1 sign bit, 8 exponent bits, 23 mantissa bits
                //half: 1 sign bit, 5 exponent bits, 10 mantissa bits
                *dest++ = HalfToFloat(*src++);
                *((float*)dest++) = 1.0f;
                *((float*)dest++) = 1.0f;
            }
        }

        private byte[] DecompressData(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            byte[] rawData;
            switch (pixelFormat)
            {
                case PixelFormat.RGBA:
                    rawData = this.DecompressRGBA(header, data, pixelFormat);
	                break;
                case PixelFormat.RGB:
                    rawData = this.DecompressRGB(header, data, pixelFormat);
                    break;
                case PixelFormat.LUMINANCE:
                case PixelFormat.LUMINANCE_ALPHA:
                    rawData = this.DecompressLum(header, data, pixelFormat);
                    break;
                case PixelFormat.DXT1:
                    rawData = this.DecompressDXT1(header, data, pixelFormat);
                    break;
                case PixelFormat.DXT2:
                    rawData = this.DecompressDXT2(header, data, pixelFormat);
                    break;
                case PixelFormat.DXT3:
                    rawData = this.DecompressDXT3(header, data, pixelFormat);
                    break;
                case PixelFormat.DXT4:
                    rawData = this.DecompressDXT4(header, data, pixelFormat);
                    break;
                case PixelFormat.DXT5:
                    rawData = this.DecompressDXT5(header, data, pixelFormat);
                    break;
                case PixelFormat.THREEDC:
                    rawData = this.Decompress3Dc(header, data, pixelFormat);
                    break;
                case PixelFormat.ATI1N:
                    rawData = this.DecompressAti1n(header, data, pixelFormat);
                    break;
                case PixelFormat.RXGB:
                    rawData = this.DecompressRXGB(header, data, pixelFormat);
                    break;
                case PixelFormat.R16F:
                case PixelFormat.G16R16F:
                case PixelFormat.A16B16G16R16F:
                case PixelFormat.R32F:
                case PixelFormat.G32R32F:
                case PixelFormat.A32B32G32R32F:
                    rawData = this.DecompressFloat(header, data, pixelFormat);
                    break;
                default:
                    throw new InvalidDataException("Unsupported DDS PixelFormat value");
            }

            return rawData;
        }

        private unsafe byte[] DecompressDXT1(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            int bpp = PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount);
            int bps = header.width * bpp * PixelFormatToBpc(pixelFormat);
            int sizeofplane = bps * header.height;
            byte[] rawData = new byte[header.depth * sizeofplane + header.height * bps + header.width * bpp];

            Color32[] colours = new Color32[4];
            colours[0].A = 0xFF;
            colours[1].A = 0xFF;
            colours[2].A = 0xFF;

            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                for (int z = 0; z < header.depth; z++)
                {
                    for (int y = 0; y < header.height; y += 4)
                    {
                        for (int x = 0; x < header.width; x += 4)
                        {
                            ushort colour0 = *((ushort*)temp);
                            ushort colour1 = *((ushort*)(temp + 2));
                            DxtcReadColor(colour0, ref colours[0]);
                            DxtcReadColor(colour1, ref colours[1]);

                            uint bitmask = ((uint*)temp)[1];
                            temp += 8;

                            if (colour0 > colour1)
                            {
                                colours[2].B = (byte)((2 * colours[0].B + colours[1].B + 1) / 3);
                                colours[2].G = (byte)((2 * colours[0].G + colours[1].G + 1) / 3);
                                colours[2].R = (byte)((2 * colours[0].R + colours[1].R + 1) / 3);

                                colours[3].B = (byte)((colours[0].B + 2 * colours[1].B + 1) / 3);
                                colours[3].G = (byte)((colours[0].G + 2 * colours[1].G + 1) / 3);
                                colours[3].R = (byte)((colours[0].R + 2 * colours[1].R + 1) / 3);
                                colours[3].A = 0xFF;
                            }
                            else
                            {
                                colours[2].B = (byte)((colours[0].B + colours[1].B) / 2);
                                colours[2].G = (byte)((colours[0].G + colours[1].G) / 2);
                                colours[2].R = (byte)((colours[0].R + colours[1].R) / 2);

                                colours[3].B = (byte)((colours[0].B + 2 * colours[1].B + 1) / 3);
                                colours[3].G = (byte)((colours[0].G + 2 * colours[1].G + 1) / 3);
                                colours[3].R = (byte)((colours[0].R + 2 * colours[1].R + 1) / 3);
                                colours[3].A = 0x00;
                            }

                            for (int j = 0, k = 0; j < 4; j++)
                            {
                                for (int i = 0; i < 4; i++, k++)
                                {
                                    int select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);
                                    Color32 col = colours[select];
                                    if (((x + i) < header.width) && ((y + j) < header.height))
                                    {
                                        uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
                                        rawData[offset + 0] = col.R;
                                        rawData[offset + 1] = col.G;
                                        rawData[offset + 2] = col.B;
                                        rawData[offset + 3] = col.A;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return rawData;
        }

        private byte[] DecompressDXT2(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
	        byte[] rawData = DecompressDXT3(header, data, pixelFormat);
            CorrectPremult((uint)(header.width * header.height * header.depth), ref rawData);
			return rawData;
        }

        private unsafe byte[] DecompressDXT3(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            int bpp = PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount);
            int bps = header.width * bpp * PixelFormatToBpc(pixelFormat);
            int sizeofplane = bps * header.height;
            byte[] rawData = new byte[header.depth * sizeofplane + header.height * bps + header.width * bpp];
            Color32[] colours = new Color32[4];
            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                for (int z = 0; z < header.depth; z++)
                {
                    for (int y = 0; y < header.height; y += 4)
                    {
                        for (int x = 0; x < header.width; x += 4)
                        {
                            byte* alpha = temp;
                            temp += 8;

                            DxtcReadColors(temp, ref colours);
                            temp += 4;

                            uint bitmask = ((uint*)temp)[0];
                            temp += 4;
                            colours[2].B = (byte)((2 * colours[0].B + colours[1].B + 1) / 3);
                            colours[2].G = (byte)((2 * colours[0].G + colours[1].G + 1) / 3);
                            colours[2].R = (byte)((2 * colours[0].R + colours[1].R + 1) / 3);

                            colours[3].B = (byte)((colours[0].B + 2 * colours[1].B + 1) / 3);
                            colours[3].G = (byte)((colours[0].G + 2 * colours[1].G + 1) / 3);
                            colours[3].R = (byte)((colours[0].R + 2 * colours[1].R + 1) / 3);

                            for (int j = 0, k = 0; j < 4; j++)
                            {
                                for (int i = 0; i < 4; k++, i++)
                                {
                                    int select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);

                                    if (((x + i) < header.width) && ((y + j) < header.height))
                                    {
                                        uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
                                        rawData[offset + 0] = colours[select].R;
                                        rawData[offset + 1] = colours[select].G;
                                        rawData[offset + 2] = colours[select].B;
                                    }
                                }
                            }

                            for (int j = 0; j < 4; j++)
                            {
                                ushort word = (ushort)(alpha[2 * j] | (alpha[2 * j + 1] << 8)); 
                                for (int i = 0; i < 4; i++)
                                {
                                    if (((x + i) < header.width) && ((y + j) < header.height))
                                    {
                                        uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
                                        rawData[offset] = (byte)(word & 0x0F);
                                        rawData[offset] = (byte)(rawData[offset] | (rawData[offset] << 4));
                                    }
                                    word >>= 4;
                                }
                            }
                        }
                    }
                }
            }
            return rawData;
        }

        private byte[] DecompressDXT4(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
	        byte[] rawData = DecompressDXT5(header, data, pixelFormat);
            CorrectPremult((uint)(header.width * header.height * header.depth), ref rawData);

            return rawData;
        }

        private unsafe byte[] DecompressDXT5(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            // allocate bitmap
            int bpp = PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount);
            int bps = header.width * bpp * PixelFormatToBpc(pixelFormat);
            int sizeofplane = bps * header.height;

            byte[] rawData = new byte[header.depth * sizeofplane + header.height * bps + header.width * bpp];
            Color32[] colours = new Color32[4];
            ushort[] alphas = new ushort[8];

            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                for (int z = 0; z < header.depth; z++)
                {
                    for (int y = 0; y < header.height; y += 4)
                    {
                        for (int x = 0; x < header.width; x += 4)
                        {
                            if (y >= header.height || x >= header.width)
                                break;

                            alphas[0] = temp[0];
                            alphas[1] = temp[1];
                            byte* alphamask = (temp + 2);
                            temp += 8;

                            DxtcReadColors(temp, ref colours);
                            uint bitmask = ((uint*)temp)[1];
                            temp += 8;
                            colours[2].B = (byte)((2 * colours[0].B + colours[1].B + 1) / 3);
                            colours[2].G = (byte)((2 * colours[0].G + colours[1].G + 1) / 3);
                            colours[2].R = (byte)((2 * colours[0].R + colours[1].R + 1) / 3);
                            colours[3].B = (byte)((colours[0].B + 2 * colours[1].B + 1) / 3);
                            colours[3].G = (byte)((colours[0].G + 2 * colours[1].G + 1) / 3);
                            colours[3].R = (byte)((colours[0].R + 2 * colours[1].R + 1) / 3);
                            int k = 0;
                            for (int j = 0; j < 4; j++)
                            {
                                for (int i = 0; i < 4; k++, i++)
                                {
                                    int select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);
                                    Color32 col = colours[select];
                                    // only put pixels out < width or height
                                    if (((x + i) < header.width) && ((y + j) < header.height))
                                    {
                                        uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
                                        rawData[offset] = col.R;
                                        rawData[offset + 1] = col.G;
                                        rawData[offset + 2] = col.B;
                                    }
                                }
                            }

                            // 8-alpha or 6-alpha block?
                            if (alphas[0] > alphas[1])
                            {
                                // 8-alpha block:  derive the other six alphas.
                                // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                                alphas[2] = (ushort)((6 * alphas[0] + 1 * alphas[1] + 3) / 7); // bit code 010
                                alphas[3] = (ushort)((5 * alphas[0] + 2 * alphas[1] + 3) / 7); // bit code 011
                                alphas[4] = (ushort)((4 * alphas[0] + 3 * alphas[1] + 3) / 7); // bit code 100
                                alphas[5] = (ushort)((3 * alphas[0] + 4 * alphas[1] + 3) / 7); // bit code 101
                                alphas[6] = (ushort)((2 * alphas[0] + 5 * alphas[1] + 3) / 7); // bit code 110
                                alphas[7] = (ushort)((1 * alphas[0] + 6 * alphas[1] + 3) / 7); // bit code 111
                            }
                            else
                            {
                                // 6-alpha block.
                                // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                                alphas[2] = (ushort)((4 * alphas[0] + 1 * alphas[1] + 2) / 5); // Bit code 010
                                alphas[3] = (ushort)((3 * alphas[0] + 2 * alphas[1] + 2) / 5); // Bit code 011
                                alphas[4] = (ushort)((2 * alphas[0] + 3 * alphas[1] + 2) / 5); // Bit code 100
                                alphas[5] = (ushort)((1 * alphas[0] + 4 * alphas[1] + 2) / 5); // Bit code 101
                                alphas[6] = 0x00; // Bit code 110
                                alphas[7] = 0xFF; // Bit code 111
                            }

                            // Note: Have to separate the next two loops,
                            // it operates on a 6-byte system.

                            // First three bytes
                            //uint bits = (uint)(alphamask[0]);
                            uint bits = (uint)((alphamask[0]) | (alphamask[1] << 8) | (alphamask[2] << 16));
                            for (int j = 0; j < 2; j++)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    // only put pixels out < width or height
                                    if (((x + i) < header.width) && ((y + j) < header.height))
                                    {
                                        uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
                                        rawData[offset] = (byte)alphas[bits & 0x07];
                                    }
                                    bits >>= 3;
                                }
                            }

                            // Last three bytes
                            //bits = (uint)(alphamask[3]);
                            bits = (uint)((alphamask[3]) | (alphamask[4] << 8) | (alphamask[5] << 16));
                            for (int j = 2; j < 4; j++)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    // only put pixels out < width or height
                                    if (((x + i) < header.width) && ((y + j) < header.height))
                                    {
                                        uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
                                        rawData[offset] = (byte)alphas[bits & 0x07];
                                    }
                                    bits >>= 3;
                                }
                            }
                        }
                    }
                }
            }

            return rawData;
        }

        private unsafe byte[] DecompressRGB(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            // allocate bitmap
            int bpp = PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount);
            int bps = header.width * bpp * PixelFormatToBpc(pixelFormat);
            int sizeofplane = bps * header.height;

            byte[] rawData = new byte[header.depth * sizeofplane + header.height * bps + header.width * bpp];
	        uint valMask;
	        unchecked
	        {
		        valMask = (uint)((header.pixelFormat.rgbbitcount == 32) ? ~0 : (1 << header.pixelFormat.rgbbitcount) - 1);
	        }
            uint pixSize = (uint)((header.pixelFormat.rgbbitcount + 7) / 8);
            int rShift1; int rMul; int rShift2;
            ComputeMaskParams(header.pixelFormat.rbitmask, out rShift1, out rMul, out rShift2);
            int gShift1; int gMul; int gShift2;
            ComputeMaskParams(header.pixelFormat.gbitmask, out gShift1, out gMul, out gShift2);
            int bShift1; int bMul; int bShift2;
            ComputeMaskParams(header.pixelFormat.bbitmask, out bShift1, out bMul, out bShift2);

            int offset = 0;
            int pixnum = header.width * header.height * header.depth;
            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                while (pixnum-- > 0)
                {
                    uint px = *((uint*)temp) & valMask;
                    temp += pixSize;
                    uint pxc = px & header.pixelFormat.rbitmask;
                    rawData[offset + 0] = (byte)(((pxc >> rShift1) * rMul) >> rShift2);
                    pxc = px & header.pixelFormat.gbitmask;
                    rawData[offset + 1] = (byte)(((pxc >> gShift1) * gMul) >> gShift2);
                    pxc = px & header.pixelFormat.bbitmask;
                    rawData[offset + 2] = (byte)(((pxc >> bShift1) * bMul) >> bShift2);
                    rawData[offset + 3] = 0xff;
                    offset += 4;
                }
            }
            return rawData;
        }

        private unsafe byte[] DecompressRGBA(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            // allocate bitmap
            int bpp = PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount);
            int bps = header.width * bpp * PixelFormatToBpc(pixelFormat);
            int sizeofplane = bps * header.height;
            
            byte[] rawData = new byte[header.depth * sizeofplane + header.height * bps + header.width * bpp];
	        uint valMask;
	        unchecked
	        {
		        valMask = (uint)((header.pixelFormat.rgbbitcount == 32) ? ~0 : (1 << header.pixelFormat.rgbbitcount) - 1);
	        }
            int pixSize = (header.pixelFormat.rgbbitcount + 7) / 8;
            int rShift1; int rMul; int rShift2;
            ComputeMaskParams(header.pixelFormat.rbitmask, out rShift1, out rMul, out rShift2);
            int gShift1; int gMul; int gShift2;
            ComputeMaskParams(header.pixelFormat.gbitmask, out gShift1, out gMul, out gShift2);
            int bShift1; int bMul; int bShift2;
            ComputeMaskParams(header.pixelFormat.bbitmask, out bShift1, out bMul, out bShift2);
            int aShift1; int aMul; int aShift2;
            ComputeMaskParams(header.pixelFormat.alphabitmask, out aShift1, out aMul, out aShift2);

            int offset = 0;
            int pixnum = header.width * header.height * header.depth;
            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;

                while (pixnum-- > 0)
                {
                    uint px = *((uint*)temp) & valMask;
                    temp += pixSize;
                    uint pxc = px & header.pixelFormat.rbitmask;
                    rawData[offset + 0] = (byte)(((pxc >> rShift1) * rMul) >> rShift2);
                    pxc = px & header.pixelFormat.gbitmask;
                    rawData[offset + 1] = (byte)(((pxc >> gShift1) * gMul) >> gShift2);
                    pxc = px & header.pixelFormat.bbitmask;
                    rawData[offset + 2] = (byte)(((pxc >> bShift1) * bMul) >> bShift2);
                    pxc = px & header.pixelFormat.alphabitmask;
                    rawData[offset + 3] = (byte)(((pxc >> aShift1) * aMul) >> aShift2);
                    offset += 4;
                }
            }
            return rawData;
        }

        private unsafe byte[] Decompress3Dc(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            // allocate bitmap
            int bpp = PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount);
            int bps = header.width * bpp * PixelFormatToBpc(pixelFormat);
            int sizeofplane = bps * header.height;
            
            byte[] rawData = new byte[header.depth * sizeofplane + header.height * bps + header.width * bpp];
            byte[] yColours = new byte[8];
            byte[] xColours = new byte[8];

            int offset = 0;
            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                for (int z = 0; z < header.depth; z++)
                {
                    for (int y = 0; y < header.height; y += 4)
                    {
                        for (int x = 0; x < header.width; x += 4)
                        {
                            byte* temp2 = temp + 8;

                            //Read Y palette
                            int t1 = yColours[0] = temp[0];
                            int t2 = yColours[1] = temp[1];
                            temp += 2;
                            if (t1 > t2)
                                for (int i = 2; i < 8; ++i)
                                    yColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 7);
                            else
                            {
                                for (int i = 2; i < 6; ++i)
                                    yColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 5);
                                yColours[6] = 0;
                                yColours[7] = 255;
                            }

                            // Read X palette
                            t1 = xColours[0] = temp2[0];
                            t2 = xColours[1] = temp2[1];
                            temp2 += 2;
                            if (t1 > t2)
                                for (int i = 2; i < 8; ++i)
                                    xColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 7);
                            else
                            {
                                for (int i = 2; i < 6; ++i)
                                    xColours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 5);
                                xColours[6] = 0;
                                xColours[7] = 255;
                            }

                            //decompress pixel data
                            int currentOffset = offset;
                            for (int k = 0; k < 4; k += 2)
                            {
                                // First three bytes
                                uint bitmask = ((uint)(temp[0]) << 0) | ((uint)(temp[1]) << 8) | ((uint)(temp[2]) << 16);
                                uint bitmask2 = ((uint)(temp2[0]) << 0) | ((uint)(temp2[1]) << 8) | ((uint)(temp2[2]) << 16);
                                for (int j = 0; j < 2; j++)
                                {
                                    // only put pixels out < height
                                    if ((y + k + j) < header.height)
                                    {
                                        for (int i = 0; i < 4; i++)
                                        {
                                            // only put pixels out < width
                                            if (((x + i) < header.width))
                                            {
                                                byte tx, ty;

                                                t1 = currentOffset + (x + i) * 3;
                                                rawData[t1 + 1] = ty = yColours[bitmask & 0x07];
                                                rawData[t1 + 0] = tx = xColours[bitmask2 & 0x07];

                                                //calculate b (z) component ((r/255)^2 + (g/255)^2 + (b/255)^2 = 1
                                                int t = 127 * 128 - (tx - 127) * (tx - 128) - (ty - 127) * (ty - 128);
                                                if (t > 0)
                                                    rawData[t1 + 2] = (byte)(Math.Sqrt(t) + 128);
                                                else
                                                    rawData[t1 + 2] = 0x7F;
                                            }
                                            bitmask >>= 3;
                                            bitmask2 >>= 3;
                                        }
                                        currentOffset += bps;
                                    }
                                }
                                temp += 3;
                                temp2 += 3;
                            }

                            //skip bytes that were read via Temp2
                            temp += 8;
                        }
                        offset += bps * 4;
                    }
                }
            }

            return rawData;
        }

        private unsafe byte[] DecompressAti1n(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            // allocate bitmap
            int bpp = PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount);
            int bps = header.width * bpp * PixelFormatToBpc(pixelFormat);
            int sizeofplane = bps * header.height;

            byte[] rawData = new byte[header.depth * sizeofplane + header.height * bps + header.width * bpp];
            byte[] colours = new byte[8];

            uint offset = 0;
            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                for (int z = 0; z < header.depth; z++)
                {
                    for (int y = 0; y < header.height; y += 4)
                    {
                        for (int x = 0; x < header.width; x += 4)
                        {
                            //Read palette
                            int t1 = colours[0] = temp[0];
                            int t2 = colours[1] = temp[1];
                            temp += 2;
                            if (t1 > t2)
                                for (int i = 2; i < 8; ++i)
                                    colours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 7);
                            else
                            {
                                for (int i = 2; i < 6; ++i)
                                    colours[i] = (byte)(t1 + ((t2 - t1) * (i - 1)) / 5);
                                colours[6] = 0;
                                colours[7] = 255;
                            }

                            //decompress pixel data
                            uint currOffset = offset;
                            for (int k = 0; k < 4; k += 2)
                            {
                                // First three bytes
                                uint bitmask = ((uint)(temp[0]) << 0) | ((uint)(temp[1]) << 8) | ((uint)(temp[2]) << 16);
                                for (int j = 0; j < 2; j++)
                                {
                                    // only put pixels out < height
                                    if ((y + k + j) < header.height)
                                    {
                                        for (int i = 0; i < 4; i++)
                                        {
                                            // only put pixels out < width
                                            if (((x + i) < header.width))
                                            {
                                                t1 = (int)(currOffset + (x + i));
                                                rawData[t1] = colours[bitmask & 0x07];
                                            }
                                            bitmask >>= 3;
                                        }
                                        currOffset += (uint)bps;
                                    }
                                }
                                temp += 3;
                            }
                        }
                        offset += (uint)(bps * 4);
                    }
                }
            }
            return rawData;
        }

        private unsafe byte[] DecompressLum(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            // allocate bitmap
            int bpp = PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount);
            int bps = header.width * bpp * PixelFormatToBpc(pixelFormat);
            int sizeofplane = bps * header.height;

            byte[] rawData = new byte[header.depth * sizeofplane + header.height * bps + header.width * bpp];

            int lShift1; int lMul; int lShift2;
            ComputeMaskParams(header.pixelFormat.rbitmask, out lShift1, out lMul, out lShift2);

            int offset = 0;
            int pixnum = header.width * header.height * header.depth;
            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                while (pixnum-- > 0)
                {
                    byte px = *(temp++);
                    rawData[offset + 0] = (byte)(((px >> lShift1) * lMul) >> lShift2);
                    rawData[offset + 1] = (byte)(((px >> lShift1) * lMul) >> lShift2);
                    rawData[offset + 2] = (byte)(((px >> lShift1) * lMul) >> lShift2);
                    rawData[offset + 3] = (byte)(((px >> lShift1) * lMul) >> lShift2);
                    offset += 4;
                }
            }
            return rawData;
        }

        private unsafe byte[] DecompressRXGB(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            // allocate bitmap
            int bpp = PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount);
            int bps = header.width * bpp * PixelFormatToBpc(pixelFormat);
            int sizeofplane = bps * header.height;

            byte[] rawData = new byte[header.depth * sizeofplane + header.height * bps + header.width * bpp];

            Color32 color_0 = new Color32();
            Color32 color_1 = new Color32();
	        Color32[] colours = new Color32[4];
	        byte[] alphas = new byte[8];

            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                for (int z = 0; z < header.depth; z++)
                {
                    for (int y = 0; y < header.height; y += 4)
                    {
                        for (int x = 0; x < header.width; x += 4)
                        {
                            if (y >= header.height || x >= header.width)
                                break;
                            alphas[0] = temp[0];
                            alphas[1] = temp[1];
                            byte* alphamask = temp + 2;
                            temp += 8;

                            DxtcReadColors(temp, ref color_0, ref color_1);
                            temp += 4;

                            uint bitmask = ((uint*)temp)[1];
                            temp += 4;

                            colours[0].R = (byte)(color_0.R << 3);
                            colours[0].G = (byte)(color_0.G << 2);
                            colours[0].B = (byte)(color_0.B << 3);
                            colours[0].A = 0xFF;

                            colours[1].R = (byte)(color_1.R << 3);
                            colours[1].G = (byte)(color_1.G << 2);
                            colours[1].B = (byte)(color_1.B << 3);
                            colours[1].A = 0xFF;

                            // Four-color block: derive the other two colors.    
                            // 00 = color_0, 01 = color_1, 10 = color_2, 11 = color_3
                            // These 2-bit codes correspond to the 2-bit fields 
                            // stored in the 64-bit block.
                            colours[2].B = (byte)((2 * colours[0].B + colours[1].B + 1) / 3);
                            colours[2].G = (byte)((2 * colours[0].G + colours[1].G + 1) / 3);
                            colours[2].R = (byte)((2 * colours[0].R + colours[1].R + 1) / 3);
                            colours[2].A = 0xFF;

                            colours[3].B = (byte)((colours[0].B + 2 * colours[1].B + 1) / 3);
                            colours[3].G = (byte)((colours[0].G + 2 * colours[1].G + 1) / 3);
                            colours[3].R = (byte)((colours[0].R + 2 * colours[1].R + 1) / 3);
                            colours[3].A = 0xFF;

                            int k = 0;
                            for (int j = 0; j < 4; j++)
                            {
                                for (int i = 0; i < 4; i++, k++)
                                {
                                    int select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);
                                    Color32 col = colours[select];

                                    // only put pixels out < width or height
                                    if (((x + i) < header.width) && ((y + j) < header.height))
                                    {
                                        uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
                                        rawData[offset + 0] = col.R;
                                        rawData[offset + 1] = col.G;
                                        rawData[offset + 2] = col.B;
                                    }
                                }
                            }

                            // 8-alpha or 6-alpha block?    
                            if (alphas[0] > alphas[1])
                            {
                                // 8-alpha block:  derive the other six alphas.    
                                // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                                alphas[2] = (byte)((6 * alphas[0] + 1 * alphas[1] + 3) / 7);	// bit code 010
                                alphas[3] = (byte)((5 * alphas[0] + 2 * alphas[1] + 3) / 7);	// bit code 011
                                alphas[4] = (byte)((4 * alphas[0] + 3 * alphas[1] + 3) / 7);	// bit code 100
                                alphas[5] = (byte)((3 * alphas[0] + 4 * alphas[1] + 3) / 7);	// bit code 101
                                alphas[6] = (byte)((2 * alphas[0] + 5 * alphas[1] + 3) / 7);	// bit code 110
                                alphas[7] = (byte)((1 * alphas[0] + 6 * alphas[1] + 3) / 7);	// bit code 111
                            }
                            else
                            {
                                // 6-alpha block.
                                // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                                alphas[2] = (byte)((4 * alphas[0] + 1 * alphas[1] + 2) / 5);	// Bit code 010
                                alphas[3] = (byte)((3 * alphas[0] + 2 * alphas[1] + 2) / 5);	// Bit code 011
                                alphas[4] = (byte)((2 * alphas[0] + 3 * alphas[1] + 2) / 5);	// Bit code 100
                                alphas[5] = (byte)((1 * alphas[0] + 4 * alphas[1] + 2) / 5);	// Bit code 101
                                alphas[6] = 0x00;										// Bit code 110
                                alphas[7] = 0xFF;										// Bit code 111
                            }

                            // Note: Have to separate the next two loops,
                            //	it operates on a 6-byte system.
                            // First three bytes
                            uint bits = *((uint*)alphamask);
                            for (int j = 0; j < 2; j++)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    // only put pixels out < width or height
                                    if (((x + i) < header.width) && ((y + j) < header.height))
                                    {
                                        uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
                                        rawData[offset] = alphas[bits & 0x07];
                                    }
                                    bits >>= 3;
                                }
                            }

                            // Last three bytes
                            bits = *((uint*)&alphamask[3]);
                            for (int j = 2; j < 4; j++)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    // only put pixels out < width or height
                                    if (((x + i) < header.width) && ((y + j) < header.height))
                                    {
                                        uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
                                        rawData[offset] = alphas[bits & 0x07];
                                    }
                                    bits >>= 3;
                                }
                            }
                        }
                    }
                }
            }
            return rawData;
        }

        private unsafe byte[] DecompressFloat(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            // allocate bitmap
            int bpp = PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount);
            int bps = header.width * bpp * PixelFormatToBpc(pixelFormat);
            int sizeofplane = bps * header.height;

            byte[] rawData = new byte[header.depth * sizeofplane + header.height * bps + header.width * bpp];
            
            fixed (byte* bytePtr = data)
            {
	            byte* temp = bytePtr;
                fixed (byte* destPtr = rawData)
                {
                    byte* destData = destPtr;
                    int size;
                    switch (pixelFormat)
                    {
                        case PixelFormat.R32F:  // Red float, green = blue = max
                            size = header.width * header.height * header.depth * 3;
                            for (int i = 0, j = 0; i < size; i += 3, j++)
                            {
                                ((float*)destData)[i] = ((float*)temp)[j];
                                ((float*)destData)[i + 1] = 1.0f;
                                ((float*)destData)[i + 2] = 1.0f;
                            }
                            break;

                        case PixelFormat.A32B32G32R32F:  // Direct copy of float RGBA data
                            Array.Copy(data, rawData, data.Length);
                            break;

                        case PixelFormat.G32R32F:  // Red float, green float, blue = max
                            size = header.width * header.height * header.depth * 3;
                            for (int i = 0, j = 0; i < size; i += 3, j += 2)
                            {
                                ((float*)destData)[i] = ((float*)temp)[j];
                                ((float*)destData)[i + 1] = ((float*)temp)[j + 1];
                                ((float*)destData)[i + 2] = 1.0f;
                            }
                            break;

                        case PixelFormat.R16F:  // Red float, green = blue = max
                            size = header.width * header.height * header.depth * bpp;
                            ConvR16ToFloat32((uint*)destData, (ushort*)temp, (uint)size);
                            break;

                        case PixelFormat.A16B16G16R16F:  // Just convert from half to float.
                            size = header.width * header.height * header.depth * bpp;
                            ConvFloat16ToFloat32((uint*)destData, (ushort*)temp, (uint)size);
                            break;

                        case PixelFormat.G16R16F:  // Convert from half to float, set blue = max.
                            size = header.width * header.height * header.depth * bpp;
                            ConvG16R16ToFloat32((uint*)destData, (ushort*)temp, (uint)size);
                            break;
	                    default:
		                    throw new NotImplementedException("Decompression of PixelFormat " + pixelFormat + " has not been implemented in this plugin.");
                    }
                }
            }

            return rawData;
        }

        private unsafe byte[] DecompressARGB(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            // allocate bitmap
            int bpp = PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount);
            int bps = header.width * bpp * PixelFormatToBpc(pixelFormat);
            int sizeofplane = bps * header.height;

            if (header.Check16BitComponents())
                return DecompressARGB16(header, data, pixelFormat);

            int sizeOfData = (header.width * header.pixelFormat.rgbbitcount / 8) * header.height * header.depth;
            byte[] rawData = new byte[header.depth * sizeofplane + header.height * bps + header.width * bpp];

            if ((pixelFormat == PixelFormat.LUMINANCE) && (header.pixelFormat.rgbbitcount == 16) && (header.pixelFormat.rbitmask == 0xFFFF))
            {
                Array.Copy(data, rawData, data.Length);
                return rawData;
            }

            uint readI = 0;
            uint redL, redR;
            uint greenL, greenR;
            uint blueL, blueR;
            uint alphaL, alphaR;

            GetBitsFromMask(header.pixelFormat.rbitmask, out redL, out redR);
            GetBitsFromMask(header.pixelFormat.gbitmask, out greenL, out greenR);
            GetBitsFromMask(header.pixelFormat.bbitmask, out blueL, out blueR);
            GetBitsFromMask(header.pixelFormat.alphabitmask, out alphaL, out alphaR);
            uint tempBpp = (uint)(header.pixelFormat.rgbbitcount / 8);

            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                for (int i = 0; i < sizeOfData; i += bpp)
                {
                    //@TODO: This is SLOOOW...
                    //but the old version crashed in release build under
                    //winxp (and xp is right to stop this code - I always
                    //wondered that it worked the old way at all)
                    if (sizeOfData - i < 4)
                    { 
                        //less than 4 byte to write?
                        if (tempBpp == 3)
                        { 
                            //this branch is extra-SLOOOW
                            readI = (uint)(*temp | ((*(temp + 1)) << 8) | ((*(temp + 2)) << 16));
                        }
                        else if (tempBpp == 1)
                            readI = *temp;
                        else if (tempBpp == 2)
                            readI = (uint)(temp[0] | (temp[1] << 8));
                    }
                    else
                        readI = (uint)(temp[0] | (temp[1] << 8) | (temp[2] << 16) | (temp[3] << 24));
                    temp += tempBpp;

                    rawData[i] = (byte)((((int)readI & (int)header.pixelFormat.rbitmask) >> (int)redR) << (int)redL);

                    if (bpp >= 3)
                    {
                        rawData[i + 1] = (byte)((((int)readI & (int)header.pixelFormat.gbitmask) >> (int)greenR) << (int)greenL);
                        rawData[i + 2] = (byte)((((int)readI & header.pixelFormat.bbitmask) >> (int)blueR) << (int)blueL);

                        if (bpp == 4)
                        {
                            rawData[i + 3] = (byte)((((int)readI & (int)header.pixelFormat.alphabitmask) >> (int)alphaR) << (int)alphaL);
                            if (alphaL >= 7)
                            {
                                rawData[i + 3] = (byte)(rawData[i + 3] != 0 ? 0xFF : 0x00);
                            }
                            else if (alphaL >= 4)
                            {
                                rawData[i + 3] = (byte)(rawData[i + 3] | (rawData[i + 3] >> 4));
                            }
                        }
                    }
                    else if (bpp == 2)
                    {
                        rawData[i + 1] = (byte)((((int)readI & (int)header.pixelFormat.alphabitmask) >> (int)alphaR) << (int)alphaL);
                        if (alphaL >= 7)
                        {
                            rawData[i + 1] = (byte)(rawData[i + 1] != 0 ? 0xFF : 0x00);
                        }
                        else if (alphaL >= 4)
                        {
                            rawData[i + 1] = (byte)(rawData[i + 1] | (rawData[i + 3] >> 4));
                        }
                    }
                }
            }
            return rawData;
        }

        private unsafe byte[] DecompressARGB16(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            // allocate bitmap
            int bpp = PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount);
            int bps = header.width * bpp * PixelFormatToBpc(pixelFormat);
            int sizeofplane = bps * header.height;

            int sizeOfData = (header.width * header.pixelFormat.rgbbitcount / 8) * header.height * header.depth;
            byte[] rawData = new byte[header.depth * sizeofplane + header.height * bps + header.width * bpp];

            uint readI = 0;
            uint redL, redR;
            uint greenL, greenR;
            uint blueL, blueR;
            uint alphaL, alphaR;
            
            GetBitsFromMask(header.pixelFormat.rbitmask, out redL, out redR);
            GetBitsFromMask(header.pixelFormat.gbitmask, out greenL, out greenR);
            GetBitsFromMask(header.pixelFormat.bbitmask, out blueL, out blueR);
            GetBitsFromMask(header.pixelFormat.alphabitmask, out alphaL, out alphaR);
            // padding
            redL += 16 - CountBitsFromMask(header.pixelFormat.rbitmask);
            greenL += 16 - CountBitsFromMask(header.pixelFormat.gbitmask);
            blueL += 16 - CountBitsFromMask(header.pixelFormat.bbitmask);
            alphaL += 16 - CountBitsFromMask(header.pixelFormat.alphabitmask);

            uint tempBpp = (uint)(header.pixelFormat.rgbbitcount / 8);
            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                fixed (byte* destPtr = rawData)
                {
                    byte* destData = destPtr;
                    for (int i = 0; i < sizeOfData / 2; i += bpp)
                    {
                        //@TODO: This is SLOOOW...
                        //but the old version crashed in release build under
                        //winxp (and xp is right to stop this code - I always
                        //wondered that it worked the old way at all)
                        if (sizeOfData - i < 4)
                        {
                            //less than 4 byte to write?
                            if (tempBpp == 3)
                            {
                                //this branch is extra-SLOOOW
                                readI = (uint)(*temp | ((*(temp + 1)) << 8) | ((*(temp + 2)) << 16));
                            }
                            else if (tempBpp == 1)
                                readI = *temp;
                            else if (tempBpp == 2)
                                readI = (uint)(temp[0] | (temp[1] << 8));
                        }
                        else
                            readI = (uint)(temp[0] | (temp[1] << 8) | (temp[2] << 16) | (temp[3] << 24));
                        temp += tempBpp;

                        ((ushort*)destData)[i + 2] = (ushort)((((int)readI & (int)header.pixelFormat.rbitmask) >> (int)redR) << (int)redL);

                        if (bpp >= 3)
                        {
                            ((ushort*)destData)[i + 1] = (ushort)((((int)readI & (int)header.pixelFormat.gbitmask) >> (int)greenR) << (int)greenL);
                            ((ushort*)destData)[i] = (ushort)((((int)readI & (int)header.pixelFormat.bbitmask) >> (int)blueR) << (int)blueL);

                            if (bpp == 4)
                            {
                                ((ushort*)destData)[i + 3] = (ushort)((((int)readI & (int)header.pixelFormat.alphabitmask) >> (int)alphaR) << (int)alphaL);
                                if (alphaL >= 7)
                                {
                                    ((ushort*)destData)[i + 3] = (ushort)(((ushort*)destData)[i + 3] != 0 ? 0xFF : 0x00);
                                }
                                else if (alphaL >= 4)
                                {
                                    ((ushort*)destData)[i + 3] = (ushort)(((ushort*)destData)[i + 3] | (((ushort*)destData)[i + 3] >> 4));
                                }
                            }
                        }
                        else if (bpp == 2)
                        {
                            ((ushort*)destData)[i + 1] = (ushort)((((int)readI & (int)header.pixelFormat.alphabitmask) >> (int)alphaR) << (int)alphaL);
                            if (alphaL >= 7)
                            {
                                ((ushort*)destData)[i + 1] = (ushort)(((ushort*)destData)[i + 1] != 0 ? 0xFF : 0x00);
                            }
                            else if (alphaL >= 4)
                            {
                                ((ushort*)destData)[i + 1] = (ushort)(((ushort*)destData)[i + 1] | (rawData[i + 3] >> 4));
                            }
                        }
                    }
                }
            }
            return rawData;
        }
        
        private const int DDSD_CAPS = 0x00000001;
        private const int DDSD_HEIGHT = 0x00000002;
        private const int DDSD_WIDTH = 0x00000004;
        private const int DDSD_PITCH = 0x00000008;
        private const int DDSD_PIXELFORMAT = 0x00001000;
        private const int DDSD_MIPMAPCOUNT = 0x00020000;
        private const int DDSD_LINEARSIZE = 0x00080000;
        private const int DDSD_DEPTH = 0x00800000;

        private const int DDPF_ALPHAPIXELS = 0x00000001;
        private const int DDPF_FOURCC = 0x00000004;
        private const int DDPF_RGB = 0x00000040;
        private const int DDPF_LUMINANCE = 0x00020000;

        // caps1
        private const int DDSCAPS_COMPLEX = 0x00000008;
        private const int DDSCAPS_TEXTURE = 0x00001000;
        private const int DDSCAPS_MIPMAP = 0x00400000;
        // caps2
        private const int DDSCAPS2_CUBEMAP = 0x00000200;
        private const int DDSCAPS2_CUBEMAP_POSITIVEX = 0x00000400;
        private const int DDSCAPS2_CUBEMAP_NEGATIVEX = 0x00000800;
        private const int DDSCAPS2_CUBEMAP_POSITIVEY = 0x00001000;
        private const int DDSCAPS2_CUBEMAP_NEGATIVEY = 0x00002000;
        private const int DDSCAPS2_CUBEMAP_POSITIVEZ = 0x00004000;
        private const int DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x00008000;
        private const int DDSCAPS2_VOLUME = 0x00200000;
    }
}
