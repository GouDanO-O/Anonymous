using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ES3Internal;
#if UNITY_2018_3_OR_NEWER
using UnityEngine.Networking;
#endif

/// <summary>
/// Easy Save 方法的主类。此类中的所有方法都是静态的。
/// </summary>
#if UNITY_VISUAL_SCRIPTING
[Unity.VisualScripting.IncludeInSettings(true)]
#elif BOLT_VISUAL_SCRIPTING
[Ludiq.IncludeInSettings(true)]
#endif
public class ES3
{
    // 枚举定义：存储位置、目录、加密类型、压缩类型、格式、引用模式、图片类型
    public enum Location
    {
        File,
        PlayerPrefs,
        InternalMS,
        Resources,
        Cache
    };

    public enum Directory
    {
        PersistentDataPath,
        DataPath
    }

    public enum EncryptionType
    {
        None,
        AES
    };

    public enum CompressionType
    {
        None,
        Gzip
    };

    public enum Format
    {
        JSON
    };

    public enum ReferenceMode
    {
        ByRef,
        ByValue,
        ByRefAndValue
    };

    public enum ImageType
    {
        JPEG,
        PNG
    };

    #region ES3.Save

    /// <summary>将值保存到默认文件中指定的键下。</summary>
    /// <param name="key">用于在文件中标识值的键。</param>
    /// <param name="value">要保存的值。</param>
    public static void Save(string key, object value)
    {
        Save<object>(key, value, new ES3Settings());
    }

    /// <summary>将值保存到指定文件路径中指定的键下。</summary>
    /// <param name="key">用于在文件中标识值的键。</param>
    /// <param name="value">要保存的值。</param>
    /// <param name="filePath">要存储值的文件的相对或绝对路径。</param>
    public static void Save(string key, object value, string filePath)
    {
        Save<object>(key, value, new ES3Settings(filePath));
    }

    /// <summary>将值保存到指定文件路径中指定的键下，使用提供的设置覆盖默认设置。</summary>
    /// <param name="key">用于在文件中标识值的键。</param>
    /// <param name="value">要保存的值。</param>
    /// <param name="filePath">要存储值的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void Save(string key, object value, string filePath, ES3Settings settings)
    {
        Save<object>(key, value, new ES3Settings(filePath, settings));
    }

    /// <summary>将值保存到指定设置定义的文件中指定的键下。</summary>
    /// <param name="key">用于在文件中标识值的键。</param>
    /// <param name="value">要保存的值。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void Save(string key, object value, ES3Settings settings)
    {
        Save<object>(key, value, settings);
    }

    /// <summary>（泛型）将值保存到默认文件中指定的键下。</summary>
    /// <param name="T">要保存的数据类型。</param>
    /// <param name="key">用于在文件中标识值的键。</param>
    /// <param name="value">要保存的值。</param>
    public static void Save<T>(string key, T value)
    {
        Save<T>(key, value, new ES3Settings());
    }

    /// <summary>（泛型）将值保存到指定文件路径中指定的键下。</summary>
    /// <param name="T">要保存的数据类型。</param>
    /// <param name="key">用于在文件中标识值的键。</param>
    /// <param name="value">要保存的值。</param>
    /// <param name="filePath">要存储值的文件的相对或绝对路径。</param>
    public static void Save<T>(string key, T value, string filePath)
    {
        Save<T>(key, value, new ES3Settings(filePath));
    }

    /// <summary>（泛型）将值保存到指定文件路径中指定的键下，使用提供的设置覆盖默认设置。</summary>
    /// <param name="T">要保存的数据类型。</param>
    /// <param name="key">用于在文件中标识值的键。</param>
    /// <param name="value">要保存的值。</param>
    /// <param name="filePath">要存储值的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void Save<T>(string key, T value, string filePath, ES3Settings settings)
    {
        Save<T>(key, value, new ES3Settings(filePath, settings));
    }

    /// <summary>（泛型）将值保存到指定设置定义的文件中指定的键下。</summary>
    /// <param name="T">要保存的数据类型。</param>
    /// <param name="key">用于在文件中标识值的键。</param>
    /// <param name="value">要保存的值。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void Save<T>(string key, T value, ES3Settings settings)
    {
        if (settings.location == Location.Cache) // 如果是缓存位置
        {
            ES3File.GetOrCreateCachedFile(settings).Save<T>(key, value); // 使用缓存的ES3File对象保存
            return;
        }

        using (var writer = ES3Writer.Create(settings)) // 创建写入器
        {
            writer.Write<T>(key, value); // 写入键值对
            writer.Save(); // 保存到文件
        }
    }

    /// <summary>使用指定的原始字节创建或覆盖一个文件。</summary>
    /// <param name="bytes">要存储的字节数组。</param>
    public static void SaveRaw(byte[] bytes)
    {
        SaveRaw(bytes, new ES3Settings());
    }

    /// <summary>使用指定的原始字节创建或覆盖指定路径的文件。</summary>
    /// <param name="bytes">要存储的字节数组。</param>
    /// <param name="filePath">要存储字节的文件的相对或绝对路径。</param>
    public static void SaveRaw(byte[] bytes, string filePath)
    {
        SaveRaw(bytes, new ES3Settings(filePath));
    }

    /// <summary>使用指定的原始字节创建或覆盖指定路径的文件，使用提供的设置覆盖默认设置。</summary>
    /// <param name="bytes">要存储的字节数组。</param>
    /// <param name="filePath">要存储字节的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void SaveRaw(byte[] bytes, string filePath, ES3Settings settings)
    {
        SaveRaw(bytes, new ES3Settings(filePath, settings));
    }

    /// <summary>使用指定的原始字节创建或覆盖指定设置定义的文件。</summary>
    /// <param name="bytes">要存储的字节数组。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void SaveRaw(byte[] bytes, ES3Settings settings)
    {
        if (settings.location == Location.Cache) // 如果是缓存位置
        {
            ES3File.GetOrCreateCachedFile(settings).SaveRaw(bytes, settings); // 使用缓存的ES3File对象保存原始字节
            return;
        }

        using (var stream = ES3Stream.CreateStream(settings, ES3FileMode.Write)) // 创建写入流
        {
            stream.Write(bytes, 0, bytes.Length); // 写入字节
        }

        ES3IO.CommitBackup(settings); // 提交备份（如果有）
    }

    /// <summary>使用指定的字符串创建或覆盖默认文件。</summary>
    /// <param name="str">要存储的字符串。</param>
    public static void SaveRaw(string str)
    {
        SaveRaw(str, new ES3Settings());
    }

    /// <summary>使用指定的字符串创建或覆盖指定路径的文件。</summary>
    /// <param name="str">要存储的字符串。</param>
    /// <param name="filePath">要存储字符串的文件的相对或绝对路径。</param>
    public static void SaveRaw(string str, string filePath)
    {
        SaveRaw(str, new ES3Settings(filePath));
    }

    /// <summary>使用指定的字符串创建或覆盖指定路径的文件，使用提供的设置覆盖默认设置。</summary>
    /// <param name="str">要存储的字符串。</param>
    /// <param name="filePath">要存储字符串的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void SaveRaw(string str, string filePath, ES3Settings settings)
    {
        SaveRaw(str, new ES3Settings(filePath, settings));
    }

    /// <summary>使用指定的字符串创建或覆盖指定设置定义的文件。</summary>
    /// <param name="str">要存储的字符串。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void SaveRaw(string str, ES3Settings settings)
    {
        var bytes = settings.encoding.GetBytes(str); // 将字符串转换为字节
        SaveRaw(bytes, settings); // 调用字节保存方法
    }

    /// <summary>将指定的字节追加到默认文件，如果文件不存在则创建。</summary>
    /// <param name="bytes">要追加的字节数组。</param>
    public static void AppendRaw(byte[] bytes)
    {
        AppendRaw(bytes, new ES3Settings());
    }

    /// <summary>将指定的字节追加到指定路径的文件，如果文件不存在则创建。</summary>
    /// <param name="bytes">要追加的字节数组。</param>
    /// <param name="filePath">要追加字节的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void AppendRaw(byte[] bytes, string filePath, ES3Settings settings)
    {
        AppendRaw(bytes, new ES3Settings(filePath, settings));
    }

    /// <summary>将指定的字节追加到指定设置定义的文件，如果文件不存在则创建。</summary>
    /// <param name="bytes">要追加的字节数组。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void AppendRaw(byte[] bytes, ES3Settings settings)
    {
        if (settings.location == Location.Cache) // 如果是缓存位置
        {
            ES3File.GetOrCreateCachedFile(settings).AppendRaw(bytes); // 使用缓存的ES3File对象追加
            return;
        }

        // 对于追加，强制不使用加密和压缩
        ES3Settings newSettings = new ES3Settings(settings.path, settings);
        newSettings.encryptionType = EncryptionType.None;
        newSettings.compressionType = CompressionType.None;

        using (var stream = ES3Stream.CreateStream(newSettings, ES3FileMode.Append)) // 创建追加模式的流
            stream.Write(bytes, 0, bytes.Length); // 写入字节
    }

    /// <summary>将指定的字符串追加到默认文件，如果文件不存在则创建。</summary>
    /// <param name="str">要追加的字符串。</param>
    public static void AppendRaw(string str)
    {
        AppendRaw(str, new ES3Settings());
    }

    /// <summary>将指定的字符串追加到指定路径的文件，如果文件不存在则创建。</summary>
    /// <param name="str">要追加的字符串。</param>
    /// <param name="filePath">要追加字符串的文件的相对或绝对路径。</param>
    public static void AppendRaw(string str, string filePath)
    {
        AppendRaw(str, new ES3Settings(filePath));
    }

