﻿using ColorVision.Engine.MySql.ORM;
using ColorVision.Engine.Services.Templates.POI;
using ColorVision.Engine.Templates.POI.AlgorithmImp;
using Newtonsoft.Json;
using System.Data;

namespace ColorVision.Engine.Templates.POI.Dao
{
    public class PoiDetailModel : PKModel
    {
        [Column("name")]
        public string? Name { get; set; }
        [Column("pid")]
        public int? Pid { get; set; }
        [Column("pt_type")]
        public RiPointTypes Type { get; set; }
        [Column("pix_x")]
        public int? PixX { get; set; }
        [Column("pix_y")]
        public int? PixY { get; set; }
        [Column("pix_width")]
        public int? PixWidth { get; set; }
        [Column("pix_height")]
        public int? PixHeight { get; set; }
        [Column("remark")]
        public string? Remark { get; set; }

        [Column("val_validate_temp_id")]
        public int? ValidateTId { get; set; }

        public PoiDetailModel()
        {

        }

        public PoiDetailModel(int pid, PoiPoint data)
        {
            Id = data.Id;
            Pid = pid;
            Name = data.Name;
            Type = data.PointType;
            PixX = (int)data.PixX;
            PixY = (int)data.PixY;
            PixWidth = (int)data.PixWidth;
            PixHeight = (int)data.PixHeight;
            ValidateTId = data.ValidateTId;
            if (EditPoiParamConfig.Instance.PoiPointParamType != PoiPointParamType.Empty)
            {
                Remark = JsonConvert.SerializeObject(data.Param);
            }
        }
    }


    public class PoiDetailDao : BaseTableDao<PoiDetailModel>
    {
        public static PoiDetailDao Instance { get; } = new PoiDetailDao();

        public PoiDetailDao() : base("t_scgd_algorithm_poi_template_detail", "id")
        {
        }
    }
}
