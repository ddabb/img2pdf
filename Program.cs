using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace Img2Pdf
{
    class Program
    {
        // 定义排序方式枚举
        public enum SortOrder
        {
            FileName,       // 按文件名排序
            ModifiedTime,   // 按修改时间排序
            CreationTime    // 按创建时间排序
        }

        static int Main(string[] args)
        {
            try
            {
                // 设置编码以支持中文
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                // 创建命令行参数解析器
                var parser = new CommandLineParser(args);

                // 解析命令行参数
                if (parser.HasFlag("--help") || parser.HasFlag("-h"))
                {
                    ShowHelp();
                    return 0;
                }

                // 如果没有提供任何参数，自动处理当前目录
                if (args.Length == 0)
                {
                    Console.WriteLine("未提供参数，将自动处理当前目录下的所有图片...");
                }

                // 获取输入路径
                List<string> inputPaths = new List<string>();
                if (parser.HasOption("-i") || parser.HasOption("--input"))
                {
                    var inputs = parser.GetOptionValues("-i") ?? parser.GetOptionValues("--input");
                    if (inputs != null && inputs.Count > 0)
                    {
                        inputPaths.AddRange(inputs);
                    }
                }
                else
                {
                    // 如果没有指定输入路径，则使用当前目录
                    inputPaths.Add(Directory.GetCurrentDirectory());
                }

                // 获取输出路径
                string outputPath = parser.GetOptionValue("-o") ?? parser.GetOptionValue("--output");
                if (string.IsNullOrEmpty(outputPath))
                {
                    // 如果没有指定输出路径，则使用当前目录下的output.pdf
                    outputPath = Path.Combine(Directory.GetCurrentDirectory(), "output.pdf");
                }

                // 获取是否保持原始尺寸
                bool keepSize = true;
                string keepSizeValue = parser.GetOptionValue("-k") ?? parser.GetOptionValue("--keep-size");
                if (!string.IsNullOrEmpty(keepSizeValue) && bool.TryParse(keepSizeValue, out bool result))
                {
                    keepSize = result;
                }

                // 获取文件类型过滤器
                List<string> fileTypes = new List<string>();
                if (parser.HasOption("-t") || parser.HasOption("--type"))
                {
                    var types = parser.GetOptionValues("-t") ?? parser.GetOptionValues("--type");
                    if (types != null && types.Count > 0)
                    {
                        foreach (var type in types)
                        {
                            // 确保类型格式正确（添加点前缀如果没有）
                            string normalizedType = type.StartsWith(".") ? type.ToLowerInvariant() : $".{type.ToLowerInvariant()}";
                            fileTypes.Add(normalizedType);
                        }
                    }
                }

                // 获取排序方式
                SortOrder sortOrder = SortOrder.FileName; // 默认按文件名排序
                string sortValue = parser.GetOptionValue("-s") ?? parser.GetOptionValue("--sort");
                if (!string.IsNullOrEmpty(sortValue))
                {
                    switch (sortValue.ToLowerInvariant())
                    {
                        case "name":
                            sortOrder = SortOrder.FileName;
                            break;
                        case "time":
                        case "modified":
                        case "mtime":
                            sortOrder = SortOrder.ModifiedTime;
                            break;
                        case "created":
                        case "ctime":
                            sortOrder = SortOrder.CreationTime;
                            break;
                    }
                }

                // 输入和输出路径已经在前面设置了默认值，不需要再验证是否为空

                // 执行转换
                ConvertImagesToPdf(inputPaths.ToArray(), outputPath, keepSize, fileTypes, sortOrder);
                Console.WriteLine($"PDF文件已成功创建: {outputPath}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"错误: {ex.Message}");
                return 1;
            }
        }

        /// <summary>
        /// 命令行参数解析器
        /// </summary>
        private class CommandLineParser
        {
            private readonly Dictionary<string, List<string>> _options = new Dictionary<string, List<string>>();
            private readonly HashSet<string> _flags = new HashSet<string>();

            public CommandLineParser(string[] args)
            {
                string currentOption = null;
                List<string> currentValues = null;

                foreach (var arg in args)
                {
                    if (arg.StartsWith("-"))
                    {
                        // 如果当前有选项，保存它
                        if (currentOption != null)
                        {
                            if (currentValues.Count == 0)
                            {
                                // 如果没有值，则视为标志
                                _flags.Add(currentOption);
                            }
                            else
                            {
                                _options[currentOption] = currentValues;
                            }
                        }

                        // 开始新的选项
                        currentOption = arg;
                        currentValues = new List<string>();
                    }
                    else if (currentOption != null)
                    {
                        // 添加值到当前选项
                        currentValues.Add(arg);
                    }
                }

                // 处理最后一个选项
                if (currentOption != null)
                {
                    if (currentValues.Count == 0)
                    {
                        _flags.Add(currentOption);
                    }
                    else
                    {
                        _options[currentOption] = currentValues;
                    }
                }
            }

            public bool HasFlag(string flag)
            {
                return _flags.Contains(flag);
            }

            public bool HasOption(string option)
            {
                return _options.ContainsKey(option);
            }

            public string GetOptionValue(string option)
            {
                if (_options.TryGetValue(option, out var values) && values.Count > 0)
                {
                    return values[0];
                }
                return null;
            }

            public List<string> GetOptionValues(string option)
            {
                if (_options.TryGetValue(option, out var values))
                {
                    return values;
                }
                return null;
            }
        }

        /// <summary>
        /// 显示帮助信息
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("用法: img2pdf [选项]");
            Console.WriteLine();
            Console.WriteLine("选项:");
            Console.WriteLine("  -i, --input <paths>      输入图片路径，可以是单个文件、多个文件或文件夹");
            Console.WriteLine("                          (默认: 当前目录)");
            Console.WriteLine("  -o, --output <path>      输出PDF文件路径");
            Console.WriteLine("                          (默认: 当前目录下的output.pdf)");
            Console.WriteLine("  -k, --keep-size <bool>   保持原始图片尺寸 (默认: true)");
            Console.WriteLine("  -t, --type <types>       指定要处理的图片类型，可多选 (例如: png jpg)");
            Console.WriteLine("  -s, --sort <order>       指定文件排序方式: name(默认), mtime(修改时间), ctime(创建时间)");
            Console.WriteLine("  -h, --help               显示帮助信息");
            Console.WriteLine();
            Console.WriteLine("示例:");
            Console.WriteLine("  img2pdf                                    # 处理当前目录下的所有图片并输出为output.pdf");
            Console.WriteLine("  img2pdf -i image1.png image2.jpg -o output.pdf");
            Console.WriteLine("  img2pdf -i C:\\Images\\ -o output.pdf");
            Console.WriteLine("  img2pdf -i C:\\Images\\ -o output.pdf -t png -s mtime");
            Console.WriteLine("  img2pdf -i C:\\Images\\ -o output.pdf -t png jpg -k false");
        }

        /// <summary>
        /// 将图片转换为PDF
        /// </summary>
        /// <param name="inputs">输入图片路径，可以是文件或文件夹</param>
        /// <param name="outputPath">输出PDF路径</param>
        /// <param name="keepOriginalSize">是否保持原始图片尺寸</param>
        /// <param name="fileTypes">要处理的文件类型列表，为空则处理所有支持的类型</param>
        /// <param name="sortOrder">文件排序方式</param>
        private static void ConvertImagesToPdf(string[] inputs, string outputPath, bool keepOriginalSize,
            List<string> fileTypes, SortOrder sortOrder)
        {
            // 收集所有图片文件
            var imageFiles = CollectImageFiles(inputs, fileTypes, sortOrder);

            if (imageFiles.Count == 0)
            {
                throw new Exception("未找到任何符合条件的图片文件");
            }

            // 确保输出目录存在
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // 创建PDF文档
            using (var document = new PdfDocument())
            {
                int processedCount = 0;
                int totalCount = imageFiles.Count;

                foreach (var imagePath in imageFiles)
                {
                    try
                    {
                        processedCount++;
                        Console.Write($"\r处理图片 {processedCount}/{totalCount}: {Path.GetFileName(imagePath)}");

                        // 使用跨平台的 ImageSharp 加载图片
                        using (var imageSharpImg = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(imagePath))
                        {
                            // 创建PDF页面
                            PdfPage page;

                            if (keepOriginalSize)
                            {
                                // 使用图片的原始尺寸
                                page = document.AddPage();
                                page.Width = imageSharpImg.Width;
                                page.Height = imageSharpImg.Height;
                            }
                            else
                            {
                                // 使用默认A4尺寸
                                page = document.AddPage();
                            }

                            // 在页面上绘制图片
                            using (XGraphics gfx = XGraphics.FromPdfPage(page))
                            {
                                // 仍然使用 XImage 从文件加载，因为 PdfSharp 需要它
                                using (XImage xImg = XImage.FromFile(imagePath))
                                {
                                    if (keepOriginalSize)
                                    {
                                        // 以原始尺寸绘制
                                        gfx.DrawImage(xImg, 0, 0, imageSharpImg.Width, imageSharpImg.Height);
                                    }
                                    else
                                    {
                                        // 适应页面尺寸
                                        double pageWidth = page.Width.Point;
                                        double pageHeight = page.Height.Point;
                                        double imgWidth = xImg.PixelWidth;
                                        double imgHeight = xImg.PixelHeight;

                                        // 计算缩放比例
                                        double scale = Math.Min(pageWidth / imgWidth, pageHeight / imgHeight);
                                        double scaledWidth = imgWidth * scale;
                                        double scaledHeight = imgHeight * scale;

                                        // 居中绘制
                                        double x = (pageWidth - scaledWidth) / 2;
                                        double y = (pageHeight - scaledHeight) / 2;

                                        gfx.DrawImage(xImg, x, y, scaledWidth, scaledHeight);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\n处理图片 {imagePath} 时出错: {ex.Message}");
                    }
                }

                Console.WriteLine(); // 换行，结束进度显示

                // 保存PDF文档，如果文件被占用则尝试使用新的文件名
                try
                {
                    document.Save(outputPath);
                }
                catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 32) // 文件被占用的错误码
                {
                    // 生成一个带时间戳的新文件名
                    string directory = Path.GetDirectoryName(outputPath);
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(outputPath);
                    string extension = Path.GetExtension(outputPath);
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string newOutputPath = Path.Combine(
                        directory ?? "",
                        $"{fileNameWithoutExt}_{timestamp}{extension}");
                    
                    Console.WriteLine($"原文件被占用，将保存到新文件: {newOutputPath}");
                    document.Save(newOutputPath);
                    outputPath = newOutputPath; // 更新输出路径以便后续使用
                }
            }
        }

        /// <summary>
        /// 收集所有图片文件
        /// </summary>
        /// <param name="inputs">输入路径，可以是文件或文件夹</param>
        /// <param name="fileTypes">要处理的文件类型列表，为空则处理所有支持的类型</param>
        /// <param name="sortOrder">文件排序方式</param>
        /// <returns>图片文件路径列表</returns>
        private static List<string> CollectImageFiles(string[] inputs, List<string> fileTypes, SortOrder sortOrder)
        {
            var imageFiles = new List<string>();
            var validExtensions = fileTypes.Count > 0
                ? fileTypes.ToArray()
                : new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff" };

            foreach (var input in inputs)
            {
                if (File.Exists(input))
                {
                    // 如果是文件，检查扩展名
                    var extension = Path.GetExtension(input).ToLowerInvariant();
                    if (validExtensions.Contains(extension))
                    {
                        imageFiles.Add(input);
                    }
                }
                else if (Directory.Exists(input))
                {
                    // 如果是目录，获取所有符合条件的图片文件
                    var files = Directory.GetFiles(input)
                        .Where(f => validExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

                    // 根据排序方式对文件进行排序
                    IEnumerable<string> sortedFiles;
                    switch (sortOrder)
                    {
                        case SortOrder.ModifiedTime:
                            sortedFiles = files.OrderBy(f => new FileInfo(f).LastWriteTime);
                            break;
                        case SortOrder.CreationTime:
                            sortedFiles = files.OrderBy(f => new FileInfo(f).CreationTime);
                            break;
                        case SortOrder.FileName:
                        default:
                            sortedFiles = files.OrderBy(f => f);
                            break;
                    }

                    imageFiles.AddRange(sortedFiles);
                }
                else
                {
                    Console.WriteLine($"警告: 找不到路径 {input}");
                }
            }

            return imageFiles;
        }
    }
}