/*
 * Copyright (C) 1999, 2000 NVIDIA Corporation
 * This file is provided without support, instruction, or implied warranty of any
 * kind.  NVIDIA makes no guarantee of its fitness for a particular purpose and is
 * not liable under any circumstances for any damages or loss whatsoever arising
 * from the use or inability to use this file or items derived from it.
 *
 * Converted to C#, assorted changes to make compatible with openBVE texture loading
 * Also some minor enum conversion & cleanup
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using OpenBveApi.Colors;
using OpenBveApi.Textures;

namespace Plugin
{
    public class DDSImage
    {
	    internal Texture myTexture;
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
            DdsHeader header = new DdsHeader();
            PixelFormat pixelFormat;
            byte[] data = null;

            if (this.ReadHeader(reader, ref header))
            {
                if (header.depth == 0) header.depth = 1;

                int blocksize;
                pixelFormat = this.GetFormat(header, out blocksize);
                data = this.ReadData(reader, header);
                if (data != null)
                {
                    byte[] rawData = this.DecompressData(header, data, pixelFormat);
                    CreateTexture(header.width, header.height, rawData);
                }
            }
        }

        private byte[] ReadData(BinaryReader reader, DdsHeader header)
        {
            byte[] compdata = null;
            int compsize = 0;

            if ((header.flags & DDSD_LINEARSIZE) > 1)
            {
                compdata = reader.ReadBytes((int)header.sizeorpitch);
                compsize = compdata.Length;
            }
            else
            {
                int bps = header.width * header.pixelFormat.rgbbitcount / 8;
                compsize = bps * header.height * header.depth;
                compdata = new byte[compsize];

                MemoryStream mem = new MemoryStream((int)compsize);

                byte[] temp;
                for (int z = 0; z < header.depth; z++)
                {
                    for (int y = 0; y < header.height; y++)
                    {
                        temp = reader.ReadBytes((int)bps);
                        mem.Write(temp, 0, temp.Length);
                    }
                }
                mem.Seek(0, SeekOrigin.Begin);

                mem.Read(compdata, 0, compdata.Length);
                mem.Close();
            }

            return compdata;
        }

        private void CreateTexture(int width, int height, byte[] rawData)
        {
            
            int size = width * height * 4;
			byte[] textureData = new byte[size];
	        for (int i = 0; i < size; i += 4)
	        {
		        textureData[i] = rawData[i + 2]; // blue
		        textureData[i + 1] = rawData[i + 1]; // green
		        textureData[i + 2] = rawData[i];   // red
		        textureData[i + 3] = rawData[i + 3];
	        }
	        myTexture = new Texture(width, height, 32, textureData, null);
        }

        private bool ReadHeader(BinaryReader reader, ref DdsHeader header)
        {
            byte[] signature = reader.ReadBytes(4);
	        if (!(signature[0] == 'D' && signature[1] == 'D' && signature[2] == 'S' && signature[3] == ' '))
	        {
		        throw new InvalidDataException("DDS Header invalid.");
	        }

	        if (reader.ReadUInt32() != 124)
	        {
		        throw new InvalidDataException("DDS Header size invalid.");
	        }

	        header.flags = reader.ReadUInt32();
            header.height = (int)reader.ReadUInt32();
            header.width = (int)reader.ReadUInt32();
            header.sizeorpitch = reader.ReadUInt32();
            header.depth = (int)reader.ReadUInt32();
            header.mipmapcount = reader.ReadUInt32();
            header.alphabitdepth = reader.ReadUInt32();

            for (int i = 0; i < 10; i++)
            {
                reader.ReadUInt32();	//Reserved 10 DWORD values : Microsoft documentation states unused
            }
	        if (reader.ReadUInt32() != 32)
	        {
		        throw new InvalidDataException("Pixel Format size invalid.");
	        }
            header.pixelFormat.flags = reader.ReadUInt32();
            header.pixelFormat.fourcc = (FourCC)reader.ReadUInt32();
            header.pixelFormat.rgbbitcount = (int)reader.ReadUInt32();
            header.pixelFormat.rbitmask = reader.ReadUInt32();
            header.pixelFormat.gbitmask = reader.ReadUInt32();
            header.pixelFormat.bbitmask = reader.ReadUInt32();
            header.pixelFormat.alphabitmask = reader.ReadUInt32();
            header.ddscaps.caps1 = reader.ReadUInt32();
            header.ddscaps.caps2 = reader.ReadUInt32();
            header.ddscaps.caps3 = reader.ReadUInt32();
            header.ddscaps.caps4 = reader.ReadUInt32();
            header.texturestage = reader.ReadUInt32();
            return true;
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
                    if ((header.pixelFormat.flags & DDPF_ALPHAPIXELS) == DDPF_ALPHAPIXELS)
                    {
                        format = PixelFormat.LUMINANCE_ALPHA;
                    }
                    else
                    {
                        format = PixelFormat.LUMINANCE;
                    }
                }
                else
                {
                    if ((header.pixelFormat.flags & DDPF_ALPHAPIXELS) == DDPF_ALPHAPIXELS)
                    {
                        format = PixelFormat.RGBA;
                    }
                    else
                    {
                        format = PixelFormat.RGB;
                    }
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

        private bool Check16BitComponents(DdsHeader header)
        {
            if (header.pixelFormat.rgbbitcount != 32)
                return false;
            // a2b10g10r10 format
            if (header.pixelFormat.rbitmask == 0x3FF00000 && header.pixelFormat.gbitmask == 0x000FFC00 && header.pixelFormat.bbitmask == 0x000003FF
                && header.pixelFormat.alphabitmask == 0xC0000000)
                return true;
            // a2r10g10b10 format
            else if (header.pixelFormat.rbitmask == 0x000003FF && header.pixelFormat.gbitmask == 0x000FFC00 && header.pixelFormat.bbitmask == 0x3FF00000
                && header.pixelFormat.alphabitmask == 0xC0000000)
                return true;

            return false;
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

        private void ComputeMaskParams(uint mask, ref int shift1, ref int mul, ref int shift2)
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
            byte r0, g0, b0, r1, g1, b1;

            b0 = (byte)(data[0] & 0x1F);
            g0 = (byte)(((data[0] & 0xE0) >> 5) | ((data[1] & 0x7) << 3));
            r0 = (byte)((data[1] & 0xF8) >> 3);

            b1 = (byte)(data[2] & 0x1F);
            g1 = (byte)(((data[2] & 0xE0) >> 5) | ((data[3] & 0x7) << 3));
            r1 = (byte)((data[3] & 0xF8) >> 3);

            op[0].R = (byte)(r0 << 3 | r0 >> 2);
            op[0].G = (byte)(g0 << 2 | g0 >> 3);
            op[0].B = (byte)(b0 << 3 | b0 >> 2);

            op[1].R = (byte)(r1 << 3 | r1 >> 2);
            op[1].G = (byte)(g1 << 2 | g1 >> 3);
            op[1].B = (byte)(b1 << 3 | b1 >> 2);
        }

        private void DxtcReadColor(ushort data, ref Color32 op)
        {
            byte r, g, b;

            b = (byte)(data & 0x1f);
            g = (byte)((data & 0x7E0) >> 5);
            r = (byte)((data & 0xF800) >> 11);

            op.R = (byte)(r << 3 | r >> 2);
            op.G = (byte)(g << 2 | g >> 3);
            op.B = (byte)(b << 3 | r >> 2);
        }

        private unsafe void DxtcReadColors(byte* data, ref Colour565 color_0, ref Colour565 color_1)
        {
            color_0.blue = (byte)(data[0] & 0x1F);
            color_0.green = (byte)(((data[0] & 0xE0) >> 5) | ((data[1] & 0x7) << 3));
            color_0.red = (byte)((data[1] & 0xF8) >> 3);

            color_0.blue = (byte)(data[2] & 0x1F);
            color_0.green = (byte)(((data[2] & 0xE0) >> 5) | ((data[3] & 0x7) << 3));
            color_0.red = (byte)((data[3] & 0xF8) >> 3);
        }

        private void GetBitsFromMask(uint mask, ref uint shiftLeft, ref uint shiftRight)
        {
            uint temp, i;

            if (mask == 0)
            {
                shiftLeft = shiftRight = 0;
                return;
            }

            temp = mask;
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
                else
                {
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
                else
                {
                    //
                    // Nan -- preserve sign and significand bits
                    //
                    return (uint)((s << 31) | 0x7f800000 | (m << 13));
                }
            }

            //
            // Normalized number
            //
            e = e + (127 - 15);
            m = m << 13;

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
            int bpp = PixelFormatToBpp(pixelFormat, (int)header.pixelFormat.rgbbitcount);
            int bps = (int)(header.width * bpp * PixelFormatToBpc(pixelFormat));
            int sizeofplane = (int)(bps * header.height);
            int width = (int)header.width;
            int height = (int)header.height;
            int depth = (int)header.depth;
            byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

            Color32[] colours = new Color32[4];
            colours[0].A = 0xFF;
            colours[1].A = 0xFF;
            colours[2].A = 0xFF;

            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y += 4)
                    {
                        for (int x = 0; x < width; x += 4)
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
                                //colours[2].A = 0xFF;

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
                                //colours[2].A = 0xFF;

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
                                    if (((x + i) < width) && ((y + j) < height))
                                    {
                                        uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
                                        rawData[offset + 0] = (byte)col.R;
                                        rawData[offset + 1] = (byte)col.G;
                                        rawData[offset + 2] = (byte)col.B;
                                        rawData[offset + 3] = (byte)col.A;
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
            int width = (int)header.width;
            int height = (int)header.height;
            int depth = (int)header.depth;
            byte[] rawData = DecompressDXT3(header, data, pixelFormat);
            CorrectPremult((uint)(width * height * depth), ref rawData);
			return rawData;
        }

        private unsafe byte[] DecompressDXT3(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            int bpp = (int)(PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount));
            int bps = (int)(header.width * bpp * PixelFormatToBpc(pixelFormat));
            int sizeofplane = (int)(bps * header.height);
            int width = (int)header.width;
            int height = (int)header.height;
            int depth = (int)header.depth;
            byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
            Color32[] colours = new Color32[4];
            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y += 4)
                    {
                        for (int x = 0; x < width; x += 4)
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
                            //colours[2].A = 0xFF;

                            colours[3].B = (byte)((colours[0].B + 2 * colours[1].B + 1) / 3);
                            colours[3].G = (byte)((colours[0].G + 2 * colours[1].G + 1) / 3);
                            colours[3].R = (byte)((colours[0].R + 2 * colours[1].R + 1) / 3);
                            //colours[3].A = 0xFF;

                            for (int j = 0, k = 0; j < 4; j++)
                            {
                                for (int i = 0; i < 4; k++, i++)
                                {
                                    int select = (int)((bitmask & (0x03 << k * 2)) >> k * 2);

                                    if (((x + i) < width) && ((y + j) < height))
                                    {
                                        uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
                                        rawData[offset + 0] = (byte)colours[select].R;
                                        rawData[offset + 1] = (byte)colours[select].G;
                                        rawData[offset + 2] = (byte)colours[select].B;
                                    }
                                }
                            }

                            for (int j = 0; j < 4; j++)
                            {
                                ushort word = (ushort)(alpha[2 * j] | (alpha[2 * j + 1] << 8)); 
                                for (int i = 0; i < 4; i++)
                                {
                                    if (((x + i) < width) && ((y + j) < height))
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
            int width = (int)header.width;
            int height = (int)header.height;
            int depth = (int)header.depth;
            byte[] rawData = DecompressDXT5(header, data, pixelFormat);
            CorrectPremult((uint)(width * height * depth), ref rawData);

            return rawData;
        }

        private unsafe byte[] DecompressDXT5(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            // allocate bitmap
            int bpp = (int)(PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount));
            int bps = (int)(header.width * bpp * PixelFormatToBpc(pixelFormat));
            int sizeofplane = (int)(bps * header.height);
            int width = (int)header.width;
            int height = (int)header.height;
            int depth = (int)header.depth;

            byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
            Color32[] colours = new Color32[4];
            ushort[] alphas = new ushort[8];

            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y += 4)
                    {
                        for (int x = 0; x < width; x += 4)
                        {
                            if (y >= height || x >= width)
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
                                    if (((x + i) < width) && ((y + j) < height))
                                    {
                                        uint offset = (uint)(z * sizeofplane + (y + j) * bps + (x + i) * bpp);
                                        rawData[offset] = (byte)col.R;
                                        rawData[offset + 1] = (byte)col.G;
                                        rawData[offset + 2] = (byte)col.B;
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
                                    if (((x + i) < width) && ((y + j) < height))
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
                                    if (((x + i) < width) && ((y + j) < height))
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
            int bpp = (int)(this.PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount));
            int bps = (int)(header.width * bpp * this.PixelFormatToBpc(pixelFormat));
            int sizeofplane = (int)(bps * header.height);
            int width = (int)header.width;
            int height = (int)header.height;
            int depth = (int)header.depth;

            byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

            uint valMask = (uint)((header.pixelFormat.rgbbitcount == 32) ? ~0 : (1 << (int)header.pixelFormat.rgbbitcount) - 1);
            uint pixSize = (uint)(((int)header.pixelFormat.rgbbitcount + 7) / 8);
            int rShift1 = 0; int rMul = 0; int rShift2 = 0;
            ComputeMaskParams(header.pixelFormat.rbitmask, ref rShift1, ref rMul, ref rShift2);
            int gShift1 = 0; int gMul = 0; int gShift2 = 0;
            ComputeMaskParams(header.pixelFormat.gbitmask, ref gShift1, ref gMul, ref gShift2);
            int bShift1 = 0; int bMul = 0; int bShift2= 0;
            ComputeMaskParams(header.pixelFormat.bbitmask, ref bShift1, ref bMul, ref bShift2);

            int offset = 0;
            int pixnum = width * height * depth;
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
            int bpp = (int)(this.PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount));
            int bps = (int)(header.width * bpp * this.PixelFormatToBpc(pixelFormat));
            int sizeofplane = (int)(bps * header.height);
            int width = (int)header.width;
            int height = (int)header.height;
            int depth = (int)header.depth;

            byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

            uint valMask = (uint)((header.pixelFormat.rgbbitcount == 32) ? ~0 : (1 << (int)header.pixelFormat.rgbbitcount) - 1);
            
            int pixSize = (header.pixelFormat.rgbbitcount + 7) / 8;
            int rShift1 = 0; int rMul = 0; int rShift2 = 0;
            ComputeMaskParams(header.pixelFormat.rbitmask, ref rShift1, ref rMul, ref rShift2);
            int gShift1 = 0; int gMul = 0; int gShift2 = 0;
            ComputeMaskParams(header.pixelFormat.gbitmask, ref gShift1, ref gMul, ref gShift2);
            int bShift1 = 0; int bMul = 0; int bShift2 = 0;
            ComputeMaskParams(header.pixelFormat.bbitmask, ref bShift1, ref bMul, ref bShift2);
            int aShift1 = 0; int aMul = 0; int aShift2 = 0;
            ComputeMaskParams(header.pixelFormat.alphabitmask, ref aShift1, ref aMul, ref aShift2);

            int offset = 0;
            int pixnum = width * height * depth;
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
            int bpp = (int)(this.PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount));
            int bps = (int)(header.width * bpp * this.PixelFormatToBpc(pixelFormat));
            int sizeofplane = (int)(bps * header.height);
            int width = (int)header.width;
            int height = (int)header.height;
            int depth = (int)header.depth;

            byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
            byte[] yColours = new byte[8];
            byte[] xColours = new byte[8];

            int offset = 0;
            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y += 4)
                    {
                        for (int x = 0; x < width; x += 4)
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
                                    if ((y + k + j) < height)
                                    {
                                        for (int i = 0; i < 4; i++)
                                        {
                                            // only put pixels out < width
                                            if (((x + i) < width))
                                            {
                                                int t;
                                                byte tx, ty;

                                                t1 = currentOffset + (x + i) * 3;
                                                rawData[t1 + 1] = ty = yColours[bitmask & 0x07];
                                                rawData[t1 + 0] = tx = xColours[bitmask2 & 0x07];

                                                //calculate b (z) component ((r/255)^2 + (g/255)^2 + (b/255)^2 = 1
                                                t = 127 * 128 - (tx - 127) * (tx - 128) - (ty - 127) * (ty - 128);
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
            int bpp = (int)(this.PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount));
            int bps = (int)(header.width * bpp * this.PixelFormatToBpc(pixelFormat));
            int sizeofplane = (int)(bps * header.height);
            int width = (int)header.width;
            int height = (int)header.height;
            int depth = (int)header.depth;

            byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
            byte[] colours = new byte[8];

            uint offset = 0;
            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y += 4)
                    {
                        for (int x = 0; x < width; x += 4)
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
                                    if ((y + k + j) < height)
                                    {
                                        for (int i = 0; i < 4; i++)
                                        {
                                            // only put pixels out < width
                                            if (((x + i) < width))
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
            int bpp = (int)(this.PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount));
            int bps = (int)(header.width * bpp * this.PixelFormatToBpc(pixelFormat));
            int sizeofplane = (int)(bps * header.height);
            int width = (int)header.width;
            int height = (int)header.height;
            int depth = (int)header.depth;

            byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

            int lShift1 = 0; int lMul = 0; int lShift2 = 0;
            ComputeMaskParams(header.pixelFormat.rbitmask, ref lShift1, ref lMul, ref lShift2);

            int offset = 0;
            int pixnum = width * height * depth;
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
            int bpp = (int)(this.PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount));
            int bps = (int)(header.width * bpp * this.PixelFormatToBpc(pixelFormat));
            int sizeofplane = (int)(bps * header.height);
            int width = (int)header.width;
            int height = (int)header.height;
            int depth = (int)header.depth;

            byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

            Colour565 color_0 = new Colour565();
            Colour565 color_1 = new Colour565();
	        Color32[]	colours = new Color32[4];
	        byte[] alphas = new byte[8];

            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                for (int z = 0; z < depth; z++)
                {
                    for (int y = 0; y < height; y += 4)
                    {
                        for (int x = 0; x < width; x += 4)
                        {
                            if (y >= height || x >= width)
                                break;
                            alphas[0] = temp[0];
                            alphas[1] = temp[1];
                            byte* alphamask = temp + 2;
                            temp += 8;

                            DxtcReadColors(temp, ref color_0, ref color_1);
                            temp += 4;

                            uint bitmask = ((uint*)temp)[1];
                            temp += 4;

                            colours[0].R = (byte)(color_0.red << 3);
                            colours[0].G = (byte)(color_0.green << 2);
                            colours[0].B = (byte)(color_0.blue << 3);
                            colours[0].A = 0xFF;

                            colours[1].R = (byte)(color_1.red << 3);
                            colours[1].G = (byte)(color_1.green << 2);
                            colours[1].B = (byte)(color_1.blue << 3);
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
                                    if (((x + i) < width) && ((y + j) < height))
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
                                    if (((x + i) < width) && ((y + j) < height))
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
                                    if (((x + i) < width) && ((y + j) < height))
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
            int bpp = (int)(this.PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount));
            int bps = (int)(header.width * bpp * this.PixelFormatToBpc(pixelFormat));
            int sizeofplane = (int)(bps * header.height);
            int width = (int)header.width;
            int height = (int)header.height;
            int depth = (int)header.depth;

            byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
            int size = 0;
            fixed (byte* bytePtr = data)
            {
                byte* temp = bytePtr;
                fixed (byte* destPtr = rawData)
                {
                    byte* destData = destPtr;
                    switch (pixelFormat)
                    {
                        case PixelFormat.R32F:  // Red float, green = blue = max
                            size = width * height * depth * 3;
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
                            size = width * height * depth * 3;
                            for (int i = 0, j = 0; i < size; i += 3, j += 2)
                            {
                                ((float*)destData)[i] = ((float*)temp)[j];
                                ((float*)destData)[i + 1] = ((float*)temp)[j + 1];
                                ((float*)destData)[i + 2] = 1.0f;
                            }
                            break;

                        case PixelFormat.R16F:  // Red float, green = blue = max
                            size = width * height * depth * bpp;
                            ConvR16ToFloat32((uint*)destData, (ushort*)temp, (uint)size);
                            break;

                        case PixelFormat.A16B16G16R16F:  // Just convert from half to float.
                            size = width * height * depth * bpp;
                            ConvFloat16ToFloat32((uint*)destData, (ushort*)temp, (uint)size);
                            break;

                        case PixelFormat.G16R16F:  // Convert from half to float, set blue = max.
                            size = width * height * depth * bpp;
                            ConvG16R16ToFloat32((uint*)destData, (ushort*)temp, (uint)size);
                            break;

                        default:
                            break;
                    }
                }
            }

            return rawData;
        }

        private unsafe byte[] DecompressARGB(DdsHeader header, byte[] data, PixelFormat pixelFormat)
        {
            // allocate bitmap
            int bpp = (int)(PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount));
            int bps = (int)(header.width * bpp * PixelFormatToBpc(pixelFormat));
            int sizeofplane = (int)(bps * header.height);
            int width = (int)header.width;
            int height = (int)header.height;
            int depth = (int)header.depth;

            if (Check16BitComponents(header))
                return DecompressARGB16(header, data, pixelFormat);

            int sizeOfData = (int)((header.width * header.pixelFormat.rgbbitcount / 8) * header.height * header.depth);
            byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

            if ((pixelFormat == PixelFormat.LUMINANCE) && (header.pixelFormat.rgbbitcount == 16) && (header.pixelFormat.rbitmask == 0xFFFF))
            {
                Array.Copy(data, rawData, data.Length);
                return rawData;
            }

            uint readI = 0, tempBpp;
            uint redL = 0, redR = 0;
            uint greenL = 0, greenR = 0;
            uint blueL = 0, blueR = 0;
            uint alphaL = 0, alphaR = 0;

            GetBitsFromMask(header.pixelFormat.rbitmask, ref redL, ref redR);
            GetBitsFromMask(header.pixelFormat.gbitmask, ref greenL, ref greenR);
            GetBitsFromMask(header.pixelFormat.bbitmask, ref blueL, ref blueR);
            GetBitsFromMask(header.pixelFormat.alphabitmask, ref alphaL, ref alphaR);
            tempBpp = (uint)(header.pixelFormat.rgbbitcount / 8);

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
                            readI = *((byte*)temp);
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
            int bpp = (int)(PixelFormatToBpp(pixelFormat, header.pixelFormat.rgbbitcount));
            int bps = (int)(header.width * bpp * PixelFormatToBpc(pixelFormat));
            int sizeofplane = (int)(bps * header.height);
            int width = (int)header.width;
            int height = (int)header.height;
            int depth = (int)header.depth;

            int sizeOfData = (int)((header.width * header.pixelFormat.rgbbitcount / 8) * header.height * header.depth);
            byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

            uint readI = 0, tempBpp  = 0;
            uint redL = 0, redR = 0;
            uint greenL = 0, greenR = 0;
            uint blueL = 0, blueR = 0;
            uint alphaL = 0, alphaR = 0;
            uint redPad = 0, greenPad = 0, bluePad = 0, alphaPad = 0;

            GetBitsFromMask(header.pixelFormat.rbitmask, ref redL, ref redR);
            GetBitsFromMask(header.pixelFormat.gbitmask, ref greenL, ref greenR);
            GetBitsFromMask(header.pixelFormat.bbitmask, ref blueL, ref blueR);
            GetBitsFromMask(header.pixelFormat.alphabitmask, ref alphaL, ref alphaR);
            redPad = 16 - CountBitsFromMask(header.pixelFormat.rbitmask);
            greenPad = 16 - CountBitsFromMask(header.pixelFormat.gbitmask);
            bluePad = 16 - CountBitsFromMask(header.pixelFormat.bbitmask);
            alphaPad = 16 - CountBitsFromMask(header.pixelFormat.alphabitmask);

            redL = redL + redPad;
            greenL = greenL + greenPad;
            blueL = blueL + bluePad;
            alphaL = alphaL + alphaPad;

            tempBpp = (uint)(header.pixelFormat.rgbbitcount / 8);
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
                                readI = *((byte*)temp);
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


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct Colour565
        {
            public ushort blue; //: 5;
            public ushort green; //: 6;
            public ushort red; //: 5;
        }

        private struct DdsHeader
        {
            public uint flags;
            public int height;
            public int width;
            public uint sizeorpitch;
            public int depth;
            public uint mipmapcount;
            public uint alphabitdepth;

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct PixelFormat
            {
                public uint flags;
                public FourCC fourcc;
                public int rgbbitcount;
                public uint rbitmask;
                public uint gbitmask;
                public uint bbitmask;
                public uint alphabitmask;
            }
            public PixelFormat pixelFormat;

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct ddscapsstruct
            {
                public uint caps1;
                public uint caps2;
                public uint caps3;
                public uint caps4;
            }
            public ddscapsstruct ddscaps;
            public uint texturestage;
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
