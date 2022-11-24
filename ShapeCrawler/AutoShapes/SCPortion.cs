﻿using System;
using DocumentFormat.OpenXml.Packaging;
using ShapeCrawler.Exceptions;
using ShapeCrawler.Shared;
using A = DocumentFormat.OpenXml.Drawing;

namespace ShapeCrawler.AutoShapes;

internal class SCPortion : IPortion
{
    private readonly ResettableLazy<SCFont> font;
    private readonly A.Field? aField;

    internal SCPortion(A.Text aText, SCParagraph paragraph, A.Field aField)
        : this(aText, paragraph)
    {
        this.aField = aField;
    }

    internal SCPortion(A.Text aText, SCParagraph paragraph)
    {
        this.AText = aText;
        this.ParentParagraph = paragraph;
        this.font = new ResettableLazy<SCFont>(() => new SCFont(this.AText, this));
    }

    #region Public Properties

    /// <inheritdoc/>
    public string Text
    {
        get => this.GetText();
        set => this.SetText(value);
    }

    /// <inheritdoc/>
    public IFont Font => this.font.Value;

    public string? Hyperlink
    {
        get => this.GetHyperlink();
        set => this.SetHyperlink(value);
    }

    public IField? Field => this.GetFiled();

    #endregion Public Properties

    internal A.Text AText { get; }

    internal bool IsRemoved { get; set; }

    internal SCParagraph ParentParagraph { get; }

    private IField? GetFiled()
    {
        if (this.aField is null)
        {
            return null;
        }
        else
        {
            return new SCField(this.aField);
        }
    }

    private string GetText()
    {
        var portionText = this.AText.Text;
        if (this.AText.Parent!.NextSibling<A.Break>() != null)
        {
            portionText += Environment.NewLine;
        }

        return portionText;
    }

    private void SetText(string text)
    {
        this.AText.Text = text;
    }

    private string? GetHyperlink()
    {
        var runProperties = this.AText.PreviousSibling<A.RunProperties>();
        if (runProperties == null)
        {
            return null;
        }

        var hyperlink = runProperties.GetFirstChild<A.HyperlinkOnClick>();
        if (hyperlink == null)
        {
            return null;
        }

        var slideObject = (SlideObject)this.ParentParagraph.ParentTextFrame.TextFrameContainer.Shape.SlideObject;
        var typedOpenXmlPart = slideObject.TypedOpenXmlPart;
        var hyperlinkRelationship = (HyperlinkRelationship)typedOpenXmlPart.GetReferenceRelationship(hyperlink.Id!);

        return hyperlinkRelationship.Uri.AbsoluteUri;
    }

    private void SetHyperlink(string? url)
    {
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            throw new ShapeCrawlerException("Hyperlink is invalid.");
        }

        var runProperties = this.AText.PreviousSibling<A.RunProperties>();
        if (runProperties == null)
        {
            runProperties = new A.RunProperties();
        }

        var hyperlink = runProperties.GetFirstChild<A.HyperlinkOnClick>();
        if (hyperlink == null)
        {
            hyperlink = new A.HyperlinkOnClick();
            runProperties.Append(hyperlink);
        }

        var slideObject = (SlideObject)this.ParentParagraph.ParentTextFrame.TextFrameContainer.Shape.SlideObject;
        var slidePart = slideObject.TypedOpenXmlPart;

        var uri = new Uri(url, UriKind.Absolute);
        var addedHyperlinkRelationship = slidePart.AddHyperlinkRelationship(uri, true);

        hyperlink.Id = addedHyperlinkRelationship.Id;
    }
}