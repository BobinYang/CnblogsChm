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
            int cateArticle = 1;//用于类别下第一篇博文时创建目录
            int AuthorStart = 1;//昵称开始位置
            int AuthorEnd = 1;//昵称结束位置
            string postAuthor = null;//昵称
            //string url = "http://www.cnblogs.com/" + userId + "/mvc/blog/sidecolumn.aspx";//获取博客分类地址
            // 2019/10/26 23:13 更新获取博客分类地址
            string url = "http://www.cnblogs.com/" + userId + "/ajax/sidecolumn.aspx";//获取博客分类地址
            Console.WriteLine("正在连接服务器，请稍候...");
            //模版设置
            string divFormat = "<div style='width:100%;float:left;margin-top:20px;background-color:#399ab2;font-size:20px;color:White;padding-left:5px'>{0}</div>";
            string liFormat = "<li style='width:49%;float:left;line-height:30px;'><a href='{0}' target='_self' title='{1}'>{1}</a></li>";
            string template = File.ReadAllText(".//temChild.html");
            StringBuilder strIndex = new StringBuilder();//目录页
            //得到显示所有分类的页面 匹配出所有链接
            MatchCollection matchsCategories = Bloghelper.FilterC(url);
            if (matchsCategories == null)
            {
                Console.WriteLine("未找到任何类别，请检查UserID是否正确");
                Thread.Sleep(5000);
                return false;
            }
            const string category = "category";//分类
            foreach (Match mCategory in matchsCategories)
            {
                //获取类别信息
                //Console.WriteLine("正则过滤后的连接: {0}", mCategory.ToString());

                var categoryNameOrigin = mCategory.Groups["text"].Value;
                var categoryName = categoryNameOrigin.RemoveDirInvalid().Trim();
                var categoryUrl = mCategory.Groups["url"].Value;

                // 2018/5/29 20:53 替换类别链接中的https
                categoryUrl = categoryUrl.Replace("https", "http");
                var lstCategory = new List<string>();
                //categoryUrl = "http://www.cnblogs.com/Uest/category/897755.html";
                if (categoryUrl.IndexOf(userId + "/" + category, StringComparison.OrdinalIgnoreCase) >= 0 && !lstCategory.Contains(categoryUrl))
                {
                    Console.WriteLine();
                    //Console.WriteLine("正在读取类别: {0}", mCategory.Groups["text"]);
                    Console.WriteLine("正在读取类别: {0}", categoryName);
                    lstCategory.Add(categoryUrl);
                    /* 2017/7/18 18:58 先判断类别下是否有博文
                    //将该类别添加到目录
                    strIndex.AppendLine(divFormat.FormatString(mCategory.Groups["text"].Value));
                    strIndex.AppendLine("<ul style='padding-top:10px;clear:both;list-style:none;'>");
                    //以类别名称创建目录
                    Directory.CreateDirectory(".//cnblogs//" + obj.cateIndex + categoryName);
                    */
                    var lstArticleUrls = new List<string>();
                    //获取该类别下的所有随笔页面内容，得到每个随笔链接
                    MatchCollection matchPosts = Bloghelper.Filter(categoryUrl);
                    foreach (Match mPost in matchPosts)
                    {
                        //随笔链接
                        var articleUrl = mPost.Groups["url"].ToString();
                        var articleTitle = mPost.Groups["text"].ToString();

                        // 2018/5/29 20:53 替换随笔链接中的https
                        articleUrl = articleUrl.Replace("https", "http");
                        if ((articleUrl.IndexOf(userId + "/" + archive, StringComparison.OrdinalIgnoreCase) >= 0
                            || articleUrl.IndexOf(userId + "/p", StringComparison.OrdinalIgnoreCase) >= 0) && articleUrl.IndexOf('#') < 0 && !lstArticleUrls.Contains(articleUrl))
                        {
                            // 2017/7/18 18:58 仅当类别下有博文时
                            if (cateArticle == 1)
                            {
                                //将该类别添加到目录
                                strIndex.AppendLine(divFormat.FormatString(categoryNameOrigin));
                                strIndex.AppendLine("<ul style='padding-top:10px;clear:both;list-style:none;'>");
                                //以类别名称创建目录
                                Directory.CreateDirectory(".//cnblogs//" + obj.cateIndex + categoryName);
                                cateArticle++;
                            }

                            lstArticleUrls.Add(articleUrl);
                            var text = articleTitle.RemoveFileInvalid();
                            var autoName = (obj.fileIndex++).ToString();
                            //打印提示信息
                            Console.WriteLine("正在下载: {0}", articleTitle);
                            //下载随笔内容 替换后保存本地
                            var contentCode = Bloghelper.GetContent(articleUrl);//获取随笔内容
                            HtmlDocument htmlCode = new HtmlDocument();
                            htmlCode.LoadHtml(contentCode);
                            var titleNode = htmlCode.GetElementbyId("cb_post_title_url");
                            var postBody = htmlCode.GetElementbyId("cnblogs_post_body");
                            var postDate = htmlCode.GetElementbyId("post-date");
                            //var topics = htmlCode.GetElementbyId("topics");
                            // 2017/11/24 20:58 增加Author(昵称)Start
                            var postDetail = htmlCode.GetElementbyId("post_detail");
                            AuthorStart = postDetail.InnerHtml.IndexOf("post-date") + 73 + userId.Length + 4;
                            AuthorEnd = postDetail.InnerHtml.IndexOf("<", AuthorStart);
                            postAuthor = postDetail.InnerHtml.Substring(AuthorStart, AuthorEnd - AuthorStart);
                            // 2017/11/24 20:58 增加Author(昵称)End
                            if (titleNode == null || postBody == null || postDate == null)
                            {
                                Console.WriteLine("该随笔获取异常，请检查是否设置了只允许注册用户访问，或其他原因。");
                                continue;
                            }
                            var localHtml = template
                                //.Replace("{channelTitle}", titleNode.InnerText)//博文标题
                            .Replace("{channelTitle}", articleTitle)//博文标题
                            .Replace("{preContent}", Bloghelper.DownImage(postBody.InnerHtml, obj.imgIndex))//博文内容
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
                    }
                    //随笔类别结束UL标记
                    strIndex.AppendLine("</ul>");
                    obj.cateIndex++;
                    cateArticle = 1;
                }
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