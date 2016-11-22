using iTextSharp.text.pdf.parser;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace PdfReadTest
{
    public class CustomStrategy : LocationTextExtractionStrategy
    {
        /**
         * Creates a new text extraction renderer.
         */

        public CustomStrategy() : this(new TextChunkLocationStrategyDefaultImp())
        {
        }

        /**
         * Creates a new text extraction renderer, with a custom strategy for
         * creating new TextChunkLocation objects based on the input of the
         * TextRenderInfo.
         * @param strat the custom strategy
         */

        public CustomStrategy(ITextChunkLocationStrategy strat) : base(strat)
        {
            tclStrat = strat;
        }

        private readonly ITextChunkLocationStrategy tclStrat;
        public Dictionary<int, string> dicBlockColName = null;
        public bool IsHaveColName = false;
        private List<TextChunk> Chunks = new List<TextChunk>();
        public List<AirportPoint> Points = new List<AirportPoint>();

        public List<AirportRoute> Routes = new List<AirportRoute>();

        public override void RenderText(TextRenderInfo renderInfo)
        {
            base.RenderText(renderInfo);

            LineSegment segment = renderInfo.GetBaseline();
            if (renderInfo.GetRise() != 0)
            { // remove the rise from the baseline - we do this because the text from a super/subscript render operations should probably be considered as part of the baseline of the text the super/sub is relative to
                Matrix riseOffsetTransform = new Matrix(0, -renderInfo.GetRise());
                segment = segment.TransformBy(riseOffsetTransform);
            }
            TextChunk tc = new TextChunk(renderInfo.GetText(), tclStrat.CreateLocation(renderInfo, segment));
            Chunks.Add(tc);
        }

        /// <summary>
        /// 根据pdf读取信息初始化数据
        /// </summary>
        /// <returns></returns>
        public string InitData()
        {
            //判断本页是否是航路点或航路
            if (!Chunks.Any())
                return "没有可处理数据";

            var firstChunk = Chunks.First();
            if (firstChunk.Text.Contains("航路点"))
            {
                return InitTablePoint();
            }
            else if (firstChunk.Text.Contains("编码表"))
            {
                return InitRoute();
            }
            else
            {
                return "本页不是航路点或航路";
            }
        }

        public string InitPoint()
        {
            if (!Chunks.Any())
                return "没有需要处理的航路点";

            string firstText = GetFirstLineText();
            //页面没有标题时，判断第一行格式是否为航路点
            Regex reg = new Regex(@"\w+\d{1,2}°\d{1,2}'\d{1,2}(.\d+)?""[NS]\d{1,3}°\d{1,2}'\d{1,2}(.\d+)?""[EW]");
            if (firstText.Contains("航路点坐标"))
            {
                return InitTablePoint();
            }
            else if (firstText.Contains("航路点"))
            {
                return InitBlockPoint();
            }
            else if (reg.IsMatch(firstText))
            {
                return InitBlockPoint();
            }

            return "本页不是航路点";
        }

        /// <summary>
        /// 初始化航路点数据
        /// </summary>
        /// <returns></returns>
        public string InitTablePoint()
        {
            TextChunk lastChunk = null;
            int row = 1;
            int col = 0;
            StringBuilder msg = new StringBuilder();

            StringBuilder lastColText = new StringBuilder();
            Dictionary<int, string> colName = new Dictionary<int, string>();
            AirportPoint point = new AirportPoint();
            //跳过的行
            int contiRow = 0;

            try
            {
                foreach (var chunk in Chunks)
                {
                    if (lastChunk == null)
                    {
                        col = 1;
                        lastChunk = chunk;
                    }

                    //判断是否换行
                    if (!chunk.SameLine(lastChunk))
                    {
                        row++;
                        col = 1;

                        //换行时，如果有上次读取的字符串为匹配，则改行数据匹配错误
                        if (!string.IsNullOrEmpty(lastColText.ToString()))
                            msg.AppendLine(string.Format("第{0}行，解析出错，内容：{1}", row, lastColText.ToString()));
                    }

                    if (row == contiRow)
                        continue;

                    string val = chunk.Text;

                    //判断是否为需要跳过的行
                    if (IsContiRow(val))
                    {
                        contiRow = row;
                        continue;
                    }

                    //对内容预处理
                    string preColText = string.Empty;
                    string fieldName = string.Empty;
                    if (colName.Keys.Contains(col))
                    {
                        fieldName = colName[col];
                    }
                    preColText = PreDisposeText(ref lastColText, ref val, fieldName);

                    //如果遇到空格则认为进入下一列
                    if (val.Contains(" "))
                    {
                        col++;

                        if (!string.IsNullOrEmpty(preColText))
                        {
                            //根据列名，设置对应的字段名
                            bool ret = SetPointColName(col - 1, preColText, ref colName);
                            //若为标题行，则不进行后续处理
                            if (!ret)
                            {
                                if (!string.IsNullOrEmpty(fieldName))
                                {
                                    SetModeFieldVal<AirportPoint>(point, fieldName, preColText);

                                    if (!string.IsNullOrEmpty(point.PointNo) &&
                                        !string.IsNullOrEmpty(point.LatLong))
                                    {
                                        Points.Add(point);
                                        point = new AirportPoint();
                                    }
                                }
                            }
                        }
                        else
                        {
                            //航路点中间列不存在空列，如果遇到空列变从1重新开始
                            col = 1;
                        }
                    }
                    else
                    {
                        //未遇到空格前不能确认单元格内容是否已读取完全，先保存内容
                        lastColText.Append(val);
                    }

                    lastChunk = chunk;
                }
            }
            catch (Exception ex)
            {
                msg.AppendLine(string.Format("解析出现异常：{0}", ex.Message));
                Console.Write(ex.Message);
            }

            return msg.ToString();
        }

        /// <summary>
        /// 非表格形式的航路点
        /// </summary>
        /// <returns></returns>
        public string InitBlockPoint()
        {
            if (null == dicBlockColName)
                dicBlockColName = new Dictionary<int, string>();
            TextChunk lastChunk = null;
            int row = 1;
            int col = 1;
            StringBuilder msg = new StringBuilder();
            AirportPoint point = new AirportPoint();

            try
            {
                StringBuilder blockText = new StringBuilder();
                bool isLast = false;
                foreach (var chunk in Chunks)
                {
                    if (lastChunk == null)
                    {
                        lastChunk = chunk;
                        col = 1;
                        continue;
                    }

                    blockText.Append(lastChunk.Text);

                    //最后一块检测不到结尾，特殊处理
                    if (Chunks.IndexOf(chunk) == Chunks.Count - 1)
                    {
                        isLast = true;
                        blockText.Append(chunk.Text);
                    }
                    if (IsChunkAtWordBoundary(chunk, lastChunk) || isLast)
                    {
                        string preColText = blockText.ToString().Trim();
                        blockText = new StringBuilder();
                        col++;

                        if (!string.IsNullOrEmpty(preColText))
                        {
                            bool ret = SetPointColName(col - 1, preColText, ref dicBlockColName);
                            //若为标题行，则不进行后续处理
                            if (!ret)
                            {
                                string fieldName = string.Empty;
                                if (dicBlockColName.Keys.Contains(col - 1))
                                {
                                    fieldName = dicBlockColName[col - 1];

                                    SetModeFieldVal(point, fieldName, preColText);

                                    if (!string.IsNullOrEmpty(point.PointNo) &&
                                       !string.IsNullOrEmpty(point.Lat) &&
                                       !string.IsNullOrEmpty(point.Long))
                                    {
                                        //转化经纬度
                                        string err = point.ConvertLatLong();
                                        if (string.IsNullOrEmpty(err))
                                        {
                                            Points.Add(point);
                                            point = new AirportPoint();
                                        }
                                        else
                                        {
                                            msg.AppendLine(string.Format("航路点(编号：{0} 纬度：{1} 经度：{2})经纬度解析失败。", point.PointNo, point.Long, point.Lat));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            col = 1;
                        }
                    }
                    //判断是否换行
                    if (!chunk.SameLine(lastChunk))
                    {
                        row++;
                        col = 1;
                        blockText = new StringBuilder();
                        point = new AirportPoint();
                    }

                    lastChunk = chunk;
                }
            }
            catch (Exception ex)
            {
                msg.AppendLine(string.Format("解析出现异常：{0}", ex.Message));
            }

            return msg.ToString();
        }

        /// <summary>
        /// 处理字符串带空格的情况
        /// </summary>
        /// <param name="lastColText"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private string SplitPointStr(ref StringBuilder lastColText, string val)
        {
            string preColText = string.Empty;
            if (val.StartsWith(" "))
            {
                preColText = lastColText.ToString();
                lastColText = new StringBuilder(val.Trim());
            }
            else if (val.EndsWith(" "))
            {
                lastColText.Append(val.Trim());
                preColText = lastColText.ToString();
                lastColText = new StringBuilder();
            }

            return preColText;
        }

        /// <summary>
        /// 预先处理字符串，对中间含空格的字符串做处理
        /// </summary>
        /// <param name="lastColText"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private string PreDisposeText(ref StringBuilder lastColText, ref string val, string fieldName)
        {
            string preColText = string.Empty;
            if (!val.Contains(" "))
                return preColText;

            if (string.IsNullOrEmpty(fieldName))
            {
                preColText = SplitPointStr(ref lastColText, val);
            }
            else
            {
                //编号被拆开的情况特殊处理
                Regex regNo = new Regex(@"\w+ N\w*");
                if (regNo.IsMatch(val))
                {
                    string[] vals = val.Split(' ');
                    lastColText.Append(vals[0]);
                    val = string.Format(" {0}", vals[1]);
                }

                //判断当前保存值是否满足要求
                if (CheckPointFieldValue(lastColText.ToString(), fieldName))
                {
                    preColText = lastColText.ToString();
                    lastColText = new StringBuilder(val.Trim());
                }
                else
                {
                    //处理空格后的数据是否满足要求
                    StringBuilder newText = new StringBuilder(lastColText.ToString());
                    if (val.StartsWith(" ") || val.EndsWith(" "))
                    {
                        newText.Append(val.Trim());
                        if (CheckPointFieldValue(newText.ToString(), fieldName))
                        {
                            preColText = newText.ToString();
                            val = " ";
                            lastColText = new StringBuilder();
                        }
                        else
                        {
                            preColText = string.Empty;
                            val = val.Trim();
                        }
                    }
                    else
                    {
                        List<string> vals = val.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        bool isMatch = false;
                        for (int i = 0; i < vals.Count; i++)
                        {
                            //将字符逐个加入，检测是否满足条件;满足条件后剩余的项放入下列
                            newText.Append(vals[i].Trim());
                            if (CheckPointFieldValue(newText.ToString(), fieldName))
                            {
                                isMatch = true;
                                preColText = newText.ToString();
                                val = " ";
                                List<string> nextStr = vals.Skip(i + 1).ToList();
                                lastColText = new StringBuilder(string.Join("", nextStr.ToArray()));
                            }
                        }

                        //判断是否匹配成功
                        if (!isMatch)
                        {
                            preColText = string.Empty;
                            val = string.Empty;
                            lastColText = new StringBuilder(newText.ToString());
                        }
                    }
                }
            }

            return preColText;
        }

        /// <summary>
        /// 检查是否满足数据要求
        /// </summary>
        /// <param name="val"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private bool CheckPointFieldValue(string val, string fieldName)
        {
            if (fieldName.Equals("PointNo"))
            {
                Regex reg = new Regex(@"\w+");
                return reg.IsMatch(val);
            }
            else if (fieldName.Equals("LatLong"))
            {
                Regex reg = new Regex(@"[SN]\d{6}(.\d+)?[EW]\d{7}(.\d+)?");
                return reg.IsMatch(val);
            }

            return false;
        }

        /// <summary>
        /// 设置航路点列对应字段
        /// </summary>
        /// <param name="col"></param>
        /// <param name="val"></param>
        /// <param name="dic"></param>
        private bool SetPointColName(int col, string val, ref Dictionary<int, string> dic)
        {
            bool ret = false;
            if (val.Contains("编号"))
            {
                if (!dic.Keys.Contains(col))
                    dic.Add(col, "PointNo");

                ret = true;
            }
            else if (val.Contains("经纬坐标"))
            {
                if (!dic.Keys.Contains(col))
                    dic.Add(col, "LatLong");
                ret = true;
            }
            else if (val.Contains("W/P ID"))
            {
                if (!dic.Keys.Contains(col))
                    dic.Add(col, "PointNo");
                ret = true;
            }
            else if (val.Contains("纬度"))
            {
                if (!dic.Keys.Contains(col))
                    dic.Add(col, "Lat");
                ret = true;
            }
            else if (val.Contains("经度"))
            {
                if (!dic.Keys.Contains(col))
                    dic.Add(col, "Long");
                ret = true;
            }

            return ret;
        }

        /// <summary>
        /// 属性设值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="m"></param>
        /// <param name="fieldName"></param>
        /// <param name="val"></param>
        private void SetModeFieldVal<T>(T m, string fieldName, object val)
        {
            var field = typeof(T).GetProperty(fieldName);
            if (null != field)
            {
                field.SetValue(m, val, null);
            }
        }

        /// <summary>
        /// 判断是否同行
        /// </summary>
        /// <param name="chunk1"></param>
        /// <param name="chunk2"></param>
        /// <returns></returns>
        private bool IsSameLine(TextChunk chunk1, TextChunk chunk2)
        {
            if (chunk1.Location.OrientationMagnitude == chunk2.Location.OrientationMagnitude &&
               chunk1.Location.DistPerpendicular == chunk2.Location.DistPerpendicular)
            {
                return true;
            }

            //判断两块高度差，是否超过第一块的行高，如未超过则认为是表格内换行

            return false;
        }

        /// <summary>
        /// 获取第一行文本
        /// </summary>
        /// <returns></returns>
        private string GetFirstLineText()
        {
            StringBuilder sb = new StringBuilder();
            TextChunk lastChunk = null;

            foreach (var chunk in Chunks)
            {
                if (null == lastChunk)
                {
                    lastChunk = chunk;
                    continue;
                }

                sb.Append(lastChunk.Text);
                if (!IsSameLine(chunk, lastChunk))
                {
                    break;
                }

                lastChunk = chunk;
            }

            return sb.ToString();
        }

        /// <summary>
        /// 判断该行是否可跳过
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private bool IsContiRow(string val)
        {
            //跳过日期所在的页脚行
            Regex reg = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
            return reg.IsMatch(val);
        }

        public string InitRoute()
        {
            TextChunk lastChunk = null;
            int row = 1;
            int col = 1;
            StringBuilder msg = new StringBuilder();
            StringBuilder preColText = new StringBuilder();
            Dictionary<int, string> dicColName = new Dictionary<int, string>();
            AirportRoute route = new AirportRoute();
            int isContiRow = 0;

            try
            {
                foreach (var chunk in Chunks)
                {
                    if (null == lastChunk)
                    {
                        lastChunk = chunk;
                        col = 1;
                    }

                    //判断是否换行
                    if (!chunk.SameLine(lastChunk))
                    {
                        row++;
                        col = 1;

                        //换行时，如果有上次读取的字符串为匹配，则改行数据匹配错误
                        if (!string.IsNullOrEmpty(preColText.ToString()))
                            msg.AppendLine(string.Format("第{0}行，解析出错，内容：{1}", row, preColText.ToString()));
                    }

                    if (row == isContiRow)
                        continue;

                    string val = chunk.Text;

                    //判断是否为需要跳过的行
                    if (IsContiRow(val))
                    {
                        isContiRow = row;
                        continue;
                    }

                    lastChunk = chunk;
                }
            }
            catch (Exception ex)
            {
                msg.AppendLine(string.Format("解析航路失败：{0}", ex.Message));
            }

            return msg.ToString();
        }
    }

    public class AirportPoint
    {
        public string PointNo { get; set; }

        public string LatLong { get; set; }

        public string Lat { get; set; }

        public string Long { get; set; }

        /// <summary>
        /// 转换经纬度
        /// </summary>
        /// <returns></returns>
        public string ConvertLatLong()
        {
            if (string.IsNullOrEmpty(Lat) || string.IsNullOrEmpty(Long))
            {
                return "经纬度信息不全";
            }

            Regex regLat = new Regex(@"\d{1,2}°\d{1,2}'\d{1,2}(.\d+)?""[NS]");
            Regex regLong = new Regex(@"\d{1,3}°\d{1,2}'\d{1,2}(.\d+)?""[EW]");
            if (regLat.IsMatch(Lat) && regLong.IsMatch(Long))
            {
                string newLat = string.Format("{0}{1}", Lat.Substring(Lat.Length - 1), Lat.Substring(0, Lat.Length - 1));
                newLat = newLat.Replace("°", string.Empty).Replace("'", string.Empty).Replace("\"", string.Empty);
                string newLong = string.Format("{0}{1}", Long.Substring(Long.Length - 1), Long.Substring(0, Long.Length - 1));
                newLong = newLong.Replace("°", string.Empty).Replace("'", string.Empty).Replace("\"", string.Empty);
                LatLong = newLat + newLong;
            }
            else
            {
                return "经纬度不满足格式";
            }
            return string.Empty;
        }
    }

    public class AirportRoute
    {
        public AirportRoute()
        {
            Route = new WayRoute();
            Points = new List<WayRoutePoint>();
        }

        /// <summary>
        /// 航路
        /// </summary>
        public WayRoute Route { get; set; }

        /// <summary>
        /// 航路点
        /// </summary>
        public List<WayRoutePoint> Points { get; set; }
    }
}