/**
 * URPMaterialHelper.cs
 * URP 材质帮助类
 * 
 * 负责：
 * - 加载和缓存 URP 2D 材质
 * - 为 SpriteRenderer 和 TilemapRenderer 设置正确的材质
 */

using UnityEngine;
using UnityEngine.Tilemaps;

namespace GDFramework.MapSystem.Rendering
{
    /// <summary>
    /// URP 材质帮助类
    /// </summary>
    public static class URPMaterialHelper
    {
        #region 缓存的材质
        
        private static Material _spriteLitMaterial;
        private static Material _spriteUnlitMaterial;
        private static Material _spriteMaskMaterial;
        
        private static bool _isInitialized;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// URP Sprite Lit 材质（支持 2D 光照）
        /// </summary>
        public static Material SpriteLitMaterial
        {
            get
            {
                EnsureInitialized();
                return _spriteLitMaterial;
            }
        }
        
        /// <summary>
        /// URP Sprite Unlit 材质（无光照）
        /// </summary>
        public static Material SpriteUnlitMaterial
        {
            get
            {
                EnsureInitialized();
                return _spriteUnlitMaterial;
            }
        }
        
        /// <summary>
        /// URP Sprite Mask 材质
        /// </summary>
        public static Material SpriteMaskMaterial
        {
            get
            {
                EnsureInitialized();
                return _spriteMaskMaterial;
            }
        }
        
        #endregion
        
        #region 初始化
        
        /// <summary>
        /// 确保已初始化
        /// </summary>
        private static void EnsureInitialized()
        {
            if (_isInitialized) return;
            
            Initialize();
        }
        
        /// <summary>
        /// 初始化材质
        /// </summary>
        public static void Initialize()
        {
            // 方法1：从 Resources 加载（需要将材质放到 Resources 文件夹）
            _spriteLitMaterial = Resources.Load<Material>("Materials/Sprite-Lit-Default");
            _spriteUnlitMaterial = Resources.Load<Material>("Materials/Sprite-Unlit-Default");
            
            // 方法2：如果 Resources 中没有，尝试查找默认材质
            if (_spriteLitMaterial == null)
            {
                _spriteLitMaterial = FindURPSpriteMaterial("Sprite-Lit-Default");
            }
            
            if (_spriteUnlitMaterial == null)
            {
                _spriteUnlitMaterial = FindURPSpriteMaterial("Sprite-Unlit-Default");
            }
            
            // 方法3：如果还是找不到，创建默认的 Sprite 材质
            if (_spriteLitMaterial == null)
            {
                _spriteLitMaterial = CreateDefaultSpriteMaterial(true);
            }
            
            if (_spriteUnlitMaterial == null)
            {
                _spriteUnlitMaterial = CreateDefaultSpriteMaterial(false);
            }
            
            _isInitialized = true;
            
            Debug.Log($"[URPMaterialHelper] 初始化完成: " +
                      $"Lit={(_spriteLitMaterial != null ? _spriteLitMaterial.name : "null")}, " +
                      $"Unlit={(_spriteUnlitMaterial != null ? _spriteUnlitMaterial.name : "null")}");
        }
        
        /// <summary>
        /// 查找 URP Sprite 材质
        /// </summary>
        private static Material FindURPSpriteMaterial(string materialName)
        {
            // 尝试通过 Shader.Find 创建材质
            Shader shader = null;
            
            if (materialName.Contains("Lit"))
            {
                // URP 2D Lit Shader
                shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
                if (shader == null)
                {
                    shader = Shader.Find("Sprites/Default"); // 回退
                }
            }
            else
            {
                // URP 2D Unlit Shader
                shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader == null)
                {
                    shader = Shader.Find("Sprites/Default"); // 回退
                }
            }
            
            if (shader != null)
            {
                Material mat = new Material(shader);
                mat.name = materialName;
                return mat;
            }
            
            return null;
        }
        
        /// <summary>
        /// 创建默认 Sprite 材质
        /// </summary>
        private static Material CreateDefaultSpriteMaterial(bool lit)
        {
            // 使用最基本的 Sprites/Default shader
            Shader shader = Shader.Find("Sprites/Default");
            
            if (shader == null)
            {
                Debug.LogError("[URPMaterialHelper] 无法找到任何可用的 Sprite Shader!");
                return null;
            }
            
            Material mat = new Material(shader);
            mat.name = lit ? "Sprite-Lit-Fallback" : "Sprite-Unlit-Fallback";
            return mat;
        }
        
        #endregion
        
        #region 应用材质
        
        /// <summary>
        /// 为 SpriteRenderer 设置 URP 材质
        /// </summary>
        /// <param name="renderer">SpriteRenderer</param>
        /// <param name="useLighting">是否使用光照</param>
        public static void SetupSpriteRenderer(SpriteRenderer renderer, bool useLighting = true)
        {
            if (renderer == null) return;
            
            EnsureInitialized();
            
            renderer.sharedMaterial = useLighting ? _spriteLitMaterial : _spriteUnlitMaterial;
        }
        
        /// <summary>
        /// 为 TilemapRenderer 设置 URP 材质
        /// </summary>
        /// <param name="renderer">TilemapRenderer</param>
        /// <param name="useLighting">是否使用光照</param>
        public static void SetupTilemapRenderer(TilemapRenderer renderer, bool useLighting = true)
        {
            if (renderer == null) return;
            
            EnsureInitialized();
            
            renderer.sharedMaterial = useLighting ? _spriteLitMaterial : _spriteUnlitMaterial;
        }
        
        /// <summary>
        /// 批量设置多个 TilemapRenderer
        /// </summary>
        public static void SetupTilemapRenderers(TilemapRenderer[] renderers, bool useLighting = true)
        {
            if (renderers == null) return;
            
            foreach (var renderer in renderers)
            {
                SetupTilemapRenderer(renderer, useLighting);
            }
        }
        
        #endregion
        
        #region 手动设置材质
        
        /// <summary>
        /// 手动设置 Lit 材质（用于从外部注入）
        /// </summary>
        public static void SetSpriteLitMaterial(Material material)
        {
            if (material != null)
            {
                _spriteLitMaterial = material;
            }
        }
        
        /// <summary>
        /// 手动设置 Unlit 材质（用于从外部注入）
        /// </summary>
        public static void SetSpriteUnlitMaterial(Material material)
        {
            if (material != null)
            {
                _spriteUnlitMaterial = material;
            }
        }
        
        #endregion
        
        #region 清理
        
        /// <summary>
        /// 清理缓存的材质
        /// </summary>
        public static void Cleanup()
        {
            // 注意：不要销毁共享材质，因为可能还有其他对象在使用
            _spriteLitMaterial = null;
            _spriteUnlitMaterial = null;
            _spriteMaskMaterial = null;
            _isInitialized = false;
        }
        
        #endregion
    }
}
