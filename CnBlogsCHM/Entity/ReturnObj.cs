/*************************************************************************************
 *
 * 文 件 名:   ReturnObj
 * 描    述:
 *
 * 版    本：  V1.0
 * 创 建 者：  bobin.yang
 * 创建时间：  2020/7/21 14:26:22
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*************************************************************************************/

using System.Collections.Generic;

namespace CnBlogsCHM.Entity
{
    public class ReturnObj
    {
        public ReturnObj()
        {
            imgIndex = 1;
            title = "我的CHM";
            fileIndex = 10001;
            cateIndex = 101;
            dicConvert = new Dictionary<string, string>();
        }

        /// <summary>
        /// 图片数量(作为图片文件名使用)，默认从1开始
        /// </summary>
        public int imgIndex { get; set; }

        /// <summary>
        /// CHM标题，默认我的CHM
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 文件数量,从10001开始
        /// </summary>
        public int fileIndex { get; set; }

        /// <summary>
        /// 目录数量，从101开始
        /// </summary>
        public int cateIndex { get; set; }//

        /// <summary>
        ///
        /// </summary>
        public Dictionary<string, string> dicConvert { get; set; }
    }
}