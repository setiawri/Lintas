using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace LintasMVC.Common
{
    public class ExportExcel
    {
        #region SupportCode ExportToExcel
        private static void SetWorkbookProperties(ExcelPackage p)
        {
            //Here setting some document properties
            p.Workbook.Properties.Author = "Harry Agung S";
            p.Workbook.Properties.Title = "Export to Excel";
        }

        private static ExcelWorksheet CreateSheet(ExcelPackage p, string sheetName)
        {
            p.Workbook.Worksheets.Add(sheetName);
            ExcelWorksheet ws = p.Workbook.Worksheets[1];
            ws.Name = sheetName; //Setting Sheet's name
            ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
            ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

            return ws;
        }
        #endregion

        public ExcelPackage ForwarderReports(List<Models.ShipmentsReportModels> list)
        {
            ExcelPackage p = new ExcelPackage();

            //set the workbook properties and add a default sheet in it
            SetWorkbookProperties(p);
            //Create a sheet
            ExcelWorksheet ws = CreateSheet(p, "Sheet1");

            //setting width column
            ws.Column(1).Width = 10;
            ws.Column(2).Width = 40;
            ws.Column(3).Width = 20;
            ws.Column(4).Width = 20;

            //header table
            ws.Cells[1, 1].Value = "NO";
            ws.Cells[1, 2].Value = "FORM";
            int col_index = 3;
            int package_no = 1;
            foreach (var item in list)
            {
                ws.Cells[1, col_index].Value = "PACKAGE #" + package_no;

                col_index++;
                package_no++;
            }
            ws.Cells[1, 1, 1, col_index - 1].Style.Font.Bold = true; ws.Cells[1, 1, 1, col_index - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var headerBorder = ws.Cells[1, 1, 1, col_index - 1].Style.Border;
            headerBorder.Left.Style = headerBorder.Top.Style = headerBorder.Right.Style = headerBorder.Bottom.Style = ExcelBorderStyle.Thin;
            var headerFill = ws.Cells[1, 1, 1, col_index - 1].Style.Fill;
            headerFill.PatternType = ExcelFillStyle.Solid;
            headerFill.BackgroundColor.SetColor(Color.LightGreen);

            //init forms
            ws.Cells[2, 1].Value = 1; ws.Cells[2, 2].Value = "Waybill Number";
            ws.Cells[3, 1].Value = 2; ws.Cells[3, 2].Value = "Service Number";
            ws.Cells[4, 1].Value = 3; ws.Cells[4, 2].Value = "Conversion Number";
            ws.Cells[5, 1].Value = 4; ws.Cells[5, 2].Value = "Origin Country";
            ws.Cells[6, 1].Value = 5; ws.Cells[6, 2].Value = "Parcel Weight";
            ws.Cells[7, 1].Value = 6; ws.Cells[7, 2].Value = "Parcel Long";
            ws.Cells[8, 1].Value = 7; ws.Cells[8, 2].Value = "Parcel Wide";
            ws.Cells[9, 1].Value = 8; ws.Cells[9, 2].Value = "Parcel High";
            ws.Cells[10, 1].Value = 9; ws.Cells[10, 2].Value = "Parcel Volume";
            ws.Cells[11, 1].Value = 10; ws.Cells[11, 2].Value = "Consignment Date";
            ws.Cells[12, 1].Value = 11; ws.Cells[12, 2].Value = "Tax Consignee Number";
            ws.Cells[13, 1].Value = 12; ws.Cells[13, 2].Value = "Consignee Name";
            ws.Cells[14, 1].Value = 13; ws.Cells[14, 2].Value = "Consignee Company";
            ws.Cells[15, 1].Value = 14; ws.Cells[15, 2].Value = "Consignee Phone";
            ws.Cells[16, 1].Value = 15; ws.Cells[16, 2].Value = "Consignee Mobile";
            ws.Cells[17, 1].Value = 16; ws.Cells[17, 2].Value = "Consignee Fax";
            ws.Cells[18, 1].Value = 17; ws.Cells[18, 2].Value = "Consignee Email";
            ws.Cells[19, 1].Value = 18; ws.Cells[19, 2].Value = "Consignee Postal Code";
            ws.Cells[20, 1].Value = 19; ws.Cells[20, 2].Value = "Consignee Country";
            ws.Cells[21, 1].Value = 20; ws.Cells[21, 2].Value = "Consignee Country Code";
            ws.Cells[22, 1].Value = 21; ws.Cells[22, 2].Value = "Consignee State";
            ws.Cells[23, 1].Value = 22; ws.Cells[23, 2].Value = "Consignee City";
            ws.Cells[24, 1].Value = 23; ws.Cells[24, 2].Value = "Consignee Address 1";
            ws.Cells[25, 1].Value = 24; ws.Cells[25, 2].Value = "Consignee Address 2";
            ws.Cells[26, 1].Value = 25; ws.Cells[26, 2].Value = "Shipper Name";
            ws.Cells[27, 1].Value = 26; ws.Cells[27, 2].Value = "Shipper Company";
            ws.Cells[28, 1].Value = 27; ws.Cells[28, 2].Value = "Shipper Phone";
            ws.Cells[29, 1].Value = 28; ws.Cells[29, 2].Value = "Shipper Mobile";
            ws.Cells[30, 1].Value = 29; ws.Cells[30, 2].Value = "Shipper Fax";
            ws.Cells[31, 1].Value = 30; ws.Cells[31, 2].Value = "Shipper Email";
            ws.Cells[32, 1].Value = 31; ws.Cells[32, 2].Value = "Shipper Postal Code";
            ws.Cells[33, 1].Value = 32; ws.Cells[33, 2].Value = "Shipper Country";
            ws.Cells[34, 1].Value = 33; ws.Cells[34, 2].Value = "Shipper Country Code";
            ws.Cells[35, 1].Value = 34; ws.Cells[35, 2].Value = "Shipper State";
            ws.Cells[36, 1].Value = 35; ws.Cells[36, 2].Value = "Shipper City";
            ws.Cells[37, 1].Value = 36; ws.Cells[37, 2].Value = "Shipper Address 1";
            ws.Cells[38, 1].Value = 37; ws.Cells[38, 2].Value = "Shipper Address 2";
            ws.Cells[39, 1].Value = 38; ws.Cells[39, 2].Value = "Parcel Quantity";
            ws.Cells[40, 1].Value = 39; ws.Cells[40, 2].Value = "Product Quantity";
            ws.Cells[41, 1].Value = 40; ws.Cells[41, 2].Value = "ProductDescription";
            ws.Cells[42, 1].Value = 41; ws.Cells[42, 2].Value = "Declaration Price";
            ws.Cells[43, 1].Value = 42; ws.Cells[43, 2].Value = "Currency";
            ws.Cells[44, 1].Value = 43; ws.Cells[44, 2].Value = "Billing Code";
            ws.Cells[45, 1].Value = 44; ws.Cells[45, 2].Value = "Billing Account";
            ws.Cells[46, 1].Value = 45; ws.Cells[46, 2].Value = "Broker Name";
            ws.Cells[47, 1].Value = 46; ws.Cells[47, 2].Value = "Broker Phone";
            ws.Cells[48, 1].Value = 47; ws.Cells[48, 2].Value = "HS Code";
            ws.Cells[49, 1].Value = 48; ws.Cells[49, 2].Value = "Freight Cost";
            ws.Cells[50, 1].Value = 49; ws.Cells[50, 2].Value = "Insurance";
            ws.Cells[51, 1].Value = 50; ws.Cells[51, 2].Value = "Bag No";
            ws.Cells[52, 1].Value = 51; ws.Cells[52, 2].Value = "Payment Type";

            int row_index = 2;
            int col_1st_package = 3;
            foreach (var item in list)
            {
                ws.Cells[row_index, col_1st_package].Value = item.WaybillNumber; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ServiceNumber; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConversionNumber; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.OriginCountry; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ParcelWeight; ws.Cells[row_index, col_1st_package].Style.Numberformat.Format = "#,##0.00"; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ParcelLong; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ParcelWide; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ParcelHigh; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ParcelVolume; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConsignmentDate; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.TaxConsigneeNumber; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConsigneeName; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConsigneeCompany; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConsigneePhone; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConsigneeMobile; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConsigneeFax; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConsigneeEmail; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConsigneePostalCode; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConsigneeCountry; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConsigneeCountryCode; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConsigneeState; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConsigneeCity; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConsigneeAddress1; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ConsigneeAddress2; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ShipperName; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ShipperCompany; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ShipperPhone; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ShipperMobile; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ShipperFax; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ShipperEmail; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ShipperPostalCode; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ShipperCountry; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ShipperCountryCode; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ShipperState; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ShipperCity; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ShipperAddress1; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ShipperAddress2; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ParcelQty; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ProductQty; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.ProductDescription; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.DeclarationPrice; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.Currency; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.BillingCode; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.BillingAccount; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.BrokerName; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.BrokerPhone; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.HsCode; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.FreightCost; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.Insurance; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.BagNo; row_index++;
                ws.Cells[row_index, col_1st_package].Value = item.PaymentType; row_index++;

                row_index = 2; col_1st_package++;
            }

            var cellBorder = ws.Cells[2, 1, 52, col_1st_package - 1].Style.Border;
            cellBorder.Left.Style = cellBorder.Top.Style = cellBorder.Right.Style = cellBorder.Bottom.Style = ExcelBorderStyle.Thin;

            return p;
        }
    }
}