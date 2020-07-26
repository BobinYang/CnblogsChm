/*************************************************************************************
 *
 * 文 件 名:   ReaderByID
 * 描    述:
 *
 * 版    本：  V1.0
 * 创 建 者：  bobin.yang
 * 创建时间：  2020/7/21 14:02:08
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*************************************************************************************/

using Chen.Ext;
using CnBlogsCHM.Entity;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CnBlogsCHM
{
    public class ReaderByID
    {
        public bool ReadBlogsByID(string userId, ReturnObj obj)
        {
            const string archive = "archive";//随笔 2013-06-17之后，随笔保存于http://www.cnblogs.com/userId/p/

            int AuthorStart = 1;//昵称开始位置
            int AuthorEnd = 1;//昵称结束位置
            string postAuthor = null;//昵称

            //模版设置
            string divFormat = "<div style='width:100%;float:left;margin-top:20px;background-color:#399ab2;font-size:20px;color:White;padding-left:5px'>{0}</div>";
            string liFormat = "<li style='width:49%;float:left;line-height:30px;'><a href='{0}' target='_self' title='{1}'>{1}</a></li>";
            string template = File.ReadAllText(".//temChild.html");


            //获取分类目录
            string urlCate = "http://www.cnblogs.com/" + userId + "/ajax/sidecolumn.aspx";//获取博客分类地址
            Console.WriteLine("正在连接服务器，请稍候...");

            string contentCate = Bloghelper.GetContent(urlCate);

            if (string.IsNullOrEmpty(contentCate))
            {
                return false;
            }


            HtmlDocument docCate = new HtmlDocument();
            docCate.LoadHtml(contentCate);
            var cateNode = docCate.GetElementbyId("sidebar_categories");
            var cateNodes = cateNode.SelectNodes("./ul/li/a/");



            if (cateNodes == null)
            {
                Console.WriteLine("未找到任何类别，请检查UserID是否正确");
                Thread.Sleep(5000);
                return false;
            }

            StringBuilder strIndex = new StringBuilder();//目录页 ,得到显示所有分类的页面 匹配出所有链接
            int cateArticle;
            foreach (HtmlNode mCategory in cateNodes)
            {   cateArticle = 1;//用于类别下第一篇博文时创建目录
                var categoryNameOrigin = mCategory.InnerText;
                var categoryName = categoryNameOrigin.RemoveDirInvalid().Trim();
                var categoryUrl = mCategory.Attributes["href"].Value;

                categoryUrl = categoryUrl.Replace("https", "http");

                Console.WriteLine();
                Console.WriteLine("正在读取类别: {0}", categoryName);

                /* 2017/7/18 18:58 先判断类别下是否有博文
                //将该类别添加到目录
                strIndex.AppendLine(divFormat.FormatString(mCategory.Groups["text"].Value));
                strIndex.AppendLine("<ul style='padding-top:10px;clear:both;list-style:none;'>");
                //以类别名称创建目录
                Directory.CreateDirectory(".//cnblogs//" + obj.cateIndex + categoryName);
                */
              
                string contentItems = Bloghelper.GetContent(categoryUrl);//获取该类别下的所有随笔页面内容，得到每个随笔链接
                HtmlDocument contentItemsDoc = new HtmlDocument();
                contentItemsDoc.LoadHtml(contentItems);
                var contentNode = contentItemsDoc.GetElementbyId("content");
                var matchPosts = contentNode.SelectNodes("./div[@class=\"post\"]");
                int i = 0;

                foreach (var mPost in matchPosts)
                {
                    var articleUrl = mPost.SelectSingleNode("h5/a").Attributes["href"].Value; //随笔链接
                    var articleTitle = mPost.SelectSingleNode("h5/a/span").InnerText;
                    var articleDate = mPost.SelectSingleNode("p[@class=\"postfoot\"]/a").InnerText;

                    articleUrl = articleUrl.Replace("https", "http");

 
                    if (cateArticle == 1)
                    {
                        //将该类别添加到目录
                        strIndex.AppendLine(divFormat.FormatString(categoryNameOrigin));
                        strIndex.AppendLine("<ul style='padding-top:10px;clear:both;list-style:none;'>");
                        //以类别名称创建目录
                        Directory.CreateDirectory(".//cnblogs//" + obj.cateIndex + categoryName);
                        cateArticle++;
                    }

                    var text = articleTitle.RemoveFileInvalid();
                    var autoName = (obj.fileIndex++).ToString();
                    //打印提示信息
                    Console.WriteLine("正在下载: {0}:{1} {2}", i++, articleTitle, articleDate);
                    //下载随笔内容 替换后保存本地
                    var contentCode = Bloghelper.GetContent(articleUrl);//获取随笔内容
                    if (contentCode == null) continue;
                    HtmlDocument htmlCode = new HtmlDocument();
                    htmlCode.LoadHtml(contentCode);
                    var titleNode = htmlCode.GetElementbyId("cb_post_title_url");
                    var postBody = htmlCode.GetElementbyId("cnblogs_post_body");
                    var postDate = htmlCode.GetElementbyId("post-date");
                    //var topics = htmlCode.GetElementbyId("topics");
         
                    var postDetail = htmlCode.GetElementbyId("post_detail");
                    AuthorStart = postDetail.InnerHtml.IndexOf("post-date") + 73 + userId.Length + 4;
                    AuthorEnd = postDetail.InnerHtml.IndexOf("<", AuthorStart);
                    postAuthor = postDetail.InnerHtml.Substring(AuthorStart, AuthorEnd - AuthorStart);

                    if (titleNode == null || postBody == null || postDate == null)
                    {
                        Console.WriteLine("该随笔获取异常，请检查是否设置了只允许注册用户访问，或其他原因。");
                        continue;
                    }
                    var localHtml = template
                    //.Replace("{channelTitle}", titleNode.InnerText)//博文标题
                    .Replace("{channelTitle}", articleTitle)//博文标题
                    .Replace("{preContent}", postBody.InnerHtml)
                    //  .Replace("{preContent}", Bloghelper.DownImage(postBody.InnerHtml, obj.imgIndex))//博文内容
                    //.Replace("{channelHref}", titleNode.GetAttributeValue("href", "#"))//博文地址
                    .Replace("{channelHref}", articleUrl)//博文地址
                    .Replace("{channelLink}", "http://www.cnblogs.com/" + userId + "/")//博客地址
                    .Replace("{channelPubDate}", postDate.InnerText)//发布时间
                    .Replace("{channelAuthor}", postAuthor);//博文作者userId
                    var fileName = "./cnblogs/" + obj.cateIndex + categoryName + "/" + autoName + ".html";
                    File.WriteAllText(fileName, localHtml, Encoding.UTF8);
                    //添加目录项
                    var relativePath = obj.cateIndex + categoryName + "/" + autoName + ".html";
                    strIndex.AppendLine(liFormat.FormatString(relativePath, text));
                    //添加该文件的友好名称
                    obj.dicConvert.Add(Path.GetFullPath(fileName), articleTitle);

                }
                //随笔类别结束UL标记
                strIndex.AppendLine("</ul>");
                obj.cateIndex++;
            }
            //创建完毕后写入目录
            if (strIndex.Length == 0)
            {
                Console.WriteLine("抓取不到这个博客的随笔分类！");
                Thread.Sleep(5000);
                return false;
            }
            //更换模版
            template = File.ReadAllText(".//temChild.html");
            var content = template.Replace("<a href='../随笔目录.html' style='float: right;font-size: 12px;font-weight: normal;'>返回目录</a>", string.Empty)
            .Replace("{channelTitle}", postAuthor + "-随笔目录")//博文标题userId
            .Replace("{preContent}", strIndex.ToString())//博文内容
            .Replace("{channelHref}", "")//博文地址
            .Replace("{channelLink}", "http://www.cnblogs.com/" + userId + "/")//博客地址
            .Replace("{channelPubDate}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"))//生成时间
            .Replace("{channelAuthor}", postAuthor);//博文作者userId
            File.WriteAllText(".//cnblogs//随笔目录.html", content, Encoding.UTF8);
            obj.title = postAuthor + "-博客园";//CHM标题userId
            return true;
        }
    }
}