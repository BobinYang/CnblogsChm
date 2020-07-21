/*************************************************************************************
 *
 * 文 件 名:   ReadFromXml
 * 描    述:
 *
 * 版    本：  V1.0
 * 创 建 者：  bobin.yang
 * 创建时间：  2020/7/21 13:59:20
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*************************************************************************************/

using Chen.DB;
using Chen.Ext;
using CnBlogsCHM.Entity;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;

namespace CnBlogsCHM
{
    public class ReaderFromXml
    {
        #region 得到博客对象

        public bool ReadCNBlogsXml(string path, ReturnObj obj)
        {
            string cateline = null;//用于xml生成目录前缀

            try
            {
                var xml = File.ReadAllText(path);//该xml为博客园随笔备份文件
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                //获取rss节点下的内容
                var channelXml = doc.SelectSingleNode("rss").InnerXml;
                //进一步细化xml格式，内容仅为rss节点下的内容
                doc.LoadXml(channelXml);
                //判断item对象个数
                var items = doc.SelectNodes("channel/item");
                if (items.Count == 1)//仅当随笔内容有1项时才会多此一举
                {
                    //1.如果仅有一个item,序列化后的json格式为 "item": {"title": "测试","link":"",....} 这样的json格式并不能序列化成List<item>的
                    //刻意去添加一个空节点 在序列化成json的格式为 "item":[{"title": "测试"...},{}...] 这样就可以序列化成List<item>了
                    var item = doc.CreateElement("item");
                    doc.FirstChild.AppendChild(item);
                }
                //将xml序列化成json，并且去掉根节点
                var json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented, true);
                var channel = JsonConvert.DeserializeObject<Channel>(json);
                obj.title = channel.title;
                //读取的xml没有类别名称
                foreach (var item in channel.items)
                {
                    item.typeName = DateTime.Parse(item.pubDate).ToString("yyyy年MM月");
                    //去掉标题中的#
                    item.title = item.title.RemoveFileInvalid();
                }

                #region 准备内容的html文件

                //加载html模板
                var template = File.ReadAllText(".//temChild.html");
                //定义目录表 结构
                DataTable dt = new DataTable();
                dt.Columns.Add("序号", typeof(int));
                dt.Columns.Add("标题", typeof(string));
                dt.Columns.Add("创建时间", typeof(string));
                foreach (var item in channel.items)
                {
                    //根据item.typeName是否相同，生成前缀
                    if (cateline == null)
                    {
                        cateline = item.typeName;
                    }
                    if (cateline != item.typeName)
                    {
                        obj.cateIndex++;
                        cateline = item.typeName;
                    }

                    item.description.content = Bloghelper.DownImage(item.description.content, obj.imgIndex);
                    //得到单个html正文
                    //以创建时间为目录分类html
                    var time = DateTime.Parse(item.pubDate);
                    var content = template
                        .Replace("{channelTitle}", item.title)//博文标题
                        .Replace("{preContent}", item.description.content)//博文内容
                        .Replace("{channelHref}", item.link)//博文地址
                        .Replace("{channelLink}", channel.link)//博客地址
                        .Replace("{channelPubDate}", DateTime.Parse(item.pubDate).ToString("yyyy-MM-dd HH:mm"))//发布时间
                        .Replace("{channelAuthor}", item.author);//博文作者
                    var dir = ".//cnblogs//" + obj.cateIndex + item.typeName;
                    Directory.CreateDirectory(dir);
                    //html的文件名
                    //File.WriteAllText(Path.Combine(dir, item.title + ".html"), content, Encoding.UTF8);
                    //参考输入博客ID序列化文件，并添加该文件的友好名称
                    File.WriteAllText(Path.Combine(dir, obj.fileIndex + ".html"), content, Encoding.UTF8);
                    obj.dicConvert.Add(Path.GetFullPath(Path.Combine(dir, obj.fileIndex + ".html")), item.title);
                    //处理完毕后 将该文件添加至目录表
                    var dr = dt.NewRow();
                    dr["序号"] = dt.Rows.Count + 1;
                    //dr["标题"] = "<a href=\"{0}/{1}.html\"'>{1}</a>".FormatString(item.typeName, item.title);
                    dr["标题"] = "<a href=\"{0}/{1}.html\"'>{2}</a>".FormatString(obj.cateIndex + item.typeName, obj.fileIndex, item.title);
                    dr["创建时间"] = DateTime.Parse(item.pubDate).ToString("yyyy-MM-dd HH:mm");
                    dt.Rows.Add(dr);
                    obj.fileIndex++;
                }
                //创建目录页
                dt.TableName = "<h3>随笔目录<h3>";
                DbCommon.CreateHtml(dt, false, ".//cnblogs//随笔目录.html");

                #endregion 准备内容的html文件

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion 得到博客对象
    }
}