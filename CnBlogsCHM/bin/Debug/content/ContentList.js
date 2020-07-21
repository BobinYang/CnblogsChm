// 生成目录索引列表
// ref: http://www.cnblogs.com/wangqiguo/p/4355032.html
function GenerateContentList()
{
	var mainContent = $('#cnblogs_post_body');
	var h2_list = $('#cnblogs_post_body h2');//如果你的章节标题不是h2,只需要将这里的h2换掉即可

	if(mainContent.length < 1)
		return;

	if(h2_list.length>0)
	{
		var content = '<div id="ContentList">';
		content += '<p style="font-size:20px"><b>本文目录</b></p>';
		content += '<ul style="margin:0 0 0 30px;">';
		for(var i=0; i<h2_list.length; i++)
		{
			var ii = i+1;//序号从1开始，方便对应
			//var go_to_top = '<div style="float: right;font-size:12px;margin: 5px 0;"><a href="#top">返回顶部</a><a name="section' + ii + '"></a></div>';
			var go_to_top = '<a name="section' + ii + '"></a>';
			$(h2_list[i]).before(go_to_top);
		   
			var h3_list = $(h2_list[i]).nextAll("h3");
			var li3_content = '';
			for(var j=0; j<h3_list.length; j++)
			{
				var jj = j+1;//序号从1开始，方便对应
				var tmp = $(h3_list[j]).prevAll('h2').first();
				if(!tmp.is(h2_list[i]))
					break;
				var li3_anchor = '<a name="section' + ii + '.' + jj + '"></a>';
				$(h3_list[j]).before(li3_anchor);
				li3_content += '<li><a href="#section' + ii + '.' + jj + '" style="font-size:14px;">' + $(h3_list[j]).text() + '</a></li>';
			}
		   
			var li2_content = '';
			if(li3_content.length > 0)
				li2_content = '<li><a href="#section' + ii + '" style="font-size:14px;">' + $(h2_list[i]).text() + '</a><ul style="margin:0 0 0 30px;">' + li3_content + '</ul></li>';
			else
				li2_content = '<li><a href="#section' + ii + '" style="font-size:14px;">' + $(h2_list[i]).text() + '</a></li>';
			content += li2_content;
		}
		content += '</ul>';         
		content += '<hr style="border: silver 1px dashed;">';
		if($('#cnblogs_post_body').length != 0 )
		{
			$($('#cnblogs_post_body')[0]).prepend(content);
		}
	}
}

$(function () {
    //生成目录索引列表
    GenerateContentList();
});