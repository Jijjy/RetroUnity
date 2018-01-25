using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RetroUnity.Utility.Rendering
{
    public interface IRenderPixels
    {
        void Render(ref Texture2D tex, IntPtr ptr, uint width, uint height, uint pitch);
    }

    public abstract class RenderPixelsBase : IRenderPixels
    {
        protected int w;
        protected int h;
        protected int p;

        public virtual void Render(ref Texture2D tex, IntPtr ptr, uint width, uint height, uint pitch)
        {
            w = Convert.ToInt32(width);
            h = Convert.ToInt32(height);

            if (tex == null)
                tex = new Texture2D(w, h, TextureFormat.RGB565, false);

            p = Convert.ToInt32(pitch);
        }
    }

    public abstract class SetPixelRenderer : RenderPixelsBase
    {
        protected abstract Color GetColor(IntPtr ptr);

        public override void Render(ref Texture2D tex, IntPtr ptr, uint width, uint height, uint pitch)
        {
            base.Render(ref tex, ptr, width, height, pitch);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    tex.SetPixel(i, j, GetColor(ptr));
                }
                tex.filterMode = FilterMode.Trilinear;
                tex.Apply();
            }
        }
    }

    public class RenderRetroPixelFormat_0RGB1555 : SetPixelRenderer
    {
        protected override Color GetColor(IntPtr ptr)
        {
            short packed = Marshal.ReadInt16(ptr);
            return new Color(((packed >> 10) & 0x001F) / 31.0f, ((packed >> 5) & 0x001F) / 31.0f, (packed & 0x001F) / 31.0f, 1.0f);
        }
    }

    public class RenderRetroPixelFormatXRGB8888 : SetPixelRenderer
    {
        protected override Color GetColor(IntPtr ptr)
        {
            int packed = Marshal.ReadInt32(ptr);
            return new Color(((packed >> 16) & 0x00FF) / 255.0f, ((packed >> 8) & 0x00FF) / 255.0f, (packed & 0x00FF) / 255.0f, 1.0f);
        }
    }

    public class RenderRetroPixelFormatRGB565 : RenderPixelsBase
    {
        byte[] _src;
        byte[] _dst;

        public override void Render(ref Texture2D tex, IntPtr ptr, uint width, uint height, uint pitch)
        {
            base.Render(ref tex, ptr, width, height, pitch);

            int srcsize565 = 2 * (p * h);
            int dstsize565 = 2 * (w * h);

            if (_src == null || _src.Length != srcsize565)
                _src = new byte[srcsize565];

            if (_dst == null || _dst.Length != dstsize565)
                _dst = new byte[dstsize565];

            Marshal.Copy(ptr, _src, 0, srcsize565);

            int m565 = 0;
            for (int y = 0; y < h; y++)
            {
                for (int k = 0 * 2 + y * p; k < w * 2 + y * p; k++)
                {
                    _dst[m565] = _src[k];
                    m565++;
                }
            }
            tex.LoadRawTextureData(_dst);
            tex.filterMode = FilterMode.Trilinear;
            tex.Apply();
        }

    }
}
