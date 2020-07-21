using System;
using System.IO;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public class ExtensionAttribute : Attribute { }
}

namespace Chen.Ext
{
    /// <summary>
    /// 字符串拓展方法
    /// </summary>
    public static partial class ExtendMethod
    {
        #region 重写Format方法

        /// <summary>
        /// 重写Format方法
        /// </summary>
        /// <param name="s"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string FormatString(this string s, params object[] args)
        {
            return string.Format(s, args);
        }

        #endregion 重写Format方法

        public static string RemoveFileInvalid(this string str)
        {
            var err = Path.GetInvalidFileNameChars();
            foreach (var e in err)
            {
                if (str.IndexOf(e) == 0)
                {
                    str = str.Remove(0, 1).Replace(e, '%');
                }
                else break;
            }
            return str.Replace("c#", "csharp").Replace("C#", "csharp").Replace("#", string.Empty).Replace("\\", " ").Replace("/", " ");
        }

        /// <summary>
        /// 处理要创建文件夹名不核发的字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveDirInvalid(this string str)
        {
            //以类别名称为目录，如果目录中有无效字符在首位则直接移除，其他位置的直接替换为%
            var err = Path.GetInvalidPathChars();
            foreach (var e in err)
            {
                if (str.IndexOf(e) == 0)
                {
                    str = str.Remove(0, 1).Replace(e, '%');
                }
                else break;
            }
            //另外检查首位的. 字符串中的/ \
            if (str.IndexOf('.') == 0) str = str.Remove(0, 1);
            str = str.Replace("/", " ").Replace("\\", " ");
            return str.Replace("c#", "csharp").Replace("C#", "csharp").Replace("#", string.Empty);
        }
    }

    public static class DirectoryUtil
    {
        #region 将指定文件夹下面的所有内容copy到目标文件夹下

        /****************************************
          * 函数名称：CopyDir
          * 功能说明：将指定文件夹下面的所有内容copy到目标文件夹下面 果目标文件夹为只读属性就会报错。
          * 参     数：srcPath:原始路径,aimPath:目标文件夹
          * 调用示列：
          *            string srcPath = Server.MapPath("test/");
          *            string aimPath = Server.MapPath("test1/");
          *            EC.FileObj.CopyDir(srcPath,aimPath);
         *****************************************/

        /// <summary>
        /// 指定文件夹下面的所有内容copy到目标文件夹下面 目标文件夹为只读属性就会报错。
        /// </summary>
        /// <param name="srcPath">原始路径</param>
        /// <param name="aimPath">目标文件夹</param>
        public static void CopyDir(string srcPath, string aimPath)
        {
            try
            {
                // 检查目标目录是否以目录分割字符结束如果不是则添加之
                if (aimPath[aimPath.Length - 1] != Path.DirectorySeparatorChar)
                    aimPath += Path.DirectorySeparatorChar;
                // 判断目标目录是否存在如果不存在则新建之
                if (!Directory.Exists(aimPath))
                    Directory.CreateDirectory(aimPath);
                // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
                //如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法
                //string[] fileList = Directory.GetFiles(srcPath);
                string[] fileList = Directory.GetFileSystemEntries(srcPath);
                //遍历所有的文件和目录
                foreach (string file in fileList)
                {
                    //先当作目录处理如果存在这个目录就递归Copy该目录下面的文件

                    if (Directory.Exists(file))
                        CopyDir(file, aimPath + Path.GetFileName(file));
                    //否则直接Copy文件
                    else
                        File.Copy(file, aimPath + Path.GetFileName(file), true);
                }
            }
            catch (Exception ee)
            {
                throw new Exception(ee.ToString());
            }
        }

        #endregion 将指定文件夹下面的所有内容copy到目标文件夹下
    }
}