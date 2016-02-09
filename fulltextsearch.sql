
use ichup
 SELECT top 100 
             FT_TBL.keywords,FT_TBL.filter_1,FT_TBL.filter_2,FT_TBL.filter_3,FT_TBL.filter_4,FT_TBL.filter_5,link, KEY_TBL.RANK FROM images AS FT_TBL INNER JOIN FREETEXTTABLE(images, keywords,'ninh phu') AS KEY_TBL ON FT_TBL.id = KEY_TBL.[KEY] 
			 where 1=1 and (filter_1 like N'%vector%') and (filter_2 like N'%ngang%' or filter_2 like N'%rộng%') order by Rank Desc

			 SELECT top 100 
             FT_TBL.id,FT_TBL.link,FT_TBL.link_thumbail_small,FT_TBL.total_views,FT_TBL.keywords,FT_TBL.filter_1,FT_TBL.filter_2,FT_TBL.filter_3,FT_TBL.filter_4,FT_TBL.filter_5,KEY_TBL.RANK FROM images AS FT_TBL INNER JOIN FREETEXTTABLE(images, keywords,'doc quyen') AS KEY_TBL ON FT_TBL.id = KEY_TBL.[KEY] 
			 where 1=1 order by Rank Desc
	 

		select * from images