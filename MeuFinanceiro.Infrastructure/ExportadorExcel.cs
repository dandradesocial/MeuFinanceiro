using ClosedXML.Excel;
using MeuFinanceiro.Core.Entities;
using MeuFinanceiro.Core.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MeuFinanceiro.Infrastructure.Export;

public class ExportadorExcel : IExportadorExcel
{
    private const int UltimaColuna = 6; // Data, Valor, Banco, Categoria, Tipo de Pagamento, Observação

    public async Task<byte[]> ExportarTransacoesAsync(IEnumerable<Transacao> transacoes)
    {
        var grupos = new List<TransacaoGrupo>
        {
            new TransacaoGrupo { Nome = "Lançamentos", Transacoes = transacoes }
        };
        return await ExportarTransacoesAgrupadasAsync(grupos);
    }

    public async Task<byte[]> ExportarTransacoesAgrupadasAsync(IEnumerable<TransacaoGrupo> grupos)
    {
        await Task.CompletedTask;
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Lançamentos");

        int row = 1;

        foreach (var grupo in grupos)
        {
            if (!grupo.Transacoes.Any())
                continue;

            var tituloRange = ws.Range(row, 1, row, UltimaColuna);
            tituloRange.Merge();
            tituloRange.Value = grupo.Nome;
            tituloRange.Style.Font.Bold = true;
            tituloRange.Style.Font.FontSize = 14;
            tituloRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row++;

            ws.Cell(row, 1).Value = "Data";
            ws.Cell(row, 2).Value = "Valor";
            ws.Cell(row, 3).Value = "Banco";
            ws.Cell(row, 4).Value = "Categoria";
            ws.Cell(row, 5).Value = "Tipo de Pagamento";
            ws.Cell(row, 6).Value = "Observação";
            var headerRange = ws.Range(row, 1, row, UltimaColuna);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            row++;

            int firstDataRow = row;
            foreach (var t in grupo.Transacoes.OrderBy(t => t.Data))
            {
                ws.Cell(row, 1).Value = t.Data.ToString("dd/MM/yyyy");
                ws.Cell(row, 2).Value = t.Valor;
                ws.Cell(row, 3).Value = t.Banco?.Nome ?? "";
                ws.Cell(row, 4).Value = t.Categoria?.Nome ?? "";
                ws.Cell(row, 5).Value = t.TipoPagamento?.ToString() ?? "";
                ws.Cell(row, 6).Value = t.Observacao ?? "";

                // Formatação condicional
                ws.Cell(row, 2).Style.NumberFormat.Format = "R$ #,##0.00";
                if (t.Tipo == TipoTransacao.Receita)
                    ws.Cell(row, 2).Style.Font.FontColor = XLColor.Green;
                else
                    ws.Cell(row, 2).Style.Font.FontColor = XLColor.Red;
                row++;
            }

            ws.Cell(row, 1).Value = "Total";
            ws.Cell(row, 2).FormulaA1 = $"SUM(B{firstDataRow}:B{row - 1})";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.NumberFormat.Format = "R$ #,##0.00";
            row++;

            var dataRange = ws.Range(firstDataRow, 1, row - 1, UltimaColuna);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            var valorRange = ws.Range(firstDataRow, 2, row - 1, 2);
            valorRange.Style.NumberFormat.Format = "R$ #,##0.00";

            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportarPlanilhaCompletaAsync(
        IEnumerable<TransacaoGrupo> gruposLancamentos,
        IEnumerable<ResumoBancoExportItem> resumoBancos)
    {
        await Task.CompletedTask;
        using var workbook = new XLWorkbook();

        // ========== ABA GERAL ==========
        var wsGeral = workbook.Worksheets.Add("Geral");
        int row = 1;

        foreach (var grupo in gruposLancamentos)
        {
            if (!grupo.Transacoes.Any())
                continue;

            var tituloRange = wsGeral.Range(row, 1, row, UltimaColuna);
            tituloRange.Merge();
            tituloRange.Value = grupo.Nome;
            tituloRange.Style.Font.Bold = true;
            tituloRange.Style.Font.FontSize = 14;
            tituloRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row++;

            wsGeral.Cell(row, 1).Value = "Data";
            wsGeral.Cell(row, 2).Value = "Valor";
            wsGeral.Cell(row, 3).Value = "Banco";
            wsGeral.Cell(row, 4).Value = "Categoria";
            wsGeral.Cell(row, 5).Value = "Tipo de Pagamento";
            wsGeral.Cell(row, 6).Value = "Observação";
            var headerRange = wsGeral.Range(row, 1, row, UltimaColuna);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            row++;

            int firstDataRow = row;
            foreach (var t in grupo.Transacoes.OrderBy(t => t.Data))
            {
                wsGeral.Cell(row, 1).Value = t.Data.ToString("dd/MM/yyyy");
                wsGeral.Cell(row, 2).Value = t.Valor;
                wsGeral.Cell(row, 3).Value = t.Banco?.Nome ?? "";
                wsGeral.Cell(row, 4).Value = t.Categoria?.Nome ?? "";
                wsGeral.Cell(row, 5).Value = t.TipoPagamento?.ToString() ?? "";
                wsGeral.Cell(row, 6).Value = t.Observacao ?? "";

                wsGeral.Cell(row, 2).Style.NumberFormat.Format = "R$ #,##0.00";
                if (t.Tipo == TipoTransacao.Receita)
                    wsGeral.Cell(row, 2).Style.Font.FontColor = XLColor.Green;
                else
                    wsGeral.Cell(row, 2).Style.Font.FontColor = XLColor.Red;
                row++;
            }

            wsGeral.Cell(row, 1).Value = "Total";
            wsGeral.Cell(row, 2).FormulaA1 = $"SUM(B{firstDataRow}:B{row - 1})";
            wsGeral.Cell(row, 1).Style.Font.Bold = true;
            wsGeral.Cell(row, 2).Style.Font.Bold = true;
            wsGeral.Cell(row, 2).Style.NumberFormat.Format = "R$ #,##0.00";
            row++;

            var dataRange = wsGeral.Range(firstDataRow, 1, row - 1, UltimaColuna);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            var valorRange = wsGeral.Range(firstDataRow, 2, row - 1, 2);
            valorRange.Style.NumberFormat.Format = "R$ #,##0.00";

            row++;
        }

        wsGeral.Columns().AdjustToContents();

        // ========== ABA BANCOS (lado a lado) ==========
        var wsBancos = workbook.Worksheets.Add("Bancos");

        const int colunasPorBanco = 3;      // Data, Valor, Categoria
        const int colunasEspacamento = 2;   // Duas colunas vazias entre blocos

        int colAtual = 1;

        foreach (var item in resumoBancos)
        {
            if (item.Banco == null)
                continue;

            int colInicial = colAtual;

            var bancoRange = wsBancos.Range(1, colInicial, 1, colInicial + colunasPorBanco - 1);
            bancoRange.Merge();
            bancoRange.Value = item.Banco.Nome;
            bancoRange.Style.Font.Bold = true;
            bancoRange.Style.Font.FontSize = 12;
            bancoRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            bancoRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            bancoRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            int linha = 2;

            if (item.PixDebitoTransacoes.Any())
            {
                wsBancos.Cell(linha, colInicial).Value = "Pix/Débito";
                wsBancos.Cell(linha, colInicial).Style.Font.Bold = true;
                linha++;

                wsBancos.Cell(linha, colInicial).Value = "Data";
                wsBancos.Cell(linha, colInicial + 1).Value = "Valor";
                wsBancos.Cell(linha, colInicial + 2).Value = "Categoria";
                var pixHeader = wsBancos.Range(linha, colInicial, linha, colInicial + colunasPorBanco - 1);
                pixHeader.Style.Font.Bold = true;
                pixHeader.Style.Fill.BackgroundColor = XLColor.LightGray;
                pixHeader.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                pixHeader.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                linha++;

                int firstPixRow = linha;
                foreach (var t in item.PixDebitoTransacoes)
                {
                    wsBancos.Cell(linha, colInicial).Value = t.Data.ToString("dd/MM/yyyy");
                    wsBancos.Cell(linha, colInicial + 1).Value = t.Valor;
                    wsBancos.Cell(linha, colInicial + 2).Value = t.Categoria?.Nome ?? "";
                    linha++;
                }

                wsBancos.Cell(linha, colInicial).Value = "Total";
                wsBancos.Cell(linha, colInicial).Style.Font.Bold = true;
                wsBancos.Cell(linha, colInicial + 1).FormulaA1 = $"SUM({GetColumnLetter(colInicial + 1)}{firstPixRow}:{GetColumnLetter(colInicial + 1)}{linha - 1})";
                wsBancos.Cell(linha, colInicial + 1).Style.NumberFormat.Format = "R$ #,##0.00";
                wsBancos.Cell(linha, colInicial + 1).Style.Font.Bold = true;
                linha++;

                var pixRange = wsBancos.Range(firstPixRow, colInicial, linha - 1, colInicial + colunasPorBanco - 1);
                pixRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                pixRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                var pixValorRange = wsBancos.Range(firstPixRow, colInicial + 1, linha - 1, colInicial + 1);
                pixValorRange.Style.NumberFormat.Format = "R$ #,##0.00";
            }

            if (item.CreditoTransacoes.Any())
            {
                wsBancos.Cell(linha, colInicial).Value = "Crédito";
                wsBancos.Cell(linha, colInicial).Style.Font.Bold = true;
                linha++;

                wsBancos.Cell(linha, colInicial).Value = "Data";
                wsBancos.Cell(linha, colInicial + 1).Value = "Valor";
                wsBancos.Cell(linha, colInicial + 2).Value = "Categoria";
                var credHeader = wsBancos.Range(linha, colInicial, linha, colInicial + colunasPorBanco - 1);
                credHeader.Style.Font.Bold = true;
                credHeader.Style.Fill.BackgroundColor = XLColor.LightGray;
                credHeader.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                credHeader.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                linha++;

                int firstCredRow = linha;
                foreach (var t in item.CreditoTransacoes)
                {
                    wsBancos.Cell(linha, colInicial).Value = t.Data.ToString("dd/MM/yyyy");
                    wsBancos.Cell(linha, colInicial + 1).Value = t.Valor;
                    wsBancos.Cell(linha, colInicial + 2).Value = t.Categoria?.Nome ?? "";
                    linha++;
                }

                wsBancos.Cell(linha, colInicial).Value = "Total";
                wsBancos.Cell(linha, colInicial).Style.Font.Bold = true;
                wsBancos.Cell(linha, colInicial + 1).FormulaA1 = $"SUM({GetColumnLetter(colInicial + 1)}{firstCredRow}:{GetColumnLetter(colInicial + 1)}{linha - 1})";
                wsBancos.Cell(linha, colInicial + 1).Style.NumberFormat.Format = "R$ #,##0.00";
                wsBancos.Cell(linha, colInicial + 1).Style.Font.Bold = true;
                linha++;

                var credRange = wsBancos.Range(firstCredRow, colInicial, linha - 1, colInicial + colunasPorBanco - 1);
                credRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                credRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                var credValorRange = wsBancos.Range(firstCredRow, colInicial + 1, linha - 1, colInicial + 1);
                credValorRange.Style.NumberFormat.Format = "R$ #,##0.00";
            }

            colAtual += colunasPorBanco + colunasEspacamento;
        }

        wsBancos.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static string GetColumnLetter(int columnNumber)
    {
        string columnLetter = string.Empty;
        while (columnNumber > 0)
        {
            int modulo = (columnNumber - 1) % 26;
            columnLetter = Convert.ToChar('A' + modulo) + columnLetter;
            columnNumber = (columnNumber - modulo) / 26;
        }
        return columnLetter;
    }
}