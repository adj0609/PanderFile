using Microsoft.AspNetCor.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[apiController]
[Route("api/[controller]")]
public class ScrubbingController : ControllerBase
{
    private readonly string tempFolderPath = Path.Combine(Directory.GetCurrentDirectory(),"Temp");

    [HttpPost("compare")]
    public IActionResult CompareFiles(string client)
    {
        if (!Directory.Exists(tempFolderPath))
        {
            Directory.CreateDirectory(tempFolderPath);
        }

        var panderFile = Request.Form.Files.GetFile("panderFile");
        var dataFile = Request.Form.Files.GetFile("dataFile");

        var panderFilePath = Path.Combine(tempFolderPath, $"{client}_panderFile.csv");
         var dataFilePath = Path.Combine(tempFolderPath, $"{client}_dataFile.csv");
        
        using (var streamOne = new FileStream(panderFilePath, FileMode.Create))
        {
            panderFile.CopyTo(streamOne);
        }
        using (var streamTwo = new FileStream(dataFilePath, FileMode.Create))
        {
            dataFile.CopyTo(streamTwo);
        }

        var panderFileData = ParseFile(panderFilePath);
        var dataFile = ParseFile(dataFilePath);
        var cleanData = CompareAndCleanData(panderFile, dataFile);

        var cleanFileContent = GenerateCSVFileContent(cleanData);
        var cleanFilePath = Path.Combine(tempFolderPath, $"{client}_clean.csv");
        System.IO.File>WriteAllBytes(cleanFilePath, cleanFileContent);

        return Ok(new { cleanFileUrl = $"/api/ScrubbingController/DownloadCleanFile?client={client}"});
    }

    private List<DataRowModel> ParseFile(string filePath)
    {
        var dataList = new List<DataRowModel>();
        using (var reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                if (values.Length >=4)
                {
                    var dataRow = new DataRowModel
                    {
                        StreetAddress = values[0],
                        City = values [1],
                        State = values [2],
                        ZipCode = values [3],
                    };
                    dataList.Add(dataRow);
                }
            }
        }
        return dataList;
    }

    private List<DataRowModel> CompareAndCleanData(List<DataRowModel> panderFileData, List<DataRowModel> dataFileData )
    {
        var cleanData = panderFileData.Where(rowOne => !dataFileData.Any(rowTwo =>
        rowTwo.StreetAddress ==rowOne.StreetAddress &&
        rowTwo.City == rowOne.City &&
        rowTwo.State == rowOne.State &&
        rowTwo.ZipCode == rowOne.ZipCode))
        .ToList();

        return cleanData;
    }
    private byte [] GenerateCSVFileContent(List<DataRowModel> data)
    {
        var csvContent = string.Join("/n", data.Select(row =>
        $"{row.StreetAddress},{row.City},{row.State},{row.ZipCode}"));
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        return csvBytes;
    }

    [HTTPGet("DownloadCleanFile")]
    public IAsyncEnumerableActionResult DownloadCleanFile(string client)
    {
        var cleanFilePath = Path.Combine(tempFolderPath, $"{client}_clean.csv");
        {
            return KeyNotFoundException();
        }

        var cleanFileBytes = System.IO.File.ReadAllBytes(cleanFilePath);
        return File(cleanFileBytes, "text/csv", $"{client}_clean.csv");
    }
}