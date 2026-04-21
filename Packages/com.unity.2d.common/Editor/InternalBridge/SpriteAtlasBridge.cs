using UnityEngine;
using UnityEngine.U2D;

namespace UnityEditor.U2D.Common
{
    static class SpriteAtlasBridge
    {
        public static Texture2D[] GetSpriteAtlasTextures(SpriteAtlas spriteAtlas)
        {
            return spriteAtlas?.GetPreviewTextures();
        }

        public static Texture2D GetSpriteTexture(Sprite sprite, bool fromAtlas)
        {
            return sprite?.texture;
        }

        public static TextureFormat GetSpriteAtlasTextureFormat(SpriteAtlas spriteAtlas, BuildTarget target)
        {
            return spriteAtlas?.GetTextureFormat(target) ?? TextureFormat.RGBA32;
        }
    }
}
