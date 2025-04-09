using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace Su
{
    public class Template
    {
        /// <summary>
        /// 設定時記得加上結尾的分隔符號
        /// </summary>
        public static string TemplateRoot { get; set; }

        private string _Template;
        public string OriginalFilename;
        string SubTemplateKey;
        Template ParentTemplate;

        private ArrayList _alChildren;
        public ArrayList alChildren
        {
            get
            {
                if (this._alChildren == null)
                    this._alChildren = new ArrayList();

                return this._alChildren;
            }
        }

        /// <summary>
        /// lang 只有在使用暫存時有效
        /// </summary>
        /// <param name="strFileNameOrTemplate"></param>
        /// <param name="IsFromFile"></param>
        /// <param name="SubTemplateKey"></param>
        /// <param name="IsFromCache"></param>
        /// <param name="lang"></param>
        public Template(string strFileNameOrTemplate, bool IsFromFile = true, string SubTemplateKey = null, bool IsFromCache = true, string lang = "")
        {
            if (IsFromFile)
            {
                if (!IsFullFilename(strFileNameOrTemplate))
                {
                    strFileNameOrTemplate = TemplateRoot + strFileNameOrTemplate;
                }

                if (IsFromCache)
                    this._Template = FileUtility.GetFileWithCache(strFileNameOrTemplate, lang);
                else
                    this._Template = System.IO.File.ReadAllText(strFileNameOrTemplate);

                this.OriginalFilename = strFileNameOrTemplate;
            }
            else
            {
                this._Template = strFileNameOrTemplate;
            }

            this.SubTemplateKey = SubTemplateKey;

            this.RemoveBlock("RemoveThis");
        }

        public bool IsFullFilename(string filename)
        {
            if (filename.Contains(":") || filename.StartsWith("/"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 一率讀 Cache
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="language"></param>
        public Template(string filename, string language = "")
        {
            if (!IsFullFilename(filename))
            {
                filename = TemplateRoot.AddPath(filename);
            }
            _Template = FileUtility.GetFileWithCache(filename, language);
            RemoveBlock("RemoveThis");
        }

        public Template SetTemplate(string strTemplate)
        {
            this._Template = strTemplate;
            return this;
        }

        public Template ClearTemplate()
        {
            this._Template = "";
            return this;
        }

        public Template RemoveBlock(string Key, bool IsThrowExceptionIfNotFound = false)
        {
            this._Template = TextFns.RemoveBlock(_Template, Key, IsThrowExceptionIfNotFound);

            return this;
        }

        /// <summary>
        /// 可以和 ReplaceTemplateInString 一起使用
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Key"></param>
        /// <param name="IsRemoveTemplate"></param>
        /// <returns></returns>
        public static string GetTemplateFromString(ref string Source, string Key, bool IsRemoveTemplate = true)
        {
            string StartKey = "<!--" + Key + " S-->"; // 再精簡一點
            string EndKey = "<!--" + Key + " E-->";

            int StartP = Source.IndexOf(StartKey);
            if (StartP == -1)
                throw new Exception("Cant find start position for template !! " + StartKey + " | " + Source);

            // 跳到註解的下一個字元
            StartP = StartP + StartKey.Length;

            int EndP = Source.IndexOf(EndKey, StartP);

            if (EndP == -1)
                throw new Exception("Cant find end position for template !!" + EndKey);

            string Template = Source.Substring(StartP, EndP - StartP);

            if (IsRemoveTemplate)
                Source = Source.Substring(0, StartP) + Source.Substring(EndP);// 保留註解, 做為把 Template 放回來的位置

            return Template;
        }


        /// <summary>
        /// 請注意, 這個和 child 是不同的, 它不會自動加到 child 中.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="IsRemoveTemplate">是否要清空 Template 的內容(但會保留標記)</param>
        /// <param name="IsAddToChildren"></param>
        /// <returns></returns>
        public Template SubTemplate(string Key, bool IsRemoveTemplate = true, bool IsAddToChildren = false)
        {
            Template oT = new Template(Template.GetTemplateFromString(ref this._Template, Key, IsRemoveTemplate), false, Key);
            oT.ParentTemplate = this;
            oT.OriginalFilename = this.OriginalFilename;

            if (IsAddToChildren)
            {
                alChildren.Add(oT);
            }

            return oT;
        }

        /// <summary>
        /// 會建立 child, 加入 _alChildren, 傳回 Child 本身
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Template IsolateSingleChild(string key)
        {
            return SubTemplate(key, true, true);
        }

        /// <summary>
        /// 會自動 Isolate, 並且找不到時會傳回 Nothing 
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="IsTryIsolate"></param>
        /// <returns></returns>
        public Template Child(string Key, bool IsTryIsolate = true)
        {
            if (_alChildren == null && IsTryIsolate)
            {
                try
                {
                    return this.IsolateSingleChild(Key);
                }
                catch (Exception)
                {
                }
            }

            // 檢查是否已存在,
            if (_alChildren != null)
            {
                foreach (Template oT in _alChildren)
                {
                    if (oT.SubTemplateKey == Key)
                        return oT;
                }
            }

            // 從 Template 中找新的.
            if (IsTryIsolate)
            {
                try
                {
                    // 沒找到直接傳回 nothing 比較方便後續處理, Bike 20101022
                    return this.IsolateSingleChild(Key);
                }
                catch (Exception)
                {
                }
            }

            return null/* TODO Change to default(_) if this is not a reference type */;
        }


        /// <summary>
        /// 移除 child
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public Template RemoveChild(string Key)
        {
            foreach (Template oT in _alChildren)
            {
                if (oT.SubTemplateKey == Key)
                {
                    _alChildren.Remove(oT);
                    return this;
                }
            }

            return this;
        }

        public Template ReplaceString(string OldStr, string NewStr)
        {
            if (this._Template != null)
            {
                this._Template = this._Template.Replace(OldStr, NewStr);
            }
            return this;
        }

        public Template Bind(DataRow row)
        {

            foreach (DataColumn col in row.Table.Columns)
            {
                this._Template = this._Template.Replace("{" + col.ColumnName + "}", Convert.IsDBNull(row[col.ColumnName]) ? "" : row[col.ColumnName].ToString());
            }
            return this;
        }

        public Template Bind(DataTable dt)
        {
            var res = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                var T = this._Template;
                foreach (DataColumn col in dt.Columns)
                {
                    T = T.Replace("{" + col.ColumnName + "}", Convert.IsDBNull(row[col.ColumnName]) ? "" : row[col.ColumnName].ToString());
                }
                res.Add(T);
            }
            this._Template = string.Join("", res);
            return this;
        }

        public Template Bind(DataTable dt, string RemovePostFix, string extendFields = "")
        {
            var res = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                var T = this._Template;
                if (extendFields.Length > 0)
                {
                    T = ReplaceExtendedFields(T, row, extendFields);
                }

                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName.EndsWith(RemovePostFix))
                    {
                        T = T.Replace("{" + col.ColumnName.Replace(RemovePostFix, "") + "}", Convert.IsDBNull(row[col.ColumnName]) ? "" : row[col.ColumnName].ToString());
                    }
                    else
                    {
                        T = T.Replace("{" + col.ColumnName + "}", Convert.IsDBNull(row[col.ColumnName]) ? "" : row[col.ColumnName].ToString());
                    }
                }
                res.Add(T);
            }
            this._Template = string.Join("", res);
            return this;
        }

        public string ReplaceExtendedFields(string strTemplate, DataRow row, string extendFields)
        {
            var res = strTemplate;
            if (extendFields.Length > 0)
            {
                foreach (string key in extendFields.Split(','))
                {
                    string op = key.Split('_')[0];
                    string field = key.Split('_')[1];

                    res = ReplaceForSpecialKey(strTemplate, key, row[field]);
                }
            }

            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strTemplate"></param>
        /// <param name="key">Is_, Is_Not_, IsPositiveOrZero_, IsNotPositiveOrZero_, IsPositive_, IsNotPositive_, IsEmpty_, IsNotEmpty_</param>
        /// <param name="value"></param>
        /// <param name="isDBNullZero"></param>
        /// <returns></returns>
        public string ReplaceForSpecialKey(string strTemplate, string key, object value, bool isDBNullZero = true)
        {
            var res = strTemplate;
            if (key.StartsWith("Is_"))
            {
                if (value.ToString() == "Y")
                {
                    res = TextFns.RemoveBlock(_Template, key.Replace("Is_", "Is_Not_"));
                    res = TextFns.RemoveBlock(_Template, key.Replace("Is_", "IsNot_"));
                }

                if (value.ToString() == "N")
                {
                    res = TextFns.RemoveBlock(_Template, key);
                }
            }

            if (key.StartsWith("IsPositiveOrZero_"))
            {
                if((value.IsDBNull() && isDBNullZero)||value.IsDBNull() && (Int32)value >= 0)
                {
                    res = TextFns.RemoveBlock(_Template, key.Replace("IsPositiveOrZero_", "IsNotPositiveOrZero_"));
                }
                else
                {
                    res = TextFns.RemoveBlock(_Template, key);
                }
            }

            if (key.StartsWith("IsPositive_"))
            {
                if (!value.IsDBNull() && (Int32)value > 0)
                {
                    res = TextFns.RemoveBlock(_Template, key.Replace("IsPositive_", "IsNotPositive_"));
                }
                else
                {
                    res = TextFns.RemoveBlock(_Template, key);
                }
            }

            if (key.StartsWith("IsEmpty_"))
            {
                if (value.ToString().Length == 0)
                {
                    res = TextFns.RemoveBlock(_Template, key.Replace("IsEmpty_", "IsNotEmpty_"));
                }
                else
                {
                    res = TextFns.RemoveBlock(_Template, key);
                }
            }

            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="RemovePostFix">通常是語言</param>
        /// <param name="crlfToBrFields"></param>
        /// <returns></returns>
        public Template Bind(object source, string RemovePostFix, string crlfToBrFields = "")
        {
            crlfToBrFields = $",{crlfToBrFields},";
            var sourceProps = source.GetType().GetProperties().Where(x => x.CanRead).ToList();
            foreach (var srcItem in sourceProps)
            {
                var item = srcItem.GetValue(source, null);
                if (item == null)
                {                    
                    if (srcItem.Name.EndsWith(RemovePostFix))
                    {
                        _Template = _Template.Replace("{" + srcItem.Name.Replace(RemovePostFix, "") + "}", "");
                    }
                    else
                    {
                        _Template = _Template.Replace("{" + srcItem.Name + "}", "");
                    }
                }
                else
                {

                    if (srcItem.Name.EndsWith(RemovePostFix))
                    {
                        _Template = _Template.Replace("{" + srcItem.Name.Replace(RemovePostFix, "") + "}",
                            crlfToBrFields.Contains($",{srcItem.Name.Replace(RemovePostFix, "")},") ? item.ToString().CRLFtoBR() : item.ToString());
                    }
                    else
                    {
                        _Template = _Template.Replace("{" + srcItem.Name + "}",
                            crlfToBrFields.Contains($",{srcItem.Name},") ? item.ToString().CRLFtoBR() : item.ToString()); ;
                    }
                }
            }

            var sourceFields = source.GetType().GetFields().ToList();

            foreach (var srcItem in sourceFields)
            {
                var item = srcItem.GetValue(source);
                if (item == null)
                {
                    if (srcItem.Name.EndsWith(RemovePostFix))
                    {
                        _Template = _Template.Replace("{" + srcItem.Name.Replace(RemovePostFix, "") + "}", "");
                    }
                    else
                    {
                        _Template = _Template.Replace("{" + srcItem.Name + "}", "");
                    }
                }
                else
                {
                    if (srcItem.Name.EndsWith(RemovePostFix))
                    {
                        _Template = _Template.Replace("{" + srcItem.Name.Replace(RemovePostFix, "") + "}", item.ToString());
                    }
                    else
                    {
                        _Template = _Template.Replace("{" + srcItem.Name + "}", item.ToString());
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// 取代 property 和 field
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Template Bind(object source)
        {
            var sourceProps = source.GetType().GetProperties().Where(x => x.CanRead).ToList();
            foreach (var srcItem in sourceProps)
            {
                var item = srcItem.GetValue(source, null);
                if(item == null)
                {
                    _Template = _Template.Replace("{" + srcItem.Name + "}", "");
                }
                else
                {
                    _Template = _Template.Replace("{" + srcItem.Name + "}", item.ToString());
                }
            }

            var sourceFields = source.GetType().GetFields().ToList();

            foreach (var srcItem in sourceFields)
            {
                var item = srcItem.GetValue(source);
                if (item == null)
                {
                    _Template = _Template.Replace("{" + srcItem.Name + "}", "");
                }
                else
                {
                    _Template = _Template.Replace("{" + srcItem.Name + "}", item.ToString());
                }
            }

            return this;
        }


        public string result
        {
            get
            {
                return _Template;
            }
            set
            {
                _Template = value;
            }
        }

        public string ResultAfterBuildAll_TRIM
        {
            get
            {
                return ResultAfterBuildAll.Trim();
            }
        }

        public string ResultAfterBuildAll
        {
            get
            {
                this.BuildALL();
                return _Template;
            }
        }

        /// <summary>
        /// 這和會 Bind Parent Template, 並且一直向上 Bind.
        /// </summary>
        /// <returns></returns>
        public Template BuildALL()
        {
            if (this.ParentTemplate != null)
            {
                this.ParentTemplate.BuildALL();
            }
            else
            {
                BuildChildren();
                //this.Build(); 若是有 PlaceHolder 或 Literal Control, 必需把資料填入.
            }

            // UW.WU.DebugWriteLine("BuildALL End: " + Now.ToString("ss.fff"))

            return this;
        }

        /// <summary>
        /// 把 Child 加回來
        /// </summary>
        /// <returns></returns>
        public Template BuildChildren()
        {
            if (_alChildren != null)
            {
                foreach (Template oT in _alChildren)
                {
                    oT.BuildChildren();
                    //oT.Build(); // 若是有 PlaceHolder 或 Literal Control, 必需把資料填入.

                    this.Bind(oT, false); // 把 child 的文字填入 _template 之中
                }
            }

            return this;
        }

        ///// <summary>
        ///// 把 template 的文字填入 place holder 或是 literal control
        ///// </summary>
        //public void Build()
        //{
        //    //.NetCore 應該不會有 place holder 或是 literal control

        //    //// UW.WU.DebugWriteLine(Me.SubTemplateKey & " Build,  Me._oL IsNot Nothing = " & (Me._oL IsNot Nothing).ToString & ", Me._ph IsNot Nothing=" & (Me._ph IsNot Nothing).ToString)

        //    //if (this._oL != null)
        //    //    this._oL.Text = this._Template;

        //    //if (this._ph != null)
        //    //{
        //    //    this._ph.Controls.RemoveAt(0);
        //    //    this._ph.Controls.Add(new LiteralControl(this._Template));
        //    //}
        //}

        public Template Bind(Template oChild, bool IsThrowExceptionIfTemplateNotFound = true)
        {
            // UW.WU.DebugWriteLine("Bind, SubTemplateKey: '" & oChild.SubTemplateKey & "', OriginalFilename in Child: " & oChild.OriginalFilename & ", NewParentOriginalFilename: " & NewParentOriginalFilename, , True)
            this._Template = this._Template.Replace("<!--" + oChild.SubTemplateKey + " S--><!--" + oChild.SubTemplateKey + " E-->", oChild.result);

            //if (IsFastBuild)
            //    this._Template = this._Template.Replace("<!--" + oChild.SubTemplateKey + " S--><!--" + oChild.SubTemplateKey + " E-->", oChild.Result);
            //else
            //{
            //    string OriginalFileMemo = "";

            //    if (oChild.OriginalFilename != this.OriginalFilename)
            //        OriginalFileMemo = "<!--OriginalFilename: " + oChild.OriginalFilename + "-->";

            //    // UW.WU.DebugWriteLine((oChild.OriginalFilename <> NewParentOriginalFilename).ToString & ", OriginalFileMemo: " & OriginalFileMemo, IsHtmlEncode:=True)

            //    UW.Template.ReplaceTemplateInString(this._Template, oChild.SubTemplateKey, OriginalFileMemo + oChild.Result, IsThrowExceptionIfTemplateNotFound);
            //}

            return this;
        }
    }
}