    /// <summary>将指定的字符串追加到指定路径的文件，如果文件不存在则创建，使用提供的设置覆盖默认设置。</summary>
    /// <param name="str">要追加的字符串。</param>
    /// <param name="filePath">要追加字符串的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void AppendRaw(string str, string filePath, ES3Settings settings)
    {
        AppendRaw(str, new ES3Settings(filePath, settings));
    }

    /// <summary>将指定的字符串追加到指定设置定义的文件，如果文件不存在则创建。</summary>
    /// <param name="str">要追加的字符串。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void AppendRaw(string str, ES3Settings settings)
    {
        var bytes = settings.encoding.GetBytes(str); // 将字符串转换为字节
        // 对于追加，强制不使用加密和压缩
        ES3Settings newSettings = new ES3Settings(settings.path, settings);
        newSettings.encryptionType = EncryptionType.None;
        newSettings.compressionType = CompressionType.None;

        if (settings.location == Location.Cache) // 如果是缓存位置
        {
            ES3File.GetOrCreateCachedFile(settings).SaveRaw(bytes); // 使用缓存的ES3File对象保存（追加在缓存中通常就是覆盖）
            return;
        }

        using (var stream = ES3Stream.CreateStream(newSettings, ES3FileMode.Append)) // 创建追加模式的流
            stream.Write(bytes, 0, bytes.Length); // 写入字节
    }

    /// <summary>将Texture2D保存为PNG或JPG，具体取决于文件路径使用的扩展名。</summary>
    /// <param name="texture">要保存为JPG或PNG的Texture2D。</param>
    /// <param name="imagePath">要创建的PNG或JPG文件的相对或绝对路径。</param>
    public static void SaveImage(Texture2D texture, string imagePath)
    {
        SaveImage(texture, new ES3Settings(imagePath));
    }

    /// <summary>将Texture2D保存为PNG或JPG，具体取决于文件路径使用的扩展名。</summary>
    /// <param name="texture">要保存为JPG或PNG的Texture2D。</param>
    /// <param name="imagePath">要创建的PNG或JPG文件的相对或绝对路径。</param>
    public static void SaveImage(Texture2D texture, string imagePath, ES3Settings settings)
    {
        SaveImage(texture, new ES3Settings(imagePath, settings));
    }

    /// <summary>将Texture2D保存为PNG或JPG，具体取决于文件路径使用的扩展名。</summary>
    /// <param name="texture">要保存为JPG或PNG的Texture2D。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void SaveImage(Texture2D texture, ES3Settings settings)
    {
        SaveImage(texture, 75, settings); // 使用默认质量75
    }

    /// <summary>将Texture2D保存为PNG或JPG，具体取决于文件路径使用的扩展名。</summary>
    /// <param name="texture">要保存为JPG或PNG的Texture2D。</param>
    /// <param name="quality">编码质量，1为最低，100为最高。注意：这仅适用于JPG。</param>
    /// <param name="imagePath">要创建的PNG或JPG文件的相对或绝对路径。</param>
    public static void SaveImage(Texture2D texture, int quality, string imagePath)
    {
        SaveImage(texture, quality, new ES3Settings(imagePath));
    }

    /// <summary>将Texture2D保存为PNG或JPG，具体取决于文件路径使用的扩展名。</summary>
    /// <param name="texture">要保存为JPG或PNG的Texture2D。</param>
    /// <param name="quality">编码质量，1为最低，100为最高。注意：这仅适用于JPG。</param>
    /// <param name="imagePath">要创建的PNG或JPG文件的相对或绝对路径。</param>
    public static void SaveImage(Texture2D texture, int quality, string imagePath, ES3Settings settings)
    {
        SaveImage(texture, quality, new ES3Settings(imagePath, settings));
    }

    /// <summary>将Texture2D保存为PNG或JPG，具体取决于文件路径使用的扩展名。</summary>
    /// <param name="texture">要保存为JPG或PNG的Texture2D。</param>
    /// <param name="quality">编码质量，1为最低，100为最高。注意：这仅适用于JPG。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void SaveImage(Texture2D texture, int quality, ES3Settings settings)
    {
        // 获取文件扩展名以确定保存格式
        string extension = ES3IO.GetExtension(settings.path).ToLower();
        if (string.IsNullOrEmpty(extension))
            throw new System.ArgumentException("使用 ES3.SaveImage 时，文件路径必须具有文件扩展名。");
        byte[] bytes;
        if (extension == ".jpg" || extension == ".jpeg")
            bytes = texture.EncodeToJPG(quality); // 编码为JPG
        else if (extension == ".png")
            bytes = texture.EncodeToPNG(); // 编码为PNG
        else
            throw new System.ArgumentException("使用 ES3.SaveImage 时，文件路径必须具有 .png、.jpg 或 .jpeg 扩展名。");

        ES3.SaveRaw(bytes, settings); // 保存原始字节
    }

    /// <summary>将Texture2D保存为PNG或JPG字节数组。</summary>
    /// <param name="texture">要保存为JPG或PNG的Texture2D。</param>
    /// <param name="quality">编码质量，1为最低，100为最高。注意：这仅适用于JPG。</param>
    /// <param name="imageType">图片类型 (JPEG 或 PNG)。</param>
    public static byte[] SaveImageToBytes(Texture2D texture, int quality, ES3.ImageType imageType)
    {
        if (imageType == ImageType.JPEG)
            return texture.EncodeToJPG(quality); // 返回JPG字节
        else
            return texture.EncodeToPNG(); // 返回PNG字节
    }

    #endregion

    #region ES3.Load<T>

    /* 标准加载方法 */

    /// <summary>从默认文件中指定键加载值。</summary>
    /// <param name="key">标识要加载的值的键。</param>
    public static object Load(string key)
    {
        return Load<object>(key, new ES3Settings());
    }

    /// <summary>从指定文件路径中指定键加载值。</summary>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="filePath">要从中加载的文件的相对或绝对路径。</param>
    public static object Load(string key, string filePath)
    {
        return Load<object>(key, new ES3Settings(filePath));
    }

    /// <summary>从指定文件路径中指定键加载值，使用提供的设置覆盖默认设置。</summary>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="filePath">要从中加载的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static object Load(string key, string filePath, ES3Settings settings)
    {
        return Load<object>(key, new ES3Settings(filePath, settings));
    }

    /// <summary>从指定设置定义的文件中指定键加载值。</summary>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static object Load(string key, ES3Settings settings)
    {
        return Load<object>(key, settings);
    }

    /// <summary>从默认文件中指定键加载值。如果文件或键不存在，则返回默认值。</summary>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="defaultValue">如果文件或键不存在则返回的值。</param>
    public static object Load(string key, object defaultValue)
    {
        return Load<object>(key, defaultValue, new ES3Settings());
    }

    /// <summary>从指定文件路径中指定键加载值。如果文件或键不存在，则返回默认值。</summary>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="filePath">要从中加载的文件的相对或绝对路径。</param>
    /// <param name="defaultValue">如果文件或键不存在则返回的值。</param>
    public static object Load(string key, string filePath, object defaultValue)
    {
        return Load<object>(key, defaultValue, new ES3Settings(filePath));
    }

    /// <summary>从指定文件路径中指定键加载值，使用提供的设置覆盖默认设置。如果文件或键不存在，则返回默认值。</summary>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="filePath">要从中加载的文件的相对或绝对路径。</param>
    /// <param name="defaultValue">如果文件或键不存在则返回的值。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static object Load(string key, string filePath, object defaultValue, ES3Settings settings)
    {
        return Load<object>(key, defaultValue, new ES3Settings(filePath, settings));
    }

    /// <summary>从指定设置定义的文件中指定键加载值。如果文件或键不存在，则返回默认值。</summary>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="defaultValue">如果文件或键不存在则返回的值。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static object Load(string key, object defaultValue, ES3Settings settings)
    {
        return Load<object>(key, defaultValue, settings);
    }

    /// <summary>（泛型）从默认文件中指定键加载值。</summary>
    /// <param name="T">要加载的数据类型。</param>
    /// <param name="key">标识要加载的值的键。</param>
    public static T Load<T>(string key)
    {
        return Load<T>(key, new ES3Settings());
    }

    /// <summary>（泛型）从指定文件路径中指定键加载值。</summary>
    /// <param name="T">要加载的数据类型。</param>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="filePath">要从中加载的文件的相对或绝对路径。</param>
    public static T Load<T>(string key, string filePath)
    {
        return Load<T>(key, new ES3Settings(filePath));
    }

    /// <summary>（泛型）从指定文件路径中指定键加载值，使用提供的设置覆盖默认设置。</summary>
    /// <param name="T">要加载的数据类型。</param>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="filePath">要从中加载的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static T Load<T>(string key, string filePath, ES3Settings settings)
    {
        if (typeof(T) == typeof(string)) // 特别警告：加载字符串时第二个参数(filePath)可能与defaultValue混淆
            ES3Debug.LogWarning(
                "使用 ES3.Load<string>(string, string) 加载字符串，但第二个参数在 defaultValue 和 filePath 之间不明确。默认情况下 C# 会假定第二个参数是 filePath。如果你希望第二个参数是 defaultValue，请使用命名参数。例如：ES3.Load<string>(\"key\", defaultValue: \"myDefaultValue\")");

        return Load<T>(key, new ES3Settings(filePath, settings));
    }

