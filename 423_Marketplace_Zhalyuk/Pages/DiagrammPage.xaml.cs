using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using MessageBox = System.Windows.MessageBox;

namespace _423_Marketplace_Zhalyuk.Pages
{
    public partial class DiagrammPage : Page
    {
        private Chart ChartProducts;

        public DiagrammPage()
        {
            InitializeComponent();

            ChartProducts = new Chart();
            ChartProducts.Dock = DockStyle.Fill;

            ChartArea chartArea = new ChartArea("Main");
            ChartProducts.ChartAreas.Add(chartArea);

            Legend legend = new Legend("Legend1");
            legend.Docking = Docking.Top;
            ChartProducts.Legends.Add(legend);

            ChartHost.Child = ChartProducts;

            using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
            {
                CmbUser.ItemsSource = db.User.ToList();
            }

            CmbDiagram.ItemsSource = Enum.GetValues(typeof(SeriesChartType));

            if (CmbUser.Items.Count > 0) CmbUser.SelectedIndex = 0;
            if (CmbDiagram.Items.Count > 0) CmbDiagram.SelectedIndex = 0;
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (!(CmbUser.SelectedItem is User currentUser)) return;
            if (!(CmbDiagram.SelectedItem is SeriesChartType currentType)) return;

            ChartProducts.Series.Clear();

            var series = new Series
            {
                Name = "Товары",
                ChartType = currentType,
                IsValueShownAsLabel = true,
                ChartArea = "Main",
                Legend = "Legend1"
            };

            using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
            {
                var products = db.Product
                    .Where(p => p.SellerID == currentUser.ID)
                    .ToList();

                var grouped = products
                    .GroupBy(p => p.Category != null ? p.Category.Name : "Без категории")
                    .Select(g => new
                    {
                        Category = g.Key,
                        Total = g.Sum(p => p.Price * p.Quantity)
                    })
                    .ToList();

                foreach (var item in grouped)
                {
                    series.Points.AddXY(item.Category, item.Total);
                }
            }

            ChartProducts.Series.Add(series);
        }

        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            if (!(CmbUser.SelectedItem is User currentUser))
            {
                MessageBox.Show("Сначала выберите продавца!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var application = new Word.Application();
                application.Visible = true;
                Word.Document document = application.Documents.Add();

                Word.Paragraph title = document.Paragraphs.Add();
                title.Range.Text = currentUser.FIO;
                title.Range.Font.Name = "Times New Roman";
                title.Range.Font.Size = 16;
                title.Range.Font.Bold = 1;
                title.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                title.Range.InsertParagraphAfter();

                using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
                {
                    var products = db.Product
                        .Where(p => p.SellerID == currentUser.ID)
                        .ToList();

                    var grouped = products
                        .GroupBy(p => p.Category != null ? p.Category.Name : "Без категории")
                        .Select(g => new { Category = g.Key, Total = g.Sum(p => p.Price * p.Quantity) })
                        .ToList();

                    Word.Table table = document.Tables.Add(
                        document.Paragraphs[document.Paragraphs.Count].Range,
                        grouped.Count + 1, 2);
                    table.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    table.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

                    table.Cell(1, 1).Range.Text = "Категория";
                    table.Cell(1, 2).Range.Text = "Сумма расходов";
                    table.Rows[1].Range.Font.Name = "Times New Roman";
                    table.Rows[1].Range.Font.Size = 14;
                    table.Rows[1].Range.Bold = 1;
                    table.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    for (int i = 0; i < grouped.Count; i++)
                    {
                        var item = grouped[i];
                        table.Cell(i + 2, 1).Range.Text = item.Category;
                        table.Cell(i + 2, 2).Range.Text = item.Total.ToString("N2") + " руб.";
                        table.Rows[i + 2].Range.Font.Name = "Times New Roman";
                        table.Rows[i + 2].Range.Font.Size = 12;
                    }

                    document.Paragraphs.Add();

                    if (products.Count > 0)
                    {
                        var maxProduct = products.OrderByDescending(p => p.Price * p.Quantity).First();
                        Word.Paragraph maxP = document.Paragraphs.Add();
                        Word.Range maxR = maxP.Range;
                        maxR.Text = $"Самый дорогостоящий товар — {maxProduct.Name} за {(maxProduct.Price * maxProduct.Quantity).ToString("N2")} руб.";
                        maxP.set_Style("Подзаголовок");
                        maxR.Font.Color = Word.WdColor.wdColorDarkRed;
                        maxR.InsertParagraphAfter();

                        var minProduct = products.OrderBy(p => p.Price * p.Quantity).First();
                        Word.Paragraph minP = document.Paragraphs.Add();
                        Word.Range minR = minP.Range;
                        minR.Text = $"Самый дешёвый товар — {minProduct.Name} за {(minProduct.Price * minProduct.Quantity).ToString("N2")} руб.";
                        minP.set_Style("Подзаголовок");
                        minR.Font.Color = Word.WdColor.wdColorDarkGreen;
                        minR.InsertParagraphAfter();
                    }
                }

                foreach (Word.Section section in document.Sections)
                {
                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.Range.Fields.Add(footer.Range, Word.WdFieldType.wdFieldPage);
                    footer.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                }

                document.Paragraphs.Add();
                MessageBox.Show("Документ Word успешно создан!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте в Word: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var application = new Excel.Application();
                application.Visible = true;

                using (var db = new DB_Marketplace_ZhalyukPolinaEntities())
                {
                    var allUsers = db.User.ToList().OrderBy(u => u.FIO).ToList();

                    application.SheetsInNewWorkbook = allUsers.Count;
                    Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);

                    decimal grandTotal = 0;

                    for (int i = 0; i < allUsers.Count; i++)
                    {
                        int startRowIndex = 1;
                        Excel.Worksheet worksheet = application.Worksheets.Item[i + 1];
                        worksheet.Name = allUsers[i].FIO.Length > 31
                            ? allUsers[i].FIO.Substring(0, 31)
                            : allUsers[i].FIO;

                        worksheet.Cells[1][startRowIndex] = "Дата";
                        worksheet.Cells[2][startRowIndex] = "Название";
                        worksheet.Cells[3][startRowIndex] = "Цена";
                        worksheet.Cells[4][startRowIndex] = "Количество";
                        worksheet.Cells[5][startRowIndex] = "Сумма";

                        Excel.Range columnHeaderRange = worksheet.Range[worksheet.Cells[1][1], worksheet.Cells[5][1]];
                        columnHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        columnHeaderRange.Font.Bold = true;
                        startRowIndex++;

                        var products = db.Product.Where(p => p.SellerID == allUsers[i].ID).ToList();

                        if (products.Count == 0)
                        {
                            worksheet.Cells[1][startRowIndex] = "Нет товаров";
                            continue;
                        }

                        var userCategories = products
                            .GroupBy(p => p.Category != null ? p.Category.Name : "Без категории")
                            .OrderBy(g => g.Key)
                            .ToList();

                        foreach (var groupCategory in userCategories)
                        {
                            Excel.Range headerRange = worksheet.Range[
                                worksheet.Cells[1][startRowIndex],
                                worksheet.Cells[5][startRowIndex]];
                            headerRange.Merge();
                            headerRange.Value = groupCategory.Key;
                            headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                            headerRange.Font.Italic = true;
                            startRowIndex++;

                            foreach (var product in groupCategory)
                            {
                                worksheet.Cells[1][startRowIndex] = product.CreatedAt.ToString("dd.MM.yyyy");
                                worksheet.Cells[2][startRowIndex] = product.Name;
                                worksheet.Cells[3][startRowIndex] = product.Price;
                                (worksheet.Cells[3][startRowIndex] as Excel.Range).NumberFormat = "0.00";
                                worksheet.Cells[4][startRowIndex] = product.Quantity;
                                worksheet.Cells[5][startRowIndex].Formula = $"=C{startRowIndex}*D{startRowIndex}";
                                (worksheet.Cells[5][startRowIndex] as Excel.Range).NumberFormat = "0.00";
                                startRowIndex++;
                            }

                            Excel.Range sumRange = worksheet.Range[
                                worksheet.Cells[1][startRowIndex],
                                worksheet.Cells[4][startRowIndex]];
                            sumRange.Merge();
                            sumRange.Value = "ИТОГО по категории:";
                            sumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                            worksheet.Cells[5][startRowIndex].Formula = $"=SUM(E{startRowIndex - groupCategory.Count()}:E{startRowIndex - 1})";
                            sumRange.Font.Bold = true;
                            (worksheet.Cells[5][startRowIndex] as Excel.Range).Font.Bold = true;

                            grandTotal += groupCategory.Sum(p => p.Price * p.Quantity);
                            startRowIndex++;
                        }

                        Excel.Range rangeBorders = worksheet.Range[
                            worksheet.Cells[1][1], worksheet.Cells[5][startRowIndex - 1]];
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle =
                        Excel.XlLineStyle.xlContinuous;

                        worksheet.Columns.AutoFit();
                    }

                    Excel.Worksheet summarySheet = workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
                    summarySheet.Name = "Общий итог";
                    summarySheet.Cells[1, 1] = "Общий итог по всем продавцам:";
                    summarySheet.Cells[1, 2] = grandTotal;

                    Excel.Range summaryRange = summarySheet.Range[summarySheet.Cells[1, 1], summarySheet.Cells[1, 2]];
                    summaryRange.Font.Color = Excel.XlRgbColor.rgbRed;
                    summaryRange.Font.Bold = true;

                    summarySheet.Columns.AutoFit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте в Excel: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}