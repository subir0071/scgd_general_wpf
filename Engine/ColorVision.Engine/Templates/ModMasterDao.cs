﻿using ColorVision.Engine.MySql.ORM;
using ColorVision.Engine.Templates.SysDictionary;
using System;
using System.Collections.Generic;
using System.Data;

namespace ColorVision.Engine.Templates
{
    public class ModMasterModel : PKModel
    {
        public ModMasterModel() : this(-1, "", 0) { }

        public ModMasterModel(int pid, string text, int tenantId)
        {
            Pid = pid;
            Name = text;
            TenantId = tenantId;
            CreateDate = DateTime.Now;
        }

        public string? Name { get; set; }
        public DateTime? CreateDate { get; set; } = DateTime.Now;
        public bool? IsEnable { get; set; } = true;
        public bool? IsDelete { get; set; } = false;
        public string? Remark { get; set; }
        public int TenantId { get; set; }
        public int Pid { get; set; }

        public int? ResourceId { get; set; }

        public string? Pcode { get; set; }
    }


    public class ModMasterDao : BaseViewDao<ModMasterModel>
    {
        public static ModMasterDao Instance { get; set; } = new ModMasterDao();

        private static Dictionary<string,int> keyValuePairs = new Dictionary<string,int>();

        public int Pid { get; set; }

        public ModMasterDao() : base("v_scgd_mod_master", "t_scgd_mod_param_master", "id", true)
        {
            Pid = -1;
        }

        public ModMasterDao(string code) : base("v_scgd_mod_master", "t_scgd_mod_param_master", "id", true)
        {
            if (keyValuePairs.TryGetValue(code, out int pid))
            {
                Pid = pid;
            }
            else
            {
                pid = string.IsNullOrWhiteSpace(code) ? -1 : SysDictionaryModMasterDao.Instance.GetByCode(code, 0)?.Id ?? -1;
                keyValuePairs.Add(code, pid);
                Pid = pid;
            }
        }

        public ModMasterDao(int pid) : base("v_scgd_mod_master", "t_scgd_mod_param_master", "id", true)
        {
            Pid = pid;
        }


        public override DataTable GetTableAllByTenantId(int tenantId)
        {

            string sql;
            if (string.IsNullOrEmpty(ViewName)) sql = $"select * from {TableName} where is_delete=0 and tenant_id={tenantId} and pid={Pid}";
            else sql = $"select * from {ViewName} where is_delete=0 and tenant_id={tenantId} and pid={Pid}";
            DataTable d_info = GetData(sql);
            return d_info;
        }


        public override ModMasterModel GetModelFromDataRow(DataRow item)
        {
            ModMasterModel model = new ModMasterModel()
            {
                Id = item.Field<int>("id"),
                Name = item.Field<string?>("name"),
                CreateDate = item.Field<DateTime?>("create_date"),
                IsEnable = item.Field<bool?>("is_enable"),
                IsDelete = item.Field<bool?>("is_delete"),
                Remark = item.Field<string?>("remark"),
                Pcode = item.Field<string>("pcode"),
                Pid = item.Field<int>("pid"),
                ResourceId = item.Field<int?>("res_pid"),
            };

            return model;
        }

        public override DataRow Model2Row(ModMasterModel item, DataRow row)
        {
            if (item != null)
            {
                if (item.Id > 0) row["id"] = item.Id;
                if (item.Name != null) row["name"] = item.Name;
                row["create_date"] = item.CreateDate;
                if (item.Remark != null) row["remark"] = item.Remark;
                row["tenant_id"] = item.TenantId;
                row["res_pid"] = item.ResourceId ?? -1;
                row["mm_id"] = item.Pid;
            }
            return row;
        }

        public DataTable GetTableAllByTenantIdAdnResId(int tenantId, int resourceId)
        {
            string sql = $"select * from {GetTableName()} where tenant_id={tenantId} and pid='{Pid}' and res_pid={resourceId}" + GetDelSQL(true);
            DataTable d_info = GetData(sql);
            return d_info;
        }

        public List<ModMasterModel> GetResourceAll(int tenantId, int resourceId)
        {
            List<ModMasterModel> list = new();
            DataTable d_info = GetTableAllByTenantIdAdnResId(tenantId, resourceId);
            foreach (var item in d_info.AsEnumerable())
            {
                ModMasterModel? model = GetModelFromDataRow(item);
                if (model != null)
                {
                    list.Add(model);
                }
            }
            return list;
        }


        public new List<ModMasterModel> GetAll(int tenantId)
        {
            List<ModMasterModel> list = new();
            DataTable d_info = GetTableAllByTenantId(tenantId);
            foreach (var item in d_info.AsEnumerable())
            {
                ModMasterModel? model = GetModelFromDataRow(item);
                if (model != null)
                {
                    list.Add(model);
                }
            }
            return list;
        }


    }
}
