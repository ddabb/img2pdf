# Img2Pdf

一个将各种图片格式转换为PDF的跨平台命令行工具。

## 功能特点

- 支持多种图片格式（PNG、JPG、JPEG、BMP、GIF、TIFF）
- 支持处理单个文件、多个文件或整个文件夹
- 可以选择保持原始图片尺寸或适应页面尺寸
- 可以指定只处理特定类型的图片（如只处理PNG）
- 支持按文件名、修改时间或创建时间排序
- 提供清晰的命令行帮助信息
- 可以作为全局工具安装，随时随地使用
- 支持.NET 6.0及以上版本
- 支持无参数模式：直接在当前目录下运行时，自动处理当前目录的所有图片
- **新增：跨平台支持**，可在 Windows、Linux 和 macOS 上运行

## 安装

```
dotnet tool install --global Img2Pdf
```

## 使用方法

```
img2pdf -i <图片路径或文件夹> -o <输出PDF路径> [选项]
```

### 示例

```
# 无参数模式：自动处理当前目录下的所有图片
img2pdf

# 将单个图片转换为PDF
img2pdf -i image.png -o output.pdf

# 将多个图片转换为PDF
img2pdf -i image1.png image2.jpg -o output.pdf

# 将文件夹中的所有图片转换为PDF
img2pdf -i C:\Images\ -o output.pdf

# 只处理PNG格式的图片
img2pdf -i C:\Images\ -o output.pdf -t png

# 处理多种指定格式的图片
img2pdf -i C:\Images\ -o output.pdf -t png jpg

# 按修改时间排序
img2pdf -i C:\Images\ -o output.pdf -s mtime

# 按创建时间排序
img2pdf -i C:\Images\ -o output.pdf -s ctime

# 按文件名排序（默认）
img2pdf -i C:\Images\ -o output.pdf -s name

# 不保持原始图片尺寸（适应页面）
img2pdf -i image.png -o output.pdf -k false

# 组合使用多个选项
img2pdf -i C:\Images\ -o output.pdf -t png -s mtime -k false
```

### 参数说明

- `-i, --input <paths>`: 输入图片路径，可以是单个文件、多个文件或文件夹
- `-o, --output <path>`: 输出PDF文件路径
- `-k, --keep-size <bool>`: 保持原始图片尺寸（默认: true）
- `-t, --type <types>`: 指定要处理的图片类型，可多选（例如: png jpg）
- `-s, --sort <order>`: 指定文件排序方式，可选值:
  - `name`: 按文件名字母顺序排序（默认）
  - `mtime`或`modified`: 按文件修改时间排序（从早到晚）
  - `ctime`或`created`: 按文件创建时间排序（从早到晚）
- `-h, --help`: 显示帮助信息

## 系统要求

- .NET 6.0 或更高版本
- 支持 Windows、Linux 和 macOS 操作系统

## 版本历史

- 1.0.4: 添加跨平台支持，使用 ImageSharp 替代 System.Drawing，支持 Windows、Linux 和 macOS
- 1.0.3: 改进错误处理和用户体验
- 1.0.1: 添加图片类型过滤和排序选项，项目更名为Img2Pdf，仅支持.NET 6.0及以上版本
- 1.0.0: 初始版本（PngToPdf）

## 许可证

MIT