    /// <summary>（泛型）从指定设置定义的文件中指定键加载值。</summary>
    /// <param name="T">要加载的数据类型。</param>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static T Load<T>(string key, ES3Settings settings)
    {
        if (settings.location == Location.Cache) // 如果是缓存位置
        {
            var cachedFile = ES3File.GetCachedFile(settings); // 获取缓存的ES3File

            if (settings.autoCacheFileOnLoad && cachedFile == null) // 如果启用自动缓存且缓存不存在
                cachedFile = ES3File.CacheFile(settings); // 从存储加载文件到缓存

            if (cachedFile == null) // 缓存文件不存在（且无法自动加载）
                throw new System.IO.FileNotFoundException("文件 \"" + settings.FullPath + "\" 未找到。");

            return cachedFile.Load<T>(key); // 从缓存文件加载
        }

        using (var reader = ES3Reader.Create(settings)) // 创建读取器
        {
            if (reader == null) // 文件不存在
                throw new System.IO.FileNotFoundException("文件 \"" + settings.FullPath + "\" 未找到。");
            return reader.Read<T>(key); // 读取并返回键对应的值
        }
    }

    /// <summary>（泛型）从默认文件中指定键加载值。如果文件或键不存在，则返回默认值。</summary>
    /// <param name="T">要加载的数据类型。</param>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="defaultValue">如果文件或键不存在则返回的值。</param>
    public static T Load<T>(string key, T defaultValue)
    {
        return Load<T>(key, defaultValue, new ES3Settings());
    }

    /// <summary>（泛型）从指定文件路径中指定键加载值。如果文件或键不存在，则返回默认值。</summary>
    /// <param name="T">要加载的数据类型。</param>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="filePath">要从中加载的文件的相对或绝对路径。</param>
    /// <param name="defaultValue">如果文件或键不存在则返回的值。</param>
    public static T Load<T>(string key, string filePath, T defaultValue)
    {
        return Load<T>(key, defaultValue, new ES3Settings(filePath));
    }

    /// <summary>（泛型）从指定文件路径中指定键加载值，使用提供的设置覆盖默认设置。如果文件或键不存在，则返回默认值。</summary>
    /// <param name="T">要加载的数据类型。</param>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="filePath">要从中加载的文件的相对或绝对路径。</param>
    /// <param name="defaultValue">如果文件或键不存在则返回的值。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static T Load<T>(string key, string filePath, T defaultValue, ES3Settings settings)
    {
        return Load<T>(key, defaultValue, new ES3Settings(filePath, settings));
    }

    /// <summary>（泛型）从指定设置定义的文件中指定键加载值。如果文件或键不存在，则返回默认值。</summary>
    /// <param name="T">要加载的数据类型。</param>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="defaultValue">如果文件或键不存在则返回的值。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static T Load<T>(string key, T defaultValue, ES3Settings settings)
    {
        if (settings.location == Location.Cache) // 如果是缓存位置
        {
            var cachedFile = ES3File.GetCachedFile(settings); // 获取缓存的ES3File

            if (settings.autoCacheFileOnLoad && cachedFile == null) // 如果启用自动缓存且缓存不存在
                cachedFile = ES3File.CacheFile(settings); // 从存储加载文件到缓存

            if (cachedFile == null) // 缓存文件不存在（且无法自动加载）
                return defaultValue; // 返回默认值

            return cachedFile.Load<T>(key, defaultValue); // 从缓存文件加载，带默认值
        }

        using (var reader = ES3Reader.Create(settings)) // 创建读取器
        {
            if (reader == null) // 文件不存在
                return defaultValue; // 返回默认值
            return reader.Read<T>(key, defaultValue); // 读取键对应的值，带默认值
        }
    }

    /* 自赋值加载方法 (LoadInto) */

    /// <summary>将值从默认文件中指定键加载到现有对象中，而不是创建新实例。</summary>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="obj">要将值加载到的对象。</param>
    public static void LoadInto<T>(string key, object obj) where T : class
    {
        LoadInto<object>(key, obj, new ES3Settings());
    }

    /// <summary>将值从指定文件路径中指定键加载到现有对象中，而不是创建新实例。</summary>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="filePath">要从中加载的文件的相对或绝对路径。</param>
    /// <param name="obj">要将值加载到的对象。</param>
    public static void LoadInto(string key, string filePath, object obj)
    {
        LoadInto<object>(key, obj, new ES3Settings(filePath));
    }

    /// <summary>将值从指定文件路径中指定键加载到现有对象中，而不是创建新实例，使用提供的设置覆盖默认设置。</summary>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="filePath">要从中加载的文件的相对或绝对路径。</param>
    /// <param name="obj">要将值加载到的对象。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void LoadInto(string key, string filePath, object obj, ES3Settings settings)
    {
        LoadInto<object>(key, obj, new ES3Settings(filePath, settings));
    }

    /// <summary>将值从指定设置定义的文件中指定键加载到现有对象中，而不是创建新实例。</summary>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="obj">要将值加载到的对象。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void LoadInto(string key, object obj, ES3Settings settings)
    {
        LoadInto<object>(key, obj, settings);
    }

    /// <summary>（泛型）将值从默认文件中指定键加载到现有对象中，而不是创建新实例。</summary>
    /// <param name="T">要加载的数据类型。</param>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="obj">要将值加载到的对象。</param>
    public static void LoadInto<T>(string key, T obj) where T : class
    {
        LoadInto<T>(key, obj, new ES3Settings());
    }

    /// <summary>（泛型）将值从指定文件路径中指定键加载到现有对象中，而不是创建新实例。</summary>
    /// <param name="T">要加载的数据类型。</param>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="filePath">要从中加载的文件的相对或绝对路径。</param>
    /// <param name="obj">要将值加载到的对象。</param>
    public static void LoadInto<T>(string key, string filePath, T obj) where T : class
    {
        LoadInto<T>(key, obj, new ES3Settings(filePath));
    }

    /// <summary>（泛型）将值从指定文件路径中指定键加载到现有对象中，而不是创建新实例，使用提供的设置覆盖默认设置。</summary>
    /// <param name="T">要加载的数据类型。</param>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="filePath">要从中加载的文件的相对或绝对路径。</param>
    /// <param name="obj">要将值加载到的对象。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void LoadInto<T>(string key, string filePath, T obj, ES3Settings settings) where T : class
    {
        LoadInto<T>(key, obj, new ES3Settings(filePath, settings));
    }

    /// <summary>（泛型）将值从指定设置定义的文件中指定键加载到现有对象中，而不是创建新实例。</summary>
    /// <param name="T">要加载的数据类型。</param>
    /// <param name="key">标识要加载的值的键。</param>
    /// <param name="obj">要将值加载到的对象。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void LoadInto<T>(string key, T obj, ES3Settings settings) where T : class
    {
        if (ES3Reflection.IsValueType(obj.GetType())) // 检查对象是否为值类型
            throw new InvalidOperationException("ES3.LoadInto 只能用于引用类型，但你加载的数据是值类型。请改用 ES3.Load。");

        if (settings.location == Location.Cache) // 如果是缓存位置
        {
            var cachedFile = ES3File.GetCachedFile(settings); // 获取缓存的ES3File

            if (settings.autoCacheFileOnLoad && cachedFile == null) // 如果启用自动缓存且缓存不存在
                cachedFile = ES3File.CacheFile(settings); // 从存储加载文件到缓存

            if (cachedFile == null) // 缓存文件不存在（且无法自动加载）
                throw new System.IO.FileNotFoundException("文件 \"" + settings.FullPath + "\" 未找到。");

            cachedFile.LoadInto<T>(key, obj); // 从缓存文件加载到对象
            return;
        }

        if (settings == null) settings = new ES3Settings(); // 确保设置不为空
        using (var reader = ES3Reader.Create(settings)) // 创建读取器
        {
            if (reader == null) // 文件不存在
                throw new System.IO.FileNotFoundException("文件 \"" + settings.FullPath + "\" 未找到。");
            reader.ReadInto<T>(key, obj); // 读取键对应的值到对象
        }
    }

    /* 专门加载字符串的方法（避免重载混淆） */

    /// <summary>从指定设置定义的文件中指定键加载字符串值。</summary>
    /// <param name="key">标识要加载的字符串的键。</param>
    /// <param name="defaultValue">如果文件或键不存在则返回的字符串。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static string LoadString(string key, string defaultValue, ES3Settings settings)
    {
        return Load<string>(key, null, defaultValue, settings); // 调用泛型Load
    }

