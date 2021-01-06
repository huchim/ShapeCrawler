<h3 align="center">

![ShapeCrawler](/resources/readme.png)

</h3>

<h3 align="center">

[![NuGet](https://img.shields.io/nuget/v/ShapeCrawler?color=blue)](https://www.nuget.org/packages/ShapeCrawler) [![.NET Standard](https://img.shields.io/badge/.NET%20Core-2.0-blue)](#) [![.NET Standard](https://img.shields.io/badge/.NET%20Standard-%3E%3D%202.0-blue.svg)](#) [![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE) 

</h3>

ShapeCrawler (formerly SlideDotNet) is a .NET library for manipulating PowerPoint presentations. It provides fluent APIs to process slides without having Microsoft Office installed.

## Getting Started
You can quickly start work with the library by following steps listed below.
### Installing
To install ShapeCrawler, run the following command in the Package Manager Console:
```
PM> Install-Package ShapeCrawler
```

### Usage

```C#
public static async void Usage()
{
    // Gets number of slides
    var presentation = new Presentation(@"c:\test.pptx");
    var slides = presentation.Slides;
    var numSlides = slides.Count();

    // Gets slide sizes in EMUs
    int slideHeight = presentation.SlideHeight;
    int slideWidth = presentation.SlideWidth;

    // Saves presentation
    presentation.SaveAs(@"c:\test_edited.pptx");

    // Gets number of shapes
    Slide slide = slides[0];
    var shapes = slide.Shapes;
    var numShapes = shapes.Count;

    // Gets slide number
    int slideNumber = slide.Number;

    // Gets slide background content
    byte[] backgroundBytes = await slide.Background.GetImageBytesValueTask();
}
```
<details>
<summary><i>Show more usage examples...</i></summary>

```C#
public static async void Usage()
{
    // Gets number of slides
    var presentation = new Presentation(@"c:\test.pptx");
    var slides = presentation.Slides;
    var numSlides = slides.Count();

    // Gets slide sizes in EMUs
    int slideHeight = presentation.SlideHeight;
    int slideWidth = presentation.SlideWidth;

    // Saves presentation
    presentation.SaveAs(@"c:\test_edited.pptx");

    // Gets number of shapes
    Slide slide = slides[0];
    var shapes = slide.Shapes;
    var numShapes = shapes.Count;

    // Gets slide number
    int slideNumber = slide.Number;

    // Gets slide background content
    byte[] backgroundBytes = await slide.Background.GetImageBytesValueTask();

    // Sets slide background
    using (FileStream fs = File.OpenRead(@"c:\test.png"))
    {
        slide.Background.SetImage(fs);
    }

    // Hides slide
    slide.Hide();
    bool isHidden = slide.Hidden; // true

    // Set some custom data in slide, e.g. tag
    slide.CustomData = "#mySlide";

    // Works with charts
    var chartShape = shapes.FirstOrDefault(s => s.HasChart);
    if (chartShape != null)
    {
        IChart chart = chartShape.Chart;
        if (chart.HasTitle)
        {
            Debug.Print(chart.Title);
        }
        if (chart.Type == ChartType.BarChart)
        {
            Debug.Print("Chart type is BarChart.");
        }
    }
}
```
</details>

# Feedback and Give a Star! :star:
The project is in development, and I’m pretty sure there are still lots of things to add in this library. Try it out and let me know your thoughts.

Feel free to submit a [ticket](https://github.com/ShapeCrawler/ShapeCrawler/issues) if you find bugs. Your valuable feedback is much appreciated to better improve this project. If you find this useful, please give it a star to show your support for this project. 

# Contributing
1. Fork it (https://github.com/ShapeCrawler/ShapeCrawler/fork)
2. Create your feature branch (`git checkout -b my-new-feature`) from master.
3. Commit your changes (`git commit -am 'Add some feature'`).
4. Push to the branch (`git push origin my-new-feature`).
5. Create a new Pull Request.

# Changelog
## Version 0.10.0 - 2021-01-01
### Added
- Added `Portion.Remove()` to be able to remove paragraph portion;
- Added setter for `Paragraph.Text` property to be able to change paragraph's text;
- Added support for .NET Core 2.0

To find out more, please check out the [CHANGELOG](https://github.com/ShapeCrawler/ShapeCrawler/blob/master/CHANGELOG.md).
