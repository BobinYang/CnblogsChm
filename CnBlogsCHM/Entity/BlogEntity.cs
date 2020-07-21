/*************************************************************************************
 *
 * 文 件 名:   BlogEntity
 * 描    述:
 *
 * 版    本：  V1.0
 * 创 建 者：  bobin.yang
 * 创建时间：  2020/7/21 14:00:40
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*************************************************************************************/

using Newtonsoft.Json;
using System.Collections.Generic;

namespace CnBlogsCHM.Entity
{
    #region 结构定义

    public class Channel
    {
        public string title { get; set; }
        public string link { get; set; }
        public string description { get; set; }
        public string language { get; set; }
        public string lastBuildDate { get; set; }
        public string pubDate { get; set; }
        public string ttl { get; set; }

        [JsonProperty("item")]
        public List<Channel_Item> items { get; set; }
    }

    public class Channel_Item
    {
        public string title { get; set; }
        public string link { get; set; }
        public string author { get; set; }
        public string pubDate { get; set; }
        public string guid { get; set; }
        public Item_Description description { get; set; }

        [JsonIgnore]
        public string typeName { get; set; }
    }

    public class Item_Description
    {
        //默认以变量名称作为json序列化的节点，由于该节点内容不符合变量定义规范，则显示指定即可
        [JsonProperty("#cdata-section")]
        public string content { get; set; }
    }

    #endregion 结构定义
}