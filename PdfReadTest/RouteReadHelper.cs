using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PdfReadTest
{
    public class RouteReadHelper
    {
        public List<Route> Routes = new List<Route>();
        public Dictionary<int, string> DicColName = new Dictionary<int, string>();

        public string ReadRouteInfo(string path)
        {
            ISheet sheet = null;
            IWorkbook workBook = null;
            FileStream fileStream = null;
            StringBuilder msg = new StringBuilder();
            Route route = new Route();

            try
            {
                if (!File.Exists(path))
                    return "文件不存在！";

                fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                if (path.IndexOf(".xlsx") > 0)
                    workBook = new XSSFWorkbook(fileStream);
                else if (path.IndexOf(".xls") > 0)
                    workBook = new HSSFWorkbook(fileStream);
                else
                    return "请选择正确格式的excel文件！";

                sheet = workBook.GetSheetAt(0);
                if (null == sheet)
                    return "获取excel信息失败！";

                //第一行为表头，获取列属性
                IRow firstRow = sheet.GetRow(0);
                InitColName(sheet, 0, firstRow);
                if (DicColName.Keys.Count == 0)
                    return "请检查第一列是否是表头！";

                int rowNum = sheet.LastRowNum;

                //遍历excel
                for (int rowIndex = 1; rowIndex <= rowNum; rowIndex++)
                {
                    IRow row = sheet.GetRow(rowIndex);
                    if (null == row)
                        continue;

                    if (IsContiRow(sheet, rowIndex, row))
                        continue;

                    RoutePoint point = new RoutePoint();
                    int colNum = row.LastCellNum;
                    bool isSetVal = false;
                    for (int colIndex = 0; colIndex < colNum; colIndex++)
                    {
                        ICell cell = row.GetCell(colIndex);

                        //只获取合并单元格的第一个单元格
                        if (cell.IsMergedCell && !IsMergedRegionFirst(sheet, rowIndex, colIndex))
                            continue;

                        string txt = cell.ToString() ?? "";
                        txt = txt.Replace("\n", "");
                        Console.WriteLine(string.Format("读取单元格（{0},{1}）：{2}", rowIndex, colIndex, txt));

                        if (txt.StartsWith("RWY"))
                        {
                            if (route.Points.Any())
                                Routes.Add(route);

                            //新航路
                            route = new Route();

                            int spaceIndex1 = txt.IndexOf(" ");
                            //跑道
                            if (spaceIndex1 > 0)
                                route.Designation = txt.Substring(0, spaceIndex1);

                            txt = txt.Substring(spaceIndex1).Trim();

                            //类型
                            int spaceIndex2 = txt.IndexOf(" ");
                            if (spaceIndex2 > 0)
                            {
                                route.FlyType = txt.Substring(0, spaceIndex2);
                                route.Code = txt.Substring(spaceIndex2 + 1).Trim();
                            }
                        }
                        else
                        {
                            if (DicColName.Keys.Contains(colIndex))
                            {
                                isSetVal = true;
                                string fieldName = DicColName[colIndex];
                                SetModeFieldVal(point, fieldName, txt);
                            }
                        }
                    }

                    if (isSetVal)
                        route.Points.Add(point);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                msg.Append(string.Format("解析失败：{0}", ex.Message));
            }

            return msg.ToString();
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
        /// 初始化列属性
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowIndex"></param>
        /// <param name="row"></param>
        private void InitColName(ISheet sheet, int rowIndex, IRow row)
        {
            int colNum = row.LastCellNum;

            for (int colIndex = 0; colIndex < colNum; colIndex++)
            {
                ICell cell = row.GetCell(colIndex);

                //只获取合并单元格的第一个单元格
                if (cell.IsMergedCell && !IsMergedRegionFirst(sheet, rowIndex, colIndex))
                    continue;

                string txt = cell.ToString() ?? "";
                txt = txt.Replace("\n", "");

                #region 设置列属性

                if (txt.Contains("航径描述"))
                {
                    if (!DicColName.Keys.Contains(colIndex))
                        DicColName.Add(colIndex, "TrackDescribe");
                }
                else if (txt.Contains("定位点标识"))
                {
                    if (!DicColName.Keys.Contains(colIndex))
                        DicColName.Add(colIndex, "Code");
                }
                else if (txt.Contains("是否飞越点"))
                {
                    if (!DicColName.Keys.Contains(colIndex))
                        DicColName.Add(colIndex, "Property");
                }
                else if (txt.Contains("磁航向"))
                {
                    if (!DicColName.Keys.Contains(colIndex))
                        DicColName.Add(colIndex, "Magnetiched");
                }
                else if (txt.Contains("转弯指示"))
                {
                    if (!DicColName.Keys.Contains(colIndex))
                        DicColName.Add(colIndex, "TurnIndicator");
                }
                else if (txt.Contains("高度"))
                {
                    if (!DicColName.Keys.Contains(colIndex))
                        DicColName.Add(colIndex, "Height");
                }
                else if (txt.Contains("速度限制"))
                {
                    if (!DicColName.Keys.Contains(colIndex))
                        DicColName.Add(colIndex, "Confinerte");
                }
                else if (txt.Contains("VPA") && txt.Contains("TCH"))
                {
                    if (!DicColName.Keys.Contains(colIndex))
                        DicColName.Add(colIndex, "Vpatch");
                }
                else if (txt.Contains("导航规范"))
                {
                    if (!DicColName.Keys.Contains(colIndex))
                        DicColName.Add(colIndex, "NavPerformance");
                }

                #endregion 设置列属性
            }
        }

        private bool IsContiRow(ISheet sheet, int rowIndex, IRow row)
        {
            //判断第一列内容，如果为表头则跳过该行
            ICell firstCell = row.GetCell(0);
            if (null == firstCell)
                return true;

            string firstCellVal = firstCell.ToString() ?? string.Empty;
            firstCellVal = firstCellVal.Replace("\n", "");
            if (firstCellVal.Contains("航径描述"))
                return true;

            //如果整行为空白则跳过
            int colNum = row.LastCellNum;
            for (int colIndex = 0; colIndex < colNum; colIndex++)
            {
                ICell cell = row.GetCell(colIndex);

                //只获取合并单元格的第一个单元格
                if (cell.IsMergedCell && !IsMergedRegionFirst(sheet, rowIndex, colIndex))
                    continue;

                string txt = cell.ToString() ?? "";
                if (!string.IsNullOrEmpty(txt.Trim()))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 获取单元格内容
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public string GetCellValue(ICell cell)
        {
            if (cell == null) return "";

            if (cell.CellType == CellType.String)
            {
                return cell.StringCellValue;
            }
            else if (cell.CellType == CellType.Boolean)
            {
                return cell.BooleanCellValue.ToString();
            }
            else if (cell.CellType == CellType.Formula)
            {
                return cell.CellFormula;
            }
            else if (cell.CellType == CellType.Numeric)
            {
                return cell.NumericCellValue.ToString();
            }

            return "";
        }

        /// <summary>
        /// 是否是合并单元格的第一个单元格
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool IsMergedRegionFirst(ISheet sheet, int row, int column)
        {
            int sheetMergeCount = sheet.NumMergedRegions;

            for (int i = 0; i < sheetMergeCount; i++)
            {
                CellRangeAddress ca = sheet.GetMergedRegion(i);
                int firstColumn = ca.FirstColumn;
                int lastColumn = ca.LastColumn;
                int firstRow = ca.FirstRow;
                int lastRow = ca.LastRow;

                //判断是否是第一个单元格
                if (row == firstRow && column == firstColumn)
                    return true;
            }

            return false;
        }
    }
}

public class Route
{
    public Route()
    {
        Points = new List<RoutePoint>();
    }

    /// <summary>
    /// 编号
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 进离场类型
    /// </summary>
    public string FlyType { get; set; }

    /// <summary>
    /// 跑道
    /// </summary>
    public string Designation { get; set; }

    /// <summary>
    /// 航路点
    /// </summary>
    public List<RoutePoint> Points { get; set; }
}

public class RoutePoint
{
    /// <summary>
    /// 航迹描述
    /// </summary>
    public string TrackDescribe { get; set; }

    /// <summary>
    /// 定位点标识
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 是否飞跃点
    /// </summary>
    public string Property { get; set; }

    /// <summary>
    /// 磁航向
    /// </summary>
    public string Magnetiched { get; set; }

    /// <summary>
    /// 转弯指示
    /// </summary>
    public string TurnIndicator { get; set; }

    /// <summary>
    /// 高度
    /// </summary>
    public string Height { get; set; }

    /// <summary>
    /// 速度限制
    /// </summary>
    public string Confinerte { get; set; }

    /// <summary>
    /// VPA/TCH
    /// </summary>
    public string Vpatch { get; set; }

    /// <summary>
    /// 导航规范
    /// </summary>
    public string NavPerformance { get; set; }
}