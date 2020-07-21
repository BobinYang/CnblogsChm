/*************************************************************************************
 *
 * 文 件 名:   helper
 * 描    述:
 *
 * 版    本：  V1.0
 * 创 建 者：  bobin.yang
 * 创建时间：  2020/7/21 14:03:21
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*************************************************************************************/

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CnBlogsCHM
{
    public static class Bloghelper
    {
        // 提取网页文件中的图片链接
        public static string[] GetHtmlImageUrls(string sHtmlText)
        {
            // 定义正则表达式用来匹配 img 标签
            Regex regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);
            // 搜索匹配的字符串
            MatchCollection matches = regImg.Matches(sHtmlText);
            int i = 0;
            string[] sUrlList = new string[matches.Count];
            // 取得匹配项列表
            foreach (Match match in matches)
                sUrlList[i++] = match.Groups["imgUrl"].Value;
            return sUrlList;
        }

        //获取页面的内容
        public static string GetContent(string url, string param = "")
        {
            try
            {
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    url = "http://" + url;
                }
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                if (!string.IsNullOrEmpty(param))
                {
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded";
                    byte[] bs = Encoding.ASCII.GetBytes(param);
                    req.ContentLength = bs.Length;
                    using (Stream reqStream = req.GetRequestStream())
                    {
                        reqStream.Write(bs, 0, bs.Length);
                    }
                }

                HttpWebResponse HttpWResp = (HttpWebResponse)req.GetResponse();
                using (Stream myStream = HttpWResp.GetResponseStream())
                {
                    if (myStream == null)
                    {
                        return string.Empty;
                    }
                    using (StreamReader sr = new StreamReader(myStream, Encoding.UTF8))
                    {
                        if (sr.Peek() > 0)
                        {
                            return sr.ReadToEnd();
                        }
                    }
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        //过滤
        public static MatchCollection Filter(string url)
        {
            string content = GetContent(url);
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }
            string regex = @"<a(?:(?!href=).)*href=(['""]?)(?<url>[^""\s>]*)\1[^>]*>(?<text>(?:(?!</?a\b).)*)</a>";
            Regex rx = new Regex(regex);
            return rx.Matches(content);
        }

        // 2019/10/26 23:13 针对分类的过滤
        public static MatchCollection FilterC(string url)
        {
            string content = GetContent(url);
            //Console.WriteLine("urlxxx: {0}", content.ToString());
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }
            //string regex = @"<a href=(.)*category[\w\W]*?</a>";
            string regex = @"<a href=(['""]?)(?<url>[^""\s>]*)\1[^>]*>(?<text>([\w\W]*?))</a>";
            Regex rx = new Regex(regex);
            return rx.Matches(content);
        }

        //下载页面中的图片
        public static string DownImage(string html, int imgIndex)
        {        //系统自带图片
            string sysImage = "ContractedBlock.gif, ExpandedBlockStart.gif,copycode.gif";
            //这里要注意一点：CHM里图片的路径用单斜杠表示
            var urls = GetHtmlImageUrls(html);

            using (var client = new WebClient())
            {
                foreach (var url in urls)
                {
                    // 2017/8/08 22:10 data:image图片无需下载
                    if (url.IndexOf("data:image", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        var fileName = Path.GetFileName(url);
                        //首先判断该图片是常用图片
                        if (sysImage.Contains(fileName))
                        {
                            //自带图片不更名
                            if (File.Exists("./cnblogs/image//" + fileName))
                            {
                                fileName = "/image/" + fileName;
                                html = html.Replace(url, ".." + fileName);
                                continue;
                            }
                            else
                            {
                                //如果是系统自带图片，则保留命名
                                fileName = "/image/" + fileName;
                            }
                        }
                        else //若图片非系统自带，则随机命名，避免重名覆盖
                        {
                            fileName = "/image/" + (imgIndex++) + Path.GetExtension(url);
                        }
                        try
                        {
                            //client.DownloadFile(url, ".//cnblogs" + fileName);
                            // 2017/7/27 21:42 图片地址问题
                            client.DownloadFile(url.Replace("images.cnitblog.com", "images0.cnblogs.com"), ".//cnblogs" + fileName);
                            html = html.Replace(url, ".." + fileName);
                        }
                        catch
                        {
                            Console.WriteLine(url + " 图片下载失败！");
                        }
                    }
                }
                return html;
            }
        }
    }
}