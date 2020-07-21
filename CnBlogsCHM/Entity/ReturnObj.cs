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
            imgIndex = 1;//图片数量(作为图片文件名使用)
            title = "我的CHM";//CHM标题
            fileIndex = 10001;//文件数量
            cateIndex = 101;//目录数量
            dicConvert = new Dictionary<string, string>();
        }

        public int imgIndex { get; set; }//图片数量(作为图片文件名使用)
        public string title { get; set; }//CHM标题
        public int fileIndex { get; set; }//文件数量
        public int cateIndex { get; set; }//目录数量
        public Dictionary<string, string> dicConvert { get; set; }
    }
}