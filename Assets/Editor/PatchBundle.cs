using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;

public class PatchBundle
{
    public static string sourcePath = Application.dataPath + "/Resources";
    const string AssetBundlesOutputPath = "Assets/OutPut";

    private static Dictionary<string, string> strDicFiles = new Dictionary<string, string>();

    private static List<string> AllResFiles = new List<string>();    //原始资源包的所有文件

    [MenuItem("Custom/BuildAssetBundle")]
    public static void BuildAssetBundle()
    {
        ClearAssetBundlesName();
        Pack(sourcePath);
        string outputPath = Path.Combine(AssetBundlesOutputPath, Platform.GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget));
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        //根据BuildSetting里面所激活的平台进行打包 设置过AssetBundleName的都会进行打包
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        AssetDatabase.Refresh();
        //BuildScenes();
        CheckFileMD5();
        CreateFileCRC();
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        Debug.Log("刷新包完成");
    }

    /// <summary>
    /// 用来获取解压文件配置表暂时弃用
    /// </summary>
    [MenuItem("Custom/LoadAllResFiles")]
    public static void LoadAllResFiles()
    {
        AllResFiles.Clear();
        CheckFileMD5ForStreamingAssets();
        CreateFileCRCForStreamingAssets();
        AssetDatabase.Refresh();
        LoadResAllFiles(Application.streamingAssetsPath);
        WriteAllRedFileInfo();
        AssetDatabase.Refresh();
        Debug.Log("资源刷新完成");
    }

    /// <summary>
    /// 清除之前设置过的AssetBundleName，避免产生不必要的资源也打包
    /// 之前说过，只要设置了AssetBundleName的，都会进行打包，不论在什么目录下
    /// </summary>
    static void ClearAssetBundlesName()
    {
        int length = AssetDatabase.GetAllAssetBundleNames().Length;
        Debug.Log(length);
        string[] oldAssetBundleNames = new string[length];
        for (int i = 0; i < length; i++)
        {
            oldAssetBundleNames[i] = AssetDatabase.GetAllAssetBundleNames()[i];
            EditorUtility.DisplayProgressBar("读取标识", oldAssetBundleNames[i], (float)i / (float)length);
        }

        for (int j = 0; j < oldAssetBundleNames.Length; j++)
        {
            AssetDatabase.RemoveAssetBundleName(oldAssetBundleNames[j], true);
            EditorUtility.DisplayProgressBar("清除标识", oldAssetBundleNames[j],(float)j/ (float)oldAssetBundleNames.Length);
        }
        length = AssetDatabase.GetAllAssetBundleNames().Length;
        Debug.Log(length);
    }

    static void Pack(string source)
    {
        //Debug.Log("Pack source " + source);
        DirectoryInfo folder = new DirectoryInfo(source);
        FileSystemInfo[] files = folder.GetFileSystemInfos();
        int length = files.Length;
        for (int i = 0; i < length; i++)
        {
            if (files[i] is DirectoryInfo)
            {
                Pack(files[i].FullName);
            }
            else
            {
                if (!files[i].Name.EndsWith(".meta"))
                {
                    fileWithDepends(files[i].FullName);
                }
            }
        }
    }

    /// <summary>
    /// 设置需要打包的文件
    /// </summary>
    /// <param name="source"></param>
    static void fileWithDepends(string source)
    {
        Debug.Log("file source " + source);
        string _source = Replace(source);
        string _assetPath = "Assets" + _source.Substring(Application.dataPath.Length);

        Debug.Log(_assetPath);
        EditorUtility.DisplayProgressBar("设置打包", _assetPath, (float)1 / 1);
        //自动获取依赖项并给其资源设置AssetBundleName
        string[] dps = AssetDatabase.GetDependencies(_assetPath);
        foreach (var dp in dps)
        {
            Debug.Log("dp " + dp);
            if (dp.EndsWith(".cs"))
                continue;
            AssetImporter assetImporter = AssetImporter.GetAtPath(dp);
            string pathTmp = dp.Substring("Assets".Length + 1);
            string assetName = pathTmp.Substring(pathTmp.IndexOf("/") + 1);
            if(Path.GetExtension(assetName)!=".unity")
                assetName = assetName.Replace(Path.GetExtension(assetName), ".data");
            else
                assetName = assetName.Replace(Path.GetExtension(assetName), ".unity3d");
            Debug.Log(assetName);
            assetImporter.assetBundleName = assetName;
        }

    }

    //设置要打包的文件
    /*static void file(string source)
    {
        Debug.Log("file source " + source);
        string _source = Replace(source);
        string _assetPath = "Assets" + _source.Substring(Application.dataPath.Length);
        string _assetPath2 = _source.Substring(Application.dataPath.Length + 1);

        //在代码中给资源设置AssetBundleName
        AssetImporter assetImporter = AssetImporter.GetAtPath(_assetPath);
        string[] dps = AssetDatabase.GetDependencies(_assetPath);
        foreach (var dp in dps)
        {
            Debug.Log("dp " + dp);
        }
        string assetName = _assetPath2.Substring(_assetPath2.IndexOf("/") + 1);
        assetName = assetName.Replace(Path.GetExtension(assetName), ".unity3d");

        Debug.Log(assetName);
        assetImporter.assetBundleName = assetName;
    }*/

    /// <summary>
    /// 除去\\
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    static string Replace(string s)
    {
        return s.Replace("\\", "/");
    }

    /// <summary>
    /// 创建版本文件路径StreamingAssets
    /// </summary>
    static void CheckFileMD5ForStreamingAssets()
    {
        strDicFiles.Clear();
        string OutputPath = Application.streamingAssetsPath;
        OutputPath = OutputPath + "/" + Platform.GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget);
        CheckCRC(OutputPath);
        foreach (var item in strDicFiles)
        {
            Debug.Log(item.Key + ":" + item.Value.ToString());
        }
    }
    /// <summary>
    /// 创建版本文件路径OutPut
    /// </summary>
    static void CheckFileMD5()
    {
        strDicFiles.Clear();
        string OutputPath = Application.dataPath + "/OutPut";
        OutputPath = OutputPath + "/" + Platform.GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget);
        CheckCRC(OutputPath);
        foreach (var item in strDicFiles)
        {
            Debug.Log(item.Key +":"+ item.Value.ToString());
        }
    }

    /// <summary>
    /// 检查所有文件获取CEC并添加入字典
    /// </summary>
    /// <param name="strPath"></param>
    static void CheckCRC(string strPath)
    {
        DirectoryInfo folder = new DirectoryInfo(strPath);
        FileSystemInfo[] files = folder.GetFileSystemInfos();
        int length = files.Length;
        for (int i = 0; i < length; i++)
        {
            if (files[i] is DirectoryInfo)
            {
                CheckCRC(files[i].FullName);
            }
            else if (IsFileType(files[i].Name, "data"))
            {
                uint crc = 0;
                BuildPipeline.GetCRCForAssetBundle(files[i].FullName, out crc);
                string strLoaclPath = Application.dataPath + "/" + "OutPut/" + Platform.GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget) + "/";
                string strFilePath = Replace(files[i].FullName);
                string FileName = strFilePath.Replace(strLoaclPath, "");
                strDicFiles.Add(FileName, crc.ToString());
            }
            else if (IsFileType(files[i].Name, "unity3d"))
            {
                string crc = GetMD5HashFromFile(files[i].FullName);
                string strLoaclPath = Application.dataPath + "/" + "OutPut/" + Platform.GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget) + "/";
                string strFilePath = Replace(files[i].FullName);
                string FileName = strFilePath.Replace(strLoaclPath, "");
                strDicFiles.Add(FileName, crc);
            }
        }
    }

    /// <summary>
    /// 检查文件类型
    /// </summary>
    /// <param name="strFileName"></param>
    /// <param name="fileType"></param>
    /// <returns></returns>
    static bool IsFileType(string strFileName,string fileType)
    {
        string [] strNames = strFileName.Split('.');
        string strType = strNames[strNames.Length - 1];
        if (string.Compare(strType, fileType) == 0)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 获取文件MD5值
    /// </summary>
    /// <param name="fileName">文件绝对路径</param>
    /// <returns>MD5值</returns>
    public static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
        }
    }

    /// <summary>
    /// 生成新的资源CRC表路径OutPut
    /// </summary>
    static void CreateFileCRC()
    {
        string strAssetPath = Application.dataPath + "/OutPut/";
        string outputPath = Path.Combine(strAssetPath, Platform.GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget));
        FileStream fs = new FileStream(outputPath + "/Varsion.txt", FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);
        string strVarsion = "1.0.0";
        sw.WriteLine("Varsion:"+ strVarsion);

        foreach (var item in strDicFiles)
        {
            string lineData = "";
            lineData = string.Format("{0}\t{1}",item.Key,item.Value);
            sw.WriteLine(lineData);
        }
        sw.Flush();
        sw.Close();
        fs.Close();
    }

    /// <summary>
    /// 生成新的资源CRC表路径StreamingAssets
    /// </summary>
    static void CreateFileCRCForStreamingAssets()
    {
        string strAssetPath = Application.streamingAssetsPath + "/";
        string outputPath = Path.Combine(strAssetPath, Platform.GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget));
        FileStream fs = new FileStream(outputPath + "/Varsion.txt", FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);
        string strVarsion = "1.0.0";
        sw.WriteLine("Varsion:" + strVarsion);

        foreach (var item in strDicFiles)
        {
            string lineData = "";
            lineData = string.Format("{0}\t{1}",
                item.Key,
                item.Value
                );
            sw.WriteLine(lineData);
        }

        sw.Flush();
        sw.Close();
        fs.Close();
    }

    /// <summary>
    /// 读取资源包目录下所有文件
    /// </summary>
    /// <param name="DirectoryPath"></param>
    static void LoadResAllFiles(string DirectoryPath)
    {
        DirectoryInfo folder = new DirectoryInfo(DirectoryPath);
        FileSystemInfo[] files = folder.GetFileSystemInfos();
        int length = files.Length;
        for (int i = 0; i < length; i++)
        {
            if (files[i] is DirectoryInfo)
            {
                LoadResAllFiles(files[i].FullName);
            }
            else
            {
                if (!files[i].Name.EndsWith(".meta"))
                {
                    string FileFullPath = files[i].FullName.Replace("\\","/");
                    string strFilePath = FileFullPath.Replace(Application.streamingAssetsPath, "");
                    AllResFiles.Add(strFilePath);
                }
            }
        }
    }

    /// <summary>
    /// 为BaseFileInfo写入资源文件
    /// </summary>
    static void WriteAllRedFileInfo()
    {
        FileStream fs = new FileStream(Application.streamingAssetsPath + "/BaseFileInfo.txt", FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);

        foreach (var item in AllResFiles)
        {
            string lineData = "";
            lineData = string.Format("{0}",item);
            sw.WriteLine(lineData);
        }
        sw.Flush();
        sw.Close();
        fs.Close();
    }
}

public class Platform
{
    public static string GetPlatformFolder(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "IOS";
            //case BuildTarget.WebPlayer:
            //    return "WebPlayer";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Win";
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSX:
                return "OSX";
            default:
                return null;
        }
    }
}