    /// <summary>从指定文件路径中指定键加载字符串值。</summary>
    /// <param name="key">标识要加载的字符串的键。</param>
    /// <param name="defaultValue">如果文件或键不存在则返回的字符串。</param>
    /// <param name="filePath">要从中加载的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static string LoadString(string key, string defaultValue, string filePath = null,
        ES3Settings settings = null)
    {
        return Load<string>(key, filePath, defaultValue, settings); // 调用泛型Load
    }

    #endregion

    #region 其他 ES3.Load 方法

    /// <summary>将默认文件加载为字节数组。</summary>
    public static byte[] LoadRawBytes()
    {
        return LoadRawBytes(new ES3Settings());
    }

    /// <summary>将指定路径的文件加载为字节数组。</summary>
    /// <param name="filePath">要作为字节数组加载的文件的相对或绝对路径。</param>
    public static byte[] LoadRawBytes(string filePath)
    {
        return LoadRawBytes(new ES3Settings(filePath));
    }

    /// <summary>将指定路径的文件加载为字节数组，使用提供的设置覆盖默认设置。</summary>
    /// <param name="filePath">要作为字节数组加载的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static byte[] LoadRawBytes(string filePath, ES3Settings settings)
    {
        return LoadRawBytes(new ES3Settings(filePath, settings));
    }

    /// <summary>将指定设置定义的文件加载为字节数组。</summary>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static byte[] LoadRawBytes(ES3Settings settings)
    {
        if (settings.location == Location.Cache) // 如果是缓存位置
        {
            var cachedFile = ES3File.GetCachedFile(settings); // 获取缓存的ES3File

            if (settings.autoCacheFileOnLoad && cachedFile == null) // 如果启用自动缓存且缓存不存在
                cachedFile = ES3File.CacheFile(settings); // 从存储加载文件到缓存

            if (cachedFile == null) // 缓存文件不存在（且无法自动加载）
                throw new System.IO.FileNotFoundException("文件 \"" + settings.FullPath + "\" 未找到。");

            return cachedFile.LoadRawBytes(); // 从缓存文件加载原始字节
        }

        using (var stream = ES3Stream.CreateStream(settings, ES3FileMode.Read)) // 创建读取流
        {
            if (stream == null) // 流创建失败（文件不存在）
                throw new System.IO.FileNotFoundException("文件 " + settings.path + " 未找到");

            if (stream.GetType() == typeof(System.IO.Compression.GZipStream)) // 如果是GZip压缩流
            {
                var gZipStream = (System.IO.Compression.GZipStream)stream;
                using (var ms = new System.IO.MemoryStream())
                {
                    ES3Stream.CopyTo(gZipStream, ms); // 解压并复制到内存流
                    return ms.ToArray(); // 返回解压后的字节
                }
            }
            else // 非压缩流
            {
                var bytes = new byte[stream.Length]; // 创建足够大的字节数组
                stream.Read(bytes, 0, bytes.Length); // 读取整个流
                return bytes; // 返回字节
            }
        }
    }

    /// <summary>将默认文件加载为字符串。</summary>
    public static string LoadRawString()
    {
        return LoadRawString(new ES3Settings());
    }

    /// <summary>将指定路径的文件加载为字符串。</summary>
    /// <param name="filePath">要作为字符串加载的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static string LoadRawString(string filePath)
    {
        return LoadRawString(new ES3Settings(filePath));
    }

    /// <summary>将指定路径的文件加载为字符串，使用提供的设置覆盖默认设置。</summary>
    /// <param name="filePath">要作为字符串加载的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static string LoadRawString(string filePath, ES3Settings settings)
    {
        return LoadRawString(new ES3Settings(filePath, settings));
    }

    /// <summary>将指定设置定义的文件加载为字符串。</summary>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static string LoadRawString(ES3Settings settings)
    {
        var bytes = ES3.LoadRawBytes(settings); // 先加载为字节数组
        return settings.encoding.GetString(bytes, 0, bytes.Length); // 使用指定编码转换为字符串
    }

    /// <summary>将PNG或JPG加载为Texture2D。</summary>
    /// <param name="imagePath">要加载为Texture2D的PNG或JPG文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static Texture2D LoadImage(string imagePath)
    {
        return LoadImage(new ES3Settings(imagePath));
    }

    /// <summary>将PNG或JPG加载为Texture2D。</summary>
    /// <param name="imagePath">要加载为Texture2D的PNG或JPG文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static Texture2D LoadImage(string imagePath, ES3Settings settings)
    {
        return LoadImage(new ES3Settings(imagePath, settings));
    }

    /// <summary>将指定设置定义的图片文件加载为Texture2D。</summary>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static Texture2D LoadImage(ES3Settings settings)
    {
        byte[] bytes = ES3.LoadRawBytes(settings); // 加载图片原始字节
        return LoadImage(bytes); // 调用字节加载方法
    }

    /// <summary>将PNG或JPG字节数组加载为Texture2D。</summary>
    /// <param name="bytes">PNG或JPG的原始字节。</param>
    public static Texture2D LoadImage(byte[] bytes)
    {
        var texture = new Texture2D(1, 1); // 创建临时纹理
        texture.LoadImage(bytes); // 让Unity加载图片字节
        return texture; // 返回纹理
    }

    /// <summary>将音频文件加载为AudioClip。注意：MP3文件在独立平台上不受支持，Ogg Vorbis文件在移动平台上不受支持。</summary>
    /// <param name="audioFilePath">要加载为AudioClip的音频文件的相对或绝对路径。</param>
#if UNITY_2018_3_OR_NEWER
    /// <param name="audioType">音频类型 (AudioType)。</param>
#endif
    public static AudioClip LoadAudio(string audioFilePath
#if UNITY_2018_3_OR_NEWER
        , AudioType audioType
#endif
    )
    {
        return LoadAudio(audioFilePath,
#if UNITY_2018_3_OR_NEWER
            audioType,
#endif
            new ES3Settings());
    }

    /// <summary>将音频文件加载为AudioClip。注意：MP3文件在独立平台上不受支持，Ogg Vorbis文件在移动平台上不受支持。</summary>
    /// <param name="audioFilePath">要加载为AudioClip的音频文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
#if UNITY_2018_3_OR_NEWER
    /// <param name="audioType">音频类型 (AudioType)。</param>
#endif
    public static AudioClip LoadAudio(string audioFilePath,
#if UNITY_2018_3_OR_NEWER
        AudioType audioType,
#endif
        ES3Settings settings)
    {
        if (settings.location != Location.File) // 仅支持文件位置
            throw new InvalidOperationException("ES3.LoadAudio 只能与文件保存位置一起使用");

        if (Application.platform == RuntimePlatform.WebGLPlayer) // WebGL不支持
            throw new InvalidOperationException("不能在 WebGL 中使用 ES3.LoadAudio");

        string extension = ES3IO.GetExtension(audioFilePath).ToLower(); // 获取文件扩展名

        // 平台格式限制检查
        if (extension == ".mp3" && (Application.platform == RuntimePlatform.WindowsPlayer ||
                                    Application.platform == RuntimePlatform.OSXPlayer))
            throw new System.InvalidOperationException("在 Unity 独立平台上只能加载 Ogg、WAV、XM、IT、MOD 或 S3M");

        if (extension == ".ogg" && (Application.platform == RuntimePlatform.IPhonePlayer
                                    || Application.platform == RuntimePlatform.Android
                                    || Application.platform == RuntimePlatform.WSAPlayerARM))
            throw new System.InvalidOperationException("在移动平台上只能加载 MP3、WAV、XM、IT、MOD 或 S3M");

        var newSettings = new ES3Settings(audioFilePath, settings); // 使用音频路径创建新设置

#if UNITY_2018_3_OR_NEWER // Unity 2018.3+ 使用 UnityWebRequest
        using (UnityWebRequest www =
               UnityWebRequestMultimedia.GetAudioClip("file://" + newSettings.FullPath, audioType))
        {
            www.SendWebRequest(); // 发送请求

            while (!www.isDone) // 等待加载完成
            {
                // Wait for it to load.
            }

            if (ES3WebClass.IsNetworkError(www)) // 检查错误
                throw new System.Exception(www.error);
            else
                return DownloadHandlerAudioClip.GetContent(www); // 获取音频片段
        }
#elif UNITY_2017_1_OR_NEWER // 旧版Unity使用WWW (已弃用)
        // ... (WWW 实现代码已省略)
#else
        // ... (更旧的WWW实现代码已省略)
#endif

        // 旧版Unity返回音频片段 (代码已省略)
    }

    #endregion

    #region 序列化/反序列化 (Serialize/Deserialize)

    /// <summary>将对象序列化为字节数组。</summary>
    /// <param name="value">要序列化的对象。</param>
    /// <param name="settings">序列化设置（可选）。</param>
    public static byte[] Serialize<T>(T value, ES3Settings settings = null)
    {
        return Serialize(value, ES3TypeMgr.GetOrCreateES3Type(typeof(T)), settings);
    }

    /// <summary>（内部）使用特定ES3Type将对象序列化为字节数组。</summary>
    internal static byte[] Serialize(object value, ES3Types.ES3Type type, ES3Settings settings = null)
    {
        if (settings == null) settings = new ES3Settings(); // 确保设置不为空

        using (var ms = new System.IO.MemoryStream()) // 使用内存流
        {
            using (var stream = ES3Stream.CreateStream(ms, settings, ES3FileMode.Write)) // 创建写入流（指向内存）
            {
                using (var baseWriter = ES3Writer.Create(stream, settings, false, false)) // 创建写入器
                {
                    baseWriter.Write(value, type, settings.referenceMode); // 写入对象（使用指定类型和引用模式）
                }

                return ms.ToArray(); // 返回内存流中的字节
            }
        }
    }

    /// <summary>将字节数组反序列化为指定类型的对象。</summary>
    /// <param name="bytes">包含序列化数据的字节数组。</param>
    /// <param name="settings">反序列化设置（可选）。</param>
    public static T Deserialize<T>(byte[] bytes, ES3Settings settings = null)
    {
        return (T)Deserialize(ES3TypeMgr.GetOrCreateES3Type(typeof(T)), bytes, settings);
    }

    /// <summary>（内部）使用特定ES3Type将字节数组反序列化为对象。</summary>
    internal static object Deserialize(ES3Types.ES3Type type, byte[] bytes, ES3Settings settings = null)
    {
        if (settings == null)
            settings = new ES3Settings(); // 确保设置不为空

        using (var ms = new System.IO.MemoryStream(bytes, false)) // 从字节创建内存流
        using (var stream = ES3Stream.CreateStream(ms, settings, ES3FileMode.Read)) // 创建读取流（指向内存）
        using (var reader = ES3Reader.Create(stream, settings, false)) // 创建读取器
            return reader.Read<object>(type); // 读取并返回对象
    }

    /// <summary>将字节数组反序列化到现有对象中。</summary>
    /// <param name="bytes">包含序列化数据的字节数组。</param>
    /// <param name="obj">要将数据加载到的现有对象。</param>
    /// <param name="settings">反序列化设置（可选）。</param>
    public static void DeserializeInto<T>(byte[] bytes, T obj, ES3Settings settings = null) where T : class
    {
        DeserializeInto(ES3TypeMgr.GetOrCreateES3Type(typeof(T)), bytes, obj, settings);
    }

    /// <summary>（泛型）使用特定ES3Type将字节数组反序列化到现有对象中。</summary>
    public static void DeserializeInto<T>(ES3Types.ES3Type type, byte[] bytes, T obj, ES3Settings settings = null)
        where T : class
    {
        if (settings == null)
            settings = new ES3Settings(); // 确保设置不为空

        using (var ms = new System.IO.MemoryStream(bytes, false)) // 从字节创建内存流
        using (var reader = ES3Reader.Create(ms, settings, false)) // 创建读取器（指向内存）
            reader.ReadInto<T>(obj, type); // 读取数据到现有对象
    }

    #endregion

    #region 其他 ES3 方法

