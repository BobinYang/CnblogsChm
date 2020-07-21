using Chen.Common;
using Chen.Ext;
using CnBlogsCHM.Entity;
using System;
using System.IO;

namespace CnBlogsCHM
{
    internal class Program
    {
        //Main
        private static void Main(string[] args)
        {
            ReturnObj obj = new ReturnObj();

            Console.WriteLine("将博客备份的xml拖到此处或者直接输入登录用户名，回车即可：");
            var input = Console.ReadLine().TrimEnd();
            bool ret;
            //创建html存储根目录
            Directory.CreateDirectory("./cnblogs/image");
            //复制content文件夹到根目录下
            DirectoryUtil.CopyDir(".//content", ".//cnblogs//content");
            //根据输入
            if (File.Exists(input))
            {
                ReaderFromXml readerXml = new ReaderFromXml();
                ret = readerXml.ReadCNBlogsXml(input, obj);
            }
            else
            {
                ReaderByID rederID = new ReaderByID();
                ret = rederID.ReadBlogsByID(input, obj);
            }
            if (!ret)
            {
                Console.WriteLine("程序异常");
                return;
            }

            //编译CHM文档
            ChmHelp chm = new ChmHelp();
            chm.RootPath = ".//cnblogs";
            chm.ChmFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), obj.title + ".chm");
            chm.DefaultPage = "随笔目录.html";
            chm.Title = obj.title;
            chm.dicConvert = obj.dicConvert;
            chm.Compile();
            Console.WriteLine("生成完毕 文件位于" + chm.ChmFileName);
            Directory.Delete(".//cnblogs", true);
            Console.WriteLine("下载文件数量:{0}个 下载图片数量:{1}个", obj.fileIndex - 10001, obj.imgIndex - 1);
            Console.ReadKey();
        }
    }
}