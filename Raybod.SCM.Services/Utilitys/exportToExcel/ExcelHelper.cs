using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.Document;
using Raybod.SCM.DataTransferObject.FinancialAccount;
using Raybod.SCM.DataTransferObject.Warehouse;
using Raybod.SCM.Domain.Enum;
using Raybod.SCM.Domain.Model;
using Raybod.SCM.Domain.View;
using Raybod.SCM.Utility.Extention;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Raybod.SCM.Services.Utilitys.exportToExcel
{
    public static class ExcelHelper
    {
        private static List<string> alphabetic = new List<string>
        {
            "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z",
            "AA","AB","AC","AD","AE","AF","AG","AH","AI","AJ","AK","AL","AM","AN","AO","AP","AQ","AR","AS","AT","AU","AV","AW","AX","AY","AZ",
            "BA","BB","BC","BD","BE","BF","BG","BH","BI","BJ","BK","BL","BM","BN","BO","BP","BQ","BR","BS","BT","BU","BV","BW","BX","BY","BZ",
            "CA","CB","CC","CD","CE","CF","CG","CH","CI","CJ","CK","CL","CM","CN","CO","CP","CQ","CR","CS","CT","CU","CV","CW","CX","CY","CZ",
            "DA","DB","DC","DD","DE","DF","DG","DH","DI","DJ","DK","DL","DM","DN","DO","DP","DQ","DR","DS","DT","DU","DV","DW","DX","DY","DZ",
            "EA","EB","EC","ED","EE","EF","EG","EH","EI","EJ","EK","EL","EM","EN","EO","EP","EQ","ER","ES","ET","EU","EV","EW","EX","EY","EZ",
            "FA","FB","FC","FD","FE","FF","FG","FH","FI","FJ","FK","FL","FM","FN","FO","FP","FQ","FR","FS","FT","FU","FV","FW","FX","FY","FZ",
            "GA","GB","GC","GD","GE","GF","GG","GH","GI","GJ","GK","GL","GM","GN","GO","GP","GQ","GR","GS","GT","GU","GV","GW","GX","GY","GZ",
            "HA","HB","HC","HD","HE","HF","HG","HH","HI","HJ","HK","HL","HM","HN","HO","HP","HQ","HR","HS","HT","HU","HV","HW","HX","HY","HZ",
            "IA","IB","IC","ID","IE","IF","IG","IH","II","IJ","IK","IL","IM","IN","IO","IP","IQ","IR","IS","IT","IU","IV","IW","IX","IY","IZ",
            "JA","JB","JC","JD","JE","JF","JG","JH","JI","JJ","JK","JL","JM","JN","JO","JP","JQ","JR","JS","JT","JU","JV","JW","JX","JY","JZ",
            "KA","KB","KC","KD","KE","KF","KG","KH","KI","KJ","KK","KL","KM","KN","KO","KP","KQ","KR","KS","KT","KU","KV","KW","KX","KY","KZ",
            "LA","LB","LC","LD","LE","LF","LG","LH","LI","LJ","LK","LL","LM","LN","LO","LP","LQ","LR","LS","LT","LU","LV","LW","LX","LY","LZ",
            "MA","MB","MC","MD","ME","MF","MG","MH","MI","MJ","MK","ML","MM","MN","MO","MP","MQ","MR","MS","MT","MU","MV","MW","MX","MY","MZ",
            "NA","NB","NC","ND","NE","NF","NG","NH","NI","NJ","NK","NL","NM","NN","NO","NP","NQ","NR","NS","NT","NU","NV","NW","NX","NY","NZ",
            "OA","OB","OC","OD","OE","OF","OG","OH","OI","OJ","OK","OL","OM","ON","OO","OP","OQ","OR","OS","OT","OU","OV","OW","OX","OY","OZ",
            "PA","PB","PC","PD","PE","PF","PG","PH","PI","PJ","PK","PL","PM","PN","PO","PP","PQ","PR","PS","PT","PU","PV","PW","PX","PY","PZ",
            "QA","QB","QC","QD","QE","QF","QG","QH","QI","QJ","QK","QL","QM","QN","QO","QP","QQ","QR","QS","QT","QU","QV","QW","QX","QY","QZ",
            "RA","RB","RC","RD","RE","RF","RG","RH","RI","RJ","RK","RL","RM","RN","RO","RP","RQ","RR","RS","RT","RU","RV","RW","RX","RY","RZ",
            "SA","SB","SC","SD","SE","SF","SG","SH","SI","SJ","SK","SL","SM","SN","SO","SP","SQ","SR","SS","ST","SU","SV","SW","SX","SY","SZ",
            "TA","TB","TC","TD","TE","TF","TG","TH","TI","TJ","TK","TL","TM","TN","TO","TP","TQ","TR","TS","TT","TU","TV","TW","TX","TY","TZ",
            "UA","UB","UC","UD","UE","UF","UG","UH","UI","UJ","UK","UL","UM","UN","UO","UP","UQ","UR","US","UT","UU","UV","UW","UX","UY","UZ",
            "VA","VB","VC","VD","VE","VF","VG","VH","VI","VJ","VK","VL","VM","VN","VO","VP","VQ","VR","VS","VT","VU","VV","VW","VX","VY","VZ",
            "WA","WB","WC","WD","WE","WF","WG","WH","WI","WJ","WK","WL","WM","WN","WO","WP","WQ","WR","WS","WT","WU","WV","WW","WX","WY","WZ",
            "XA","XB","XC","XD","XE","XF","XG","XH","XI","XJ","XK","XL","XM","XN","XO","XP","XQ","XR","XS","XT","XU","XV","XW","XX","XY","XZ",
            "YA","YB","YC","YD","YE","YF","YG","YH","YI","YJ","YK","YL","YM","YN","YO","YP","YQ","YR","YS","YT","YU","YV","YW","YX","YY","YZ",
            "ZA","ZB","ZC","ZD","ZE","ZF","ZG","ZH","ZI","ZJ","ZK","ZL","ZM","ZN","ZO","ZP","ZQ","ZR","ZS","ZT","ZU","ZV","ZW","ZX","ZY","ZZ",
        };
        private static List<string> counter = new List<string>
        {
            "First","Second","Third","Fourth","Fifth","Sixth","Seventh","Eighth","Ninth","Tenth","Eleventh","Twelveth","Thirteenth","Fourteenth",
            "Fifteenth","Sixteenth","Seventeenth","Eighteenth","Nineteenth","twentieth"
        };
        public static DownloadFileDto ExportToExcel<T>(IEnumerable<T> collectionList, string fileName, string sheetName)
        {

            var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add(sheetName);
                workSheet.Cells.LoadFromCollection(collectionList, true, OfficeOpenXml.Table.TableStyles.Medium1);
                package.Save();
            }

            stream.Position = 0;
            string excelName = $"{fileName}-{DateTime.Now.ToPersianDate()}.xlsx";

            return new DownloadFileDto
            {
                Stream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = excelName
            };
        }
        public static DownloadFileDto KardexExportToExcelWithStyle(List<WarehouseProductLogExcelDto> collectionList, string fileName, string sheetName)
        {
            var stream = new MemoryStream();
            using (ExcelPackage ExcelPkg = new ExcelPackage(stream))

            {
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add(sheetName);
                int index = 5;


                using (ExcelRange Rng = wsSheet1.Cells[$"B4:G{5 + collectionList.Count - 1}"])
                {
                    ExcelTableCollection tblcollection = wsSheet1.Tables;
                    ExcelTable table = tblcollection.Add(Rng, "tblSalesman");
                    //Set Columns position & name  
                    table.Columns[0].Name = "Date";
                    table.Columns[1].Name = "Operation";
                    table.Columns[2].Name = "Ref.Number";
                    table.Columns[3].Name = "Quantitn In";
                    table.Columns[4].Name = "Quantity Out";
                    table.Columns[5].Name = "Remained Quantity";

                    table.Columns[0].TotalsRowLabel = "Total";
                    table.Columns[3].TotalsRowFormula = "SUBTOTAL(109,[Quantitn In])";
                    table.Columns[4].TotalsRowFormula = "SUBTOTAL(109,[Quantity Out])";
                    table.Columns[5].TotalsRowFormula = "SUBTOTAL(109,[Quantitn In])-SUBTOTAL(109,[Quantity Out])";
                    table.ShowTotal = true;
                    table.ShowFilter = false;
                    table.TableStyle = TableStyles.Dark9;
                    foreach (var item in collectionList)
                    {

                        wsSheet1.Cells["B" + index.ToString()].Value = item.Date;
                        wsSheet1.Cells["C" + index.ToString()].Value = item.Operation;
                        wsSheet1.Cells["D" + index.ToString()].Value = item.Reference;
                        wsSheet1.Cells["E" + index.ToString()].Value = item.QuantityIn;
                        wsSheet1.Cells["F" + index.ToString()].Value = item.QuantityOut;
                        wsSheet1.Cells["G" + index.ToString()].Value = item.RemaindQuantity;
                        index++;
                    }
                }
                wsSheet1.Cells[wsSheet1.Dimension.Address].AutoFitColumns();
                ExcelPkg.Save();
            }
            stream.Position = 0;
            string excelName = $"{fileName}-{DateTime.Now.ToPersianDate()}.xlsx";

            return new DownloadFileDto
            {
                Stream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = excelName
            };
        }

        public static DownloadFileDto ExportDocumentsToExcel(List<ExportExcelDocumentDto> collectionList, string fileName, string sheetName)
        {
            var stream = new MemoryStream();
            using (ExcelPackage ExcelPkg = new ExcelPackage(stream))

            {
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add(sheetName);
                int index = 2;


                using (ExcelRange Rng = wsSheet1.Cells[$"A1:K{ collectionList.Count +2}"])
                {
                    ExcelTableCollection tblcollection = wsSheet1.Tables;
                    ExcelTable table = tblcollection.Add(Rng, "tblSalesman");
                    //Set Columns position & name  
                    table.Columns[0].Name = "#";
                    table.Columns[1].Name = "Document code";
                    table.Columns[2].Name = "Client code";
                    table.Columns[3].Name = "Title";
                    table.Columns[4].Name = "Group";
                    table.Columns[5].Name = "Class";
                    table.Columns[6].Name = "Area";
                    table.Columns[7].Name = "Last Rev.";
                    table.Columns[8].Name = "Status";
                    table.Columns[9].Name = "Comment";
                    table.Columns[10].Name = "Remark";
                    table.ShowFilter = false;
                    table.TableStyle = TableStyles.None;
                    foreach (var item in collectionList)
                    {

                        wsSheet1.Cells["A" + index.ToString()].Value = index-1;
                        wsSheet1.Cells["A" + index.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        wsSheet1.Cells["B" + index.ToString()].Value = item.DocNumber;
                        wsSheet1.Cells["C" + index.ToString()].Value = item.ClientDocNumber;
                        wsSheet1.Cells["D" + index.ToString()].Value = item.DocTitle;
                        wsSheet1.Cells["E" + index.ToString()].Value = item.DocumentGroupTitle;
                        wsSheet1.Cells["F" + index.ToString()].Value = (item.DocClass == DocumentClass.FA) ? "FA" : "FI";
                        wsSheet1.Cells["G" + index.ToString()].Value = item.AreaTitle;
                        wsSheet1.Cells["H" + index.ToString()].Value = item.LastRevisionCode;
                        wsSheet1.Cells["I" + index.ToString()].Value = MapRevisionStatus(item.LastRevisionStatus);
                        wsSheet1.Cells["J" + index.ToString()].Value = MapCommentStatus(item.CommentStatus);
                        wsSheet1.Cells["K" + index.ToString()].Value = item.Remark;
                        index++;
                    }
                }
                wsSheet1.Cells[wsSheet1.Dimension.Address].AutoFitColumns();
                ExcelPkg.Save();
            }
            stream.Position = 0;
            string excelName = $"{fileName}.xlsx";

            return new DownloadFileDto
            {
                Stream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = excelName
            };
        }
        public static DownloadFileDto InventoryExportToExcelWithStyle(List<ExportExcelWarehouseProductDto> collectionList, string fileName, string sheetName)
        {
            var stream = new MemoryStream();
            using (ExcelPackage ExcelPkg = new ExcelPackage(stream))

            {
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add(sheetName);
                int index = 5;


                using (ExcelRange Rng = wsSheet1.Cells[$"B4:H{5 + collectionList.Count - 1}"])
                {
                    ExcelTableCollection tblcollection = wsSheet1.Tables;
                    ExcelTable table = tblcollection.Add(Rng, "tblSalesman");
                    //Set Columns position & name  
                    table.Columns[0].Name = "Equipment Name";
                    table.Columns[1].Name = "Equipment Code";
                    table.Columns[2].Name = "Technical Number";
                    table.Columns[3].Name = "Group";
                    table.Columns[4].Name = "Unit";
                    table.Columns[5].Name = "Inventory";
                    table.Columns[6].Name = "Last Updated";
                    table.ShowTotal = false;
                    table.ShowFilter = false;
                    table.TableStyle = TableStyles.Dark9;
                    foreach (var item in collectionList)
                    {

                        wsSheet1.Cells["B" + index.ToString()].Value = item.EquipmentName;
                        wsSheet1.Cells["C" + index.ToString()].Value = item.EquipmentCode;
                        wsSheet1.Cells["D" + index.ToString()].Value = item.TechnicalNumber;
                        wsSheet1.Cells["E" + index.ToString()].Value = item.Group;
                        wsSheet1.Cells["F" + index.ToString()].Value = item.Unit;
                        wsSheet1.Cells["F" + index.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        wsSheet1.Cells["G" + index.ToString()].Value = item.Inventory;
                        wsSheet1.Cells["G" + index.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        wsSheet1.Cells["H" + index.ToString()].Value = item.LastUpdated;
                        wsSheet1.Cells["G" + index.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        index++;
                    }
                }
                wsSheet1.Cells[wsSheet1.Dimension.Address].AutoFitColumns();
                ExcelPkg.Save();
            }
            stream.Position = 0;
            string excelName = $"{fileName}-{DateTime.Now.ToPersianDate()}.xlsx";

            return new DownloadFileDto
            {
                Stream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = excelName
            };
        }
        public static DownloadFileDto SupplierFinancialExportToExcelWithStyle(List<FinancialAccountOfSupplierDto> collectionList, string fileName, string sheetName, string currencyType, CurrencyType currency)
        {
            var stream = new MemoryStream();
            using (ExcelPackage ExcelPkg = new ExcelPackage(stream))

            {
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add(sheetName);
                int index = 5;

                using (ExcelRange Rng = wsSheet1.Cells["B2:F2"])
                {
                    Rng.Value = $"صورت حساب {currencyType} شرکت {sheetName}";
                    Rng.Merge = true;
                    Rng.Style.Font.Size = 13;
                    Rng.Style.Font.Bold = true;
                    Rng.Style.Font.Italic = true;
                }
                using (ExcelRange Rng = wsSheet1.Cells[$"B4:G{5 + collectionList.Count - 1}"])
                {
                    ExcelTableCollection tblcollection = wsSheet1.Tables;
                    ExcelTable table = tblcollection.Add(Rng, "tblSalesman");
                    //Set Columns position & name  
                    table.Columns[0].Name = "Date";
                    table.Columns[1].Name = "Currency";
                    table.Columns[2].Name = "Invoice";
                    table.Columns[3].Name = "Payment";
                    table.Columns[4].Name = "Purchase Rejected";
                    table.Columns[5].Name = "Remained";



                    table.Columns[0].TotalsRowLabel = "Total";
                    table.Columns[2].TotalsRowFormula = "SUBTOTAL(109,[Invoice])";
                    table.Columns[3].TotalsRowFormula = "SUBTOTAL(109,[Payment])";
                    table.Columns[4].TotalsRowFormula = "SUBTOTAL(109,[Purchase Rejected])";
                    table.Columns[5].TotalsRowFormula = "SUBTOTAL(109,[Invoice])-SUBTOTAL(109,[Payment])-SUBTOTAL(109,[Purchase Rejected])";
                    table.ShowTotal = true;
                    table.ShowFilter = false;
                    table.TableStyle = TableStyles.Dark9;
                    foreach (var item in collectionList)
                    {
                        wsSheet1.Cells["B" + index.ToString()].Value = item.DateDone.UnixTimestampToDateTime().ToPersianDate();
                        wsSheet1.Cells["C" + index.ToString()].Value = currency.GetDisplayName();
                        wsSheet1.Cells["D" + index.ToString()].Value = item.PurchaseAmount;
                        wsSheet1.Cells["E" + index.ToString()].Value = item.PaymentAmount;
                        wsSheet1.Cells["F" + index.ToString()].Value = item.PurchaseRejectAmount;
                        wsSheet1.Cells["G" + index.ToString()].Value = item.RemainedAmount;


                        index++;
                    }
                }
                wsSheet1.Cells[wsSheet1.Dimension.Address].AutoFitColumns();
                ExcelPkg.Save();
            }
            stream.Position = 0;
            string excelName = $"{fileName}-{DateTime.Now.ToPersianDate()}.xlsx";

            return new DownloadFileDto
            {
                Stream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = excelName
            };
        }
        public static DownloadFileDto FinancialAccountExportToExcelWithStyle(List<ExportExcelFinancialAccountDto> collectionList, string fileName, string sheetName)
        {
            var stream = new MemoryStream();
            using (ExcelPackage ExcelPkg = new ExcelPackage(stream))

            {
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add(sheetName);
                int index = 5;


                using (ExcelRange Rng = wsSheet1.Cells[$"B4:G{5 + collectionList.Count - 1}"])
                {
                    ExcelTableCollection tblcollection = wsSheet1.Tables;
                    ExcelTable table = tblcollection.Add(Rng, "tblSalesman");
                    //Set Columns position & name  
                    table.Columns[0].Name = "Supplier Name";
                    table.Columns[1].Name = "Currency";
                    table.Columns[2].Name = "Invoice";
                    table.Columns[3].Name = "Payment";
                    table.Columns[4].Name = "Purchase Rejected";
                    table.Columns[5].Name = "Remained";
                    table.ShowTotal = false;
                    table.ShowFilter = false;
                    table.TableStyle = TableStyles.Dark9;
                    foreach (var item in collectionList)
                    {

                        wsSheet1.Cells["B" + index.ToString()].Value = item.Name;
                        wsSheet1.Cells["C" + index.ToString()].Value = item.Currency.GetDisplayName();
                        wsSheet1.Cells["D" + index.ToString()].Value = item.PurchaseAmount;
                        wsSheet1.Cells["E" + index.ToString()].Value = item.PaymentAmount;
                        wsSheet1.Cells["F" + index.ToString()].Value = item.RejectPurchaseAmount;
                        wsSheet1.Cells["G" + index.ToString()].Value = item.RemainedAmount;
                        index++;
                    }
                }
                wsSheet1.Cells[wsSheet1.Dimension.Address].AutoFitColumns();
                ExcelPkg.Save();
            }
            stream.Position = 0;
            string excelName = $"{fileName}-{DateTime.Now.ToPersianDate()}.xlsx";

            return new DownloadFileDto
            {
                Stream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = excelName
            };
        }

        public static DownloadFileDto ExportDocumentsHistoryToExcel(List<Document> documents, string fileName, string sheetName)
        {
            var stream = new MemoryStream();
            using (ExcelPackage ExcelPkg = new ExcelPackage(stream))

            {
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add(sheetName);
                int index = 2;
                int j = 5;
                var revisionCount = documents.Select(a => a.DocumentRevisions.Where(b => !b.IsDeleted).Count()).ToList();
                var maxRevisionCount = revisionCount.Max();
                var revisions = documents.SelectMany(a => a.DocumentRevisions.Where(b => !b.IsDeleted)).ToList();
                var transmittalCount = revisions.Select(a => a.TransmittalRevisions.Count()).ToList();
                var maxTransmittalCount = transmittalCount.Max();
                var commentCount = revisions.Select(a => a.DocumentCommunications.Count()).ToList();
                var maxCommentCount = commentCount.Max();
                var rowsCount = (documents.Count) + 1;
                var columnCount = ((maxRevisionCount * 2) + (maxRevisionCount * ((maxTransmittalCount * 2) + (maxCommentCount * 3))));
                var revisionSectionCount = 2 + (maxTransmittalCount * 2) + (maxCommentCount * 3);
                int increamentalCount = 0;
                int transmittalCounter = 0;
                int commentCounter = 0;
                int revisionCounter = 0;
                using (ExcelRange Rng = wsSheet1.Cells[$"A1:{alphabetic[(j-1) + columnCount]}{rowsCount}"])
                {
                    ExcelTableCollection tblcollection = wsSheet1.Tables;

                    //Set Columns position & name  
                    wsSheet1.Cells["A1"].Value = "#";
                    wsSheet1.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    wsSheet1.Cells["A1"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["A1"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["A1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["A1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["B1"].Value = "Doc Number";
                    wsSheet1.Cells["B1"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["B1"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["B1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["B1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["C1"].Value = "Client Doc Number";
                    wsSheet1.Cells["C1"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["C1"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["C1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["C1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["D1"].Value = "Doc Title";
                    wsSheet1.Cells["D1"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["D1"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["D1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["D1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["E1"].Value = "Doc Class";
                    wsSheet1.Cells["E1"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["E1"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["E1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["E1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["F1"].Value = "Area";
                    wsSheet1.Cells["F1"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["F1"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["F1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Cells["F1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    for (int i = 0; i < maxRevisionCount; i++)
                    {

                        wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Value = $"{counter[i]} Revision Code";
                        wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        j += 1;

                        wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Value = $"{counter[i]} Revision Create Date";
                        wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        j += 1;
                        for (int k = 0; k < maxTransmittalCount; k++)
                        {
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Value = $"{counter[k]} Transmittal Number";
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            j += 1;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Value = $"{counter[k]} Transmittal Create Date";
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            j += 1;
                        }
                        for (int k = 0; k < maxCommentCount; k++)
                        {
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Value = $"{counter[k]} Comment Code";
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            j += 1;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Value = $"{counter[k]} Comment Create Date";
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            j += 1;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Value = $"{counter[k]} Comment Reply Date";
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[j + 1]}1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            j += 1;
                        }




                    }


                    foreach (var item in documents)
                    {
                        increamentalCount = 6;
                        revisionCounter = 0;
                        wsSheet1.Cells[$"A{index}"].Value = index-1;
                        wsSheet1.Cells[$"A{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"A{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"A{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"A{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"A{index}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        wsSheet1.Cells[$"B{index}"].Value = item.DocNumber;
                        wsSheet1.Cells[$"B{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"B{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"B{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"B{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"C{index}"].Value = item.ClientDocNumber;
                        wsSheet1.Cells[$"C{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"C{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"C{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"C{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"D{index}"].Value = item.DocTitle;
                        wsSheet1.Cells[$"D{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"D{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"D{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"D{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"E{index}"].Value = (item.DocClass == DocumentClass.FA) ? "FA" : "FI";
                        wsSheet1.Cells[$"E{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"E{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"E{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"E{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"F{index}"].Value = (item.Area != null) ? item.Area.AreaTitle : "";
                        wsSheet1.Cells[$"F{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"F{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"F{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[$"F{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;


                        foreach (var revision in item.DocumentRevisions.Where(a => !a.IsDeleted))
                        {
                            revisionCounter += 1;
                            transmittalCounter = 0;
                            commentCounter = 0;
                            wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = revision.DocumentRevisionCode;
                            wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 242, 216, 174);
                            wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            increamentalCount += 1;
                            wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = revision.CreatedDate.ToPersianDateString();
                            wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 242, 216, 174);
                            wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            increamentalCount += 1;
                            foreach (var transmittal in revision.TransmittalRevisions)
                            {

                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = transmittal.Transmittal.TransmittalNumber;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 237, 176, 76);
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                increamentalCount += 1;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = transmittal.Transmittal.CreatedDate.ToPersianDateString();
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 237, 176, 76);
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                increamentalCount += 1;
                                transmittalCounter += 1;
                            }
                            if (transmittalCounter < maxTransmittalCount)
                            {
                                for (int k = transmittalCounter; k < maxTransmittalCount; k++)
                                {
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = "";
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 237, 176, 76);
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    increamentalCount += 1;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = "";
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 237, 176, 76);
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    increamentalCount += 1;
                                }


                            }
                            foreach (var comment in revision.DocumentCommunications)
                            {

                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = comment.CommunicationCode;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 204, 128, 5);
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                increamentalCount += 1;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = comment.CreatedDate.ToPersianDateString();
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 204, 128, 5);
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                increamentalCount += 1;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = (comment.CommentStatus == CommunicationCommentStatus.Commented && comment.CommunicationStatus == DocumentCommunicationStatus.Replyed) ? comment.CommunicationQuestions.First().CommunicationReply.CreatedDate.ToPersianDateString() : "";
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 204, 128, 5);
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                increamentalCount += 1;
                                commentCounter += 1;
                            }
                            if (commentCounter < maxCommentCount)
                            {
                                for (int k = commentCounter; k < maxCommentCount; k++)
                                {
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = "";
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 204, 128, 5);
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    increamentalCount += 1;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = "";
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 204, 128, 5);
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    increamentalCount += 1;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = "";
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 204, 128, 5);
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    increamentalCount += 1;
                                }

                            }
                        }
                        if (revisionCounter < maxRevisionCount)
                        {
                            for (int z = revisionCounter; z < maxRevisionCount; z++)
                            {
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = "";
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.DarkGrid;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 242, 216, 174);
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                increamentalCount += 1;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = "";
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.DarkGrid;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 242, 216, 174);
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                increamentalCount += 1;
                                for (int k = 0; k < maxTransmittalCount; k++)
                                {
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = "";
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.DarkGrid;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 237, 176, 76);
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    increamentalCount += 1;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = "";
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.DarkGrid;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 237, 176, 76);
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    increamentalCount += 1;
                                }
                                for (int k = 0; k < maxCommentCount; k++)
                                {
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = "";
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.DarkGrid;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 204, 128, 5);
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    increamentalCount += 1;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = "";
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.DarkGrid;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 204, 128, 5);
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    increamentalCount += 1;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Value = "";
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.PatternType = ExcelFillStyle.DarkGrid;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Fill.BackgroundColor.SetColor(1, 204, 128, 5);
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    wsSheet1.Cells[$"{alphabetic[increamentalCount]}{index}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    increamentalCount += 1;
                                }
                            }

                        }
                        index += 1;
                    }
                }
                wsSheet1.Cells[wsSheet1.Dimension.Address].AutoFitColumns();
                wsSheet1.Cells[wsSheet1.Dimension.Address].AutoFilter = false;
                ExcelPkg.Save();
            }
            stream.Position = 0;
            string excelName = $"{fileName}.xlsx";

            return new DownloadFileDto
            {
                Stream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = excelName
            };
        }

        public static DownloadFileDto ExportDocumentsRevisionHistoryToExcel(List<DocumentRevision> revisions, string fileName, string sheetName)
        {
            var stream = new MemoryStream();
            using (ExcelPackage ExcelPkg = new ExcelPackage(stream))

            {
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add(sheetName);
                int index = 2;
                var transmittalCount = revisions.Select(a => a.TransmittalRevisions.Count()).ToList();
                var maxTransmittalCount = transmittalCount.Max();
                var commentCount = revisions.Select(a => a.DocumentCommunications.Count()).ToList();
                var maxCommentCount = commentCount.Max();
                var tqCount = revisions.Select(a => a.DocumentTQNCRs.Where(a=>a.CommunicationType==CommunicationType.TQ).Count()).ToList();
                var maxTQCount = commentCount.Max();
                var ncrCount = revisions.Select(a => a.DocumentTQNCRs.Where(a => a.CommunicationType == CommunicationType.NCR).Count()).ToList();
                var maxNCRCount = commentCount.Max();
                using (ExcelRange Rng = wsSheet1.Cells[$"A1:K{(revisions.Count*(2+maxCommentCount+maxCommentCount+maxNCRCount+maxTQCount))}"])
                {
                    ExcelTableCollection tblcollection = wsSheet1.Tables;
                    ExcelTable table = tblcollection.Add(Rng, "tblSalesman");
                    //Set Columns position & name  
                    table.Columns[0].Name = "#";
                    table.Columns[1].Name = "Doc Number";
                    table.Columns[2].Name = "Client Doc Number";
                    table.Columns[3].Name = "Doc Title";
                    table.Columns[4].Name = "Doc Class";
                    table.Columns[5].Name = "Rev Code";
                    table.Columns[6].Name = "Action";
                    table.Columns[7].Name = "Action Code";
                    table.Columns[8].Name = "Date";
                    table.Columns[9].Name = "Time";
                    table.Columns[10].Name = "User";
                    table.ShowFilter = false;
                    table.TableStyle = TableStyles.None;
                    

                    foreach (var item in revisions)
                    {

                        wsSheet1.Cells[$"A{index}"].Value = index-1;
                        wsSheet1.Cells[$"A{index}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        wsSheet1.Cells[$"B{index}"].Value = item.Document.DocNumber;
                        wsSheet1.Cells[$"C{index}"].Value = item.Document.ClientDocNumber;
                        wsSheet1.Cells[$"D{index}"].Value = item.Document.DocTitle;
                        wsSheet1.Cells[$"E{index}"].Value = (item.Document.DocClass == DocumentClass.FA) ? "FA" : "FI";
                        wsSheet1.Cells[$"F{index}"].Value = item.DocumentRevisionCode;
                        wsSheet1.Cells[$"G{index}"].Value = "Create Revision";
                        wsSheet1.Cells[$"H{index}"].Value = "";
                        wsSheet1.Cells[$"I{index}"].Value = item.CreatedDate.ToPersianDateString();
                        wsSheet1.Cells[$"J{index}"].Value = item.CreatedDate.Value.ToLocalTime().ToLongTimeString();
                        wsSheet1.Cells[$"K{index}"].Value = item.AdderUser.FullName;
                        index += 1;
                        if(item.RevisionStatus>=RevisionStatus.Confirmed)
                        {
                            wsSheet1.Cells[$"A{index}"].Value = index - 1;
                            wsSheet1.Cells[$"A{index}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            wsSheet1.Cells[$"B{index}"].Value = item.Document.DocNumber;
                            wsSheet1.Cells[$"C{index}"].Value = item.Document.ClientDocNumber;
                            wsSheet1.Cells[$"D{index}"].Value = item.Document.DocTitle;
                            wsSheet1.Cells[$"E{index}"].Value = (item.Document.DocClass == DocumentClass.FA) ? "FA" : "FI";
                            wsSheet1.Cells[$"F{index}"].Value = item.DocumentRevisionCode;
                            wsSheet1.Cells[$"G{index}"].Value = "Finalize Revision";
                            wsSheet1.Cells[$"H{index}"].Value = "";
                            wsSheet1.Cells[$"I{index}"].Value = item.ConfirmationWorkFlows.First(a=>a.Status==ConfirmationWorkFlowStatus.Confirm).UpdateDate.ToPersianDateString();
                            wsSheet1.Cells[$"J{index}"].Value = item.ConfirmationWorkFlows.First(a=>a.Status==ConfirmationWorkFlowStatus.Confirm).UpdateDate.Value.ToLocalTime().ToLongTimeString(); ;
                            wsSheet1.Cells[$"K{index}"].Value = item.ConfirmationWorkFlows.First(a => a.Status == ConfirmationWorkFlowStatus.Confirm).ModifierUser.FullName;
                            index += 1;
                        }
                        foreach(var trans in item.TransmittalRevisions.OrderBy(a=>a.Transmittal.CreatedDate))
                        {
                            wsSheet1.Cells[$"A{index}"].Value = index - 1;
                            wsSheet1.Cells[$"A{index}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            wsSheet1.Cells[$"B{index}"].Value = item.Document.DocNumber;
                            wsSheet1.Cells[$"C{index}"].Value = item.Document.ClientDocNumber;
                            wsSheet1.Cells[$"D{index}"].Value = item.Document.DocTitle;
                            wsSheet1.Cells[$"E{index}"].Value = (item.Document.DocClass == DocumentClass.FA) ? "FA" : "FI";
                            wsSheet1.Cells[$"F{index}"].Value = item.DocumentRevisionCode;
                            wsSheet1.Cells[$"G{index}"].Value = "Transmittal";
                            wsSheet1.Cells[$"H{index}"].Value = trans.Transmittal.TransmittalNumber;
                            wsSheet1.Cells[$"I{index}"].Value = trans.Transmittal.CreatedDate.ToPersianDateString();
                            wsSheet1.Cells[$"J{index}"].Value = trans.Transmittal.CreatedDate.Value.ToLocalTime().ToLongTimeString();
                            wsSheet1.Cells[$"K{index}"].Value = trans.Transmittal.AdderUser.FullName;
                            index += 1;
                        }
                        foreach (var comment in item.DocumentCommunications.OrderBy(a => a.CreatedDate))
                        {
                            wsSheet1.Cells[$"A{index}"].Value = index - 1;
                            wsSheet1.Cells[$"A{index}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            wsSheet1.Cells[$"B{index}"].Value = item.Document.DocNumber;
                            wsSheet1.Cells[$"C{index}"].Value = item.Document.ClientDocNumber;
                            wsSheet1.Cells[$"D{index}"].Value = item.Document.DocTitle;
                            wsSheet1.Cells[$"E{index}"].Value = (item.Document.DocClass == DocumentClass.FA) ? "FA" : "FI";
                            wsSheet1.Cells[$"F{index}"].Value = item.DocumentRevisionCode;
                            wsSheet1.Cells[$"G{index}"].Value = "Comment";
                            wsSheet1.Cells[$"H{index}"].Value = comment.CommunicationCode;
                            wsSheet1.Cells[$"I{index}"].Value = comment.CreatedDate.ToPersianDateString();
                            wsSheet1.Cells[$"J{index}"].Value = comment.CreatedDate.Value.ToLocalTime().ToLongTimeString();
                            wsSheet1.Cells[$"K{index}"].Value = comment.CommunicationQuestions.First().AdderUser.FullName;
                            index += 1;
                            if (comment.CommentStatus == CommunicationCommentStatus.Commented && comment.CommunicationStatus == DocumentCommunicationStatus.Replyed)
                            {
                                wsSheet1.Cells[$"A{index}"].Value = index - 1;
                                wsSheet1.Cells[$"A{index}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                wsSheet1.Cells[$"B{index}"].Value = item.Document.DocNumber;
                                wsSheet1.Cells[$"C{index}"].Value = item.Document.ClientDocNumber;
                                wsSheet1.Cells[$"D{index}"].Value = item.Document.DocTitle;
                                wsSheet1.Cells[$"E{index}"].Value = (item.Document.DocClass == DocumentClass.FA) ? "FA" : "FI";
                                wsSheet1.Cells[$"F{index}"].Value = item.DocumentRevisionCode;
                                wsSheet1.Cells[$"G{index}"].Value = "Comment Reply";
                                wsSheet1.Cells[$"H{index}"].Value = comment.CommunicationCode;
                                wsSheet1.Cells[$"I{index}"].Value = comment.CommunicationQuestions.First().CommunicationReply.CreatedDate.ToPersianDateString();
                                wsSheet1.Cells[$"J{index}"].Value = comment.CommunicationQuestions.First().CommunicationReply.CreatedDate.Value.ToLocalTime().ToLongTimeString();
                                wsSheet1.Cells[$"K{index}"].Value = comment.CommunicationQuestions.First().CommunicationReply.AdderUser.FullName;
                                index += 1;
                            }
                        }

                        foreach (var tq in item.DocumentTQNCRs.Where(a=>a.CommunicationType==CommunicationType.TQ).OrderBy(a => a.CreatedDate))
                        {
                            wsSheet1.Cells[$"A{index}"].Value = index - 1;
                            wsSheet1.Cells[$"A{index}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            wsSheet1.Cells[$"B{index}"].Value = item.Document.DocNumber;
                            wsSheet1.Cells[$"C{index}"].Value = item.Document.ClientDocNumber;
                            wsSheet1.Cells[$"D{index}"].Value = item.Document.DocTitle;
                            wsSheet1.Cells[$"E{index}"].Value = (item.Document.DocClass == DocumentClass.FA) ? "FA" : "FI";
                            wsSheet1.Cells[$"F{index}"].Value = item.DocumentRevisionCode;
                            wsSheet1.Cells[$"G{index}"].Value = "TQ";
                            wsSheet1.Cells[$"H{index}"].Value = tq.CommunicationCode;
                            wsSheet1.Cells[$"I{index}"].Value = tq.CreatedDate.ToPersianDateString();
                            wsSheet1.Cells[$"J{index}"].Value = tq.CreatedDate.Value.ToLocalTime().ToLongTimeString();
                            wsSheet1.Cells[$"K{index}"].Value = tq.AdderUser.FullName;
                            index += 1;
                            if (tq.CommunicationQuestions.First().CommunicationReply!=null)
                            {
                                wsSheet1.Cells[$"A{index}"].Value = index - 1;
                                wsSheet1.Cells[$"A{index}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                wsSheet1.Cells[$"B{index}"].Value = item.Document.DocNumber;
                                wsSheet1.Cells[$"C{index}"].Value = item.Document.ClientDocNumber;
                                wsSheet1.Cells[$"D{index}"].Value = item.Document.DocTitle;
                                wsSheet1.Cells[$"E{index}"].Value = (item.Document.DocClass == DocumentClass.FA) ? "FA" : "FI";
                                wsSheet1.Cells[$"F{index}"].Value = item.DocumentRevisionCode;
                                wsSheet1.Cells[$"G{index}"].Value = "TQ Reply";
                                wsSheet1.Cells[$"H{index}"].Value = tq.CommunicationCode;
                                wsSheet1.Cells[$"I{index}"].Value = tq.CommunicationQuestions.First().CommunicationReply.CreatedDate.ToPersianDateString();
                                wsSheet1.Cells[$"J{index}"].Value = tq.CommunicationQuestions.First().CommunicationReply.CreatedDate.Value.ToLocalTime().ToLongTimeString();
                                wsSheet1.Cells[$"K{index}"].Value = tq.CommunicationQuestions.First().CommunicationReply.AdderUser.FullName;
                                index += 1;
                            }
                        }
                        foreach (var ncr in item.DocumentCommunications.Where(a => a.CommunicationType == CommunicationType.NCR).OrderBy(a => a.CreatedDate))
                        {
                            wsSheet1.Cells[$"A{index}"].Value = index - 1;
                            wsSheet1.Cells[$"A{index}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            wsSheet1.Cells[$"B{index}"].Value = item.Document.DocNumber;
                            wsSheet1.Cells[$"C{index}"].Value = item.Document.ClientDocNumber;
                            wsSheet1.Cells[$"D{index}"].Value = item.Document.DocTitle;
                            wsSheet1.Cells[$"E{index}"].Value = (item.Document.DocClass == DocumentClass.FA) ? "FA" : "FI";
                            wsSheet1.Cells[$"F{index}"].Value = item.DocumentRevisionCode;
                            wsSheet1.Cells[$"G{index}"].Value = "NCR";
                            wsSheet1.Cells[$"H{index}"].Value = ncr.CommunicationCode;
                            wsSheet1.Cells[$"I{index}"].Value = ncr.CreatedDate.ToPersianDateString();
                            wsSheet1.Cells[$"J{index}"].Value = ncr.CreatedDate.Value.ToLocalTime().ToLongTimeString();
                            wsSheet1.Cells[$"K{index}"].Value = ncr.AdderUser.FullName;
                            index += 1;
                            if (ncr.CommunicationQuestions.First().CommunicationReply != null)
                            {
                                wsSheet1.Cells[$"A{index}"].Value = index - 1;
                                wsSheet1.Cells[$"A{index}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                wsSheet1.Cells[$"B{index}"].Value = item.Document.DocNumber;
                                wsSheet1.Cells[$"C{index}"].Value = item.Document.ClientDocNumber;
                                wsSheet1.Cells[$"D{index}"].Value = item.Document.DocTitle;
                                wsSheet1.Cells[$"E{index}"].Value = (item.Document.DocClass == DocumentClass.FA) ? "FA" : "FI";
                                wsSheet1.Cells[$"F{index}"].Value = item.DocumentRevisionCode;
                                wsSheet1.Cells[$"G{index}"].Value = "Comment Reply";
                                wsSheet1.Cells[$"H{index}"].Value = ncr.CommunicationCode;
                                wsSheet1.Cells[$"I{index}"].Value = ncr.CommunicationQuestions.First().CommunicationReply.CreatedDate.ToPersianDateString();
                                wsSheet1.Cells[$"J{index}"].Value = ncr.CommunicationQuestions.First().CommunicationReply.CreatedDate.Value.ToLocalTime().ToLongTimeString();
                                wsSheet1.Cells[$"K{index}"].Value = ncr.CommunicationQuestions.First().CommunicationReply.AdderUser.FullName;
                                index += 1;
                            }
                        }
                    }
                }
                wsSheet1.Cells[wsSheet1.Dimension.Address].AutoFitColumns();
                wsSheet1.Cells[wsSheet1.Dimension.Address].AutoFilter = false;
                ExcelPkg.Save();
            }
            stream.Position = 0;
            string excelName = $"{fileName}.xlsx";

            return new DownloadFileDto
            {
                Stream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = excelName
            };
        }
        private static string MapCommentStatus(CommunicationCommentStatus commentStatus)
        {
            string result = "";
            switch (commentStatus)
            {
                case CommunicationCommentStatus.NotHave:
                    {
                        result = "Not Have";
                        break;
                    }


                case CommunicationCommentStatus.Rejected:
                    {
                        result = "Rejected";
                        break;
                    }
                case CommunicationCommentStatus.ApproveAsNote:
                    {
                        result = "Approve As Noted";
                        break;
                    }
                case CommunicationCommentStatus.Approved:
                    {
                        result = "Approved";
                        break;
                    }
                case CommunicationCommentStatus.Commented:
                    {
                        result = "Commented";
                        break;
                    }

            }
            return result;
        }
        private static string MapRevisionStatus(RevisionStatus revisionStatus)
        {
            string result = "";
            switch (revisionStatus)
            {
                case RevisionStatus.DeActive:
                    {
                        result = "DeActive";
                        break;
                    }


                case RevisionStatus.Confirmed:
                    {
                        result = "Confirmed";
                        break;
                    }
                case RevisionStatus.InProgress:
                    {
                        result = "InProgress";
                        break;
                    }
                case RevisionStatus.PendingForModify:
                    {
                        result = "Pending For Modify";
                        break;
                    }
                case RevisionStatus.PendingConfirm:
                    {
                        result = "Pending For Confirm";
                        break;
                    }
                case RevisionStatus.TransmittalIFA:
                    {
                        result = "Transmittal-IFA";
                        break;
                    }
                case RevisionStatus.TransmittalIFI:
                    {
                        result = "Transmittal-IFI";
                        break;
                    }
                case RevisionStatus.TransmittalIFC:
                    {
                        result = "Transmittal-IFC";
                        break;
                    }
                case RevisionStatus.TransmittalASB:
                    {
                        result = "Transmittal-ASB";
                        break;
                    }
            }
            return result;
        }
    }
}