#if !DISABLE_ENCRYPTION // 如果没有禁用加密

    /// <summary>使用密码加密字节数组。</summary>
    /// <param name="bytes">要加密的字节。</param>
    /// <param name="password">加密密码（可选，使用默认设置中的密码）。</param>
    public static byte[] EncryptBytes(byte[] bytes, string password = null)
    {
        if (string.IsNullOrEmpty(password))
            password = ES3Settings.defaultSettings.encryptionPassword; // 使用默认密码
        return new AESEncryptionAlgorithm().Encrypt(bytes, password, ES3Settings.defaultSettings.bufferSize); // AES加密
    }

    /// <summary>使用密码解密字节数组。</summary>
    /// <param name="bytes">要解密的字节。</param>
    /// <param name="password">解密密码（可选，使用默认设置中的密码）。</param>
    public static byte[] DecryptBytes(byte[] bytes, string password = null)
    {
        if (string.IsNullOrEmpty(password))
            password = ES3Settings.defaultSettings.encryptionPassword; // 使用默认密码
        return new AESEncryptionAlgorithm().Decrypt(bytes, password, ES3Settings.defaultSettings.bufferSize); // AES解密
    }

    /// <summary>使用密码加密字符串。</summary>
    /// <param name="str">要加密的字符串。</param>
    /// <param name="password">加密密码（可选，使用默认设置中的密码）。</param>
    public static string EncryptString(string str, string password = null)
    {
        return Convert.ToBase64String(EncryptBytes(ES3Settings.defaultSettings.encoding.GetBytes(str),
            password)); // 编码->加密->Base64
    }

    /// <summary>使用密码解密字符串。</summary>
    /// <param name="str">要解密的Base64字符串。</param>
    /// <param name="password">解密密码（可选，使用默认设置中的密码）。</param>
    public static string DecryptString(string str, string password = null)
    {
        return ES3Settings.defaultSettings.encoding.GetString(DecryptBytes(Convert.FromBase64String(str),
            password)); // Base64解码->解密->字符串
    }

#endif

    /// <summary>压缩字节数组。</summary>
    /// <param name="bytes">要压缩的字节。</param>
    public static byte[] CompressBytes(byte[] bytes)
    {
        using (var ms = new System.IO.MemoryStream()) // 使用内存流
        {
            var settings = new ES3Settings(); // 创建临时设置
            settings.location = ES3.Location.InternalMS; // 内部内存流
            settings.compressionType = ES3.CompressionType.Gzip; // Gzip压缩
            settings.encryptionType = EncryptionType.None; // 无加密

            using (var stream = ES3Stream.CreateStream(ms, settings, ES3FileMode.Write)) // 创建写入流（压缩）
                stream.Write(bytes, 0, bytes.Length); // 写入（压缩）字节

            return ms.ToArray(); // 返回压缩后的字节
        }
    }

    /// <summary>解压缩字节数组。</summary>
    /// <param name="bytes">要解压缩的字节（Gzip格式）。</param>
    public static byte[] DecompressBytes(byte[] bytes)
    {
        using (var ms = new System.IO.MemoryStream(bytes)) // 从压缩字节创建内存流
        {
            var settings = new ES3Settings(); // 创建临时设置
            settings.location = ES3.Location.InternalMS; // 内部内存流
            settings.compressionType = ES3.CompressionType.Gzip; // Gzip压缩
            settings.encryptionType = EncryptionType.None; // 无加密

            using (var output = new System.IO.MemoryStream()) // 创建输出内存流
            {
                using (var input = ES3Stream.CreateStream(ms, settings, ES3FileMode.Read)) // 创建读取流（解压缩）
                    ES3Stream.CopyTo(input, output); // 解压并复制到输出流
                return output.ToArray(); // 返回解压后的字节
            }
        }
    }

    /// <summary>压缩字符串。</summary>
    /// <param name="str">要压缩的字符串。</param>
    public static string CompressString(string str)
    {
        return Convert.ToBase64String(
            CompressBytes(ES3Settings.defaultSettings.encoding.GetBytes(str))); // 编码->压缩->Base64
    }

    /// <summary>解压缩字符串。</summary>
    /// <param name="str">要解压缩的Base64字符串（由CompressString生成）。</param>
    public static string DecompressString(string str)
    {
        return ES3Settings.defaultSettings.encoding.GetString(
            DecompressBytes(Convert.FromBase64String(str))); // Base64解码->解压->字符串
    }

    /// <summary>删除默认文件。</summary>
    public static void DeleteFile()
    {
        DeleteFile(new ES3Settings());
    }

    /// <summary>删除指定路径的文件（使用默认设置）。</summary>
    /// <param name="filePath">要删除的文件的相对或绝对路径。</param>
    public static void DeleteFile(string filePath)
    {
        DeleteFile(new ES3Settings(filePath));
    }

    /// <summary>删除指定路径的文件，使用提供的设置覆盖默认设置。</summary>
    /// <param name="filePath">要删除的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void DeleteFile(string filePath, ES3Settings settings)
    {
        DeleteFile(new ES3Settings(filePath, settings));
    }

    /// <summary>删除由提供的ES3Settings对象指定的文件。</summary>
    /// <param name="settings">定义要删除的文件的设置。</param>
    public static void DeleteFile(ES3Settings settings)
    {
        switch (settings.location)
        {
            case Location.File:
                ES3IO.DeleteFile(settings.FullPath); // 删除物理文件
                break;
            case Location.PlayerPrefs:
                PlayerPrefs.DeleteKey(settings.FullPath); // 删除PlayerPrefs键
                break;
            case Location.Cache:
                ES3File.RemoveCachedFile(settings); // 从缓存中移除文件
                break;
            case Location.Resources:
                throw new System.NotSupportedException("无法从Resources中删除文件。"); // Resources只读
        }
    }

    /// <summary>将文件从一个路径复制到另一个路径。</summary>
    /// <param name="oldFilePath">要复制的源文件的相对或绝对路径。</param>
    /// <param name="newFilePath">要创建的目标副本的相对或绝对路径。</param>
    public static void CopyFile(string oldFilePath, string newFilePath)
    {
        CopyFile(new ES3Settings(oldFilePath), new ES3Settings(newFilePath));
    }

    /// <summary>使用提供的ES3Settings覆盖默认设置，将文件从一个位置复制到另一个位置。</summary>
    /// <param name="oldFilePath">要复制的源文件的相对或绝对路径。</param>
    /// <param name="newFilePath">要创建的目标副本的相对或绝对路径。</param>
    /// <param name="oldSettings">复制旧文件时使用的设置。</param>
    /// <param name="newSettings">创建新文件时使用的设置。</param>
    public static void CopyFile(string oldFilePath, string newFilePath, ES3Settings oldSettings,
        ES3Settings newSettings)
    {
        CopyFile(new ES3Settings(oldFilePath, oldSettings), new ES3Settings(newFilePath, newSettings));
    }

    /// <summary>使用提供的ES3Settings确定位置，将文件从一个位置复制到另一个位置。</summary>
    /// <param name="oldSettings">定义源文件的设置。</param>
    /// <param name="newSettings">定义目标文件的设置。</param>
    public static void CopyFile(ES3Settings oldSettings, ES3Settings newSettings)
    {
        if (oldSettings.location != newSettings.location) // 源和目标位置必须相同
            throw new InvalidOperationException("无法从 " + oldSettings.location + " 复制文件到 " + newSettings.location +
                                                "。源和目标的位置必须相同。");

        switch (oldSettings.location)
        {
            case Location.File:
                if (ES3IO.FileExists(oldSettings.FullPath)) // 源文件存在
                {
                    // 创建目标目录（如果不存在）
                    string newDirectory = ES3IO.GetDirectoryPath(newSettings.FullPath);
                    if (!ES3IO.DirectoryExists(newDirectory))
                        ES3IO.CreateDirectory(newDirectory);
                    // 删除目标文件（如果存在）以便覆盖
                    else
                        ES3IO.DeleteFile(newSettings.FullPath);
                    ES3IO.CopyFile(oldSettings.FullPath, newSettings.FullPath); // 复制文件
                }

                break;
            case Location.PlayerPrefs:
                PlayerPrefs.SetString(newSettings.FullPath,
                    PlayerPrefs.GetString(oldSettings.FullPath)); // 复制PlayerPrefs值
                break;
            case Location.Cache:
                ES3File.CopyCachedFile(oldSettings, newSettings); // 复制缓存中的文件
                break;
            case Location.Resources:
                throw new System.NotSupportedException("不允许修改Resources中的文件。"); // Resources只读
        }
    }

    /// <summary>重命名文件。</summary>
    /// <param name="oldFilePath">要重命名的源文件的相对或绝对路径。</param>
    /// <param name="newFilePath">要将源文件重命名为的新相对或绝对路径。</param>
    public static void RenameFile(string oldFilePath, string newFilePath)
    {
        RenameFile(new ES3Settings(oldFilePath), new ES3Settings(newFilePath));
    }

    /// <summary>重命名文件。</summary>
    /// <param name="oldFilePath">要重命名的源文件的相对或绝对路径。</param>
    /// <param name="newFilePath">要将源文件重命名为的新相对或绝对路径。</param>
    /// <param name="oldSettings">源文件的设置。</param>
    /// <param name="newSettings">目标文件的设置。</param>
    public static void RenameFile(string oldFilePath, string newFilePath, ES3Settings oldSettings,
        ES3Settings newSettings)
    {
        RenameFile(new ES3Settings(oldFilePath, oldSettings), new ES3Settings(newFilePath, newSettings));
    }

    /// <summary>重命名文件。</summary>
    /// <param name="oldSettings">定义源文件的设置。</param>
    /// <param name="newSettings">定义目标文件的设置。</param>
    public static void RenameFile(ES3Settings oldSettings, ES3Settings newSettings)
    {
        if (oldSettings.location != newSettings.location) // 源和目标位置必须相同
            throw new InvalidOperationException("无法将文件从 " + oldSettings.location + " 重命名到 " + newSettings.location +
                                                "。源和目标的位置必须相同。");

        switch (oldSettings.location)
        {
            case Location.File:
                if (ES3IO.FileExists(oldSettings.FullPath)) // 源文件存在
                {
                    ES3IO.DeleteFile(newSettings.FullPath); // 删除可能存在的目标文件
                    ES3IO.MoveFile(oldSettings.FullPath, newSettings.FullPath); // 移动/重命名文件
                }

                break;
            case Location.PlayerPrefs:
                PlayerPrefs.SetString(newSettings.FullPath, PlayerPrefs.GetString(oldSettings.FullPath)); // 复制值
                PlayerPrefs.DeleteKey(oldSettings.FullPath); // 删除旧键
                break;
            case Location.Cache:
                ES3File.CopyCachedFile(oldSettings, newSettings); // 复制到新键
                ES3File.RemoveCachedFile(oldSettings); // 移除旧键
                break;
            case Location.Resources:
                throw new System.NotSupportedException("不允许修改Resources中的文件。"); // Resources只读
        }
    }

    /// <summary>将目录从一个路径复制到另一个路径。</summary>
    /// <param name="oldDirectoryPath">要复制的源目录的相对或绝对路径。</param>
    /// <param name="newDirectoryPath">要创建的目标副本目录的相对或绝对路径。</param>
    public static void CopyDirectory(string oldDirectoryPath, string newDirectoryPath)
    {
        CopyDirectory(new ES3Settings(oldDirectoryPath), new ES3Settings(newDirectoryPath));
    }

    /// <summary>使用提供的ES3Settings覆盖默认设置，将目录从一个位置复制到另一个位置。</summary>
    /// <param name="oldDirectoryPath">要复制的源目录的相对或绝对路径。</param>
    /// <param name="newDirectoryPath">要创建的目标副本目录的相对或绝对路径。</param>
    /// <param name="oldSettings">复制旧目录时使用的设置。</param>
    /// <param name="newSettings">创建新目录时使用的设置。</param>
    public static void CopyDirectory(string oldDirectoryPath, string newDirectoryPath, ES3Settings oldSettings,
        ES3Settings newSettings)
    {
        CopyDirectory(new ES3Settings(oldDirectoryPath, oldSettings), new ES3Settings(newDirectoryPath, newSettings));
    }

    /// <summary>使用提供的ES3Settings确定位置，将目录从一个位置复制到另一个位置。</summary>
    /// <param name="oldSettings">定义源目录的设置。</param>
    /// <param name="newSettings">定义目标目录的设置。</param>
    public static void CopyDirectory(ES3Settings oldSettings, ES3Settings newSettings)
    {
        if (oldSettings.location != Location.File) // 目前只支持文件系统位置
            throw new InvalidOperationException("ES3.CopyDirectory 只能在保存位置为 'File' 时使用，并且不能与 WebGL 一起使用。");

        if (!DirectoryExists(oldSettings)) // 源目录不存在
            throw new System.IO.DirectoryNotFoundException("目录 " + oldSettings.FullPath + " 未找到");

        if (!DirectoryExists(newSettings)) // 目标目录不存在
            ES3IO.CreateDirectory(newSettings.FullPath); // 创建目标目录

        // 复制所有文件
        foreach (var fileName in ES3.GetFiles(oldSettings))
            CopyFile(ES3IO.CombinePathAndFilename(oldSettings.path, fileName),
                ES3IO.CombinePathAndFilename(newSettings.path, fileName), oldSettings, newSettings);

        // 递归复制所有子目录
        foreach (var directoryName in GetDirectories(oldSettings))
            CopyDirectory(ES3IO.CombinePathAndFilename(oldSettings.path, directoryName),
                ES3IO.CombinePathAndFilename(newSettings.path, directoryName), oldSettings, newSettings);
    }

    /// <summary>重命名目录。</summary>
    /// <param name="oldDirectoryPath">要重命名的源目录的相对或绝对路径。</param>
    /// <param name="newDirectoryPath">要将源目录重命名为的新相对或绝对路径。</param>
    public static void RenameDirectory(string oldDirectoryPath, string newDirectoryPath)
    {
        RenameDirectory(new ES3Settings(oldDirectoryPath), new ES3Settings(newDirectoryPath));
    }

    /// <summary>重命名目录。</summary>
    /// <param name="oldDirectoryPath">要重命名的源目录的相对或绝对路径。</param>
    /// <param name="newDirectoryPath">要将源目录重命名为的新相对或绝对路径。</param>
    /// <param name="oldSettings">源目录的设置。</param>
    /// <param name="newSettings">目标目录的设置。</param>
    public static void RenameDirectory(string oldDirectoryPath, string newDirectoryPath, ES3Settings oldSettings,
        ES3Settings newSettings)
    {
        RenameDirectory(new ES3Settings(oldDirectoryPath, oldSettings), new ES3Settings(newDirectoryPath, newSettings));
    }

    /// <summary>重命名目录。</summary>
    /// <param name="oldSettings">定义源目录的设置。</param>
    /// <param name="newSettings">定义目标目录的设置。</param>
    public static void RenameDirectory(ES3Settings oldSettings, ES3Settings newSettings)
    {
        if (oldSettings.location == Location.File) // 文件系统位置
        {
            if (ES3IO.DirectoryExists(oldSettings.FullPath)) // 源目录存在
            {
                ES3IO.DeleteDirectory(newSettings.FullPath); // 删除可能存在的目标目录
                ES3IO.MoveDirectory(oldSettings.FullPath, newSettings.FullPath); // 移动/重命名目录
            }
        }
        else if (oldSettings.location == Location.PlayerPrefs || oldSettings.location == Location.Cache) // 不支持的位置
            throw new System.NotSupportedException("当保存到缓存(Cache)、PlayerPrefs、tvOS 或使用 WebGL 时，无法重命名目录。");
        else if (oldSettings.location == Location.Resources) // Resources只读
            throw new System.NotSupportedException("不允许修改Resources中的文件。");
    }

    /// <summary>删除指定路径的目录（使用默认设置）。</summary>
    /// <param name="directoryPath">要删除的目录的相对或绝对路径。</param>
    public static void DeleteDirectory(string directoryPath)
    {
        DeleteDirectory(new ES3Settings(directoryPath));
    }

    /// <summary>删除指定路径的目录，使用提供的设置覆盖默认设置。</summary>
    /// <param name="directoryPath">要删除的目录的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void DeleteDirectory(string directoryPath, ES3Settings settings)
    {
        DeleteDirectory(new ES3Settings(directoryPath, settings));
    }

    /// <summary>删除由提供的设置定义的目录。</summary>
    /// <param name="settings">定义要删除的目录的设置。</param>
    public static void DeleteDirectory(ES3Settings settings)
    {
        if (settings.location == Location.File) // 文件系统位置
            ES3IO.DeleteDirectory(settings.FullPath); // 删除物理目录
        else if (settings.location == Location.PlayerPrefs || settings.location == Location.Cache) // 不支持的位置
            throw new System.NotSupportedException("使用缓存(Cache)或PlayerPrefs删除目录不受支持。");
        else if (settings.location == Location.Resources) // Resources只读
            throw new System.NotSupportedException("不允许从Resources中删除目录。");
    }

    /// <summary>删除默认文件中的指定键。</summary>
    /// <param name="key">要删除的键。</param>
    public static void DeleteKey(string key)
    {
        DeleteKey(key, new ES3Settings());
    }

    /// <summary>删除指定文件中的指定键（使用默认设置）。</summary>
    /// <param name="key">要删除的键。</param>
    /// <param name="filePath">要从中删除键的文件的相对或绝对路径。</param>
    public static void DeleteKey(string key, string filePath)
    {
        DeleteKey(key, new ES3Settings(filePath));
    }

    /// <summary>删除指定文件中的指定键，使用提供的设置覆盖默认设置。</summary>
    /// <param name="key">要删除的键。</param>
    /// <param name="filePath">要从中删除键的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void DeleteKey(string key, string filePath, ES3Settings settings)
    {
        DeleteKey(key, new ES3Settings(filePath, settings));
    }

    /// <summary>删除由提供的设置定义的文件中的指定键。</summary>
    /// <param name="key">要删除的键。</param>
    /// <param name="settings">定义文件位置的设置。</param>
    public static void DeleteKey(string key, ES3Settings settings)
    {
        if (settings.location == Location.Resources) // Resources只读
            throw new System.NotSupportedException("不允许修改Resources中的文件。");
        else if (settings.location == Location.Cache) // 缓存位置
            ES3File.DeleteKey(key, settings); // 从缓存文件删除键
        else if (ES3.FileExists(settings)) // 文件存在且不是Resources
        {
            using (var writer = ES3Writer.Create(settings)) // 创建写入器
            {
                writer.MarkKeyForDeletion(key); // 标记键为删除
                writer.Save(); // 保存更改（实际删除键）
            }
        }
    }

    /// <summary>检查默认文件中是否存在指定的键。</summary>
    /// <param name="key">要检查是否存在的键。</param>
    /// <returns>如果键存在则为 True，否则为 False。</returns>
    public static bool KeyExists(string key)
    {
        return KeyExists(key, new ES3Settings());
    }

    /// <summary>检查指定文件中是否存在指定的键。</summary>
    /// <param name="key">要检查是否存在的键。</param>
    /// <param name="filePath">要搜索的文件的相对或绝对路径。</param>
    /// <returns>如果键存在则为 True，否则为 False。</returns>
    public static bool KeyExists(string key, string filePath)
    {
        return KeyExists(key, new ES3Settings(filePath));
    }

    /// <summary>检查指定文件中是否存在指定的键，使用提供的设置覆盖默认设置。</summary>
    /// <param name="key">要检查是否存在的键。</param>
    /// <param name="filePath">要搜索的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    /// <returns>如果键存在则为 True，否则为 False。</returns>
    public static bool KeyExists(string key, string filePath, ES3Settings settings)
    {
        return KeyExists(key, new ES3Settings(filePath, settings));
    }

    /// <summary>检查文件中是否存在指定的键。</summary>
    /// <param name="key">要检查是否存在的键。</param>
    /// <param name="settings">定义文件位置的设置。</param>
    /// <returns>如果键存在则为 True，否则为 False。</returns>
    public static bool KeyExists(string key, ES3Settings settings)
    {
        if (settings.location == Location.Cache) // 缓存位置
            return ES3File.KeyExists(key, settings); // 检查缓存文件

        using (var reader = ES3Reader.Create(settings)) // 创建读取器
        {
            if (reader == null) // 文件不存在
                return false;
            return reader.Goto(key); // 尝试定位到该键
        }
    }

    /// <summary>检查默认文件是否存在。</summary>
    /// <returns>如果文件存在则为 True，否则为 False。</returns>
    public static bool FileExists()
    {
        return FileExists(new ES3Settings());
    }

    /// <summary>检查指定路径的文件是否存在。</summary>
    /// <param name="filePath">要检查是否存在的文件的相对或绝对路径。</param>
    /// <returns>如果文件存在则为 True，否则为 False。</returns>
    public static bool FileExists(string filePath)
    {
        return FileExists(new ES3Settings(filePath));
    }

    /// <summary>检查指定路径的文件是否存在，使用提供的设置覆盖默认设置。</summary>
    /// <param name="filePath">要检查是否存在的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    /// <returns>如果文件存在则为 True，否则为 False。</returns>
    public static bool FileExists(string filePath, ES3Settings settings)
    {
        return FileExists(new ES3Settings(filePath, settings));
    }

    /// <summary>检查文件是否存在。</summary>
    /// <param name="settings">定义文件位置的设置。</param>
    /// <returns>如果文件存在则为 True，否则为 False。</returns>
    public static bool FileExists(ES3Settings settings)
    {
        switch (settings.location)
        {
            case Location.File:
                return ES3IO.FileExists(settings.FullPath); // 检查物理文件
            case Location.PlayerPrefs:
                return PlayerPrefs.HasKey(settings.FullPath); // 检查PlayerPrefs键
            case Location.Cache:
                return ES3File.FileExists(settings); // 检查缓存文件
            case Location.Resources:
                return Resources.Load(settings.FullPath) != null; // 检查Resources资源
            default:
                return false;
        }
    }

    /// <summary>检查指定路径的目录是否存在（使用默认设置）。</summary>
    /// <param name="folderPath">要检查是否存在的目录的相对或绝对路径。</param>
    /// <returns>如果目录存在则为 True，否则为 False。</returns>
    public static bool DirectoryExists(string folderPath)
    {
        return DirectoryExists(new ES3Settings(folderPath));
    }

    /// <summary>检查指定路径的目录是否存在，使用提供的设置覆盖默认设置。</summary>
    /// <param name="folderPath">要检查是否存在的目录的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    /// <returns>如果目录存在则为 True，否则为 False。</returns>
    public static bool DirectoryExists(string folderPath, ES3Settings settings)
    {
        return DirectoryExists(new ES3Settings(folderPath, settings));
    }

    /// <summary>检查目录是否存在。</summary>
    /// <param name="settings">定义目录位置的设置。</param>
    /// <returns>如果目录存在则为 True，否则为 False。</returns>
    public static bool DirectoryExists(ES3Settings settings)
    {
        if (settings.location == Location.File) // 文件系统位置
            return ES3IO.DirectoryExists(settings.FullPath); // 检查物理目录
        else if (settings.location == Location.PlayerPrefs ||
                 Application.platform == RuntimePlatform.WebGLPlayer) // 不支持的位置
            throw new System.NotSupportedException("PlayerPrefs 或 WebGL 不支持目录。");
        else if (settings.location == Location.Resources) // 不支持
            throw new System.NotSupportedException("不支持检查Resources中的文件夹是否存在。");
        return false;
    }

    /// <summary>获取默认文件中所有键名的数组。</summary>
    public static string[] GetKeys()
    {
        return GetKeys(new ES3Settings());
    }

    /// <summary>获取指定文件中所有键名的数组。</summary>
    /// <param name="filePath">要从中获取键名的文件的相对或绝对路径。</param>
    public static string[] GetKeys(string filePath)
    {
        return GetKeys(new ES3Settings(filePath));
    }

    /// <summary>获取指定文件中所有键名的数组，使用提供的设置覆盖默认设置。</summary>
    /// <param name="filePath">要从中获取键名的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static string[] GetKeys(string filePath, ES3Settings settings)
    {
        return GetKeys(new ES3Settings(filePath, settings));
    }

    /// <summary>获取文件中所有键名的数组。</summary>
    /// <param name="settings">定义文件位置的设置。</param>
    public static string[] GetKeys(ES3Settings settings)
    {
        if (settings.location == Location.Cache) // 缓存位置
            return ES3File.GetKeys(settings); // 从缓存文件获取键

        var keys = new List<string>(); // 创建键列表
        using (var reader = ES3Reader.Create(settings)) // 创建读取器
        {
            if (reader == null) // 文件不存在
                throw new System.IO.FileNotFoundException("无法从文件 " + settings.FullPath + " 获取键，因为文件不存在");

            foreach (string key in reader.Properties) // 遍历所有属性（键）
            {
                keys.Add(key); // 添加键名
                reader.Skip(); // 跳过该键的值（不解析）
            }
        }

        return keys.ToArray(); // 返回键名数组
    }

    /// <summary>获取默认目录中所有文件名的数组。</summary>
    public static string[] GetFiles()
    {
        var settings = new ES3Settings();
        if (settings.location == ES3.Location.File)
        {
            if (settings.directory == ES3.Directory.PersistentDataPath)
                settings.path = ES3IO.persistentDataPath; // 使用持久化数据路径
            else
                settings.path = ES3IO.dataPath; // 使用数据路径
        }

        return GetFiles(settings); // 调用核心方法
    }

    /// <summary>获取指定目录中所有文件名的数组。</summary>
    /// <param name="directoryPath">要从中获取文件名的目录的相对或绝对路径。</param>
    public static string[] GetFiles(string directoryPath)
    {
        return GetFiles(new ES3Settings(directoryPath));
    }

    /// <summary>获取指定目录中所有文件名的数组，使用提供的设置覆盖默认设置。</summary>
    /// <param name="directoryPath">要从中获取文件名的目录的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static string[] GetFiles(string directoryPath, ES3Settings settings)
    {
        return GetFiles(new ES3Settings(directoryPath, settings));
    }

    /// <summary>获取目录中所有文件名的数组。</summary>
    /// <param name="settings">定义目录位置的设置。</param>
    public static string[] GetFiles(ES3Settings settings)
    {
        if (settings.location == Location.Cache) // 缓存位置
            return ES3File.GetFiles(); // 获取缓存中的所有文件名
        else if (settings.location != ES3.Location.File) // 仅支持文件系统或缓存
            throw new System.NotSupportedException("ES3.GetFiles 只能在位置设置为文件(File)或缓存(Cache)时使用。");
        return ES3IO.GetFiles(settings.FullPath, false); // 获取文件系统目录中的文件（不包括子目录）
    }

    /// <summary>获取默认目录中所有子目录名的数组。</summary>
    public static string[] GetDirectories()
    {
        return GetDirectories(new ES3Settings());
    }

    /// <summary>获取指定目录中所有子目录名的数组。</summary>
    /// <param name="directoryPath">要从中获取子目录名的目录的相对或绝对路径。</param>
    public static string[] GetDirectories(string directoryPath)
    {
        return GetDirectories(new ES3Settings(directoryPath));
    }

    /// <summary>获取指定目录中所有子目录名的数组，使用提供的设置覆盖默认设置。</summary>
    /// <param name="directoryPath">要从中获取子目录名的目录的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static string[] GetDirectories(string directoryPath, ES3Settings settings)
    {
        return GetDirectories(new ES3Settings(directoryPath, settings));
    }

    /// <summary>获取目录中所有子目录名的数组。</summary>
    /// <param name="settings">定义目录位置的设置。</param>
    public static string[] GetDirectories(ES3Settings settings)
    {
        if (settings.location != ES3.Location.File) // 仅支持文件系统位置
            throw new System.NotSupportedException("ES3.GetDirectories 只能在位置设置为文件(File)时使用。");
        return ES3IO.GetDirectories(settings.FullPath, false); // 获取文件系统目录中的子目录（不包括子目录的子目录）
    }

    /// <summary>创建默认文件的备份。</summary>
    /// <remarks>备份是通过复制文件并给它一个 .bak 扩展名来创建的。
    /// 如果备份已存在，它将被覆盖，因此在调用此方法之前，你需要确保不再需要旧的备份。</remarks>
    public static void CreateBackup()
    {
        CreateBackup(new ES3Settings());
    }

    /// <summary>创建指定路径的文件的备份。</summary>
    /// <remarks>备份是通过复制文件并给它一个 .bak 扩展名来创建的。
    /// 如果备份已存在，它将被覆盖，因此在调用此方法之前，你需要确保不再需要旧的备份。</remarks>
    /// <param name="filePath">要创建备份的文件的相对或绝对路径。</param>
    public static void CreateBackup(string filePath)
    {
        CreateBackup(new ES3Settings(filePath));
    }

    /// <summary>创建指定路径的文件的备份，使用提供的设置覆盖默认设置。</summary>
    /// <remarks>备份是通过复制文件并给它一个 .bak 扩展名来创建的。
    /// 如果备份已存在，它将被覆盖，因此在调用此方法之前，你需要确保不再需要旧的备份。</remarks>
    /// <param name="filePath">要创建备份的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static void CreateBackup(string filePath, ES3Settings settings)
    {
        CreateBackup(new ES3Settings(filePath, settings));
    }

    /// <summary>创建指定文件的备份。</summary>
    /// <remarks>备份是通过复制文件并给它一个 .bak 扩展名来创建的。
    /// 如果备份已存在，它将被覆盖，因此在调用此方法之前，你需要确保不再需要旧的备份。</remarks>
    /// <param name="settings">定义要备份的文件的设置。</param>
    public static void CreateBackup(ES3Settings settings)
    {
        var backupSettings = new ES3Settings(settings.path + ES3IO.backupFileSuffix, settings); // 创建带.bak后缀的设置
        ES3.CopyFile(settings, backupSettings); // 复制文件到备份位置
    }

    /// <summary>恢复文件的备份。</summary>
    /// <param name="filePath">要恢复其备份的文件的相对或绝对路径。</param>
    /// <returns>如果备份被恢复则为 True，或者如果找不到备份则为 False。</returns>
    public static bool RestoreBackup(string filePath)
    {
        return RestoreBackup(new ES3Settings(filePath));
    }

    /// <summary>恢复文件的备份，使用提供的设置覆盖默认设置。</summary>
    /// <param name="filePath">要恢复其备份的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    /// <returns>如果备份被恢复则为 True，或者如果找不到备份则为 False。</returns>
    public static bool RestoreBackup(string filePath, ES3Settings settings)
    {
        return RestoreBackup(new ES3Settings(filePath, settings));
    }

    /// <summary>恢复文件的备份。</summary>
    /// <param name="settings">定义要恢复备份的文件的设置。</param>
    /// <returns>如果备份被恢复则为 True，或者如果找不到备份则为 False。</returns>
    public static bool RestoreBackup(ES3Settings settings)
    {
        var backupSettings = new ES3Settings(settings.path + ES3IO.backupFileSuffix, settings); // 备份文件设置（带.bak）

        if (!FileExists(backupSettings)) // 备份文件不存在
            return false;

        ES3.RenameFile(backupSettings, settings); // 将备份文件重命名为原始文件

        return true;
    }

    /// <summary>获取默认文件的时间戳（最后修改时间）。</summary>
    public static DateTime GetTimestamp()
    {
        return GetTimestamp(new ES3Settings());
    }

    /// <summary>获取指定路径的文件的时间戳（最后修改时间）。</summary>
    /// <param name="filePath">要获取时间戳的文件的相对或绝对路径。</param>
    public static DateTime GetTimestamp(string filePath)
    {
        return GetTimestamp(new ES3Settings(filePath));
    }

    /// <summary>获取指定路径的文件的时间戳（最后修改时间），使用提供的设置覆盖默认设置。</summary>
    /// <param name="filePath">要获取时间戳的文件的相对或绝对路径。</param>
    /// <param name="settings">用于覆盖默认设置的设置。</param>
    public static DateTime GetTimestamp(string filePath, ES3Settings settings)
    {
        return GetTimestamp(new ES3Settings(filePath, settings));
    }

    /// <summary>获取文件的最后更新时间戳（UTC 时区）。</summary>
    /// <param name="settings">定义文件位置的设置。</param>
    /// <returns>表示文件最后更新的UTC日期时间的DateTime对象。</returns>
    public static DateTime GetTimestamp(ES3Settings settings)
    {
        switch (settings.location)
        {
            case Location.File:
                return ES3IO.GetTimestamp(settings.FullPath); // 获取文件系统时间戳
            case Location.PlayerPrefs:
                // 从PlayerPrefs中存储的特殊键获取时间戳
                return new DateTime(long.Parse(PlayerPrefs.GetString("timestamp_" + settings.FullPath, "0")),
                    DateTimeKind.Utc);
            case Location.Cache:
                return ES3File.GetTimestamp(settings); // 获取缓存文件时间戳
            default:
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc); // 默认（纪元开始）
        }
    }

    /// <summary>将默认缓存文件存储到持久化存储。</summary>
    /// <remarks>备份是通过复制文件并给它一个 .bak 扩展名来创建的。
    /// 如果备份已存在，它将被覆盖，因此在调用此方法之前，你需要确保不再需要旧的备份。</remarks>
    public static void StoreCachedFile()
    {
        ES3File.Store(); // 存储默认缓存文件
    }

    /// <summary>将缓存文件存储到指定路径的持久化存储。</summary>
    /// <param name="filePath">要将缓存文件存储到的文件名或路径。</param>
    public static void StoreCachedFile(string filePath)
    {
        StoreCachedFile(new ES3Settings(filePath)); // 使用路径创建设置
    }

    /// <summary>将缓存文件存储到指定路径的持久化存储。</summary>
    /// <param name="filePath">要将缓存文件存储到的文件名或路径。</param>
    /// <param name="settings">目标文件的设置。</param>
    public static void StoreCachedFile(string filePath, ES3Settings settings)
    {
        StoreCachedFile(new ES3Settings(filePath, settings)); // 合并路径和设置
    }

    /// <summary>将缓存文件存储到持久化存储。</summary>
    /// <param name="settings">定义目标文件位置的设置。</param>
    public static void StoreCachedFile(ES3Settings settings)
    {
        ES3File.Store(settings); // 调用ES3File存储方法
    }

    /// <summary>将默认文件从持久化存储加载到缓存中。</summary>
    public static void CacheFile()
    {
        CacheFile(new ES3Settings());
    }

    /// <summary>将指定路径的文件从持久化存储加载到缓存中。</summary>
    /// <param name="filePath">要缓存的文件名或路径。</param>
    public static void CacheFile(string filePath)
    {
        CacheFile(new ES3Settings(filePath));
    }

    /// <summary>将指定路径的文件从持久化存储加载到缓存中。</summary>
    /// <param name="filePath">要缓存的文件名或路径。</param>
    /// <param name="settings">要缓存的文件的设置。</param>
    public static void CacheFile(string filePath, ES3Settings settings)
    {
        CacheFile(new ES3Settings(filePath, settings));
    }

    /// <summary>将文件从持久化存储加载到缓存中。</summary>
    /// <param name="settings">定义要缓存的文件的设置。</param>
    public static void CacheFile(ES3Settings settings)
    {
        ES3File.CacheFile(settings); // 调用ES3File缓存方法
    }

    /// <summary>初始化 Easy Save。在调用任何 ES3 方法时会自动发生，但如果你想在调用 ES3 方法之前执行初始化，则很有用。</summary>
    public static void Init()
    {
        var settings = ES3Settings.defaultSettings; // 访问默认设置（可能触发初始化）
        var pdp = ES3IO.persistentDataPath; // 访问持久化路径（初始化ES3IO，用于线程安全）
        ES3TypeMgr.Init(); // 初始化类型管理器（加载ES3Types）
    }

    #endregion
}